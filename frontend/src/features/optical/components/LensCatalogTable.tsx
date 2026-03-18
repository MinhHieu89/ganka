import { useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useForm, Controller } from "react-hook-form"
import { toast } from "sonner"
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
import { IconEdit, IconPlus, IconChevronDown, IconChevronRight, IconAdjustments, IconArrowRight } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { NumberInput } from "@/shared/components/NumberInput"
import { Badge } from "@/shared/components/Badge"
import { DataTable } from "@/shared/components/DataTable"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Field, FieldLabel } from "@/shared/components/Field"
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
import { useAdjustLensStock } from "@/features/optical/api/optical-queries"

interface LensCatalogTableProps {
  lenses: LensCatalogItemDto[]
  onEdit: (lens: LensCatalogItemDto) => void
  onAdjustStock: (lens: LensCatalogItemDto) => void
}

function formatPower(value: number): string {
  const sign = value >= 0 ? "+" : ""
  return `${sign}${value.toFixed(2)}`
}

// ---- Inline Stock Adjust Dialog ----

function InlineStockAdjustDialog({
  open,
  onOpenChange,
  entry,
  lensName,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  entry: LensStockEntryDto
  lensName: string
}) {
  const { t } = useTranslation("optical")
  const adjustMutation = useAdjustLensStock()
  const form = useForm({
    defaultValues: {
      quantityChange: 0 as number | string,
      minStockLevel: entry.minStockLevel as number | string,
    },
  })

  const rawQty = form.watch("quantityChange")
  const quantityChange = typeof rawQty === "string" ? parseInt(rawQty, 10) || 0 : rawQty || 0
  const rawMin = form.watch("minStockLevel")
  const minStockLevel = typeof rawMin === "string" ? parseInt(rawMin, 10) || 0 : rawMin || 0
  const afterAdjust = entry.quantity + quantityChange

  const handleSubmit = async () => {
    const data = { quantityChange, minStockLevel }
    try {
      await adjustMutation.mutateAsync({
        lensCatalogItemId: entry.lensCatalogItemId,
        sph: entry.sph,
        cyl: entry.cyl,
        add: entry.add ?? null,
        quantityChange: data.quantityChange,
        reason: "Manual stock adjustment",
        minStockLevel: data.minStockLevel,
      })
      toast.success(t("lenses.stockAdjusted"))
      onOpenChange(false)
    } catch {
      // onError in mutation handles toast
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{t("lenses.adjustStock")}</DialogTitle>
        </DialogHeader>

        <div className="rounded-md bg-muted/50 px-3 py-2 text-sm space-y-1">
          <div className="font-medium">{lensName}</div>
          <div className="flex gap-4 text-xs text-muted-foreground font-mono">
            <span>SPH: {formatPower(entry.sph)}</span>
            <span>CYL: {formatPower(entry.cyl)}</span>
            {entry.add != null && <span>ADD: {formatPower(entry.add)}</span>}
          </div>
        </div>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Current → After preview */}
          <div className="flex items-center gap-3 rounded-lg border bg-muted/30 px-4 py-3">
            <div className="text-center">
              <p className="text-xs text-muted-foreground">{t("lenses.currentQty")}</p>
              <p className="text-xl font-bold">{entry.quantity}</p>
            </div>
            <IconArrowRight className="h-5 w-5 text-muted-foreground shrink-0" />
            <div className="text-center">
              <p className="text-xs text-muted-foreground">{t("lenses.afterAdjust")}</p>
              <p className={`text-xl font-bold ${afterAdjust < 0 ? "text-destructive" : afterAdjust !== entry.quantity ? "text-primary" : ""}`}>
                {afterAdjust}
              </p>
            </div>
          </div>

          <Controller
            name="quantityChange"
            control={form.control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor={field.name}>{t("lenses.quantityChange")}</FieldLabel>
                <NumberInput
                  {...field}
                  id={field.name}
                  step={1}
                />
                <p className="text-xs text-muted-foreground mt-1">{t("lenses.quantityChangeHint")}</p>
              </Field>
            )}
          />

          <Controller
            name="minStockLevel"
            control={form.control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor={field.name}>{t("lenses.minStock")}</FieldLabel>
                <NumberInput
                  {...field}
                  id={field.name}
                  step={1}
                  min={0}
                />
              </Field>
            )}
          />

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t("common.cancel")}
            </Button>
            <Button type="submit" disabled={adjustMutation.isPending || afterAdjust < 0}>
              {t("common.save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// ---- Stock Entries Sub-Table ----

function LensStockEntries({ stockEntries, lensName }: { stockEntries: LensStockEntryDto[]; lensName: string }) {
  const { t } = useTranslation("optical")
  const [adjustEntry, setAdjustEntry] = useState<LensStockEntryDto | null>(null)

  if (!stockEntries.length) {
    return (
      <div className="px-8 py-3 text-sm text-muted-foreground italic">
        {t("lenses.empty")}
      </div>
    )
  }

  return (
    <div className="border-t bg-muted/20">
      <Table>
        <TableHeader>
          <TableRow className="hover:bg-transparent">
            <TableHead className="h-8 text-xs font-medium pl-12">{t("lenses.sph")}</TableHead>
            <TableHead className="h-8 text-xs font-medium">{t("lenses.cyl")}</TableHead>
            <TableHead className="h-8 text-xs font-medium">{t("lenses.add")}</TableHead>
            <TableHead className="h-8 text-xs font-medium">{t("lenses.quantity")}</TableHead>
            <TableHead className="h-8 text-xs font-medium">{t("lenses.minStock")}</TableHead>
            <TableHead className="h-8 text-xs font-medium">{t("lenses.status")}</TableHead>
            <TableHead className="h-8 text-xs font-medium w-10" />
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
                      {t("frames.outOfStock")}
                    </Badge>
                  ) : isLow ? (
                    <Badge
                      variant="outline"
                      className="text-xs h-5 border-yellow-500 text-yellow-700 dark:text-yellow-400"
                    >
                      {t("frames.lowStock")}
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
                <TableCell className="py-1.5">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-6 w-6 p-0"
                    onClick={() => setAdjustEntry(entry)}
                    title={t("lenses.adjustStock")}
                  >
                    <IconAdjustments className="h-3.5 w-3.5" />
                  </Button>
                </TableCell>
              </TableRow>
            )
          })}
        </TableBody>
      </Table>

      {adjustEntry && (
        <InlineStockAdjustDialog
          open={!!adjustEntry}
          onOpenChange={(open) => { if (!open) setAdjustEntry(null) }}
          entry={adjustEntry}
          lensName={lensName}
        />
      )}
    </div>
  )
}

