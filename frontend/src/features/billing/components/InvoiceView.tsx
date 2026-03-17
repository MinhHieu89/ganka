import { useState } from "react"
import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { toast } from "sonner"
import {
  IconCash,
  IconBuildingBank,
  IconQrcode,
  IconCreditCard,
  IconReceipt,
  IconPrinter,
  IconCheck,
  IconLoader2,
  IconDiscount2,
  IconReceiptRefund,
} from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Separator } from "@/shared/components/Separator"
import { Skeleton } from "@/shared/components/Skeleton"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/components/AlertDialog"
import { formatVND } from "@/shared/lib/format-vnd"
import { useBillingHub } from "@/features/billing/hooks/use-billing-hub"
import {
  useInvoice,
  useFinalizeInvoice,
  PAYMENT_METHOD_I18N_KEY,
  PAYMENT_STATUS_I18N_KEY,
  APPROVAL_STATUS_I18N_KEY,
  DISCOUNT_TYPE_I18N_KEY,
} from "@/features/billing/api/billing-api"
import type { PaymentDto, DiscountDto } from "@/features/billing/api/billing-api"
import { useCurrentShift, getInvoicePdf, getReceiptPdf } from "@/features/billing/api/shift-api"
import { InvoiceLineItemsTable } from "./InvoiceLineItemsTable"
import { PaymentForm } from "./PaymentForm"
import { DiscountDialog } from "./DiscountDialog"
import { RefundDialog } from "./RefundDialog"
import { EInvoiceExportButton } from "./EInvoiceExportButton"

interface InvoiceViewProps {
  invoiceId: string
}

const STATUS_VARIANT: Record<number, "default" | "secondary" | "destructive" | "outline"> = {
  0: "secondary",  // Draft
  1: "outline",    // Finalized (styled green via className)
  2: "destructive", // Voided = red
}

const STATUS_CLASS: Record<number, string> = {
  1: "border-green-600 bg-green-50 text-green-700 dark:bg-green-950 dark:text-green-400",
}

const PAYMENT_METHOD_ICON: Record<number, React.ComponentType<{ className?: string }>> = {
  0: IconCash,
  1: IconBuildingBank,
  2: IconQrcode,
  3: IconQrcode,
  4: IconQrcode,
  5: IconCreditCard,
  6: IconCreditCard,
}

