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
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!log) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setStart(log.startTime);
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setEnd(log.endTime ?? "");
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setReason("");
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setError(null);
  }, [log]);

  if (!log) return null;

  async function submit() {
    if (!log) return;
    setError(null);
    if (!start || !end) {
      setError("Start and end times are required.");
      return;
    }
    setSubmitting(true);
    try {
      await apiTimeTracking.createModification({
        timeLogId: log.id,
        requestedStartTime: start,
        requestedEndTime: end,
        reason: reason || undefined,
      });
      onSubmitted();
      onClose();
    } catch (e: unknown) {
      // Surface the API's error message so the user knows why it failed
      // (e.g. 401 "Cannot request modification for another employee's log").
      let msg = "Could not submit the modification.";
      if (typeof e === "object" && e !== null && "response" in e) {
        const resp = (e as { response?: { status?: number; data?: { message?: string } } }).response;
        if (resp?.status === 401 || resp?.status === 403) {
          msg = "You can only request modifications for your own time logs.";
        } else if (resp?.data?.message) {
          msg = resp.data.message;
        }
      }
      setError(msg);
    } finally {
      setSubmitting(false);
    }
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
        {error && <p className="mt-2 text-xs text-red-600">{error}</p>}
        <div className="mt-4 flex justify-end gap-2">
          <Button type="button" variant="outline" size="sm" onClick={onClose}>Cancel</Button>
          <Button type="button" size="sm" onClick={submit} disabled={submitting}>
            {submitting ? "Submitting…" : "Submit"}
          </Button>
        </div>
      </div>
    </>
  );
}
