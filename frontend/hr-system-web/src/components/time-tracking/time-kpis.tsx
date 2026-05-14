"use client";

import { useEffect, useState } from "react";
import { apiTimeTracking, timeHelpers } from "@/lib/api/time-tracking";
import type { DailySummaryDto, WeeklySummaryDto } from "@/types";

interface Props { refreshKey?: number; }

export function TimeKpis({ refreshKey }: Props) {
  const [daily, setDaily] = useState<DailySummaryDto | null>(null);
  const [weekly, setWeekly] = useState<WeeklySummaryDto | null>(null);

  useEffect(() => {
    const todayIso = timeHelpers.isoDate(new Date());
    apiTimeTracking.dailySummary(todayIso).then(setDaily);
    apiTimeTracking.weeklySummary().then(setWeekly);
  }, [refreshKey]);

  const sessions = weekly?.days.reduce((s, d) => s + d.sessionCount, 0) ?? 0;
  const avgPerDay = weekly && weekly.days.filter((d) => d.totalHours > 0).length > 0
    ? (weekly.totalHours / weekly.days.filter((d) => d.totalHours > 0).length).toFixed(1)
    : "—";

  const cards = [
    { label: "HOURS TODAY",   value: daily ? daily.totalHours.toFixed(1) : "—" },
    { label: "HOURS THIS WEEK", value: weekly ? weekly.totalHours.toFixed(1) : "—" },
    { label: "SESSIONS LOGGED", value: String(sessions) },
    { label: "WEEKLY AVERAGE",  value: String(avgPerDay) },
  ];

  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
      {cards.map((c) => (
        <div key={c.label} className="rounded-2xl border border-border bg-card p-4 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
          <div className="text-[10px] font-semibold tracking-wider text-muted-foreground">{c.label}</div>
          <div className="mt-1 text-2xl font-bold text-foreground">{c.value}</div>
        </div>
      ))}
    </div>
  );
}
