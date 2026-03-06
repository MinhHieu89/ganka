import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import {
  getConsumableItems,
  createConsumableItem,
  updateConsumableItem,
  addConsumableStock,
  adjustConsumableStock,
  getConsumableAlerts,
} from "./consumables-api"
import type {
  CreateConsumableItemInput,
  UpdateConsumableItemInput,
  AddConsumableStockInput,
  AdjustConsumableStockInput,
} from "./consumables-api"

// -- Query key factory --

export const consumableKeys = {
  all: ["consumables"] as const,
  items: {
    all: () => [...consumableKeys.all, "items"] as const,
  },
  alerts: {
    all: () => [...consumableKeys.all, "alerts"] as const,
  },
}

// -- Query hooks --

export function useConsumableItems() {
  return useQuery({
    queryKey: consumableKeys.items.all(),
    queryFn: getConsumableItems,
  })
}

export function useConsumableAlerts() {
  return useQuery({
    queryKey: consumableKeys.alerts.all(),
    queryFn: getConsumableAlerts,
  })
}

// -- Mutation hooks --

export function useCreateConsumableItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: CreateConsumableItemInput) => createConsumableItem(input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: consumableKeys.items.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUpdateConsumableItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & UpdateConsumableItemInput) =>
      updateConsumableItem(id, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: consumableKeys.items.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useAddConsumableStock() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & AddConsumableStockInput) =>
      addConsumableStock(id, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: consumableKeys.items.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useAdjustConsumableStock() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & AdjustConsumableStockInput) =>
      adjustConsumableStock(id, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: consumableKeys.items.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}
