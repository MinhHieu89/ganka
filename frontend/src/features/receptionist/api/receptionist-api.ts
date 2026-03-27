import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"
import type {
  ReceptionistDashboardRow,
  ReceptionistKpi,
  AvailableSlot,
  DashboardFilters,
} from "@/features/receptionist/types/receptionist.types"

// -- Query key factory --

export const receptionistKeys = {
  all: ["receptionist"] as const,
  dashboard: (filters: DashboardFilters) =>
    [...receptionistKeys.all, "dashboard", filters] as const,
  kpi: () => [...receptionistKeys.all, "kpi"] as const,
  slots: (date: string, doctorId?: string) =>
    [...receptionistKeys.all, "slots", date, doctorId] as const,
}

// -- Queries --

export function useReceptionistDashboard(filters: DashboardFilters) {
  return useQuery({
    queryKey: receptionistKeys.dashboard(filters),
    queryFn: async (): Promise<ReceptionistDashboardRow[]> => {
      const { data, error } = await api.GET(
        "/api/scheduling/receptionist/dashboard" as never,
        {
          params: {
            query: {
              status: filters.status,
              search: filters.search,
              page: filters.page,
              pageSize: filters.pageSize,
            },
          },
        } as never,
      )
      if (error) throw new Error("Failed to fetch receptionist dashboard")
      // Backend returns Result<ReceptionistDashboardDto> with data property
      const result = data as { data?: ReceptionistDashboardRow[] } | ReceptionistDashboardRow[]
      return (Array.isArray(result) ? result : result?.data) ?? []
    },
    refetchInterval: 15_000,
  })
}

export function useReceptionistKpi() {
  return useQuery({
    queryKey: receptionistKeys.kpi(),
    queryFn: async (): Promise<ReceptionistKpi> => {
      const { data, error } = await api.GET(
        "/api/scheduling/receptionist/kpi" as never,
      )
      if (error) throw new Error("Failed to fetch receptionist KPI")
      const result = data as { data?: ReceptionistKpi } | ReceptionistKpi
      if (result && "todayAppointments" in result) return result
      return (result as { data?: ReceptionistKpi })?.data ?? {
        todayAppointments: 0,
        notArrived: 0,
        waiting: 0,
        examining: 0,
        completed: 0,
      }
    },
    refetchInterval: 30_000,
  })
}

export function useAvailableSlots(date: string, doctorId?: string) {
  return useQuery({
    queryKey: receptionistKeys.slots(date, doctorId),
    queryFn: async (): Promise<AvailableSlot[]> => {
      const { data, error } = await api.GET(
        "/api/scheduling/slots" as never,
        {
          params: {
            query: { date, doctorId },
          },
        } as never,
      )
      if (error) throw new Error("Failed to fetch available slots")
      const result = data as { data?: AvailableSlot[] } | AvailableSlot[]
      return (Array.isArray(result) ? result : result?.data) ?? []
    },
    enabled: !!date,
  })
}

// -- Mutations --

export function useCheckInMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (appointmentId: string) => {
      const { error, response } = await api.POST(
        `/api/scheduling/check-in/${appointmentId}` as never,
      )
      if (error || !response.ok) throw new Error("Failed to check in")
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
    },
  })
}

export function useBookGuestMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (command: {
      guestName: string
      guestPhone: string
      reason?: string
      doctorId?: string
      date: string
      startTime: string
    }) => {
      const { data, error, response } = await api.POST(
        "/api/scheduling/guest-booking" as never,
        { body: command } as never,
      )
      if (error || !response.ok) {
        const status = response.status
        if (status === 409) throw new Error("DOUBLE_BOOKING")
        throw new Error("Failed to book guest appointment")
      }
      return data as { id: string }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
    },
  })
}

export function useMarkNoShowMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (appointmentId: string) => {
      const { error, response } = await api.POST(
        `/api/scheduling/no-show/${appointmentId}` as never,
      )
      if (error || !response.ok) throw new Error("Failed to mark no-show")
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
    },
  })
}

export function useCreateWalkInVisitMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (command: {
      patientId: string
      reason?: string
    }) => {
      const { data, error, response } = await api.POST(
        "/api/clinical/walk-in" as never,
        { body: command } as never,
      )
      if (error || !response.ok) throw new Error("Failed to create walk-in visit")
      return data as { id: string }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
    },
  })
}

export function useCancelVisitMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ visitId, reason }: { visitId: string; reason: string }) => {
      const { error, response } = await api.POST(
        `/api/clinical/cancel-visit` as never,
        { body: { visitId, reason } } as never,
      )
      if (error || !response.ok) throw new Error("Failed to cancel visit")
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
    },
  })
}

export function useRegisterFromIntakeMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (command: Record<string, unknown>) => {
      const { data, error, response } = await api.POST(
        "/api/patients/intake" as never,
        { body: command } as never,
      )
      if (error || !response.ok) {
        const err = error as Record<string, unknown> | undefined
        if (err?.errors) throw new Error(JSON.stringify(err))
        throw new Error("Failed to register patient")
      }
      return data as { id: string }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
    },
  })
}

export function useUpdateFromIntakeMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ patientId, ...command }: Record<string, unknown> & { patientId: string }) => {
      const { error, response } = await api.PUT(
        `/api/patients/intake/${patientId}` as never,
        { body: command } as never,
      )
      if (error || !response.ok) {
        const err = error as Record<string, unknown> | undefined
        if (err?.errors) throw new Error(JSON.stringify(err))
        throw new Error("Failed to update patient intake")
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
    },
  })
}

export function useAdvanceStageMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ visitId, newStage }: { visitId: string; newStage: number }) => {
      const { error, response } = await api.PUT(
        `/api/clinical/${visitId}/stage` as never,
        { body: { visitId, newStage } } as never,
      )
      if (error || !response.ok) throw new Error("Failed to advance workflow stage")
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
    },
  })
}
