import { createFileRoute } from "@tanstack/react-router"
import { PublicBookingPage } from "@/features/booking/components/PublicBookingPage"

export const Route = createFileRoute("/book/")({
  component: PublicBookingPage,
})
