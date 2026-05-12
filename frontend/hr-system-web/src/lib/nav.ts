import {
  Building2,
  CalendarDays,
  ClipboardList,
  Clock,
  DollarSign,
  FileText,
  LayoutDashboard,
  Megaphone,
  PartyPopper,
  Timer,
  UserPlus,
  Users,
  UsersRound,
  type LucideIcon,
} from "lucide-react";

export type NavGroup =
  | "OVERVIEW"
  | "WORKFORCE"
  | "ATTENDANCE"
  | "OPERATIONS"
  | "DEVELOPMENT";

export type NavItem = {
  label: string;
  href: string;
  icon: LucideIcon;
  group: NavGroup;
  eyebrow: string;
  title: string;
  subtitle: string;
};

export const NAV_ITEMS: NavItem[] = [
  {
    label: "Dashboard",
    href: "/dashboard",
    icon: LayoutDashboard,
    group: "OVERVIEW",
    eyebrow: "OVERVIEW",
    title: "Dashboard",
    subtitle: "Role-based overview of current work, approvals, and activity.",
  },
  {
    label: "Employees",
    href: "/employees",
    icon: Users,
    group: "WORKFORCE",
    eyebrow: "WORKFORCE",
    title: "Employees",
    subtitle: "Manage employee records, structure, and workforce changes.",
  },
  {
    label: "Departments",
    href: "/departments",
    icon: Building2,
    group: "WORKFORCE",
    eyebrow: "WORKFORCE",
    title: "Departments",
    subtitle: "Maintain departmental ownership, teams, and reporting lines.",
  },
  {
    label: "Teams",
    href: "/teams",
    icon: UsersRound,
    group: "WORKFORCE",
    eyebrow: "WORKFORCE",
    title: "Teams",
    subtitle: "Manage teams within departments.",
  },
  {
    label: "Time Tracking",
    href: "/time-tracking",
    icon: Clock,
    group: "ATTENDANCE",
    eyebrow: "ATTENDANCE",
    title: "Time Tracking",
    subtitle: "Review attendance, working hours, and daily activity.",
  },
  {
    label: "Leave",
    href: "/leave",
    icon: CalendarDays,
    group: "ATTENDANCE",
    eyebrow: "ATTENDANCE",
    title: "Leave Management",
    subtitle: "Track balances, requests, approvals, and leave history.",
  },
  {
    label: "Overtime",
    href: "/overtime",
    icon: Timer,
    group: "ATTENDANCE",
    eyebrow: "ATTENDANCE",
    title: "Overtime",
    subtitle: "Review overtime records, approvals, and detected exceptions.",
  },
  {
    label: "Salary",
    href: "/salary",
    icon: DollarSign,
    group: "OPERATIONS",
    eyebrow: "OPERATIONS",
    title: "Salary",
    subtitle: "Access payroll-related information and compensation records.",
  },
  {
    label: "Tasks",
    href: "/tasks",
    icon: ClipboardList,
    group: "OPERATIONS",
    eyebrow: "OPERATIONS",
    title: "Tasks",
    subtitle: "Track work assigned across your teams.",
  },
  {
    label: "Documents",
    href: "/documents",
    icon: FileText,
    group: "OPERATIONS",
    eyebrow: "OPERATIONS",
    title: "Documents",
    subtitle: "Manage employee documents, track expirations, and organize categories.",
  },
  {
    label: "Announcements",
    href: "/announcements",
    icon: Megaphone,
    group: "OPERATIONS",
    eyebrow: "OPERATIONS",
    title: "Announcements",
    subtitle: "Company announcements and updates.",
  },
  {
    label: "Holidays",
    href: "/holidays",
    icon: PartyPopper,
    group: "OPERATIONS",
    eyebrow: "OPERATIONS",
    title: "Holidays",
    subtitle: "View company holidays and public holidays.",
  },
  {
    label: "Onboarding",
    href: "/onboarding",
    icon: UserPlus,
    group: "DEVELOPMENT",
    eyebrow: "DEVELOPMENT",
    title: "Onboarding",
    subtitle: "Manage employee onboarding templates and checklists.",
  },
];

export const NAV_GROUP_ORDER: NavGroup[] = [
  "OVERVIEW",
  "WORKFORCE",
  "ATTENDANCE",
  "OPERATIONS",
  "DEVELOPMENT",
];

export const NAV_GROUPS = NAV_GROUP_ORDER.map((name) => ({
  name,
  items: NAV_ITEMS.filter((i) => i.group === name),
}));

export function findNavByPath(pathname: string): NavItem | undefined {
  return NAV_ITEMS.find(
    (i) => pathname === i.href || pathname.startsWith(`${i.href}/`),
  );
}
