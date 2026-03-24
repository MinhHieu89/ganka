import { createFileRoute } from "@tanstack/react-router"
import { DrugCatalogPage } from "@/features/pharmacy/components/DrugCatalogPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/pharmacy/drug-catalog")({
  beforeLoad: () => requirePermission("Pharmacy.View"),
  component: DrugCatalogPage,
})
