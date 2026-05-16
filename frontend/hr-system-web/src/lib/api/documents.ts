import { mockDocuments, mockDocumentCategories } from "@/lib/mock/documents";
import type {
  CreateDocumentCategoryDto,
  UploadDocumentDto,
} from "@/types";

export const apiDocuments = {
  listByEmployee: (employeeId: number) => mockDocuments.listByEmployee(employeeId),
  listAll:        ()                   => mockDocuments.listAll(),
  expiring:       (daysAhead = 30)     => mockDocuments.expiring(daysAhead),
  upload:         (employeeId: number, file: File, dto: UploadDocumentDto) =>
                    mockDocuments.upload(employeeId, file, dto),
  remove:         (id: number)         => mockDocuments.remove(id),
};

export const apiDocumentCategories = {
  list:          ()                                       => mockDocumentCategories.list(),
  create:        (dto: CreateDocumentCategoryDto)         => mockDocumentCategories.create(dto),
  update:        (id: number, dto: CreateDocumentCategoryDto) => mockDocumentCategories.update(id, dto),
  remove:        (id: number)                             => mockDocumentCategories.remove(id),
  documentCount: (id: number)                             => mockDocumentCategories.documentCount(id),
};
