import { createFileRoute } from "@tanstack/react-router"
import { z } from "zod"
import { requirePermission } from "@/shared/utils/permission-guard"
import { PatientIntakeForm } from "@/features/receptionist/components/intake/PatientIntakeForm"
import { usePatientById } from "@/features/patient/api/patient-api"
import { Skeleton } from "@/shared/components/Skeleton"

const searchSchema = z.object({
  patientId: z.string().optional(),
})

export const Route = createFileRoute("/_authenticated/patients/intake")({
  beforeLoad: () => requirePermission("Patient.Create"),
  component: PatientIntakePage,
  validateSearch: searchSchema,
})

function PatientIntakePage() {
  const { patientId } = Route.useSearch()

  if (patientId) {
    return <EditModeIntake patientId={patientId} />
  }

  return <PatientIntakeForm mode="create" />
}

function EditModeIntake({ patientId }: { patientId: string }) {
  const { data: patient, isLoading } = usePatientById(patientId)

  if (isLoading) {
    return (
      <div className="flex flex-col gap-6 p-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48 w-full" />
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  if (!patient) {
    return (
      <div className="flex items-center justify-center p-12">
        <p className="text-muted-foreground">Khong tim thay benh nhan</p>
      </div>
    )
  }

  // Map patient data to intake form defaults
  const defaultValues = {
    fullName: patient.fullName ?? "",
    phone: patient.phone ?? "",
    dateOfBirth: patient.dateOfBirth
      ? new Date(patient.dateOfBirth).toISOString().split("T")[0]
      : "",
    gender: patient.gender ?? "",
    address: patient.address ?? "",
    cccd: patient.cccd ?? "",
    email: patient.email ?? "",
    occupation: patient.occupation ?? "",
  }

  return (
    <PatientIntakeForm
      patientId={patientId}
      defaultValues={defaultValues}
      mode="edit"
    />
  )
}
