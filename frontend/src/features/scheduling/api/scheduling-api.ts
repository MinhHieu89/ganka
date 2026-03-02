import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"

// -- Types matching backend Scheduling.Contracts.Dtos --

export interface AppointmentDto {
  id: string
  patientId: string
  patientName: string
  doctorId: string
  doctorName: string
  startTime: string
  endTime: string
  appointmentTypeId: string
  appointmentTypeName: string
  status: number
  color: string
  notes?: string | null
}

export interface AppointmentTypeDto {
  id: string
  name: string
  nameVi: string
  defaultDurationMinutes: number
  color: string
}

export interface ClinicScheduleDto {
  dayOfWeek: number
  isOpen: boolean
  openTime?: string | null
  closeTime?: string | null
}

export interface SelfBookingRequestDto {
  id: string
  patientName: string
  phone: string
  email?: string | null
  preferredDate: string
  preferredTimeSlot?: string | null
  appointmentTypeName: string
  status: number
  referenceNumber: string
  rejectionReason?: string | null
  createdAt: string
}

export interface BookAppointmentCommand {
  patientId: string
  patientName: string
  doctorId: string
  doctorName: string
  startTime: string
  appointmentTypeId: string
  notes?: string | null
}

export interface CancelAppointmentCommand {
  cancellationReason: number
  cancellationNote?: string | null
}

export interface RescheduleAppointmentCommand {
  newStartTime: string
}

export interface ApproveSelfBookingCommand {
  doctorId: string
  doctorName: string
  patientName: string
  startTime: string
}

export interface RejectSelfBookingCommand {
  reason: string
}

// -- Scheduling query keys --

export const schedulingKeys = {
  all: ["scheduling"] as const,
  appointments: () => [...schedulingKeys.all, "appointments"] as const,
  appointmentsByDoctor: (doctorId: string, dateFrom: string, dateTo: string) =>
    [...schedulingKeys.appointments(), "by-doctor", doctorId, dateFrom, dateTo] as const,
  appointmentsByPatient: (patientId: string) =>
    [...schedulingKeys.appointments(), "by-patient", patientId] as const,
  types: () => [...schedulingKeys.all, "types"] as const,
  schedule: () => [...schedulingKeys.all, "schedule"] as const,
  pendingSelfBookings: () => [...schedulingKeys.all, "self-bookings", "pending"] as const,
}

// -- Queries --

export function useAppointmentsByDoctor(
  doctorId: string | undefined,
  dateFrom: string,
  dateTo: string,
) {
  return useQuery({
    queryKey: schedulingKeys.appointmentsByDoctor(doctorId ?? "", dateFrom, dateTo),
    queryFn: async (): Promise<AppointmentDto[]> => {
      const { data, error } = await api.GET(
        "/api/appointments/by-doctor/{doctorId}" as never,
        {
          params: {
            path: { doctorId },
            query: { dateFrom, dateTo },
          },
        } as never,
      )
      if (error) throw new Error("Failed to fetch appointments")
      return (data as AppointmentDto[]) ?? []
    },
    enabled: !!doctorId,
    staleTime: 30_000,
  })
}

export function useAppointmentsByPatient(patientId: string | undefined) {
  return useQuery({
    queryKey: schedulingKeys.appointmentsByPatient(patientId ?? ""),
    queryFn: async (): Promise<AppointmentDto[]> => {
      const { data, error } = await api.GET(
        "/api/appointments/by-patient/{patientId}" as never,
        {
          params: { path: { patientId } },
        } as never,
      )
      if (error) throw new Error("Failed to fetch patient appointments")
      return (data as AppointmentDto[]) ?? []
    },
    enabled: !!patientId,
  })
}

export function useAppointmentTypes() {
  return useQuery({
    queryKey: schedulingKeys.types(),
    queryFn: async (): Promise<AppointmentTypeDto[]> => {
      const { data, error } = await api.GET("/api/appointments/types" as never)
      if (error) throw new Error("Failed to fetch appointment types")
      return (data as AppointmentTypeDto[]) ?? []
    },
    staleTime: 1000 * 60 * 30, // 30 minutes -- reference data rarely changes
  })
}

