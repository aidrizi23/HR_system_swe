// src/lib/mock/overtime.ts
import type {
  OvertimeRecordDto,
  CreateOvertimeRequestDto,
  OvertimeFilterDto,
  OvertimeStatus,
} from "@/types";
import { mockUsers, getCurrentMockUser } from "./users";

let nextId = 1;

function newGuid(): string {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

const daysFromNow = (n: number) => new Date(Date.now() + n * 86400000).toISOString().slice(0, 10);

function make(employeeId: number, date: string, minutes: number, status: OvertimeStatus, reason?: string): OvertimeRecordDto {
  const id = nextId++;
  const employee = mockUsers.find((u) => u.id === employeeId);
  return {
    id,
    publicId: newGuid(),
    employeeId,
    employeeName: employee?.name,
    date,
    overtimeMinutes: minutes,
    overtimeHours: Math.round((minutes / 60) * 10) / 10,
    type: "Manual",
    reason,
    status,
    createdAt: new Date().toISOString(),
  };
}

let records: OvertimeRecordDto[] = [
  make(1, daysFromNow(-3), 120, "Approved", "End-of-month payroll close"),
  make(5, daysFromNow(-2), 90,  "Pending",  "Vendor escalation"),
  make(1, daysFromNow(-7), 60,  "Rejected", "Personal task"),
];

function matchesFilter(r: OvertimeRecordDto, f: OvertimeFilterDto): boolean {
  if (f.employeeId != null && r.employeeId !== f.employeeId) return false;
  if (f.status != null && r.status !== f.status) return false;
  if (f.dateFrom && r.date < f.dateFrom) return false;
  if (f.dateTo && r.date > f.dateTo) return false;
  return true;
}

export const mockOvertime = {
  async listMine(): Promise<OvertimeRecordDto[]> {
    const me = getCurrentMockUser();
    return records.filter((r) => r.employeeId === me.id);
  },
  async listAll(filter: OvertimeFilterDto = {}): Promise<OvertimeRecordDto[]> {
    return records.filter((r) => matchesFilter(r, filter));
  },
  async listPending(): Promise<OvertimeRecordDto[]> {
    return records.filter((r) => r.status === "Pending");
  },
  async create(dto: CreateOvertimeRequestDto): Promise<OvertimeRecordDto> {
    const me = getCurrentMockUser();
    const rec = make(me.id, dto.date, dto.overtimeMinutes, "Pending", dto.reason);
    records = [rec, ...records];
    return rec;
  },
  async process(id: number, dto: { approve: boolean; comments?: string }): Promise<OvertimeRecordDto | null> {
    const r = records.find((x) => x.id === id);
    if (!r) return null;
    r.status = dto.approve ? "Approved" : "Rejected";
    r.approverComments = dto.comments;
    r.processedAt = new Date().toISOString();
    return r;
  },
};
