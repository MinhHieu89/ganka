import { api } from "@/shared/lib/api-client"

// -- Types matching backend Pharmacy.Contracts.Dtos (Consumables) --

export interface ConsumableItemDto {
  id: string
  name: string
  nameVi: string
  unit: string
  trackingMode: number
  currentStock: number
  minStockLevel: number
  isLowStock: boolean
  isActive: boolean
}

export interface ConsumableBatchDto {
  id: string
  consumableItemId: string
  batchNumber: string
  expiryDate: string
  initialQuantity: number
  currentQuantity: number
  isExpired: boolean
  isNearExpiry: boolean
}

export interface ConsumableAlertDto {
  consumableItemId: string
  name: string
  currentStock: number
  minStockLevel: number
}

// -- Request types --

export interface CreateConsumableItemInput {
  name: string
  nameVi: string
  unit: string
  trackingMode: number
  minStockLevel: number
}

export interface UpdateConsumableItemInput {
  name: string
  nameVi: string
  unit: string
  minStockLevel: number
  isActive?: boolean
}

export interface AddConsumableStockInput {
  quantity: number
  batchNumber?: string | null
  expiryDate?: string | null
  notes?: string | null
}

export interface AdjustConsumableStockInput {
  quantityChange: number
  reason: number
  notes?: string | null
  consumableBatchId?: string | null
}

// -- API functions --

export async function getConsumableItems(): Promise<ConsumableItemDto[]> {
  const { data, error } = await api.GET("/api/consumables" as never)
  if (error) throw new Error("Failed to fetch consumable items")
  return (data as ConsumableItemDto[]) ?? []
}

export async function createConsumableItem(
  input: CreateConsumableItemInput,
): Promise<{ id: string }> {
  const { data, error, response } = await api.POST("/api/consumables" as never, {
    body: input,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create consumable item")
  }
  return data as { id: string }
}

export async function updateConsumableItem(
  id: string,
  input: UpdateConsumableItemInput,
): Promise<void> {
  const { error, response } = await api.PUT(`/api/consumables/${id}` as never, {
    body: { id, ...input },
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update consumable item")
  }
}

export async function addConsumableStock(
  id: string,
  input: AddConsumableStockInput,
): Promise<void> {
  const { error, response } = await api.POST(`/api/consumables/${id}/stock` as never, {
    body: input,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to add consumable stock")
  }
}

export async function adjustConsumableStock(
  id: string,
  input: AdjustConsumableStockInput,
): Promise<void> {
  const { error, response } = await api.POST(`/api/consumables/${id}/adjust` as never, {
    body: input,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    if (err?.detail) throw new Error(String(err.detail))
    if (err?.title) throw new Error(String(err.title))
    throw new Error("Failed to adjust consumable stock")
  }
}

export async function getConsumableBatches(id: string): Promise<ConsumableBatchDto[]> {
  const { data, error } = await api.GET(`/api/consumables/${id}/batches` as never)
  if (error) throw new Error("Failed to fetch consumable batches")
  return (data as ConsumableBatchDto[]) ?? []
}

export async function getConsumableAlerts(): Promise<ConsumableItemDto[]> {
  const { data, error } = await api.GET("/api/consumables/alerts" as never)
  if (error) throw new Error("Failed to fetch consumable alerts")
  return (data as ConsumableItemDto[]) ?? []
}
