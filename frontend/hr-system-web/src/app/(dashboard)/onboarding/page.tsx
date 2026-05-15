// src/app/(dashboard)/onboarding/page.tsx
"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/shared/page-header";
import { Button } from "@/components/ui/button";
import { ActiveChecklists } from "@/components/onboarding/active-checklists";
import { TemplateEditor } from "@/components/onboarding/template-editor";
import { AssignOnboardingDialog } from "@/components/onboarding/assign-onboarding-dialog";
import { apiOnboarding } from "@/lib/api/onboarding";
import { getCurrentMockUser, isHrOrAbove } from "@/lib/mock/users";
import type { OnboardingChecklistDto } from "@/types";

type Tab = "active" | "templates" | "analytics";

const TABS: Array<{ key: Tab; label: string; hrOnly: boolean }> = [
  { key: "active",    label: "Active",    hrOnly: false },
  { key: "templates", label: "Templates", hrOnly: true },
  { key: "analytics", label: "Analytics", hrOnly: false },
];

export default function OnboardingPage() {
  const me = getCurrentMockUser();
  const isHr = isHrOrAbove(me.role);
  const visibleTabs = TABS.filter((t) => !t.hrOnly || isHr);
  const [active, setActive] = useState<Tab>("active");
  const [assignOpen, setAssignOpen] = useState(false);
  const [checklists, setChecklists] = useState<OnboardingChecklistDto[]>([]);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => { apiOnboarding.listChecklists().then(setChecklists); }, [refreshKey]);

  const activeCount = checklists.filter((c) => c.status === "Active").length;
  const completedCount = checklists.filter((c) => c.status === "Completed").length;
  const avgProgress = checklists.length === 0
    ? 0
    : Math.round(checklists.reduce((s, c) => s + (c.totalItems > 0 ? c.completedItems / c.totalItems : 0), 0) / checklists.length * 100);

  return (
    <div className="space-y-6">
      <PageHeader
        title="Onboarding"
        subtitle="Manage employee onboarding templates and checklists."
        actions={isHr ? <Button size="sm" onClick={() => setAssignOpen(true)}>+ Assign Onboarding</Button> : undefined}
      />

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
        {[
          { label: "ACTIVE",       value: String(activeCount) },
          { label: "COMPLETED",    value: String(completedCount) },
          { label: "AVG. PROGRESS",value: `${avgProgress}%` },
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

      {active === "active"    && <ActiveChecklists refreshKey={refreshKey} />}
      {active === "templates" && isHr && <TemplateEditor />}
      {active === "analytics" && (
        <div className="rounded-2xl border border-border bg-card p-12 text-center text-sm text-muted-foreground">
          Analytics aggregate ships in a later branch.
        </div>
      )}

      <AssignOnboardingDialog
        open={assignOpen}
        onClose={() => setAssignOpen(false)}
        onAssigned={() => setRefreshKey((x) => x + 1)}
      />
    </div>
  );
}
