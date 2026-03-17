import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { api } from "@/shared/lib/api-client"
import { shiftKeys } from "./shift-api"

// -- Types matching backend Billing.Contracts.Dtos --

export interface InvoiceLineItemDto {
  id: string
  description: string
  descriptionVi: string | null
  unitPrice: number
  quantity: number
  lineTotal: number
  department: number
  sourceId: string | null
  sourceType: string | null
}

export interface PaymentDto {
  id: string
  invoiceId: string
  method: number
  amount: number
  status: number
  referenceNumber: string | null
  cardLast4: string | null
  cardType: string | null
  notes: string | null
  recordedById: string
  recordedAt: string
  cashierShiftId: string | null
  treatmentPackageId: string | null
  isSplitPayment: boolean
  splitSequence: number | null
}

export interface DiscountDto {
  id: string
  invoiceLineItemId: string | null
  type: number
  value: number
  calculatedAmount: number
  reason: string
  approvalStatus: number
  requestedById: string
  requestedAt: string
  approvedById: string | null
  approvedAt: string | null
}

export interface RefundDto {
  id: string
  invoiceLineItemId: string | null
  amount: number
  reason: string
  status: number
  requestedById: string
  requestedAt: string
  approvedById: string | null
  approvedAt: string | null
  processedById: string | null
  processedAt: string | null
}

export interface InvoiceDto {
  id: string
  invoiceNumber: string
  visitId: string | null
  patientId: string
  patientName: string
  status: number
  subTotal: number
  discountTotal: number
  totalAmount: number
  paidAmount: number
  balanceDue: number
  cashierShiftId: string | null
  finalizedById: string | null
  finalizedAt: string | null
  createdAt: string
  lineItems: InvoiceLineItemDto[]
  payments: PaymentDto[]
  discounts: DiscountDto[]
  refunds: RefundDto[]
}

export interface InvoiceSummaryDto {
  id: string
  invoiceNumber: string
  patientName: string
  status: number
  totalAmount: number
  paidAmount: number
  balanceDue: number
  createdAt: string
}

// -- Request types --

export interface CreateInvoiceInput {
  visitId: string | null
  patientId: string
  patientName: string
}

export interface AddLineItemInput {
  description: string
  descriptionVi?: string | null
  unitPrice: number
  quantity: number
  department: number
  sourceId?: string | null
  sourceType?: string | null
}

export interface RecordPaymentInput {
  invoiceId: string
  method: number
  amount: number
  referenceNumber?: string | null
  cardLast4?: string | null
  cardType?: string | null
  notes?: string | null
  cashierShiftId?: string | null
  treatmentPackageId?: string | null
  isSplitPayment?: boolean
  splitSequence?: number | null
}

export interface ApplyDiscountInput {
  invoiceId: string
  invoiceLineItemId?: string | null
  discountType: number
  value: number
  reason: string
}

export interface ApproveDiscountInput {
  invoiceId: string
  managerId: string
  managerPin: string
}

export interface RequestRefundInput {
  invoiceId: string
  invoiceLineItemId?: string | null
  amount: number
  reason: string
}

export interface ApproveRefundInput {
  managerPin: string
}

export interface FinalizeInvoiceInput {
  cashierShiftId: string
}

// -- Enum i18n key maps (CR-19) --
// Values are i18n keys in the "billing" namespace. Use with t(key) for localized display.

export const PAYMENT_METHOD_I18N_KEY: Record<number, string> = {
  0: "paymentMethods.cash",
  1: "paymentMethods.bankTransfer",
  2: "paymentMethods.qrVnpay",
  3: "paymentMethods.qrMomo",
  4: "paymentMethods.qrZalopay",
  5: "paymentMethods.cardVisa",
  6: "paymentMethods.cardMc",
}

export const DEPARTMENT_I18N_KEY: Record<number, string> = {
  0: "departments.medical",
  1: "departments.pharmacy",
  2: "departments.optical",
  3: "departments.treatment",
}

export const INVOICE_STATUS_I18N_KEY: Record<number, string> = {
  0: "status.draft",
  1: "status.finalized",
  2: "status.voided",
}

export const PAYMENT_STATUS_I18N_KEY: Record<number, string> = {
  0: "paymentStatus.pending",
  1: "paymentStatus.confirmed",
  2: "paymentStatus.refunded",
}

export const DISCOUNT_TYPE_I18N_KEY: Record<number, string> = {
  0: "discountTypes.percentage",
  1: "discountTypes.fixedAmount",
}

export const APPROVAL_STATUS_I18N_KEY: Record<number, string> = {
  0: "approvalStatus.pending",
  1: "approvalStatus.approved",
  2: "approvalStatus.rejected",
}

