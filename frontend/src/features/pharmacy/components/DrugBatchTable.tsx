import { useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { Badge } from "@/shared/components/Badge"
import { Skeleton } from "@/shared/components/Skeleton"
import { DataTable } from "@/shared/components/DataTable"
import { type DrugBatchDto } from "@/features/pharmacy/api/pharmacy-api"
import { useDrugBatches } from "@/features/pharmacy/api/pharmacy-queries"

interface DrugBatchTableProps {
  drugId: string
  drugName: string
}

const columnHelper = createColumnHelper<DrugBatchDto>()

function getBatchStatus(batch: DrugBatchDto): "expired" | "nearExpiry" | "active" {
  if (batch.isExpired) return "expired"
  if (batch.isNearExpiry) return "nearExpiry"
  return "active"
}

function formatDate(dateStr: string): string {
  try {
    const d = new Date(dateStr)
    return d.toLocaleDateString("vi-VN")
  } catch {
    return dateStr
  }
}

export function DrugBatchTable({ drugId, drugName }: DrugBatchTableProps) {
  const { t } = useTranslation("pharmacy")
  const { data: batches, isLoading } = useDrugBatches(drugId)
  const [sorting, setSorting] = useState<SortingState>([{ id: "expiryDate", desc: false }])

  const columns = useMemo(
    () => [
      columnHelper.accessor("batchNumber", {
        header: () => t("batch.batchNumber"),
        cell: (info) => <span className="font-mono text-sm">{info.getValue()}</span>,
        enableSorting: true,
      }),
      columnHelper.accessor("supplierName", {
        header: () => t("batch.supplier"),
        cell: (info) => <span className="text-sm">{info.getValue()}</span>,
        enableSorting: false,
      }),
      columnHelper.accessor("expiryDate", {
        header: () => t("batch.expiryDate"),
        cell: (info) => {
          const status = getBatchStatus(info.row.original)
          return (
            <span
              className={
                status === "expired"
                  ? "text-destructive font-medium text-sm"
                  : status === "nearExpiry"
                    ? "text-yellow-600 dark:text-yellow-400 font-medium text-sm"
                    : "text-sm"
              }
            >
              {formatDate(info.getValue())}
            </span>
          )
        },
        enableSorting: true,
      }),
      columnHelper.accessor("initialQuantity", {
        header: () => t("batch.initialQty"),
        cell: (info) => <span className="text-sm text-muted-foreground">{info.getValue()}</span>,
        enableSorting: false,
      }),
      columnHelper.accessor("currentQuantity", {
        header: () => t("batch.currentQty"),
        cell: (info) => (
          <span
            className={`text-sm font-medium ${info.getValue() === 0 ? "text-muted-foreground" : ""}`}
          >
            {info.getValue()}
          </span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("purchasePrice", {
        header: () => t("batch.purchasePrice"),
        cell: (info) => (
          <span className="text-sm">{info.getValue().toLocaleString("vi-VN")} ₫</span>
        ),
        enableSorting: false,
      }),
      columnHelper.display({
        id: "status",
        header: () => t("inventory.status"),
        cell: ({ row }) => {
          const status = getBatchStatus(row.original)
          if (status === "expired") {
            return <Badge variant="destructive" className="text-xs">{t("batch.statusExpired")}</Badge>
          }
          if (status === "nearExpiry") {
            return (
              <Badge
                variant="outline"
                className="text-xs border-yellow-500 text-yellow-700 dark:text-yellow-400"
              >
                {t("batch.statusNearExpiry")}
              </Badge>
            )
          }
          return <Badge variant="outline" className="text-xs">{t("batch.statusActive")}</Badge>
        },
      }),
    ],
    [t],
  )

  const table = useReactTable({
    data: batches ?? [],
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
  })

  if (isLoading) {
    return (
      <div className="space-y-2 py-2">
        <Skeleton className="h-8 w-full" />
        <Skeleton className="h-8 w-full" />
        <Skeleton className="h-8 w-3/4" />
      </div>
    )
  }

  return (
    <div className="space-y-2">
      <p className="text-xs text-muted-foreground font-medium">
        {t("batch.title")} — {drugName} ({t("batch.fefoOrder")})
      </p>
      <DataTable
        table={table}
        columns={columns}
        emptyMessage={t("batch.empty")}
        rowClassName={(batch) => {
          if (batch.isExpired) return "bg-destructive/5"
          if (batch.isNearExpiry) return "bg-yellow-50 dark:bg-yellow-950/20"
          return ""
        }}
      />
    </div>
  )
}
