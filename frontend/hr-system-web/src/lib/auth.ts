const TOKEN_KEY = "hr.auth.token";
const EXPIRES_KEY = "hr.auth.expires";
const USER_KEY = "hr.auth.user";

export type AuthUser = {
  id: number;
  email: string;
  role: string;
  employeeId: number | null;
  name?: string;        // full name from the backend (optional — may not be in older sessions)
  jobTitle?: string;
};

export type StoredAuth = {
  token: string;
  expiresAt: string;
  user?: AuthUser;
};

export function getStoredAuth(): StoredAuth | null {
  if (typeof window === "undefined") return null;
  const token = window.localStorage.getItem(TOKEN_KEY);
  const expiresAt = window.localStorage.getItem(EXPIRES_KEY);
  if (!token || !expiresAt) return null;
  const userJson = window.localStorage.getItem(USER_KEY);
  let user: AuthUser | undefined;
  if (userJson) {
    try { user = JSON.parse(userJson) as AuthUser; } catch { /* ignore stale */ }
  }
  return { token, expiresAt, user };
}

export function getStoredUser(): AuthUser | null {
  return getStoredAuth()?.user ?? null;
}

export function setStoredAuth(auth: StoredAuth): void {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(TOKEN_KEY, auth.token);
  window.localStorage.setItem(EXPIRES_KEY, auth.expiresAt);
  if (auth.user) {
    window.localStorage.setItem(USER_KEY, JSON.stringify(auth.user));
  }
}

export function clearStoredAuth(): void {
  if (typeof window === "undefined") return;
  window.localStorage.removeItem(TOKEN_KEY);
  window.localStorage.removeItem(EXPIRES_KEY);
  window.localStorage.removeItem(USER_KEY);
}

export function isExpired(auth: StoredAuth): boolean {
  const expiresAtMs = new Date(auth.expiresAt).getTime();
  if (Number.isNaN(expiresAtMs)) return true;
  return expiresAtMs <= Date.now();
}

// Turn "HRManager" / "SuperAdmin" / "DepartmentManager" into "HR Manager" / "Super Admin" / etc.
export function humanizeRole(role: string | undefined | null): string {
  if (!role) return "";
  return role.replace(/([a-z])([A-Z])/g, "$1 $2");
}

export function initialsFromName(name: string | undefined | null, fallbackEmail?: string): string {
  if (name && name.trim().length > 0) {
    return name.trim().split(/\s+/).slice(0, 2).map((p) => p[0]?.toUpperCase() ?? "").join("");
  }
  if (fallbackEmail) return fallbackEmail[0]?.toUpperCase() ?? "?";
  return "?";
}
