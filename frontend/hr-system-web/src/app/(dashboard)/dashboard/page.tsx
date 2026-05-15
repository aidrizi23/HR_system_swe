"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/shared/page-header";
import { Button } from "@/components/ui/button";
import { type AuthUser, getStoredUser } from "@/lib/auth";

function greetingFor(hour: number): string {
  if (hour < 12) return "Good morning";
  if (hour < 18) return "Good afternoon";
  return "Good evening";
}

export default function DashboardPage() {
  const [user, setUser] = useState<AuthUser | null>(null);
  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setUser(getStoredUser());
  }, []);

  const firstName = user?.name?.split(/\s+/)[0] ?? user?.email?.split("@")[0] ?? "there";
  const title = `${greetingFor(new Date().getHours())}, ${firstName}`;

  return (
    <div className="space-y-6">
      <PageHeader
        title={title}
        subtitle="Welcome back. Approvals, activity, and team status will appear here."
        actions={
          <>
            <Button variant="outline" size="sm">
              Add Employee
            </Button>
            <Button variant="outline" size="sm">
              Create Announcement
            </Button>
            <Button size="sm">Run Report</Button>
          </>
        }
      />

      <div className="rounded-2xl border border-border bg-card p-12 text-center shadow-[0_1px_2px_rgba(15,23,42,0.04)]">
        <p className="text-sm text-muted-foreground">
          Dashboard widgets land in a later branch.
        </p>
      </div>
    </div>
  );
}
