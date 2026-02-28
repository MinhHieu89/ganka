import { createFileRoute } from "@tanstack/react-router"
import { RoleManagementPage } from "@/features/admin/components/RoleManagementPage"

export const Route = createFileRoute("/_authenticated/admin/roles")({
  component: RoleManagementPage,
})
