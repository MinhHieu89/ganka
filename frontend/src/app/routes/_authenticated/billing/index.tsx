import { createFileRoute, Link } from "@tanstack/react-router"
import { requirePermission } from "@/shared/utils/permission-guard"
import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import {
  IconReceipt,
  IconClock,
  IconCash,
  IconArrowRight,
} from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { Separator } from "@/shared/components/Separator"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/shared/components/Tooltip"
import { formatVND } from "@/shared/lib/format-vnd"
import {
  usePendingInvoices,
  type InvoiceSummaryDto,
} from "@/features/billing/api/billing-api"
import {
  useCurrentShift,
  type CashierShiftDto,
} from "@/features/billing/api/shift-api"
import { useBillingHub, type ConnectionStatus } from "@/features/billing/hooks/use-billing-hub"

export const Route = createFileRoute("/_authenticated/billing/")({
  beforeLoad: () => requirePermission("Billing.View"),
  component: BillingDashboard,
})

function ConnectionStatusIndicator({ status }: { status: ConnectionStatus }) {
  const { t } = useTranslation("billing")

  const config: Record<ConnectionStatus, { color: string; label: string }> = {
    connected: { color: "bg-green-500", label: t("connection.connected") },
    reconnecting: { color: "bg-yellow-500", label: t("connection.reconnecting") },
    disconnected: { color: "bg-red-500", label: t("connection.disconnected") },
  }

  const { color, label } = config[status]

  return (
    <Badge variant="outline" className="gap-1.5 text-xs font-normal">
      <span className={`h-2 w-2 rounded-full ${color}`} />
      {label}
    </Badge>
  )
}

function BillingDashboard() {
  const { t } = useTranslation("billing")
  const { data: invoices, isLoading: invoicesLoading } = usePendingInvoices()
  const { data: shift, isLoading: shiftLoading } = useCurrentShift()
  const connectionStatus = useBillingHub()

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("dashboard")}</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {t("title")}
          </p>
        </div>
        <ConnectionStatusIndicator status={connectionStatus} />
      </div>

      {/* Two-column layout: invoices left, shift status right */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left panel: Pending invoices (2/3 width) */}
        <div className="lg:col-span-2">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="flex items-center gap-2">
                <IconReceipt className="h-5 w-5" />
                {t("pendingInvoices")}
              </CardTitle>
              <Button variant="outline" size="sm" asChild>
                <Link to="/billing/invoices">{t("viewAllInvoices")}</Link>
              </Button>
            </CardHeader>
            <CardContent>
              {invoicesLoading ? (
                <InvoicesLoadingSkeleton />
              ) : !invoices || invoices.length === 0 ? (
                <div className="text-center py-12 text-muted-foreground">
                  <IconReceipt className="h-10 w-10 mx-auto mb-3 opacity-30" />
                  <p>{t("noPendingInvoices")}</p>
                </div>
              ) : (
                <div className="space-y-2">
                  {invoices.map((invoice) => (
                    <InvoiceRow key={invoice.id} invoice={invoice} />
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Right panel: Current shift status (1/3 width) */}
        <div>
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <IconClock className="h-5 w-5" />
                {t("currentShift")}
              </CardTitle>
            </CardHeader>
            <CardContent>
              {shiftLoading ? (
                <ShiftLoadingSkeleton />
              ) : !shift ? (
                <NoOpenShift />
              ) : (
                <ShiftInfo shift={shift} />
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}

function InvoiceRow({ invoice }: { invoice: InvoiceSummaryDto }) {
  const { t } = useTranslation("billing")

  return (
    <Link
      to="/billing/invoices/$invoiceId"
      params={{ invoiceId: invoice.id }}
      className="flex items-center justify-between p-3 rounded-lg border hover:bg-muted/50 transition-colors cursor-pointer group"
    >
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className="font-medium text-sm truncate">
            {invoice.invoiceNumber}
          </span>
          <Badge variant="secondary" className="text-xs">
            {t("status.draft")}
          </Badge>
        </div>
        <p className="text-sm text-muted-foreground truncate mt-0.5">
          {invoice.patientName}
        </p>
        <p className="text-xs text-muted-foreground mt-0.5">
          {format(new Date(invoice.createdAt), "dd/MM/yyyy HH:mm")}
        </p>
      </div>
      <div className="flex items-center gap-3 ml-4">
        <span className="font-medium text-sm whitespace-nowrap">
          {formatVND(invoice.totalAmount)}
        </span>
        <IconArrowRight className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity" />
      </div>
    </Link>
  )
}

function NoOpenShift() {
  const { t } = useTranslation("billing")

  return (
    <div className="text-center py-8">
      <IconCash className="h-10 w-10 mx-auto mb-3 text-muted-foreground opacity-30" />
      <p className="text-muted-foreground mb-4">{t("noOpenShift")}</p>
      <Tooltip>
        <TooltipTrigger asChild>
          <Button disabled>
            {t("openShift")}
          </Button>
        </TooltipTrigger>
        <TooltipContent>{t("shiftManagement.comingSoon")}</TooltipContent>
      </Tooltip>
    </div>
  )
}

function ShiftInfo({ shift }: { shift: CashierShiftDto }) {
  const { t } = useTranslation("billing")

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">{t("cashierName")}</span>
          <span className="font-medium">{shift.cashierName}</span>
        </div>
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">{t("openedAt")}</span>
          <span className="font-medium">
            {format(new Date(shift.openedAt), "HH:mm dd/MM")}
          </span>
        </div>
      </div>

      <Separator />

      <div className="space-y-2">
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">{t("totalRevenue")}</span>
          <span className="font-medium">{formatVND(shift.totalRevenue)}</span>
        </div>
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">{t("transactionCount")}</span>
          <span className="font-medium">{shift.transactionCount}</span>
        </div>
      </div>

      <Separator />

      <Tooltip>
        <TooltipTrigger asChild>
          <Button variant="outline" className="w-full" disabled>
            {t("closeShift")}
          </Button>
        </TooltipTrigger>
        <TooltipContent>{t("shiftManagement.comingSoon")}</TooltipContent>
      </Tooltip>
    </div>
  )
}

function InvoicesLoadingSkeleton() {
  return (
    <div className="space-y-3">
      {Array.from({ length: 4 }).map((_, i) => (
        <div key={i} className="flex items-center justify-between p-3 border rounded-lg">
          <div className="space-y-2 flex-1">
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-3 w-48" />
          </div>
          <Skeleton className="h-4 w-24" />
        </div>
      ))}
    </div>
  )
}

function ShiftLoadingSkeleton() {
  return (
    <div className="space-y-3">
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-3/4" />
      <Skeleton className="h-8 w-full mt-4" />
    </div>
  )
}
