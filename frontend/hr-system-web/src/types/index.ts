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

// ───────── Time Tracking ─────────

export interface TimeLogDto {
  id: number;
  publicId: string;
  employeeId: number;
  date: string;            // ISO date (YYYY-MM-DD)
  startTime: string;       // "HH:mm"
  endTime?: string;        // "HH:mm", undefined while session is open
  durationMinutes: number;
  notes?: string;
  createdAt: string;
}

export interface CreateTimeLogDto {
  date: string;
  startTime: string;
  endTime: string;
  notes?: string;
}

export interface UpdateTimeLogDto {
  startTime?: string;
  endTime?: string;
  notes?: string;
}

export interface DailySummaryDto {
  date: string;
  employeeId?: number;
  employeeName?: string;
  sessions: TimeLogDto[];
  totalMinutes: number;
  totalHours: number;
  sessionCount: number;
  standardHours: number;
  isOvertime: boolean;
  activeSessionStartTime?: string;
}

export interface WeeklySummaryDto {
  weekStart: string;       // ISO date of Monday
  days: DailySummaryDto[];
  totalMinutes: number;
  totalHours: number;
  standardWeeklyHours: number;
}

export interface ModificationRequestDto {
  id: number;
  publicId: string;
  employeeId: number;
  employeeName?: string;
  timeLogId: number;
  requestedStartTime: string;
  requestedEndTime: string;
  reason?: string;
  status: "Pending" | "Approved" | "Rejected";
  approvedById?: number;
  processedAt?: string;
  createdAt: string;
}

export interface CreateModificationRequestDto {
  timeLogId: number;
  requestedStartTime: string;
  requestedEndTime: string;
  reason?: string;
}

// ───────── Overtime ─────────

export type OvertimeStatus = "Pending" | "Recommended" | "Approved" | "Rejected";

export interface OvertimeRecordDto {
  id: number;
  publicId: string;
  employeeId: number;
  employeeName?: string;
  date: string;
  overtimeMinutes: number;
  overtimeHours: number;
  type: string;
  reason?: string;
  status: OvertimeStatus;
  recommendedById?: number;
  recommenderComments?: string;
  recommendedAt?: string;
  approvedById?: number;
  approverComments?: string;
  processedAt?: string;
  createdAt: string;
}

export interface CreateOvertimeRequestDto {
  date: string;
  overtimeMinutes: number;   // 1-720
  reason?: string;           // max 1000
}

export interface OvertimeFilterDto {
  employeeId?: number;
  status?: OvertimeStatus;
  dateFrom?: string;
  dateTo?: string;
}

// ───────── Onboarding ─────────

export type OnboardingResponsibleRole = "Employee" | "HR" | "Manager" | "IT";

export interface OnboardingTemplateItemDto {
  id: number;
  publicId: string;
  description: string;
  responsibleRole: OnboardingResponsibleRole;
  defaultDueDays: number;
}

export interface OnboardingTemplateDto {
  id: number;
  publicId: string;
  name: string;
  description?: string;
  targetEmploymentType?: string;
  slug: string;
  items: OnboardingTemplateItemDto[];
  createdAt: string;
}

export interface CreateOnboardingTemplateItemDto {
  description: string;
  responsibleRole: OnboardingResponsibleRole;
  defaultDueDays: number;
}

export interface CreateOnboardingTemplateDto {
  name: string;
  description?: string;
  targetEmploymentType?: string;
  items: CreateOnboardingTemplateItemDto[];
}

export interface OnboardingChecklistItemDto {
  id: number;
  publicId: string;
  description: string;
  responsiblePartyId?: number;
  responsiblePartyName?: string;
  dueDate: string;
  completedAt?: string;
  status: "Pending" | "Completed" | "Overdue";
}

export interface OnboardingChecklistDto {
  id: number;
  publicId: string;
  employeeId: number;
  employeeName: string;
  templateId: number;
  templateName: string;
  startedAt: string;
  completedAt?: string;
  status: string;
  totalItems: number;
  completedItems: number;
  items: OnboardingChecklistItemDto[];
}

export interface AssignChecklistDto {
  employeeId: number;
  templateId: number;
}

// ───────── Notifications ─────────

export type NotificationType =
  | "LeaveRequest" | "OvertimeRequest" | "TaskAssigned" | "AnnouncementPosted"
  | "DocumentExpiring" | "OnboardingItemDue" | "Other";

export interface NotificationDto {
  id: number;
  publicId: string;
  recipientUserId: number;
  type: NotificationType;
  typeName: string;
  title: string;
  message: string;
  isRead: boolean;
  readAt?: string;
  relatedEntityType?: string;
  relatedEntityId?: number;
  createdAt: string;
}

export interface EmailPreferenceDto {
  notificationType: NotificationType;
  typeName: string;
  isEmailEnabled: boolean;
}

// ───────── Announcements ─────────

export type AnnouncementPriority = "Low" | "Normal" | "High";

export interface AnnouncementDto {
  id: number;
  publicId: string;
  title: string;
  body: string;
  priority: AnnouncementPriority;
  departmentId?: number;
  departmentName?: string;
  isPinned: boolean;
  publishDate: string;
  authorId: number;
  authorName: string;
  isRead: boolean;
  slug: string;
  createdAt: string;
}

export interface CreateAnnouncementDto {
  title: string;
  body: string;
  priority: number;     // 0=Low, 1=Normal, 2=High
  departmentId?: number;
  isPinned: boolean;
  publishDate?: string;
}
