import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  useReactTable,
  getCoreRowModel,
  getExpandedRowModel,
  flexRender,
  type ColumnDef,
  type ExpandedState,
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
import { Button } from "@/shared/components/ui/button"
import { Skeleton } from "@/shared/components/ui/skeleton"
import {
  IconChevronDown,
  IconChevronRight,
  IconChevronLeft,
} from "@tabler/icons-react"
import { AuditLogDetail } from "./AuditLogDetail"
import type { AuditLogDto } from "@/features/audit/api/audit-api"

const ACTION_BADGE_STYLES: Record<string, string> = {
  Created: "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400 border-green-200 dark:border-green-800",
  Updated: "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400 border-blue-200 dark:border-blue-800",
  Deleted: "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400 border-red-200 dark:border-red-800",
  Login: "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-400 border-emerald-200 dark:border-emerald-800",
  LoginFailed: "bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-400 border-orange-200 dark:border-orange-800",
  Logout: "bg-stone-100 text-stone-800 dark:bg-stone-900/30 dark:text-stone-400 border-stone-200 dark:border-stone-800",
  ViewRecord: "bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400 border-purple-200 dark:border-purple-800",
}

interface AuditLogTableProps {
  data: AuditLogDto[]
  isLoading: boolean
  hasNextPage: boolean
  hasPreviousPage: boolean
  currentPage: number
  onNextPage: () => void
  onPreviousPage: () => void
}

export function AuditLogTable({
  data,
  isLoading,
  hasNextPage,
  hasPreviousPage,
  currentPage,
  onNextPage,
  onPreviousPage,
}: AuditLogTableProps) {
  const { t } = useTranslation("audit")
  const [expanded, setExpanded] = useState<ExpandedState>({})

  const formatTimestamp = (ts: string) => {
    return new Date(ts).toLocaleString(undefined, {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
    })
  }

  const columns = useMemo<ColumnDef<AuditLogDto>[]>(
    () => [
      {
        id: "expander",
        header: () => null,
        cell: ({ row }) => (
          <Button
            variant="ghost"
            size="icon"
            className="h-6 w-6"
            onClick={() => row.toggleExpanded()}
            aria-label={t("details")}
          >
            {row.getIsExpanded() ? (
              <IconChevronDown className="h-4 w-4" />
            ) : (
              <IconChevronRight className="h-4 w-4" />
            )}
          </Button>
        ),
        size: 40,
      },
      {
        accessorKey: "timestamp",
        header: () => t("timestamp"),
        cell: ({ getValue }) => (
          <span className="whitespace-nowrap text-xs">
            {formatTimestamp(getValue<string>())}
          </span>
        ),
      },
      {
        accessorKey: "userEmail",
        header: () => t("actor"),
        cell: ({ getValue }) => (
          <span className="text-sm">{getValue<string>()}</span>
        ),
      },
      {
        accessorKey: "action",
        header: () => t("action"),
        cell: ({ getValue }) => {
          const action = getValue<string>()
          return (
            <Badge
              variant="outline"
              className={ACTION_BADGE_STYLES[action] ?? ""}
            >
              {t(`actions.${action.toLowerCase()}`, action)}
            </Badge>
          )
        },
      },
      {
        accessorKey: "entityName",
        header: () => t("entity"),
        cell: ({ getValue }) => (
          <span className="font-mono text-xs">{getValue<string>()}</span>
        ),
      },
      {
        accessorKey: "entityId",
        header: () => "ID",
        cell: ({ getValue }) => (
          <span className="font-mono text-xs text-muted-foreground truncate max-w-[120px] block">
            {getValue<string>()}
          </span>
        ),
      },
    ],
    [t]
  )

  const table = useReactTable({
    data,
    columns,
    state: { expanded },
    onExpandedChange: setExpanded,
    getCoreRowModel: getCoreRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
    getRowCanExpand: () => true,
    manualPagination: true,
  })

  if (isLoading && data.length === 0) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div className="border">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id} style={{ width: header.getSize() !== 150 ? header.getSize() : undefined }}>
                    {header.isPlaceholder
                      ? null
                      : flexRender(
                          header.column.columnDef.header,
                          header.getContext()
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
                  {t("noData", "No audit logs found")}
                </TableCell>
              </TableRow>
            ) : (
              table.getRowModel().rows.map((row) => (
                <>
                  <TableRow
                    key={row.id}
                    data-state={row.getIsExpanded() ? "selected" : undefined}
                    className="cursor-pointer"
                    onClick={() => row.toggleExpanded()}
                  >
                    {row.getVisibleCells().map((cell) => (
                      <TableCell key={cell.id}>
                        {flexRender(
                          cell.column.columnDef.cell,
                          cell.getContext()
                        )}
                      </TableCell>
                    ))}
                  </TableRow>
                  {row.getIsExpanded() && (
                    <TableRow key={`${row.id}-detail`}>
                      <TableCell colSpan={columns.length} className="p-0">
                        <AuditLogDetail log={row.original} />
                      </TableCell>
                    </TableRow>
                  )}
                </>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination controls */}
      <div className="flex items-center justify-between">
        <span className="text-sm text-muted-foreground">
          {t("page", "Page")} {currentPage}
        </span>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={onPreviousPage}
            disabled={!hasPreviousPage}
          >
            <IconChevronLeft className="h-4 w-4" />
            {t("previous", "Previous")}
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={onNextPage}
            disabled={!hasNextPage}
          >
            {t("next", "Next")}
            <IconChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </div>
  )
}
