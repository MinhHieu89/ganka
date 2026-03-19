import { useState, useMemo, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
  type SortingState,
  type ColumnFiltersState,
} from "@tanstack/react-table"
import { IconPlus } from "@tabler/icons-react"
import { differenceInDays, format } from "date-fns"
import { Link } from "@tanstack/react-router"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { DataTable } from "@/shared/components/DataTable"
import { Skeleton } from "@/shared/components/Skeleton"
import { formatVND } from "@/shared/lib/format-vnd"
import { useAuthStore } from "@/shared/stores/authStore"
import { useActiveTreatments } from "@/features/treatment/api/treatment-api"
import type { TreatmentPackageDto } from "@/features/treatment/api/treatment-types"
import { DueSoonSection } from "./DueSoonSection"
import { TreatmentPackageForm } from "./TreatmentPackageForm"

// -- Status badge mapping --

const STATUS_STYLES: Record<string, string> = {
  Active: "border-green-500 text-green-700 dark:text-green-400",
  Paused: "border-yellow-500 text-yellow-700 dark:text-yellow-400",
  PendingCancellation: "border-orange-500 text-orange-700 dark:text-orange-400",
  Completed: "border-blue-500 text-blue-700 dark:text-blue-400",
  Cancelled: "border-red-500 text-red-700 dark:text-red-400",
  Switched: "border-gray-500 text-gray-700 dark:text-gray-400",
}

const TREATMENT_TYPE_STYLES: Record<string, string> = {
  IPL: "border-violet-500 text-violet-700 dark:text-violet-400",
  LLLT: "border-blue-500 text-blue-700 dark:text-blue-400",
  LidCare: "border-emerald-500 text-emerald-700 dark:text-emerald-400",
}

// -- Column helper --

const columnHelper = createColumnHelper<TreatmentPackageDto>()

