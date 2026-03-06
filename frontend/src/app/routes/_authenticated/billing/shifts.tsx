import { useState, useMemo } from "react"
import { createFileRoute } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import {
  type ColumnDef,
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  type SortingState,
  getExpandedRowModel,
} from "@tanstack/react-table"
import { format } from "date-fns"
import {
  IconClock,
  IconCash,
  IconReceipt2,
} from "@tabler/icons-react"
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { DataTable } from "@/shared/components/DataTable"
import { formatVND } from "@/shared/lib/format-vnd"
import {
  useCurrentShift,
  useShiftHistory,
  type CashierShiftDto,
} from "@/features/billing/api/shift-api"
import { ShiftOpenDialog } from "@/features/billing/components/ShiftOpenDialog"
import { ShiftCloseDialog } from "@/features/billing/components/ShiftCloseDialog"
import { ShiftReportView } from "@/features/billing/components/ShiftReportView"

export const Route = createFileRoute("/_authenticated/billing/shifts")({
  component: ShiftsPage,
})

function ShiftsPage() {
  const { t } = useTranslation("billing")
  const { data: currentShift, isLoading: isLoadingCurrent } = useCurrentShift()
  const { data: historyData, isLoading: isLoadingHistory } = useShiftHistory()
  const [sorting, setSorting] = useState<SortingState>([])

  const columns = useMemo<ColumnDef<CashierShiftDto>[]>(
    () => [
      {
        accessorKey: "openedAt",
        header: t("date"),
        cell: ({ getValue }) =>
          format(new Date(getValue<string>()), "dd/MM/yyyy"),
        enableSorting: true,
      },
      {
        accessorKey: "cashierName",
        header: t("cashierName"),
      },
      {
        accessorKey: "openedAt",
        id: "openTime",
        header: t("openedAt"),
        cell: ({ getValue }) =>
          format(new Date(getValue<string>()), "HH:mm"),
      },
      {
        accessorKey: "closedAt",
        header: t("closedAt"),
        cell: ({ getValue }) => {
          const val = getValue<string | null>()
          return val ? format(new Date(val), "HH:mm") : "--"
        },
      },
      {
        accessorKey: "totalRevenue",
        header: t("totalRevenue"),
        cell: ({ getValue }) => formatVND(getValue<number>()),
        enableSorting: true,
      },
      {
        accessorKey: "status",
        header: t("status.draft", { defaultValue: "Status" }),
        cell: ({ getValue }) => {
          const status = getValue<number>()
          return (
            <Badge variant={status === 0 ? "default" : "secondary"}>
              {status === 0 ? t("shiftStatus.open") : t("shiftStatus.closed")}
            </Badge>
          )
        },
      },
    ],
    [t],
  )

  const shifts = historyData?.items ?? []

  const table = useReactTable({
    data: shifts,
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
    getRowCanExpand: () => true,
  })

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div>
        <h1 className="text-2xl font-bold">{t("shifts")}</h1>
        <p className="text-sm text-muted-foreground mt-0.5">
          {t("shiftsSubtitle")}
        </p>
      </div>

      {/* Current shift status */}
      {isLoadingCurrent ? (
        <Skeleton className="h-40 w-full" />
      ) : currentShift ? (
        <CurrentShiftCard shift={currentShift} />
      ) : (
        <NoShiftCard />
      )}

      {/* Shift history table */}
      <div className="space-y-3">
        <h2 className="text-lg font-semibold">{t("shiftHistory")}</h2>
        {isLoadingHistory ? (
          <div className="space-y-3">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        ) : (
          <DataTable
            table={table}
            columns={columns}
            emptyMessage={t("noShiftHistory")}
            onRowClick={(_row, tanstackRow) => tanstackRow.toggleExpanded()}
            renderSubRow={(row) => (
              <div className="p-6 bg-muted/30">
                <ShiftReportView shiftId={row.id} />
              </div>
            )}
          />
        )}
      </div>
    </div>
  )
}

// -- Sub-components --

function CurrentShiftCard({ shift }: { shift: CashierShiftDto }) {
  const { t } = useTranslation("billing")

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <IconClock className="h-5 w-5" />
            {t("currentShift")}
          </CardTitle>
          <Badge variant="default">{t("shiftStatus.open")}</Badge>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
          <StatItem
            label={t("cashierName")}
            value={shift.cashierName}
          />
          <StatItem
            label={t("openedAt")}
            value={format(new Date(shift.openedAt), "HH:mm dd/MM/yyyy")}
          />
          <StatItem
            icon={<IconCash className="h-4 w-4 text-green-600" />}
            label={t("totalRevenue")}
            value={formatVND(shift.totalRevenue)}
          />
          <StatItem
            icon={<IconReceipt2 className="h-4 w-4 text-blue-600" />}
            label={t("transactionCount")}
            value={String(shift.transactionCount)}
          />
        </div>
        <div className="flex justify-end">
          <ShiftCloseDialog
            shiftId={shift.id}
            expectedCash={shift.expectedCashAmount}
          />
        </div>
      </CardContent>
    </Card>
  )
}

function NoShiftCard() {
  const { t } = useTranslation("billing")

  return (
    <Card>
      <CardContent className="py-8 flex flex-col items-center gap-4">
        <IconClock className="h-10 w-10 text-muted-foreground" />
        <p className="text-muted-foreground">{t("noOpenShift")}</p>
        <ShiftOpenDialog>
          <Button>{t("openShift")}</Button>
        </ShiftOpenDialog>
      </CardContent>
    </Card>
  )
}

function StatItem({
  icon,
  label,
  value,
}: {
  icon?: React.ReactNode
  label: string
  value: string
}) {
  return (
    <div>
      <p className="text-xs text-muted-foreground flex items-center gap-1">
        {icon}
        {label}
      </p>
      <p className="font-medium mt-0.5">{value}</p>
    </div>
  )
}
