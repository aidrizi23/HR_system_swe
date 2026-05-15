// Live backend not yet built — owned by G's `documents-and-onboarding` branch (see BRANCH_PLAN.md row 9).
// Swap this body to axios calls against /api/onboarding when that branch lands.
import { mockOnboarding } from "@/lib/mock/onboarding";
import type {
  AssignChecklistDto,
  CreateOnboardingTemplateDto,
  CreateOnboardingTemplateItemDto,
} from "@/types";

export const apiOnboarding = {
  // templates
  listTemplates:  ()                                       => mockOnboarding.listTemplates(),
  createTemplate: (dto: CreateOnboardingTemplateDto)       => mockOnboarding.createTemplate(dto),
  updateTemplate: (id: number, dto: Partial<CreateOnboardingTemplateDto>) => mockOnboarding.updateTemplate(id, dto),
  deleteTemplate: (id: number)                             => mockOnboarding.deleteTemplate(id),
  // items
  addItem:        (templateId: number, dto: CreateOnboardingTemplateItemDto) => mockOnboarding.addItem(templateId, dto),
  updateItem:     (templateId: number, itemId: number, dto: Partial<CreateOnboardingTemplateItemDto>) => mockOnboarding.updateItem(templateId, itemId, dto),
  deleteItem:     (templateId: number, itemId: number)    => mockOnboarding.deleteItem(templateId, itemId),
  reorderItems:   (templateId: number, orderedItemIds: number[]) => mockOnboarding.reorderItems(templateId, orderedItemIds),
  // checklists
  listChecklists: ()                                       => mockOnboarding.listChecklists(),
  assign:         (dto: AssignChecklistDto)                => mockOnboarding.assign(dto),
  markItemComplete: (checklistId: number, itemId: number) => mockOnboarding.markItemComplete(checklistId, itemId),
};
