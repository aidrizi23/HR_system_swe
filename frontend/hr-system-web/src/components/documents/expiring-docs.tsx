"use client";

import { useEffect, useState } from "react";
import { apiDocuments } from "@/lib/api/documents";
import type { EmployeeDocumentDto } from "@/types";

function daysUntil(iso: string): number {
  return Math.ceil((new Date(iso).getTime() - Date.now()) / 86400000);
}

export function ExpiringDocs() {
  const [docs, setDocs] = useState<EmployeeDocumentDto[]>([]);

  useEffect(() => {
    apiDocuments.expiring(30).then(setDocs);
  }, []);

  if (docs.length === 0) {
    return (
      <div className="rounded-2xl border border-border bg-card p-12 text-center text-sm text-muted-foreground">
        No documents expiring in the next 30 days
      </div>
    );
  }

  return (
    <div className="overflow-hidden rounded-2xl border border-border bg-card">
      <table className="w-full text-sm">
        <thead className="bg-muted/40 text-[10px] uppercase tracking-wider text-muted-foreground">
          <tr>
            <th className="px-4 py-3 text-left">Employee</th>
            <th className="px-4 py-3 text-left">File</th>
            <th className="px-4 py-3 text-left">Category</th>
            <th className="px-4 py-3 text-left">Expires</th>
            <th className="px-4 py-3 text-left">Status</th>
          </tr>
        </thead>
        <tbody>
          {docs.map((d) => {
            const days = d.expiryDate ? daysUntil(d.expiryDate) : null;
            const tone =
              days == null ? "" :
              days < 0     ? "bg-red-100 text-red-800" :
              days <= 7    ? "bg-amber-100 text-amber-800" :
                             "bg-blue-100 text-blue-800";
            const label =
              days == null ? "—" :
              days < 0     ? `Expired ${-days}d ago` :
              days === 0   ? "Expires today" :
                             `${days}d`;
            return (
              <tr key={d.id} className="border-t border-border hover:bg-muted/30">
                <td className="px-4 py-3 font-medium">{d.employeeName}</td>
                <td className="px-4 py-3">{d.fileName}</td>
                <td className="px-4 py-3">{d.categoryName}</td>
                <td className="px-4 py-3 text-muted-foreground">
                  {d.expiryDate ? new Date(d.expiryDate).toLocaleDateString() : "—"}
                </td>
                <td className="px-4 py-3">
                  <span className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${tone}`}>
                    {label}
                  </span>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
