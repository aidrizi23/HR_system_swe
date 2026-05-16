"use client";

import { useEffect, useRef, useState } from "react";
import { ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";

export interface SelectOption {
  value: string;
  label: string;
}

interface Props {
  value: string;
  onChange: (value: string) => void;
  options: SelectOption[];
  placeholder?: string;
  className?: string;
  disabled?: boolean;
  size?: "sm" | "md";
}

// Drop-in replacement for native <select>. Renders its menu with plain-CSS
// `relative` + `absolute` positioning so it never collapses to the top-left
// the way native selects do under certain browser/CSS combinations.
export function Select({ value, onChange, options, placeholder = "Select…", className, disabled, size = "md" }: Props) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!open) return;
    const onMouseDown = (e: MouseEvent) => {
      if (!ref.current?.contains(e.target as Node)) setOpen(false);
    };
    const onKey = (e: KeyboardEvent) => { if (e.key === "Escape") setOpen(false); };
    document.addEventListener("mousedown", onMouseDown);
    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onMouseDown);
      document.removeEventListener("keydown", onKey);
    };
  }, [open]);

  const selected = options.find((o) => o.value === value);
  const label = selected?.label ?? placeholder;
  const pad = size === "sm" ? "px-3 py-1.5 text-xs" : "px-3 py-2 text-sm";

  return (
    <div ref={ref} className={cn("relative", className)}>
      <button
        type="button"
        onClick={() => !disabled && setOpen((v) => !v)}
        aria-haspopup="listbox"
        aria-expanded={open}
        disabled={disabled}
        className={cn(
          "inline-flex w-full items-center justify-between gap-2 rounded-lg border border-border bg-background text-left text-foreground transition-colors hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-50",
          pad,
        )}
      >
        <span className={selected ? "" : "text-muted-foreground"}>{label}</span>
        <ChevronDown className={cn("h-4 w-4 shrink-0 text-muted-foreground transition-transform", open && "rotate-180")} />
      </button>
      {open && (
        <ul
          role="listbox"
          className="absolute left-0 right-0 top-[calc(100%+4px)] z-50 max-h-60 overflow-y-auto rounded-lg border border-border bg-card py-1 shadow-xl"
        >
          {options.map((o) => (
            <li key={o.value}>
              <button
                type="button"
                role="option"
                aria-selected={o.value === value}
                onClick={() => { onChange(o.value); setOpen(false); }}
                className={cn(
                  "block w-full px-3 py-1.5 text-left text-sm hover:bg-secondary",
                  o.value === value && "bg-primary/10 font-semibold text-foreground",
                )}
              >
                {o.label}
              </button>
            </li>
          ))}
          {options.length === 0 && (
            <li className="px-3 py-2 text-xs text-muted-foreground">No options</li>
          )}
        </ul>
      )}
    </div>
  );
}
