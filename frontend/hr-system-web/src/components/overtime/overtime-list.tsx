// src/components/overtime/overtime-list.tsx
"use client";

import { Button } from "@/components/ui/button";
import type { OvertimeRecordDto } from "@/types";

interface Props {
  records: OvertimeRecordDto[];
  showApprovalActions?: boolean;
  onRecommend?: (id: number) => void;
  onApprove?: (id: number) => void;
  onReject?: (id: number) => void;
  emptyMessage?: string;
}

const STATUS_TONE: Record<OvertimeRecordDto["status"], string> = {
  Pending:     "bg-amber-100 text-amber-800",
  Recommended: "bg-blue-100 text-blue-800",
  Approved:    "bg-green-100 text-green-800",
  Rejected:    "bg-red-100 text-red-800",
};

export function OvertimeList({ records, showApprovalActions, onRecommend, onApprove, onReject, emptyMessage = "No overtime records" }: Props) {
  if (records.length === 0) {
    return (
      <div className="rounded-2xl border border-border bg-card p-12 text-center text-sm text-muted-foreground">
        {emptyMessage}
      </div>
    );
  }
  return (
    <div className="overflow-hidden rounded-2xl border border-border bg-card">
      <table className="w-full text-sm">
        <thead className="bg-muted/40 text-[10px] uppercase tracking-wider text-muted-foreground">
          <tr>
            <th className="px-4 py-3 text-left">Employee</th>
            <th className="px-4 py-3 text-left">Date</th>
            <th className="px-4 py-3 text-left">Hours</th>
            <th className="px-4 py-3 text-left">Reason</th>
            <th className="px-4 py-3 text-left">Status</th>
            {showApprovalActions && <th className="px-4 py-3 text-right">Actions</th>}
          </tr>
        </thead>
        <tbody>
          {records.map((r) => (
            <tr key={r.id} className="border-t border-border">
              <td className="px-4 py-3 font-medium">{r.employeeName ?? `#${r.employeeId}`}</td>
              <td className="px-4 py-3 text-muted-foreground">{new Date(r.date).toLocaleDateString()}</td>
              <td className="px-4 py-3">{r.overtimeHours.toFixed(1)}h</td>
              <td className="px-4 py-3 text-muted-foreground">{r.reason ?? "—"}</td>
              <td className="px-4 py-3">
                <span className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${STATUS_TONE[r.status]}`}>
                  {r.status}
                </span>
              </td>
              {showApprovalActions && (
                <td className="px-4 py-3 text-right">
                  {r.status === "Pending" && onRecommend && (
                    <Button variant="outline" size="sm" onClick={() => onRecommend(r.id)}>Recommend</Button>
                  )}
                  {r.status === "Recommended" && onApprove && (
                    <Button variant="outline" size="sm" onClick={() => onApprove(r.id)}>Approve</Button>
                  )}
                  {(r.status === "Pending" || r.status === "Recommended") && onReject && (
                    <Button variant="outline" size="sm" className="ml-2 text-red-600" onClick={() => onReject(r.id)}>Reject</Button>
                  )}
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
