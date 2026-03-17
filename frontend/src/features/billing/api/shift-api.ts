import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { api } from "@/shared/lib/api-client"
import { useAuthStore } from "@/shared/stores/authStore"
import { billingKeys } from "./billing-api"

// -- Types matching backend Billing.Contracts.Dtos --

export interface CashierShiftDto {
  id: string
  cashierId: string
  cashierName: string
  shiftTemplateId: string | null
  status: number
  openedAt: string
  closedAt: string | null
  openingBalance: number
  expectedCashAmount: number
  cashReceived: number
  cashRefunds: number
  actualCashCount: number | null
  discrepancy: number | null
  managerNote: string | null
  totalRevenue: number
  transactionCount: number
}

export interface ShiftReportDto {
  shiftId: string
  cashierName: string
  openedAt: string
  closedAt: string | null
  revenueByMethod: Record<string, number>
  transactionCount: number
  openingBalance: number
  cashReceived: number
  cashRefunds: number
  expectedCash: number
  actualCash: number | null
  discrepancy: number | null
  managerNote: string | null
}

export interface ShiftTemplateDto {
  id: string
  name: string
  nameVi: string | null
  defaultStartTime: string
  defaultEndTime: string
  isActive: boolean
}

// -- Request types --

export interface OpenShiftInput {
  shiftTemplateId?: string | null
  openingBalance: number
}

export interface CloseShiftInput {
  actualCashCount: number
  managerNote?: string | null
}

// -- Shift status enum map --

export const SHIFT_STATUS_MAP: Record<number, string> = {
  0: "Open",
  1: "Closed",
}

// -- Query key factory --

export interface ShiftHistoryResult {
  items: CashierShiftDto[]
  totalCount: number
}

export const shiftKeys = {
  all: ["shifts"] as const,
  current: () => [...shiftKeys.all, "current"] as const,
  list: (page?: number) =>
    page != null
      ? ([...shiftKeys.all, "list", page] as const)
      : ([...shiftKeys.all, "list"] as const),
  report: (id: string) => [...shiftKeys.all, "report", id] as const,
  templates: () => [...shiftKeys.all, "templates"] as const,
}

// -- API functions --

async function getCurrentShift(): Promise<CashierShiftDto | null> {
  const { data, error, response } = await api.GET(
    "/api/billing/shifts/current" as never,
  )
  if (response.status === 404) return null
  if (error) throw new Error("Failed to fetch current shift")
  return data as CashierShiftDto
}

async function openShift(command: OpenShiftInput): Promise<{ id: string }> {
  const { data, error, response } = await api.POST(
    "/api/billing/shifts/open" as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to open shift")
  }
  return data as { id: string }
}

async function closeShift(
  _shiftId: string,
  command: CloseShiftInput,
): Promise<void> {
  const { error, response } = await api.POST(
    "/api/billing/shifts/close" as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to close shift")
  }
}

async function getShiftReport(shiftId: string): Promise<ShiftReportDto> {
  const { data, error } = await api.GET(
    `/api/billing/shifts/${shiftId}/report` as never,
  )
  if (error) throw new Error("Failed to fetch shift report")
  return data as ShiftReportDto
}

async function getShiftTemplates(): Promise<ShiftTemplateDto[]> {
  const { data, error } = await api.GET(
    "/api/billing/shifts/templates" as never,
  )
  if (error) throw new Error("Failed to fetch shift templates")
  return (data as ShiftTemplateDto[]) ?? []
}

async function getShiftHistory(
  page: number = 1,
  pageSize: number = 20,
): Promise<ShiftHistoryResult> {
  const { data, error } = await api.GET("/api/billing/shifts" as never, {
    params: { query: { page, pageSize } },
  } as never)
  if (error) throw new Error("Failed to fetch shift history")
  const result = data as { items: CashierShiftDto[]; totalCount: number } | null
  if (result && "items" in result) {
    return { items: result.items ?? [], totalCount: result.totalCount ?? 0 }
  }
  return { items: (data as CashierShiftDto[]) ?? [], totalCount: 0 }
}

// -- PDF / Print functions (native fetch for blob responses) --

const API_URL =
  (import.meta as never as { env: Record<string, string> }).env?.VITE_API_URL ??
  "http://localhost:5255"

async function fetchPdf(url: string): Promise<Blob> {
  const token = useAuthStore.getState().accessToken
  const res = await fetch(url, {
    headers: { Authorization: `Bearer ${token}` },
    credentials: "include",
  })
  if (!res.ok) {
    throw new Error("Failed to generate PDF")
  }
  return res.blob()
}

export async function getInvoicePdf(invoiceId: string): Promise<Blob> {
  return fetchPdf(`${API_URL}/api/billing/print/${invoiceId}/invoice`)
}

export async function getReceiptPdf(invoiceId: string): Promise<Blob> {
  return fetchPdf(`${API_URL}/api/billing/print/${invoiceId}/receipt`)
}

export async function getEInvoicePdf(invoiceId: string): Promise<Blob> {
  return fetchPdf(`${API_URL}/api/billing/print/${invoiceId}/e-invoice`)
}

export async function getShiftReportPdf(shiftId: string): Promise<Blob> {
  return fetchPdf(`${API_URL}/api/billing/shifts/${shiftId}/report/pdf`)
}

// -- E-Invoice export functions (JSON/XML for MISA import) --

async function fetchExport(url: string): Promise<Blob> {
  const token = useAuthStore.getState().accessToken
  const res = await fetch(url, {
    headers: { Authorization: `Bearer ${token}` },
    credentials: "include",
  })
  if (!res.ok) {
    throw new Error("Failed to export e-invoice")
  }
  return res.blob()
}

export async function exportEInvoiceJson(invoiceId: string): Promise<Blob> {
  return fetchExport(
    `${API_URL}/api/billing/invoices/${invoiceId}/export/e-invoice.json`,
  )
}

export async function exportEInvoiceXml(invoiceId: string): Promise<Blob> {
  return fetchExport(
    `${API_URL}/api/billing/invoices/${invoiceId}/export/e-invoice.xml`,
  )
}

// -- TanStack Query hooks --

export function useCurrentShift() {
  return useQuery({
    queryKey: shiftKeys.current(),
    queryFn: getCurrentShift,
    refetchInterval: 60_000,
    refetchIntervalInBackground: false,
  })
}

export function useShiftReport(shiftId: string | undefined) {
  return useQuery({
    queryKey: shiftKeys.report(shiftId ?? ""),
    queryFn: () => getShiftReport(shiftId!),
    enabled: !!shiftId,
  })
}

export function useShiftTemplates() {
  return useQuery({
    queryKey: shiftKeys.templates(),
    queryFn: getShiftTemplates,
  })
}

export function useShiftHistory(page: number = 1) {
  return useQuery({
    queryKey: shiftKeys.list(page),
    queryFn: () => getShiftHistory(page),
  })
}

export function useOpenShift() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: OpenShiftInput) => openShift(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: shiftKeys.current() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useCloseShift() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      shiftId,
      ...command
    }: { shiftId: string } & CloseShiftInput) => closeShift(shiftId, command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: shiftKeys.current() })
      queryClient.invalidateQueries({
        queryKey: shiftKeys.report(variables.shiftId),
      })
      // Refresh billing data linked to shift
      queryClient.invalidateQueries({ queryKey: billingKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

// Re-export API functions for direct use
export {
  getCurrentShift,
  openShift,
  closeShift,
  getShiftReport,
  getShiftTemplates,
  getShiftHistory,
}
