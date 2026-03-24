import { createFileRoute } from "@tanstack/react-router"
import { requirePermission } from "@/shared/utils/permission-guard"
import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { IconFileSpreadsheet } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/Tabs"
import { DataTable } from "@/shared/components/DataTable"
import { Skeleton } from "@/shared/components/Skeleton"
import { Badge } from "@/shared/components/Badge"
import { StockImportForm } from "@/features/pharmacy/components/StockImportForm"
import { ExcelImportDialog } from "@/features/pharmacy/components/ExcelImportDialog"
import type { StockImportDto } from "@/features/pharmacy/api/pharmacy-api"
import { useStockImports } from "@/features/pharmacy/api/pharmacy-queries"

export const Route = createFileRoute("/_authenticated/pharmacy/stock-import")({
  beforeLoad: () => requirePermission("Pharmacy.Create"),
  component: StockImportPage,
})

const columnHelper = createColumnHelper<StockImportDto>()

function StockImportPage() {
  const { t } = useTranslation("pharmacy")
  const { data: imports, isLoading } = useStockImports()
  const [sorting, setSorting] = useState<SortingState>([])
  const [excelDialogOpen, setExcelDialogOpen] = useState(false)

  const columns = useMemo(
    () => [
      columnHelper.accessor("supplierName", {
        header: () => t("stockImport.historySupplier"),
        cell: (info) => <span className="font-medium">{info.getValue()}</span>,
        enableSorting: true,
      }),
      columnHelper.accessor("invoiceNumber", {
        header: () => t("stockImport.historyInvoice"),
        cell: (info) => (
          <span className="text-sm text-muted-foreground">
            {info.getValue() ?? "—"}
          </span>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("importDate", {
        header: () => t("stockImport.historyDate"),
        cell: (info) => (
          <span className="text-sm">
            {new Date(info.getValue()).toLocaleDateString("vi-VN")}
          </span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("lineCount", {
        header: () => t("stockImport.historyLines"),
        cell: (info) => <span className="text-sm">{info.getValue()}</span>,
        enableSorting: false,
      }),
      columnHelper.accessor("totalAmount", {
        header: () => t("stockImport.historyAmount"),
        cell: (info) => (
          <span className="text-sm font-medium">
            {info.getValue().toLocaleString("vi-VN")} ₫
          </span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("importSource", {
        header: () => t("stockImport.historySource"),
        cell: (info) =>
          info.getValue() === 0 ? (
            <Badge variant="outline">{t("stockImport.sourceInvoice")}</Badge>
          ) : (
            <Badge variant="secondary">{t("stockImport.sourceExcel")}</Badge>
          ),
        enableSorting: false,
      }),
    ],
    [t],
  )

  const table = useReactTable({
    data: imports ?? [],
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
  })

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("stockImport.title")}</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {t("stockImport.subtitle")}
          </p>
        </div>
        <Button variant="outline" onClick={() => setExcelDialogOpen(true)}>
          <IconFileSpreadsheet className="h-4 w-4 mr-2" />
          {t("stockImport.tabExcel")}
        </Button>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="invoice">
        <TabsList>
          <TabsTrigger value="invoice">{t("stockImport.tabInvoice")}</TabsTrigger>
          <TabsTrigger value="history">{t("stockImport.historyTitle")}</TabsTrigger>
        </TabsList>

        {/* Invoice import form */}
        <TabsContent value="invoice" className="pt-4">
          <StockImportForm />
        </TabsContent>

        {/* Import history */}
        <TabsContent value="history" className="pt-4">
          {isLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-10 w-full" />
              ))}
            </div>
          ) : (
            <DataTable
              table={table}
              columns={columns}
              emptyMessage={t("stockImport.historyEmpty")}
            />
          )}
        </TabsContent>
      </Tabs>

      {/* Excel import dialog */}
      <ExcelImportDialog
        open={excelDialogOpen}
        onOpenChange={setExcelDialogOpen}
      />
    </div>
  )
}
