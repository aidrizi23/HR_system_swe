"use client";

import { useEffect, useState } from "react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { apiTimeTracking } from "@/lib/api/time-tracking";
import type { WeeklySummaryDto } from "@/types";

const DAY_LABELS = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

interface Props { refreshKey?: number; }

export function HoursChart({ refreshKey }: Props) {
  const [data, setData] = useState<Array<{ day: string; hours: number }>>([]);

  useEffect(() => {
    apiTimeTracking.weeklySummary().then((w: WeeklySummaryDto) => {
      const points = w.days.map((d, i) => ({ day: DAY_LABELS[i], hours: Math.round(d.totalHours * 10) / 10 }));
      setData(points);
    });
  }, [refreshKey]);

  return (
    <div className="rounded-2xl border border-border bg-card p-5 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
      <div className="text-sm font-semibold text-foreground">Hours this week</div>
      <div className="mt-4 h-56">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={data}>
            <CartesianGrid strokeDasharray="3 3" stroke="#eef1f6" vertical={false} />
            <XAxis dataKey="day" stroke="#94a3b8" fontSize={11} tickLine={false} axisLine={false} />
            <YAxis stroke="#94a3b8" fontSize={11} tickLine={false} axisLine={false} domain={[0, (max: number) => Math.max(10, Math.ceil(max))]} />
            <Tooltip contentStyle={{ borderRadius: 8, border: "1px solid #e2e8f0", fontSize: 12 }} />
            <Bar dataKey="hours" fill="#2952ec" radius={[6, 6, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
