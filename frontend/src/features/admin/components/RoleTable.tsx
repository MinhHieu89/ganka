import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  getCoreRowModel,
  useReactTable,
} from "@tanstack/react-table"
import { Badge } from "@/shared/components/Badge"
import { DataTable } from "@/shared/components/DataTable"
import type { RoleDto } from "@/features/admin/api/admin-api"

interface RoleTableProps {
  roles: RoleDto[]
  selectedRoleId: string | null
  onSelectRole: (role: RoleDto) => void
}

const columnHelper = createColumnHelper<RoleDto>()

export function RoleTable({
  roles,
  selectedRoleId,
  onSelectRole,
}: RoleTableProps) {
  const { t } = useTranslation("auth")

  const columns = useMemo(
    () => [
      columnHelper.accessor("name", {
        header: () => t("admin.name"),
        cell: (info) => (
          <span className="font-medium">{info.getValue()}</span>
        ),
      }),
      columnHelper.accessor("description", {
        header: () => t("admin.description"),
        cell: (info) => (
          <span className="text-muted-foreground">{info.getValue()}</span>
        ),
      }),
      columnHelper.accessor("isSystem", {
        header: () => "",
        cell: (info) =>
          info.getValue() ? (
            <Badge variant="secondary" className="text-xs">
              {t("admin.systemRole")}
            </Badge>
          ) : (
            <Badge variant="outline" className="text-xs">
              {t("admin.customRole")}
            </Badge>
          ),
      }),
      columnHelper.accessor("permissions", {
        header: () => t("admin.permissionCount"),
        cell: (info) => (
          <Badge variant="outline">{info.getValue().length}</Badge>
        ),
      }),
    ],
    [t],
  )

  const table = useReactTable({
    data: roles,
    columns,
    getCoreRowModel: getCoreRowModel(),
  })

  return (
    <DataTable
      table={table}
      columns={columns}
      onRowClick={(row) => onSelectRole(row)}
      rowClassName={(row) =>
        row.id === selectedRoleId ? "bg-accent" : "hover:bg-muted/50"
      }
      emptyMessage="No roles found"
    />
  )
}
