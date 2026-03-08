import { useMemo, useState } from "react"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { IconCheck, IconAlertTriangle, IconChevronLeft, IconChevronRight } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { DataTable } from "@/shared/components/DataTable"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { OrderStatusBadge } from "./OrderStatusBadge"
import { type GlassesOrderDto, PROCESSING_TYPE_MAP } from "@/features/optical/api/optical-api"
import { formatVND } from "@/shared/lib/format-vnd"
import { cn } from "@/shared/lib/utils"

interface GlassesOrderTableProps {
  orders: GlassesOrderDto[]
  onRowClick?: (order: GlassesOrderDto) => void
  statusFilter: number | undefined
  onStatusFilterChange: (status: number | undefined) => void
}

const columnHelper = createColumnHelper<GlassesOrderDto>()

const STATUS_FILTER_OPTIONS = [
  { value: "all", label: "All Statuses" },
  { value: "0", label: "Ordered" },
  { value: "1", label: "Processing" },
  { value: "2", label: "Received" },
  { value: "3", label: "Ready for Pickup" },
  { value: "4", label: "Delivered" },
]

export function GlassesOrderTable({
  orders,
  onRowClick,
  statusFilter,
  onStatusFilterChange,
}: GlassesOrderTableProps) {
  const [sorting, setSorting] = useState<SortingState>([{ id: "createdAt", desc: true }])
  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 20 })

  const columns = useMemo(
    () => [
      columnHelper.accessor("id", {
        id: "orderNumber",
        header: () => "Order #",
        cell: (info) => (
          <span className="font-mono text-xs text-muted-foreground">
            {info.getValue().substring(0, 8).toUpperCase()}
          </span>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("patientName", {
        header: () => "Patient Name",
        cell: (info) => <span className="font-medium">{info.getValue()}</span>,
        enableSorting: true,
      }),
      columnHelper.accessor("status", {
        header: () => "Status",
        cell: (info) => (
          <OrderStatusBadge
            status={info.getValue()}
            isOverdue={info.row.original.isOverdue}
            isPaymentConfirmed={info.row.original.isPaymentConfirmed}
          />
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("processingType", {
        header: () => "Processing",
        cell: (info) => (
          <span className="text-sm">
            {PROCESSING_TYPE_MAP[info.getValue()] ?? `Type ${info.getValue()}`}
          </span>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("totalPrice", {
        header: () => "Total Price",
        cell: (info) => (
          <span className="font-medium tabular-nums">{formatVND(info.getValue())}</span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("isPaymentConfirmed", {
        header: () => "Payment",
        cell: (info) =>
          info.getValue() ? (
            <span className="inline-flex items-center gap-1 text-green-600 dark:text-green-400 text-sm font-medium">
              <IconCheck className="h-3.5 w-3.5" />
              Confirmed
            </span>
          ) : (
            <span className="inline-flex items-center gap-1 text-orange-600 dark:text-orange-400 text-sm">
              <IconAlertTriangle className="h-3.5 w-3.5" />
              Pending
            </span>
          ),
        enableSorting: false,
      }),
      columnHelper.accessor("estimatedDeliveryDate", {
        header: () => "Est. Delivery",
        cell: (info) => {
          const val = info.getValue()
          if (!val) return <span className="text-muted-foreground text-sm">-</span>
          const date = new Date(val)
          const isOverdue = info.row.original.isOverdue
          return (
            <span
              className={cn(
                "text-sm",
                isOverdue ? "text-red-600 dark:text-red-400 font-medium" : "",
              )}
            >
              {date.toLocaleDateString("vi-VN")}
            </span>
          )
        },
        enableSorting: true,
      }),
      columnHelper.accessor("createdAt", {
        header: () => "Created At",
        cell: (info) => (
          <span className="text-sm text-muted-foreground">
            {new Date(info.getValue()).toLocaleDateString("vi-VN")}
          </span>
        ),
        enableSorting: true,
      }),
    ],
    [],
  )

  const table = useReactTable({
    data: orders,
    columns,
    state: { sorting, pagination },
    onSortingChange: setSorting,
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  })

  const rowClassName = (order: GlassesOrderDto) => {
    if (order.isOverdue) {
      return "bg-red-50/50 hover:bg-red-100/50 dark:bg-red-950/20 dark:hover:bg-red-950/30"
    }
    return ""
  }

  const handleStatusChange = (value: string) => {
    if (value === "all") {
      onStatusFilterChange(undefined)
    } else {
      onStatusFilterChange(Number(value))
    }
  }

  const totalPages = table.getPageCount()
  const currentPage = pagination.pageIndex + 1

  return (
    <div className="space-y-4">
      {/* Filter row */}
      <div className="flex items-center gap-3">
        <Select
          value={statusFilter !== undefined ? String(statusFilter) : "all"}
          onValueChange={handleStatusChange}
        >
          <SelectTrigger className="w-[200px]">
            <SelectValue placeholder="Filter by status" />
          </SelectTrigger>
          <SelectContent>
            {STATUS_FILTER_OPTIONS.map((opt) => (
              <SelectItem key={opt.value} value={opt.value}>
                {opt.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <span className="text-sm text-muted-foreground ml-auto">
          {orders.length} order{orders.length !== 1 ? "s" : ""}
        </span>
      </div>

      <DataTable
        table={table}
        columns={columns}
        emptyMessage="No glasses orders found."
        onRowClick={onRowClick}
        rowClassName={rowClassName}
      />

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">
            Page {currentPage} of {totalPages}
          </span>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage()}
            >
              <IconChevronLeft className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.nextPage()}
              disabled={!table.getCanNextPage()}
            >
              <IconChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
