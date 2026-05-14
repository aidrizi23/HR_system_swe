// src/components/onboarding/checklist-card.tsx
"use client";

import { apiOnboarding } from "@/lib/api/onboarding";
import type { OnboardingChecklistDto } from "@/types";

interface Props {
  checklist: OnboardingChecklistDto;
  onChanged: () => void;
}

export function ChecklistCard({ checklist, onChanged }: Props) {
  const pct = checklist.totalItems === 0 ? 0 : Math.round((checklist.completedItems / checklist.totalItems) * 100);

  async function complete(itemId: number) {
    await apiOnboarding.markItemComplete(checklist.id, itemId);
    onChanged();
  }

  return (
    <div className="rounded-2xl border border-border bg-card p-5 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
      <div className="flex items-center justify-between">
        <div>
          <div className="text-sm font-semibold">{checklist.employeeName}</div>
          <div className="text-xs text-muted-foreground">{checklist.templateName}</div>
        </div>
        <div className="text-right">
          <div className="text-xs text-muted-foreground">{checklist.completedItems}/{checklist.totalItems} done</div>
          <div className="mt-1 h-2 w-32 overflow-hidden rounded-full bg-muted">
            <div className="h-full bg-primary" style={{ width: `${pct}%` }} />
          </div>
        </div>
      </div>
      <ul className="mt-4 space-y-2">
        {checklist.items.map((it) => (
          <li key={it.id} className="flex items-center gap-3 text-sm">
            <input
              type="checkbox"
              checked={it.status === "Completed"}
              onChange={() => it.status !== "Completed" && complete(it.id)}
              disabled={it.status === "Completed"}
              className="h-4 w-4 rounded border-border"
            />
            <span className={it.status === "Completed" ? "text-muted-foreground line-through" : ""}>
              {it.description}
            </span>
            <span className="ml-auto text-[10px] text-muted-foreground">
              {new Date(it.dueDate).toLocaleDateString()}
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}
