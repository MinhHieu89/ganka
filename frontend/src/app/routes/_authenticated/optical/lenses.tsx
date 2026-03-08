import { createFileRoute } from "@tanstack/react-router"
import { LensCatalogPage } from "@/features/optical/components/LensCatalogPage"

export const Route = createFileRoute("/_authenticated/optical/lenses")({
  component: LensCatalogPage,
})
