import { createFileRoute } from "@tanstack/react-router"
import { FrameCatalogPage } from "@/features/optical/components/FrameCatalogPage"

export const Route = createFileRoute("/_authenticated/optical/frames")({
  component: FrameCatalogPage,
})
