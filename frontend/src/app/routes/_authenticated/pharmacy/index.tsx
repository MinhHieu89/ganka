import { createFileRoute } from "@tanstack/react-router"
import { DrugCatalogPage } from "@/features/pharmacy/components/DrugCatalogPage"

export const Route = createFileRoute("/_authenticated/pharmacy/")({
  component: DrugCatalogPage,
})
