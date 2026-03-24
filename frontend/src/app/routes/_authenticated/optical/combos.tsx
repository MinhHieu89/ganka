import { createFileRoute } from "@tanstack/react-router"
import { ComboPackagePage } from "@/features/optical/components/ComboPackagePage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/optical/combos")({
  beforeLoad: () => requirePermission("Optical.View"),
  component: ComboPackagePage,
})
