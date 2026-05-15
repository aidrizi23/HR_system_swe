// src/components/onboarding/active-checklists.tsx
"use client";

import { useEffect, useState } from "react";
import { apiOnboarding } from "@/lib/api/onboarding";
import { ChecklistCard } from "./checklist-card";
import type { OnboardingChecklistDto } from "@/types";

interface Props { refreshKey?: number; }

export function ActiveChecklists({ refreshKey }: Props) {
  const [checklists, setChecklists] = useState<OnboardingChecklistDto[]>([]);
  const [bump, setBump] = useState(0);

  useEffect(() => {
    apiOnboarding.listChecklists().then((cs) => setChecklists(cs.filter((c) => c.status === "Active")));
  }, [refreshKey, bump]);

  if (checklists.length === 0) {
    return (
      <div className="rounded-2xl border border-border bg-card p-12 text-center text-sm text-muted-foreground">
        No active onboardings. Assign a template to an employee to begin.
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
      {checklists.map((c) => (
        <ChecklistCard key={c.id} checklist={c} onChanged={() => setBump((x) => x + 1)} />
      ))}
    </div>
  );
}
