import { useQuery } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"
import type { PatientSearchResult } from "@/features/patient/hooks/usePatientSearch"

export function useRecentPatients(count: number = 10) {
  return useQuery({
    queryKey: ["patients", "recent", count],
    queryFn: async (): Promise<PatientSearchResult[]> => {
      const { data, error } = await api.GET("/api/patients/recent" as never, {
        params: { query: { count } } as never,
      })
      if (error) {
        throw new Error("Failed to fetch recent patients")
      }
      return (data as PatientSearchResult[]) ?? []
    },
    staleTime: 60_000,
  })
}
