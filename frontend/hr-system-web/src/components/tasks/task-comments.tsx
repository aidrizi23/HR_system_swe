"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { apiTasks } from "@/lib/api/tasks";
import type { TaskCommentDto } from "@/types";

interface Props {
  taskId: number;
}

function initials(name: string): string {
  return name.split(" ").map((p) => p[0]).slice(0, 2).join("").toUpperCase();
}

function formatWhen(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" });
}

export function TaskComments({ taskId }: Props) {
  const [comments, setComments] = useState<TaskCommentDto[]>([]);
  const [draft, setDraft] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    apiTasks.comments(taskId).then(setComments);
  }, [taskId]);

  async function submit() {
    const trimmed = draft.trim();
    if (!trimmed || trimmed.length > 2000) return;
    setSubmitting(true);
    const created = await apiTasks.addComment(taskId, trimmed);
    if (created) {
      setComments((prev) => [...prev, created]);
      setDraft("");
    }
    setSubmitting(false);
  }

  return (
    <div className="space-y-3">
      <div className="text-[11px] font-bold tracking-wider text-muted-foreground">
        COMMENTS · {comments.length}
      </div>
      <div className="space-y-2">
        {comments.length === 0 && (
          <div className="text-xs text-muted-foreground">No comments yet.</div>
        )}
        {comments.map((c) => (
          <div key={c.id} className="flex gap-2">
            <div className="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full bg-primary text-[10px] font-semibold text-primary-foreground">
              {initials(c.authorName)}
            </div>
            <div className="flex-1 rounded-lg bg-muted/50 px-3 py-2">
              <div className="text-xs">
                <span className="font-semibold">{c.authorName}</span>
                <span className="ml-2 text-muted-foreground">{formatWhen(c.createdAt)}</span>
              </div>
              <div className="mt-1 text-xs whitespace-pre-wrap">{c.content}</div>
            </div>
          </div>
        ))}
      </div>
      <div className="flex gap-2">
        <Input
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          placeholder="Write a comment…"
          onKeyDown={(e) => {
            if (e.key === "Enter" && (e.metaKey || e.ctrlKey)) submit();
          }}
        />
        <Button size="sm" onClick={submit} disabled={submitting || !draft.trim()}>
          Send
        </Button>
      </div>
    </div>
  );
}
