import { createFileRoute } from "@tanstack/react-router"
import { UserManagementPage } from "@/features/admin/components/UserManagementPage"

export const Route = createFileRoute("/_authenticated/admin/users")({
  component: UserManagementPage,
})
