import { createFileRoute } from "@tanstack/react-router"
import { z } from "zod"
import { requirePermission } from "@/shared/utils/permission-guard"
import { NewAppointmentPage } from "@/features/receptionist/components/booking/NewAppointmentPage"

const searchSchema = z.object({
  patientId: z.string().optional(),
})

export const Route = createFileRoute("/_authenticated/appointments/new")({
  beforeLoad: () => requirePermission("Scheduling.Create"),
  component: NewAppointmentRoute,
  validateSearch: searchSchema,
})

function NewAppointmentRoute() {
  const { patientId } = Route.useSearch()
  return <NewAppointmentPage initialPatientId={patientId} />
}
