import { useQuery, useMutation } from "@tanstack/react-query"
import createClient from "openapi-fetch"

// Public API client WITHOUT auth middleware -- for patient self-booking
// Uses same base URL pattern as the authenticated client
const API_URL = (import.meta as never as { env: Record<string, string> }).env?.VITE_API_URL ?? "http://localhost:5255"

const publicApi = createClient({ baseUrl: API_URL })

// -- Types matching backend contracts --

export interface SubmitBookingCommand {
  patientName: string
  phone: string
  email?: string | null
  preferredDoctorId?: string | null
  preferredDate: string
  preferredTimeSlot?: string | null
  appointmentTypeId: string
  notes?: string | null
}

export interface BookingStatusResponse {
  referenceNumber: string
  status: number
  rejectionReason?: string | null
  appointmentDate?: string | null
}

export interface PublicAppointmentTypeDto {
  id: string
  name: string
  nameVi: string
  defaultDurationMinutes: number
  color: string
}

export interface PublicClinicScheduleDto {
  dayOfWeek: number
  isOpen: boolean
  openTime?: string | null
  closeTime?: string | null
}

export interface PublicDoctorDto {
  id: string
  fullName: string
}

// Booking status enum
export const BookingStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2,
} as const

// -- Query keys --

export const bookingKeys = {
  all: ["public-booking"] as const,
  status: (ref: string) => [...bookingKeys.all, "status", ref] as const,
  types: () => [...bookingKeys.all, "types"] as const,
  schedule: () => [...bookingKeys.all, "schedule"] as const,
  doctors: () => [...bookingKeys.all, "doctors"] as const,
}

// -- Queries (NO auth) --

export function useCheckBookingStatus(referenceNumber: string) {
  return useQuery({
    queryKey: bookingKeys.status(referenceNumber),
    queryFn: async (): Promise<BookingStatusResponse> => {
      const { data, error } = await publicApi.GET(
        "/api/public/booking/status/{referenceNumber}" as never,
        {
          params: { path: { referenceNumber } },
        } as never,
      )
      if (error) throw new Error("Failed to check booking status")
      return data as BookingStatusResponse
    },
    enabled: !!referenceNumber && referenceNumber.length > 0,
    retry: false,
  })
}

export function usePublicAppointmentTypes() {
  return useQuery({
    queryKey: bookingKeys.types(),
    queryFn: async (): Promise<PublicAppointmentTypeDto[]> => {
      const { data, error } = await publicApi.GET(
        "/api/public/booking/types" as never,
      )
      if (error) throw new Error("Failed to fetch appointment types")
      return (data as PublicAppointmentTypeDto[]) ?? []
    },
    staleTime: 1000 * 60 * 30,
  })
}

export function usePublicSchedule() {
  return useQuery({
    queryKey: bookingKeys.schedule(),
    queryFn: async (): Promise<PublicClinicScheduleDto[]> => {
      const { data, error } = await publicApi.GET(
        "/api/public/booking/schedule" as never,
      )
      if (error) throw new Error("Failed to fetch schedule")
      return (data as PublicClinicScheduleDto[]) ?? []
    },
    staleTime: 1000 * 60 * 60,
  })
}

// -- Mutations (NO auth) --

export function useSubmitBooking() {
  return useMutation({
    mutationFn: async (command: SubmitBookingCommand) => {
      const { data, error, response } = await publicApi.POST(
        "/api/public/booking" as never,
        { body: command } as never,
      )
      if (error || !response.ok) {
        const status = response.status
        if (status === 429) throw new Error("RATE_LIMITED")
        if (status === 400) {
          const err = error as Record<string, unknown> | undefined
          if (err?.errors) {
            throw new Error(JSON.stringify(err))
          }
          throw new Error("VALIDATION_ERROR")
        }
        throw new Error("Failed to submit booking")
      }
      return data as { referenceNumber: string }
    },
  })
}
