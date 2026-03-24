import { createFileRoute, Outlet } from "@tanstack/react-router"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/optical/orders")({
  beforeLoad: () => requirePermission("Optical.View"),
  component: () => <Outlet />,
})
