import { api } from "./client";
import type { EmailPreferenceDto, NotificationDto } from "@/types";

interface PagedNotificationsResponse {
  items: NotificationDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const apiNotifications = {
  list: async (): Promise<NotificationDto[]> => {
    const { data } = await api.get<PagedNotificationsResponse>("/notifications", {
      params: { page: 1, pageSize: 20 },
    });
    return data.items;
  },

  unreadCount: async (): Promise<number> => {
    const { data } = await api.get<{ count: number }>("/notifications/unread-count");
    return data.count;
  },

  markRead: async (id: number): Promise<void> => {
    await api.post(`/notifications/${id}/read`);
  },

  markAllRead: async (): Promise<void> => {
    await api.post("/notifications/read-all");
  },

  emailPreferences: async (): Promise<EmailPreferenceDto[]> => {
    const { data } = await api.get<EmailPreferenceDto[]>("/notifications/email-preferences");
    return data;
  },

  updateEmailPreferences: async (prefs: EmailPreferenceDto[]): Promise<void> => {
    await api.put("/notifications/email-preferences", prefs);
  },
};
