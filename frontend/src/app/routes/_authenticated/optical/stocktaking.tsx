import { createFileRoute } from "@tanstack/react-router"
import { StocktakingPage } from "@/features/optical/components/StocktakingPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/optical/stocktaking")({
  beforeLoad: () => requirePermission("Optical.View"),
  component: StocktakingPage,
})
