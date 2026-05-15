import { mockTasks } from "@/lib/mock/tasks";
import type {
  CreateWorkTaskDto,
  TaskFilterDto,
  UpdateWorkTaskDto,
  WorkTaskStatus,
} from "@/types";

export const apiTasks = {
  list:         (filter: TaskFilterDto = {}) => mockTasks.list(filter),
  get:          (id: number)                 => mockTasks.get(id),
  create:       (dto: CreateWorkTaskDto)     => mockTasks.create(dto),
  update:       (id: number, dto: UpdateWorkTaskDto) => mockTasks.update(id, dto),
  updateStatus: (id: number, status: WorkTaskStatus) => mockTasks.updateStatus(id, status),
  remove:       (id: number)                 => mockTasks.remove(id),
  comments:     (id: number)                 => mockTasks.comments(id),
  addComment:   (id: number, content: string) => mockTasks.addComment(id, content),
};