export const REFUND_STATUS_I18N_KEY: Record<number, string> = {
  0: "refundStatus.requested",
  1: "refundStatus.approved",
  2: "refundStatus.processed",
  3: "refundStatus.rejected",
}

// -- Query key factory --

export const billingKeys = {
  all: ["billing"] as const,
  invoices: () => [...billingKeys.all, "invoices"] as const,
  pendingInvoices: () => [...billingKeys.all, "invoices", "pending"] as const,
  invoice: (id: string) => [...billingKeys.all, "invoice", id] as const,
  visitInvoice: (visitId: string) => [...billingKeys.all, "visit", visitId] as const,
  pendingApprovals: () => [...billingKeys.all, "pendingApprovals"] as const,
  paymentsByInvoice: (invoiceId: string) =>
    [...billingKeys.all, "payments", invoiceId] as const,
}

// -- API functions --

async function getInvoiceById(id: string): Promise<InvoiceDto> {
  const { data, error } = await api.GET(`/api/billing/invoices/${id}` as never)
  if (error) throw new Error("Failed to fetch invoice")
  return data as InvoiceDto
}

async function getInvoiceByVisit(visitId: string): Promise<InvoiceDto | null> {
  const { data, error, response } = await api.GET(
    `/api/billing/invoices/by-visit/${visitId}` as never,
  )
  if (response.status === 404) return null
  if (error) throw new Error("Failed to fetch invoice for visit")
  return data as InvoiceDto
}

async function getPendingInvoices(): Promise<InvoiceSummaryDto[]> {
  const { data, error } = await api.GET(
    "/api/billing/invoices/pending" as never,
  )
  if (error) throw new Error("Failed to fetch pending invoices")
  return (data as InvoiceSummaryDto[]) ?? []
}

async function createInvoice(command: CreateInvoiceInput): Promise<{ id: string }> {
  const { data, error, response } = await api.POST(
    "/api/billing/invoices" as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create invoice")
  }
  return data as { id: string }
}

async function addLineItem(
  invoiceId: string,
  command: AddLineItemInput,
): Promise<{ id: string }> {
  const { data, error, response } = await api.POST(
    `/api/billing/invoices/${invoiceId}/line-items` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to add line item")
  }
  return data as { id: string }
}

async function removeLineItem(
  invoiceId: string,
  lineItemId: string,
): Promise<void> {
  const { error, response } = await api.DELETE(
    `/api/billing/invoices/${invoiceId}/line-items/${lineItemId}` as never,
    {} as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to remove line item")
  }
}

async function finalizeInvoice(
  invoiceId: string,
  command: FinalizeInvoiceInput,
): Promise<void> {
  const { error, response } = await api.POST(
    `/api/billing/invoices/${invoiceId}/finalize` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    const detail = (err?.detail as string) ?? ""
    if (detail.toLowerCase().includes("no line items") || detail.toLowerCase().includes("at least one"))
      throw new Error("NO_LINE_ITEMS")
    throw new Error("FINALIZE_FAILED")
  }
}

async function recordPayment(command: RecordPaymentInput): Promise<{ id: string }> {
  const { data, error, response } = await api.POST(
    "/api/billing/payments" as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to record payment")
  }
  return data as { id: string }
}

async function getPaymentsByInvoice(invoiceId: string): Promise<PaymentDto[]> {
  const { data, error } = await api.GET(
    `/api/billing/payments/by-invoice/${invoiceId}` as never,
  )
  if (error) throw new Error("Failed to fetch payments")
  return (data as PaymentDto[]) ?? []
}

async function applyDiscount(command: ApplyDiscountInput): Promise<{ id: string }> {
  const { data, error, response } = await api.POST(
    "/api/billing/discounts" as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to apply discount")
  }
  return data as { id: string }
}

async function approveDiscount(
  discountId: string,
  command: ApproveDiscountInput,
): Promise<void> {
  const { error, response } = await api.POST(
    `/api/billing/discounts/${discountId}/approve` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to approve discount")
  }
}

async function requestRefund(command: RequestRefundInput): Promise<{ id: string }> {
  const { data, error, response } = await api.POST(
    "/api/billing/refunds" as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to request refund")
  }
  return data as { id: string }
}

async function approveRefund(
  refundId: string,
  command: ApproveRefundInput,
): Promise<void> {
  const { error, response } = await api.POST(
    `/api/billing/refunds/${refundId}/approve` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to approve refund")
  }
}

async function processRefund(refundId: string): Promise<void> {
  const { error, response } = await api.POST(
    `/api/billing/refunds/${refundId}/process` as never,
    {} as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to process refund")
  }
}

