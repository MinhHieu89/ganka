import { useQuery } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"

export interface PatientSearchResult {
  id: string
  fullName: string
  patientCode: string
  phone: string
}

interface UsePatientSearchOptions {
  enabled?: boolean
}

export function usePatientSearch(
  term: string,
  options?: UsePatientSearchOptions,
) {
  return useQuery({
    queryKey: ["patients", "search", term],
    queryFn: async (): Promise<PatientSearchResult[]> => {
      const { data, error } = await api.GET("/api/patients/search" as never, {
        params: { query: { term } },
      } as never)
      if (error) {
        throw new Error("Failed to search patients")
      }
      return (data as PatientSearchResult[]) ?? []
    },
    enabled: options?.enabled ?? term.length >= 2,
    staleTime: 30_000,
    placeholderData: (prev: PatientSearchResult[] | undefined) => prev,
  })
}
