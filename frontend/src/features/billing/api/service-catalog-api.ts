import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { api } from "@/shared/lib/api-client"

// -- Types matching backend Billing.Contracts.Dtos.ServiceCatalogItemDto --

export interface ServiceCatalogItemDto {
  id: string
  code: string
  name: string
  nameVi: string
  price: number
  isActive: boolean
  description: string | null
  createdAt: string
  updatedAt: string | null
}

export interface CreateServiceCatalogItemInput {
  code: string
  name: string
  nameVi: string
  price: number
  description?: string | null
}

export interface UpdateServiceCatalogItemInput {
  name: string
  nameVi: string
  price: number
  isActive: boolean
  description?: string | null
}

// -- Query key factory --

export const serviceCatalogKeys = {
  all: ["service-catalog"] as const,
  list: (includeInactive?: boolean) =>
    [...serviceCatalogKeys.all, "list", { includeInactive }] as const,
}

// -- API functions --

async function getServiceCatalogItems(
  includeInactive?: boolean,
): Promise<ServiceCatalogItemDto[]> {
  const { data, error } = await api.GET("/api/billing/service-catalog" as never, {
    params: { query: { includeInactive: includeInactive ?? false } },
  } as never)
  if (error) throw new Error("Failed to fetch service catalog items")
  return (data as ServiceCatalogItemDto[]) ?? []
}

async function createServiceCatalogItem(
  command: CreateServiceCatalogItemInput,
): Promise<ServiceCatalogItemDto> {
  const { data, error, response } = await api.POST(
    "/api/billing/service-catalog" as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create service catalog item")
  }
  return data as ServiceCatalogItemDto
}

async function updateServiceCatalogItem(
  id: string,
  command: UpdateServiceCatalogItemInput,
): Promise<ServiceCatalogItemDto> {
  const { data, error, response } = await api.PUT(
    `/api/billing/service-catalog/${id}` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update service catalog item")
  }
  return data as ServiceCatalogItemDto
}

// -- TanStack Query hooks --

export function useServiceCatalogItems(includeInactive?: boolean) {
  return useQuery({
    queryKey: serviceCatalogKeys.list(includeInactive),
    queryFn: () => getServiceCatalogItems(includeInactive),
  })
}

export function useCreateServiceCatalogItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: CreateServiceCatalogItemInput) =>
      createServiceCatalogItem(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: serviceCatalogKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUpdateServiceCatalogItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      id,
      ...command
    }: { id: string } & UpdateServiceCatalogItemInput) =>
      updateServiceCatalogItem(id, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: serviceCatalogKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

// Re-export for direct use
export { getServiceCatalogItems, createServiceCatalogItem, updateServiceCatalogItem }
