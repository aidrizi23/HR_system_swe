import { api } from "./client";
import type {
  CreateOvertimeRequestDto,
  OvertimeFilterDto,
  OvertimeRecordDto,
} from "@/types";

export interface OvertimeDecisionDto {
  comments?: string;
}

export interface RejectOvertimeDto {
  reason: string;
}

export const apiOvertime = {
  listMine: async (): Promise<OvertimeRecordDto[]> => {
    const { data } = await api.get<OvertimeRecordDto[]>("/overtime/mine");
    return data;
  },

  listAll: async (filter?: OvertimeFilterDto): Promise<OvertimeRecordDto[]> => {
    const { data } = await api.get<OvertimeRecordDto[]>("/overtime", { params: filter });
    return data;
  },

  listPending: async (): Promise<OvertimeRecordDto[]> => {
    const { data } = await api.get<OvertimeRecordDto[]>("/overtime/pending");
    return data;
  },

  create: async (dto: CreateOvertimeRequestDto): Promise<OvertimeRecordDto> => {
    const { data } = await api.post<OvertimeRecordDto>("/overtime", dto);
    return data;
  },

  recommend: async (id: number, dto: OvertimeDecisionDto = {}): Promise<OvertimeRecordDto> => {
    const { data } = await api.post<OvertimeRecordDto>(`/overtime/${id}/recommend`, dto);
    return data;
  },

  approve: async (id: number, dto: OvertimeDecisionDto = {}): Promise<OvertimeRecordDto> => {
    const { data } = await api.post<OvertimeRecordDto>(`/overtime/${id}/approve`, dto);
    return data;
  },

  reject: async (id: number, dto: RejectOvertimeDto): Promise<OvertimeRecordDto> => {
    const { data } = await api.post<OvertimeRecordDto>(`/overtime/${id}/reject`, dto);
    return data;
  },
};
