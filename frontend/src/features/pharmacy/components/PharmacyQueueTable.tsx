import { useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { Badge } from "@/shared/components/Badge"
import { Input } from "@/shared/components/Input"
import { Skeleton } from "@/shared/components/Skeleton"
import { DataTable } from "@/shared/components/DataTable"
import { type PendingPrescriptionDto } from "@/features/pharmacy/api/pharmacy-api"
import { usePendingPrescriptions } from "@/features/pharmacy/api/pharmacy-queries"
import { DispensingDialog } from "./DispensingDialog"

interface PharmacyQueueTableProps {
  patientId?: string
}

const columnHelper = createColumnHelper<PendingPrescriptionDto>()

function formatDate(dateStr: string): string {
  try {
    const d = new Date(dateStr)
    return d.toLocaleString("vi-VN", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    })
  } catch {
    return dateStr
  }
}

export function PharmacyQueueTable({ patientId }: PharmacyQueueTableProps) {
  const { t } = useTranslation("pharmacy")
  const [sorting, setSorting] = useState<SortingState>([
    { id: "prescribedAt", desc: false },
  ])
  const [globalFilter, setGlobalFilter] = useState("")
  const [selectedPrescription, setSelectedPrescription] =
    useState<PendingPrescriptionDto | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)

  const { data: allPrescriptions, isLoading } = usePendingPrescriptions()

  // Filter by patientId if provided (for patient profile tab)
  const prescriptions = useMemo(() => {
    const list = allPrescriptions ?? []
    if (patientId) {
      return list.filter((p) => p.patientId === patientId)
    }
    return list
  }, [allPrescriptions, patientId])

  const columns = useMemo(
    () => [
      columnHelper.accessor("patientName", {
        header: () => t("queue.patientName"),
        cell: (info) => (
          <span className="font-medium text-sm">{info.getValue()}</span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("prescriptionCode", {
        header: () => t("queue.prescriptionCode"),
        cell: (info) => (
          <span className="font-mono text-sm text-muted-foreground">
            {info.getValue() ?? "—"}
          </span>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("prescribedAt", {
        id: "prescribedAt",
        header: () => t("queue.prescribedAt"),
        cell: (info) => (
          <span className="text-sm">{formatDate(info.getValue())}</span>
        ),
        enableSorting: true,
      }),
      columnHelper.display({
        id: "itemCount",
        header: () => t("queue.itemCount"),
        cell: ({ row }) => (
          <span className="text-sm text-center block">
            {row.original.items?.length ?? row.original.itemCount ?? 0}
          </span>
        ),
      }),
      columnHelper.accessor("daysRemaining", {
        header: () => t("queue.daysRemaining"),
        cell: (info) => {
          const days = info.getValue()
          const isExpired = info.row.original.isExpired
          if (isExpired) {
            return (
              <Badge variant="destructive" className="text-xs">
                {t("queue.expired")}
              </Badge>
            )
          }
          if (days <= 2) {
            return (
              <Badge
                variant="outline"
                className="text-xs border-yellow-500 text-yellow-700 dark:text-yellow-400"
              >
                {t("queue.daysLeft", { days })}
              </Badge>
            )
          }
          return (
            <span className="text-sm text-muted-foreground">
              {t("queue.daysLeft", { days })}
            </span>
          )
        },
        enableSorting: true,
      }),
      columnHelper.display({
        id: "status",
        header: () => t("queue.status"),
        cell: ({ row }) => {
          const { isExpired, daysRemaining } = row.original
          if (isExpired) {
            return (
              <Badge variant="destructive" className="text-xs">
                {t("queue.statusExpired")}
              </Badge>
            )
          }
          if (daysRemaining <= 2) {
            return (
              <Badge
                variant="outline"
                className="text-xs border-yellow-500 text-yellow-700 dark:text-yellow-400"
              >
                {t("queue.statusUrgent")}
              </Badge>
            )
          }
          return (
            <Badge variant="outline" className="text-xs">
              {t("queue.statusPending")}
            </Badge>
          )
        },
      }),
    ],
    [t],
  )

  const table = useReactTable({
    data: prescriptions,
    columns,
    state: { sorting, globalFilter },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    globalFilterFn: (row, _columnId, filterValue: string) => {
      const search = filterValue.toLowerCase()
      const p = row.original
      return (
        p.patientName.toLowerCase().includes(search) ||
        (p.prescriptionCode?.toLowerCase().includes(search) ?? false)
      )
    },
  })

  if (isLoading) {
    return (
      <div className="space-y-3">
        <Skeleton className="h-10 w-72" />
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-10 w-full" />
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {/* Search filter — only show when not filtered by patient */}
      {!patientId && (
        <Input
          value={globalFilter}
          onChange={(e) => setGlobalFilter(e.target.value)}
          placeholder={t("queue.search")}
          className="max-w-sm"
        />
      )}

      <DataTable
        table={table}
        columns={columns}
        emptyMessage={t("queue.empty")}
        onRowClick={(row) => {
          setSelectedPrescription(row)
          setDialogOpen(true)
        }}
        rowClassName={(row) => {
          if (row.isExpired) return "bg-destructive/5"
          if (row.daysRemaining <= 2) return "bg-yellow-50 dark:bg-yellow-950/20"
          return ""
        }}
      />

      <DispensingDialog
        prescription={selectedPrescription}
        open={dialogOpen}
        onOpenChange={(open) => {
          setDialogOpen(open)
          if (!open) setSelectedPrescription(null)
        }}
      />
    </div>
  )
}
