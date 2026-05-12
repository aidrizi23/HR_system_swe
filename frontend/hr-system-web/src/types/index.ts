export type Role =
  | "Employee"
  | "TeamLead"
  | "DepartmentManager"
  | "HRManager"
  | "SuperAdmin";

// ───────── Tasks ─────────

export type TaskPriority = "Low" | "Medium" | "High" | "Urgent";
export type WorkTaskStatus = "Open" | "InProgress" | "OnHold" | "Done";

export interface WorkTaskDto {
  id: number;
  publicId: string;
  title: string;
  description?: string;
  assignedToId: number;
  assignedToName: string;
  assignedById: number;
  assignedByName: string;
  priority: TaskPriority;
  status: WorkTaskStatus;
  dueDate?: string;
  category?: string;
  tags?: string;
  completedAt?: string;
  slug: string;
  createdAt: string;
  commentCount: number;
}

export interface CreateWorkTaskDto {
  title: string;
  description?: string;
  assignedToId: number;
  priority?: TaskPriority;
  dueDate?: string;
  category?: string;
  tags?: string;
}

export interface UpdateWorkTaskDto {
  title?: string;
  description?: string;
  assignedToId?: number;
  priority?: TaskPriority;
  status?: WorkTaskStatus;
  dueDate?: string;
  category?: string;
  tags?: string;
}

export interface UpdateTaskStatusDto {
  status: WorkTaskStatus;
}

export interface TaskFilterDto {
  assignedToId?: number;
  status?: WorkTaskStatus;
  priority?: TaskPriority;
  dueDateFrom?: string;
  dueDateTo?: string;
  page?: number;
  pageSize?: number;
}

export interface TaskCommentDto {
  id: number;
  publicId: string;
  taskId: number;
  authorId: number;
  authorName: string;
  content: string;
  createdAt: string;
}

export interface CreateTaskCommentDto {
  content: string;
}

// ───────── Documents ─────────

export interface EmployeeDocumentDto {
  id: number;
  publicId: string;
  employeeId: number;
  employeeName: string;
  categoryId: number;
  categoryName: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  expiryDate?: string;
  uploadedById: number;
  notes?: string;
  createdAt: string;
}

export interface DocumentCategoryDto {
  id: number;
  publicId: string;
  name: string;
  description?: string;
  slug: string;
}

export interface CreateDocumentCategoryDto {
  name: string;
  description?: string;
}

export interface UploadDocumentDto {
  categoryId: number;
  expiryDate?: string;
  notes?: string;
}
