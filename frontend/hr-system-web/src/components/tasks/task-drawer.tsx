"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { TaskComments } from "./task-comments";
import { apiTasks } from "@/lib/api/tasks";
import type { WorkTaskDto, WorkTaskStatus, TaskPriority } from "@/types";

interface Props {
  taskId: number | null;
  onClose: () => void;
  onStatusChange: (id: number, status: WorkTaskStatus) => void;
  onEdit?: (task: WorkTaskDto) => void;
}

const STATUS_LABELS: Record<WorkTaskStatus, string> = {
  Open:       "Open",
  InProgress: "In Progress",
  OnHold:     "On Hold",
  Done:       "Done",
};

const STATUS_CHIP: Record<WorkTaskStatus, string> = {
  Open:       "bg-slate-200 text-slate-800",
  InProgress: "bg-blue-100 text-blue-800",
  OnHold:     "bg-amber-100 text-amber-800",
  Done:       "bg-green-100 text-green-800",
};

const PRIORITY_CHIP: Record<TaskPriority, string> = {
  Low:    "bg-blue-100 text-blue-800",
  Medium: "bg-amber-100 text-amber-800",
  High:   "bg-red-100 text-red-800",
  Urgent: "bg-red-200 text-red-900",
};

export function TaskDrawer({ taskId, onClose, onStatusChange, onEdit }: Props) {
  const [task, setTask] = useState<WorkTaskDto | null>(null);

  useEffect(() => {
    let cancelled = false;
    const fetchTask = taskId == null
      ? Promise.resolve(null)
      : apiTasks.get(taskId);
    fetchTask.then((t) => { if (!cancelled) setTask(t); });
    return () => { cancelled = true; };
  }, [taskId]);

  useEffect(() => {
    function handleKey(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    if (taskId != null) window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [taskId, onClose]);

  if (taskId == null) return null;

  return (
    <>
      <div
        className="fixed inset-0 z-40 bg-black/30"
        onClick={onClose}
        aria-label="Close drawer"
      />
      <aside className="fixed inset-y-0 right-0 z-50 flex w-full max-w-md flex-col gap-4 overflow-y-auto border-l border-border bg-card p-5 shadow-2xl">
        {!task && <div className="text-sm text-muted-foreground">Loading…</div>}
        {task && (
          <>
            <div className="flex items-start justify-between gap-3">
              <h2 className="text-lg font-bold leading-snug">{task.title}</h2>
              <button
                type="button"
                onClick={onClose}
                className="text-muted-foreground hover:text-foreground"
                aria-label="Close"
              >
                ✕
              </button>
            </div>

            <div className="flex flex-wrap items-center gap-2">
              <span className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${PRIORITY_CHIP[task.priority]}`}>
                {task.priority.toUpperCase()}
              </span>
              <select
                value={task.status}
                onChange={(e) => {
                  const s = e.target.value as WorkTaskStatus;
                  setTask({ ...task, status: s });
                  onStatusChange(task.id, s);
                }}
                className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${STATUS_CHIP[task.status]}`}
              >
                {(Object.keys(STATUS_LABELS) as WorkTaskStatus[]).map((s) => (
                  <option key={s} value={s}>{STATUS_LABELS[s]}</option>
                ))}
              </select>
              {onEdit && (
                <Button variant="outline" size="sm" className="ml-auto" onClick={() => onEdit(task)}>
                  Edit
                </Button>
              )}
            </div>

            {task.description && (
              <div className="whitespace-pre-wrap text-sm text-foreground/90">
                {task.description}
              </div>
            )}

            <dl className="grid grid-cols-2 gap-3 rounded-xl bg-muted/40 p-3 text-xs">
              <div><dt className="text-muted-foreground">Assignee</dt><dd className="font-medium">{task.assignedToName}</dd></div>
              <div><dt className="text-muted-foreground">Reporter</dt><dd className="font-medium">{task.assignedByName}</dd></div>
              <div><dt className="text-muted-foreground">Due</dt><dd className="font-medium">{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : "—"}</dd></div>
              <div><dt className="text-muted-foreground">Category</dt><dd className="font-medium">{task.category ?? "—"}</dd></div>
              <div className="col-span-2"><dt className="text-muted-foreground">Tags</dt><dd className="font-medium">{task.tags ?? "—"}</dd></div>
            </dl>

            <div className="border-t border-border pt-3">
              <TaskComments taskId={task.id} />
            </div>
          </>
        )}
      </aside>
    </>
  );
}
