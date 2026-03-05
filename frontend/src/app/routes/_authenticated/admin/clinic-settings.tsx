import { createFileRoute } from "@tanstack/react-router"
import { ClinicSettingsPage } from "@/features/admin/components/ClinicSettingsPage"

export const Route = createFileRoute("/_authenticated/admin/clinic-settings")({
  component: ClinicSettingsPage,
})
