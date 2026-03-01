import { createFileRoute } from "@tanstack/react-router"
import { PatientProfilePage } from "@/features/patient/components/PatientProfilePage"

export const Route = createFileRoute("/_authenticated/patients/$patientId")({
  component: PatientProfileRoute,
})

function PatientProfileRoute() {
  const { patientId } = Route.useParams()
  return <PatientProfilePage patientId={patientId} />
}
