import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"
import type {
  TechnicianDashboardRow,
  TechnicianKpi,
  TechnicianDashboardFilters,
  TechnicianDashboardResponse,
} from "@/features/technician/types/technician.types"

// -- Query key factory --

export const technicianKeys = {
  all: ["technician"] as const,
  dashboard: (filters: TechnicianDashboardFilters) =>
    [...technicianKeys.all, "dashboard", filters] as const,
  kpi: () => [...technicianKeys.all, "kpi"] as const,
}

// -- Queries --

export function useTechnicianDashboard(filters: TechnicianDashboardFilters) {
  return useQuery({
    queryKey: technicianKeys.dashboard(filters),
    queryFn: async (): Promise<TechnicianDashboardRow[]> => {
      const { data, error } = await api.GET(
        "/api/clinical/technician/dashboard" as never,
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
      if (error) throw new Error("Failed to fetch technician dashboard")
      const result = data as TechnicianDashboardResponse | TechnicianDashboardRow[] | unknown
      if (Array.isArray(result)) return result
      if (result && typeof result === "object" && "items" in result) return (result as TechnicianDashboardResponse).items
      if (result && typeof result === "object" && "value" in result) {
        const wrapped = result as { value?: TechnicianDashboardResponse }
        if (wrapped.value?.items) return wrapped.value.items
      }
      return []
    },
    refetchInterval: 15_000, // per D-17
  })
}

export function useTechnicianKpi() {
  return useQuery({
    queryKey: technicianKeys.kpi(),
    queryFn: async (): Promise<TechnicianKpi> => {
      const { data, error } = await api.GET(
        "/api/clinical/technician/kpi" as never,
      )
      if (error) throw new Error("Failed to fetch technician KPI")
      const result = data as TechnicianKpi | { value?: TechnicianKpi } | unknown
      if (result && typeof result === "object" && "waiting" in result) return result as TechnicianKpi
      if (result && typeof result === "object" && "value" in result) {
        const wrapped = result as { value?: TechnicianKpi }
        if (wrapped.value) return wrapped.value
      }
      return { waiting: 0, inProgress: 0, completed: 0, redFlag: 0 }
    },
    refetchInterval: 30_000, // per D-17
  })
}

// -- Mutations --

export function useAcceptOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (orderId: string) => {
      const { data, error } = await api.POST(
        `/api/clinical/technician/orders/${orderId}/accept` as never,
      )
      if (error) throw error
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: technicianKeys.all })
    },
  })
}

export function useCompleteOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (orderId: string) => {
      const { data, error } = await api.POST(
        `/api/clinical/technician/orders/${orderId}/complete` as never,
      )
      if (error) throw error
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: technicianKeys.all })
    },
  })
}

export function useReturnToQueue() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (orderId: string) => {
      const { data, error } = await api.POST(
        `/api/clinical/technician/orders/${orderId}/return-to-queue` as never,
      )
      if (error) throw error
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: technicianKeys.all })
    },
  })
}

export function useRedFlagOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, reason }: { orderId: string; reason: string }) => {
      const { data, error } = await api.POST(
        `/api/clinical/technician/orders/${orderId}/red-flag` as never,
        { body: { reason } } as never,
      )
      if (error) throw error
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: technicianKeys.all })
    },
  })
}
