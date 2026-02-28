import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  flexRender,
  getCoreRowModel,
  getSortedRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { useState } from "react"
import { IconArrowsSort, IconEdit } from "@tabler/icons-react"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import type { UserDto } from "@/features/admin/api/admin-api"

interface UserTableProps {
  users: UserDto[]
  onEdit: (user: UserDto) => void
}

const columnHelper = createColumnHelper<UserDto>()

export function UserTable({ users, onEdit }: UserTableProps) {
  const { t } = useTranslation("auth")
  const [sorting, setSorting] = useState<SortingState>([])

  const columns = useMemo(
    () => [
      columnHelper.accessor("email", {
        header: () => t("admin.email"),
        cell: (info) => (
          <span className="font-medium">{info.getValue()}</span>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("fullName", {
        header: () => t("admin.fullName"),
        cell: (info) => info.getValue(),
        enableSorting: true,
      }),
      columnHelper.accessor("roles", {
        header: () => t("admin.roles"),
        cell: (info) => (
          <div className="flex flex-wrap gap-1">
            {info.getValue().map((role) => (
              <Badge key={role} variant="secondary" className="text-xs">
                {role}
              </Badge>
            ))}
          </div>
        ),
        enableSorting: false,
      }),
      columnHelper.accessor("isActive", {
        header: () => t("admin.status"),
        cell: (info) => (
          <Badge variant={info.getValue() ? "default" : "outline"}>
            {info.getValue() ? t("admin.active") : t("admin.inactive")}
          </Badge>
        ),
        enableSorting: false,
      }),
      columnHelper.display({
        id: "actions",
        header: () => t("admin.actions"),
        cell: ({ row }) => (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => onEdit(row.original)}
          >
            <IconEdit className="h-4 w-4 mr-1" />
            {t("admin.editUser")}
          </Button>
        ),
      }),
    ],
    [t, onEdit],
  )

  const table = useReactTable({
    data: users,
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
  })

  return (
    <div className="border">
      <Table>
        <TableHeader>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <TableHead
                  key={header.id}
                  className={
                    header.column.getCanSort()
                      ? "cursor-pointer select-none"
                      : ""
                  }
                  onClick={header.column.getToggleSortingHandler()}
                >
                  <div className="flex items-center gap-1">
                    {header.isPlaceholder
                      ? null
                      : flexRender(
                          header.column.columnDef.header,
                          header.getContext(),
                        )}
                    {header.column.getCanSort() && (
                      <IconArrowsSort className="h-3 w-3 text-muted-foreground" />
                    )}
                  </div>
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
                No users found
              </TableCell>
            </TableRow>
          ) : (
            table.getRowModel().rows.map((row) => (
              <TableRow key={row.id}>
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
