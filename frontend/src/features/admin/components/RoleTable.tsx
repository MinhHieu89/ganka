import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  flexRender,
  getCoreRowModel,
  useReactTable,
} from "@tanstack/react-table"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table"
import { Badge } from "@/shared/components/ui/badge"
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
    <div className="border">
      <Table>
        <TableHeader>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <TableHead key={header.id}>
                  {header.isPlaceholder
                    ? null
                    : flexRender(
                        header.column.columnDef.header,
                        header.getContext(),
                      )}
                </TableHead>
              ))}
            </TableRow>
          ))}
        </TableHeader>
        <TableBody>
          {table.getRowModel().rows.length === 0 ? (
            <TableRow>
              <TableCell
                colSpan={columns.length}
                className="h-24 text-center text-muted-foreground"
              >
                No roles found
              </TableCell>
            </TableRow>
          ) : (
            table.getRowModel().rows.map((row) => (
              <TableRow
                key={row.id}
                className={`cursor-pointer ${
                  row.original.id === selectedRoleId
                    ? "bg-accent"
                    : "hover:bg-muted/50"
                }`}
                onClick={() => onSelectRole(row.original)}
              >
                {row.getVisibleCells().map((cell) => (
                  <TableCell key={cell.id}>
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </TableCell>
                ))}
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  )
}
