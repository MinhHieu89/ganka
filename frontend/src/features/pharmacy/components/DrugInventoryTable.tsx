import { useMemo, useState, useCallback } from "react"
import { useTranslation } from "react-i18next"
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
import { IconEdit, IconChevronDown, IconChevronRight } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Badge } from "@/shared/components/Badge"
import { DataTable } from "@/shared/components/DataTable"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { createValidationMessages } from "@/shared/lib/validation"
import { toast } from "sonner"
import { IconLoader2 } from "@tabler/icons-react"
import { type DrugInventoryDto, DRUG_FORM_MAP } from "@/features/pharmacy/api/pharmacy-api"
import { useUpdateDrugPricing } from "@/features/pharmacy/api/pharmacy-queries"
import { DrugBatchTable } from "./DrugBatchTable"

interface DrugInventoryTableProps {
  drugs: DrugInventoryDto[]
}

const columnHelper = createColumnHelper<DrugInventoryDto>()

// Pricing dialog schema
function createPricingSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    sellingPrice: z.coerce.number().min(0, v.mustBeNonNegative),
    minStockLevel: z.coerce.number().int().min(0, v.mustBeNonNegative),
  })
}
type PricingValues = z.infer<ReturnType<typeof createPricingSchema>>

interface EditPricingDialogProps {
  drug: DrugInventoryDto | null
  open: boolean
  onOpenChange: (open: boolean) => void
}

