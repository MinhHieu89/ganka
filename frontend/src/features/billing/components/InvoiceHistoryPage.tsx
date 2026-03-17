import { useState, useMemo } from "react"
import { Link } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { IconReceipt, IconSearch } from "@tabler/icons-react"
import { Tabs, TabsList, TabsTrigger } from "@/shared/components/Tabs"
import { Input } from "@/shared/components/Input"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import { formatVND } from "@/shared/lib/format-vnd"
import {
  useAllInvoices,
  INVOICE_STATUS_I18N_KEY,
} from "@/features/billing/api/billing-api"
import { useDebounce } from "@/shared/hooks/useDebounce"

const STATUS_VARIANT: Record<number, "default" | "secondary" | "destructive" | "outline"> = {
  0: "secondary",
  1: "outline",
  2: "destructive",
}

const STATUS_CLASS: Record<number, string> = {
  1: "border-green-600 bg-green-50 text-green-700 dark:bg-green-950 dark:text-green-400",
}

const PAGE_SIZE = 20

export function InvoiceHistoryPage() {
  const { t } = useTranslation("billing")
  const [statusTab, setStatusTab] = useState<string>("all")
  const [searchInput, setSearchInput] = useState("")
  const [page, setPage] = useState(1)
  const debouncedSearch = useDebounce(searchInput, 300)

  const statusFilter = statusTab === "all" ? undefined : Number(statusTab)

  const { data, isLoading } = useAllInvoices({
    status: statusFilter,
    search: debouncedSearch || undefined,
    page,
    pageSize: PAGE_SIZE,
  })

  const totalPages = useMemo(() => {
    if (!data) return 1
    return Math.max(1, Math.ceil(data.totalCount / PAGE_SIZE))
  }, [data])

  function handleTabChange(value: string) {
    setStatusTab(value)
    setPage(1)
  }

  function handleSearchChange(e: React.ChangeEvent<HTMLInputElement>) {
    setSearchInput(e.target.value)
    setPage(1)
  }

  return (
    <div className="space-y-4">
      {/* Search and filter */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <IconSearch className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            value={searchInput}
            onChange={handleSearchChange}
            placeholder={t("searchInvoices")}
            className="pl-9"
          />
        </div>
      </div>

      {/* Status tabs */}
      <Tabs value={statusTab} onValueChange={handleTabChange}>
        <TabsList>
          <TabsTrigger value="all">{t("statusFilter.all")}</TabsTrigger>
          <TabsTrigger value="0">{t("statusFilter.draft")}</TabsTrigger>
          <TabsTrigger value="1">{t("statusFilter.finalized")}</TabsTrigger>
          <TabsTrigger value="2">{t("statusFilter.voided")}</TabsTrigger>
        </TabsList>
      </Tabs>

      {/* Table */}
      {isLoading ? (
        <LoadingSkeleton />
      ) : !data || data.items.length === 0 ? (
        <EmptyState />
      ) : (
        <>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t("invoiceNumber")}</TableHead>
                  <TableHead>{t("patientName")}</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">{t("total")}</TableHead>
                  <TableHead className="text-right">{t("paid")}</TableHead>
                  <TableHead className="text-right">{t("balanceDue")}</TableHead>
                  <TableHead>{t("createdAt")}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((invoice) => {
                  const statusI18nKey = INVOICE_STATUS_I18N_KEY[invoice.status]
                  return (
                    <TableRow key={invoice.id} className="cursor-pointer hover:bg-muted/50">
                      <TableCell>
                        <Link
                          to="/billing/invoices/$invoiceId"
                          params={{ invoiceId: invoice.id }}
                          className="font-medium text-primary hover:underline"
                        >
                          {invoice.invoiceNumber}
                        </Link>
                      </TableCell>
                      <TableCell>{invoice.patientName}</TableCell>
                      <TableCell>
                        <Badge
                          variant={STATUS_VARIANT[invoice.status] ?? "secondary"}
                          className={STATUS_CLASS[invoice.status] ?? ""}
                        >
                          {statusI18nKey ? t(statusI18nKey) : `Status ${invoice.status}`}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">{formatVND(invoice.totalAmount)}</TableCell>
                      <TableCell className="text-right">{formatVND(invoice.paidAmount)}</TableCell>
                      <TableCell className="text-right">
                        {invoice.balanceDue > 0 ? (
                          <span className="text-orange-600 font-medium">
                            {formatVND(invoice.balanceDue)}
                          </span>
                        ) : (
                          <span className="text-green-600">{formatVND(0)}</span>
                        )}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {format(new Date(invoice.createdAt), "dd/MM/yyyy HH:mm")}
                      </TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>
          </div>

          {/* Pagination */}
          <div className="flex items-center justify-between">
            <p className="text-sm text-muted-foreground">
              {t("pageOf", { page, total: totalPages })}
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setPage((p) => p - 1)}
              >
                {t("previous")}
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
              >
                {t("next")}
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}

function EmptyState() {
  const { t } = useTranslation("billing")
  return (
    <div className="text-center py-16 text-muted-foreground">
      <IconReceipt className="h-12 w-12 mx-auto mb-4 opacity-30" />
      <p>{t("noInvoices")}</p>
    </div>
  )
}

function LoadingSkeleton() {
  return (
    <div className="space-y-3">
      {Array.from({ length: 5 }).map((_, i) => (
        <div key={i} className="flex items-center gap-4 p-3 border rounded-lg">
          <Skeleton className="h-4 w-28" />
          <Skeleton className="h-4 w-36" />
          <Skeleton className="h-5 w-16" />
          <Skeleton className="h-4 w-24 ml-auto" />
        </div>
      ))}
    </div>
  )
}
