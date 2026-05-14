"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { apiTimeTracking } from "@/lib/api/time-tracking";
import type { TimeLogDto } from "@/types";

interface Props {
  log: TimeLogDto | null;
  onClose: () => void;
  onSubmitted: () => void;
}

export function ModificationRequestDialog({ log, onClose, onSubmitted }: Props) {
  const [start, setStart] = useState("");
  const [end, setEnd] = useState("");
  const [reason, setReason] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!log) return;
    Promise.resolve({ start: log.startTime, end: log.endTime ?? "", reason: "" }).then(
      ({ start: s, end: e, reason: r }) => {
        setStart(s);
        setEnd(e);
        setReason(r);
      }
    );
  }, [log]);

  if (!log) return null;

  async function submit() {
    if (!log) return;
    setSubmitting(true);
    await apiTimeTracking.createModification({
      timeLogId: log.id,
      requestedStartTime: start,
      requestedEndTime: end,
      reason: reason || undefined,
    });
    setSubmitting(false);
    onSubmitted();
    onClose();
  }

  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/30" onClick={onClose} />
      <div className="fixed left-1/2 top-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-2xl border border-border bg-card p-5 shadow-2xl">
        <h3 className="text-lg font-bold">Request modification</h3>
        <div className="mt-4 grid grid-cols-2 gap-3">
          <div>
            <Label>Start</Label>
            <Input type="time" value={start} onChange={(e) => setStart(e.target.value)} />
          </div>
          <div>
            <Label>End</Label>
            <Input type="time" value={end} onChange={(e) => setEnd(e.target.value)} />
          </div>
        </div>
        <div className="mt-3">
          <Label>Reason</Label>
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={3}
            maxLength={500}
            className="w-full rounded-lg border border-border bg-background p-2 text-sm"
          />
        </div>
        <div className="mt-4 flex justify-end gap-2">
          <Button type="button" variant="outline" size="sm" onClick={onClose}>Cancel</Button>
          <Button type="button" size="sm" onClick={submit} disabled={submitting}>Submit</Button>
        </div>
      </div>
    </>
  );
}