export function useClinicSchedule() {
  return useQuery({
    queryKey: schedulingKeys.schedule(),
    queryFn: async (): Promise<ClinicScheduleDto[]> => {
      const { data, error } = await api.GET("/api/appointments/schedule" as never)
      if (error) throw new Error("Failed to fetch clinic schedule")
      return (data as ClinicScheduleDto[]) ?? []
    },
    staleTime: 1000 * 60 * 60, // 1 hour -- schedule rarely changes
  })
}

export function usePendingSelfBookings() {
  return useQuery({
    queryKey: schedulingKeys.pendingSelfBookings(),
    queryFn: async (): Promise<SelfBookingRequestDto[]> => {
      const { data, error } = await api.GET(
        "/api/appointments/self-bookings/pending" as never,
      )
      if (error) throw new Error("Failed to fetch pending bookings")
      return (data as SelfBookingRequestDto[]) ?? []
    },
    refetchInterval: 60_000, // Poll every minute for new booking requests
  })
}

// -- Mutations --

export function useBookAppointment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (command: BookAppointmentCommand) => {
      const { data, error, response } = await api.POST(
        "/api/appointments" as never,
        { body: command } as never,
      )
      if (error || !response.ok) {
        const status = response.status
        if (status === 409) throw new Error("DOUBLE_BOOKING")
        if (status === 400) {
          const err = error as Record<string, unknown> | undefined
          if (err?.errors) {
            throw new Error(JSON.stringify(err))
          }
          throw new Error("VALIDATION_ERROR")
        }
        throw new Error("Failed to book appointment")
      }
      return data as { id: string }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: schedulingKeys.appointments() })
    },
  })
}

export function useCancelAppointment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({
      appointmentId,
      ...command
    }: CancelAppointmentCommand & { appointmentId: string }) => {
      const { error, response } = await api.PUT(
        "/api/appointments/{appointmentId}/cancel" as never,
        {
          params: { path: { appointmentId } },
          body: command,
        } as never,
      )
      if (error || !response.ok) throw new Error("Failed to cancel appointment")
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: schedulingKeys.appointments() })
    },
  })
}

export function useRescheduleAppointment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({
      appointmentId,
      newStartTime,
    }: {
      appointmentId: string
      newStartTime: string
    }) => {
      const { error, response } = await api.PUT(
        "/api/appointments/{appointmentId}/reschedule" as never,
        {
          params: { path: { appointmentId } },
          body: { newStartTime },
        } as never,
      )
      if (error || !response.ok) {
        const status = response.status
        if (status === 409) throw new Error("DOUBLE_BOOKING")
        if (status === 400) {
          const err = error as Record<string, unknown> | undefined
          if (err?.errors) {
            throw new Error(JSON.stringify(err))
          }
          throw new Error("VALIDATION_ERROR")
        }
        throw new Error("Failed to reschedule appointment")
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: schedulingKeys.appointments() })
    },
  })
}

export function useApproveSelfBooking() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({
      id,
      ...command
    }: ApproveSelfBookingCommand & { id: string }) => {
      const { error, response } = await api.POST(
        "/api/appointments/self-bookings/{id}/approve" as never,
        {
          params: { path: { id } },
          body: command,
        } as never,
      )
      if (error || !response.ok) {
        const status = response.status
        if (status === 409) throw new Error("DOUBLE_BOOKING")
        if (status === 400) {
          const err = error as Record<string, unknown> | undefined
          if (err?.errors) {
            throw new Error(JSON.stringify(err))
          }
        }
        throw new Error("Failed to approve booking")
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: schedulingKeys.pendingSelfBookings() })
      queryClient.invalidateQueries({ queryKey: schedulingKeys.appointments() })
    },
  })
}

export function useRejectSelfBooking() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({
      id,
      reason,
    }: {
      id: string
      reason: string
    }) => {
      const { error, response } = await api.POST(
        "/api/appointments/self-bookings/{id}/reject" as never,
        {
          params: { path: { id } },
          body: { reason },
        } as never,
      )
      if (error || !response.ok) throw new Error("Failed to reject booking")
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: schedulingKeys.pendingSelfBookings() })
    },
  })
}
