const TOKEN_KEY = "hr.auth.token";
const EXPIRES_KEY = "hr.auth.expires";

export type StoredAuth = {
  token: string;
  expiresAt: string;
};

export function getStoredAuth(): StoredAuth | null {
  if (typeof window === "undefined") return null;
  const token = window.localStorage.getItem(TOKEN_KEY);
  const expiresAt = window.localStorage.getItem(EXPIRES_KEY);
  if (!token || !expiresAt) return null;
  return { token, expiresAt };
}

export function setStoredAuth(auth: StoredAuth): void {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(TOKEN_KEY, auth.token);
  window.localStorage.setItem(EXPIRES_KEY, auth.expiresAt);
}

export function clearStoredAuth(): void {
  if (typeof window === "undefined") return;
  window.localStorage.removeItem(TOKEN_KEY);
  window.localStorage.removeItem(EXPIRES_KEY);
}

export function isExpired(auth: StoredAuth): boolean {
  const expiresAtMs = new Date(auth.expiresAt).getTime();
  if (Number.isNaN(expiresAtMs)) return true;
  return expiresAtMs <= Date.now();
}
