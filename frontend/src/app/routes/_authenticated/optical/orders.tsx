import { createFileRoute } from "@tanstack/react-router"
import { GlassesOrdersPage } from "@/features/optical/components/GlassesOrdersPage"

export const Route = createFileRoute("/_authenticated/optical/orders")({
  component: GlassesOrdersPage,
})
