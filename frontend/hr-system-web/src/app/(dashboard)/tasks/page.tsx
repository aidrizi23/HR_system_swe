"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/shared/page-header";
import { Button } from "@/components/ui/button";
import { TaskKpis } from "@/components/tasks/task-kpis";
import { TaskBoard } from "@/components/tasks/task-board";
import { TaskDrawer } from "@/components/tasks/task-drawer";
import { TaskCreateDialog } from "@/components/tasks/task-create-dialog";
import { apiTasks } from "@/lib/api/tasks";
import type { WorkTaskDto, WorkTaskStatus } from "@/types";

type Scope = "mine" | "byMe" | "all";
type PriorityFilter = "" | "Low" | "Medium" | "High" | "Urgent";

export default function TasksPage() {
  const [tasks, setTasks] = useState<WorkTaskDto[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [createOpen, setCreateOpen] = useState(false);

  const [scope, setScope] = useState<Scope>("mine");
  const [search, setSearch] = useState("");
  const [priorityFilter, setPriorityFilter] = useState<PriorityFilter>("");

  useEffect(() => {
    apiTasks.list({ pageSize: 200 }).then((r) => setTasks(r.items));
  }, []);

  const me = 1; // mock current user id

  const visible = useMemo(() => {
    return tasks.filter((t) => {
      if (scope === "mine" && t.assignedToId !== me) return false;
      if (scope === "byMe" && t.assignedById !== me) return false;
      if (search && !t.title.toLowerCase().includes(search.toLowerCase())) return false;
      if (priorityFilter && t.priority !== priorityFilter) return false;
      return true;
    });
  }, [tasks, scope, search, priorityFilter]);

  async function handleStatusChange(id: number, status: WorkTaskStatus) {
    setTasks((prev) => prev.map((t) => (t.id === id ? { ...t, status } : t)));
    await apiTasks.updateStatus(id, status);
  }

  const tabs: Array<{ key: Scope; label: string }> = [
    { key: "mine", label: "My Tasks" },
    { key: "byMe", label: "Assigned by Me" },
    { key: "all",  label: "All Tasks" },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title="Tasks"
        subtitle="Track work assigned across your teams."
        actions={<Button size="sm" onClick={() => setCreateOpen(true)}>+ New Task</Button>}
      />
      <TaskKpis tasks={visible} />
      <div className="flex gap-6 border-b border-border">
        {tabs.map((t) => (
          <button
            key={t.key}
            onClick={() => setScope(t.key)}
            className={`-mb-px border-b-2 py-2 text-sm transition ${
              scope === t.key
                ? "border-primary font-semibold text-primary"
                : "border-transparent text-muted-foreground hover:text-foreground"
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>
      <div className="flex flex-wrap gap-2">
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search tasks…"
          className="flex-1 rounded-lg border border-border bg-card px-3 py-2 text-sm"
        />
        <select
          value={priorityFilter}
          onChange={(e) => setPriorityFilter(e.target.value as PriorityFilter)}
          className="rounded-lg border border-border bg-card px-3 py-2 text-sm"
        >
          <option value="">All Priorities</option>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
          <option value="Urgent">Urgent</option>
        </select>
      </div>
      <TaskBoard
        tasks={visible}
        onStatusChange={handleStatusChange}
        onCardClick={(id) => setSelectedId(id)}
      />
      <TaskDrawer
        taskId={selectedId}
        onClose={() => setSelectedId(null)}
        onStatusChange={handleStatusChange}
      />
      <TaskCreateDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={(t) => setTasks((prev) => [t, ...prev])}
      />
    </div>
  );
}
