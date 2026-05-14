// src/lib/mock/onboarding.ts
import type {
  OnboardingTemplateDto,
  OnboardingTemplateItemDto,
  CreateOnboardingTemplateDto,
  CreateOnboardingTemplateItemDto,
  OnboardingChecklistDto,
  AssignChecklistDto,
  OnboardingResponsibleRole,
} from "@/types";
import { mockUsers } from "./users";

let nextTemplateId = 1;
let nextItemId = 1;
let nextChecklistId = 1;
let nextChecklistItemId = 1;

function newGuid(): string {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

function slugify(s: string): string {
  return s.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
}

function mkItem(description: string, role: OnboardingResponsibleRole, dueDays: number): OnboardingTemplateItemDto {
  return {
    id: nextItemId++,
    publicId: newGuid(),
    description,
    responsibleRole: role,
    defaultDueDays: dueDays,
  };
}

function seedTemplate(name: string, description: string, items: OnboardingTemplateItemDto[]): OnboardingTemplateDto {
  const id = nextTemplateId++;
  return {
    id,
    publicId: newGuid(),
    name,
    description,
    slug: slugify(name),
    items,
    createdAt: new Date().toISOString(),
  };
}

let templates: OnboardingTemplateDto[] = [
  seedTemplate("Standard Employee Onboarding", "Default checklist for new hires", [
    mkItem("Complete personal information form", "Employee", 1),
    mkItem("Sign employment contract",           "Employee", 1),
    mkItem("Set up work email and laptop",       "IT",       2),
    mkItem("Meet team lead",                     "Manager",  3),
    mkItem("Submit tax & benefits forms",        "HR",       5),
  ]),
  seedTemplate("Engineering Onboarding", "Specific to engineering hires", [
    mkItem("Set up dev environment",             "IT",       3),
    mkItem("Codebase walkthrough with mentor",   "Manager",  5),
    mkItem("First PR merged",                    "Employee", 14),
  ]),
];

function mkChecklist(employeeId: number, templateId: number, daysAgo: number): OnboardingChecklistDto {
  const tmpl = templates.find((t) => t.id === templateId)!;
  const employee = mockUsers.find((u) => u.id === employeeId)!;
  const startedAt = new Date(Date.now() - daysAgo * 86400000);
  const items = tmpl.items.map((it, idx) => ({
    id: nextChecklistItemId++,
    publicId: newGuid(),
    description: it.description,
    dueDate: new Date(startedAt.getTime() + it.defaultDueDays * 86400000).toISOString(),
    completedAt: idx < 2 ? new Date(startedAt.getTime() + (idx + 0.5) * 86400000).toISOString() : undefined,
    status: (idx < 2 ? "Completed" : "Pending") as "Pending" | "Completed",
  }));
  const completed = items.filter((i) => i.status === "Completed").length;
  return {
    id: nextChecklistId++,
    publicId: newGuid(),
    employeeId,
    employeeName: employee.name,
    templateId,
    templateName: tmpl.name,
    startedAt: startedAt.toISOString(),
    status: completed === items.length ? "Completed" : "Active",
    totalItems: items.length,
    completedItems: completed,
    items,
  };
}

let checklists: OnboardingChecklistDto[] = [
  mkChecklist(5, 1, 3),
  mkChecklist(6, 1, 1),
];

export const mockOnboarding = {
  // Templates
  async listTemplates(): Promise<OnboardingTemplateDto[]> {
    return [...templates];
  },
  async createTemplate(dto: CreateOnboardingTemplateDto): Promise<OnboardingTemplateDto> {
    const items = dto.items.map((i) => mkItem(i.description, i.responsibleRole, i.defaultDueDays));
    const t = seedTemplate(dto.name, dto.description ?? "", items);
    templates = [...templates, t];
    return t;
  },
  async updateTemplate(id: number, dto: Partial<CreateOnboardingTemplateDto>): Promise<OnboardingTemplateDto | null> {
    const t = templates.find((x) => x.id === id);
    if (!t) return null;
    if (dto.name) t.name = dto.name;
    if (dto.description !== undefined) t.description = dto.description;
    return t;
  },
  async deleteTemplate(id: number): Promise<boolean> {
    const inUse = checklists.some((c) => c.templateId === id);
    if (inUse) throw new Error("Template in use by active checklists");
    const before = templates.length;
    templates = templates.filter((t) => t.id !== id);
    return templates.length < before;
  },
  async addItem(templateId: number, dto: CreateOnboardingTemplateItemDto): Promise<OnboardingTemplateItemDto | null> {
    const t = templates.find((x) => x.id === templateId);
    if (!t) return null;
    const it = mkItem(dto.description, dto.responsibleRole, dto.defaultDueDays);
    t.items = [...t.items, it];
    return it;
  },
  async updateItem(templateId: number, itemId: number, dto: Partial<CreateOnboardingTemplateItemDto>): Promise<OnboardingTemplateItemDto | null> {
    const t = templates.find((x) => x.id === templateId);
    if (!t) return null;
    const it = t.items.find((i) => i.id === itemId);
    if (!it) return null;
    if (dto.description) it.description = dto.description;
    if (dto.responsibleRole) it.responsibleRole = dto.responsibleRole;
    if (dto.defaultDueDays != null) it.defaultDueDays = dto.defaultDueDays;
    return it;
  },
  async deleteItem(templateId: number, itemId: number): Promise<boolean> {
    const t = templates.find((x) => x.id === templateId);
    if (!t) return false;
    const before = t.items.length;
    t.items = t.items.filter((i) => i.id !== itemId);
    return t.items.length < before;
  },
  async reorderItems(templateId: number, orderedItemIds: number[]): Promise<OnboardingTemplateDto | null> {
    const t = templates.find((x) => x.id === templateId);
    if (!t) return null;
    const map = new Map(t.items.map((i) => [i.id, i] as const));
    t.items = orderedItemIds.map((id) => map.get(id)!).filter(Boolean);
    return t;
  },
  // Checklists
  async listChecklists(): Promise<OnboardingChecklistDto[]> {
    return [...checklists];
  },
  async assign(dto: AssignChecklistDto): Promise<OnboardingChecklistDto> {
    const c = mkChecklist(dto.employeeId, dto.templateId, 0);
    // reset items to all-pending for a fresh assignment
    c.items = c.items.map((i) => ({ ...i, completedAt: undefined, status: "Pending" }));
    c.completedItems = 0;
    c.status = "Active";
    checklists = [c, ...checklists];
    return c;
  },
  async markItemComplete(checklistId: number, itemId: number): Promise<OnboardingChecklistDto | null> {
    const c = checklists.find((x) => x.id === checklistId);
    if (!c) return null;
    const it = c.items.find((i) => i.id === itemId);
    if (!it) return null;
    it.completedAt = new Date().toISOString();
    it.status = "Completed";
    c.completedItems = c.items.filter((i) => i.status === "Completed").length;
    if (c.completedItems === c.totalItems) {
      c.status = "Completed";
      c.completedAt = new Date().toISOString();
    }
    return c;
  },
};

