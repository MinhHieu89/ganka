import { createFileRoute } from "@tanstack/react-router"
import { GlassesOrderDetailPage } from "@/features/optical/components/GlassesOrderDetailPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/optical/orders/$orderId")({
  beforeLoad: () => requirePermission("Optical.View"),
  component: GlassesOrderDetailRoute,
})

function GlassesOrderDetailRoute() {
  const { orderId } = Route.useParams()
  return <GlassesOrderDetailPage orderId={orderId} />
}
