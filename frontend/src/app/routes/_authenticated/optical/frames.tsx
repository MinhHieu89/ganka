import { createFileRoute } from "@tanstack/react-router"
import { FrameCatalogPage } from "@/features/optical/components/FrameCatalogPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/optical/frames")({
  beforeLoad: () => requirePermission("Optical.View"),
  component: FrameCatalogPage,
})
