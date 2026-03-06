import { createFileRoute } from "@tanstack/react-router"
import { useState, useMemo, useCallback } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { IconPlus, IconEdit, IconToggleRight, IconToggleLeft } from "@tabler/icons-react"
import { toast } from "sonner"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Badge } from "@/shared/components/Badge"
import { DataTable } from "@/shared/components/DataTable"
import { Skeleton } from "@/shared/components/Skeleton"
import { SupplierForm } from "@/features/pharmacy/components/SupplierForm"
import type { SupplierDto } from "@/features/pharmacy/api/pharmacy-api"
import {
  useSuppliers,
  useUpdateSupplier,
} from "@/features/pharmacy/api/pharmacy-queries"

export const Route = createFileRoute("/_authenticated/pharmacy/suppliers")({
  component: SuppliersPage,
})

const columnHelper = createColumnHelper<SupplierDto>()

function SuppliersPage() {
  const { t } = useTranslation("pharmacy")
  const { data: suppliers, isLoading } = useSuppliers()
  const updateSupplier = useUpdateSupplier()

  const [sorting, setSorting] = useState<SortingState>([])
  const [globalFilter, setGlobalFilter] = useState("")
  const [formOpen, setFormOpen] = useState(false)
  const [editingSupplier, setEditingSupplier] = useState<SupplierDto | undefined>(undefined)

  const openCreate = useCallback(() => {
    setEditingSupplier(undefined)
    setFormOpen(true)
  }, [])

  const openEdit = useCallback((supplier: SupplierDto) => {
    setEditingSupplier(supplier)
    setFormOpen(true)
  }, [])

  const handleToggleActive = useCallback(
    async (supplier: SupplierDto) => {
      try {
        await updateSupplier.mutateAsync({
          id: supplier.id,
          name: supplier.name,
          contactInfo: supplier.contactInfo,
          isActive: !supplier.isActive,
        })
        toast.success(
          supplier.isActive ? t("supplier.deactivated") : t("supplier.activated"),
        )
      } catch {
        // onError in mutation handles toast
      }
    },
    [updateSupplier, t],
  )

  const columns = useMemo(
    () => [
      columnHelper.accessor("name", {
        header: () => t("supplier.name"),
        cell: (info) => <span className="font-medium">{info.getValue()}</span>,
        enableSorting: true,
      }),
      columnHelper.accessor("contactInfo", {
        header: () => t("supplier.contactInfo"),
        cell: (info) => (
          <span className="text-sm text-muted-foreground whitespace-pre-line">
            {info.getValue() ?? "—"}
          </span>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("isActive", {
        header: () => t("catalog.active"),
        cell: (info) =>
          info.getValue() ? (
            <Badge variant="outline" className="border-green-500 text-green-700 dark:text-green-400">
              {t("catalog.active")}
            </Badge>
          ) : (
            <Badge variant="outline" className="border-muted-foreground text-muted-foreground">
              {t("catalog.inactive")}
            </Badge>
          ),
        enableSorting: true,
      }),
      columnHelper.display({
        id: "actions",
        header: () => "",
        cell: ({ row }) => {
          const supplier = row.original
          return (
            <div className="flex items-center gap-1 justify-end">
              <Button
                variant="ghost"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation()
                  openEdit(supplier)
                }}
                title={t("supplier.editSupplier")}
              >
                <IconEdit className="h-4 w-4" />
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation()
                  handleToggleActive(supplier)
                }}
                title={supplier.isActive ? t("supplier.deactivate") : t("supplier.activate")}
              >
                {supplier.isActive ? (
                  <IconToggleRight className="h-4 w-4 text-green-600" />
                ) : (
                  <IconToggleLeft className="h-4 w-4 text-muted-foreground" />
                )}
              </Button>
            </div>
          )
        },
      }),
    ],
    [t, openEdit, handleToggleActive],
  )

  const table = useReactTable({
    data: suppliers ?? [],
    columns,
    state: { sorting, globalFilter },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    globalFilterFn: (row, _columnId, filterValue: string) => {
      const search = filterValue.toLowerCase()
      const supplier = row.original
      return (
        supplier.name.toLowerCase().includes(search) ||
        (supplier.contactInfo?.toLowerCase().includes(search) ?? false)
      )
    },
  })

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("supplier.title")}</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {t("supplier.subtitle")}
          </p>
        </div>
        <Button onClick={openCreate}>
          <IconPlus className="h-4 w-4 mr-2" />
          {t("supplier.addSupplier")}
        </Button>
      </div>

      {/* Filter */}
      <Input
        value={globalFilter}
        onChange={(e) => setGlobalFilter(e.target.value)}
        placeholder={t("supplier.search")}
        className="max-w-sm"
      />

      {/* Table */}
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
          emptyMessage={t("supplier.empty")}
        />
      )}

      {/* Create/Edit dialog */}
      <SupplierForm
        supplier={editingSupplier}
        open={formOpen}
        onOpenChange={setFormOpen}
      />
    </div>
  )
}
