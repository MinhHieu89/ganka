import { createFileRoute } from "@tanstack/react-router"
import { PatientListPage } from "@/features/patient/components/PatientListPage"

export const Route = createFileRoute("/_authenticated/patients/")({
  component: PatientListPage,
})
