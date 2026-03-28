import { createFileRoute } from "@tanstack/react-router"
import { z } from "zod"
import { requirePermission } from "@/shared/utils/permission-guard"
import { PatientIntakeForm } from "@/features/receptionist/components/intake/PatientIntakeForm"
import { usePatientById } from "@/features/patient/api/patient-api"
import { useAppointmentById } from "@/features/receptionist/api/receptionist-api"
import { useVisitById } from "@/features/clinical/api/clinical-api"
import { Skeleton } from "@/shared/components/Skeleton"
import { toLocalDateString } from "@/shared/lib/format-date"

const searchSchema = z.object({
  patientId: z.string().optional(),
  appointmentId: z.string().optional(),
  visitId: z.string().optional(),
})

export const Route = createFileRoute("/_authenticated/patients/intake")({
  beforeLoad: () => requirePermission("Patient.Create"),
  component: PatientIntakePage,
  validateSearch: searchSchema,
})

function PatientIntakePage() {
  const { patientId, appointmentId, visitId } = Route.useSearch()

  if (patientId) {
    return <EditModeIntake patientId={patientId} visitId={visitId} />
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

function EditModeIntake({ patientId, visitId }: { patientId: string; visitId?: string }) {
  const { data: patient, isLoading } = usePatientById(patientId)
  const { data: visit, isLoading: visitLoading } = useVisitById(visitId)

  if (isLoading || visitLoading) {
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

  const allergiesStr = patient.allergies?.length
    ? patient.allergies.map((a: { name: string }) => a.name).join(", ")
    : ""

  // Map normalized gender string back to form select value (number string)
  const genderFormMap: Record<string, string> = {
    Male: "0", Female: "1", Other: "2",
  }
  // Map backend enum strings to frontend form values
  const workEnvMap: Record<string, string> = {
    Office: "office", Outdoor: "outdoor", Factory: "mixed", Mixed: "mixed", Other: "other",
  }
  const contactLensMap: Record<string, string> = {
    None: "none", Soft: "soft", Daily: "soft", Rgp: "rgp", Occasional: "rgp",
    OrthoK: "ortho_k", Ortho_K: "ortho_k", Other: "other",
  }

  const defaultValues = {
    fullName: patient.fullName ?? "",
    phone: patient.phone ?? "",
    dateOfBirth: patient.dateOfBirth
      ? toLocalDateString(new Date(patient.dateOfBirth))
      : "",
    gender: patient.gender ? (genderFormMap[patient.gender] ?? "") : "",
    address: patient.address ?? "",
    cccd: patient.cccd ?? "",
    email: patient.email ?? "",
    occupation: patient.occupation ?? "",
    allergies: allergiesStr,
    ocularHistory: patient.ocularHistory ?? "",
    systemicHistory: patient.systemicHistory ?? "",
    currentMedications: patient.currentMedications ?? "",
    screenTimeHours: patient.screenTimeHours ?? undefined,
    workEnvironment: patient.workEnvironment ? workEnvMap[patient.workEnvironment] : undefined,
    contactLensUsage: patient.contactLensUsage ? contactLensMap[patient.contactLensUsage] : undefined,
    lifestyleNotes: patient.lifestyleNotes ?? "",
    reason: visit?.reason ?? "",
  }

  return (
    <PatientIntakeForm
      patientId={patientId}
      visitId={visitId}
      defaultValues={defaultValues}
      mode="edit"
    />
  )
}
