"use client";

import { usePathname, useRouter } from "next/navigation";
import {
  Calendar,
  ChevronDown,
  LogOut,
  Moon,
  PanelLeft,
  Settings,
  ShieldCheck,
  Sun,
  UserRound,
} from "lucide-react";
import { NotificationBell } from "@/components/notifications/notification-bell";
import { useTheme } from "next-themes";
import { useEffect, useRef, useState } from "react";
import { type AuthUser, clearStoredAuth, getStoredUser, humanizeRole, initialsFromName } from "@/lib/auth";
import { findNavByPath } from "@/lib/nav";

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  weekday: "short",
  month: "short",
  day: "numeric",
});

export function Topbar() {
  const pathname = usePathname();
  const router = useRouter();
  const nav = findNavByPath(pathname);
  const { resolvedTheme, setTheme } = useTheme();
  const [user, setUser] = useState<AuthUser | null>(null);
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    // localStorage is unavailable during SSR; read it after mount.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setUser(getStoredUser());
  }, []);

  // Close the menu on outside-click and on Escape.
  useEffect(() => {
    if (!menuOpen) return;
    const onClick = (e: MouseEvent) => {
      if (!menuRef.current?.contains(e.target as Node)) setMenuOpen(false);
    };
    const onKey = (e: KeyboardEvent) => { if (e.key === "Escape") setMenuOpen(false); };
    document.addEventListener("mousedown", onClick);
    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onClick);
      document.removeEventListener("keydown", onKey);
    };
  }, [menuOpen]);

  const today = dateFormatter.format(new Date());

  const eyebrow = nav?.eyebrow ?? "OVERVIEW";
  const title = nav?.title ?? "HR System";
  const subtitle = nav?.subtitle ?? "";

  const displayName = user?.name?.trim() || user?.email || "—";
  const displayRole = humanizeRole(user?.role);
  const initials = initialsFromName(user?.name, user?.email);

  const handleSignOut = () => {
    setMenuOpen(false);
    clearStoredAuth();
    router.replace("/login");
  };

  return (
    <header className="flex items-center justify-between gap-6 border-b border-[#eef0f5] bg-transparent px-7 py-[18px]">
      <div className="flex min-w-0 items-start gap-3.5">
        <button
          type="button"
          aria-label="Toggle sidebar"
          className="mt-1.5 grid h-[38px] w-[38px] shrink-0 place-items-center rounded-[10px] border border-border bg-card text-[#1f2a3a] transition-colors hover:bg-secondary"
        >
          <PanelLeft className="h-[18px] w-[18px]" strokeWidth={1.8} />
        </button>
        <div className="flex min-w-0 flex-col gap-0.5">
          <span className="text-[10.5px] font-semibold uppercase tracking-[0.18em] text-muted-foreground">
            {eyebrow}
          </span>
          <h1 className="text-[22px] font-extrabold tracking-[-0.01em] text-foreground">
            {title}
          </h1>
          {subtitle && (
            <p className="mt-0.5 text-[13px] text-muted-foreground">
              {subtitle}
            </p>
          )}
        </div>
      </div>

      <div className="flex shrink-0 items-center gap-2.5">
        <span
          className="hidden h-[38px] items-center gap-2 rounded-full border border-border bg-card px-3.5 text-[13px] font-semibold text-[#1f2a3a] sm:inline-flex"
          suppressHydrationWarning
        >
          <Calendar
            className="h-4 w-4 text-muted-foreground"
            strokeWidth={1.8}
          />
          {today}
        </span>

        {displayRole && (
          <span className="hidden h-[38px] items-center gap-2 rounded-full border border-border bg-card px-3.5 text-[13px] font-semibold text-[#1f2a3a] sm:inline-flex">
            <ShieldCheck
              className="h-4 w-4 text-muted-foreground"
              strokeWidth={1.8}
            />
            {displayRole}
          </span>
        )}

        <button
          type="button"
          aria-label="Toggle theme"
          onClick={() => setTheme(resolvedTheme === "dark" ? "light" : "dark")}
          className="grid h-[38px] w-[38px] place-items-center rounded-full border border-border bg-card text-[#1f2a3a] transition-colors hover:bg-secondary"
        >
          <Sun className="h-4 w-4 dark:hidden" strokeWidth={1.8} />
          <Moon className="hidden h-4 w-4 dark:block" strokeWidth={1.8} />
        </button>

        <NotificationBell />

        <div ref={menuRef} className="relative">
          <button
            type="button"
            onClick={() => setMenuOpen((v) => !v)}
            aria-haspopup="menu"
            aria-expanded={menuOpen}
            className="inline-flex h-[46px] items-center gap-2.5 rounded-full border border-border bg-card py-1 pl-1.5 pr-3.5 transition-colors hover:bg-secondary"
          >
            <span className="grid h-[34px] w-[34px] place-items-center rounded-full bg-[#0b1220] text-[13.5px] font-bold text-white">
              {initials || "?"}
            </span>
            <span className="hidden flex-col leading-[1.15] md:flex">
              <strong className="text-[12.5px] font-bold text-foreground">
                {displayName}
              </strong>
              <span className="text-[11.5px] text-muted-foreground">
                {displayRole || "—"}
              </span>
            </span>
            <ChevronDown
              className={`ml-0.5 h-[14px] w-[14px] text-muted-foreground transition-transform ${menuOpen ? "rotate-180" : ""}`}
              strokeWidth={2}
            />
          </button>
          {menuOpen && (
            <div
              role="menu"
              className="absolute right-0 top-[calc(100%+6px)] z-50 w-56 overflow-hidden rounded-xl border border-border bg-card shadow-xl"
            >
              <button
                type="button"
                role="menuitem"
                className="flex w-full items-center gap-2 px-3 py-2 text-left text-sm hover:bg-secondary"
              >
                <UserRound className="h-4 w-4" />
                Profile
              </button>
              <button
                type="button"
                role="menuitem"
                className="flex w-full items-center gap-2 px-3 py-2 text-left text-sm hover:bg-secondary"
              >
                <Settings className="h-4 w-4" />
                Settings
              </button>
              <div className="h-px bg-border" />
              <button
                type="button"
                role="menuitem"
                onClick={handleSignOut}
                className="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-destructive hover:bg-destructive/10"
              >
                <LogOut className="h-4 w-4" />
                Sign out
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
