import { createFileRoute } from "@tanstack/react-router"
import { StocktakingPage } from "@/features/optical/components/StocktakingPage"

export const Route = createFileRoute("/_authenticated/optical/stocktaking")({
  component: StocktakingPage,
})
