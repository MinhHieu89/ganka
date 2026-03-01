import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import {
  createColumnHelper,
  getCoreRowModel,
  useReactTable,
  type PaginationState,
} from "@tanstack/react-table"
import { IconEye, IconAlertTriangle } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { DataTable } from "@/shared/components/DataTable"
import type { PatientDto } from "@/features/patient/api/patient-api"

interface PatientTableProps {
  data: PatientDto[]
  totalCount: number
  pagination: PaginationState
  onPaginationChange: (pagination: PaginationState) => void
}

const columnHelper = createColumnHelper<PatientDto>()

export function PatientTable({
  data,
  totalCount,
  pagination,
  onPaginationChange,
}: PatientTableProps) {
  const { t } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const navigate = useNavigate()

  const columns = useMemo(
    () => [
      columnHelper.accessor("patientCode", {
        header: () => t("patientCode"),
        cell: (info) => (
          <span className="font-mono text-sm">
            {info.getValue() ?? "---"}
          </span>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("fullName", {
        header: () => t("fullName"),
        cell: (info) => (
          <span className="font-medium">{info.getValue()}</span>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("phone", {
        header: () => t("phone"),
        cell: (info) => info.getValue(),
        enableSorting: false,
      }),
      columnHelper.accessor("patientType", {
        header: () => t("patientType"),
        cell: (info) => (
          <Badge
            variant={info.getValue() === "Medical" ? "default" : "secondary"}
          >
            {info.getValue() === "Medical"
              ? t("medicalPatient")
              : t("walkInCustomer")}
          </Badge>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("gender", {
        header: () => t("gender"),
        cell: (info) => {
          const val = info.getValue()
          if (!val) return "---"
          const genderMap: Record<string, string> = {
            Male: t("male"),
            Female: t("female"),
            Other: t("other"),
          }
          return genderMap[val] ?? val
        },
        enableSorting: false,
      }),
      columnHelper.accessor("allergies", {
        header: () => t("allergies"),
        cell: (info) => {
          const allergies = info.getValue()
          if (!allergies || allergies.length === 0) return "---"
          return (
            <Badge variant="outline" className="gap-1">
              <IconAlertTriangle className="h-3 w-3 text-orange-500" />
              {allergies.length}
            </Badge>
          )
        },
        enableSorting: false,
      }),
      columnHelper.accessor("isActive", {
        header: () => t("status"),
        cell: (info) => (
          <Badge variant={info.getValue() ? "default" : "outline"}>
            {info.getValue() ? t("active") : t("inactive")}
          </Badge>
        ),
        enableSorting: false,
      }),
      columnHelper.display({
        id: "actions",
        cell: ({ row }) => (
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation()
              navigate({
                to: "/patients/$patientId" as string,
                params: { patientId: row.original.id } as never,
              })
            }}
          >
            <IconEye className="h-4 w-4" />
          </Button>
        ),
      }),
    ],
    [t, tCommon, navigate],
  )

  const pageCount = Math.ceil(totalCount / pagination.pageSize)

  const table = useReactTable({
    data,
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
    <div className="space-y-4">
      <DataTable
        table={table}
        columns={columns}
        emptyMessage={tCommon("table.noData")}
        onRowClick={(row) =>
          navigate({
            to: "/patients/$patientId" as string,
            params: { patientId: row.id } as never,
          })
        }
      />

      {/* Pagination */}
      {pageCount > 1 && (
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>
            {tCommon("table.page")} {pagination.pageIndex + 1} {tCommon("table.of")}{" "}
            {pageCount} ({totalCount} {tCommon("table.total").toLowerCase()})
          </span>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={!table.getCanPreviousPage()}
              onClick={() => table.previousPage()}
            >
              {tCommon("buttons.previous")}
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={!table.getCanNextPage()}
              onClick={() => table.nextPage()}
            >
              {tCommon("buttons.next")}
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
