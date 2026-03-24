import { createFileRoute } from "@tanstack/react-router"
import { requirePermission } from "@/shared/utils/permission-guard"
import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  getExpandedRowModel,
  useReactTable,
  type SortingState,
  type ExpandedState,
} from "@tanstack/react-table"
import { IconShoppingCart, IconChevronDown, IconChevronRight } from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Button } from "@/shared/components/Button"
import { DataTable } from "@/shared/components/DataTable"
import { Skeleton } from "@/shared/components/Skeleton"
import { Badge } from "@/shared/components/Badge"
import { OtcSaleForm } from "@/features/pharmacy/components/OtcSaleForm"
import type { OtcSaleDto } from "@/features/pharmacy/api/pharmacy-api"
import { useOtcSales } from "@/features/pharmacy/api/pharmacy-queries"

export const Route = createFileRoute("/_authenticated/pharmacy/otc-sales")({
  beforeLoad: () => requirePermission("Pharmacy.View"),
  component: OtcSalesPage,
})

const columnHelper = createColumnHelper<OtcSaleDto>()

function OtcSalesPage() {
  const { t } = useTranslation("pharmacy")
  const { data: sales, isLoading } = useOtcSales()
  const [sorting, setSorting] = useState<SortingState>([])
  const [expanded, setExpanded] = useState<ExpandedState>({})

  const columns = useMemo(
    () => [
      columnHelper.display({
        id: "expander",
        size: 40,
        cell: ({ row }) => (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            className="h-7 w-7 p-0"
            onClick={(e) => {
              e.stopPropagation()
              row.toggleExpanded()
            }}
          >
            {row.getIsExpanded() ? (
              <IconChevronDown className="h-4 w-4" />
            ) : (
              <IconChevronRight className="h-4 w-4" />
            )}
          </Button>
        ),
      }),
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
      columnHelper.accessor("soldAt", {
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
      columnHelper.display({
        id: "lineCount",
        header: () => t("otcSale.historyItems"),
        cell: ({ row }) => (
          <span className="text-sm">{row.original.lines.length}</span>
        ),
      }),
      columnHelper.display({
        id: "totalAmount",
        header: () => t("otcSale.historyTotal"),
        cell: ({ row }) => {
          const total = row.original.lines.reduce(
            (sum, l) => sum + l.quantity * l.unitPrice, 0
          )
          return (
            <span className="text-sm font-medium">
              {total.toLocaleString("vi-VN")} ₫
            </span>
          )
        },
      }),
    ],
    [t],
  )

  const table = useReactTable({
    data: sales ?? [],
    columns,
    state: { sorting, expanded },
    onSortingChange: setSorting,
    onExpandedChange: setExpanded,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
    getRowCanExpand: () => true,
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

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
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
                renderSubRow={(sale) => (
                  <div className="p-4 bg-muted/30">
                    <p className="text-xs font-medium text-muted-foreground mb-2">
                      {t("otcSale.historyItems")}
                    </p>
                    <div className="rounded-lg border overflow-hidden">
                      <table className="w-full text-sm">
                        <thead>
                          <tr className="border-b bg-muted/40">
                            <th className="text-left px-3 py-2 font-medium">{t("otcSale.drug")}</th>
                            <th className="text-right px-3 py-2 font-medium">{t("otcSale.quantity")}</th>
                            <th className="text-right px-3 py-2 font-medium">{t("otcSale.unitPrice")}</th>
                            <th className="text-right px-3 py-2 font-medium">{t("otcSale.historyTotal")}</th>
                          </tr>
                        </thead>
                        <tbody>
                          {sale.lines.map((line) => (
                            <tr key={line.id} className="border-b last:border-0">
                              <td className="px-3 py-2">{line.drugName}</td>
                              <td className="px-3 py-2 text-right">{line.quantity}</td>
                              <td className="px-3 py-2 text-right">{line.unitPrice.toLocaleString("vi-VN")} ₫</td>
                              <td className="px-3 py-2 text-right font-medium">
                                {(line.quantity * line.unitPrice).toLocaleString("vi-VN")} ₫
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                    {sale.notes && (
                      <p className="mt-2 text-xs text-muted-foreground">
                        {t("otcSale.notes")}: {sale.notes}
                      </p>
                    )}
                  </div>
                )}
              />
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
