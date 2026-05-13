"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { apiDocumentCategories } from "@/lib/api/documents";
import type { DocumentCategoryDto } from "@/types";

export function DocumentCategories() {
  const [categories, setCategories] = useState<DocumentCategoryDto[]>([]);
  const [counts, setCounts] = useState<Record<number, number>>({});
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function refresh() {
    const cats = await apiDocumentCategories.list();
    setCategories(cats);
    const c: Record<number, number> = {};
    for (const cat of cats) c[cat.id] = await apiDocumentCategories.documentCount(cat.id);
    setCounts(c);
  }

  useEffect(() => {
    apiDocumentCategories.list().then(async (cats) => {
      setCategories(cats);
      const c: Record<number, number> = {};
      for (const cat of cats) c[cat.id] = await apiDocumentCategories.documentCount(cat.id);
      setCounts(c);
    });
  }, []);

  async function create() {
    setError(null);
    if (!name.trim()) { setError("Name is required"); return; }
    if (name.length > 200) { setError("Name max 200 chars"); return; }
    await apiDocumentCategories.create({ name, description: description || undefined });
    setName("");
    setDescription("");
    refresh();
  }

  async function remove(id: number) {
    setError(null);
    try {
      await apiDocumentCategories.remove(id);
      refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Delete failed");
    }
  }

  return (
    <div className="space-y-4">
      <div className="rounded-2xl border border-border bg-card p-4">
        <h3 className="text-sm font-semibold">New category</h3>
        <div className="mt-3 grid grid-cols-2 gap-3">
          <div>
            <Label>Name</Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} maxLength={200} />
          </div>
          <div>
            <Label>Description</Label>
            <Input value={description} onChange={(e) => setDescription(e.target.value)} maxLength={500} />
          </div>
        </div>
        {error && <p className="mt-2 text-xs text-red-600">{error}</p>}
        <div className="mt-3 flex justify-end">
          <Button size="sm" onClick={create}>Create</Button>
        </div>
      </div>

      <div className="overflow-hidden rounded-2xl border border-border bg-card">
        <table className="w-full text-sm">
          <thead className="bg-muted/40 text-[10px] uppercase tracking-wider text-muted-foreground">
            <tr>
              <th className="px-4 py-3 text-left">Name</th>
              <th className="px-4 py-3 text-left">Description</th>
              <th className="px-4 py-3 text-left">Documents</th>
              <th className="px-4 py-3 text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            {categories.map((c) => (
              <tr key={c.id} className="border-t border-border hover:bg-muted/30">
                <td className="px-4 py-3 font-medium">{c.name}</td>
                <td className="px-4 py-3 text-muted-foreground">{c.description ?? "—"}</td>
                <td className="px-4 py-3">{counts[c.id] ?? 0}</td>
                <td className="px-4 py-3 text-right">
                  <Button
                    variant="outline"
                    size="sm"
                    className="text-red-600"
                    onClick={() => remove(c.id)}
                    disabled={(counts[c.id] ?? 0) > 0}
                  >
                    Delete
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
