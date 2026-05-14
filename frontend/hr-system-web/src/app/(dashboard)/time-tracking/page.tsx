"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/shared/page-header";
import { Input } from "@/components/ui/input";
import { ClockWidget } from "@/components/time-tracking/clock-widget";
import { TimeKpis } from "@/components/time-tracking/time-kpis";
import { HoursChart } from "@/components/time-tracking/hours-chart";
import { TimeLogsTable } from "@/components/time-tracking/time-logs-table";
import { ModificationRequestDialog } from "@/components/time-tracking/modification-request-dialog";
import { apiTimeTracking, timeHelpers } from "@/lib/api/time-tracking";
import { apiUsers, type DirectoryUser } from "@/lib/api/users";
import type { TimeLogDto, ModificationRequestDto } from "@/types";

type Tab = "team" | "requests" | "department" | "reports";

const TABS: Array<{ key: Tab; label: string }> = [
  { key: "team",       label: "Team Time" },
  { key: "requests",   label: "Requests" },
  { key: "department", label: "Department" },
  { key: "reports",    label: "Reports" },
];

export default function TimeTrackingPage() {
  const [active, setActive] = useState<Tab>("team");
  const [date, setDate] = useState<string>(timeHelpers.isoDate(new Date()));
  const [teamLogs, setTeamLogs] = useState<TimeLogDto[]>([]);
  const [requests, setRequests] = useState<ModificationRequestDto[]>([]);
  const [users, setUsers] = useState<DirectoryUser[]>([]);
  const [modTarget, setModTarget] = useState<TimeLogDto | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    apiUsers.list().then(setUsers);
  }, []);

  useEffect(() => {
    apiTimeTracking.listTeam(date).then(setTeamLogs);
  }, [date, refreshKey]);

  useEffect(() => {
    apiTimeTracking.listModifications().then(setRequests);
  }, [refreshKey]);

  const bump = () => setRefreshKey((x) => x + 1);

  return (
    <div className="space-y-6">
      <PageHeader
        title="Time Tracking"
        subtitle="Log and manage your work sessions."
      />
      <ClockWidget onSessionChange={bump} />
      <TimeKpis refreshKey={refreshKey} />
      <HoursChart refreshKey={refreshKey} />

      <div className="flex gap-6 border-b border-border">
        {TABS.map((t) => (
          <button
            key={t.key}
            onClick={() => setActive(t.key)}
            className={`-mb-px border-b-2 py-2 text-sm transition ${
              active === t.key
                ? "border-primary font-semibold text-primary"
                : "border-transparent text-muted-foreground hover:text-foreground"
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {active === "team" && (
        <div className="space-y-3">
          <div className="flex items-center gap-2">
            <span className="text-xs text-muted-foreground">Date</span>
            <Input
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              className="w-40"
            />
          </div>
          <TimeLogsTable
            logs={teamLogs}
            users={users}
            onRequestModification={(l) => setModTarget(l)}
          />
        </div>
      )}

      {active === "requests" && (
        <div className="overflow-hidden rounded-2xl border border-border bg-card">
          {requests.length === 0 ? (
            <div className="p-12 text-center text-sm text-muted-foreground">No modification requests</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="bg-muted/40 text-[10px] uppercase tracking-wider text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 text-left">Date</th>
                  <th className="px-4 py-3 text-left">Requested start</th>
                  <th className="px-4 py-3 text-left">Requested end</th>
                  <th className="px-4 py-3 text-left">Reason</th>
                  <th className="px-4 py-3 text-left">Status</th>
                </tr>
              </thead>
              <tbody>
                {requests.map((r) => (
                  <tr key={r.id} className="border-t border-border">
                    <td className="px-4 py-3">{new Date(r.createdAt).toLocaleDateString()}</td>
                    <td className="px-4 py-3">{r.requestedStartTime}</td>
                    <td className="px-4 py-3">{r.requestedEndTime}</td>
                    <td className="px-4 py-3 text-muted-foreground">{r.reason ?? "—"}</td>
                    <td className="px-4 py-3">
                      <span className="rounded-full bg-amber-100 px-2 py-0.5 text-[10px] font-semibold text-amber-800">
                        {r.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {active === "department" && (
        <div className="rounded-2xl border border-border bg-card p-12 text-center text-sm text-muted-foreground">
          Department aggregate view lands when B&apos;s leave-and-time-tracking backend ships.
        </div>
      )}

      {active === "reports" && (
        <div className="rounded-2xl border border-border bg-card p-12 text-center text-sm text-muted-foreground">
          Reports land when B&apos;s leave-and-time-tracking backend ships.
        </div>
      )}

      <ModificationRequestDialog
        log={modTarget}
        onClose={() => setModTarget(null)}
        onSubmitted={bump}
      />
    </div>
  );
}
