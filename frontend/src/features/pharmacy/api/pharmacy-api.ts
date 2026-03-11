import { api } from "@/shared/lib/api-client"

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
  sellingPrice?: number | null
  minStockLevel?: number | null
}

export interface SupplierDto {
  id: string
  name: string
  contactInfo: string | null
  isActive: boolean
  supplierDrugPrices?: SupplierDrugPriceDto[]
}

export interface SupplierDrugPriceDto {
  id: string
  supplierId: string
  drugCatalogItemId: string
  drugName: string
  defaultPurchasePrice: number
}

export interface DrugBatchDto {
  id: string
  drugCatalogItemId: string
  supplierId: string
  supplierName: string
  batchNumber: string
  expiryDate: string
  initialQuantity: number
  currentQuantity: number
  purchasePrice: number
  isExpired: boolean
  isNearExpiry: boolean
}

export interface DrugInventoryDto {
  drugCatalogItemId: string
  name: string
  nameVi: string
  genericName: string
  form: number
  strength: string | null
  unit: string
  sellingPrice: number
  minStockLevel: number
  totalStock: number
  batchCount: number
  isLowStock: boolean
  hasExpiryAlert: boolean
}

export interface StockImportDto {
  id: string
  supplierId: string
  supplierName: string
  invoiceNumber: string | null
  importDate: string
  totalAmount: number
  lineCount: number
  importSource: number
}

export interface StockImportLineDto {
  drugCatalogItemId: string
  drugName: string
  batchNumber: string
  expiryDate: string
  quantity: number
  purchasePrice: number
}

export interface ExpiryAlertDto {
  drugCatalogItemId: string
  drugName: string
  batchNumber: string
  expiryDate: string
  currentQuantity: number
  daysUntilExpiry: number
}

export interface LowStockAlertDto {
  drugCatalogItemId: string
  drugName: string
  totalStock: number
  minStockLevel: number
}

export interface PendingPrescriptionItemDto {
  prescriptionItemId: string
  drugCatalogItemId: string | null
  drugName: string
  quantity: number
  unit: string
  dosage: string | null
  isOffCatalog: boolean
}

export interface PendingPrescriptionDto {
  prescriptionId: string
  visitId: string
  patientId: string
  patientName: string
  prescriptionCode: string | null
  prescribedAt: string
  daysRemaining: number
  isExpired: boolean
  itemCount: number
  items: PendingPrescriptionItemDto[]
}

export interface DispensingRecordDto {
  id: string
  prescriptionId: string
  patientName: string
  dispensedAt: string
  dispensedByName: string
  lineCount: number
}

export interface OtcSaleDto {
  id: string
  patientId: string | null
  customerName: string | null
  soldAt: string
  notes: string | null
  lines: OtcSaleLineDto[]
}

export interface OtcSaleLineDto {
  id: string
  drugCatalogItemId: string
  drugName: string
  quantity: number
  unitPrice: number
}

export interface OtcSalesPagedResult {
  items: OtcSaleDto[]
  totalCount: number
}

export interface PendingCountDto {
  count: number
}

// -- Request types --

export interface CreateSupplierInput {
  name: string
  contactInfo?: string | null
}

export interface UpdateSupplierInput {
  name: string
  contactInfo?: string | null
  isActive?: boolean
}

export interface UpdateDrugPricingInput {
  sellingPrice: number
  minStockLevel: number
}

export interface AdjustStockInput {
  drugBatchId: string
  quantityChange: number
  reason: number
  notes?: string | null
}

export interface CreateStockImportInput {
  supplierId: string
  invoiceNumber?: string | null
  importDate: string
  lines: StockImportLineDto[]
}

export interface DispenseDrugsInput {
  prescriptionId: string
  visitId: string
  patientId: string
  patientName: string
  prescribedAt: string
  overrideReason?: string | null
  lines: DispenseLineInput[]
}

export interface DispenseLineInput {
  prescriptionItemId: string
  drugCatalogItemId: string | null
  drugName: string
  quantity: number
  isOffCatalog: boolean
  skip: boolean
  manualBatches?: BatchOverride[] | null
}

export interface BatchOverride {
  batchId: string
  quantity: number
}

