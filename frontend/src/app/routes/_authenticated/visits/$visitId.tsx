import { createFileRoute } from "@tanstack/react-router"
import { VisitDetailPage } from "@/features/clinical/components/VisitDetailPage"

export const Route = createFileRoute("/_authenticated/visits/$visitId")({
  component: VisitDetailRoute,
})

function VisitDetailRoute() {
  const { visitId } = Route.useParams()
  return <VisitDetailPage visitId={visitId} />
}
