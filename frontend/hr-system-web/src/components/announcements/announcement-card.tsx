// src/components/announcements/announcement-card.tsx
"use client";

import { useState } from "react";
import { Pin } from "lucide-react";
import type { AnnouncementDto } from "@/types";

interface Props {
  announcement: AnnouncementDto;
  readCount?: number;
  totalMembers?: number;
}

const PRIORITY_TONE: Record<AnnouncementDto["priority"], string> = {
  Low:    "bg-blue-100 text-blue-800",
  Normal: "bg-slate-200 text-slate-800",
  High:   "bg-red-100 text-red-800",
};

function initials(name: string) {
  return name.split(" ").map((p) => p[0]).slice(0, 2).join("").toUpperCase();
}

export function AnnouncementCard({ announcement: a, readCount, totalMembers }: Props) {
  const [expanded, setExpanded] = useState(false);
  return (
    <div className={`rounded-2xl border ${a.isPinned ? "border-primary/30" : "border-border"} bg-card p-5 shadow-[0_1px_2px_rgba(15,23,42,0.04)]`}>
      <div className="flex items-start gap-3">
        <div className="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full bg-primary text-xs font-bold text-primary-foreground">
          {initials(a.authorName)}
        </div>
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold">{a.authorName}</span>
            <span className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${PRIORITY_TONE[a.priority]}`}>{a.priority}</span>
            {a.isPinned && <span className="flex items-center gap-1 rounded-full bg-primary/10 px-2 py-0.5 text-[10px] font-semibold text-primary"><Pin className="h-3 w-3" /> Pinned</span>}
            <span className="ml-auto text-[11px] text-muted-foreground">{new Date(a.createdAt).toLocaleDateString()}</span>
          </div>
          <h3 className="mt-2 text-base font-bold">{a.title}</h3>
          <p className={`mt-1 text-sm text-foreground/80 ${expanded ? "" : "line-clamp-2"}`}>{a.body}</p>
          {a.body.length > 200 && (
            <button type="button" onClick={() => setExpanded((x) => !x)} className="mt-1 text-[11px] text-primary hover:underline">
              {expanded ? "Show less" : "Show more"}
            </button>
          )}
          {readCount != null && totalMembers != null && (
            <div className="mt-3 text-[11px] text-muted-foreground">Read by {readCount} of {totalMembers}</div>
          )}
        </div>
      </div>
    </div>
  );
}
