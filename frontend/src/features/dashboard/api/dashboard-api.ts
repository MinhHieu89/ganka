import { useQuery } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"

export interface DashboardStatsDto {
  totalPatients: number
  todayAppointments: number
  activeVisits: number
  activeTreatments: number
}

export async function getDashboardStats(): Promise<DashboardStatsDto> {
  const { data, error } = await api.GET("/api/dashboard/stats" as never)
  if (error) {
    throw new Error("Failed to fetch dashboard stats")
  }
  return data as unknown as DashboardStatsDto
}

export function useDashboardStats() {
  return useQuery({
    queryKey: ["dashboard", "stats"],
    queryFn: getDashboardStats,
    staleTime: 30_000, // 30 seconds
    refetchInterval: 60_000, // Refresh every minute
  })
}
