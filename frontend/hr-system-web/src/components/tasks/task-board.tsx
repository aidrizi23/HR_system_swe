"use client";

import { useMemo } from "react";
import {
  DndContext,
  DragEndEvent,
  PointerSensor,
  useDroppable,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import { useDraggable } from "@dnd-kit/core";
import { TaskCard } from "./task-card";
import type { WorkTaskDto, WorkTaskStatus } from "@/types";

const COLUMNS: Array<{ key: WorkTaskStatus; label: string }> = [
  { key: "Open",       label: "OPEN" },
  { key: "InProgress", label: "IN PROGRESS" },
  { key: "OnHold",     label: "ON HOLD" },
  { key: "Done",       label: "DONE" },
];

interface Props {
  tasks: WorkTaskDto[];
  onStatusChange: (id: number, status: WorkTaskStatus) => void;
  onCardClick: (id: number) => void;
}

function DraggableCard({
  task,
  onClick,
}: {
  task: WorkTaskDto;
  onClick: () => void;
}) {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: `task-${task.id}`,
  });

  return (
    <div
      ref={setNodeRef}
      style={{
        transform: transform ? `translate(${transform.x}px, ${transform.y}px)` : undefined,
        opacity: isDragging ? 0.4 : 1,
      }}
      {...attributes}
      {...listeners}
    >
      <TaskCard task={task} onClick={onClick} />
    </div>
  );
}

function Column({
  status,
  label,
  tasks,
  onCardClick,
}: {
  status: WorkTaskStatus;
  label: string;
  tasks: WorkTaskDto[];
  onCardClick: (id: number) => void;
}) {
  const { setNodeRef, isOver } = useDroppable({ id: `col-${status}` });

  return (
    <div
      ref={setNodeRef}
      className={`flex min-h-[300px] flex-col gap-2 rounded-2xl border p-3 transition ${
        isOver ? "border-primary/50 bg-primary/5" : "border-border bg-muted/40"
      }`}
    >
      <div className="flex items-center justify-between text-[11px] font-bold tracking-wider text-muted-foreground">
        <span>{label}</span>
        <span className="rounded-full bg-border px-2 py-0.5 text-[10px]">
          {tasks.length}
        </span>
      </div>
      {tasks.length === 0 && (
        <div className="py-6 text-center text-[11px] text-muted-foreground/70">
          No tasks
        </div>
      )}
      {tasks.map((t) => (
        <DraggableCard key={t.id} task={t} onClick={() => onCardClick(t.id)} />
      ))}
    </div>
  );
}

export function TaskBoard({ tasks, onStatusChange, onCardClick }: Props) {
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
  );

  const grouped = useMemo(() => {
    const map: Record<WorkTaskStatus, WorkTaskDto[]> = {
      Open: [], InProgress: [], OnHold: [], Done: [],
    };
    for (const t of tasks) map[t.status].push(t);
    return map;
  }, [tasks]);

  function handleDragEnd(e: DragEndEvent) {
    if (!e.over) return;
    const taskId = Number(String(e.active.id).replace(/^task-/, ""));
    const newStatus = String(e.over.id).replace(/^col-/, "") as WorkTaskStatus;
    onStatusChange(taskId, newStatus);
  }

  return (
    <DndContext sensors={sensors} onDragEnd={handleDragEnd}>
      <div className="grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-4">
        {COLUMNS.map((c) => (
          <Column
            key={c.key}
            status={c.key}
            label={c.label}
            tasks={grouped[c.key]}
            onCardClick={onCardClick}
          />
        ))}
      </div>
    </DndContext>
  );
}
