"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { apiTimeTracking, timeHelpers } from "@/lib/api/time-tracking";

interface Props {
  open: boolean;
  onClose: () => void;
  onSubmitted: (savedDate: string) => void;
}

export function ManualEntryDialog({ open, onClose, onSubmitted }: Props) {
  const [date, setDate] = useState(timeHelpers.isoDate(new Date()));
  const [hours, setHours] = useState("");
  const [notes, setNotes] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!open) return null;

  async function submit() {
    setError(null);
    const h = Number(hours);
    if (!h || h <= 0 || h > 16) {
      setError("Enter hours between 0 and 16.");
      return;
    }
    setSubmitting(true);
    try {
      await apiTimeTracking.manualEntry({ date, hours: h, notes: notes || undefined });
      const savedDate = date;
      setHours("");
      setNotes("");
      onSubmitted(savedDate);
      onClose();
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : "Failed to submit";
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/30" onClick={onClose} />
      <div className="fixed left-1/2 top-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-2xl border border-border bg-card p-5 shadow-2xl">
        <h3 className="text-lg font-bold">Log time manually</h3>
        <p className="mt-1 text-xs text-muted-foreground">
          Retroactively record hours you already worked. Start time defaults to 09:00.
        </p>

        <div className="mt-4 grid grid-cols-2 gap-3">
          <div>
            <Label>Date</Label>
            <Input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          </div>
          <div>
            <Label>Hours</Label>
            <Input
              type="number"
              min="0.25"
              max="16"
              step="0.25"
              placeholder="e.g. 4.5"
              value={hours}
              onChange={(e) => setHours(e.target.value)}
            />
          </div>
        </div>

        <div className="mt-3">
          <Label>Notes (optional)</Label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={2}
            maxLength={500}
            className="w-full rounded-lg border border-border bg-background p-2 text-sm"
            placeholder="e.g. client call + ticket triage"
          />
        </div>

        {error && <p className="mt-2 text-xs text-red-600">{error}</p>}

        <div className="mt-4 flex justify-end gap-2">
          <Button type="button" variant="outline" size="sm" onClick={onClose}>Cancel</Button>
          <Button type="button" size="sm" onClick={submit} disabled={submitting}>
            {submitting ? "Saving…" : "Save entry"}
          </Button>
        </div>
      </div>
    </>
  );
}
