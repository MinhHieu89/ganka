import { useMemo, useState, useCallback } from "react"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { IconEdit, IconPlus, IconAdjustments } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Badge } from "@/shared/components/Badge"
import { DataTable } from "@/shared/components/DataTable"
import { type ConsumableItemDto } from "@/features/consumables/api/consumables-api"
import { useConsumableItems } from "@/features/consumables/api/consumables-queries"
import { ConsumableItemForm } from "./ConsumableItemForm"
import { AddStockDialog } from "./AddStockDialog"
import { ConsumableAdjustDialog } from "./ConsumableAdjustDialog"

// Tracking mode enum values (matches backend ConsumableTrackingMode)
const TRACKING_MODE_EXPIRY = 0
const TRACKING_MODE_SIMPLE = 1

const columnHelper = createColumnHelper<ConsumableItemDto>()

export function ConsumableItemTable() {
  const { data: items = [], isLoading } = useConsumableItems()
  const [sorting, setSorting] = useState<SortingState>([])
  const [globalFilter, setGlobalFilter] = useState("")

  // Edit dialog state
  const [editItem, setEditItem] = useState<ConsumableItemDto | null>(null)
  const [editDialogOpen, setEditDialogOpen] = useState(false)

  // Add stock dialog state
  const [addStockItem, setAddStockItem] = useState<ConsumableItemDto | null>(null)
  const [addStockDialogOpen, setAddStockDialogOpen] = useState(false)

  // Adjust stock dialog state
  const [adjustItem, setAdjustItem] = useState<ConsumableItemDto | null>(null)
  const [adjustDialogOpen, setAdjustDialogOpen] = useState(false)

  const openEditDialog = useCallback((item: ConsumableItemDto, e: React.MouseEvent) => {
    e.stopPropagation()
    setEditItem(item)
    setEditDialogOpen(true)
  }, [])

  const openAddStockDialog = useCallback((item: ConsumableItemDto, e: React.MouseEvent) => {
    e.stopPropagation()
    setAddStockItem(item)
    setAddStockDialogOpen(true)
  }, [])

  const openAdjustDialog = useCallback((item: ConsumableItemDto, e: React.MouseEvent) => {
    e.stopPropagation()
    setAdjustItem(item)
    setAdjustDialogOpen(true)
  }, [])

  const columns = useMemo(
    () => [
      columnHelper.accessor("nameVi", {
        header: "Tên vật tư",
        cell: (info) => (
          <div>
            <div className="font-medium">{info.getValue()}</div>
            <div className="text-xs text-muted-foreground">{info.row.original.name}</div>
          </div>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("unit", {
        header: "Đơn vị",
        cell: (info) => <span className="text-sm">{info.getValue()}</span>,
        enableSorting: false,
      }),
      columnHelper.accessor("trackingMode", {
        header: "Phân loại",
        cell: (info) => {
          const mode = info.getValue()
          return mode === TRACKING_MODE_EXPIRY ? (
            <Badge variant="outline" className="text-xs border-blue-500 text-blue-700 dark:text-blue-400">
              Theo lô
            </Badge>
          ) : (
            <Badge variant="outline" className="text-xs">
              Đơn giản
            </Badge>
          )
        },
        enableSorting: false,
      }),
      columnHelper.accessor("currentStock", {
        header: "Tồn kho",
        cell: (info) => (
          <span
            className={`text-sm font-medium ${
              info.row.original.isLowStock ? "text-destructive" : ""
            }`}
          >
            {info.getValue()}
          </span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("minStockLevel", {
        header: "Tồn kho tối thiểu",
        cell: (info) => (
          <span className="text-sm text-muted-foreground">{info.getValue()}</span>
        ),
        enableSorting: false,
      }),
      columnHelper.display({
        id: "status",
        header: "Trạng thái",
        cell: ({ row }) => {
          const item = row.original
          if (item.isOutOfStock || item.currentStock === 0) {
            return (
              <Badge variant="destructive" className="text-xs">
                Hết hàng
              </Badge>
            )
          }
          if (item.isLowStock) {
            return (
              <Badge
                variant="outline"
                className="text-xs border-yellow-500 text-yellow-700 dark:text-yellow-400"
              >
                Sắp hết
              </Badge>
            )
          }
          return (
            <Badge variant="outline" className="text-xs">
              Còn hàng
            </Badge>
          )
        },
      }),
      columnHelper.display({
        id: "actions",
        header: "",
        cell: ({ row }) => (
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => openAddStockDialog(row.original, e)}
              title="Thêm hàng"
            >
              <IconPlus className="h-4 w-4" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => openAdjustDialog(row.original, e)}
              title="Điều chỉnh"
            >
              <IconAdjustments className="h-4 w-4" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => openEditDialog(row.original, e)}
              title="Chỉnh sửa"
            >
              <IconEdit className="h-4 w-4" />
            </Button>
          </div>
        ),
      }),
    ],
    [openAddStockDialog, openAdjustDialog, openEditDialog],
  )

  const table = useReactTable({
    data: items,
    columns,
    state: { sorting, globalFilter },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    globalFilterFn: (row, _columnId, filterValue: string) => {
      const search = filterValue.toLowerCase()
      const item = row.original
      return (
        item.name.toLowerCase().includes(search) ||
        item.nameVi.toLowerCase().includes(search)
      )
    },
  })

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(5)].map((_, i) => (
          <div key={i} className="h-10 bg-muted animate-pulse rounded" />
        ))}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <Input
        value={globalFilter}
        onChange={(e) => setGlobalFilter(e.target.value)}
        placeholder="Tìm kiếm vật tư..."
        className="max-w-sm"
      />
      <DataTable
        table={table}
        columns={columns}
        emptyMessage="Không có vật tư nào"
      />

      {/* Edit dialog */}
      <ConsumableItemForm
        mode="edit"
        item={editItem ?? undefined}
        open={editDialogOpen}
        onOpenChange={setEditDialogOpen}
      />

      {/* Add stock dialog */}
      <AddStockDialog
        item={addStockItem}
        open={addStockDialogOpen}
        onOpenChange={setAddStockDialogOpen}
      />

      {/* Adjust dialog */}
      <ConsumableAdjustDialog
        item={adjustItem}
        open={adjustDialogOpen}
        onOpenChange={setAdjustDialogOpen}
      />
    </div>
  )
}

// Tracking mode constant for external use
export { TRACKING_MODE_SIMPLE, TRACKING_MODE_EXPIRY }
