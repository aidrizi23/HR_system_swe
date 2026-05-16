import type {
  EmployeeDocumentDto,
  DocumentCategoryDto,
  CreateDocumentCategoryDto,
  UploadDocumentDto,
} from "@/types";
import { mockUsers, getCurrentMockUser } from "./users";

let nextDocId = 1;
let nextCategoryId = 1;

function slugify(s: string): string {
  return s.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
}

function newGuid(): string {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

const daysFromNow = (n: number) =>
  new Date(Date.now() + n * 86400000).toISOString();

function seedCategory(name: string, description?: string): DocumentCategoryDto {
  const id = nextCategoryId++;
  return {
    id,
    publicId: newGuid(),
    name,
    description,
    slug: slugify(name),
  };
}

let categories: DocumentCategoryDto[] = [
  seedCategory("Contracts",  "Signed employment contracts"),
  seedCategory("ID & Tax",   "Government-issued IDs and tax forms"),
  seedCategory("Policies",   "Acknowledged company policies"),
];

function seedDoc(
  employeeId: number,
  categoryId: number,
  fileName: string,
  contentType: string,
  fileSize: number,
  expiryDate?: string,
): EmployeeDocumentDto {
  const id = nextDocId++;
  const employee = mockUsers.find((u) => u.id === employeeId)!;
  const category = categories.find((c) => c.id === categoryId)!;
  return {
    id,
    publicId: newGuid(),
    employeeId,
    employeeName: employee.name,
    categoryId,
    categoryName: category.name,
    fileName,
    fileSize,
    contentType,
    expiryDate,
    uploadedById: 2, // HR
    createdAt: daysFromNow(-30),
  };
}

let documents: EmployeeDocumentDto[] = [
  seedDoc(1, 1, "Employment_contract_admin.pdf", "application/pdf", 524_288),
  seedDoc(1, 3, "Code_of_conduct_signed.pdf",    "application/pdf", 233_472),
  seedDoc(5, 1, "Employment_contract_noah.pdf",  "application/pdf", 511_876, daysFromNow(20)),
  seedDoc(5, 2, "ID_scan_noah.png",              "image/png",       1_152_000),
  seedDoc(6, 1, "Employment_contract_priya.pdf", "application/pdf", 489_201, daysFromNow(5)),
];

export const mockDocuments = {
  async listByEmployee(employeeId: number): Promise<EmployeeDocumentDto[]> {
    return documents.filter((d) => d.employeeId === employeeId);
  },
  async listAll(): Promise<EmployeeDocumentDto[]> {
    return [...documents];
  },
  async expiring(daysAhead = 30): Promise<EmployeeDocumentDto[]> {
    const cutoff = Date.now() + daysAhead * 86400000;
    return documents.filter(
      (d) => d.expiryDate && new Date(d.expiryDate).getTime() <= cutoff,
    );
  },
  async upload(
    employeeId: number,
    file: File,
    dto: UploadDocumentDto,
  ): Promise<EmployeeDocumentDto> {
    const me = getCurrentMockUser();
    const category = categories.find((c) => c.id === dto.categoryId);
    if (!category) throw new Error("Category not found");
    const employee = mockUsers.find((u) => u.id === employeeId);
    if (!employee) throw new Error("Employee not found");
    const id = nextDocId++;
    const doc: EmployeeDocumentDto = {
      id,
      publicId: newGuid(),
      employeeId,
      employeeName: employee.name,
      categoryId: category.id,
      categoryName: category.name,
      fileName: file.name,
      fileSize: file.size,
      contentType: file.type || "application/octet-stream",
      expiryDate: dto.expiryDate,
      uploadedById: me.id,
      notes: dto.notes,
      createdAt: new Date().toISOString(),
    };
    documents = [doc, ...documents];
    return doc;
  },
  async remove(id: number): Promise<boolean> {
    const before = documents.length;
    documents = documents.filter((d) => d.id !== id);
    return documents.length < before;
  },
};

export const mockDocumentCategories = {
  async list(): Promise<DocumentCategoryDto[]> {
    return [...categories];
  },
  async create(dto: CreateDocumentCategoryDto): Promise<DocumentCategoryDto> {
    const id = nextCategoryId++;
    const cat: DocumentCategoryDto = {
      id,
      publicId: newGuid(),
      name: dto.name,
      description: dto.description,
      slug: slugify(dto.name),
    };
    categories = [...categories, cat];
    return cat;
  },
  async update(id: number, dto: CreateDocumentCategoryDto): Promise<DocumentCategoryDto | null> {
    const c = categories.find((x) => x.id === id);
    if (!c) return null;
    c.name = dto.name;
    c.description = dto.description;
    c.slug = slugify(dto.name);
    return c;
  },
  async remove(id: number): Promise<boolean> {
    const inUse = documents.some((d) => d.categoryId === id);
    if (inUse) throw new Error("Category in use by existing documents");
    const before = categories.length;
    categories = categories.filter((c) => c.id !== id);
    return categories.length < before;
  },
  async documentCount(id: number): Promise<number> {
    return documents.filter((d) => d.categoryId === id).length;
  },
};
