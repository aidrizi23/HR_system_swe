"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/shared/page-header";
import { Button } from "@/components/ui/button";
import { DocumentList } from "@/components/documents/document-list";
import { UploadModal } from "@/components/documents/upload-modal";
import { UploadDragOverlay } from "@/components/documents/upload-drag-overlay";
import { ExpiringDocs } from "@/components/documents/expiring-docs";
import { DocumentCategories } from "@/components/documents/document-categories";
import { apiDocuments } from "@/lib/api/documents";
import { getCurrentMockUser, isHrOrAbove, mockUsers } from "@/lib/mock/users";
import type { EmployeeDocumentDto } from "@/types";

const _me = getCurrentMockUser();

const TABS = [
  { key: "mine",       label: "My Documents", hrOnly: false },
  { key: "employee",   label: "Employee Docs", hrOnly: true },
  { key: "expiring",   label: "Expiring",     hrOnly: true },
  { key: "categories", label: "Categories",   hrOnly: true },
] as const;

type TabKey = (typeof TABS)[number]["key"];

export default function DocumentsPage() {
  const me = _me;
  const isHr = isHrOrAbove(me.role);
  const visibleTabs = useMemo(() => TABS.filter((t) => !t.hrOnly || isHr), [isHr]);
  const [active, setActive] = useState<TabKey>(visibleTabs[0].key);
  const [uploadOpen, setUploadOpen] = useState(false);
  const [droppedFile, setDroppedFile] = useState<File | null>(null);

  const [myDocs, setMyDocs] = useState<EmployeeDocumentDto[]>([]);
  const [employeeDocs, setEmployeeDocs] = useState<EmployeeDocumentDto[]>([]);
  const [selectedEmployeeId, setSelectedEmployeeId] = useState<number>(me.employeeId);

  async function refreshMine() {
    const docs = await apiDocuments.listByEmployee(me.employeeId);
    setMyDocs(docs);
  }
  async function refreshEmployee(id: number) {
    const docs = await apiDocuments.listByEmployee(id);
    setEmployeeDocs(docs);
  }

  useEffect(() => {
    apiDocuments.listByEmployee(_me.employeeId).then(setMyDocs);
  }, []);
  useEffect(() => {
    if (isHr) apiDocuments.listByEmployee(selectedEmployeeId).then(setEmployeeDocs);
  }, [isHr, selectedEmployeeId]);

  function openUploadFromDrop(file: File) {
    setDroppedFile(file);
    setUploadOpen(true);
  }

  function openUploadButton() {
    setDroppedFile(null);
    setUploadOpen(true);
  }

  async function handleDelete(id: number) {
    await apiDocuments.remove(id);
    refreshMine();
    if (isHr) refreshEmployee(selectedEmployeeId);
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Employee Documents"
        subtitle="Manage employee documents, track expirations, and organize categories."
        actions={<Button size="sm" onClick={openUploadButton}>↑ Upload Document</Button>}
      />

      <div className="flex gap-6 border-b border-border">
        {visibleTabs.map((t) => (
          <button
            key={t.key}
            onClick={() => setActive(t.key)}
            className={`-mb-px border-b-2 py-2 text-sm transition ${
              active === t.key
                ? "border-primary font-semibold text-primary"
                : "border-transparent text-muted-foreground hover:text-foreground"
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {active === "mine" && (
        <DocumentList documents={myDocs} canDelete={isHr} onDelete={handleDelete} />
      )}

      {active === "employee" && isHr && (
        <div className="space-y-4">
          <div className="rounded-2xl border border-border bg-card p-4">
            <label className="text-xs font-semibold text-muted-foreground">Employee</label>
            <select
              value={selectedEmployeeId}
              onChange={(e) => setSelectedEmployeeId(Number(e.target.value))}
              className="mt-1 w-full rounded-lg border border-border bg-background p-2 text-sm"
            >
              {mockUsers.map((u) => <option key={u.id} value={u.employeeId}>{u.name}</option>)}
            </select>
          </div>
          <DocumentList documents={employeeDocs} canDelete={isHr} onDelete={handleDelete} />
        </div>
      )}

      {active === "expiring" && isHr && <ExpiringDocs />}
      {active === "categories" && isHr && <DocumentCategories />}

      <UploadModal
        open={uploadOpen}
        initialFile={droppedFile}
        onClose={() => { setUploadOpen(false); setDroppedFile(null); }}
        onUploaded={() => { refreshMine(); if (isHr) refreshEmployee(selectedEmployeeId); }}
      />

      <UploadDragOverlay onFileDrop={openUploadFromDrop} />
    </div>
  );
}
