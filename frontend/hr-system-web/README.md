# HR System — Web

Frontend for the HR System. Next.js (App Router) + TypeScript + Tailwind.

## Run

```bash
npm install
npm run dev
```

Dev server runs on `http://localhost:3000`. The backend API is expected at `http://localhost:5056/api`.

## Structure

```
src/
  app/                  routes (App Router)
    (auth)/             unauthenticated screens (login)
    (dashboard)/        authenticated shell + feature pages
  components/
    ui/                 shadcn primitives
    shared/             cross-page shell pieces (sidebar, topbar)
  lib/
    api/                axios client + per-domain endpoint modules
    auth.ts             token storage helpers
  types/                shared TypeScript types
```
