import type {
  TimeLogDto,
  CreateTimeLogDto,
  UpdateTimeLogDto,
  DailySummaryDto,
  WeeklySummaryDto,
  ModificationRequestDto,
  CreateModificationRequestDto,
} from "@/types";
import { mockUsers, getCurrentMockUser } from "./users";

let nextLogId = 1;
let nextModId = 1;

function newGuid(): string {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

function isoDate(d: Date): string {
  return d.toISOString().slice(0, 10);
}

function hhmm(h: number, m: number): string {
  return `${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}`;
}

function minutesBetween(start: string, end: string): number {
  const [sh, sm] = start.split(":").map(Number);
  const [eh, em] = end.split(":").map(Number);
  return eh * 60 + em - (sh * 60 + sm);
}

function mondayOf(d: Date): Date {
  const out = new Date(d);
  const day = out.getDay();
  const diff = (day + 6) % 7;
  out.setDate(out.getDate() - diff);
  out.setHours(0, 0, 0, 0);
  return out;
}

const STANDARD_DAILY_HOURS = 8;
const STANDARD_WEEKLY_HOURS = 40;

const today = new Date();

function seedLogs(): TimeLogDto[] {
  const logs: TimeLogDto[] = [];
  const monday = mondayOf(today);
  // Seed Mon-Fri sessions for current user and one teammate
  for (let dayOffset = 0; dayOffset < 5; dayOffset++) {
    const d = new Date(monday);
    d.setDate(monday.getDate() + dayOffset);
    if (d > today) break;
    // current user 9-13 + 14-17
    logs.push({
      id: nextLogId++,
      publicId: newGuid(),
      employeeId: 1,
      date: isoDate(d),
      startTime: "09:00",
      endTime: "13:00",
      durationMinutes: 240,
      createdAt: d.toISOString(),
    });
    logs.push({
      id: nextLogId++,
      publicId: newGuid(),
      employeeId: 1,
      date: isoDate(d),
      startTime: "14:00",
      endTime: "17:30",
      durationMinutes: 210,
      createdAt: d.toISOString(),
    });
    // teammate (id 5) 10-18
    logs.push({
      id: nextLogId++,
      publicId: newGuid(),
      employeeId: 5,
      date: isoDate(d),
      startTime: "10:00",
      endTime: "18:00",
      durationMinutes: 480,
      createdAt: d.toISOString(),
    });
  }
  return logs;
}

let logs: TimeLogDto[] = seedLogs();
let modifications: ModificationRequestDto[] = [];

function summarizeDay(date: string, employeeId: number): DailySummaryDto {
  const sessions = logs.filter((l) => l.employeeId === employeeId && l.date === date);
  const totalMinutes = sessions.reduce((sum, s) => sum + s.durationMinutes, 0);
  const totalHours = totalMinutes / 60;
  const active = sessions.find((s) => !s.endTime);
  const employee = mockUsers.find((u) => u.id === employeeId);
  return {
    date,
    employeeId,
    employeeName: employee?.name,
    sessions,
    totalMinutes,
    totalHours,
    sessionCount: sessions.length,
    standardHours: STANDARD_DAILY_HOURS,
    isOvertime: totalHours > STANDARD_DAILY_HOURS,
    activeSessionStartTime: active?.startTime,
  };
}

function summarizeWeek(weekStart: string, employeeId: number): WeeklySummaryDto {
  const start = new Date(weekStart);
  const days: DailySummaryDto[] = [];
  for (let i = 0; i < 7; i++) {
    const d = new Date(start);
    d.setDate(start.getDate() + i);
    days.push(summarizeDay(isoDate(d), employeeId));
  }
  const totalMinutes = days.reduce((s, d) => s + d.totalMinutes, 0);
  return {
    weekStart,
    days,
    totalMinutes,
    totalHours: totalMinutes / 60,
    standardWeeklyHours: STANDARD_WEEKLY_HOURS,
  };
}

export const mockTimeTracking = {
  async listMine(date?: string): Promise<TimeLogDto[]> {
    const me = getCurrentMockUser();
    return logs.filter((l) => l.employeeId === me.id && (!date || l.date === date));
  },
  async listTeam(date: string): Promise<TimeLogDto[]> {
    return logs.filter((l) => l.date === date);
  },
  async openSession(): Promise<TimeLogDto | null> {
    const me = getCurrentMockUser();
    return logs.find((l) => l.employeeId === me.id && !l.endTime) ?? null;
  },
  async clockIn(): Promise<TimeLogDto> {
    const me = getCurrentMockUser();
    const open = logs.find((l) => l.employeeId === me.id && !l.endTime);
    if (open) throw new Error("Already clocked in");
    const now = new Date();
    const log: TimeLogDto = {
      id: nextLogId++,
      publicId: newGuid(),
      employeeId: me.id,
      date: isoDate(now),
      startTime: hhmm(now.getHours(), now.getMinutes()),
      durationMinutes: 0,
      createdAt: now.toISOString(),
    };
    logs = [log, ...logs];
    return log;
  },
  async clockOut(): Promise<TimeLogDto | null> {
    const me = getCurrentMockUser();
    const open = logs.find((l) => l.employeeId === me.id && !l.endTime);
    if (!open) return null;
    const now = new Date();
    open.endTime = hhmm(now.getHours(), now.getMinutes());
    open.durationMinutes = minutesBetween(open.startTime, open.endTime);
    return open;
  },
  async create(dto: CreateTimeLogDto): Promise<TimeLogDto> {
    const me = getCurrentMockUser();
    const id = nextLogId++;
    const log: TimeLogDto = {
      id,
      publicId: newGuid(),
      employeeId: me.id,
      date: dto.date,
      startTime: dto.startTime,
      endTime: dto.endTime,
      durationMinutes: minutesBetween(dto.startTime, dto.endTime),
      notes: dto.notes,
      createdAt: new Date().toISOString(),
    };
    logs = [log, ...logs];
    return log;
  },
  async update(id: number, dto: UpdateTimeLogDto): Promise<TimeLogDto | null> {
    const l = logs.find((x) => x.id === id);
    if (!l) return null;
    if (dto.startTime) l.startTime = dto.startTime;
    if (dto.endTime !== undefined) l.endTime = dto.endTime;
    if (dto.notes !== undefined) l.notes = dto.notes;
    if (l.endTime) l.durationMinutes = minutesBetween(l.startTime, l.endTime);
    return l;
  },
  async dailySummary(date: string): Promise<DailySummaryDto> {
    const me = getCurrentMockUser();
    return summarizeDay(date, me.id);
  },
  async weeklySummary(weekStart?: string): Promise<WeeklySummaryDto> {
    const me = getCurrentMockUser();
    const start = weekStart ?? isoDate(mondayOf(new Date()));
    return summarizeWeek(start, me.id);
  },
  async listModifications(): Promise<ModificationRequestDto[]> {
    const me = getCurrentMockUser();
    return modifications.filter((m) => m.employeeId === me.id);
  },
  async createModification(dto: CreateModificationRequestDto): Promise<ModificationRequestDto> {
    const me = getCurrentMockUser();
    const req: ModificationRequestDto = {
      id: nextModId++,
      publicId: newGuid(),
      employeeId: me.id,
      employeeName: me.name,
      timeLogId: dto.timeLogId,
      requestedStartTime: dto.requestedStartTime,
      requestedEndTime: dto.requestedEndTime,
      reason: dto.reason,
      status: "Pending",
      createdAt: new Date().toISOString(),
    };
    modifications = [req, ...modifications];
    return req;
  },
};

// Helpers exposed for chart components
export const timeTrackingHelpers = { mondayOf, isoDate };
