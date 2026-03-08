import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import {
  getFrames,
  searchFrames,
  createFrame,
  updateFrame,
  generateBarcode,
  getLensCatalog,
  createLensCatalogItem,
  updateLensCatalogItem,
  adjustLensStock,
  getLowLensStockAlerts,
  getGlassesOrders,
  getGlassesOrderById,
  getOverdueOrders,
  createGlassesOrder,
  updateOrderStatus,
  getComboPackages,
  createComboPackage,
  updateComboPackage,
  getWarrantyClaims,
  createWarrantyClaim,
  approveWarrantyClaim,
  uploadWarrantyDocument,
  getPatientPrescriptionHistory,
  getPrescriptionComparison,
  getStocktakingSessions,
  getStocktakingSession,
  getDiscrepancyReport,
  startStocktakingSession,
  recordStocktakingItem,
  completeStocktaking,
} from "./optical-api"
import type {
  GetFramesParams,
  SearchFramesParams,
  CreateFrameInput,
  UpdateFrameInput,
  CreateLensCatalogItemInput,
  UpdateLensCatalogItemInput,
  AdjustLensStockInput,
  GetGlassesOrdersParams,
  CreateGlassesOrderInput,
  UpdateOrderStatusInput,
  CreateComboPackageInput,
  UpdateComboPackageInput,
  GetWarrantyClaimsParams,
  CreateWarrantyClaimInput,
  ApproveWarrantyClaimInput,
  GetStocktakingSessionsParams,
  StartStocktakingSessionInput,
  RecordStocktakingItemInput,
} from "./optical-api"

// -- Query key factory --

export const opticalKeys = {
  all: ["optical"] as const,
  frames: {
    all: () => [...opticalKeys.all, "frames"] as const,
    list: (params: GetFramesParams) => [...opticalKeys.all, "frames", "list", params] as const,
    search: (params: SearchFramesParams) => [...opticalKeys.all, "frames", "search", params] as const,
  },
  lenses: {
    all: () => [...opticalKeys.all, "lenses"] as const,
    catalog: (includeInactive?: boolean) => [...opticalKeys.all, "lenses", "catalog", includeInactive] as const,
    alerts: () => [...opticalKeys.all, "lenses", "alerts"] as const,
  },
  orders: {
    all: () => [...opticalKeys.all, "orders"] as const,
    list: (params: GetGlassesOrdersParams) => [...opticalKeys.all, "orders", "list", params] as const,
    detail: (id: string) => [...opticalKeys.all, "orders", id] as const,
    overdue: () => [...opticalKeys.all, "orders", "overdue"] as const,
  },
  combos: {
    all: () => [...opticalKeys.all, "combos"] as const,
    list: (includeInactive?: boolean) => [...opticalKeys.all, "combos", "list", includeInactive] as const,
  },
  warranty: {
    all: () => [...opticalKeys.all, "warranty"] as const,
    list: (params: GetWarrantyClaimsParams) => [...opticalKeys.all, "warranty", "list", params] as const,
    byOrder: (orderId: string) => [...opticalKeys.all, "warranty", orderId] as const,
  },
  prescriptions: {
    byPatient: (patientId: string) => [...opticalKeys.all, "prescriptions", patientId] as const,
    comparison: (patientId: string, id1: string, id2: string) =>
      [...opticalKeys.all, "prescriptions", "compare", patientId, id1, id2] as const,
  },
  stocktaking: {
    all: () => [...opticalKeys.all, "stocktaking"] as const,
    list: (params: GetStocktakingSessionsParams) => [...opticalKeys.all, "stocktaking", "list", params] as const,
    detail: (id: string) => [...opticalKeys.all, "stocktaking", id] as const,
    discrepancy: (id: string) => [...opticalKeys.all, "stocktaking", id, "discrepancy"] as const,
    current: () => [...opticalKeys.all, "stocktaking", "current"] as const,
  },
}

// -- Frame query hooks --

export function useFrames(params: GetFramesParams = {}) {
  return useQuery({
    queryKey: opticalKeys.frames.list(params),
    queryFn: () => getFrames(params),
  })
}

export function useSearchFrames(params: SearchFramesParams) {
  return useQuery({
    queryKey: opticalKeys.frames.search(params),
    queryFn: () => searchFrames(params),
    enabled: !!(params.searchTerm && params.searchTerm.length >= 2) ||
      params.material != null ||
      params.frameType != null ||
      params.gender != null,
  })
}

// -- Frame mutation hooks --

