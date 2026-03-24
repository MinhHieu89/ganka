import { createFileRoute } from "@tanstack/react-router"
import { VisitDetailPage } from "@/features/clinical/components/VisitDetailPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/visits/$visitId")({
  beforeLoad: () => requirePermission("Clinical.View"),
  component: VisitDetailRoute,
})

function VisitDetailRoute() {
  const { visitId } = Route.useParams()
  return <VisitDetailPage visitId={visitId} />
}
