import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import {
  IconCash,
  IconBuildingBank,
  IconQrcode,
  IconCreditCard,
  IconReceipt,
} from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { Separator } from "@/shared/components/Separator"
import { Skeleton } from "@/shared/components/Skeleton"
import { formatVND } from "@/shared/lib/format-vnd"
import { useInvoice } from "@/features/billing/api/billing-api"
import {
  PAYMENT_METHOD_MAP,
  PAYMENT_STATUS_MAP,
  APPROVAL_STATUS_MAP,
  DISCOUNT_TYPE_MAP,
} from "@/features/billing/api/billing-api"
import type { PaymentDto, DiscountDto } from "@/features/billing/api/billing-api"
import { InvoiceLineItemsTable } from "./InvoiceLineItemsTable"

interface InvoiceViewProps {
  invoiceId: string
}

const STATUS_VARIANT: Record<number, "default" | "secondary" | "destructive" | "outline"> = {
  0: "secondary",  // Draft = yellow-ish
  1: "default",    // Finalized = green
  2: "destructive", // Voided = red
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
  const { data: invoice, isLoading } = useInvoice(invoiceId)

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
        <Badge variant={STATUS_VARIANT[invoice.status] ?? "outline"}>
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
          <InvoiceLineItemsTable lineItems={invoice.lineItems} />
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

      {/* Action buttons placeholder area (for payment, discount, finalize -- implemented in later plans) */}
      <div className="flex gap-2">
        {/* Placeholder: action buttons will be added in plans 07-19, 07-20, 07-21 */}
      </div>
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
  const MethodIcon = PAYMENT_METHOD_ICON[payment.method] ?? IconCash
  const methodName = PAYMENT_METHOD_MAP[payment.method] ?? "Unknown"
  const statusName = PAYMENT_STATUS_MAP[payment.status] ?? "Unknown"
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
  const typeName = DISCOUNT_TYPE_MAP[discount.type] ?? "Unknown"
  const statusName = APPROVAL_STATUS_MAP[discount.approvalStatus] ?? "Unknown"
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
