import axios, { type AxiosInstance } from "axios";
import { clearStoredAuth, getStoredAuth, isExpired } from "@/lib/auth";

const baseURL =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5056/api";

export const api: AxiosInstance = axios.create({
  baseURL,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use((config) => {
  const auth = getStoredAuth();
  if (auth && !isExpired(auth)) {
    config.headers.Authorization = `Bearer ${auth.token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error?.response?.status === 401) {
      clearStoredAuth();
      if (
        typeof window !== "undefined" &&
        window.location.pathname !== "/login"
      ) {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  },
);
