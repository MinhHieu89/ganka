import { useMemo, useState, useCallback } from "react"
import { addDays, startOfWeek, endOfWeek, formatISO } from "date-fns"
import type { EventInput } from "@fullcalendar/core"
import {
  useAppointmentsByDoctor,
  useClinicSchedule,
  type AppointmentDto,
  type ClinicScheduleDto,
} from "@/features/scheduling/api/scheduling-api"

const APPOINTMENT_COLORS: Record<string, { backgroundColor: string; borderColor: string }> = {
  NewPatient: { backgroundColor: "#3b82f6", borderColor: "#2563eb" },
  FollowUp: { backgroundColor: "#22c55e", borderColor: "#16a34a" },
  Treatment: { backgroundColor: "#f97316", borderColor: "#ea580c" },
  OrthoK: { backgroundColor: "#a855f7", borderColor: "#9333ea" },
}

// Appointment status enum matching backend
export const AppointmentStatus = {
  Pending: 0,
  Confirmed: 1,
  Cancelled: 2,
  Completed: 3,
} as const

// Cancellation reason enum matching backend
export const CancellationReason = {
  PatientNoShow: 0,
  PatientRequest: 1,
  DoctorUnavailable: 2,
  Other: 3,
} as const

function getStatusBorderStyle(status: number): string {
  switch (status) {
    case AppointmentStatus.Pending:
      return "2px dashed"
    case AppointmentStatus.Cancelled:
      return "2px solid #ef4444"
    case AppointmentStatus.Completed:
      return "2px solid #6b7280"
    default:
      return "2px solid"
  }
}

function appointmentToEvent(apt: AppointmentDto): EventInput {
  const typeKey = apt.appointmentTypeName.replace(/[\s-]/g, "")
  const colors = APPOINTMENT_COLORS[typeKey] ?? {
    backgroundColor: apt.color || "#6b7280",
    borderColor: apt.color || "#4b5563",
  }

  return {
    id: apt.id,
    title: apt.patientName,
    start: apt.startTime,
    end: apt.endTime,
    ...colors,
    borderColor: apt.status === AppointmentStatus.Cancelled ? "#ef4444" : colors.borderColor,
    textColor: "#ffffff",
    editable: apt.status !== AppointmentStatus.Cancelled && apt.status !== AppointmentStatus.Completed,
    extendedProps: {
      appointmentId: apt.id,
      patientId: apt.patientId,
      patientName: apt.patientName,
      doctorId: apt.doctorId,
      doctorName: apt.doctorName,
      appointmentTypeId: apt.appointmentTypeId,
      appointmentTypeName: apt.appointmentTypeName,
      appointmentTypeNameVi: apt.appointmentTypeNameVi,
      status: apt.status,
      notes: apt.notes,
      color: apt.color,
    },
    display: apt.status === AppointmentStatus.Cancelled ? "none" : "auto",
  }
}

/**
 * Convert clinic schedule DTOs to FullCalendar businessHours format.
 * FullCalendar uses dayOfWeek: 0=Sunday through 6=Saturday.
 */
export function scheduleToBusinessHours(
  schedule: ClinicScheduleDto[] | undefined,
): Array<{
  daysOfWeek: number[]
  startTime: string
  endTime: string
}> {
  if (!schedule) return []

  return schedule
    .filter((s) => s.isOpen && s.openTime && s.closeTime)
    .map((s) => ({
      daysOfWeek: [s.dayOfWeek],
      startTime: s.openTime!,
      endTime: s.closeTime!,
    }))
}

export function useAppointmentsForCalendar(doctorId: string | undefined) {
  const [dateRange, setDateRange] = useState(() => {
    const now = new Date()
    const start = startOfWeek(now, { weekStartsOn: 1 })
    const end = endOfWeek(now, { weekStartsOn: 1 })
    return {
      dateFrom: formatISO(start, { representation: "date" }),
      dateTo: formatISO(addDays(end, 1), { representation: "date" }),
    }
  })

  const { data: appointments, isLoading: isLoadingAppointments } =
    useAppointmentsByDoctor(doctorId, dateRange.dateFrom, dateRange.dateTo)

  const { data: clinicSchedule, isLoading: isLoadingSchedule } = useClinicSchedule()

  const events = useMemo<EventInput[]>(
    () => (appointments ?? []).map(appointmentToEvent),
    [appointments],
  )

  const businessHours = useMemo(
    () => scheduleToBusinessHours(clinicSchedule),
    [clinicSchedule],
  )

  const handleDatesSet = useCallback(
    (info: { startStr: string; endStr: string }) => {
      setDateRange({
        dateFrom: info.startStr.split("T")[0],
        dateTo: info.endStr.split("T")[0],
      })
    },
    [],
  )

  return {
    events,
    businessHours,
    clinicSchedule,
    isLoading: isLoadingAppointments || isLoadingSchedule,
    handleDatesSet,
  }
}
