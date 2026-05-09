import { PageHeader } from "@/components/shared/page-header";
import { Button } from "@/components/ui/button";

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Good afternoon, admin"
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
