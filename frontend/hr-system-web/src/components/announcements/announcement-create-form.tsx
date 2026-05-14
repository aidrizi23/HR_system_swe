// src/components/announcements/announcement-create-form.tsx
"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { apiAnnouncements } from "@/lib/api/announcements";

interface Props { onCreated: () => void; }

const PRIORITY_LABEL = ["Low", "Normal", "High"];

export function AnnouncementCreateForm({ onCreated }: Props) {
  const [title, setTitle] = useState("");
  const [body, setBody] = useState("");
  const [priority, setPriority] = useState(1);
  const [pinned, setPinned] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function submit() {
    setError(null);
    if (!title.trim()) { setError("Title is required"); return; }
    if (!body.trim()) { setError("Body is required"); return; }
    setSubmitting(true);
    await apiAnnouncements.create({
      title: title.trim(),
      body: body.trim(),
      priority,
      isPinned: pinned,
    });
    setSubmitting(false);
    setTitle(""); setBody(""); setPriority(1); setPinned(false);
    onCreated();
  }

  return (
    <div className="rounded-2xl border border-border bg-card p-5">
      <h3 className="text-sm font-semibold">New announcement</h3>
      <div className="mt-4 space-y-3">
        <div>
          <Label>Title</Label>
          <Input value={title} onChange={(e) => setTitle(e.target.value)} maxLength={300} />
        </div>
        <div>
          <Label>Body</Label>
          <textarea
            value={body}
            onChange={(e) => setBody(e.target.value)}
            rows={6}
            maxLength={5000}
            className="w-full rounded-lg border border-border bg-background p-2 text-sm"
          />
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <Label>Priority</Label>
            <select value={priority} onChange={(e) => setPriority(Number(e.target.value))} className="w-full rounded-lg border border-border bg-background p-2 text-sm">
              {PRIORITY_LABEL.map((p, i) => <option key={p} value={i}>{p}</option>)}
            </select>
          </div>
          <div className="flex items-center pt-6">
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={pinned} onChange={(e) => setPinned(e.target.checked)} />
              Pin to top
            </label>
          </div>
        </div>
        {error && <p className="text-xs text-red-600">{error}</p>}
        <div className="flex justify-end">
          <Button size="sm" onClick={submit} disabled={submitting}>Publish</Button>
        </div>
      </div>
    </div>
  );
}
