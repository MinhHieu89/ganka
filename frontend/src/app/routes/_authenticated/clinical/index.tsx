import { createFileRoute } from "@tanstack/react-router"
import { WorkflowDashboard } from "@/features/clinical/components/WorkflowDashboard"

export const Route = createFileRoute("/_authenticated/clinical/")({
  component: ClinicalWorkflowPage,
})

function ClinicalWorkflowPage() {
  return <WorkflowDashboard />
}
