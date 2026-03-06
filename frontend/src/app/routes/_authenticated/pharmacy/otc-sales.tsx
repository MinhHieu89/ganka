import { createFileRoute } from "@tanstack/react-router"
import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { IconShoppingCart } from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { DataTable } from "@/shared/components/DataTable"
import { Skeleton } from "@/shared/components/Skeleton"
import { Badge } from "@/shared/components/Badge"
import { OtcSaleForm } from "@/features/pharmacy/components/OtcSaleForm"
import type { OtcSaleDto } from "@/features/pharmacy/api/pharmacy-api"
import { useOtcSales } from "@/features/pharmacy/api/pharmacy-queries"

export const Route = createFileRoute("/_authenticated/pharmacy/otc-sales")({
  component: OtcSalesPage,
})

const columnHelper = createColumnHelper<OtcSaleDto>()

function OtcSalesPage() {
  const { t } = useTranslation("pharmacy")
  const { data: sales, isLoading } = useOtcSales()
  const [sorting, setSorting] = useState<SortingState>([])

  const columns = useMemo(
    () => [
      columnHelper.accessor("customerName", {
        header: () => t("otcSale.historyCustomer"),
        cell: (info) => {
          const name = info.getValue()
          if (!name) {
            return (
              <Badge variant="outline" className="text-xs font-normal text-muted-foreground">
                {t("otcSale.anonymous_customer")}
              </Badge>
            )
          }
          return <span className="font-medium">{name}</span>
        },
        enableSorting: true,
      }),
      columnHelper.accessor("saleDate", {
        header: () => t("otcSale.historyDate"),
        cell: (info) => (
          <span className="text-sm">
            {new Date(info.getValue()).toLocaleDateString("vi-VN", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
            })}
          </span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("lineCount", {
        header: () => t("otcSale.historyItems"),
        cell: (info) => (
          <span className="text-sm">{info.getValue()}</span>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("totalAmount", {
        header: () => t("otcSale.historyTotal"),
        cell: (info) => (
          <span className="text-sm font-medium">
            {info.getValue().toLocaleString("vi-VN")} ₫
          </span>
        ),
        enableSorting: true,
      }),
    ],
    [t],
  )

  const table = useReactTable({
    data: sales ?? [],
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
  })

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-start justify-between gap-4">
        <div className="space-y-1">
          <div className="flex items-center gap-3">
            <IconShoppingCart className="h-6 w-6 text-muted-foreground" />
            <h1 className="text-2xl font-bold">{t("otcSale.title")}</h1>
          </div>
          <p className="text-sm text-muted-foreground">{t("otcSale.subtitle")}</p>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-[480px_1fr]">
        {/* New sale form */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t("otcSale.newSale")}</CardTitle>
          </CardHeader>
          <CardContent>
            <OtcSaleForm />
          </CardContent>
        </Card>

        {/* Sales history */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t("otcSale.history")}</CardTitle>
          </CardHeader>
          <CardContent>
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
                emptyMessage={t("otcSale.historyEmpty")}
              />
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
