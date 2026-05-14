// src/components/onboarding/template-item-row.tsx
"use client";

import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { Button } from "@/components/ui/button";
import type { OnboardingTemplateItemDto } from "@/types";

interface Props {
  item: OnboardingTemplateItemDto;
  onEdit: () => void;
  onDelete: () => void;
}

export function TemplateItemRow({ item, onEdit, onDelete }: Props) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: item.id });
  return (
    <div
      ref={setNodeRef}
      style={{ transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.5 : 1 }}
      className="flex items-center gap-3 rounded-lg border border-border bg-card p-2 text-sm"
    >
      <span {...attributes} {...listeners} className="cursor-grab text-muted-foreground select-none">⋮⋮</span>
      <span className="flex-1 truncate">{item.description}</span>
      <span className="rounded-full bg-muted px-2 py-0.5 text-[10px] font-semibold text-foreground/70">
        {item.responsibleRole}
      </span>
      <span className="text-[11px] text-muted-foreground">Day {item.defaultDueDays}</span>
      <Button variant="outline" size="sm" onClick={onEdit}>Edit</Button>
      <Button variant="outline" size="sm" className="text-red-600" onClick={onDelete}>×</Button>
    </div>
  );
}
