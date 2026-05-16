"use client";

import { useEffect, useState } from "react";
import { DndContext, DragEndEvent, PointerSensor, useSensor, useSensors } from "@dnd-kit/core";
import { SortableContext, arrayMove, verticalListSortingStrategy } from "@dnd-kit/sortable";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { TemplateItemRow } from "./template-item-row";
import { TemplateItemEdit } from "./template-item-edit";
import { apiOnboarding } from "@/lib/api/onboarding";
import type { OnboardingTemplateDto } from "@/types";

export function TemplateEditor() {
  const [templates, setTemplates] = useState<OnboardingTemplateDto[]>([]);
  const [expandedId, setExpandedId] = useState<number | null>(null);
  const [editingItem, setEditingItem] = useState<{ templateId: number; itemId: number | "new" } | null>(null);
  const [renaming, setRenaming] = useState<{ id: number; value: string } | null>(null);
  const [error, setError] = useState<string | null>(null);
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));

  function refresh() {
    apiOnboarding.listTemplates().then(setTemplates);
  }
  useEffect(() => { refresh(); }, []);

  async function createTemplate() {
    const t = await apiOnboarding.createTemplate({
      name: `New Template ${templates.length + 1}`,
      items: [{ description: "First item", responsibleRole: "Employee", defaultDueDays: 1 }],
    });
    setExpandedId(t.id);
    refresh();
  }

  async function rename(id: number, name: string) {
    if (!name.trim()) { setRenaming(null); return; }
    await apiOnboarding.updateTemplate(id, { name: name.trim() });
    setRenaming(null);
    refresh();
  }

  async function deleteTemplate(id: number) {
    setError(null);
    try {
      await apiOnboarding.deleteTemplate(id);
      refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Delete failed");
    }
  }

  async function deleteItem(templateId: number, itemId: number) {
    await apiOnboarding.deleteItem(templateId, itemId);
    refresh();
  }

  async function dragEnd(e: DragEndEvent, template: OnboardingTemplateDto) {
    if (!e.over || e.active.id === e.over.id) return;
    const oldIndex = template.items.findIndex((i) => i.id === e.active.id);
    const newIndex = template.items.findIndex((i) => i.id === e.over!.id);
    const prevItems = template.items;
    const reordered = arrayMove(template.items, oldIndex, newIndex);
    // Optimistic local update so an in-flight "+ Add item" form isn't unmounted by a full refresh
    setTemplates((prev) => prev.map((t) => (t.id === template.id ? { ...t, items: reordered } : t)));
    try {
      await apiOnboarding.reorderItems(template.id, reordered.map((i) => i.id));
    } catch (err) {
      // Roll back to the pre-drag order if the server rejected the new order.
      setTemplates((prev) => prev.map((t) => (t.id === template.id ? { ...t, items: prevItems } : t)));
      setError(err instanceof Error ? err.message : "Reorder failed");
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div className="text-sm font-semibold">Templates</div>
        <Button size="sm" onClick={createTemplate}>+ New Template</Button>
      </div>
      {error && <p className="text-xs text-red-600">{error}</p>}

      {templates.map((t) => {
        const expanded = expandedId === t.id;
        return (
          <div key={t.id} className="rounded-2xl border border-border bg-card">
            <div className="flex items-center justify-between gap-3 p-4">
              <button
                type="button"
                onClick={() => setExpandedId(expanded ? null : t.id)}
                className="flex-1 text-left"
              >
                {renaming?.id === t.id ? (
                  <Input
                    autoFocus
                    value={renaming.value}
                    onChange={(e) => setRenaming({ id: t.id, value: e.target.value })}
                    onKeyDown={(e) => e.key === "Enter" && rename(t.id, renaming.value)}
                    onClick={(e) => e.stopPropagation()}
                  />
                ) : (
                  <div className="font-semibold">{t.name}</div>
                )}
                <div className="text-xs text-muted-foreground">{t.items.length} items</div>
              </button>
              <Button variant="outline" size="sm" onClick={() => setRenaming({ id: t.id, value: t.name })}>Rename</Button>
              <Button variant="outline" size="sm" className="text-red-600" onClick={() => deleteTemplate(t.id)}>×</Button>
            </div>

            {expanded && (
              <div className="space-y-2 border-t border-border p-4">
                <DndContext sensors={sensors} onDragEnd={(e) => dragEnd(e, t)}>
                  <SortableContext items={t.items.map((i) => i.id)} strategy={verticalListSortingStrategy}>
                    {t.items.map((item) =>
                      editingItem?.templateId === t.id && editingItem.itemId === item.id ? (
                        <TemplateItemEdit
                          key={`edit-${item.id}`}
                          initial={item}
                          onSave={async (vals) => {
                            await apiOnboarding.updateItem(t.id, item.id, vals);
                            setEditingItem(null);
                            refresh();
                          }}
                          onCancel={() => setEditingItem(null)}
                        />
                      ) : (
                        <TemplateItemRow
                          key={item.id}
                          item={item}
                          onEdit={() => setEditingItem({ templateId: t.id, itemId: item.id })}
                          onDelete={() => deleteItem(t.id, item.id)}
                        />
                      )
                    )}
                  </SortableContext>
                </DndContext>

                {editingItem?.templateId === t.id && editingItem.itemId === "new" ? (
                  <TemplateItemEdit
                    onSave={async (vals) => {
                      await apiOnboarding.addItem(t.id, vals);
                      setEditingItem(null);
                      refresh();
                    }}
                    onCancel={() => setEditingItem(null)}
                  />
                ) : (
                  <Button variant="outline" size="sm" onClick={() => setEditingItem({ templateId: t.id, itemId: "new" })}>
                    + Add item
                  </Button>
                )}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
