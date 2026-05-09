"use client";

import { useEffect, useSyncExternalStore } from "react";
import { useRouter } from "next/navigation";
import { getStoredAuth, isExpired } from "@/lib/auth";

type AuthStatus = "unknown" | "authed" | "unauthed";

function getAuthStatus(): AuthStatus {
  const auth = getStoredAuth();
  if (!auth || isExpired(auth)) return "unauthed";
  return "authed";
}

function subscribe(): () => void {
  return () => {};
}

function getServerSnapshot(): AuthStatus {
  return "unknown";
}

export function AuthGate({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const status = useSyncExternalStore<AuthStatus>(
    subscribe,
    getAuthStatus,
    getServerSnapshot,
  );

  useEffect(() => {
    if (status === "unauthed") {
      router.replace("/login");
    }
  }, [status, router]);

  if (status !== "authed") {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background text-sm text-muted-foreground">
        Loading…
      </div>
    );
  }

  return <>{children}</>;
}
