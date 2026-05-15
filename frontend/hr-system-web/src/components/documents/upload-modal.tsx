"use client";

import { useEffect, useRef, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select } from "@/components/ui/select";
import { apiDocuments, apiDocumentCategories } from "@/lib/api/documents";
import { getCurrentMockUser } from "@/lib/mock/users";
import type { DocumentCategoryDto, EmployeeDocumentDto } from "@/types";

interface Props {
  open: boolean;
  initialFile?: File | null;
  onClose: () => void;
  onUploaded: (doc: EmployeeDocumentDto) => void;
}

export function UploadModal({ open, initialFile, onClose, onUploaded }: Props) {
  const [file, setFile] = useState<File | null>(null);
  const [categories, setCategories] = useState<DocumentCategoryDto[]>([]);
  const [categoryId, setCategoryId] = useState<number | "">("");
  const [expiry, setExpiry] = useState("");
  const [notes, setNotes] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!open) return;
    apiDocumentCategories.list().then((cats) => {
      setCategories(cats);
      setFile(initialFile ?? null);
      setError(null);
    });
  }, [open, initialFile]);

  if (!open) return null;

  async function submit() {
    setError(null);
    if (!file) { setError("Choose a file"); return; }
    if (!categoryId) { setError("Pick a category"); return; }
    setSubmitting(true);
    try {
      const me = getCurrentMockUser();
      const doc = await apiDocuments.upload(me.employeeId, file, {
        categoryId: Number(categoryId),
        expiryDate: expiry || undefined,
        notes: notes || undefined,
      });
      onUploaded(doc);
      setFile(null);
      setCategoryId("");
      setExpiry("");
      setNotes("");
      onClose();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Upload failed");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/30" onClick={onClose} />
      <div className="fixed left-1/2 top-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-2xl border border-border bg-card p-5 shadow-2xl">
        <h3 className="text-lg font-bold">Upload Document</h3>

        <button
          type="button"
          onClick={() => inputRef.current?.click()}
          onDragOver={(e) => e.preventDefault()}
          onDrop={(e) => {
            e.preventDefault();
            const f = e.dataTransfer.files?.[0];
            if (f) setFile(f);
          }}
          className="mt-4 flex w-full flex-col items-center gap-1 rounded-xl border-2 border-dashed border-primary/40 bg-primary/5 p-6 text-sm text-primary"
        >
          <div className="text-lg">↑</div>
          <div className="font-medium">{file ? file.name : "Drop a file here or click to browse"}</div>
          <div className="text-[11px] text-muted-foreground">PDF, DOCX, PNG, JPG · max 10MB</div>
          <input
            ref={inputRef}
            type="file"
            className="hidden"
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          />
        </button>

        <div className="mt-3 grid grid-cols-2 gap-3">
          <div>
            <Label>Category</Label>
            <Select
              value={categoryId === "" ? "" : String(categoryId)}
              onChange={(v) => setCategoryId(v ? Number(v) : "")}
              placeholder="— Select —"
              options={categories.map((c) => ({ value: String(c.id), label: c.name }))}
            />
          </div>
          <div>
            <Label>Expiry (optional)</Label>
            <Input type="date" value={expiry} onChange={(e) => setExpiry(e.target.value)} />
          </div>
        </div>

        <div className="mt-3">
          <Label>Notes</Label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={2}
            maxLength={1000}
            className="w-full rounded-lg border border-border bg-background p-2 text-sm"
          />
        </div>

        {error && <p className="mt-2 text-xs text-red-600">{error}</p>}

        <div className="mt-4 flex justify-end gap-2">
          <Button type="button" variant="outline" size="sm" onClick={onClose}>Cancel</Button>
          <Button type="button" size="sm" onClick={submit} disabled={submitting}>Upload</Button>
        </div>
      </div>
    </>
  );
}
