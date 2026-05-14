import { api } from "./client";
import type {
  CreateModificationRequestDto,
  DailySummaryDto,
  ModificationRequestDto,
  TimeLogDto,
  WeeklySummaryDto,
} from "@/types";

function isoDate(d: Date): string {
  return d.toISOString().slice(0, 10);
}

function mondayOf(d: Date): Date {
  const out = new Date(d);
  const day = out.getDay();
  const diff = (day + 6) % 7;
  out.setDate(out.getDate() - diff);
  out.setHours(0, 0, 0, 0);
  return out;
}

function todayIso(): string {
  return isoDate(new Date());
}

export const apiTimeTracking = {
  listMine: async (date?: string): Promise<TimeLogDto[]> => {
    const { data } = await api.get<TimeLogDto[]>("/time-tracking/mine", {
      params: date ? { date } : undefined,
    });
    return data;
  },

  listTeam: async (date: string): Promise<TimeLogDto[]> => {
    const { data } = await api.get<TimeLogDto[]>("/time-tracking/team", { params: { date } });
    return data;
  },

  openSession: async (): Promise<TimeLogDto | null> => {
    const { data } = await api.get<TimeLogDto[]>("/time-tracking/mine", {
      params: { date: todayIso() },
    });
    return data.find((l) => !l.endTime) ?? null;
  },

  clockIn: async (): Promise<TimeLogDto> => {
    const { data } = await api.post<TimeLogDto>("/time-tracking/clock-in");
    return data;
  },

  clockOut: async (): Promise<TimeLogDto | null> => {
    const { data } = await api.post<TimeLogDto>("/time-tracking/clock-out");
    return data;
  },

  dailySummary: async (date: string): Promise<DailySummaryDto> => {
    const { data } = await api.get<DailySummaryDto>("/time-tracking/summary/daily/mine", {
      params: { date },
    });
    return data;
  },

  weeklySummary: async (weekStart?: string): Promise<WeeklySummaryDto> => {
    const monday = weekStart ?? isoDate(mondayOf(new Date()));
    const { data } = await api.get<WeeklySummaryDto>("/time-tracking/summary/weekly/mine", {
      params: { weekStart: monday },
    });
    return data;
  },

  listModifications: async (): Promise<ModificationRequestDto[]> => {
    const { data } = await api.get<ModificationRequestDto[]>("/time-tracking/modifications/mine");
    return data;
  },

  createModification: async (dto: CreateModificationRequestDto): Promise<ModificationRequestDto> => {
    const { data } = await api.post<ModificationRequestDto>("/time-tracking/modifications", dto);
    return data;
  },
};

export const timeHelpers = { isoDate, mondayOf };
