import { createFileRoute } from "@tanstack/react-router"
import { GlassesOrderDetailPage } from "@/features/optical/components/GlassesOrderDetailPage"

export const Route = createFileRoute("/_authenticated/optical/orders/$orderId")({
  component: GlassesOrderDetailRoute,
})

function GlassesOrderDetailRoute() {
  const { orderId } = Route.useParams()
  return <GlassesOrderDetailPage orderId={orderId} />
}
