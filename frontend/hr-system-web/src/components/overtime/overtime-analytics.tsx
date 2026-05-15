// src/components/overtime/overtime-analytics.tsx
"use client";

import { useEffect, useState } from "react";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { apiOvertime } from "@/lib/api/overtime";

export function OvertimeAnalytics() {
  const [data, setData] = useState<Array<{ week: string; hours: number }>>([]);

  useEffect(() => {
    apiOvertime.listAll().then((rs) => {
      // bucket by ISO week
      const buckets: Record<string, number> = {};
      for (const r of rs) {
        const d = new Date(r.date);
        const onejan = new Date(d.getFullYear(), 0, 1);
        const wk = Math.ceil((((d.getTime() - onejan.getTime()) / 86400000) + onejan.getDay() + 1) / 7);
        const key = `W${wk}`;
        buckets[key] = (buckets[key] ?? 0) + r.overtimeHours;
      }
      setData(Object.entries(buckets).map(([week, hours]) => ({ week, hours: Math.round(hours * 10) / 10 })));
    });
  }, []);

  return (
    <div className="rounded-2xl border border-border bg-card p-5">
      <div className="text-sm font-semibold">Overtime by week</div>
      <div className="mt-4 h-48">
        {data.length === 0 ? (
          <div className="flex h-full items-center justify-center text-sm text-muted-foreground">No overtime recorded</div>
        ) : (
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={data}>
              <CartesianGrid strokeDasharray="3 3" stroke="#eef1f6" vertical={false} />
              <XAxis dataKey="week" stroke="#94a3b8" fontSize={11} tickLine={false} axisLine={false} />
              <YAxis stroke="#94a3b8" fontSize={11} tickLine={false} axisLine={false} />
              <Tooltip contentStyle={{ borderRadius: 8, border: "1px solid #e2e8f0", fontSize: 12 }} />
              <Bar dataKey="hours" fill="#2952ec" radius={[6, 6, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        )}
      </div>
    </div>
  );
}
