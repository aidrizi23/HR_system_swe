"use client";

import { useCallback, useEffect, useState } from "react";
import { PageHeader } from "@/components/shared/page-header";
import { Button } from "@/components/ui/button";
import { OvertimeForm } from "@/components/overtime/overtime-form";
import { OvertimeList } from "@/components/overtime/overtime-list";
import { OvertimeAnalytics } from "@/components/overtime/overtime-analytics";
import { apiOvertime } from "@/lib/api/overtime";
import { getCurrentMockUser, isHrOrAbove } from "@/lib/mock/users";
import type { OvertimeRecordDto } from "@/types";

type Tab = "approvals" | "team" | "analytics";

const TABS: Array<{ key: Tab; label: string }> = [
  { key: "approvals", label: "Approvals" },
  { key: "team",      label: "Team Overview" },
  { key: "analytics", label: "Analytics" },
];

function extractError(e: unknown, fallback: string): string {
  if (typeof e === "object" && e !== null && "response" in e) {
    const resp = (e as { response?: { data?: { message?: string } } }).response;
    if (resp?.data?.message) return resp.data.message;
  }
  return fallback;
}

export default function OvertimePage() {
  const me = getCurrentMockUser();
  const isHr = isHrOrAbove(me.role);
  const [active, setActive] = useState<Tab>("approvals");
  const [pending, setPending] = useState<OvertimeRecordDto[]>([]);
  const [all, setAll] = useState<OvertimeRecordDto[]>([]);
  const [formOpen, setFormOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(() => {
    apiOvertime.listPending().then(setPending).catch(() => setPending([]));
    apiOvertime.listAll().then(setAll).catch(() => setAll([]));
  }, []);

  useEffect(() => { refresh(); }, [refresh]);

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

      {active === "approvals" && (
        <OvertimeList
          records={pending}
          showApprovalActions
          onRecommend={recommend}
          onApprove={isHr ? approve : undefined}
          onReject={reject}
          emptyMessage="No pending overtime requests"
        />
      )}
      {active === "team"      && <OvertimeList records={all} emptyMessage="No overtime records" />}
      {active === "analytics" && <OvertimeAnalytics />}

      <OvertimeForm open={formOpen} onClose={() => setFormOpen(false)} onSubmitted={refresh} />
    </div>
  );
}
