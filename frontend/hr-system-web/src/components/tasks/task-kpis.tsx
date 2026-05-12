import type { WorkTaskDto } from "@/types";

const NOW = Date.now();

interface Props {
  tasks: WorkTaskDto[];
}

export function TaskKpis({ tasks }: Props) {
  const total = tasks.length;
  const open = tasks.filter((t) => t.status === "Open").length;
  const inProgress = tasks.filter((t) => t.status === "InProgress").length;
  const overdue = tasks.filter(
    (t) =>
      t.dueDate &&
      new Date(t.dueDate).getTime() < NOW &&
      t.status !== "Done",
  ).length;

  const cards = [
    { label: "TOTAL", value: total },
    { label: "OPEN", value: open },
    { label: "IN PROGRESS", value: inProgress },
    { label: "OVERDUE", value: overdue },
  ];

  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
      {cards.map((c) => (
        <div
          key={c.label}
          className="rounded-2xl border border-border bg-card p-4 shadow-[0_1px_2px_rgba(15,23,42,0.04)]"
        >
          <div className="text-[10px] font-semibold tracking-wider text-muted-foreground">
            {c.label}
          </div>
          <div className="mt-1 text-2xl font-bold text-foreground">{c.value}</div>
        </div>
      ))}
    </div>
  );
}
