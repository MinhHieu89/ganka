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
import { IconEdit } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Checkbox } from "@/shared/components/Checkbox"
import { Label } from "@/shared/components/Label"
import { DataTable } from "@/shared/components/DataTable"
import { formatVND } from "@/shared/lib/format-vnd"
import type { ServiceCatalogItemDto } from "@/features/billing/api/service-catalog-api"

interface ServiceCatalogTableProps {
  items: ServiceCatalogItemDto[]
  includeInactive: boolean
  onIncludeInactiveChange: (value: boolean) => void
  onEdit: (item: ServiceCatalogItemDto) => void
}

const columnHelper = createColumnHelper<ServiceCatalogItemDto>()

export function ServiceCatalogTable({
  items,
  includeInactive,
  onIncludeInactiveChange,
  onEdit,
}: ServiceCatalogTableProps) {
  const { t } = useTranslation("billing")
  const [sorting, setSorting] = useState<SortingState>([])
  const [globalFilter, setGlobalFilter] = useState("")

  const columns = useMemo(
    () => [
      columnHelper.accessor("code", {
        header: () => t("serviceCatalog.code"),
        cell: (info) => (
          <span className="font-mono text-sm">{info.getValue()}</span>
        ),
      }),
      columnHelper.accessor("name", {
        header: () => t("serviceCatalog.name"),
        cell: (info) => info.getValue(),
      }),
      columnHelper.accessor("nameVi", {
        header: () => t("serviceCatalog.nameVi"),
        cell: (info) => info.getValue(),
      }),
      columnHelper.accessor("price", {
        header: () => t("serviceCatalog.price"),
        cell: (info) => formatVND(info.getValue()),
      }),
      columnHelper.accessor("isActive", {
        header: () => t("serviceCatalog.status"),
        cell: (info) =>
          info.getValue() ? (
            <Badge variant="default">{t("serviceCatalog.active")}</Badge>
          ) : (
            <Badge variant="secondary">{t("serviceCatalog.inactive")}</Badge>
          ),
      }),
      columnHelper.display({
        id: "actions",
        cell: (info) => (
          <Button
            variant="ghost"
            size="icon"
            onClick={(e) => {
              e.stopPropagation()
              onEdit(info.row.original)
            }}
          >
            <IconEdit className="h-4 w-4" />
          </Button>
        ),
      }),
    ],
    [t, onEdit],
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
  })

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-4">
        <div className="flex items-center space-x-2">
          <Checkbox
            id="show-inactive"
            checked={includeInactive}
            onCheckedChange={(checked) =>
              onIncludeInactiveChange(checked === true)
            }
          />
          <Label htmlFor="show-inactive" className="text-sm font-normal cursor-pointer">
            {t("serviceCatalog.showInactive")}
          </Label>
        </div>
      </div>

      <DataTable
        table={table}
        columns={columns}
        emptyMessage={t("serviceCatalog.noItems")}
      />
    </div>
  )
}
