// Live backend not yet built — owned by A's `admin-features` branch (see BRANCH_PLAN.md row 5).
// Swap this body to axios calls against /api/announcements when that branch lands.
import { mockAnnouncements } from "@/lib/mock/announcements";
import type { CreateAnnouncementDto } from "@/types";

export const apiAnnouncements = {
  list:      ()                          => mockAnnouncements.list(),
  create:    (dto: CreateAnnouncementDto) => mockAnnouncements.create(dto),
  markRead:  (id: number)                => mockAnnouncements.markRead(id),
  readCount: (id: number)                => mockAnnouncements.readCount(id),
};
