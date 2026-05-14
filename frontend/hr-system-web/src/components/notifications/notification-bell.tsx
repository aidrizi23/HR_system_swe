// src/components/notifications/notification-bell.tsx
"use client";

import { useCallback, useEffect, useState } from "react";
import { Bell } from "lucide-react";
import { usePathname } from "next/navigation";
import { apiNotifications } from "@/lib/api/notifications";
import type { NotificationDto } from "@/types";

export function NotificationBell() {
  const [open, setOpen] = useState(false);
  const [items, setItems] = useState<NotificationDto[]>([]);
  const [unread, setUnread] = useState(0);
  const pathname = usePathname();

  const refresh = useCallback(async () => {
    const [list, count] = await Promise.all([apiNotifications.list(), apiNotifications.unreadCount()]);
    setItems(list.slice(0, 10));
    setUnread(count);
  }, []);

  // refresh on page nav
  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void refresh();
  }, [pathname, refresh]);

  // refresh on bell click (when opening)
  function toggle() {
    const next = !open;
    setOpen(next);
    if (next) refresh();
  }

  async function markRead(id: number) {
    await apiNotifications.markRead(id);
    refresh();
  }
  async function markAll() {
    await apiNotifications.markAllRead();
    refresh();
  }

  return (
    <div className="relative">
      <button
        type="button"
        onClick={toggle}
        className="relative flex h-9 w-9 items-center justify-center rounded-full border border-border bg-card hover:bg-muted"
        aria-label="Notifications"
      >
        <Bell className="h-4 w-4 text-foreground" />
        {unread > 0 && (
          <span className="absolute -right-1 -top-1 flex h-4 min-w-[16px] items-center justify-center rounded-full bg-red-500 px-1 text-[9px] font-bold text-white">
            {unread > 9 ? "9+" : unread}
          </span>
        )}
      </button>

      {open && (
        <>
          <div className="fixed inset-0 z-30" onClick={() => setOpen(false)} />
          <div className="absolute right-0 top-10 z-40 w-80 rounded-xl border border-border bg-card shadow-xl">
            <div className="flex items-center justify-between border-b border-border px-3 py-2">
              <span className="text-sm font-semibold">Notifications</span>
              <button type="button" onClick={markAll} className="text-[11px] text-primary hover:underline">
                Mark all as read
              </button>
            </div>
            <ul className="max-h-80 overflow-y-auto">
              {items.length === 0 && (
                <li className="p-6 text-center text-xs text-muted-foreground">No notifications</li>
              )}
              {items.map((n) => (
                <li key={n.id}>
                  <button
                    type="button"
                    onClick={() => markRead(n.id)}
                    className={`block w-full border-b border-border px-3 py-2 text-left text-xs hover:bg-muted ${n.isRead ? "" : "bg-primary/5"}`}
                  >
                    <div className="flex items-start gap-2">
                      {!n.isRead && <span className="mt-1 h-2 w-2 flex-shrink-0 rounded-full bg-primary" />}
                      <div className="flex-1">
                        <div className="font-semibold">{n.title}</div>
                        <div className="mt-0.5 text-muted-foreground line-clamp-2">{n.message}</div>
                        <div className="mt-1 text-[10px] text-muted-foreground">{new Date(n.createdAt).toLocaleString()}</div>
                      </div>
                    </div>
                  </button>
                </li>
              ))}
            </ul>
            <div className="border-t border-border px-3 py-2 text-center">
              <span className="text-[11px] text-muted-foreground">View all (coming soon)</span>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
