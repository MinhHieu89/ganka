import { useMemo } from "react"
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getPaginationRowModel,
  createColumnHelper,
  type SortingState,
} from "@tanstack/react-table"
import { useState } from "react"
import { IconClipboardList } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { DataTable } from "@/shared/components/DataTable"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/Select"
import { StatusBadge } from "./StatusBadge"
import { SourceBadge } from "./SourceBadge"
import { RowActionMenu } from "./RowActionMenu"
import type {
  ReceptionistDashboardRow,
  DashboardFilters,
} from "@/features/receptionist/types/receptionist.types"

const columnHelper = createColumnHelper<ReceptionistDashboardRow>()

interface PatientQueueTableProps {
  data: ReceptionistDashboardRow[] | undefined
  isLoading: boolean
  onCheckIn: (row: ReceptionistDashboardRow) => void
  onActionMenu: (row: ReceptionistDashboardRow) => void
  filters: DashboardFilters
  onFiltersChange: (filters: DashboardFilters) => void
}

export function PatientQueueTable({
  data,
  isLoading,
  onCheckIn,
  onActionMenu,
  filters,
  onFiltersChange,
}: PatientQueueTableProps) {
  const [sorting, setSorting] = useState<SortingState>([
    { id: "appointmentTime", desc: false },
  ])

  const columns = useMemo(
    () => [
      columnHelper.display({
        id: "stt",
        header: "STT",
        size: 48,
        cell: (info) => (
          <span className="text-muted-foreground tabular-nums">
            {info.row.index + 1 + (filters.page - 1) * filters.pageSize}
          </span>
        ),
      }),
      columnHelper.accessor("patientName", {
        header: "Ho ten",
        size: 160,
        enableSorting: true,
        cell: (info) => (
          <div>
            <div className="font-semibold text-sm">{info.getValue()}</div>
            {info.row.original.patientCode && (
              <div className="font-mono text-xs text-muted-foreground">
                {info.row.original.patientCode}
              </div>
            )}
          </div>
        ),
      }),
      columnHelper.accessor("birthYear", {
        header: "Nam sinh",
        size: 80,
        enableSorting: true,
        meta: { className: "hidden lg:table-cell" },
        cell: (info) => info.getValue() ?? "—",
      }),
      columnHelper.accessor("appointmentTime", {
        header: "Gio hen",
        size: 80,
        enableSorting: true,
        cell: (info) => {
          const val = info.getValue()
          if (!val) return "—"
          try {
            return new Date(val).toLocaleTimeString("vi-VN", {
              hour: "2-digit",
              minute: "2-digit",
              hour12: false,
            })
          } catch {
            return "—"
          }
        },
      }),
      columnHelper.accessor("source", {
        header: "Nguon",
        size: 80,
        enableSorting: true,
        meta: { className: "hidden lg:table-cell" },
        cell: (info) => <SourceBadge source={info.getValue()} />,
      }),
      columnHelper.accessor("reason", {
        header: "Ly do kham",
        size: 120,
        enableSorting: false,
        cell: (info) => {
          const reason = info.getValue()
          if (!reason) {
            return <span className="italic text-muted-foreground">Chua ro</span>
          }
          return <span className="text-sm">{reason}</span>
        },
      }),
      columnHelper.accessor("status", {
        header: "Trang thai",
        size: 100,
        enableSorting: true,
        cell: (info) => <StatusBadge status={info.getValue()} />,
      }),
      columnHelper.display({
        id: "actions",
        header: "Thao tac",
        size: 120,
        cell: (info) => {
          const row = info.row.original
          if (row.status === "not_arrived") {
            return (
              <div className="flex items-center gap-1">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation()
                    onCheckIn(row)
                  }}
                  className="border-[var(--checkin-confirm)] text-[var(--checkin-confirm)] hover:bg-[var(--status-not-arrived-bg)]"
                >
                  Check-in
                </Button>
                <RowActionMenu row={row} onCheckIn={() => onCheckIn(row)} />
              </div>
            )
          }
          return (
            <RowActionMenu row={row} onCheckIn={() => onCheckIn(row)} />
          )
        },
      }),
    ],
    [onCheckIn, onActionMenu, filters.page, filters.pageSize],
  )

  const tableData = useMemo(() => data ?? [], [data])

  const table = useReactTable({
    data: tableData,
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: {
      pagination: { pageSize: filters.pageSize },
    },
  })

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    )
  }

  if (!data || data.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <div className="flex size-14 items-center justify-center bg-muted rounded-full mb-4">
          <IconClipboardList className="h-7 w-7 text-muted-foreground" />
        </div>
        <h3 className="text-base font-semibold mb-1">
          Chua co benh nhan nao hom nay
        </h3>
        <p className="text-sm text-muted-foreground max-w-sm">
          Bat dau tiep nhan benh nhan moi hoac cho benh nhan da hen den.
        </p>
      </div>
    )
  }

  const totalRows = data.length

  return (
    <div className="space-y-4">
      <DataTable
        table={table}
        columns={columns}
        rowClassName={(row) =>
          row.status === "not_arrived" ? "bg-secondary" : ""
        }
      />
      <div className="flex items-center justify-between text-sm text-muted-foreground">
        <span>
          Hien thi {Math.min(table.getRowModel().rows.length, filters.pageSize)} / {totalRows} benh nhan hom nay
        </span>
        <div className="flex items-center gap-2">
          <Select
            value={String(filters.pageSize)}
            onValueChange={(val) =>
              onFiltersChange({ ...filters, pageSize: Number(val), page: 1 })
            }
          >
            <SelectTrigger className="w-[70px] h-8">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="10">10</SelectItem>
              <SelectItem value="20">20</SelectItem>
              <SelectItem value="50">50</SelectItem>
            </SelectContent>
          </Select>
          <div className="flex gap-1">
            <Button
              variant="outline"
              size="sm"
              disabled={!table.getCanPreviousPage()}
              onClick={() => {
                table.previousPage()
                onFiltersChange({ ...filters, page: filters.page - 1 })
              }}
            >
              Truoc
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={!table.getCanNextPage()}
              onClick={() => {
                table.nextPage()
                onFiltersChange({ ...filters, page: filters.page + 1 })
              }}
            >
              Sau
            </Button>
          </div>
        </div>
      </div>
    </div>
  )
}
