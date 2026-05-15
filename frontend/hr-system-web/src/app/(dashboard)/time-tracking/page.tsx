"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/shared/page-header";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { ClockWidget } from "@/components/time-tracking/clock-widget";
import { TimeKpis } from "@/components/time-tracking/time-kpis";
import { HoursChart } from "@/components/time-tracking/hours-chart";
import { TimeLogsTable } from "@/components/time-tracking/time-logs-table";
import { ModificationRequestDialog } from "@/components/time-tracking/modification-request-dialog";
import { ManualEntryDialog } from "@/components/time-tracking/manual-entry-dialog";
import { apiTimeTracking, timeHelpers } from "@/lib/api/time-tracking";
import { apiUsers, type DirectoryUser } from "@/lib/api/users";
import { type AuthUser, getStoredUser } from "@/lib/auth";
import type { TimeLogDto, ModificationRequestDto } from "@/types";

type Tab = "mine" | "team" | "requests" | "department" | "reports";

const TABS: Array<{ key: Tab; label: string }> = [
  { key: "mine",       label: "My Time" },
  { key: "team",       label: "Team Time" },
  { key: "requests",   label: "Requests" },
  { key: "department", label: "Department" },
  { key: "reports",    label: "Reports" },
];

export default function TimeTrackingPage() {
  const [active, setActive] = useState<Tab>("mine");
  const [date, setDate] = useState<string>(timeHelpers.isoDate(new Date()));
  const [myLogs, setMyLogs] = useState<TimeLogDto[]>([]);
  const [teamLogs, setTeamLogs] = useState<TimeLogDto[]>([]);
  const [requests, setRequests] = useState<ModificationRequestDto[]>([]);
  const [users, setUsers] = useState<DirectoryUser[]>([]);
  const [modTarget, setModTarget] = useState<TimeLogDto | null>(null);
  const [manualOpen, setManualOpen] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);
  const [user, setUser] = useState<AuthUser | null>(null);
  const [pendingMods, setPendingMods] = useState<ModificationRequestDto[]>([]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setUser(getStoredUser());
  }, []);

  const isApprover =
    user?.role === "TeamLead" ||
    user?.role === "DepartmentManager" ||
    user?.role === "HRManager" ||
    user?.role === "SuperAdmin";

  useEffect(() => {
    apiUsers.list().then(setUsers);
  }, []);

  useEffect(() => {
    if (!isApprover) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setPendingMods([]);
      return;
    }
    apiTimeTracking.listPendingModifications()
      .then(setPendingMods)
      .catch(() => setPendingMods([]));
  }, [isApprover, refreshKey]);

  async function approveMod(id: number) {
    try {
      await apiTimeTracking.approveModification(id);
      bump();
    } catch { /* swallow — refresh on next bump */ }
  }

  async function rejectMod(id: number) {
    const reason = window.prompt("Reject reason?");
    if (!reason) return;
    try {
      await apiTimeTracking.rejectModification(id, reason);
      bump();
    } catch { /* swallow */ }
  }

  useEffect(() => {
    // "My Time" tab shows ALL the current user's logs across dates, not filtered.
    apiTimeTracking.listMine().then(setMyLogs).catch(() => setMyLogs([]));
  }, [refreshKey]);

  useEffect(() => {
    // "Team Time" tab is date-filtered. Falls back to the user's own entries for that
    // date when scope is empty (employee with no team to manage).
    Promise.all([apiTimeTracking.listTeam(date), apiTimeTracking.listMine(date)])
      .then(([team, mine]) => {
        const seen = new Set(team.map((t) => t.id));
        setTeamLogs([...team, ...mine.filter((m) => !seen.has(m.id))]);
      });
  }, [date, refreshKey]);

  useEffect(() => {
    apiTimeTracking.listModifications().then(setRequests);
  }, [refreshKey]);

  const bump = () => setRefreshKey((x) => x + 1);

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <PageHeader
          title="Time Tracking"
          subtitle="Log and manage your work sessions."
        />
        <Button variant="outline" size="sm" onClick={() => setManualOpen(true)}>
          Log time manually
        </Button>
      </div>
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

      {active === "mine" && (
        <div className="space-y-3">
          <p className="text-xs text-muted-foreground">
            All time entries you&apos;ve logged. Click <span className="font-semibold">Request edit</span> on any row to propose a change.
          </p>
          <TimeLogsTable
            logs={myLogs}
            users={users}
            currentEmployeeId={user?.employeeId ?? null}
            onRequestModification={(l) => setModTarget(l)}
            emptyMessage="You haven't logged any time yet. Use Clock In or Log time manually to get started."
          />
        </div>
      )}

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
            currentEmployeeId={user?.employeeId ?? null}
            onRequestModification={(l) => setModTarget(l)}
          />
        </div>
      )}

      {active === "requests" && (
        <div className="space-y-6">
          {isApprover && (
            <div>
              <h3 className="mb-2 text-[11px] font-bold uppercase tracking-wider text-muted-foreground">
                Pending approvals · {pendingMods.length}
              </h3>
              <div className="overflow-hidden rounded-2xl border border-border bg-card">
                {pendingMods.length === 0 ? (
                  <div className="p-8 text-center text-sm text-muted-foreground">No pending modification requests in your scope</div>
                ) : (
                  <table className="w-full text-sm">
                    <thead className="bg-muted/40 text-[10px] uppercase tracking-wider text-muted-foreground">
                      <tr>
                        <th className="px-4 py-3 text-left">Employee</th>
                        <th className="px-4 py-3 text-left">Submitted</th>
                        <th className="px-4 py-3 text-left">Requested start</th>
                        <th className="px-4 py-3 text-left">Requested end</th>
                        <th className="px-4 py-3 text-left">Reason</th>
                        <th className="px-4 py-3 text-right">Action</th>
                      </tr>
                    </thead>
                    <tbody>
                      {pendingMods.map((r) => {
                        const emp = users.find((u) => u.id === r.employeeId);
                        return (
                          <tr key={r.id} className="border-t border-border">
                            <td className="px-4 py-3 font-medium">{emp?.name ?? `#${r.employeeId}`}</td>
                            <td className="px-4 py-3 text-muted-foreground">{new Date(r.createdAt).toLocaleDateString()}</td>
                            <td className="px-4 py-3">{r.requestedStartTime}</td>
                            <td className="px-4 py-3">{r.requestedEndTime}</td>
                            <td className="px-4 py-3 text-muted-foreground">{r.reason ?? "—"}</td>
                            <td className="px-4 py-3 text-right">
                              <button
                                type="button"
                                onClick={() => approveMod(r.id)}
                                className="mr-3 text-xs font-semibold text-emerald-700 hover:underline"
                              >
                                Approve
                              </button>
                              <button
                                type="button"
                                onClick={() => rejectMod(r.id)}
                                className="text-xs font-semibold text-red-600 hover:underline"
                              >
                                Reject
                              </button>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                )}
              </div>
            </div>
          )}

          <div>
            <h3 className="mb-2 text-[11px] font-bold uppercase tracking-wider text-muted-foreground">
              My requests · {requests.length}
            </h3>
            <div className="overflow-hidden rounded-2xl border border-border bg-card">
              {requests.length === 0 ? (
                <div className="p-8 text-center text-sm text-muted-foreground">You haven&apos;t submitted any modification requests</div>
              ) : (
                <table className="w-full text-sm">
                  <thead className="bg-muted/40 text-[10px] uppercase tracking-wider text-muted-foreground">
                    <tr>
                      <th className="px-4 py-3 text-left">Submitted</th>
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
          </div>
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
      <ManualEntryDialog
        open={manualOpen}
        onClose={() => setManualOpen(false)}
        onSubmitted={(savedDate) => {
          // Jump the table's date picker to whatever date the user just logged so the
          // entry is immediately visible without them hunting for the right day.
          if (savedDate) setDate(savedDate);
          setActive("team");
          bump();
        }}
      />
    </div>
  );
}