async function getPendingApprovals(): Promise<{
  discounts: DiscountDto[]
  refunds: RefundDto[]
}> {
  const { data, error } = await api.GET(
    "/api/billing/approvals/pending" as never,
  )
  if (error)
    throw new Error("Failed to fetch pending approvals")
  return (data as { discounts: DiscountDto[]; refunds: RefundDto[] }) ?? {
    discounts: [],
    refunds: [],
  }
}

// -- TanStack Query hooks --

export function useVisitInvoice(visitId: string | undefined) {
  return useQuery({
    queryKey: billingKeys.visitInvoice(visitId ?? ""),
    queryFn: () => getInvoiceByVisit(visitId!),
    enabled: !!visitId,
  })
}

export function useInvoice(invoiceId: string | undefined) {
  return useQuery({
    queryKey: billingKeys.invoice(invoiceId ?? ""),
    queryFn: () => getInvoiceById(invoiceId!),
    enabled: !!invoiceId,
  })
}

export function usePendingInvoices() {
  return useQuery({
    queryKey: billingKeys.pendingInvoices(),
    queryFn: getPendingInvoices,
    refetchInterval: 30_000,
    refetchIntervalInBackground: false,
  })
}

export function usePaymentsByInvoice(invoiceId: string | undefined) {
  return useQuery({
    queryKey: billingKeys.paymentsByInvoice(invoiceId ?? ""),
    queryFn: () => getPaymentsByInvoice(invoiceId!),
    enabled: !!invoiceId,
  })
}

export function usePendingApprovals() {
  return useQuery({
    queryKey: billingKeys.pendingApprovals(),
    queryFn: getPendingApprovals,
    refetchInterval: 30_000,
    refetchIntervalInBackground: false,
  })
}

export function useCreateInvoice() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: CreateInvoiceInput) => createInvoice(command),
    onSuccess: (_data, variables) => {
      if (variables.visitId) {
        queryClient.invalidateQueries({
          queryKey: billingKeys.visitInvoice(variables.visitId),
        })
      }
      queryClient.invalidateQueries({ queryKey: billingKeys.invoices() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useAddLineItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      invoiceId,
      ...command
    }: { invoiceId: string } & AddLineItemInput) =>
      addLineItem(invoiceId, command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: billingKeys.invoice(variables.invoiceId),
      })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useRemoveLineItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      invoiceId,
      lineItemId,
    }: {
      invoiceId: string
      lineItemId: string
    }) => removeLineItem(invoiceId, lineItemId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: billingKeys.invoice(variables.invoiceId),
      })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useRecordPayment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: RecordPaymentInput) => recordPayment(command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: billingKeys.invoice(variables.invoiceId),
      })
      queryClient.invalidateQueries({
        queryKey: billingKeys.paymentsByInvoice(variables.invoiceId),
      })
      // Also invalidate shift data since payment affects shift totals
      queryClient.invalidateQueries({ queryKey: shiftKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useApplyDiscount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: ApplyDiscountInput) => applyDiscount(command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: billingKeys.invoice(variables.invoiceId),
      })
      queryClient.invalidateQueries({
        queryKey: billingKeys.pendingApprovals(),
      })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useApproveDiscount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      discountId,
      ...command
    }: { discountId: string } & ApproveDiscountInput) =>
      approveDiscount(discountId, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: billingKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useRequestRefund() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: RequestRefundInput) => requestRefund(command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: billingKeys.invoice(variables.invoiceId),
      })
      queryClient.invalidateQueries({
        queryKey: billingKeys.pendingApprovals(),
      })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useApproveRefund() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      refundId,
      ...command
    }: { refundId: string } & ApproveRefundInput) =>
      approveRefund(refundId, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: billingKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useProcessRefund() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ refundId }: { refundId: string }) =>
      processRefund(refundId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: billingKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useFinalizeInvoice() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      invoiceId,
      ...command
    }: { invoiceId: string } & FinalizeInvoiceInput) =>
      finalizeInvoice(invoiceId, command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: billingKeys.invoice(variables.invoiceId),
      })
      queryClient.invalidateQueries({ queryKey: billingKeys.invoices() })
      // Refresh shift data since finalization links invoice to shift
      queryClient.invalidateQueries({ queryKey: shiftKeys.all })
    },
  })
}

// Re-export API functions for direct use
export {
  getInvoiceById,
  getInvoiceByVisit,
  getPendingInvoices,
  createInvoice,
  addLineItem,
  removeLineItem,
  finalizeInvoice,
  recordPayment,
  getPaymentsByInvoice,
  applyDiscount,
  approveDiscount,
  requestRefund,
  approveRefund,
  processRefund,
  getPendingApprovals,
}
