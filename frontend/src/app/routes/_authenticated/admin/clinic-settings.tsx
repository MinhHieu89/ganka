import { createFileRoute } from "@tanstack/react-router"
import { ClinicSettingsPage } from "@/features/admin/components/ClinicSettingsPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/admin/clinic-settings")({
  beforeLoad: () => requirePermission("Settings.View"),
  component: ClinicSettingsPage,
})
