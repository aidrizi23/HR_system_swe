"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { apiTasks } from "@/lib/api/tasks";
import { mockUsers } from "@/lib/mock/users";
import type { TaskPriority, WorkTaskDto } from "@/types";

const PRIORITIES: TaskPriority[] = ["Low", "Medium", "High", "Urgent"];

const schema = z.object({
  title:         z.string().min(1, "Required").max(300),
  description:   z.string().max(5000).optional(),
  assignedToId:  z.coerce.number().int().positive("Required"),
  priority:      z.enum(["Low", "Medium", "High", "Urgent"]).default("Medium"),
  dueDate:       z.string().optional(),
  category:      z.string().max(100).optional(),
  tags:          z.string().max(500).optional(),
});

type InputValues = z.input<typeof schema>;
type FormValues = z.output<typeof schema>;

interface Props {
  open: boolean;
  onClose: () => void;
  onCreated: (task: WorkTaskDto) => void;
}

export function TaskCreateDialog({ open, onClose, onCreated }: Props) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<InputValues, unknown, FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { priority: "Medium" },
  });

  if (!open) return null;

  async function submit(values: FormValues) {
    const created = await apiTasks.create({
      title:        values.title,
      description:  values.description || undefined,
      assignedToId: values.assignedToId,
      priority:     values.priority,
      dueDate:      values.dueDate || undefined,
      category:     values.category || undefined,
      tags:         values.tags || undefined,
    });
    onCreated(created);
    reset();
    onClose();
  }

  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/30" onClick={onClose} />
      <div className="fixed left-1/2 top-1/2 z-50 w-full max-w-lg -translate-x-1/2 -translate-y-1/2 rounded-2xl border border-border bg-card p-5 shadow-2xl">
        <h3 className="text-lg font-bold">New Task</h3>
        <form onSubmit={handleSubmit(submit)} className="mt-4 space-y-3">
          <div>
            <Label>Title</Label>
            <Input {...register("title")} />
            {errors.title && <p className="mt-1 text-xs text-red-600">{errors.title.message}</p>}
          </div>
          <div>
            <Label>Description</Label>
            <textarea
              {...register("description")}
              rows={4}
              className="w-full rounded-lg border border-border bg-background p-2 text-sm"
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <Label>Assignee</Label>
              <select
                {...register("assignedToId")}
                className="w-full rounded-lg border border-border bg-background p-2 text-sm"
              >
                <option value="">— Select —</option>
                {mockUsers.map((u) => (
                  <option key={u.id} value={u.id}>{u.name}</option>
                ))}
              </select>
              {errors.assignedToId && <p className="mt-1 text-xs text-red-600">{errors.assignedToId.message}</p>}
            </div>
            <div>
              <Label>Priority</Label>
              <select
                {...register("priority")}
                className="w-full rounded-lg border border-border bg-background p-2 text-sm"
              >
                {PRIORITIES.map((p) => <option key={p} value={p}>{p}</option>)}
              </select>
            </div>
            <div>
              <Label>Due date</Label>
              <Input type="date" {...register("dueDate")} />
            </div>
            <div>
              <Label>Category</Label>
              <Input {...register("category")} placeholder="HR, Engineering, …" />
            </div>
          </div>
          <div>
            <Label>Tags</Label>
            <Input {...register("tags")} placeholder="comma,separated,tags" />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="outline" size="sm" onClick={onClose}>Cancel</Button>
            <Button type="submit" size="sm" disabled={isSubmitting}>Create</Button>
          </div>
        </form>
      </div>
    </>
  );
}
