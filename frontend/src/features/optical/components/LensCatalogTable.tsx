import { useMemo, useState } from "react"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getExpandedRowModel,
  useReactTable,
  type SortingState,
  type ExpandedState,
} from "@tanstack/react-table"
import { IconEdit, IconPlus, IconChevronDown, IconChevronRight } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Badge } from "@/shared/components/Badge"
import { DataTable } from "@/shared/components/DataTable"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import {
  type LensCatalogItemDto,
  type LensStockEntryDto,
  LENS_MATERIAL_MAP,
  LENS_COATING_MAP,
  decodeCoatings,
} from "@/features/optical/api/optical-api"

interface LensCatalogTableProps {
  lenses: LensCatalogItemDto[]
  onEdit: (lens: LensCatalogItemDto) => void
  onAdjustStock: (lens: LensCatalogItemDto) => void
}

function formatPower(value: number): string {
  const sign = value >= 0 ? "+" : ""
  return `${sign}${value.toFixed(2)}`
}

function LensStockEntries({ stockEntries }: { stockEntries: LensStockEntryDto[] }) {
  if (!stockEntries.length) {
    return (
      <div className="px-8 py-3 text-sm text-muted-foreground italic">
        No stock entries — click the + button to add a power combination.
      </div>
    )
  }

  return (
    <div className="border-t bg-muted/20">
      <Table>
        <TableHeader>
          <TableRow className="hover:bg-transparent">
            <TableHead className="h-8 text-xs font-medium pl-12">SPH</TableHead>
            <TableHead className="h-8 text-xs font-medium">CYL</TableHead>
            <TableHead className="h-8 text-xs font-medium">ADD</TableHead>
            <TableHead className="h-8 text-xs font-medium">Qty</TableHead>
            <TableHead className="h-8 text-xs font-medium">Min Stock</TableHead>
            <TableHead className="h-8 text-xs font-medium">Status</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {stockEntries.map((entry) => {
            const isLow = entry.quantity <= entry.minStockLevel
            return (
              <TableRow
                key={entry.id}
                className={
                  isLow
                    ? "bg-yellow-50/80 dark:bg-yellow-950/20 hover:bg-yellow-50 dark:hover:bg-yellow-950/30"
                    : "hover:bg-muted/30"
                }
              >
                <TableCell className="py-1.5 pl-12 text-xs font-mono">
                  {formatPower(entry.sph)}
                </TableCell>
                <TableCell className="py-1.5 text-xs font-mono">
                  {formatPower(entry.cyl)}
                </TableCell>
                <TableCell className="py-1.5 text-xs font-mono">
                  {entry.add != null ? formatPower(entry.add) : "—"}
                </TableCell>
                <TableCell className="py-1.5 text-xs">
                  <span
                    className={
                      entry.quantity === 0
                        ? "font-semibold text-destructive"
                        : isLow
                          ? "font-semibold text-yellow-600 dark:text-yellow-400"
                          : "font-medium"
                    }
                  >
                    {entry.quantity}
                  </span>
                </TableCell>
                <TableCell className="py-1.5 text-xs text-muted-foreground">
                  {entry.minStockLevel}
                </TableCell>
                <TableCell className="py-1.5">
                  {entry.quantity === 0 ? (
                    <Badge
                      variant="outline"
                      className="text-xs h-5 border-destructive text-destructive"
                    >
                      Out of stock
                    </Badge>
                  ) : isLow ? (
                    <Badge
                      variant="outline"
                      className="text-xs h-5 border-yellow-500 text-yellow-700 dark:text-yellow-400"
                    >
                      Low stock
                    </Badge>
                  ) : (
                    <Badge
                      variant="outline"
                      className="text-xs h-5 border-green-500 text-green-700 dark:text-green-400"
                    >
                      OK
                    </Badge>
                  )}
                </TableCell>
              </TableRow>
            )
          })}
        </TableBody>
      </Table>
    </div>
  )
}

const columnHelper = createColumnHelper<LensCatalogItemDto>()

