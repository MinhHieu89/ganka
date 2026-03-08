import { createFileRoute } from "@tanstack/react-router"
import { ComboPackagePage } from "@/features/optical/components/ComboPackagePage"

export const Route = createFileRoute("/_authenticated/optical/combos")({
  component: ComboPackagePage,
})
