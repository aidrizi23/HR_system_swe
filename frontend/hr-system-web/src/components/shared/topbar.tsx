"use client";

import { usePathname, useRouter } from "next/navigation";
import {
  Bell,
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
import { useTheme } from "next-themes";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { clearStoredAuth } from "@/lib/auth";
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

  const today = dateFormatter.format(new Date());

  const eyebrow = nav?.eyebrow ?? "OVERVIEW";
  const title = nav?.title ?? "HR System";
  const subtitle = nav?.subtitle ?? "";

  const handleSignOut = () => {
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

        <span className="hidden h-[38px] items-center gap-2 rounded-full border border-border bg-card px-3.5 text-[13px] font-semibold text-[#1f2a3a] sm:inline-flex">
          <ShieldCheck
            className="h-4 w-4 text-muted-foreground"
            strokeWidth={1.8}
          />
          Super Admin
        </span>

        <button
          type="button"
          aria-label="Toggle theme"
          onClick={() => setTheme(resolvedTheme === "dark" ? "light" : "dark")}
          className="grid h-[38px] w-[38px] place-items-center rounded-full border border-border bg-card text-[#1f2a3a] transition-colors hover:bg-secondary"
        >
          <Sun className="h-4 w-4 dark:hidden" strokeWidth={1.8} />
          <Moon className="hidden h-4 w-4 dark:block" strokeWidth={1.8} />
        </button>

        <button
          type="button"
          aria-label="Notifications"
          className="relative grid h-[38px] w-[38px] place-items-center rounded-full border border-border bg-card text-[#1f2a3a] transition-colors hover:bg-secondary"
        >
          <Bell className="h-4 w-4" strokeWidth={1.8} />
          <span className="absolute right-[10px] top-[9px] h-[7px] w-[7px] rounded-full border-[2px] border-card bg-[#e44b4b]" />
        </button>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <button
              type="button"
              className="inline-flex h-[46px] items-center gap-2.5 rounded-full border border-border bg-card py-1 pl-1.5 pr-3.5 transition-colors hover:bg-secondary"
            >
              <span className="grid h-[34px] w-[34px] place-items-center rounded-full bg-[#0b1220] text-[13.5px] font-bold text-white">
                A
              </span>
              <span className="hidden flex-col leading-[1.15] md:flex">
                <strong className="text-[12.5px] font-bold text-foreground">
                  admin@hrsystem.com
                </strong>
                <span className="text-[11.5px] text-muted-foreground">
                  Super Admin
                </span>
              </span>
              <ChevronDown
                className="ml-0.5 h-[14px] w-[14px] text-muted-foreground"
                strokeWidth={2}
              />
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-56">
            <DropdownMenuItem>
              <UserRound className="mr-2 h-4 w-4" />
              Profile
            </DropdownMenuItem>
            <DropdownMenuItem>
              <Settings className="mr-2 h-4 w-4" />
              Settings
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              onSelect={handleSignOut}
              className="text-destructive focus:text-destructive"
            >
              <LogOut className="mr-2 h-4 w-4" />
              Sign out
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </header>
  );
}
