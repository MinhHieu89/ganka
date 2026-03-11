import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import {
  getSuppliers,
  createSupplier,
  updateSupplier,
  toggleSupplierActive,
  getDrugInventory,
  getDrugBatches,
  updateDrugPricing,
  adjustStock,
  getStockImports,
  createStockImport,
  getExpiryAlerts,
  getLowStockAlerts,
  getPendingPrescriptions,
  getPendingCount,
  dispenseDrugs,
  getDispensingHistory,
  createOtcSale,
  getOtcSales,
  getDrugCatalogList,
  searchDrugCatalog,
  createDrugCatalogItem,
  updateDrugCatalogItem,
} from "./pharmacy-api"
import type {
  CreateSupplierInput,
  UpdateSupplierInput,
  UpdateDrugPricingInput,
  AdjustStockInput,
  CreateStockImportInput,
  DispenseDrugsInput,
  CreateOtcSaleInput,
  DrugCatalogItemDto,
} from "./pharmacy-api"

// -- Query key factory --

export const pharmacyKeys = {
  all: ["pharmacy"] as const,
  drugs: () => [...pharmacyKeys.all, "drugs"] as const,
  drugSearch: (term: string) => [...pharmacyKeys.all, "drugs", "search", term] as const,
  suppliers: {
    all: () => [...pharmacyKeys.all, "suppliers"] as const,
  },
  inventory: {
    all: () => [...pharmacyKeys.all, "inventory"] as const,
    batches: (drugId: string) => [...pharmacyKeys.all, "inventory", "batches", drugId] as const,
  },
  stockImports: {
    list: (page: number) => [...pharmacyKeys.all, "stock-imports", page] as const,
  },
  alerts: {
    expiry: (days?: number) =>
      days != null
        ? ([...pharmacyKeys.all, "alerts", "expiry", days] as const)
        : ([...pharmacyKeys.all, "alerts", "expiry"] as const),
    lowStock: () => [...pharmacyKeys.all, "alerts", "low-stock"] as const,
  },
  dispensing: {
    pending: () => [...pharmacyKeys.all, "dispensing", "pending"] as const,
    pendingCount: () => [...pharmacyKeys.all, "dispensing", "pending-count"] as const,
    history: (page: number) => [...pharmacyKeys.all, "dispensing", "history", page] as const,
  },
  otcSales: {
    list: (page: number) => [...pharmacyKeys.all, "otc-sales", page] as const,
  },
}

// -- Query hooks --

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

export function useSuppliers() {
  return useQuery({
    queryKey: pharmacyKeys.suppliers.all(),
    queryFn: getSuppliers,
  })
}

export function useDrugInventory() {
  return useQuery({
    queryKey: pharmacyKeys.inventory.all(),
    queryFn: getDrugInventory,
  })
}

export function useDrugBatches(drugId: string | undefined) {
  return useQuery({
    queryKey: pharmacyKeys.inventory.batches(drugId ?? ""),
    queryFn: () => getDrugBatches(drugId!),
    enabled: !!drugId,
  })
}

export function useStockImports(page: number = 1) {
  return useQuery({
    queryKey: pharmacyKeys.stockImports.list(page),
    queryFn: () => getStockImports(page),
  })
}

export function useExpiryAlerts(days?: number) {
  return useQuery({
    queryKey: pharmacyKeys.alerts.expiry(days),
    queryFn: () => getExpiryAlerts(days),
  })
}

export function useLowStockAlerts() {
  return useQuery({
    queryKey: pharmacyKeys.alerts.lowStock(),
    queryFn: getLowStockAlerts,
  })
}

export function usePendingPrescriptions() {
  return useQuery({
    queryKey: pharmacyKeys.dispensing.pending(),
    queryFn: () => getPendingPrescriptions(),
    refetchInterval: 30_000,
    refetchIntervalInBackground: false,
  })
}

export function usePendingCount() {
  return useQuery({
    queryKey: pharmacyKeys.dispensing.pendingCount(),
    queryFn: getPendingCount,
    refetchInterval: 30_000,
    refetchIntervalInBackground: false,
  })
}

export function useDispensingHistory(page: number = 1, patientId?: string | null) {
  return useQuery({
    queryKey: patientId
      ? [...pharmacyKeys.dispensing.history(page), patientId]
      : pharmacyKeys.dispensing.history(page),
    queryFn: () => getDispensingHistory(page, 20, patientId),
  })
}

export function useOtcSales(page: number = 1) {
  return useQuery({
    queryKey: pharmacyKeys.otcSales.list(page),
    queryFn: () => getOtcSales(page),
  })
}

// -- Mutation hooks --

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
    mutationFn: ({ id, ...command }: Omit<DrugCatalogItemDto, "isActive">) =>
      updateDrugCatalogItem(id, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.drugs() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useCreateSupplier() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: CreateSupplierInput) => createSupplier(input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.suppliers.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUpdateSupplier() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & UpdateSupplierInput) =>
      updateSupplier(id, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.suppliers.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useToggleSupplierActive() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => toggleSupplierActive(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.suppliers.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUpdateDrugPricing() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ drugId, ...input }: { drugId: string } & UpdateDrugPricingInput) =>
      updateDrugPricing(drugId, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.inventory.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useAdjustStock() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: AdjustStockInput) => adjustStock(input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.inventory.all() })
      queryClient.invalidateQueries({ queryKey: [...pharmacyKeys.all, "inventory", "batches"] })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useCreateStockImport() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: CreateStockImportInput) => createStockImport(input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.inventory.all() })
      queryClient.invalidateQueries({ queryKey: [...pharmacyKeys.all, "stock-imports"] })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useDispenseDrugs() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: DispenseDrugsInput) => dispenseDrugs(input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.dispensing.pending() })
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.dispensing.pendingCount() })
      queryClient.invalidateQueries({
        queryKey: [...pharmacyKeys.all, "dispensing", "history"],
      })
      // Dispensing deducts stock — refresh inventory, batches, and alerts
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.inventory.all() })
      queryClient.invalidateQueries({ queryKey: [...pharmacyKeys.all, "inventory", "batches"] })
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.alerts.lowStock() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useCreateOtcSale() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: CreateOtcSaleInput) => createOtcSale(input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: pharmacyKeys.inventory.all() })
      queryClient.invalidateQueries({ queryKey: [...pharmacyKeys.all, "otc-sales"] })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}
