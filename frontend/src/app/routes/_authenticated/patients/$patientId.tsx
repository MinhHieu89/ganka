import { createFileRoute } from "@tanstack/react-router"
import { z } from "zod"
import { PatientProfilePage } from "@/features/patient/components/PatientProfilePage"
import { requirePermission } from "@/shared/utils/permission-guard"

const searchSchema = z.object({
  tab: z.string().optional(),
})

export const Route = createFileRoute("/_authenticated/patients/$patientId")({
  beforeLoad: () => requirePermission("Patient.View"),
  component: PatientProfileRoute,
  validateSearch: searchSchema,
})

function PatientProfileRoute() {
  const { patientId } = Route.useParams()
  const { tab } = Route.useSearch()
  return <PatientProfilePage patientId={patientId} initialTab={tab} />
}
