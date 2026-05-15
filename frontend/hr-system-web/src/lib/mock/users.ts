export type MockRole =
  | "Employee"
  | "TeamLead"
  | "DepartmentManager"
  | "HRManager"
  | "SuperAdmin";

export interface MockUser {
  id: number;
  employeeId: number;
  name: string;
  email: string;
  role: MockRole;
}

export const mockUsers: MockUser[] = [
  { id: 1, employeeId: 1, name: "Admin User",     email: "admin@hrsystem.com",     role: "SuperAdmin" },
  { id: 2, employeeId: 2, name: "Hannah Reyes",   email: "hannah@hrsystem.com",    role: "HRManager" },
  { id: 3, employeeId: 3, name: "Diego Marin",    email: "diego@hrsystem.com",     role: "DepartmentManager" },
  { id: 4, employeeId: 4, name: "Lila Park",      email: "lila@hrsystem.com",      role: "TeamLead" },
  { id: 5, employeeId: 5, name: "Noah Schmidt",   email: "noah@hrsystem.com",      role: "Employee" },
  { id: 6, employeeId: 6, name: "Priya Iyer",     email: "priya@hrsystem.com",     role: "Employee" },
];

// Toggle this constant to test role gating before A's auth lands.
const CURRENT_USER_ID = 1;

export function getCurrentMockUser(): MockUser {
  return mockUsers.find((u) => u.id === CURRENT_USER_ID)!;
}

export function isHrOrAbove(role: MockRole): boolean {
  return role === "HRManager" || role === "SuperAdmin";
}