const columnHelper = createColumnHelper<LensCatalogItemDto>()

export function LensCatalogTable({
  lenses,
  onEdit,
  onAdjustStock,
}: LensCatalogTableProps) {
  const { t } = useTranslation("optical")
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
        header: () => t("lenses.brand"),
        cell: (info) => (
          <span className="font-medium">{info.getValue()}</span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("name", {
        header: () => t("lenses.name"),
        cell: (info) => info.getValue(),
        enableSorting: true,
      }),
      columnHelper.accessor("lensType", {
        header: () => t("lenses.lensType"),
        cell: (info) => {
          const typeMap: Record<string, string> = {
            single_vision: t("enums.lensType.singleVision"),
            bifocal: t("enums.lensType.bifocal"),
            progressive: t("enums.lensType.progressive"),
            reading: t("enums.lensType.reading"),
          }
          return typeMap[info.getValue()] ?? info.getValue()
        },
        enableSorting: true,
      }),
      columnHelper.accessor("material", {
        header: () => t("lenses.material"),
        cell: (info) => t(LENS_MATERIAL_MAP[info.getValue()] ?? String(info.getValue())),
        enableSorting: false,
      }),
      columnHelper.accessor("availableCoatings", {
        header: () => t("lenses.coatings"),
        cell: (info) => {
          const coatingBits = decodeCoatings(info.getValue())
          if (!coatingBits.length)
            return <span className="text-muted-foreground text-xs">None</span>
          return (
            <div className="flex flex-wrap gap-1">
              {coatingBits.map((bit) => (
                <Badge key={bit} variant="secondary" className="text-xs h-5">
                  {t(LENS_COATING_MAP[bit] ?? String(bit))}
                </Badge>
              ))}
            </div>
          )
        },
        enableSorting: false,
      }),
      columnHelper.accessor("sellingPrice", {
        header: () => t("lenses.sellingPrice"),
        cell: (info) => (
          <span className="font-mono text-sm">
            {info.getValue().toLocaleString("vi-VN")}
          </span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("costPrice", {
        header: () => t("lenses.costPrice"),
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
          header: () => t("lenses.totalStock"),
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
        header: () => t("lenses.status"),
        cell: (info) =>
          info.getValue() ? (
            <Badge
              variant="outline"
              className="border-green-500 text-green-700 dark:text-green-400"
            >
              {t("lenses.active")}
            </Badge>
          ) : (
            <Badge variant="outline" className="text-muted-foreground">
              {t("lenses.inactive")}
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
              title={t("lenses.adjustStock")}
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
              title={t("lenses.editLens")}
            >
              <IconEdit className="h-4 w-4" />
            </Button>
          </div>
        ),
      }),
    ],
    [onEdit, onAdjustStock, t],
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
        placeholder={t("lenses.search")}
        className="max-w-sm"
      />
      <DataTable
        table={table}
        columns={columns}
        emptyMessage={t("lenses.empty")}
        renderSubRow={(lens) => (
          <LensStockEntries stockEntries={lens.stockEntries} lensName={`${lens.brand} ${lens.name}`} />
        )}
      />
    </div>
  )
}
