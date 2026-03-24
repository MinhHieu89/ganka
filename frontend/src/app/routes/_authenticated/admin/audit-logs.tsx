import { createFileRoute } from "@tanstack/react-router"
import { AuditLogPage } from "@/features/audit/components/AuditLogPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/admin/audit-logs")({
  beforeLoad: () => requirePermission("Audit.View"),
  component: AuditLogPage,
})
