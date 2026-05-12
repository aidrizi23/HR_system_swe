import type {
  WorkTaskDto,
  CreateWorkTaskDto,
  UpdateWorkTaskDto,
  WorkTaskStatus,
  TaskFilterDto,
  TaskCommentDto,
} from "@/types";
import { mockUsers, getCurrentMockUser } from "./users";

let nextTaskId = 1;
let nextCommentId = 1;

function slugify(s: string): string {
  return s.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
}

function newGuid(): string {
  // mock GUID — not cryptographically meaningful
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

const now = new Date();
const daysFromNow = (n: number) =>
  new Date(now.getTime() + n * 86400000).toISOString();

function seed(): WorkTaskDto[] {
  const tasks: WorkTaskDto[] = [];
  const seeds: Array<Omit<WorkTaskDto, "id" | "publicId" | "slug" | "createdAt" | "commentCount">> = [
    { title: "Review onboarding documents", description: "Sign all standard new-hire docs.", assignedToId: 1, assignedToName: "Admin User", assignedById: 2, assignedByName: "Hannah Reyes", priority: "High",   status: "Open",       dueDate: daysFromNow(3) },
    { title: "Q2 leave audit",              description: "Cross-check Q2 leave balances.",   assignedToId: 1, assignedToName: "Admin User", assignedById: 2, assignedByName: "Hannah Reyes", priority: "Medium", status: "Open",       dueDate: daysFromNow(7) },
    { title: "Vendor contract review",      assignedToId: 5, assignedToName: "Noah Schmidt", assignedById: 3, assignedByName: "Diego Marin",  priority: "Low",    status: "Open",       dueDate: daysFromNow(11) },
    { title: "Migrate payroll cycle",       description: "Move to new monthly cadence.",     assignedToId: 1, assignedToName: "Admin User", assignedById: 2, assignedByName: "Hannah Reyes", priority: "High",   status: "InProgress", dueDate: daysFromNow(1) },
    { title: "Refactor leave service",      assignedToId: 4, assignedToName: "Lila Park",    assignedById: 3, assignedByName: "Diego Marin",  priority: "Medium", status: "InProgress", dueDate: daysFromNow(4) },
    { title: "Update employee handbook",    description: "Reflect new PTO policy.",          assignedToId: 4, assignedToName: "Lila Park",    assignedById: 2, assignedByName: "Hannah Reyes", priority: "Low",    status: "OnHold",     dueDate: daysFromNow(20) },
    { title: "May payroll close",           assignedToId: 2, assignedToName: "Hannah Reyes", assignedById: 1, assignedByName: "Admin User",   priority: "Medium", status: "Done",       dueDate: daysFromNow(-2), completedAt: daysFromNow(-2) },
    { title: "Q1 audit pack",               assignedToId: 5, assignedToName: "Noah Schmidt", assignedById: 2, assignedByName: "Hannah Reyes", priority: "Low",    status: "Done",       dueDate: daysFromNow(-4), completedAt: daysFromNow(-4) },
    { title: "Office desk reassignment",    assignedToId: 6, assignedToName: "Priya Iyer",   assignedById: 3, assignedByName: "Diego Marin",  priority: "Low",    status: "Open",       dueDate: daysFromNow(9) },
    { title: "Run engagement survey",       description: "Annual pulse survey rollout.",     assignedToId: 2, assignedToName: "Hannah Reyes", assignedById: 1, assignedByName: "Admin User",   priority: "Medium", status: "Open",       dueDate: daysFromNow(14) },
    { title: "New hire equipment order",    assignedToId: 6, assignedToName: "Priya Iyer",   assignedById: 4, assignedByName: "Lila Park",    priority: "Urgent", status: "InProgress", dueDate: daysFromNow(0) },
    { title: "Archive old payroll batches", assignedToId: 1, assignedToName: "Admin User",   assignedById: 1, assignedByName: "Admin User",   priority: "Low",    status: "OnHold",     dueDate: daysFromNow(30) },
  ];

  for (const s of seeds) {
    const id = nextTaskId++;
    tasks.push({
      ...s,
      id,
      publicId: newGuid(),
      slug: slugify(s.title) + "-" + id,
      createdAt: daysFromNow(-Math.floor(Math.random() * 10) - 1),
      commentCount: 0,
    });
  }
  return tasks;
}

let tasks: WorkTaskDto[] = seed();
let comments: TaskCommentDto[] = [
  { id: nextCommentId++, publicId: newGuid(), taskId: 1, authorId: 2, authorName: "Hannah Reyes", content: "Started on this — first batch done.", createdAt: daysFromNow(-1) },
  { id: nextCommentId++, publicId: newGuid(), taskId: 1, authorId: 1, authorName: "Admin User",   content: "Nice, ping me when ready.",            createdAt: daysFromNow(0) },
];
// keep commentCount in sync with the seed comments
for (const c of comments) {
  const t = tasks.find((x) => x.id === c.taskId);
  if (t) t.commentCount++;
}

function matchesFilter(t: WorkTaskDto, f: TaskFilterDto): boolean {
  if (f.assignedToId != null && t.assignedToId !== f.assignedToId) return false;
  if (f.status != null && t.status !== f.status) return false;
  if (f.priority != null && t.priority !== f.priority) return false;
  if (f.dueDateFrom && (!t.dueDate || t.dueDate < f.dueDateFrom)) return false;
  if (f.dueDateTo && (!t.dueDate || t.dueDate > f.dueDateTo)) return false;
  return true;
}

export const mockTasks = {
  async list(filter: TaskFilterDto = {}) {
    const filtered = tasks.filter((t) => matchesFilter(t, filter));
    const page = filter.page ?? 1;
    const pageSize = filter.pageSize ?? 50;
    const start = (page - 1) * pageSize;
    return {
      items: filtered.slice(start, start + pageSize),
      totalCount: filtered.length,
      page,
      pageSize,
      totalPages: Math.ceil(filtered.length / pageSize),
    };
  },
  async get(id: number) {
    return tasks.find((t) => t.id === id) ?? null;
  },
  async create(dto: CreateWorkTaskDto): Promise<WorkTaskDto> {
    const me = getCurrentMockUser();
    const assignee = mockUsers.find((u) => u.id === dto.assignedToId) ?? me;
    const id = nextTaskId++;
    const task: WorkTaskDto = {
      id,
      publicId: newGuid(),
      title: dto.title,
      description: dto.description,
      assignedToId: assignee.id,
      assignedToName: assignee.name,
      assignedById: me.id,
      assignedByName: me.name,
      priority: dto.priority ?? "Medium",
      status: "Open",
      dueDate: dto.dueDate,
      category: dto.category,
      tags: dto.tags,
      slug: slugify(dto.title) + "-" + id,
      createdAt: new Date().toISOString(),
      commentCount: 0,
    };
    tasks = [task, ...tasks];
    return task;
  },
  async update(id: number, dto: UpdateWorkTaskDto): Promise<WorkTaskDto | null> {
    const t = tasks.find((x) => x.id === id);
    if (!t) return null;
    Object.assign(t, dto);
    if (dto.assignedToId != null) {
      const u = mockUsers.find((u) => u.id === dto.assignedToId);
      if (u) t.assignedToName = u.name;
    }
    return t;
  },
  async updateStatus(id: number, status: WorkTaskStatus): Promise<WorkTaskDto | null> {
    const t = tasks.find((x) => x.id === id);
    if (!t) return null;
    t.status = status;
    t.completedAt = status === "Done" ? new Date().toISOString() : undefined;
    return t;
  },
  async remove(id: number): Promise<boolean> {
    const before = tasks.length;
    tasks = tasks.filter((t) => t.id !== id);
    comments = comments.filter((c) => c.taskId !== id);
    return tasks.length < before;
  },
  async comments(taskId: number): Promise<TaskCommentDto[]> {
    return comments.filter((c) => c.taskId === taskId);
  },
  async addComment(taskId: number, content: string): Promise<TaskCommentDto | null> {
    const t = tasks.find((x) => x.id === taskId);
    if (!t) return null;
    const me = getCurrentMockUser();
    const c: TaskCommentDto = {
      id: nextCommentId++,
      publicId: newGuid(),
      taskId,
      authorId: me.id,
      authorName: me.name,
      content,
      createdAt: new Date().toISOString(),
    };
    comments.push(c);
    t.commentCount++;
    return c;
  },
};
