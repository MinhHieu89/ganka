import { useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { IconEdit, IconBarcode, IconChevronLeft, IconChevronRight } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Badge } from "@/shared/components/Badge"
import { DataTable } from "@/shared/components/DataTable"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { BarcodeDisplay } from "./BarcodeDisplay"
import {
  type FrameDto,
  FRAME_MATERIAL_MAP,
  FRAME_TYPE_MAP,
  FRAME_GENDER_MAP,
} from "@/features/optical/api/optical-api"

interface FrameCatalogTableProps {
  frames: FrameDto[]
  onEdit: (frame: FrameDto) => void
  onGenerateBarcode: (frameId: string) => void
  isGeneratingBarcode?: boolean
}

const columnHelper = createColumnHelper<FrameDto>()

const ALL_VALUE = "__all__"

function formatVnd(amount: number): string {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(amount)
}

export function FrameCatalogTable({
  frames,
  onEdit,
  onGenerateBarcode,
  isGeneratingBarcode = false,
}: FrameCatalogTableProps) {
  const { t } = useTranslation("optical")
  const [sorting, setSorting] = useState<SortingState>([])
  const [globalFilter, setGlobalFilter] = useState("")
  const [materialFilter, setMaterialFilter] = useState<string>(ALL_VALUE)
  const [frameTypeFilter, setFrameTypeFilter] = useState<string>(ALL_VALUE)
  const [genderFilter, setGenderFilter] = useState<string>(ALL_VALUE)

  const filteredFrames = useMemo(() => {
    return frames.filter((frame) => {
      if (materialFilter !== ALL_VALUE && frame.material !== Number(materialFilter)) return false
      if (frameTypeFilter !== ALL_VALUE && frame.frameType !== Number(frameTypeFilter)) return false
      if (genderFilter !== ALL_VALUE && frame.gender !== Number(genderFilter)) return false
      return true
    })
  }, [frames, materialFilter, frameTypeFilter, genderFilter])

  const columns = useMemo(
    () => [
      columnHelper.accessor("brand", {
        header: t("frames.brand"),
        cell: (info) => <span className="font-medium">{info.getValue()}</span>,
        enableSorting: true,
      }),
      columnHelper.accessor("model", {
        header: t("frames.model"),
        cell: (info) => info.getValue(),
        enableSorting: true,
      }),
      columnHelper.accessor("color", {
        header: t("frames.color"),
        cell: (info) => info.getValue(),
        enableSorting: false,
      }),
      columnHelper.display({
        id: "size",
        header: t("frames.size"),
        cell: ({ row }) => {
          const { lensWidth, bridgeWidth, templeLength } = row.original
          return (
            <span className="font-mono text-sm">
              {lensWidth}-{bridgeWidth}-{templeLength}
            </span>
          )
        },
      }),
      columnHelper.accessor("material", {
        header: t("frames.material"),
        cell: (info) => t(FRAME_MATERIAL_MAP[info.getValue()] ?? String(info.getValue())),
        enableSorting: false,
      }),
      columnHelper.accessor("frameType", {
        header: t("frames.frameType"),
        cell: (info) => t(FRAME_TYPE_MAP[info.getValue()] ?? String(info.getValue())),
        enableSorting: false,
      }),
      columnHelper.accessor("gender", {
        header: t("frames.gender"),
        cell: (info) => t(FRAME_GENDER_MAP[info.getValue()] ?? String(info.getValue())),
        enableSorting: false,
      }),
      columnHelper.accessor("sellingPrice", {
        header: t("frames.sellingPrice"),
        cell: (info) => (
          <span className="font-medium">{formatVnd(info.getValue())}</span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("stockQuantity", {
        header: t("frames.stock"),
        cell: (info) => {
          const qty = info.getValue()
          const isLow = qty <= 0
          return (
            <Badge variant={isLow ? "destructive" : "secondary"}>
              {qty}
            </Badge>
          )
        },
        enableSorting: true,
      }),
      columnHelper.accessor("barcode", {
        header: t("frames.barcode"),
        cell: (info) => {
          const barcode = info.getValue()
          if (!barcode) return <span className="text-muted-foreground text-sm">—</span>
          return (
            <div className="max-w-[200px]">
              <BarcodeDisplay value={barcode} height={40} showText={true} />
            </div>
          )
        },
        enableSorting: false,
      }),
      columnHelper.display({
        id: "actions",
        header: t("common.actions"),
        cell: ({ row }) => (
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEdit(row.original)}
              title={t("frames.editFrame")}
            >
              <IconEdit className="h-4 w-4" />
            </Button>
            {!row.original.barcode && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onGenerateBarcode(row.original.id)}
                disabled={isGeneratingBarcode}
                title={t("frames.generateBarcode")}
              >
                <IconBarcode className="h-4 w-4" />
              </Button>
            )}
          </div>
        ),
      }),
    ],
    [onEdit, onGenerateBarcode, isGeneratingBarcode, t],
  )

  const table = useReactTable({
    data: filteredFrames,
    columns,
    state: { sorting, globalFilter },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: { pagination: { pageSize: 20 } },
    globalFilterFn: (row, _columnId, filterValue: string) => {
      const search = filterValue.toLowerCase()
      const frame = row.original
      return (
        frame.brand.toLowerCase().includes(search) ||
        frame.model.toLowerCase().includes(search) ||
        frame.color.toLowerCase().includes(search) ||
        (frame.barcode?.toLowerCase().includes(search) ?? false)
      )
    },
  })

  return (
    <div className="space-y-4">
      {/* Search and filters row */}
      <div className="flex flex-wrap items-center gap-3">
        <Input
          value={globalFilter}
          onChange={(e) => setGlobalFilter(e.target.value)}
          placeholder={t("frames.search")}
          className="max-w-sm"
        />

        <Select value={materialFilter} onValueChange={setMaterialFilter}>
          <SelectTrigger className="w-36">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>{t("frames.material")}</SelectItem>
            {Object.entries(FRAME_MATERIAL_MAP).map(([value, label]) => (
              <SelectItem key={value} value={value}>
                {t(label)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={frameTypeFilter} onValueChange={setFrameTypeFilter}>
          <SelectTrigger className="w-36">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>{t("frames.frameType")}</SelectItem>
            {Object.entries(FRAME_TYPE_MAP).map(([value, label]) => (
              <SelectItem key={value} value={value}>
                {t(label)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={genderFilter} onValueChange={setGenderFilter}>
          <SelectTrigger className="w-32">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>{t("frames.gender")}</SelectItem>
            {Object.entries(FRAME_GENDER_MAP).map(([value, label]) => (
              <SelectItem key={value} value={value}>
                {t(label)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        {(materialFilter !== ALL_VALUE || frameTypeFilter !== ALL_VALUE || genderFilter !== ALL_VALUE || globalFilter) && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => {
              setMaterialFilter(ALL_VALUE)
              setFrameTypeFilter(ALL_VALUE)
              setGenderFilter(ALL_VALUE)
              setGlobalFilter("")
            }}
          >
            {t("common.clearFilter")}
          </Button>
        )}
      </div>

      {/* Data table */}
      <DataTable
        table={table}
        columns={columns}
        emptyMessage={t("frames.empty")}
      />

      {/* Pagination */}
      {table.getPageCount() > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            {t("frames.showingResults", {
              from: table.getState().pagination.pageIndex * table.getState().pagination.pageSize + 1,
              to: Math.min(
                (table.getState().pagination.pageIndex + 1) * table.getState().pagination.pageSize,
                filteredFrames.length,
              ),
              total: filteredFrames.length,
            })}
          </p>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage()}
            >
              <IconChevronLeft className="h-4 w-4" />
            </Button>
            <span className="text-sm">
              {t("frames.pageOf", {
                current: table.getState().pagination.pageIndex + 1,
                total: table.getPageCount(),
              })}
            </span>
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
