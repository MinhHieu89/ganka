import { createFileRoute } from "@tanstack/react-router"
import { PatientListPage } from "@/features/patient/components/PatientListPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/patients/")({
  beforeLoad: () => requirePermission("Patient.View"),
  component: PatientListPage,
})
