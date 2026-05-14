// src/lib/mock/notifications.ts
import type { NotificationDto, NotificationType, EmailPreferenceDto } from "@/types";
import { getCurrentMockUser } from "./users";

let nextId = 1;

function newGuid(): string {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

function mk(userId: number, type: NotificationType, title: string, message: string, daysAgo: number, isRead = false): NotificationDto {
  return {
    id: nextId++,
    publicId: newGuid(),
    recipientUserId: userId,
    type,
    typeName: type,
    title,
    message,
    isRead,
    createdAt: new Date(Date.now() - daysAgo * 86400000).toISOString(),
  };
}

let notifications: NotificationDto[] = [
  mk(1, "AnnouncementPosted",   "New announcement: All-hands Friday", "Join us at 10am for the quarterly all-hands.", 0, false),
  mk(1, "OvertimeRequest",      "Overtime approved", "Your overtime on May 9 was approved.", 1, false),
  mk(1, "TaskAssigned",         "New task assigned",  "Review onboarding documents — due Friday.", 1, true),
  mk(1, "DocumentExpiring",     "Document expiring",  "Employment contract expires in 14 days.", 2, true),
  mk(1, "OnboardingItemDue",    "Onboarding item due","Submit tax & benefits forms.", 3, true),
];

const types: NotificationType[] = ["LeaveRequest","OvertimeRequest","TaskAssigned","AnnouncementPosted","DocumentExpiring","OnboardingItemDue","Other"];
let emailPrefs: EmailPreferenceDto[] = types.map((t) => ({ notificationType: t, typeName: t, isEmailEnabled: t !== "Other" }));

export const mockNotifications = {
  async list(): Promise<NotificationDto[]> {
    const me = getCurrentMockUser();
    return notifications.filter((n) => n.recipientUserId === me.id).sort((a, b) => b.createdAt.localeCompare(a.createdAt));
  },
  async unreadCount(): Promise<number> {
    const me = getCurrentMockUser();
    return notifications.filter((n) => n.recipientUserId === me.id && !n.isRead).length;
  },
  async markRead(id: number): Promise<void> {
    const n = notifications.find((x) => x.id === id);
    if (n && !n.isRead) { n.isRead = true; n.readAt = new Date().toISOString(); }
  },
  async markAllRead(): Promise<void> {
    const me = getCurrentMockUser();
    notifications = notifications.map((n) =>
      n.recipientUserId === me.id && !n.isRead
        ? { ...n, isRead: true, readAt: new Date().toISOString() }
        : n
    );
  },
  async emailPreferences(): Promise<EmailPreferenceDto[]> {
    return [...emailPrefs];
  },
  async updateEmailPreferences(prefs: EmailPreferenceDto[]): Promise<void> {
    emailPrefs = prefs;
  },
};
