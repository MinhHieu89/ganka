// -- Types matching backend Scheduling.Contracts.Dtos for Receptionist --

export type ReceptionistStatus = "not_arrived" | "waiting" | "examining" | "completed" | "cancelled"

export type ReceptionistSource = "appointment" | "walkin"

export interface ReceptionistDashboardRow {
  id: string
  appointmentId: string | null
  visitId: string | null
  patientId: string | null
  patientName: string
  patientCode: string | null
  birthYear: number | null
  appointmentTime: string | null
  source: ReceptionistSource
  reason: string | null
  status: ReceptionistStatus
  checkedInAt: string | null
  isGuestBooking: boolean
  guestPhone: string | null
}

export interface ReceptionistKpi {
  todayAppointments: number
  notArrived: number
  waiting: number
  examining: number
  completed: number
  cancelled: number
}

export interface AvailableSlot {
  doctorId: string
  doctorName: string
  startTime: string
  endTime: string
  durationMinutes: number
}

export interface DashboardFilters {
  status?: ReceptionistStatus
  search?: string
  page: number
  pageSize: number
}
