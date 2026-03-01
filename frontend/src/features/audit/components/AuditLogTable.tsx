import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  useReactTable,
  getCoreRowModel,
  getExpandedRowModel,
  type ColumnDef,
  type ExpandedState,
} from "@tanstack/react-table"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { DataTable } from "@/shared/components/DataTable"
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
            onClick={(e) => {
              e.stopPropagation()
              row.toggleExpanded()
            }}
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
      <DataTable
        table={table}
        columns={columns}
        onRowClick={(_row, tanstackRow) => tanstackRow.toggleExpanded()}
        renderSubRow={(row) => <AuditLogDetail log={row} />}
        emptyMessage={t("noData", "No audit logs found")}
        headerStyle={(_, size) => ({
          width: size !== 150 ? size : undefined,
        })}
      />

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
