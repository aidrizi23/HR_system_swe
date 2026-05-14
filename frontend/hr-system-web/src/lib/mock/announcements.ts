// src/lib/mock/announcements.ts
import type {
  AnnouncementDto,
  AnnouncementPriority,
  CreateAnnouncementDto,
} from "@/types";
import { mockUsers, getCurrentMockUser } from "./users";

let nextId = 1;
const readReceipts: Set<string> = new Set(); // "userId:announcementId"

function newGuid(): string {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

function slugify(s: string): string {
  return s.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
}

function mk(authorId: number, title: string, body: string, priority: AnnouncementPriority, isPinned: boolean, daysAgo: number): AnnouncementDto {
  const id = nextId++;
  const author = mockUsers.find((u) => u.id === authorId)!;
  const created = new Date(Date.now() - daysAgo * 86400000).toISOString();
  return {
    id,
    publicId: newGuid(),
    title,
    body,
    priority,
    isPinned,
    publishDate: created,
    authorId,
    authorName: author.name,
    isRead: false, // set per-user via getReadStatus below
    slug: slugify(title) + "-" + id,
    createdAt: created,
  };
}

let announcements: AnnouncementDto[] = [
  mk(2, "All-hands Friday at 10am",       "Quarterly all-hands meeting in the main conference room. Agenda attached.", "High",   true,  0),
  mk(2, "New leave policy effective June 1", "We are rolling out the updated PTO policy. Highlights inside.",            "Normal", true,  2),
  mk(1, "Office closure on Memorial Day", "Reminder: the office will be closed on May 26.",                              "Low",    false, 5),
  mk(2, "Welcome to our new hires",       "Please join us in welcoming Noah and Priya to the team.",                     "Normal", false, 7),
];

function withReadStatus(items: AnnouncementDto[], userId: number): AnnouncementDto[] {
  return items.map((a) => ({ ...a, isRead: readReceipts.has(`${userId}:${a.id}`) }));
}

export const mockAnnouncements = {
  async list(): Promise<AnnouncementDto[]> {
    const me = getCurrentMockUser();
    return withReadStatus([...announcements].sort((a, b) => {
      // pinned first, then newest
      if (a.isPinned !== b.isPinned) return a.isPinned ? -1 : 1;
      return b.createdAt.localeCompare(a.createdAt);
    }), me.id);
  },
  async create(dto: CreateAnnouncementDto): Promise<AnnouncementDto> {
    const me = getCurrentMockUser();
    const priority: AnnouncementPriority = dto.priority === 0 ? "Low" : dto.priority === 2 ? "High" : "Normal";
    const ann = mk(me.id, dto.title, dto.body, priority, dto.isPinned, 0);
    if (dto.departmentId) {
      ann.departmentId = dto.departmentId;
    }
    announcements = [ann, ...announcements];
    return ann;
  },
  async markRead(id: number): Promise<void> {
    const me = getCurrentMockUser();
    readReceipts.add(`${me.id}:${id}`);
  },
  async readCount(id: number): Promise<number> {
    let count = 0;
    for (const key of readReceipts) if (key.endsWith(`:${id}`)) count++;
    return count;
  },
};