export function TreatmentsPage() {
  const { t } = useTranslation("treatment")
  const { data: packages = [], isLoading, isError } = useActiveTreatments()
  const navigate = useNavigate()
  const [createDialogOpen, setCreateDialogOpen] = useState(false)
  const canCreate = useAuthStore(
    (s) => s.user?.permissions?.includes("Treatment.Create") || s.user?.permissions?.includes("Admin"),
  )
  const [sorting, setSorting] = useState<SortingState>([])
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([])

  // Filter state
  const [statusFilter, setStatusFilter] = useState<string>("all")
  const [typeFilter, setTypeFilter] = useState<string>("all")

  // Filter options (translated)
  const statusFilterOptions = useMemo(() => [
    { value: "all", label: t("list.allStatuses") },
    { value: "Active", label: t("status.Active") },
    { value: "Paused", label: t("status.Paused") },
    { value: "PendingCancellation", label: t("status.PendingCancellation") },
    { value: "Completed", label: t("status.Completed") },
    { value: "Cancelled", label: t("status.Cancelled") },
    { value: "Switched", label: t("status.Switched") },
  ], [t])

  const typeFilterOptions = useMemo(() => [
    { value: "all", label: t("list.allTypes") },
    { value: "IPL", label: t("treatmentType.IPL") },
    { value: "LLLT", label: t("treatmentType.LLLT") },
    { value: "LidCare", label: t("treatmentType.LidCare") },
  ], [t])

  // Apply filters
  const filteredData = useMemo(() => {
    let result = packages
    if (statusFilter !== "all") {
      result = result.filter((p) => p.status === statusFilter)
    }
    if (typeFilter !== "all") {
      result = result.filter((p) => p.treatmentType === typeFilter)
    }
    return result
  }, [packages, statusFilter, typeFilter])

  const handleRowClick = useCallback(
    (row: TreatmentPackageDto) => {
      void navigate({ to: "/treatments/$packageId", params: { packageId: row.id } })
    },
    [navigate],
  )

  const columns = useMemo(
    () => [
      columnHelper.accessor("patientName", {
        header: t("list.patient"),
        cell: (info) => (
          <Link
            to="/patients/$patientId"
            params={{ patientId: info.row.original.patientId }}
            className="font-medium text-sm hover:underline text-primary"
            onClick={(e) => e.stopPropagation()}
          >
            {info.getValue()}
          </Link>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("treatmentType", {
        header: t("list.type"),
        cell: (info) => {
          const type = info.getValue()
          return (
            <Badge
              variant="outline"
              className={`text-xs ${TREATMENT_TYPE_STYLES[type] ?? ""}`}
            >
              {t(`treatmentType.${type}`)}
            </Badge>
          )
        },
        enableSorting: false,
      }),
      columnHelper.accessor("status", {
        header: t("list.status"),
        cell: (info) => {
          const status = info.getValue()
          const className = STATUS_STYLES[status] ?? ""
          return (
            <Badge
              variant="outline"
              className={`text-xs ${className}`}
            >
              {t(`status.${status}`)}
            </Badge>
          )
        },
        enableSorting: false,
      }),
      columnHelper.display({
        id: "progress",
        header: t("list.progress"),
        cell: ({ row }) => {
          const pkg = row.original
          const percent =
            pkg.totalSessions > 0
              ? Math.round((pkg.sessionsCompleted / pkg.totalSessions) * 100)
              : 0
          return (
            <div className="flex items-center gap-2">
              <div className="w-20 h-2 bg-muted rounded-full overflow-hidden">
                <div
                  className="h-full bg-primary rounded-full transition-all"
                  style={{ width: `${percent}%` }}
                />
              </div>
              <span className="text-xs text-muted-foreground whitespace-nowrap">
                {pkg.sessionsCompleted}/{pkg.totalSessions}
              </span>
            </div>
          )
        },
      }),
      columnHelper.accessor("packagePrice", {
        header: t("list.pricing"),
        cell: (info) => {
          const pkg = info.row.original
          const displayPrice =
            pkg.pricingMode === "PerPackage"
              ? pkg.packagePrice
              : pkg.sessionPrice
          return (
            <span className="text-sm whitespace-nowrap">
              {formatVND(displayPrice)}
            </span>
          )
        },
        enableSorting: true,
      }),
      columnHelper.accessor("lastSessionDate", {
        header: t("list.lastSession"),
        cell: (info) => {
          const dateStr = info.getValue()
          if (!dateStr) {
            return (
              <span className="text-xs text-muted-foreground">--</span>
            )
          }
          const days = differenceInDays(new Date(), new Date(dateStr))
          return (
            <span className="text-xs text-muted-foreground">
              {days === 0
                ? t("list.today")
                : days === 1
                  ? t("list.oneDayAgo")
                  : t("list.daysAgo", { days })}
            </span>
          )
        },
        enableSorting: true,
      }),
      columnHelper.accessor("nextDueDate", {
        header: t("list.nextDue"),
        cell: (info) => {
          const dateStr = info.getValue()
          if (!dateStr) {
            return (
              <span className="text-xs text-muted-foreground">--</span>
            )
          }
          const dueDate = new Date(dateStr)
          const now = new Date()
          const isOverdue = dueDate < now

          if (isOverdue) {
            return (
              <Badge variant="destructive" className="text-xs">
                {t("list.overdue")}
              </Badge>
            )
          }
          return (
            <span className="text-xs text-muted-foreground">
              {format(dueDate, "dd/MM/yyyy")}
            </span>
          )
        },
        enableSorting: true,
      }),
      columnHelper.display({
        id: "actions",
        header: "",
        cell: ({ row }) => (
          <Link
            to="/treatments/$packageId"
            params={{ packageId: row.original.id }}
            className="text-sm text-primary hover:underline"
            onClick={(e) => e.stopPropagation()}
          >
            {t("list.view")}
          </Link>
        ),
      }),
    ],
    [t],
  )

  const table = useReactTable({
    data: filteredData,
    columns,
    state: { sorting, columnFilters },
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: {
      pagination: { pageSize: 20 },
    },
  })

  if (isError) {
    return (
      <div className="text-center py-12 text-destructive">
        {t("list.loadError")}
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("title")}</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {t("list.pageDescription")}
          </p>
        </div>
        {canCreate && (
          <Button onClick={() => setCreateDialogOpen(true)}>
            <IconPlus className="h-4 w-4 mr-2" />
            {t("createPackage")}
          </Button>
        )}
      </div>

      {/* Due Soon section */}
      <DueSoonSection />

      {/* Filters */}
      <div className="flex items-center gap-3">
        <Select value={typeFilter} onValueChange={setTypeFilter}>
          <SelectTrigger className="w-[160px]">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {typeFilterOptions.map((opt) => (
              <SelectItem key={opt.value} value={opt.value}>
                {opt.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-[180px]">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {statusFilterOptions.map((opt) => (
              <SelectItem key={opt.value} value={opt.value}>
                {opt.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* DataTable */}
      {isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      ) : (
        <DataTable
          table={table}
          columns={columns}
          onRowClick={handleRowClick}
          emptyMessage={t("list.emptyMessage")}
        />
      )}

      {/* Create package dialog */}
      <TreatmentPackageForm
        open={createDialogOpen}
        onOpenChange={setCreateDialogOpen}
      />
    </div>
  )
}
