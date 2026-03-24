import { createFileRoute } from "@tanstack/react-router"
import { RoleManagementPage } from "@/features/admin/components/RoleManagementPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/admin/roles")({
  beforeLoad: () => requirePermission("Auth.View"),
  component: RoleManagementPage,
})
