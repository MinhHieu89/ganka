import { createFileRoute } from "@tanstack/react-router"
import { UserManagementPage } from "@/features/admin/components/UserManagementPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/admin/users")({
  beforeLoad: () => requirePermission("Auth.View"),
  component: UserManagementPage,
})