export function InvoiceView({ invoiceId }: InvoiceViewProps) {
  const { t } = useTranslation("billing")
  useBillingHub() // Real-time updates when prescription items are added
  const { data: invoice, isLoading } = useInvoice(invoiceId)
  const { data: currentShift } = useCurrentShift()
  const finalizeInvoice = useFinalizeInvoice()

  // Dialog state
  const [paymentOpen, setPaymentOpen] = useState(false)
  const [discountOpen, setDiscountOpen] = useState(false)
  const [refundOpen, setRefundOpen] = useState(false)
  const [isPrinting, setIsPrinting] = useState(false)

  if (isLoading) {
    return <InvoiceViewSkeleton />
  }

  if (!invoice) {
    return (
      <div className="text-center py-12 text-muted-foreground">
        {t("noPendingInvoices")}
      </div>
    )
  }

  const statusKey = invoice.status === 0 ? "draft" : invoice.status === 1 ? "finalized" : "voided"
  const isDraft = invoice.status === 0
  const isFinalized = invoice.status === 1
  const hasBalance = invoice.balanceDue > 0
  const isFullyPaid = invoice.balanceDue <= 0
  const hasPayments = invoice.payments.length > 0

  const handlePrintInvoice = async () => {
    setIsPrinting(true)
    try {
      const blob = await getInvoicePdf(invoiceId)
      const url = URL.createObjectURL(blob)
      window.open(url, "_blank")
      setTimeout(() => URL.revokeObjectURL(url), 30000)
    } catch {
      toast.error(t("printError"))
    } finally {
      setIsPrinting(false)
    }
  }

  const handlePrintReceipt = async () => {
    setIsPrinting(true)
    try {
      const blob = await getReceiptPdf(invoiceId)
      const url = URL.createObjectURL(blob)
      window.open(url, "_blank")
      setTimeout(() => URL.revokeObjectURL(url), 30000)
    } catch {
      toast.error(t("printError"))
    } finally {
      setIsPrinting(false)
    }
  }

  const handleFinalize = () => {
    if (!currentShift?.id) {
      toast.error(t("noOpenShift"))
      return
    }
    finalizeInvoice.mutate(
      { invoiceId, cashierShiftId: currentShift.id },
      {
        onSuccess: () => {
          toast.success(t("invoiceFinalized"))
        },
        onError: (error: Error) => {
          if (error.message === "NO_LINE_ITEMS") {
            toast.error(t("errors.finalizeNoLineItems"))
          } else {
            toast.error(t("errors.finalizeFailed"))
          }
        },
      },
    )
  }

  return (
    <div className="space-y-6">
      {/* Header: Invoice number + Status */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <IconReceipt className="h-6 w-6 text-muted-foreground" />
          <div>
            <h2 className="text-xl font-bold">{invoice.invoiceNumber}</h2>
            <p className="text-sm text-muted-foreground">
              {format(new Date(invoice.createdAt), "dd/MM/yyyy HH:mm")}
            </p>
          </div>
        </div>
        <Badge
          variant={STATUS_VARIANT[invoice.status] ?? "outline"}
          className={STATUS_CLASS[invoice.status] ?? ""}
        >
          {t(`status.${statusKey}`)}
        </Badge>
      </div>

      {/* Patient info */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-sm font-medium text-muted-foreground">
            {t("patientName")}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="font-medium">{invoice.patientName}</p>
        </CardContent>
      </Card>

      {/* Line items grouped by department */}
      <Card>
        <CardHeader>
          <CardTitle>{t("invoices")}</CardTitle>
        </CardHeader>
        <CardContent>
          <InvoiceLineItemsTable
            lineItems={invoice.lineItems}
            invoiceId={invoiceId}
            isDraft={isDraft}
          />
        </CardContent>
      </Card>

      {/* Summary section */}
      <Card>
        <CardHeader>
          <CardTitle>{t("total")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          <SummaryRow label={t("subtotal")} value={formatVND(invoice.subTotal)} />
          {invoice.discountTotal > 0 && (
            <SummaryRow
              label={t("discount")}
              value={`-${formatVND(invoice.discountTotal)}`}
              className="text-orange-600"
            />
          )}
          <Separator />
          <SummaryRow
            label={t("total")}
            value={formatVND(invoice.totalAmount)}
            className="font-bold text-lg"
          />
          <SummaryRow label={t("paid")} value={formatVND(invoice.paidAmount)} />
          <Separator />
          <SummaryRow
            label={t("balanceDue")}
            value={formatVND(invoice.balanceDue)}
            className={`font-bold ${invoice.balanceDue > 0 ? "text-destructive" : "text-green-600"}`}
          />
        </CardContent>
      </Card>

      {/* Payments list */}
      <Card>
        <CardHeader>
          <CardTitle>
            {t("collectPayment")} ({invoice.payments.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {invoice.payments.length === 0 ? (
            <p className="text-sm text-muted-foreground">{t("noPayments")}</p>
          ) : (
            <div className="space-y-3">
              {invoice.payments.map((payment) => (
                <PaymentRow key={payment.id} payment={payment} />
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Discounts list */}
      {invoice.discounts.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>
              {t("discount")} ({invoice.discounts.length})
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {invoice.discounts.map((discount) => (
                <DiscountRow key={discount.id} discount={discount} />
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Action buttons */}
      <div className="flex flex-wrap gap-2">
        {/* Collect Payment -- only when Draft and has balance */}
        {isDraft && hasBalance && (
          <Button onClick={() => setPaymentOpen(true)}>
            <IconCash className="mr-2 h-4 w-4" />
            {t("collectPayment")}
          </Button>
        )}

        {/* Apply Discount -- only for Draft invoices */}
        {isDraft && (
          <Button
            variant="outline"
            onClick={() => setDiscountOpen(true)}
          >
            <IconDiscount2 className="mr-2 h-4 w-4" />
            {t("applyDiscount")}
          </Button>
        )}

        {/* Finalize Invoice -- only when Draft and fully paid */}
        {isDraft && isFullyPaid && (
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button variant="default">
                <IconCheck className="mr-2 h-4 w-4" />
                {t("finalize")}
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>{t("finalize")}</AlertDialogTitle>
                <AlertDialogDescription>
                  {t("finalizeConfirm")}
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>{t("buttons.cancel", { ns: "common" })}</AlertDialogCancel>
                <AlertDialogAction
                  onClick={handleFinalize}
                  disabled={finalizeInvoice.isPending}
                >
                  {finalizeInvoice.isPending && (
                    <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
                  )}
                  {t("buttons.confirm", { ns: "common" })}
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        )}

        {/* Request Refund -- only for Finalized invoices */}
        {isFinalized && (
          <Button
            variant="outline"
            onClick={() => setRefundOpen(true)}
          >
            <IconReceiptRefund className="mr-2 h-4 w-4" />
            {t("requestRefund")}
          </Button>
        )}

        {/* E-Invoice Export -- only for Finalized invoices */}
        {isFinalized && (
          <EInvoiceExportButton
            invoiceId={invoiceId}
            invoiceNumber={invoice.invoiceNumber}
          />
        )}

        {/* Print Invoice -- always available */}
        <Button
          variant="outline"
          onClick={handlePrintInvoice}
          disabled={isPrinting}
        >
          {isPrinting ? (
            <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
          ) : (
            <IconPrinter className="mr-2 h-4 w-4" />
          )}
          {t("printInvoice")}
        </Button>

        {/* Print Receipt -- visible when at least one payment exists */}
        {hasPayments && (
          <Button variant="outline" onClick={handlePrintReceipt} disabled={isPrinting}>
            <IconReceipt className="mr-2 h-4 w-4" />
            {t("printReceipt")}
          </Button>
        )}
      </div>

      {/* Payment Form Dialog */}
      <PaymentForm
        invoiceId={invoiceId}
        balanceDue={invoice.balanceDue}
        open={paymentOpen}
        onOpenChange={setPaymentOpen}
      />

      {/* Discount Dialog */}
      <DiscountDialog
        open={discountOpen}
        onOpenChange={setDiscountOpen}
        invoiceId={invoiceId}
        subTotal={invoice.subTotal}
        lineItems={invoice.lineItems}
      />

      {/* Refund Dialog */}
      <RefundDialog
        open={refundOpen}
        onOpenChange={setRefundOpen}
        invoiceId={invoiceId}
        totalAmount={invoice.totalAmount}
        lineItems={invoice.lineItems}
      />
    </div>
  )
}

function SummaryRow({
  label,
  value,
  className,
}: {
  label: string
  value: string
  className?: string
}) {
  return (
    <div className={`flex items-center justify-between ${className ?? ""}`}>
      <span>{label}</span>
      <span>{value}</span>
    </div>
  )
}

function PaymentRow({ payment }: { payment: PaymentDto }) {
  const { t } = useTranslation("billing")
  const MethodIcon = PAYMENT_METHOD_ICON[payment.method] ?? IconCash
  const methodName = t(PAYMENT_METHOD_I18N_KEY[payment.method] ?? "paymentMethods.cash")
  const statusName = t(PAYMENT_STATUS_I18N_KEY[payment.status] ?? "paymentStatus.pending")
  const statusVariant =
    payment.status === 1 ? "default" : payment.status === 2 ? "destructive" : "secondary"

  return (
    <div className="flex items-center justify-between border rounded-lg p-3">
      <div className="flex items-center gap-3">
        <MethodIcon className="h-5 w-5 text-muted-foreground" />
        <div>
          <p className="font-medium text-sm">{methodName}</p>
          {payment.referenceNumber && (
            <p className="text-xs text-muted-foreground">
              Ref: {payment.referenceNumber}
            </p>
          )}
          <p className="text-xs text-muted-foreground">
            {format(new Date(payment.recordedAt), "dd/MM/yyyy HH:mm")}
          </p>
        </div>
      </div>
      <div className="flex items-center gap-3">
        <Badge variant={statusVariant}>{statusName}</Badge>
        <span className="font-medium">{formatVND(payment.amount)}</span>
      </div>
    </div>
  )
}

function DiscountRow({ discount }: { discount: DiscountDto }) {
  const { t } = useTranslation("billing")
  const typeName = t(DISCOUNT_TYPE_I18N_KEY[discount.type] ?? "discountTypes.percentage")
  const statusName = t(APPROVAL_STATUS_I18N_KEY[discount.approvalStatus] ?? "approvalStatus.pending")
  const statusVariant =
    discount.approvalStatus === 1
      ? "default"
      : discount.approvalStatus === 2
        ? "destructive"
        : "secondary"

  return (
    <div className="flex items-center justify-between border rounded-lg p-3">
      <div>
        <p className="font-medium text-sm">
          {typeName}: {discount.type === 0 ? `${discount.value}%` : formatVND(discount.value)}
        </p>
        {discount.reason && (
          <p className="text-xs text-muted-foreground">{discount.reason}</p>
        )}
      </div>
      <div className="flex items-center gap-3">
        <Badge variant={statusVariant}>{statusName}</Badge>
        <span className="font-medium text-orange-600">
          -{formatVND(discount.calculatedAmount)}
        </span>
      </div>
    </div>
  )
}

function InvoiceViewSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="space-y-2">
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-32" />
        </div>
        <Skeleton className="h-6 w-20" />
      </div>
      <Skeleton className="h-24 w-full" />
      <Skeleton className="h-64 w-full" />
      <Skeleton className="h-40 w-full" />
      <Skeleton className="h-32 w-full" />
    </div>
  )
}
