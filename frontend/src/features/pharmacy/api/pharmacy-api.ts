import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"
import { toast } from "sonner"

// -- Types matching backend Pharmacy.Contracts.Dtos --

export interface DrugCatalogItemDto {
  id: string
  name: string
  nameVi: string
  genericName: string
  form: number
  strength: string | null
  route: number
  unit: string
  defaultDosageTemplate: string | null
  isActive: boolean
}

// -- Enum maps for frontend display --

export const DRUG_FORM_MAP: Record<number, string> = {
  0: "eyeDrops",
  1: "tablet",
  2: "capsule",
  3: "ointment",
  4: "injection",
  5: "gel",
  6: "solution",
  7: "suspension",
  8: "cream",
  9: "spray",
}

export const DRUG_ROUTE_MAP: Record<number, string> = {
  0: "topical",
  1: "oral",
  2: "intramuscular",
  3: "intravenous",
  4: "subconjunctival",
  5: "intravitreal",
  6: "periocular",
}

// -- Query key factory --

export const pharmacyKeys = {
  all: ["pharmacy"] as const,
  drugs: () => [...pharmacyKeys.all, "drugs"] as const,
  drugSearch: (term: string) =>
    [...pharmacyKeys.all, "drugs", "search", term] as const,
}

// -- API functions --

async function getDrugCatalogList(): Promise<DrugCatalogItemDto[]> {
  const { data, error } = await api.GET("/api/pharmacy/drugs" as never)
  if (error) throw new Error("Failed to fetch drug catalog")
  return (data as DrugCatalogItemDto[]) ?? []
}

async function searchDrugCatalog(
  term: string,
): Promise<DrugCatalogItemDto[]> {
  const { data, error } = await api.GET(
    "/api/pharmacy/drugs/search" as never,
    {
      params: { query: { term } },
    } as never,
  )
  if (error) throw new Error("Failed to search drug catalog")
  return (data as DrugCatalogItemDto[]) ?? []
}

async function createDrugCatalogItem(
  command: Omit<DrugCatalogItemDto, "id" | "isActive">,
): Promise<{ id: string }> {
  const { data, error, response } = await api.POST(
    "/api/pharmacy/drugs" as never,
    {
      body: command,
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create drug catalog item")
  }
  return data as { id: string }
}

async function updateDrugCatalogItem(
  id: string,
  command: Omit<DrugCatalogItemDto, "id" | "isActive">,
): Promise<void> {
  const { error, response } = await api.PUT(
    `/api/pharmacy/drugs/${id}` as never,
    {
      body: { id, ...command },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update drug catalog item")
  }
}

// -- TanStack Query hooks --

export function useDrugCatalogList() {
  return useQuery({
    queryKey: pharmacyKeys.drugs(),
    queryFn: getDrugCatalogList,
  })
}

export function useDrugCatalogSearch(term: string) {
  return useQuery({
    queryKey: pharmacyKeys.drugSearch(term),
    queryFn: () => searchDrugCatalog(term),
    enabled: term.length >= 2,
  })
}

export function useCreateDrugCatalogItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: Omit<DrugCatalogItemDto, "id" | "isActive">) =>
      createDrugCatalogItem(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.drugs() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUpdateDrugCatalogItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      id,
      ...command
    }: Omit<DrugCatalogItemDto, "isActive">) =>
      updateDrugCatalogItem(id, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.drugs() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}
