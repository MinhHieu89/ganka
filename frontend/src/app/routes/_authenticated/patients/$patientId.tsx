import { createFileRoute } from "@tanstack/react-router"
import { z } from "zod"
import { PatientProfilePage } from "@/features/patient/components/PatientProfilePage"

const searchSchema = z.object({
  tab: z.string().optional(),
})

export const Route = createFileRoute("/_authenticated/patients/$patientId")({
  component: PatientProfileRoute,
  validateSearch: searchSchema,
})

function PatientProfileRoute() {
  const { patientId } = Route.useParams()
  const { tab } = Route.useSearch()
  return <PatientProfilePage patientId={patientId} initialTab={tab} />
}
