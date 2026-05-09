"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Briefcase } from "lucide-react";
import { NAV_GROUPS } from "@/lib/nav";
import { cn } from "@/lib/utils";

export function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="hidden w-[270px] shrink-0 flex-col gap-1.5 self-start rounded-[20px] border border-border bg-sidebar p-3.5 pb-[18px] shadow-[0_1px_2px_rgba(15,23,42,0.03)] lg:sticky lg:top-[18px] lg:m-[18px] lg:mr-0 lg:flex lg:max-h-[calc(100vh-36px)] lg:overflow-y-auto">
      <div className="flex items-center gap-3 rounded-[14px] border border-border bg-card p-3 shadow-[0_1px_1px_rgba(15,23,42,0.02)]">
        <div className="grid h-10 w-10 place-items-center rounded-[10px] bg-primary text-[13px] font-extrabold tracking-[0.04em] text-primary-foreground">
          HR
        </div>
        <div className="flex min-w-0 flex-col leading-[1.2]">
          <strong className="max-w-[160px] truncate text-[13.5px] font-bold text-foreground">
            Human Resources S…
          </strong>
          <span className="text-[12px] text-muted-foreground">Super Admin</span>
        </div>
      </div>

      {NAV_GROUPS.map((group) => (
        <div key={group.name} className="mt-3.5">
          <div className="px-3 pb-2 pt-1.5 text-[10.5px] font-bold uppercase tracking-[0.18em] text-[#8a93a4]">
            {group.name}
          </div>
          {group.items.map((item) => {
            const Icon = item.icon;
            const active =
              pathname === item.href || pathname.startsWith(`${item.href}/`);
            return (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  "flex items-center gap-3 rounded-[10px] px-3 py-[9px] text-[13.5px] font-medium transition-colors",
                  active
                    ? "bg-sidebar-accent font-semibold text-sidebar-accent-foreground"
                    : "text-[#3b465a] hover:bg-secondary hover:text-foreground",
                )}
              >
                <span
                  className={cn(
                    "grid h-[18px] w-[18px] shrink-0 place-items-center",
                    active ? "text-primary" : "text-[#6b7689]",
                  )}
                >
                  <Icon className="h-[18px] w-[18px]" strokeWidth={1.8} />
                </span>
                <span className="truncate">{item.label}</span>
              </Link>
            );
          })}
        </div>
      ))}

      <div className="mt-auto border-t border-[#eef0f5] pt-3.5">
        <div className="flex items-center gap-3 rounded-[14px] border border-border bg-[#fafbfd] px-3 py-2.5">
          <div className="grid h-[34px] w-[34px] shrink-0 place-items-center rounded-[10px] bg-[#eaf0ff] text-[#2952ec]">
            <Briefcase className="h-[18px] w-[18px]" strokeWidth={1.8} />
          </div>
          <div className="flex min-w-0 flex-col leading-[1.2]">
            <strong className="text-[13px] font-bold text-foreground">
              Super Admin
            </strong>
            <span className="max-w-[160px] truncate text-[11.5px] text-muted-foreground">
              admin@hrsystem.com
            </span>
          </div>
        </div>
      </div>
    </aside>
  );
}
