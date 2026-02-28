import { createFileRoute, redirect } from "@tanstack/react-router"
import { useAuthStore } from "@/shared/stores/authStore"
import { AuditLogPage } from "@/features/audit/components/AuditLogPage"
import { toast } from "sonner"

export const Route = createFileRoute("/_authenticated/admin/audit-logs")({
  beforeLoad: () => {
    const { user } = useAuthStore.getState()
    const hasPermission =
      user?.permissions?.includes("Audit.View") ||
      user?.permissions?.includes("Admin")

    if (!hasPermission) {
      toast.error("You do not have permission to view audit logs")
      throw redirect({ to: "/dashboard" })
    }
  },
  component: AuditLogPage,
})
