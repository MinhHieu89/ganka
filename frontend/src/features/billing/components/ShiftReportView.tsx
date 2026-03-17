import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { IconPrinter, IconClock, IconUser } from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Separator } from "@/shared/components/Separator"
import { Skeleton } from "@/shared/components/Skeleton"
import { Button } from "@/shared/components/Button"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import { formatVND } from "@/shared/lib/format-vnd"
import { cn } from "@/shared/lib/utils"
import { useShiftReport, getShiftReportPdf } from "../api/shift-api"
import { PAYMENT_METHOD_I18N_KEY } from "../api/billing-api"

interface ShiftReportViewProps {
  shiftId: string
}

export function ShiftReportView({ shiftId }: ShiftReportViewProps) {
  const { t } = useTranslation("billing")
  const { data: report, isLoading } = useShiftReport(shiftId)

  if (isLoading) {
    return <ShiftReportSkeleton />
  }

  if (!report) {
    return (
      <div className="text-center py-12 text-muted-foreground">
        {t("shiftReport")} not found
      </div>
    )
  }

  const handlePrint = async () => {
    try {
      const blob = await getShiftReportPdf(shiftId)
      const url = URL.createObjectURL(blob)
      window.open(url, "_blank")
    } catch {
      // Error is handled by the API function
    }
  }

  // Parse revenue by method from the report
  // Backend sends revenueByMethod as Record<string, number> (method name -> amount)
  const revenueEntries = Object.entries(report.revenueByMethod ?? {})
  const grandTotal = revenueEntries.reduce(
    (sum, [, amount]) => sum + amount,
    0,
  )

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold">{t("shiftReport")}</h2>
          <div className="flex items-center gap-4 mt-1 text-sm text-muted-foreground">
            <span className="flex items-center gap-1">
              <IconUser className="h-4 w-4" />
              {report.cashierName}
            </span>
            <span className="flex items-center gap-1">
              <IconClock className="h-4 w-4" />
              {format(new Date(report.openedAt), "dd/MM/yyyy HH:mm")}
              {report.closedAt &&
                ` - ${format(new Date(report.closedAt), "HH:mm")}`}
            </span>
          </div>
        </div>
        <Button variant="outline" onClick={handlePrint}>
          <IconPrinter className="h-4 w-4 mr-2" />
          {t("printShiftReport")}
        </Button>
      </div>

      {/* Revenue by payment method */}
      <Card>
        <CardHeader>
          <CardTitle>{t("revenueByMethod")}</CardTitle>
        </CardHeader>
        <CardContent>
          {revenueEntries.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              {t("table.noData", { ns: "common" })}
            </p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t("paymentMethod")}</TableHead>
                  <TableHead className="text-right">{t("amount")}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {revenueEntries.map(([method, amount]) => (
                  <TableRow key={method}>
                    <TableCell>
                      {getMethodDisplayName(method, t)}
                    </TableCell>
                    <TableCell className="text-right font-medium">
                      {formatVND(amount)}
                    </TableCell>
                  </TableRow>
                ))}
                {/* Total row */}
                <TableRow className="font-bold border-t-2">
                  <TableCell>{t("total")}</TableCell>
                  <TableCell className="text-right">
                    {formatVND(grandTotal)}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Cash reconciliation */}
      <Card>
        <CardHeader>
          <CardTitle>{t("cashReconciliation")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <ReconciliationRow
            label={t("openingBalance")}
            value={formatVND(report.openingBalance)}
          />
          <ReconciliationRow
            label={`+ ${t("paymentMethods.cash")} ${t("amount").toLowerCase()}`}
            value={formatVND(report.cashReceived)}
            className="text-green-600"
          />
          <ReconciliationRow
            label={`- ${t("refundAmount", { defaultValue: "Cash refunds" })}`}
            value={formatVND(report.cashRefunds)}
            className="text-red-600"
          />
          <Separator />
          <ReconciliationRow
            label={`= ${t("expectedCash")}`}
            value={formatVND(report.expectedCash)}
            className="font-bold"
          />

          {report.actualCash != null && (
            <>
              <ReconciliationRow
                label={t("actualCashCount")}
                value={formatVND(report.actualCash)}
                className="font-medium"
              />
              <Separator />
              <ReconciliationRow
                label={t("discrepancy")}
                value={formatDiscrepancy(
                  report.discrepancy,
                  t("matches"),
                  t("surplus"),
                  t("deficit"),
                )}
                className={cn(
                  "font-bold text-lg",
                  getDiscrepancyColor(report.discrepancy),
                )}
              />
            </>
          )}

          {report.managerNote && (
            <div className="mt-4 rounded-lg border bg-muted/50 p-3">
              <p className="text-xs text-muted-foreground font-medium mb-1">
                {t("managerNote")}
              </p>
              <p className="text-sm">{report.managerNote}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Summary stats */}
      <div className="grid grid-cols-2 gap-4">
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-sm text-muted-foreground">{t("totalRevenue")}</p>
            <p className="text-2xl font-bold">{formatVND(grandTotal)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-sm text-muted-foreground">
              {t("transactionCount")}
            </p>
            <p className="text-2xl font-bold">{report.transactionCount}</p>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

// -- Helper components --

function ReconciliationRow({
  label,
  value,
  className,
}: {
  label: string
  value: string
  className?: string
}) {
  return (
    <div className={cn("flex items-center justify-between", className)}>
      <span>{label}</span>
      <span>{value}</span>
    </div>
  )
}

// -- Helper functions --

// Map backend enum string names (case-insensitive) to i18n keys
const METHOD_NAME_TO_I18N: Record<string, string> = {
  cash: "paymentMethods.cash",
  banktransfer: "paymentMethods.bankTransfer",
  qrvnpay: "paymentMethods.qrVnpay",
  qrmomo: "paymentMethods.qrMomo",
  qrzalopay: "paymentMethods.qrZalopay",
  cardvisa: "paymentMethods.cardVisa",
  cardmastercard: "paymentMethods.cardMc",
}

function getMethodDisplayName(method: string, t: (key: string) => string): string {
  // Try numeric key first (when backend sends numeric keys)
  const numKey = parseInt(method, 10)
  if (!isNaN(numKey) && PAYMENT_METHOD_I18N_KEY[numKey]) {
    return t(PAYMENT_METHOD_I18N_KEY[numKey])
  }
  // Try string name mapping (case-insensitive)
  const key = METHOD_NAME_TO_I18N[method.toLowerCase()]
  if (key) return t(key)
  return method
}

function formatDiscrepancy(
  discrepancy: number | null | undefined,
  matchesLabel: string,
  surplusLabel: string,
  deficitLabel: string,
): string {
  if (discrepancy == null || discrepancy === 0) return matchesLabel
  if (discrepancy > 0) return `${surplusLabel} ${formatVND(discrepancy)}`
  return `${deficitLabel} ${formatVND(Math.abs(discrepancy))}`
}

function getDiscrepancyColor(discrepancy: number | null | undefined): string {
  if (discrepancy == null || discrepancy === 0) return "text-green-600"
  if (discrepancy > 0) return "text-blue-600"
  return "text-red-600"
}

function ShiftReportSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="space-y-2">
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-64" />
        </div>
        <Skeleton className="h-9 w-36" />
      </div>
      <Skeleton className="h-48 w-full" />
      <Skeleton className="h-64 w-full" />
      <div className="grid grid-cols-2 gap-4">
        <Skeleton className="h-24 w-full" />
        <Skeleton className="h-24 w-full" />
      </div>
    </div>
  )
}
