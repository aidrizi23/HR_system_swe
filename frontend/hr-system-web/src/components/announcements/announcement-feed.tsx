"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { AnnouncementCard } from "./announcement-card";
import { apiAnnouncements } from "@/lib/api/announcements";
import { apiUsers } from "@/lib/api/users";
import type { AnnouncementDto } from "@/types";

interface Props { refreshKey?: number; }

export function AnnouncementFeed({ refreshKey }: Props) {
  const [items, setItems] = useState<AnnouncementDto[]>([]);
  const [readCounts, setReadCounts] = useState<Record<number, number>>({});
  const [totalMembers, setTotalMembers] = useState(0);
  const seen = useRef<Set<number>>(new Set());

  useEffect(() => {
    apiUsers.list().then((users) => setTotalMembers(users.length));
  }, []);

  const refresh = useCallback(() => {
    apiAnnouncements.list().then((list) => {
      setItems(list);
      Promise.all(list.map((a) => apiAnnouncements.readCount(a.id).then((c) => [a.id, c] as [number, number]))).then(
        (pairs) => setReadCounts(Object.fromEntries(pairs)),
      );
    });
  }, []);
  useEffect(() => { refresh(); }, [refreshKey, refresh]);

  useEffect(() => {
    // Already-read items go straight into `seen` so the observer skips them on rebuild,
    // preventing a burst of redundant markRead calls when the feed re-renders.
    for (const a of items) {
      if (a.isRead) seen.current.add(a.id);
    }
    const observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (!entry.isIntersecting) continue;
          const id = Number((entry.target as HTMLElement).dataset.announcementId);
          if (Number.isNaN(id) || seen.current.has(id)) continue;
          seen.current.add(id);
          apiAnnouncements.markRead(id).then(refresh);
        }
      },
      { threshold: 0.5 },
    );
    const elements = document.querySelectorAll<HTMLElement>("[data-announcement-id]");
    elements.forEach((el) => observer.observe(el));
    return () => observer.disconnect();
  }, [items, refresh]);

  if (items.length === 0) {
    return (
      <div className="rounded-2xl border border-border bg-card p-12 text-center text-sm text-muted-foreground">
        No announcements found. New announcements will appear here.
      </div>
    );
  }

  const pinned = items.filter((a) => a.isPinned);
  const rest = items.filter((a) => !a.isPinned);

  return (
    <div className="space-y-4">
      {pinned.length > 0 && (
        <div className="space-y-3">
          <div className="text-[11px] font-bold tracking-wider text-muted-foreground">📌 PINNED</div>
          {pinned.map((a) => (
            <div key={a.id} data-announcement-id={a.id}>
              <AnnouncementCard announcement={a} readCount={readCounts[a.id]} totalMembers={totalMembers} />
            </div>
          ))}
        </div>
      )}
      {rest.length > 0 && (
        <div className="space-y-3">
          {rest.map((a) => (
            <div key={a.id} data-announcement-id={a.id}>
              <AnnouncementCard announcement={a} readCount={readCounts[a.id]} totalMembers={totalMembers} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
