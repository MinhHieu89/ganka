import { createFileRoute } from "@tanstack/react-router"
import { WarrantyClaimsPage } from "@/features/optical/components/WarrantyClaimsPage"

export const Route = createFileRoute("/_authenticated/optical/warranty")({
  component: WarrantyClaimsPage,
})