export function LensCatalogTable({
  lenses,
  onEdit,
  onAdjustStock,
}: LensCatalogTableProps) {
  const [sorting, setSorting] = useState<SortingState>([])
  const [globalFilter, setGlobalFilter] = useState("")
  const [expanded, setExpanded] = useState<ExpandedState>({})

  const columns = useMemo(
    () => [
      columnHelper.display({
        id: "expand",
        header: () => null,
        cell: ({ row }) => (
          <Button
            variant="ghost"
            size="sm"
            className="h-7 w-7 p-0"
            onClick={(e) => {
              e.stopPropagation()
              row.toggleExpanded()
            }}
            title={
              row.getIsExpanded()
                ? "Hide stock entries"
                : `Show ${row.original.stockEntries.length} stock entries`
            }
          >
            {row.getIsExpanded() ? (
              <IconChevronDown className="h-4 w-4" />
            ) : (
              <IconChevronRight className="h-4 w-4" />
            )}
          </Button>
        ),
        size: 40,
      }),
      columnHelper.accessor("brand", {
        header: () => "Brand",
        cell: (info) => (
          <span className="font-medium">{info.getValue()}</span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("name", {
        header: () => "Name",
        cell: (info) => info.getValue(),
        enableSorting: true,
      }),
      columnHelper.accessor("lensType", {
        header: () => "Lens Type",
        cell: (info) => {
          const typeMap: Record<string, string> = {
            single_vision: "Single Vision",
            bifocal: "Bifocal",
            progressive: "Progressive",
            reading: "Reading",
          }
          return typeMap[info.getValue()] ?? info.getValue()
        },
        enableSorting: true,
      }),
      columnHelper.accessor("material", {
        header: () => "Material",
        cell: (info) => LENS_MATERIAL_MAP[info.getValue()] ?? String(info.getValue()),
        enableSorting: false,
      }),
      columnHelper.accessor("availableCoatings", {
        header: () => "Coatings",
        cell: (info) => {
          const coatingBits = decodeCoatings(info.getValue())
          if (!coatingBits.length)
            return <span className="text-muted-foreground text-xs">None</span>
          return (
            <div className="flex flex-wrap gap-1">
              {coatingBits.map((bit) => (
                <Badge key={bit} variant="secondary" className="text-xs h-5">
                  {LENS_COATING_MAP[bit] ?? String(bit)}
                </Badge>
              ))}
            </div>
          )
        },
        enableSorting: false,
      }),
      columnHelper.accessor("sellingPrice", {
        header: () => "Sell Price",
        cell: (info) => (
          <span className="font-mono text-sm">
            {info.getValue().toLocaleString("vi-VN")}
          </span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("costPrice", {
        header: () => "Cost Price",
        cell: (info) => (
          <span className="font-mono text-sm text-muted-foreground">
            {info.getValue().toLocaleString("vi-VN")}
          </span>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor(
        (row) => row.stockEntries.reduce((sum, e) => sum + e.quantity, 0),
        {
          id: "totalStock",
          header: () => "Total Stock",
          cell: (info) => {
            const total = info.getValue()
            const hasLow = info.row.original.stockEntries.some(
              (e) => e.quantity <= e.minStockLevel,
            )
            return (
              <span
                className={
                  total === 0
                    ? "font-semibold text-destructive"
                    : hasLow
                      ? "font-semibold text-yellow-600 dark:text-yellow-400"
                      : "font-medium"
                }
              >
                {total}
              </span>
            )
          },
          enableSorting: true,
        },
      ),
      columnHelper.accessor("isActive", {
        header: () => "Status",
        cell: (info) =>
          info.getValue() ? (
            <Badge
              variant="outline"
              className="border-green-500 text-green-700 dark:text-green-400"
            >
              Active
            </Badge>
          ) : (
            <Badge variant="outline" className="text-muted-foreground">
              Inactive
            </Badge>
          ),
        enableSorting: false,
      }),
      columnHelper.display({
        id: "actions",
        header: () => null,
        cell: ({ row }) => (
          <div className="flex items-center gap-1 justify-end">
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => {
                e.stopPropagation()
                onAdjustStock(row.original)
              }}
              title="Add / adjust stock entry"
            >
              <IconPlus className="h-4 w-4" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => {
                e.stopPropagation()
                onEdit(row.original)
              }}
              title="Edit lens catalog item"
            >
              <IconEdit className="h-4 w-4" />
            </Button>
          </div>
        ),
      }),
    ],
    [onEdit, onAdjustStock],
  )

  const table = useReactTable({
    data: lenses,
    columns,
    state: { sorting, globalFilter, expanded },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    onExpandedChange: setExpanded,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
    globalFilterFn: (row, _columnId, filterValue: string) => {
      const search = filterValue.toLowerCase()
      const lens = row.original
      return (
        lens.brand.toLowerCase().includes(search) ||
        lens.name.toLowerCase().includes(search) ||
        lens.lensType.toLowerCase().includes(search)
      )
    },
  })

  return (
    <div className="space-y-4">
      <Input
        value={globalFilter}
        onChange={(e) => setGlobalFilter(e.target.value)}
        placeholder="Search by brand, name, or type..."
        className="max-w-sm"
      />
      <DataTable
        table={table}
        columns={columns}
        emptyMessage="No lens catalog items found."
        renderSubRow={(lens) => (
          <LensStockEntries stockEntries={lens.stockEntries} />
        )}
      />
    </div>
  )
}
