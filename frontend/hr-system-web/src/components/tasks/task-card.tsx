import type { WorkTaskDto, TaskPriority } from "@/types";

interface Props {
  task: WorkTaskDto;
  onClick?: () => void;
}

const PRIORITY_STYLES: Record<TaskPriority, string> = {
  Low:    "bg-blue-100 text-blue-800",
  Medium: "bg-amber-100 text-amber-800",
  High:   "bg-red-100 text-red-800",
  Urgent: "bg-red-200 text-red-900",
};

function initials(name: string): string {
  return name
    .split(" ")
    .map((p) => p[0])
    .slice(0, 2)
    .join("")
    .toUpperCase();
}

function formatDue(iso?: string): string {
  if (!iso) return "";
  const d = new Date(iso);
  return d.toLocaleDateString(undefined, { month: "short", day: "numeric" });
}

export function TaskCard({ task, onClick }: Props) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="w-full rounded-xl border border-border bg-card p-3 text-left shadow-[0_1px_2px_rgba(15,23,42,0.04)] transition hover:border-primary/40"
    >
      <div className="text-sm font-semibold text-foreground line-clamp-2">
        {task.title}
      </div>
      <div className="mt-2 flex items-center gap-2">
        <span
          className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${PRIORITY_STYLES[task.priority]}`}
        >
          {task.priority.toUpperCase()}
        </span>
        {task.dueDate && (
          <span className="text-[11px] text-muted-foreground">{formatDue(task.dueDate)}</span>
        )}
        <span className="ml-auto inline-flex h-5 w-5 items-center justify-center rounded-full bg-primary text-[9px] font-semibold text-primary-foreground">
          {initials(task.assignedToName)}
        </span>
      </div>
      {task.commentCount > 0 && (
        <div className="mt-1 text-[10px] text-muted-foreground">
          {task.commentCount} comment{task.commentCount === 1 ? "" : "s"}
        </div>
      )}
    </button>
  );
}
