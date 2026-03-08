import { api } from "@/shared/lib/api-client"

// -- Enums matching backend --

export const FRAME_MATERIAL_MAP: Record<number, string> = {
  0: "Metal",
  1: "Plastic",
  2: "Titanium",
}

export const FRAME_TYPE_MAP: Record<number, string> = {
  0: "FullRim",
  1: "SemiRimless",
  2: "Rimless",
}

export const FRAME_GENDER_MAP: Record<number, string> = {
  0: "Male",
  1: "Female",
  2: "Unisex",
}

export const GLASSES_ORDER_STATUS_MAP: Record<number, string> = {
  0: "Ordered",
  1: "Processing",
  2: "Received",
  3: "Ready",
  4: "Delivered",
}

export const PROCESSING_TYPE_MAP: Record<number, string> = {
  0: "InHouse",
  1: "Outsourced",
}

export const WARRANTY_RESOLUTION_MAP: Record<number, string> = {
  0: "Replace",
  1: "Repair",
  2: "Discount",
}

export const WARRANTY_APPROVAL_STATUS_MAP: Record<number, string> = {
  0: "Pending",
  1: "Approved",
  2: "Rejected",
}

export const STOCKTAKING_STATUS_MAP: Record<number, string> = {
  0: "InProgress",
  1: "Completed",
  2: "Cancelled",
}

// -- DTOs matching backend Optical.Contracts.Dtos --

export interface FrameDto {
  id: string
  brand: string
  model: string
  color: string
  lensWidth: number
  bridgeWidth: number
  templeLength: number
  material: number
  frameType: number
  gender: number
  sellingPrice: number
  costPrice: number
  barcode: string | null
  stockQuantity: number
  isActive: boolean
  branchId: string
}

export interface LensCatalogItemDto {
  id: string
  name: string
  material: number
  coatings: number
  basePrice: number
  isActive: boolean
  stockEntries: LensStockEntryDto[]
}

export interface LensStockEntryDto {
  id: string
  sph: number
  cyl: number
  add: number | null
  quantity: number
}

export interface LowLensStockAlertDto {
  lensCatalogItemId: string
  lensName: string
  sph: number
  cyl: number
  add: number | null
  currentQuantity: number
}

export interface GlassesOrderDto {
  id: string
  patientId: string
  patientName: string
  visitId: string
  opticalPrescriptionId: string
  status: number
  processingType: number
  isPaymentConfirmed: boolean
  estimatedDeliveryDate: string | null
  deliveredAt: string | null
  totalPrice: number
  frameId: string | null
  frameBrand: string | null
  frameModel: string | null
  lensCatalogItemId: string | null
  lensName: string | null
  comboPackageId: string | null
  comboPackageName: string | null
  notes: string | null
  isOverdue: boolean
  isUnderWarranty: boolean
  createdAt: string
  updatedAt: string | null
}

export interface ComboPackageDto {
  id: string
  name: string
  description: string | null
  frameId: string | null
  frameName: string | null
  lensCatalogItemId: string | null
  lensName: string | null
  comboPrice: number
  originalTotalPrice: number | null
  savings: number | null
  isActive: boolean
  createdAt: string
}

export interface WarrantyClaimDto {
  id: string
  glassesOrderId: string
  patientName: string
  claimDate: string
  resolutionType: number
  approvalStatus: number
  notes: string | null
  approvalNotes: string | null
  approvedAt: string | null
  documentUrls: string[]
  createdAt: string
}

export interface StocktakingSessionDto {
  id: string
  name: string
  status: number
  startedAt: string
  completedAt: string | null
  itemCount: number
}

export interface StocktakingItemDto {
  id: string
  sessionId: string
  barcode: string
  frameId: string | null
  frameName: string | null
  systemCount: number
  physicalCount: number
  discrepancy: number
  recordedAt: string
}

export interface DiscrepancyReportDto {
  sessionId: string
  sessionName: string
  completedAt: string | null
  items: StocktakingItemDto[]
  totalDiscrepancy: number
}

export interface OpticalPrescriptionHistoryDto {
  prescriptionId: string
  visitId: string
  prescribedAt: string
  rightSph: number | null
  rightCyl: number | null
  rightAxis: number | null
  rightAdd: number | null
  rightVa: string | null
  leftSph: number | null
  leftCyl: number | null
  leftAxis: number | null
  leftAdd: number | null
  leftVa: string | null
  pupillaryDistance: number | null
  notes: string | null
}

