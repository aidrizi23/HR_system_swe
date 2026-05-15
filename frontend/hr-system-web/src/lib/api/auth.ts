import { api } from "./client";

export type LoginRequest = {
  email: string;
  password: string;
};

export type LoginUser = {
  id: number;
  email: string;
  role: string;
  employeeId: number | null;
};

export type LoginResponse = {
  token: string;
  refreshToken?: string;
  expiresAt: string;
  user: LoginUser;
};

export async function login(payload: LoginRequest): Promise<LoginResponse> {
  const { data } = await api.post<LoginResponse>("/Auth/login", payload);
  return data;
}

// Pull the employee profile (name + job title) so the topbar/sidebar can show it.
export async function getEmployeeById(id: number): Promise<{ firstName: string; lastName: string; jobTitle?: string } | null> {
  try {
    const { data } = await api.get<{ firstName: string; lastName: string; jobTitle?: string }>(`/employees/${id}`);
    return data;
  } catch {
    return null;
  }
}
