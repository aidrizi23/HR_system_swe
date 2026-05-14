"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { apiOnboarding } from "@/lib/api/onboarding";
import { apiUsers, type DirectoryUser } from "@/lib/api/users";
import type { OnboardingTemplateDto } from "@/types";

interface Props {
  open: boolean;
  onClose: () => void;
  onAssigned: () => void;
}

export function AssignOnboardingDialog({ open, onClose, onAssigned }: Props) {
  const [employeeId, setEmployeeId] = useState<number | "">("");
  const [templateId, setTemplateId] = useState<number | "">("");
  const [templates, setTemplates] = useState<OnboardingTemplateDto[]>([]);
  const [users, setUsers] = useState<DirectoryUser[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) return;
    apiOnboarding.listTemplates().then(setTemplates);
    apiUsers.list().then(setUsers);
  }, [open]);

  if (!open) return null;

  async function submit() {
    if (!employeeId || !templateId) { setError("Employee and template are required"); return; }
    await apiOnboarding.assign({ employeeId: Number(employeeId), templateId: Number(templateId) });
    onAssigned();
    setEmployeeId(""); setTemplateId(""); setError(null);
    onClose();
  }

  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/30" onClick={onClose} />
      <div className="fixed left-1/2 top-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-2xl border border-border bg-card p-5 shadow-2xl">
        <h3 className="text-lg font-bold">Assign Onboarding</h3>
        <div className="mt-4 space-y-3">
          <div>
            <Label>Employee</Label>
            <select value={employeeId} onChange={(e) => setEmployeeId(e.target.value ? Number(e.target.value) : "")} className="w-full rounded-lg border border-border bg-background p-2 text-sm">
              <option value="">— Select —</option>
              {users.map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
            </select>
          </div>
          <div>
            <Label>Template</Label>
            <select value={templateId} onChange={(e) => setTemplateId(e.target.value ? Number(e.target.value) : "")} className="w-full rounded-lg border border-border bg-background p-2 text-sm">
              <option value="">— Select —</option>
              {templates.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
            </select>
          </div>
          {error && <p className="text-xs text-red-600">{error}</p>}
        </div>
        <div className="mt-4 flex justify-end gap-2">
          <Button type="button" variant="outline" size="sm" onClick={onClose}>Cancel</Button>
          <Button type="button" size="sm" onClick={submit}>Assign</Button>
        </div>
      </div>
    </>
  );
}
