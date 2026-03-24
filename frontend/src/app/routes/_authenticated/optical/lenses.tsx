import { createFileRoute } from "@tanstack/react-router"
import { LensCatalogPage } from "@/features/optical/components/LensCatalogPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/optical/lenses")({
  beforeLoad: () => requirePermission("Optical.View"),
  component: LensCatalogPage,
})
