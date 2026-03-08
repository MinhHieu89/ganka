import { createFileRoute } from "@tanstack/react-router"
import { TreatmentPackageDetail } from "@/features/treatment/components/TreatmentPackageDetail"

export const Route = createFileRoute("/_authenticated/treatments/$packageId")({
  component: TreatmentPackageDetailRoute,
})

function TreatmentPackageDetailRoute() {
  const { packageId } = Route.useParams()
  return <TreatmentPackageDetail packageId={packageId} />
}