export interface PrescriptionComparisonDto {
  prescription1: OpticalPrescriptionHistoryDto
  prescription2: OpticalPrescriptionHistoryDto
  rightSphChange: number | null
  rightCylChange: number | null
  leftSphChange: number | null
  leftCylChange: number | null
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface GenerateBarcodeDto {
  barcode: string
  frameId: string
}

// -- Request input types --

export interface GetFramesParams {
  includeInactive?: boolean
  page?: number
  pageSize?: number
}

export interface SearchFramesParams {
  searchTerm?: string
  material?: number
  frameType?: number
  gender?: number
  page?: number
  pageSize?: number
}

export interface CreateFrameInput {
  brand: string
  model: string
  color: string
  lensWidth: number
  bridgeWidth: number
  templeLength: number
  material: number
  frameType: number
  gender: number
  sellingPrice: number
  costPrice: number
  barcode?: string | null
  stockQuantity?: number
}

export interface UpdateFrameInput {
  brand: string
  model: string
  color: string
  lensWidth: number
  bridgeWidth: number
  templeLength: number
  material: number
  frameType: number
  gender: number
  sellingPrice: number
  costPrice: number
  barcode?: string | null
  stockQuantity?: number
  isActive?: boolean
}

export interface CreateLensCatalogItemInput {
  name: string
  material: number
  coatings: number
  basePrice: number
}

export interface UpdateLensCatalogItemInput {
  name: string
  material: number
  coatings: number
  basePrice: number
  isActive?: boolean
}

export interface AdjustLensStockInput {
  lensCatalogItemId: string
  sph: number
  cyl: number
  add?: number | null
  quantityChange: number
}

export interface GetGlassesOrdersParams {
  statusFilter?: number
  page?: number
  pageSize?: number
}

export interface CreateGlassesOrderInput {
  patientId: string
  visitId: string
  opticalPrescriptionId: string
  processingType: number
  estimatedDeliveryDate?: string | null
  frameId?: string | null
  lensCatalogItemId?: string | null
  comboPackageId?: string | null
  totalPrice: number
  notes?: string | null
}

export interface UpdateOrderStatusInput {
  newStatus: number
}

export interface CreateComboPackageInput {
  name: string
  description?: string | null
  frameId?: string | null
  lensCatalogItemId?: string | null
  packagePrice: number
}

export interface UpdateComboPackageInput {
  name: string
  description?: string | null
  frameId?: string | null
  lensCatalogItemId?: string | null
  packagePrice: number
  isActive?: boolean
}

export interface GetWarrantyClaimsParams {
  approvalStatusFilter?: number
  page?: number
  pageSize?: number
}

export interface DeliveredOrderSummaryDto {
  id: string
  orderNumber: string
  patientName: string
  orderDate: string
  deliveredAt: string
  warrantyExpiresAt: string
  isUnderWarranty: boolean
  daysRemainingInWarranty: number | null
}

export interface CreateWarrantyClaimInput {
  glassesOrderId: string
  resolutionType: number
  notes?: string | null
}

export interface ApproveWarrantyClaimInput {
  approve: boolean
  reason?: string | null
}

export interface GetStocktakingSessionsParams {
  page?: number
  pageSize?: number
}

export interface StartStocktakingSessionInput {
  name: string
}

export interface RecordStocktakingItemInput {
  barcode: string
  physicalCount: number
}

// -- Frame endpoints --

export async function getFrames(params: GetFramesParams = {}): Promise<PagedResult<FrameDto>> {
  const { data, error } = await api.GET("/api/optical/frames" as never, {
    params: { query: params },
  } as never)
  if (error) throw new Error("Failed to fetch frames")
  return (data as PagedResult<FrameDto>) ?? { items: [], totalCount: 0, page: 1, pageSize: 20 }
}

export async function searchFrames(params: SearchFramesParams = {}): Promise<PagedResult<FrameDto>> {
  const { data, error } = await api.GET("/api/optical/frames/search" as never, {
    params: { query: params },
  } as never)
  if (error) throw new Error("Failed to search frames")
  return (data as PagedResult<FrameDto>) ?? { items: [], totalCount: 0, page: 1, pageSize: 20 }
}

export async function createFrame(data: CreateFrameInput): Promise<{ id: string }> {
  const { data: responseData, error, response } = await api.POST("/api/optical/frames" as never, {
    body: data,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create frame")
  }
  return responseData as { id: string }
}

export async function updateFrame(id: string, data: UpdateFrameInput): Promise<void> {
  const { error, response } = await api.PUT(`/api/optical/frames/${id}` as never, {
    body: { id, ...data },
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update frame")
  }
}

export async function generateBarcode(frameId: string): Promise<GenerateBarcodeDto> {
  const { data, error, response } = await api.POST(`/api/optical/frames/${frameId}/barcode` as never, {} as never)
  if (error || !response.ok) {
    throw new Error("Failed to generate barcode")
  }
  return data as GenerateBarcodeDto
}

// -- Lens endpoints --

export async function getLensCatalog(params: { includeInactive?: boolean } = {}): Promise<LensCatalogItemDto[]> {
  const { data, error } = await api.GET("/api/optical/lenses" as never, {
    params: { query: params },
  } as never)
  if (error) throw new Error("Failed to fetch lens catalog")
  return (data as LensCatalogItemDto[]) ?? []
}

export async function createLensCatalogItem(data: CreateLensCatalogItemInput): Promise<{ id: string }> {
  const { data: responseData, error, response } = await api.POST("/api/optical/lenses" as never, {
    body: data,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create lens catalog item")
  }
  return responseData as { id: string }
}

export async function updateLensCatalogItem(id: string, data: UpdateLensCatalogItemInput): Promise<void> {
  const { error, response } = await api.PUT(`/api/optical/lenses/${id}` as never, {
    body: { id, ...data },
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update lens catalog item")
  }
}

export async function adjustLensStock(data: AdjustLensStockInput): Promise<void> {
  const { error, response } = await api.POST("/api/optical/lenses/stock/adjust" as never, {
    body: data,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to adjust lens stock")
  }
}

export async function getLowLensStockAlerts(): Promise<LowLensStockAlertDto[]> {
  const { data, error } = await api.GET("/api/optical/lenses/alerts/low-stock" as never)
  if (error) throw new Error("Failed to fetch low lens stock alerts")
  return (data as LowLensStockAlertDto[]) ?? []
}

// -- Order endpoints --

export async function getGlassesOrders(params: GetGlassesOrdersParams = {}): Promise<PagedResult<GlassesOrderDto>> {
  const { data, error } = await api.GET("/api/optical/orders" as never, {
    params: { query: params },
  } as never)
  if (error) throw new Error("Failed to fetch glasses orders")
  return (data as PagedResult<GlassesOrderDto>) ?? { items: [], totalCount: 0, page: 1, pageSize: 20 }
}

export async function getGlassesOrderById(id: string): Promise<GlassesOrderDto> {
  const { data, error } = await api.GET(`/api/optical/orders/${id}` as never)
  if (error) throw new Error("Failed to fetch glasses order")
  return data as GlassesOrderDto
}

export async function getOverdueOrders(): Promise<GlassesOrderDto[]> {
  const { data, error } = await api.GET("/api/optical/orders/overdue" as never)
  if (error) throw new Error("Failed to fetch overdue orders")
  return (data as GlassesOrderDto[]) ?? []
}

export async function createGlassesOrder(data: CreateGlassesOrderInput): Promise<{ id: string }> {
  const { data: responseData, error, response } = await api.POST("/api/optical/orders" as never, {
    body: data,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create glasses order")
  }
  return responseData as { id: string }
}

export async function updateOrderStatus(id: string, data: UpdateOrderStatusInput): Promise<void> {
  const { error, response } = await api.PUT(`/api/optical/orders/${id}/status` as never, {
    body: { orderId: id, ...data },
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update order status")
  }
}

// -- Combo package endpoints --

export async function getComboPackages(params: { includeInactive?: boolean } = {}): Promise<ComboPackageDto[]> {
  const { data, error } = await api.GET("/api/optical/combos" as never, {
    params: { query: params },
  } as never)
  if (error) throw new Error("Failed to fetch combo packages")
  return (data as ComboPackageDto[]) ?? []
}

export async function createComboPackage(data: CreateComboPackageInput): Promise<{ id: string }> {
  const { data: responseData, error, response } = await api.POST("/api/optical/combos" as never, {
    body: data,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create combo package")
  }
  return responseData as { id: string }
}

export async function updateComboPackage(id: string, data: UpdateComboPackageInput): Promise<void> {
  const { error, response } = await api.PUT(`/api/optical/combos/${id}` as never, {
    body: { id, ...data },
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update combo package")
  }
}

// -- Warranty endpoints --

export async function getDeliveredGlassesOrders(): Promise<DeliveredOrderSummaryDto[]> {
  const { data, error } = await api.GET("/api/optical/orders/delivered" as never)
  if (error) throw new Error("Failed to fetch delivered orders")
  return (data as DeliveredOrderSummaryDto[]) ?? []
}

export async function getWarrantyClaims(params: GetWarrantyClaimsParams = {}): Promise<PagedResult<WarrantyClaimDto>> {
  const { data, error } = await api.GET("/api/optical/warranty" as never, {
    params: { query: params },
  } as never)
  if (error) throw new Error("Failed to fetch warranty claims")
  return (data as PagedResult<WarrantyClaimDto>) ?? { items: [], totalCount: 0, page: 1, pageSize: 20 }
}

export async function createWarrantyClaim(data: CreateWarrantyClaimInput): Promise<{ id: string }> {
  const { data: responseData, error, response } = await api.POST("/api/optical/warranty" as never, {
    body: data,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create warranty claim")
  }
  return responseData as { id: string }
}

export async function approveWarrantyClaim(id: string, data: ApproveWarrantyClaimInput): Promise<void> {
  const { error, response } = await api.PUT(`/api/optical/warranty/${id}/approval` as never, {
    body: { claimId: id, ...data },
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to process warranty claim approval")
  }
}

export async function uploadWarrantyDocument(id: string, file: File): Promise<{ documentUrl: string }> {
  const API_URL = (import.meta as never as { env: Record<string, string> }).env?.VITE_API_URL ?? "http://localhost:5255"
  const token = (await import("@/shared/stores/authStore")).useAuthStore.getState().accessToken

  const formData = new FormData()
  formData.append("file", file)

  const res = await fetch(`${API_URL}/api/optical/warranty/${id}/documents`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: formData,
    credentials: "include",
  })
  if (!res.ok) {
    const body = await res.json().catch(() => null)
    throw new Error(body?.detail || body?.title || "Failed to upload warranty document")
  }
  return res.json()
}

// -- Prescription endpoints --

export async function getPatientPrescriptionHistory(patientId: string): Promise<OpticalPrescriptionHistoryDto[]> {
  const { data, error } = await api.GET(`/api/optical/prescriptions/patient/${patientId}` as never)
  if (error) throw new Error("Failed to fetch prescription history")
  return (data as OpticalPrescriptionHistoryDto[]) ?? []
}

export async function getPrescriptionComparison(params: {
  patientId: string
  id1: string
  id2: string
}): Promise<PrescriptionComparisonDto> {
  const { data, error } = await api.GET("/api/optical/prescriptions/compare" as never, {
    params: { query: params },
  } as never)
  if (error) throw new Error("Failed to fetch prescription comparison")
  return data as PrescriptionComparisonDto
}

// -- Stocktaking endpoints --

export async function getStocktakingSessions(params: GetStocktakingSessionsParams = {}): Promise<PagedResult<StocktakingSessionDto>> {
  const { data, error } = await api.GET("/api/optical/stocktaking" as never, {
    params: { query: params },
  } as never)
  if (error) throw new Error("Failed to fetch stocktaking sessions")
  return (data as PagedResult<StocktakingSessionDto>) ?? { items: [], totalCount: 0, page: 1, pageSize: 20 }
}

export async function getStocktakingSession(id: string): Promise<StocktakingSessionDto> {
  const { data, error } = await api.GET(`/api/optical/stocktaking/${id}` as never)
  if (error) throw new Error("Failed to fetch stocktaking session")
  return data as StocktakingSessionDto
}

export async function getDiscrepancyReport(id: string): Promise<DiscrepancyReportDto> {
  const { data, error } = await api.GET(`/api/optical/stocktaking/${id}/discrepancy` as never)
  if (error) throw new Error("Failed to fetch discrepancy report")
  return data as DiscrepancyReportDto
}

export async function startStocktakingSession(data: StartStocktakingSessionInput): Promise<{ id: string }> {
  const { data: responseData, error, response } = await api.POST("/api/optical/stocktaking" as never, {
    body: data,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to start stocktaking session")
  }
  return responseData as { id: string }
}

export async function recordStocktakingItem(
  sessionId: string,
  data: RecordStocktakingItemInput,
): Promise<void> {
  const { error, response } = await api.POST(`/api/optical/stocktaking/${sessionId}/items` as never, {
    body: data,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to record stocktaking item")
  }
}

export async function completeStocktaking(sessionId: string): Promise<void> {
  const { error, response } = await api.PUT(`/api/optical/stocktaking/${sessionId}/complete` as never, {} as never)
  if (error || !response.ok) {
    throw new Error("Failed to complete stocktaking session")
  }
}
