import { useMemo, useEffect, useState, useCallback } from "react"
import { useTranslation } from "react-i18next"
import {
  useReactTable,
  getCoreRowModel,
  createColumnHelper,
  flexRender,
} from "@tanstack/react-table"
import { IconClipboardList } from "@tabler/icons-react"
import { Skeleton } from "@/shared/components/Skeleton"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import { TechnicianStatusBadge } from "./TechnicianStatusBadge"
import { TechnicianActionMenu } from "./TechnicianActionMenu"
import type { TechnicianDashboardRow } from "@/features/technician/types/technician.types"

const columnHelper = createColumnHelper<TechnicianDashboardRow>()

const visitTypeKeys: Record<string, string> = {
  new: "visitType.new",
  follow_up: "visitType.followUp",
  additional: "visitType.additional",
}

interface TechnicianQueueTableProps {
  rows: TechnicianDashboardRow[]
  isLoading: boolean
  onAction: (action: string, row: TechnicianDashboardRow) => void
}

export function TechnicianQueueTable({
  rows,
  isLoading,
  onAction,
}: TechnicianQueueTableProps) {
  const { t } = useTranslation("technician")

  // Client-side wait time increment every 60s (D-17)
  const [tickCount, setTickCount] = useState(0)
  useEffect(() => {
    const interval = setInterval(() => {
      setTickCount((c) => c + 1)
    }, 60_000)
    return () => clearInterval(interval)
  }, [])

  const getWaitMinutes = useCallback(
    (baseMinutes: number) => baseMinutes + tickCount,
    [tickCount],
  )

  const columns = useMemo(
    () => [
      columnHelper.display({
        id: "index",
        header: t("table.index"),
        size: 32,
        cell: (info) => (
          <span className="text-muted-foreground tabular-nums text-sm">
            {info.row.index + 1}
          </span>
        ),
      }),
      columnHelper.accessor("patientName", {
        header: t("table.name"),
        size: 200,
        cell: (info) => {
          const row = info.row.original
          return (
            <div>
              <div
                className="text-sm font-semibold"
                style={
                  row.isRedFlag
                    ? { color: "var(--tech-action-destructive)" }
                    : undefined
                }
              >
                {info.getValue()}
              </div>
              {row.patientCode && (
                <div className="font-mono text-xs text-muted-foreground">
                  {row.patientCode}
                </div>
              )}
            </div>
          )
        },
      }),
      columnHelper.accessor("birthYear", {
        header: t("table.birthYear"),
        size: 46,
        cell: (info) => (
          <span className="text-sm tabular-nums">
            {info.getValue() ?? "--"}
          </span>
        ),
      }),
      columnHelper.accessor("checkinTime", {
        header: t("table.checkin"),
        size: 60,
        cell: (info) => {
          const val = info.getValue()
          if (!val) return "--"
          try {
            return (
              <span className="text-sm tabular-nums">
                {new Date(val).toLocaleTimeString("vi-VN", {
                  hour: "2-digit",
                  minute: "2-digit",
                })}
              </span>
            )
          } catch {
            return "--"
          }
        },
      }),
      columnHelper.accessor("waitMinutes", {
        header: t("table.wait"),
        size: 54,
        cell: (info) => {
          const minutes = getWaitMinutes(info.getValue())
          const isUrgent = minutes >= 25
          return (
            <span
              className="text-sm tabular-nums font-medium"
              style={{
                color: isUrgent
                  ? "var(--tech-wait-urgent)"
                  : "var(--tech-wait-default)",
              }}
            >
              {minutes}p
            </span>
          )
        },
      }),
      columnHelper.accessor("reason", {
        header: t("table.reason"),
        cell: (info) => {
          const row = info.row.original
          const isRedFlag = row.isRedFlag
          const text = isRedFlag && row.redFlagReason ? row.redFlagReason : info.getValue()
          return (
            <span
              className="text-sm truncate max-w-[200px] block"
              style={
                isRedFlag
                  ? { color: "var(--tech-action-destructive)" }
                  : undefined
              }
            >
              {text || "--"}
            </span>
          )
        },
      }),
      columnHelper.accessor("visitType", {
        header: t("table.type"),
        size: 54,
        cell: (info) => {
          const key = visitTypeKeys[info.getValue()] ?? "visitType.new"
          return (
            <span className="text-xs border rounded-full px-2 py-0.5 whitespace-nowrap">
              {t(key)}
            </span>
          )
        },
      }),
      columnHelper.accessor("status", {
        header: t("table.status"),
        size: 82,
        cell: (info) => <TechnicianStatusBadge status={info.getValue()} />,
      }),
      columnHelper.display({
        id: "actions",
        header: "",
        size: 40,
        cell: (info) => (
          <TechnicianActionMenu
            row={info.row.original}
            onAction={onAction}
          />
        ),
      }),
    ],
    [t, onAction, getWaitMinutes],
  )

  const table = useReactTable({
    data: rows,
    columns,
    getCoreRowModel: getCoreRowModel(),
  })

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    )
  }

  if (rows.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <div className="flex size-14 items-center justify-center bg-muted rounded-full mb-4">
          <IconClipboardList className="h-7 w-7 text-muted-foreground" />
        </div>
        <h3 className="text-base font-semibold mb-1">{t("empty.title")}</h3>
        <p className="text-sm text-muted-foreground max-w-sm">
          {t("empty.message")}
        </p>
      </div>
    )
  }

  return (
    <div className="border rounded-lg overflow-hidden">
      <Table>
        <TableHeader>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <TableHead key={header.id} style={{ width: header.getSize() }}>
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
          {table.getRowModel().rows.map((row) => (
            <TableRow
              key={row.id}
              className={
                row.original.status === "completed" ? "opacity-55" : ""
              }
            >
              {row.getVisibleCells().map((cell) => (
                <TableCell key={cell.id}>
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
