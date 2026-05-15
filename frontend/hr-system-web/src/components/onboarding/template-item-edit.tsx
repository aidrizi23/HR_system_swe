// src/components/onboarding/template-item-edit.tsx
"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select } from "@/components/ui/select";
import type { OnboardingTemplateItemDto, OnboardingResponsibleRole } from "@/types";

const ROLES: OnboardingResponsibleRole[] = ["Employee", "HR", "Manager", "IT"];

interface Props {
  initial?: Partial<OnboardingTemplateItemDto>;
  onSave: (values: { description: string; responsibleRole: OnboardingResponsibleRole; defaultDueDays: number }) => void;
  onCancel: () => void;
}

export function TemplateItemEdit({ initial, onSave, onCancel }: Props) {
  const [description, setDescription] = useState(initial?.description ?? "");
  const [responsibleRole, setRole] = useState<OnboardingResponsibleRole>(initial?.responsibleRole ?? "Employee");
  const [defaultDueDays, setDays] = useState<string>(String(initial?.defaultDueDays ?? 1));
  const [error, setError] = useState<string | null>(null);

  function save() {
    setError(null);
    if (!description.trim()) { setError("Description is required"); return; }
    const days = parseInt(defaultDueDays, 10);
    if (!days || days < 1 || days > 365) { setError("Due days must be 1–365"); return; }
    onSave({ description: description.trim(), responsibleRole, defaultDueDays: days });
  }

  return (
    <div className="space-y-2 rounded-lg border border-primary/30 bg-primary/5 p-3">
      <div>
        <Label>Description</Label>
        <Input value={description} onChange={(e) => setDescription(e.target.value)} maxLength={500} />
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <Label>Owner</Label>
          <Select
            value={responsibleRole}
            onChange={(v) => setRole(v as OnboardingResponsibleRole)}
            options={ROLES.map((r) => ({ value: r, label: r }))}
          />
        </div>
        <div>
          <Label>Due day offset</Label>
          <Input type="number" min="1" max="365" value={defaultDueDays} onChange={(e) => setDays(e.target.value)} />
        </div>
      </div>
      {error && <p className="text-xs text-red-600">{error}</p>}
      <div className="flex justify-end gap-2 pt-1">
        <Button type="button" variant="outline" size="sm" onClick={onCancel}>Cancel</Button>
        <Button type="button" size="sm" onClick={save}>Save</Button>
      </div>
    </div>
  );
}
