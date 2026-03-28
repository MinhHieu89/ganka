import { createFileRoute } from "@tanstack/react-router"
import { z } from "zod"
import { requirePermission } from "@/shared/utils/permission-guard"
import { PatientIntakeForm } from "@/features/receptionist/components/intake/PatientIntakeForm"
import { usePatientById } from "@/features/patient/api/patient-api"
import { useAppointmentById } from "@/features/receptionist/api/receptionist-api"
import { Skeleton } from "@/shared/components/Skeleton"
import { toLocalDateString } from "@/shared/lib/format-date"

const searchSchema = z.object({
  patientId: z.string().optional(),
  appointmentId: z.string().optional(),
})

export const Route = createFileRoute("/_authenticated/patients/intake")({
  beforeLoad: () => requirePermission("Patient.Create"),
  component: PatientIntakePage,
  validateSearch: searchSchema,
})

function PatientIntakePage() {
  const { patientId, appointmentId } = Route.useSearch()

  if (patientId) {
    return <EditModeIntake patientId={patientId} />
  }

  if (appointmentId) {
    return <GuestIntake appointmentId={appointmentId} />
  }

  return <PatientIntakeForm mode="create" />
}

function GuestIntake({ appointmentId }: { appointmentId: string }) {
  const { data: appointment, isLoading } = useAppointmentById(appointmentId)

  if (isLoading) {
    return (
      <div className="flex flex-col gap-6 p-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  const defaultValues = appointment
    ? {
        fullName: appointment.guestName ?? appointment.patientName ?? "",
        phone: appointment.guestPhone ?? "",
        reason: appointment.guestReason ?? appointment.notes ?? "",
      }
    : undefined

  return (
    <PatientIntakeForm
      mode="create"
      defaultValues={defaultValues}
      appointmentId={appointmentId}
    />
  )
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

  const defaultValues = {
    fullName: patient.fullName ?? "",
    phone: patient.phone ?? "",
    dateOfBirth: patient.dateOfBirth
      ? toLocalDateString(new Date(patient.dateOfBirth))
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
