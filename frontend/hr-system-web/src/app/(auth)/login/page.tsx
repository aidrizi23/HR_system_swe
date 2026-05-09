"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import {
  ArrowRight,
  Briefcase,
  Building2,
  CheckSquare,
  Clock,
  ShieldCheck,
  UserCog,
  UserRound,
  Users,
} from "lucide-react";
import { login } from "@/lib/api/auth";
import { getStoredAuth, isExpired, setStoredAuth } from "@/lib/auth";
import { cn } from "@/lib/utils";

const loginSchema = z.object({
  email: z.string().min(1, "Email is required").email("Enter a valid email"),
  password: z.string().min(1, "Password is required"),
});

type LoginValues = z.infer<typeof loginSchema>;

const DEMO_ACCOUNTS = [
  {
    role: "Super Admin",
    email: "admin@hrsystem.com",
    icon: ShieldCheck,
    tint: { bg: "#fdecec", fg: "#d24b4b" },
  },
  {
    role: "HR Manager",
    email: "hr@hrsystem.com",
    icon: Users,
    tint: { bg: "#e6f0fb", fg: "#2f7ed1" },
  },
  {
    role: "Department Manager",
    email: "manager@hrsystem.com",
    icon: Briefcase,
    tint: { bg: "#fbf0d9", fg: "#b3791b" },
  },
  {
    role: "Team Lead",
    email: "lead@hrsystem.com",
    icon: UserCog,
    tint: { bg: "#efe9fb", fg: "#6e4dc6" },
  },
  {
    role: "Employee",
    email: "employee@hrsystem.com",
    icon: UserRound,
    tint: { bg: "#e2f3ea", fg: "#2f9b65" },
  },
];

const FEATURES = [
  {
    icon: Users,
    title: "People Operations",
    body: "Employees, departments, onboarding, and structured approvals.",
    tint: { bg: "#eaf0ff", fg: "#2952ec" },
    role: false,
  },
  {
    icon: Clock,
    title: "Time & Leave",
    body: "Daily logging, overtime, holiday planning, and request workflows.",
    tint: { bg: "#efe9fb", fg: "#6e4dc6" },
    role: false,
  },
  {
    icon: CheckSquare,
    title: "Execution",
    body: "Tasks, notifications, and role-specific action queues in one place.",
    tint: { bg: "#e6f0fb", fg: "#2f7ed1" },
    role: false,
  },
  {
    icon: ShieldCheck,
    title: "ROLE-AWARE WORKSPACE",
    body: "Each user sees a sharper view of the same system: HR gets oversight, managers get team control, leads get delivery focus, and employees get a cleaner daily workspace.",
    tint: { bg: "#e2f3ea", fg: "#2f9b65" },
    role: true,
  },
];

