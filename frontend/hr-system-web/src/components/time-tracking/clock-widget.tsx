"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { apiTimeTracking } from "@/lib/api/time-tracking";
import type { TimeLogDto } from "@/types";

interface Props {
  onSessionChange?: () => void;
}

function formatElapsed(startHHMM: string): string {
  const [h, m] = startHHMM.split(":").map(Number);
  const now = new Date();
  const startMs = new Date(now);
  startMs.setHours(h, m, 0, 0);
  const diffMs = now.getTime() - startMs.getTime();
  const totalMin = Math.max(0, Math.floor(diffMs / 60000));
  const hh = Math.floor(totalMin / 60);
  const mm = totalMin % 60;
  return `${hh}h ${mm}m`;
}

export function ClockWidget({ onSessionChange }: Props) {
  const [session, setSession] = useState<TimeLogDto | null>(null);
  const [, setTick] = useState(0);
  const [submitting, setSubmitting] = useState(false);

  async function refresh() {
    const s = await apiTimeTracking.openSession();
    setSession(s);
  }

  useEffect(() => {
    apiTimeTracking.openSession().then(setSession);
  }, []);

  useEffect(() => {
    if (!session) return;
    const t = setInterval(() => setTick((x) => x + 1), 30_000); // tick every 30s
    return () => clearInterval(t);
  }, [session]);

  async function clockIn() {
    setSubmitting(true);
    await apiTimeTracking.clockIn();
    await refresh();
    onSessionChange?.();
    setSubmitting(false);
  }
  async function clockOut() {
    setSubmitting(true);
    await apiTimeTracking.clockOut();
    await refresh();
    onSessionChange?.();
    setSubmitting(false);
  }

  return (
    <div className="rounded-2xl border border-border bg-card p-5 shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
      {session ? (
        <div className="flex items-center justify-between">
          <div>
            <div className="text-[11px] font-semibold tracking-wider text-muted-foreground">CLOCKED IN</div>
            <div className="mt-1 text-xl font-bold text-foreground">{session.startTime}</div>
            <div className="text-xs text-muted-foreground">Running {formatElapsed(session.startTime)}</div>
          </div>
          <Button onClick={clockOut} disabled={submitting} size="sm">Clock Out</Button>
        </div>
      ) : (
        <div className="flex items-center justify-between">
          <div>
            <div className="text-[11px] font-semibold tracking-wider text-muted-foreground">NOT CLOCKED IN</div>
            <div className="mt-1 text-sm text-muted-foreground">Press Clock In to start a new session.</div>
          </div>
          <Button onClick={clockIn} disabled={submitting} size="sm">Clock In</Button>
        </div>
      )}
    </div>
  );
}
