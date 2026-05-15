// src/app/(dashboard)/announcements/page.tsx
"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/shared/page-header";
import { AnnouncementFeed } from "@/components/announcements/announcement-feed";
import { AnnouncementCreateForm } from "@/components/announcements/announcement-create-form";
import { apiAnnouncements } from "@/lib/api/announcements";
import { getCurrentMockUser, isHrOrAbove } from "@/lib/mock/users";
import type { AnnouncementDto } from "@/types";

type Tab = "feed" | "create" | "analytics";

const TABS: Array<{ key: Tab; label: string; hrOnly: boolean }> = [
  { key: "feed",      label: "Feed",      hrOnly: false },
  { key: "create",    label: "Create",    hrOnly: true },
  { key: "analytics", label: "Analytics", hrOnly: false },
];

export default function AnnouncementsPage() {
  const me = getCurrentMockUser();
  const isHr = isHrOrAbove(me.role);
  const visibleTabs = TABS.filter((t) => !t.hrOnly || isHr);
  const [active, setActive] = useState<Tab>("feed");
  const [items, setItems] = useState<AnnouncementDto[]>([]);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => { apiAnnouncements.list().then(setItems); }, [refreshKey]);

  const total = items.length;
  const unread = items.filter((a) => !a.isRead).length;
  const pinned = items.filter((a) => a.isPinned).length;

  return (
    <div className="space-y-6">
      <PageHeader title="Announcements" subtitle="Company announcements and updates." />

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
        {[
          { label: "TOTAL",  value: String(total) },
          { label: "UNREAD", value: String(unread) },
          { label: "PINNED", value: String(pinned) },
        ].map((c) => (
          <div key={c.label} className="rounded-2xl border border-border bg-card p-4">
            <div className="text-[10px] font-semibold tracking-wider text-muted-foreground">{c.label}</div>
            <div className="mt-1 text-2xl font-bold">{c.value}</div>
          </div>
        ))}
      </div>

      <div className="flex gap-6 border-b border-border">
        {visibleTabs.map((t) => (
          <button
            key={t.key}
            onClick={() => setActive(t.key)}
            className={`-mb-px border-b-2 py-2 text-sm transition ${
              active === t.key ? "border-primary font-semibold text-primary" : "border-transparent text-muted-foreground hover:text-foreground"
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {active === "feed"   && <AnnouncementFeed refreshKey={refreshKey} />}
      {active === "create" && isHr && (
        <AnnouncementCreateForm onCreated={() => { setRefreshKey((x) => x + 1); setActive("feed"); }} />
      )}
      {active === "analytics" && (
        <div className="rounded-2xl border border-border bg-card p-12 text-center text-sm text-muted-foreground">
          Analytics ships in a later branch.
        </div>
      )}
    </div>
  );
}
