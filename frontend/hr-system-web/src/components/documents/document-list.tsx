"use client";

import type { EmployeeDocumentDto } from "@/types";
import { Button } from "@/components/ui/button";

interface Props {
  documents: EmployeeDocumentDto[];
  canDelete: boolean;
  onDelete?: (id: number) => void;
}

function humanSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

function fileBadge(ct: string): string {
  if (ct.includes("pdf")) return "PDF";
  if (ct.startsWith("image/")) return "IMG";
  if (ct.includes("word")) return "DOC";
  return "FILE";
}

export function DocumentList({ documents, canDelete, onDelete }: Props) {
  if (documents.length === 0) {
    return (
      <div className="rounded-2xl border border-border bg-card p-12 text-center text-sm text-muted-foreground">
        No documents found
      </div>
    );
  }

  return (
    <div className="overflow-hidden rounded-2xl border border-border bg-card">
      <table className="w-full text-sm">
        <thead className="bg-muted/40 text-[10px] uppercase tracking-wider text-muted-foreground">
          <tr>
            <th className="px-4 py-3 text-left">File</th>
            <th className="px-4 py-3 text-left">Category</th>
            <th className="px-4 py-3 text-left">Size</th>
            <th className="px-4 py-3 text-left">Uploaded</th>
            <th className="px-4 py-3 text-left">Expiry</th>
            <th className="px-4 py-3 text-right">Actions</th>
          </tr>
        </thead>
        <tbody>
          {documents.map((d) => (
            <tr key={d.id} className="border-t border-border hover:bg-muted/30">
              <td className="px-4 py-3">
                <div className="flex items-center gap-3">
                  <span className="rounded-md bg-primary/10 px-2 py-1 text-[10px] font-semibold text-primary">
                    {fileBadge(d.contentType)}
                  </span>
                  <span className="font-medium">{d.fileName}</span>
                </div>
              </td>
              <td className="px-4 py-3">{d.categoryName}</td>
              <td className="px-4 py-3">{humanSize(d.fileSize)}</td>
              <td className="px-4 py-3 text-muted-foreground">
                {new Date(d.createdAt).toLocaleDateString()}
              </td>
              <td className="px-4 py-3 text-muted-foreground">
                {d.expiryDate ? new Date(d.expiryDate).toLocaleDateString() : "—"}
              </td>
              <td className="px-4 py-3 text-right">
                <Button variant="outline" size="sm">Download</Button>
                {canDelete && (
                  <Button
                    variant="outline"
                    size="sm"
                    className="ml-2 text-red-600"
                    onClick={() => onDelete?.(d.id)}
                  >
                    Delete
                  </Button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
