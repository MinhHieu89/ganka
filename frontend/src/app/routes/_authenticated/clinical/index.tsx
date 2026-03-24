import { createFileRoute } from "@tanstack/react-router"
import { WorkflowDashboard } from "@/features/clinical/components/WorkflowDashboard"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/clinical/")({
  beforeLoad: () => requirePermission("Clinical.View"),
  component: ClinicalWorkflowPage,
})

function ClinicalWorkflowPage() {
  return <WorkflowDashboard />
}
