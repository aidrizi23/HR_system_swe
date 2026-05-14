"use client";

import type { TimeLogDto } from "@/types";
import type { DirectoryUser } from "@/lib/api/users";

interface Props {
  logs: TimeLogDto[];
  users: DirectoryUser[];
  emptyMessage?: string;
  onRequestModification?: (log: TimeLogDto) => void;
}

function fmtDate(iso: string): string {
  return new Date(iso).toLocaleDateString();
}

function fmtDuration(min: number): string {
  if (min === 0) return "—";
  const h = Math.floor(min / 60);
  const m = min % 60;
  return `${h}h ${m}m`;
}

export function TimeLogsTable({ logs, users, emptyMessage = "No team time logs found for this date", onRequestModification }: Props) {
  if (logs.length === 0) {
    return (
      <div className="rounded-2xl border border-border bg-card p-12 text-center text-sm text-muted-foreground">
        {emptyMessage}
      </div>
    );
  }
  return (
    <div className="overflow-hidden rounded-2xl border border-border bg-card">
      <table className="w-full text-sm">
        <thead className="bg-muted/40 text-[10px] uppercase tracking-wider text-muted-foreground">
          <tr>
            <th className="px-4 py-3 text-left">Employee</th>
            <th className="px-4 py-3 text-left">Date</th>
            <th className="px-4 py-3 text-left">Start</th>
            <th className="px-4 py-3 text-left">End</th>
            <th className="px-4 py-3 text-left">Duration</th>
            {onRequestModification && <th className="px-4 py-3 text-right">Actions</th>}
          </tr>
        </thead>
        <tbody>
          {logs.map((l) => {
            const emp = users.find((u) => u.id === l.employeeId);
            return (
              <tr key={l.id} className="border-t border-border hover:bg-muted/30">
                <td className="px-4 py-3 font-medium">{emp?.name ?? `#${l.employeeId}`}</td>
                <td className="px-4 py-3 text-muted-foreground">{fmtDate(l.date)}</td>
                <td className="px-4 py-3">{l.startTime}</td>
                <td className="px-4 py-3">{l.endTime ?? "(open)"}</td>
                <td className="px-4 py-3">{fmtDuration(l.durationMinutes)}</td>
                {onRequestModification && (
                  <td className="px-4 py-3 text-right">
                    <button
                      type="button"
                      className="text-xs text-primary hover:underline"
                      onClick={() => onRequestModification(l)}
                    >
                      Request edit
                    </button>
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