export interface CreateOtcSaleInput {
  patientId?: string | null
  customerName?: string | null
  lines: OtcSaleLineInput[]
}

export interface OtcSaleLineInput {
  drugCatalogItemId: string
  drugName: string
  quantity: number
  unitPrice: number
}

export interface ExcelImportErrorDto {
  rowNumber: number
  columnName: string
  message: string
}

export interface ExcelImportPreviewDto {
  validLines: StockImportLineDto[]
  errors: ExcelImportErrorDto[]
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

// -- API functions --

const API_URL = (import.meta as never as { env: Record<string, string> }).env?.VITE_API_URL ?? "http://localhost:5255"

// Drug catalog

async function getDrugCatalogList(): Promise<DrugCatalogItemDto[]> {
  const { data, error } = await api.GET("/api/pharmacy/drugs" as never)
  if (error) throw new Error("Failed to fetch drug catalog")
  return (data as DrugCatalogItemDto[]) ?? []
}

async function searchDrugCatalog(term: string): Promise<DrugCatalogItemDto[]> {
  const { data, error } = await api.GET("/api/pharmacy/drugs/search" as never, {
    params: { query: { term } },
  } as never)
  if (error) throw new Error("Failed to search drug catalog")
  return (data as DrugCatalogItemDto[]) ?? []
}

async function createDrugCatalogItem(
  command: Omit<DrugCatalogItemDto, "id" | "isActive">,
): Promise<{ id: string }> {
  const { data, error, response } = await api.POST("/api/pharmacy/drugs" as never, {
    body: command,
  } as never)
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
  const { error, response } = await api.PUT(`/api/pharmacy/drugs/${id}` as never, {
    body: { id, ...command },
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update drug catalog item")
  }
}

// Suppliers

export async function getSuppliers(): Promise<SupplierDto[]> {
  const { data, error } = await api.GET("/api/pharmacy/suppliers" as never)
  if (error) throw new Error("Failed to fetch suppliers")
  return (data as SupplierDto[]) ?? []
}

export async function createSupplier(input: CreateSupplierInput): Promise<{ id: string }> {
  const { data, error, response } = await api.POST("/api/pharmacy/suppliers" as never, {
    body: input,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create supplier")
  }
  return data as { id: string }
}

export async function updateSupplier(id: string, input: UpdateSupplierInput): Promise<void> {
  const { error, response } = await api.PUT(`/api/pharmacy/suppliers/${id}` as never, {
    body: { id, ...input },
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update supplier")
  }
}

// Inventory

export async function getDrugInventory(): Promise<DrugInventoryDto[]> {
  const { data, error } = await api.GET("/api/pharmacy/inventory" as never)
  if (error) throw new Error("Failed to fetch drug inventory")
  return (data as DrugInventoryDto[]) ?? []
}

export async function getDrugBatches(drugId: string): Promise<DrugBatchDto[]> {
  const { data, error } = await api.GET(`/api/pharmacy/inventory/${drugId}/batches` as never)
  if (error) throw new Error("Failed to fetch drug batches")
  return (data as DrugBatchDto[]) ?? []
}

export async function updateDrugPricing(
  drugId: string,
  input: UpdateDrugPricingInput,
): Promise<void> {
  const { error, response } = await api.PUT(
    `/api/pharmacy/inventory/${drugId}/pricing` as never,
    { body: { drugId, ...input } } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update drug pricing")
  }
}

export async function adjustStock(input: AdjustStockInput): Promise<void> {
  const { error, response } = await api.POST("/api/pharmacy/inventory/adjust" as never, {
    body: input,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to adjust stock")
  }
}

// Stock imports

export async function getStockImports(
  page: number = 1,
  pageSize: number = 20,
): Promise<StockImportDto[]> {
  const { data, error } = await api.GET("/api/pharmacy/stock-imports" as never, {
    params: { query: { page, pageSize } },
  } as never)
  if (error) throw new Error("Failed to fetch stock imports")
  return (data as StockImportDto[]) ?? []
}

export async function createStockImport(
  input: CreateStockImportInput,
): Promise<{ id: string }> {
  const { data, error, response } = await api.POST("/api/pharmacy/stock-imports" as never, {
    body: input,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create stock import")
  }
  return data as { id: string }
}

export async function importStockFromExcel(
  file: File,
  supplierId: string,
): Promise<ExcelImportPreviewDto> {
  const formData = new FormData()
  formData.append("file", file)
  formData.append("supplierId", supplierId)

  const token = (await import("@/shared/stores/authStore")).useAuthStore.getState().accessToken
  const res = await fetch(`${API_URL}/api/pharmacy/stock-imports/excel`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: formData,
    credentials: "include",
  })
  if (!res.ok) {
    const body = await res.json().catch(() => null)
    throw new Error(body?.detail || body?.title || "Failed to import stock from Excel")
  }
  return res.json()
}

// Alerts

export async function getExpiryAlerts(days?: number): Promise<ExpiryAlertDto[]> {
  const params = days != null ? { params: { query: { days } } } : {}
  const { data, error } = await api.GET(
    "/api/pharmacy/alerts/expiry" as never,
    params as never,
  )
  if (error) throw new Error("Failed to fetch expiry alerts")
  return (data as ExpiryAlertDto[]) ?? []
}

export async function getLowStockAlerts(): Promise<LowStockAlertDto[]> {
  const { data, error } = await api.GET("/api/pharmacy/alerts/low-stock" as never)
  if (error) throw new Error("Failed to fetch low stock alerts")
  return (data as LowStockAlertDto[]) ?? []
}

// Dispensing

export async function getPendingPrescriptions(patientId?: string | null): Promise<PendingPrescriptionDto[]> {
  const params = patientId ? { params: { query: { patientId } } } : {}
  const { data, error } = await api.GET("/api/pharmacy/dispensing/pending" as never, params as never)
  if (error) throw new Error("Failed to fetch pending prescriptions")
  return (data as PendingPrescriptionDto[]) ?? []
}

export async function getPendingCount(): Promise<number> {
  const { data, error } = await api.GET("/api/pharmacy/dispensing/pending/count" as never)
  if (error) throw new Error("Failed to fetch pending count")
  return ((data as PendingCountDto)?.count) ?? 0
}

export async function dispenseDrugs(input: DispenseDrugsInput): Promise<{ id: string }> {
  const { data, error, response } = await api.POST("/api/pharmacy/dispensing" as never, {
    body: input,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to dispense drugs")
  }
  return data as { id: string }
}

export async function getDispensingHistory(
  page: number = 1,
  pageSize: number = 20,
  patientId?: string | null,
): Promise<DispensingHistoryResult> {
  const queryParams: Record<string, unknown> = { page, pageSize }
  if (patientId) queryParams.patientId = patientId
  const { data, error } = await api.GET("/api/pharmacy/dispensing/history" as never, {
    params: { query: queryParams },
  } as never)
  if (error) throw new Error("Failed to fetch dispensing history")
  // Backend returns { items, totalCount }
  const result = data as { items: DispensingRecordDto[]; totalCount: number } | null
  if (result && "items" in result) {
    return { items: result.items ?? [], totalCount: result.totalCount ?? 0 }
  }
  return { items: (data as DispensingRecordDto[]) ?? [], totalCount: 0 }
}

export interface DispensingHistoryResult {
  items: DispensingRecordDto[]
  totalCount: number
}

// OTC sales

export async function createOtcSale(input: CreateOtcSaleInput): Promise<{ id: string }> {
  const { data, error, response } = await api.POST("/api/pharmacy/otc-sales" as never, {
    body: input,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create OTC sale")
  }
  return data as { id: string }
}

export async function getOtcSales(
  page: number = 1,
  pageSize: number = 20,
): Promise<OtcSaleDto[]> {
  const { data, error } = await api.GET("/api/pharmacy/otc-sales" as never, {
    params: { query: { page, pageSize } },
  } as never)
  if (error) throw new Error("Failed to fetch OTC sales")
  const result = data as OtcSalesPagedResult | OtcSaleDto[] | undefined
  if (result && "items" in result) return result.items
  return (result as OtcSaleDto[]) ?? []
}

// Re-export drug catalog functions for backward compatibility
export {
  getDrugCatalogList,
  searchDrugCatalog,
  createDrugCatalogItem,
  updateDrugCatalogItem,
}