function EditPricingDialog({ drug, open, onOpenChange }: EditPricingDialogProps) {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")
  const updatePricing = useUpdateDrugPricing()

  const pricingSchema = useMemo(() => createPricingSchema(tCommon), [tCommon])
  const form = useForm<PricingValues>({
    resolver: zodResolver(pricingSchema),
    defaultValues: {
      sellingPrice: drug?.sellingPrice ?? 0,
      minStockLevel: drug?.minStockLevel ?? 0,
    },
  })

  // Reset when drug changes
  useMemo(() => {
    if (open && drug) {
      form.reset({
        sellingPrice: drug.sellingPrice,
        minStockLevel: drug.minStockLevel,
      })
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, drug])

  const handleSubmit = async (data: PricingValues) => {
    if (!drug) return
    try {
      await updatePricing.mutateAsync({
        drugId: drug.drugCatalogItemId,
        sellingPrice: data.sellingPrice,
        minStockLevel: data.minStockLevel,
      })
      toast.success(t("inventory.pricingUpdated"))
      onOpenChange(false)
    } catch {
      // onError in mutation handles toast
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{t("inventory.editPricing")}</DialogTitle>
          {drug && (
            <p className="text-sm text-muted-foreground">{drug.nameVi || drug.name}</p>
          )}
        </DialogHeader>
        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          <Controller
            name="sellingPrice"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel required htmlFor={field.name}>{t("inventory.sellingPrice")}</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  type="number"
                  min={0}
                  step={100}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />
          <Controller
            name="minStockLevel"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel required htmlFor={field.name}>{t("inventory.minStockLevel")}</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  type="number"
                  min={0}
                  step={1}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={updatePricing.isPending}>
              {updatePricing.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {tCommon("buttons.save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export function DrugInventoryTable({ drugs }: DrugInventoryTableProps) {
  const { t } = useTranslation("pharmacy")
  const [sorting, setSorting] = useState<SortingState>([])
  const [globalFilter, setGlobalFilter] = useState("")
  const [expanded, setExpanded] = useState<ExpandedState>({})
  const [pricingDialogOpen, setPricingDialogOpen] = useState(false)
  const [editingDrug, setEditingDrug] = useState<DrugInventoryDto | null>(null)

  const openPricingDialog = useCallback(
    (drug: DrugInventoryDto, e: React.MouseEvent) => {
      e.stopPropagation()
      setEditingDrug(drug)
      setPricingDialogOpen(true)
    },
    [],
  )

  const columns = useMemo(
    () => [
      // Expand toggle column
      columnHelper.display({
        id: "expander",
        size: 40,
        cell: ({ row }) => (
          <Button
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
      columnHelper.accessor("nameVi", {
        header: () => t("inventory.drugName"),
        cell: (info) => (
          <div>
            <div className="font-medium">{info.getValue()}</div>
            <div className="text-xs text-muted-foreground">{info.row.original.name}</div>
          </div>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("genericName", {
        header: () => t("catalog.genericName"),
        cell: (info) => <span className="text-sm text-muted-foreground">{info.getValue()}</span>,
        enableSorting: true,
      }),
      columnHelper.accessor("form", {
        header: () => t("catalog.form"),
        cell: (info) => {
          const key = DRUG_FORM_MAP[info.getValue()]
          return <span className="text-sm">{key ? t(`form.${key}`) : String(info.getValue())}</span>
        },
        enableSorting: false,
      }),
      columnHelper.accessor("unit", {
        header: () => t("catalog.unit"),
        cell: (info) => <span className="text-sm">{info.getValue()}</span>,
        enableSorting: false,
      }),
      columnHelper.accessor("sellingPrice", {
        header: () => t("inventory.sellingPrice"),
        cell: (info) => {
          const value = info.getValue();
          return (
            <span className="text-sm font-medium">
              {value != null ? `${value.toLocaleString("vi-VN")} ₫` : "—"}
            </span>
          );
        },
        enableSorting: true,
      }),
      columnHelper.accessor("totalStock", {
        header: () => t("inventory.totalStock"),
        cell: (info) => (
          <span className={`text-sm font-medium ${info.row.original.isLowStock ? "text-destructive" : ""}`}>
            {info.getValue()}
          </span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("batchCount", {
        header: () => t("inventory.batchCount"),
        cell: (info) => <span className="text-sm text-muted-foreground">{info.getValue()}</span>,
        enableSorting: false,
      }),
      columnHelper.accessor("minStockLevel", {
        header: () => t("inventory.minStockLevel"),
        cell: (info) => <span className="text-sm text-muted-foreground">{info.getValue()}</span>,
        enableSorting: false,
      }),
      columnHelper.display({
        id: "status",
        header: () => t("inventory.status"),
        cell: ({ row }) => {
          const drug = row.original
          const badges: React.ReactNode[] = []

          if (drug.hasExpiryAlert) {
            badges.push(
              <Badge key="expiry" variant="destructive" className="text-xs">{t("inventory.statusExpiry")}</Badge>
            )
          }

          if (drug.isOutOfStock || drug.totalStock === 0) {
            badges.push(
              <Badge key="oos" variant="destructive" className="text-xs">
                {t("inventory.statusOutOfStock")}
              </Badge>
            )
          } else if (drug.isLowStock) {
            badges.push(
              <Badge key="low" variant="outline" className="text-xs border-yellow-500 text-yellow-700 dark:text-yellow-400">
                {t("inventory.statusLow")}
              </Badge>
            )
          }

          if (badges.length === 0) {
            return <Badge variant="outline" className="text-xs">{t("inventory.statusOk")}</Badge>
          }

          return badges.length === 1 ? badges[0] : (
            <div className="flex flex-col gap-1">{badges}</div>
          )
        },
      }),
      columnHelper.display({
        id: "actions",
        header: () => "",
        cell: ({ row }) => (
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => openPricingDialog(row.original, e)}
            title={t("inventory.editPricing")}
          >
            <IconEdit className="h-4 w-4" />
          </Button>
        ),
      }),
    ],
    [t, openPricingDialog],
  )

  const table = useReactTable({
    data: drugs,
    columns,
    state: { sorting, globalFilter, expanded },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    onExpandedChange: setExpanded,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
    getRowCanExpand: () => true,
    globalFilterFn: (row, _columnId, filterValue: string) => {
      const search = filterValue.toLowerCase()
      const drug = row.original
      return (
        drug.name.toLowerCase().includes(search) ||
        drug.nameVi.toLowerCase().includes(search) ||
        drug.genericName.toLowerCase().includes(search)
      )
    },
  })

  return (
    <div className="space-y-4">
      <Input
        value={globalFilter}
        onChange={(e) => setGlobalFilter(e.target.value)}
        placeholder={t("inventory.search")}
        className="max-w-sm"
      />
      <DataTable
        table={table}
        columns={columns}
        emptyMessage={t("inventory.empty")}
        renderSubRow={(drug) => (
          <div className="p-4 bg-muted/30">
            <DrugBatchTable drugId={drug.drugCatalogItemId} drugName={drug.nameVi || drug.name} />
          </div>
        )}
      />
      <EditPricingDialog
        drug={editingDrug}
        open={pricingDialogOpen}
        onOpenChange={setPricingDialogOpen}
      />
    </div>
  )
}
