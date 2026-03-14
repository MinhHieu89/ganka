import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  getCoreRowModel,
  useReactTable,
  type PaginationState,
} from "@tanstack/react-table"
import { IconEdit } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { DataTable } from "@/shared/components/DataTable"
import {
  type DrugCatalogItemDto,
  DRUG_FORM_MAP,
  DRUG_ROUTE_MAP,
} from "@/features/pharmacy/api/pharmacy-api"

interface DrugCatalogTableProps {
  drugs: DrugCatalogItemDto[]
  onEdit: (drug: DrugCatalogItemDto) => void
  pagination: PaginationState
  onPaginationChange: (pagination: PaginationState) => void
  pageCount: number
}

const columnHelper = createColumnHelper<DrugCatalogItemDto>()

export function DrugCatalogTable({
  drugs,
  onEdit,
  pagination,
  onPaginationChange,
  pageCount,
}: DrugCatalogTableProps) {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")

  const columns = useMemo(
    () => [
      columnHelper.accessor("name", {
        header: () => t("catalog.name"),
        cell: (info) => (
          <span className="font-medium">{info.getValue()}</span>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("genericName", {
        header: () => t("catalog.genericName"),
        cell: (info) => info.getValue(),
        enableSorting: false,
      }),
      columnHelper.accessor("form", {
        header: () => t("catalog.form"),
        cell: (info) => {
          const key = DRUG_FORM_MAP[info.getValue()]
          return key ? t(`form.${key}`) : String(info.getValue())
        },
        enableSorting: false,
      }),
      columnHelper.accessor("strength", {
        header: () => t("catalog.strength"),
        cell: (info) => info.getValue() ?? "-",
        enableSorting: false,
      }),
      columnHelper.accessor("route", {
        header: () => t("catalog.route"),
        cell: (info) => {
          const key = DRUG_ROUTE_MAP[info.getValue()]
          return key ? t(`route.${key}`) : String(info.getValue())
        },
        enableSorting: false,
      }),
      columnHelper.accessor("unit", {
        header: () => t("catalog.unit"),
        cell: (info) => info.getValue(),
        enableSorting: false,
      }),
      columnHelper.accessor("defaultDosageTemplate", {
        header: () => t("catalog.defaultDosage"),
        cell: (info) => info.getValue() ?? "-",
        enableSorting: false,
      }),
      columnHelper.accessor("isActive", {
        header: () => t("catalog.active"),
        cell: (info) => (
          <Badge variant={info.getValue() ? "default" : "outline"}>
            {info.getValue() ? t("catalog.active") : t("catalog.inactive")}
          </Badge>
        ),
        enableSorting: false,
      }),
      columnHelper.display({
        id: "actions",
        header: () => tCommon("buttons.edit"),
        cell: ({ row }) => (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => onEdit(row.original)}
          >
            <IconEdit className="h-4 w-4" />
          </Button>
        ),
      }),
    ],
    [t, tCommon, onEdit],
  )

  const table = useReactTable({
    data: drugs,
    columns,
    state: { pagination },
    onPaginationChange: (updater) => {
      const next =
        typeof updater === "function" ? updater(pagination) : updater
      onPaginationChange(next)
    },
    manualPagination: true,
    pageCount,
    getCoreRowModel: getCoreRowModel(),
  })

  return (
    <DataTable
      table={table}
      columns={columns}
      emptyMessage={t("catalog.empty")}
    />
  )
}
