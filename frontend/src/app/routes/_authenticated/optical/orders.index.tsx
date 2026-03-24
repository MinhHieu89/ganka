import { createFileRoute } from "@tanstack/react-router"
import { GlassesOrdersPage } from "@/features/optical/components/GlassesOrdersPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/optical/orders/")({
  beforeLoad: () => requirePermission("Optical.View"),
  component: GlassesOrdersPage,
})