export default function LoginPage() {
  const router = useRouter();
  const [submitError, setSubmitError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  useEffect(() => {
    const auth = getStoredAuth();
    if (auth && !isExpired(auth)) {
      router.replace("/dashboard");
    }
  }, [router]);

  const onSubmit = async (values: LoginValues) => {
    setSubmitError(null);
    try {
      const result = await login(values);
      setStoredAuth({ token: result.token, expiresAt: result.expiresAt });
      router.replace("/dashboard");
    } catch (err: unknown) {
      const status =
        typeof err === "object" && err !== null && "response" in err
          ? (err as { response?: { status?: number } }).response?.status
          : undefined;
      setSubmitError(
        status === 401
          ? "Invalid email or password."
          : "Something went wrong. Try again.",
      );
    }
  };

  const handleDemoFill = (email: string) => {
    setValue("email", email, { shouldValidate: true });
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-6 py-4">
      <div className="grid w-full max-w-[1320px] gap-6 lg:grid-cols-[1.18fr_1fr]">
        <section className="rounded-[22px] border border-border bg-card p-8 lg:px-10 lg:py-8">
          <div className="mb-8 flex items-center gap-4">
            <div className="grid h-[54px] w-[54px] place-items-center rounded-[14px] bg-primary text-primary-foreground shadow-[0_4px_14px_rgba(41,82,236,0.28)]">
              <Building2 className="h-[26px] w-[26px]" strokeWidth={1.8} />
            </div>
            <div>
              <div className="text-[11px] font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                HR System
              </div>
              <div className="mt-0.5 text-[22px] font-extrabold tracking-[-0.01em] text-foreground">
                Operations Console
              </div>
            </div>
          </div>

          <div className="mt-1.5 text-[11px] font-semibold uppercase tracking-[0.16em] text-muted-foreground">
            Structured workforce management
          </div>
          <h1 className="mt-3 max-w-[18ch] text-[44px] font-extrabold leading-[1.05] tracking-[-0.02em] text-[#0a1020]">
            Modern, role-aware HR workflows without visual noise.
          </h1>
          <p className="mb-6 mt-4 max-w-[48ch] text-[14.5px] leading-[1.6] text-muted-foreground">
            Designed for HR teams, department managers, team leads, and
            employees with focused dashboards, clear approvals, and a
            professional operating rhythm.
          </p>

          <div className="flex flex-col gap-2.5">
            {FEATURES.map((f) => {
              const Icon = f.icon;
              return (
                <div
                  key={f.title}
                  className="flex items-start gap-4 rounded-[14px] border border-border bg-card px-5 py-[14px]"
                >
                  <div
                    className="grid h-[38px] w-[38px] shrink-0 place-items-center rounded-[10px]"
                    style={{ background: f.tint.bg, color: f.tint.fg }}
                  >
                    <Icon className="h-5 w-5" strokeWidth={1.8} />
                  </div>
                  <div>
                    {f.role ? (
                      <h4 className="mb-2 text-[11px] font-bold uppercase tracking-[0.16em] text-muted-foreground">
                        {f.title}
                      </h4>
                    ) : (
                      <h4 className="mb-1 text-[15.5px] font-bold tracking-[-0.005em] text-foreground">
                        {f.title}
                      </h4>
                    )}
                    <p
                      className={cn(
                        "text-[13.5px] leading-[1.5]",
                        f.role ? "text-[#48536a]" : "text-muted-foreground",
                      )}
                    >
                      {f.body}
                    </p>
                  </div>
                </div>
              );
            })}
          </div>
        </section>

        <section className="rounded-[22px] border border-border bg-card p-8 lg:px-10 lg:py-8">
          <div className="mb-5 inline-flex items-center gap-2.5 rounded-full border border-border bg-card py-[6px] pl-[6px] pr-[18px] text-[13.5px] font-semibold text-foreground">
            <span className="grid h-8 w-8 place-items-center rounded-full bg-primary text-primary-foreground">
              <Building2 className="h-4 w-4" strokeWidth={2} />
            </span>
            <span>Secure sign in</span>
          </div>

          <div className="mb-2 text-[11px] font-semibold uppercase tracking-[0.16em] text-muted-foreground">
            Welcome back
          </div>
          <h2 className="mb-3 max-w-[14ch] text-[32px] font-extrabold leading-[1.12] tracking-[-0.02em] text-[#0a1020]">
            Sign in to your workspace
          </h2>
          <p className="mb-5 max-w-[46ch] text-[14px] leading-[1.55] text-muted-foreground">
            Access the correct dashboard for your role and continue your daily
            workflow without unnecessary steps.
          </p>

          <form
            onSubmit={handleSubmit(onSubmit)}
            className="rounded-2xl border border-border bg-card p-[22px]"
            noValidate
          >
            <div className="mb-4 flex flex-col gap-2">
              <label
                htmlFor="email"
                className="text-[13.5px] font-semibold text-[#1f2a3a]"
              >
                Email address
              </label>
              <input
                id="email"
                type="email"
                placeholder="name@company.com"
                autoComplete="email"
                className="h-11 rounded-[10px] border border-border bg-[#f4f6fb] px-3.5 text-[15px] text-foreground placeholder:text-[#9aa3b4] outline-none transition-colors focus:border-primary focus:bg-card focus:shadow-[0_0_0_4px_rgba(41,82,236,0.18)]"
                {...register("email")}
              />
              {errors.email && (
                <p className="text-xs text-destructive">
                  {errors.email.message}
                </p>
              )}
            </div>

            <div className="mb-4 flex flex-col gap-2">
              <label
                htmlFor="password"
                className="text-[13.5px] font-semibold text-[#1f2a3a]"
              >
                Password
              </label>
              <input
                id="password"
                type="password"
                placeholder="Enter your password"
                autoComplete="current-password"
                className="h-11 rounded-[10px] border border-border bg-[#f4f6fb] px-3.5 text-[15px] text-foreground placeholder:text-[#9aa3b4] outline-none transition-colors focus:border-primary focus:bg-card focus:shadow-[0_0_0_4px_rgba(41,82,236,0.18)]"
                {...register("password")}
              />
              {errors.password && (
                <p className="text-xs text-destructive">
                  {errors.password.message}
                </p>
              )}
            </div>

            {submitError && (
              <div className="mb-4 rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-xs text-destructive">
                {submitError}
              </div>
            )}

            <button
              type="submit"
              disabled={isSubmitting}
              className="flex h-12 w-full items-center justify-center gap-2.5 rounded-xl bg-primary text-[15px] font-bold text-primary-foreground tracking-[0.005em] transition-colors hover:bg-[#1f44d6] active:translate-y-px disabled:opacity-60"
            >
              <span>{isSubmitting ? "Signing in…" : "Continue"}</span>
              <ArrowRight className="h-[18px] w-[18px]" strokeWidth={2.2} />
            </button>
          </form>

          <div className="mb-3 mt-5 flex items-center justify-between">
            <span className="text-[11px] font-semibold uppercase tracking-[0.16em] text-muted-foreground">
              Demo access
            </span>
            <span className="text-[12.5px] text-muted-foreground">
              Tap any account to autofill
            </span>
          </div>
          <div className="grid grid-cols-2 gap-3">
            {DEMO_ACCOUNTS.map((acc) => {
              const Icon = acc.icon;
              return (
                <button
                  key={acc.email}
                  type="button"
                  onClick={() => handleDemoFill(acc.email)}
                  className="flex items-center gap-3 rounded-xl border border-border bg-card px-3.5 py-3 text-left transition-all hover:border-[#cfd6e4] hover:shadow-[0_1px_2px_rgba(15,23,42,0.04)]"
                >
                  <span
                    className="grid h-9 w-9 shrink-0 place-items-center rounded-[10px]"
                    style={{ background: acc.tint.bg, color: acc.tint.fg }}
                  >
                    <Icon className="h-[18px] w-[18px]" strokeWidth={1.8} />
                  </span>
                  <span className="flex min-w-0 flex-col leading-[1.25]">
                    <strong className="text-[13.5px] font-bold text-foreground">
                      {acc.role}
                    </strong>
                    <span className="truncate text-[12px] text-muted-foreground">
                      {acc.email}
                    </span>
                  </span>
                </button>
              );
            })}
          </div>
        </section>
      </div>
    </div>
  );
}
