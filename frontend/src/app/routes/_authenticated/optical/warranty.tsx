import { createFileRoute } from "@tanstack/react-router"
import { WarrantyClaimsPage } from "@/features/optical/components/WarrantyClaimsPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/optical/warranty")({
  beforeLoad: () => requirePermission("Optical.View"),
  component: WarrantyClaimsPage,
})
