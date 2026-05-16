import { api } from "./client";
import type {
  AssignChecklistDto,
  CreateOnboardingTemplateDto,
  CreateOnboardingTemplateItemDto,
  OnboardingChecklistDto,
  OnboardingChecklistItemDto,
  OnboardingTemplateDto,
} from "@/types";

// The backend only supports full-template PUT (replace all items wholesale). The mock
// exposed item-level methods; we translate those by fetching the template, mutating
// the items array, and PUTting the entire body so the page-level UX is unchanged.
//
// Multiple rapid edits to the same template would otherwise race: both calls would
// fetch the same pre-edit body and the second PUT would silently overwrite the first.
// We chain calls per-template through a promise so reads always see the latest write.
const templateMutex = new Map<number, Promise<unknown>>();

async function withTemplateLock<T>(templateId: number, fn: () => Promise<T>): Promise<T> {
  const prev = templateMutex.get(templateId) ?? Promise.resolve();
  const next = prev.catch(() => undefined).then(fn);
  templateMutex.set(templateId, next);
  try {
    return await next;
  } finally {
    if (templateMutex.get(templateId) === next) templateMutex.delete(templateId);
  }
}

async function fetchTemplate(id: number): Promise<OnboardingTemplateDto> {
  const { data } = await api.get<OnboardingTemplateDto>(`/onboarding/templates/${id}`);
  return data;
}

function templateToCreateDto(t: OnboardingTemplateDto): CreateOnboardingTemplateDto {
  return {
    name: t.name,
    description: t.description,
    targetEmploymentType: t.targetEmploymentType,
    items: t.items.map((i) => ({
      description: i.description,
      responsibleRole: i.responsibleRole,
      defaultDueDays: i.defaultDueDays,
    })),
  };
}

async function putTemplate(id: number, body: CreateOnboardingTemplateDto): Promise<OnboardingTemplateDto> {
  const { data } = await api.put<OnboardingTemplateDto>(`/onboarding/templates/${id}`, body);
  return data;
}

export const apiOnboarding = {
  // templates
  listTemplates: async (): Promise<OnboardingTemplateDto[]> => {
    const { data } = await api.get<OnboardingTemplateDto[]>("/onboarding/templates");
    return data;
  },

  createTemplate: async (dto: CreateOnboardingTemplateDto): Promise<OnboardingTemplateDto> => {
    const { data } = await api.post<OnboardingTemplateDto>("/onboarding/templates", dto);
    return data;
  },

  updateTemplate: (
    id: number,
    dto: Partial<CreateOnboardingTemplateDto>,
  ): Promise<OnboardingTemplateDto> =>
    withTemplateLock(id, async () => {
      const current = await fetchTemplate(id);
      const merged: CreateOnboardingTemplateDto = {
        ...templateToCreateDto(current),
        ...(dto.name !== undefined ? { name: dto.name } : {}),
        ...(dto.description !== undefined ? { description: dto.description } : {}),
        ...(dto.targetEmploymentType !== undefined ? { targetEmploymentType: dto.targetEmploymentType } : {}),
        ...(dto.items !== undefined ? { items: dto.items } : {}),
      };
      return putTemplate(id, merged);
    }),

  deleteTemplate: async (id: number): Promise<void> => {
    await api.delete(`/onboarding/templates/${id}`);
  },

  // items — translated to wholesale PUT against the backend
  addItem: (templateId: number, dto: CreateOnboardingTemplateItemDto): Promise<OnboardingTemplateDto> =>
    withTemplateLock(templateId, async () => {
      const current = await fetchTemplate(templateId);
      const body = templateToCreateDto(current);
      body.items.push(dto);
      return putTemplate(templateId, body);
    }),

  updateItem: (
    templateId: number,
    itemId: number,
    dto: Partial<CreateOnboardingTemplateItemDto>,
  ): Promise<OnboardingTemplateDto> =>
    withTemplateLock(templateId, async () => {
      const current = await fetchTemplate(templateId);
      const body = templateToCreateDto(current);
      const idx = current.items.findIndex((i) => i.id === itemId);
      if (idx >= 0) {
        body.items[idx] = { ...body.items[idx], ...dto };
      }
      return putTemplate(templateId, body);
    }),

  deleteItem: (templateId: number, itemId: number): Promise<OnboardingTemplateDto> =>
    withTemplateLock(templateId, async () => {
      const current = await fetchTemplate(templateId);
      const body = templateToCreateDto(current);
      const idx = current.items.findIndex((i) => i.id === itemId);
      if (idx >= 0) body.items.splice(idx, 1);
      return putTemplate(templateId, body);
    }),

  reorderItems: (templateId: number, orderedItemIds: number[]): Promise<OnboardingTemplateDto> =>
    withTemplateLock(templateId, async () => {
      const current = await fetchTemplate(templateId);
      const byId = new Map(current.items.map((i) => [i.id, i]));
      const reordered = orderedItemIds
        .map((id) => byId.get(id))
        .filter((i): i is typeof current.items[number] => i !== undefined);
      const body: CreateOnboardingTemplateDto = {
        ...templateToCreateDto(current),
        items: reordered.map((i) => ({
          description: i.description,
          responsibleRole: i.responsibleRole,
          defaultDueDays: i.defaultDueDays,
        })),
      };
      return putTemplate(templateId, body);
    }),

  // checklists
  listChecklists: async (): Promise<OnboardingChecklistDto[]> => {
    const { data } = await api.get<OnboardingChecklistDto[]>("/onboarding/checklists");
    return data;
  },

  assign: async (dto: AssignChecklistDto): Promise<OnboardingChecklistDto> => {
    const { data } = await api.post<OnboardingChecklistDto>("/onboarding/assign", dto);
    return data;
  },

  // Backend route only takes the item id; checklistId is accepted to keep the existing
  // component signature stable but is not sent to the server.
  markItemComplete: async (_checklistId: number, itemId: number): Promise<OnboardingChecklistItemDto> => {
    const { data } = await api.post<OnboardingChecklistItemDto>(
      `/onboarding/checklists/items/${itemId}/complete`,
    );
    return data;
  },
};
