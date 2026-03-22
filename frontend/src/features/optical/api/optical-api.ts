import { api } from "@/shared/lib/api-client"

// -- Enums matching backend --

export const FRAME_MATERIAL_MAP: Record<number, string> = {
  0: "enums.material.metal",
  1: "enums.material.plastic",
  2: "enums.material.titanium",
  3: "enums.material.acetate",
  4: "enums.material.stainlessSteel",
}

export const FRAME_TYPE_MAP: Record<number, string> = {
  0: "enums.frameType.fullRim",
  1: "enums.frameType.semiRimless",
  2: "enums.frameType.rimless",
}

export const FRAME_GENDER_MAP: Record<number, string> = {
  0: "enums.gender.male",
  1: "enums.gender.female",
  2: "enums.gender.unisex",
}

export const GLASSES_ORDER_STATUS_MAP: Record<number, string> = {
  0: "enums.orderStatus.ordered",
  1: "enums.orderStatus.processing",
  2: "enums.orderStatus.received",
  3: "enums.orderStatus.ready",
  4: "enums.orderStatus.delivered",
}

export const PROCESSING_TYPE_MAP: Record<number, string> = {
  0: "enums.processingType.inHouse",
  1: "enums.processingType.outsourced",
}

export const WARRANTY_RESOLUTION_MAP: Record<number, string> = {
  0: "enums.warrantyResolution.replace",
  1: "enums.warrantyResolution.repair",
  2: "enums.warrantyResolution.discount",
}

export const WARRANTY_APPROVAL_STATUS_MAP: Record<number, string> = {
  0: "enums.warrantyApproval.pending",
  1: "enums.warrantyApproval.approved",
  2: "enums.warrantyApproval.rejected",
}

export const STOCKTAKING_STATUS_MAP: Record<number, string> = {
  0: "enums.stocktakingStatus.inProgress",
  1: "enums.stocktakingStatus.completed",
  2: "Cancelled",
}

export const LENS_MATERIAL_MAP: Record<number, string> = {
  0: "enums.lensMaterial.cr39",
  1: "enums.lensMaterial.polycarbonate",
  2: "enums.lensMaterial.hiIndex",
  3: "enums.lensMaterial.trivex",
}

export const LENS_COATING_MAP: Record<number, string> = {
  1: "enums.coatings.antiReflective",
  2: "enums.coatings.blueCut",
  4: "enums.coatings.photochromic",
  8: "enums.coatings.scratchResistant",
  16: "enums.coatings.uvProtection",
}

export const LENS_COATING_BITS = [1, 2, 4, 8, 16] as const

export const LENS_TYPE_OPTIONS = [
  { value: "single_vision", label: "enums.lensType.singleVision" },
  { value: "bifocal", label: "enums.lensType.bifocal" },
  { value: "progressive", label: "enums.lensType.progressive" },
  { value: "reading", label: "enums.lensType.reading" },
]

/** Decode flags bitfield into array of coating bit values */
export function decodeCoatings(coatings: number): number[] {
  return LENS_COATING_BITS.filter((bit) => (coatings & bit) !== 0)
}

/** Encode array of coating bit values back into bitfield */
export function encodeCoatings(bits: number[]): number {
  return bits.reduce((acc, bit) => acc | bit, 0)
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
  brand: string
  name: string
  lensType: string
  material: number
  availableCoatings: number
  basePrice: number
  sellingPrice: number
  costPrice: number
  isActive: boolean
  preferredSupplierId: string | null
  supplierName: string | null
  stockEntries: LensStockEntryDto[]
  createdAt: string
}

export interface LensStockEntryDto {
  id: string
  lensCatalogItemId: string
  sph: number
  cyl: number
  add: number | null
  quantity: number
  minStockLevel: number
}

export interface LowLensStockAlertDto {
  lensCatalogItemId: string
  brand: string
  lensName: string
  sph: number
  cyl: number
  add: number | null
  currentStock: number
  minStockLevel: number
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
  resolution: number
  approvalStatus: number
  requiresApproval: boolean
  assessmentNotes: string | null
  approvalNotes: string | null
  approvedAt: string | null
  documentUrls: string[]
  createdAt: string
}

export interface StocktakingSessionDto {
  id: string
  name: string
  status: number
  startedById: string
  startedByName: string | null
  createdAt: string
  completedAt: string | null
  totalItemsScanned: number
  discrepancyCount: number
  notes: string | null
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
  totalScanned: number
  totalDiscrepancies: number
  overCount: number
  underCount: number
  missingFromSystem: number
  items: StocktakingItemDto[]
}

export interface OpticalPrescriptionHistoryDto {
  id: string
  visitId: string
  visitDate: string
  sphOd: number | null
  cylOd: number | null
  axisOd: number | null
  addOd: number | null
  sphOs: number | null
  cylOs: number | null
  axisOs: number | null
  addOs: number | null
  pd: number | null
  notes: string | null
}

export interface FieldChangeDto {
  fieldName: string
  oldValue: string | null
  newValue: string | null
  direction: string
}

export interface PrescriptionComparisonDto {
  older: OpticalPrescriptionHistoryDto
  newer: OpticalPrescriptionHistoryDto
  changes: FieldChangeDto[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

// Summary DTO for delivered orders used by warranty claim form
export interface DeliveredOrderSummaryDto {
  id: string
  patientName: string
  deliveredAt: string | null
  warrantyExpiresAt: string | null
  isUnderWarranty: boolean
  daysRemainingInWarranty: number | null
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
  brand: string
  name: string
  lensType: string
  material: number
  availableCoatings: number
  sellingPrice: number
  costPrice: number
  preferredSupplierId?: string | null
}

export interface UpdateLensCatalogItemInput {
  brand: string
  name: string
  lensType: string
  material: number
  availableCoatings: number
  sellingPrice: number
  costPrice: number
  preferredSupplierId?: string | null
  isActive?: boolean
}

export interface AdjustLensStockInput {
  lensCatalogItemId: string
  sph: number
  cyl: number
  add?: number | null
  quantityChange: number
  reason: string
  minStockLevel?: number
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
  comboPrice: number
  originalTotalPrice?: number | null
}

export interface UpdateComboPackageInput {
  name: string
  description?: string | null
  frameId?: string | null
  lensCatalogItemId?: string | null
  comboPrice: number
  originalTotalPrice?: number | null
  isActive?: boolean
}

export interface GetWarrantyClaimsParams {
  approvalStatusFilter?: number
  page?: number
  pageSize?: number
}

export interface CreateWarrantyClaimInput {
  glassesOrderId: string
  resolution: number
  assessmentNotes: string
  discountAmount?: number | null
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
  const { data, error, response } = await api.POST(`/api/optical/frames/${frameId}/generate-barcode` as never, {} as never)
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
  const { error, response } = await api.POST("/api/optical/lenses/stock-adjust" as never, {
    body: data,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to adjust lens stock")
  }
}

export async function getLowLensStockAlerts(): Promise<LowLensStockAlertDto[]> {
  const { data, error } = await api.GET("/api/optical/lenses/alerts" as never)
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

export async function getDeliveredOrders(): Promise<DeliveredOrderSummaryDto[]> {
  const { data, error } = await api.GET("/api/optical/orders/delivered" as never)
  if (error) throw new Error("Failed to fetch delivered orders")
  return (data as DeliveredOrderSummaryDto[]) ?? []
}

// Alias for backward compatibility with optical-queries.ts
export const getDeliveredGlassesOrders = getDeliveredOrders

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
