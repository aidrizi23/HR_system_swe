"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/shared/page-header";
import { Button } from "@/components/ui/button";
import { OvertimeForm } from "@/components/overtime/overtime-form";
import { OvertimeList } from "@/components/overtime/overtime-list";
import { OvertimeAnalytics } from "@/components/overtime/overtime-analytics";
import { apiOvertime } from "@/lib/api/overtime";
import { type AuthUser, getStoredUser } from "@/lib/auth";
import type { OvertimeRecordDto } from "@/types";

type Tab = "mine" | "approvals" | "team" | "analytics";

const ALL_TABS: Array<{ key: Tab; label: string; approverOnly?: boolean }> = [
  { key: "mine",      label: "My Overtime" },
  { key: "approvals", label: "Approvals",      approverOnly: true },
  { key: "team",      label: "Team Overview",  approverOnly: true },
  { key: "analytics", label: "Analytics" },
];

function isApproverRole(role: string | undefined): boolean {
  return role === "TeamLead" || role === "DepartmentManager" || role === "HRManager" || role === "SuperAdmin";
}
function isHrRole(role: string | undefined): boolean {
  return role === "HRManager" || role === "SuperAdmin";
}

function extractError(e: unknown, fallback: string): string {
  if (typeof e === "object" && e !== null && "response" in e) {
    const resp = (e as { response?: { data?: { message?: string } } }).response;
    if (resp?.data?.message) return resp.data.message;
  }
  return fallback;
}

export default function OvertimePage() {
  const [user, setUser] = useState<AuthUser | null>(null);
  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setUser(getStoredUser());
  }, []);

  const isApprover = isApproverRole(user?.role);
  const isHr = isHrRole(user?.role);
  const tabs = ALL_TABS.filter((t) => !t.approverOnly || isApprover);

  const [active, setActive] = useState<Tab>("mine");
  const [mine, setMine]       = useState<OvertimeRecordDto[]>([]);
  const [pending, setPending] = useState<OvertimeRecordDto[]>([]);
  const [all, setAll]         = useState<OvertimeRecordDto[]>([]);
  const [formOpen, setFormOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // React Compiler memoizes this automatically; no useCallback needed.
  function refresh() {
    apiOvertime.listMine().then(setMine).catch(() => setMine([]));
    if (isApprover) {
      apiOvertime.listPending().then(setPending).catch(() => setPending([]));
      apiOvertime.listAll().then(setAll).catch(() => setAll([]));
    }
  }

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    refresh();
  }, [isApprover]);

  async function recommend(id: number) {
    try { await apiOvertime.recommend(id); refresh(); }
    catch (e) { setError(extractError(e, "Recommend failed")); }
  }
  async function approve(id: number) {
    try { await apiOvertime.approve(id); refresh(); }
    catch (e) { setError(extractError(e, "Approve failed")); }
  }
  async function reject(id: number) {
    const reason = window.prompt("Reject reason?");
    if (!reason) return;
    try { await apiOvertime.reject(id, { reason }); refresh(); }
    catch (e) { setError(extractError(e, "Reject failed")); }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Overtime"
        subtitle="Review overtime records, approvals, and detected exceptions."
        actions={<Button size="sm" onClick={() => setFormOpen(true)}>Submit overtime</Button>}
      />

      {error && (
        <div className="flex items-start gap-3 rounded-2xl border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          <span className="flex-1">{error}</span>
          <button type="button" onClick={() => setError(null)} className="text-red-500 hover:text-red-700">×</button>
        </div>
      )}

      <div className="flex gap-6 border-b border-border">
        {tabs.map((t) => (
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
        <OvertimeList
          records={mine}
          emptyMessage="You haven't submitted any overtime yet. Click 'Submit overtime' to log one."
        />
      )}
      {active === "approvals" && isApprover && (
        <OvertimeList
          records={pending}
          showApprovalActions
          onRecommend={recommend}
          onApprove={isHr ? approve : undefined}
          onReject={reject}
          emptyMessage="No pending overtime requests"
        />
      )}
      {active === "team"      && isApprover && <OvertimeList records={all} emptyMessage="No overtime records" />}
      {active === "analytics" && <OvertimeAnalytics />}

      <OvertimeForm open={formOpen} onClose={() => setFormOpen(false)} onSubmitted={refresh} />
    </div>
  );
}