export function useCreateFrame() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateFrameInput) => createFrame(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.frames.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUpdateFrame() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...data }: { id: string } & UpdateFrameInput) => updateFrame(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.frames.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useGenerateBarcode() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (frameId: string) => generateBarcode(frameId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.frames.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

// -- Lens query hooks --

export function useLensCatalog(includeInactive?: boolean) {
  return useQuery({
    queryKey: opticalKeys.lenses.catalog(includeInactive),
    queryFn: () => getLensCatalog({ includeInactive }),
  })
}

export function useLowLensStockAlerts() {
  return useQuery({
    queryKey: opticalKeys.lenses.alerts(),
    queryFn: getLowLensStockAlerts,
  })
}

// -- Lens mutation hooks --

export function useCreateLensCatalogItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateLensCatalogItemInput) => createLensCatalogItem(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.lenses.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUpdateLensCatalogItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...data }: { id: string } & UpdateLensCatalogItemInput) =>
      updateLensCatalogItem(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.lenses.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useAdjustLensStock() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: AdjustLensStockInput) => adjustLensStock(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.lenses.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

// -- Order query hooks --

export function useGlassesOrders(params: GetGlassesOrdersParams = {}) {
  return useQuery({
    queryKey: opticalKeys.orders.list(params),
    queryFn: () => getGlassesOrders(params),
  })
}

export function useGlassesOrderById(id: string | undefined) {
  return useQuery({
    queryKey: opticalKeys.orders.detail(id ?? ""),
    queryFn: () => getGlassesOrderById(id!),
    enabled: !!id,
  })
}

export function useOverdueOrders() {
  return useQuery({
    queryKey: opticalKeys.orders.overdue(),
    queryFn: getOverdueOrders,
  })
}

// -- Order mutation hooks --

export function useCreateGlassesOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateGlassesOrderInput) => createGlassesOrder(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.orders.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUpdateOrderStatus() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...data }: { id: string } & UpdateOrderStatusInput) =>
      updateOrderStatus(id, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.orders.all() })
      queryClient.invalidateQueries({ queryKey: opticalKeys.orders.detail(variables.id) })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

// -- Combo package query hooks --

export function useComboPackages(includeInactive?: boolean) {
  return useQuery({
    queryKey: opticalKeys.combos.list(includeInactive),
    queryFn: () => getComboPackages({ includeInactive }),
  })
}

// -- Combo package mutation hooks --

export function useCreateComboPackage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateComboPackageInput) => createComboPackage(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.combos.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUpdateComboPackage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...data }: { id: string } & UpdateComboPackageInput) =>
      updateComboPackage(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.combos.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

// -- Warranty query hooks --

export function useWarrantyClaims(params: GetWarrantyClaimsParams = {}) {
  return useQuery({
    queryKey: opticalKeys.warranty.list(params),
    queryFn: () => getWarrantyClaims(params),
  })
}

// -- Warranty mutation hooks --

export function useCreateWarrantyClaim() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateWarrantyClaimInput) => createWarrantyClaim(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.warranty.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useApproveWarrantyClaim() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...data }: { id: string } & ApproveWarrantyClaimInput) =>
      approveWarrantyClaim(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.warranty.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUploadWarrantyDocument() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) => uploadWarrantyDocument(id, file),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.warranty.byOrder(variables.id) })
      queryClient.invalidateQueries({ queryKey: opticalKeys.warranty.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

// -- Prescription query hooks --

export function usePatientPrescriptionHistory(patientId: string | undefined) {
  return useQuery({
    queryKey: opticalKeys.prescriptions.byPatient(patientId ?? ""),
    queryFn: () => getPatientPrescriptionHistory(patientId!),
    enabled: !!patientId,
  })
}

export function usePrescriptionComparison(
  params: { patientId: string; id1: string; id2: string } | undefined,
) {
  return useQuery({
    queryKey: params
      ? opticalKeys.prescriptions.comparison(params.patientId, params.id1, params.id2)
      : (["optical", "prescriptions", "compare", undefined] as const),
    queryFn: () => getPrescriptionComparison(params!),
    enabled: !!params && !!params.patientId && !!params.id1 && !!params.id2,
  })
}

// -- Stocktaking query hooks --

export function useStocktakingSessions(params: GetStocktakingSessionsParams = {}) {
  return useQuery({
    queryKey: opticalKeys.stocktaking.list(params),
    queryFn: () => getStocktakingSessions(params),
  })
}

export function useStocktakingSession(id: string | undefined) {
  return useQuery({
    queryKey: opticalKeys.stocktaking.detail(id ?? ""),
    queryFn: () => getStocktakingSession(id!),
    enabled: !!id,
  })
}

export function useDiscrepancyReport(id: string | undefined) {
  return useQuery({
    queryKey: opticalKeys.stocktaking.discrepancy(id ?? ""),
    queryFn: () => getDiscrepancyReport(id!),
    enabled: !!id,
  })
}

// -- Stocktaking mutation hooks --

export function useStartStocktakingSession() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: StartStocktakingSessionInput) => startStocktakingSession(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.stocktaking.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useRecordStocktakingItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ sessionId, ...data }: { sessionId: string } & RecordStocktakingItemInput) =>
      recordStocktakingItem(sessionId, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: opticalKeys.stocktaking.detail(variables.sessionId),
      })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useCompleteStocktaking() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (sessionId: string) => completeStocktaking(sessionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opticalKeys.stocktaking.all() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}
