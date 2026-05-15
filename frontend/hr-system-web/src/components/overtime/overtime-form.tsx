// src/components/overtime/overtime-form.tsx
"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { apiOvertime } from "@/lib/api/overtime";

interface Props {
  open: boolean;
  onClose: () => void;
  onSubmitted: () => void;
}

export function OvertimeForm({ open, onClose, onSubmitted }: Props) {
  const [date, setDate] = useState("");
  const [hours, setHours] = useState("");
  const [reason, setReason] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (!open) return null;

  async function submit() {
    setError(null);
    const h = parseFloat(hours);
    if (!date) { setError("Date required"); return; }
    if (!h || h <= 0 || h > 12) { setError("Hours must be 0.1 to 12"); return; }
    setSubmitting(true);
    await apiOvertime.create({
      date,
      overtimeMinutes: Math.round(h * 60),
      reason: reason || undefined,
    });
    setSubmitting(false);
    setDate(""); setHours(""); setReason("");
    onSubmitted();
    onClose();
  }

  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/30" onClick={onClose} />
      <div className="fixed left-1/2 top-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-2xl border border-border bg-card p-5 shadow-2xl">
        <h3 className="text-lg font-bold">Submit overtime</h3>
        <div className="mt-4 grid grid-cols-2 gap-3">
          <div>
            <Label>Date</Label>
            <Input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          </div>
          <div>
            <Label>Hours</Label>
            <Input type="number" step="0.1" min="0.1" max="12" value={hours} onChange={(e) => setHours(e.target.value)} />
          </div>
        </div>
        <div className="mt-3">
          <Label>Reason</Label>
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={3}
            maxLength={1000}
            className="w-full rounded-lg border border-border bg-background p-2 text-sm"
          />
        </div>
        {error && <p className="mt-2 text-xs text-red-600">{error}</p>}
        <div className="mt-4 flex justify-end gap-2">
          <Button type="button" variant="outline" size="sm" onClick={onClose}>Cancel</Button>
          <Button type="button" size="sm" onClick={submit} disabled={submitting}>Submit</Button>
        </div>
      </div>
    </>
  );
}
