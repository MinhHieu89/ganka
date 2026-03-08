import { createFileRoute } from "@tanstack/react-router"
import { TreatmentsPage } from "@/features/treatment/components/TreatmentsPage"

export const Route = createFileRoute("/_authenticated/treatments/")({
  component: TreatmentsPage,
})
