import { useState, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { Link } from "@tanstack/react-router"
import { IconCalendar, IconUserPlus, IconRefresh, IconAlertTriangle } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Tooltip, TooltipContent, TooltipTrigger } from "@/shared/components/Tooltip"
import { useReceptionistDashboard, useReceptionistKpi } from "@/features/receptionist/api/receptionist-api"
import { useManualRefresh } from "@/features/receptionist/hooks/useReceptionistPolling"
import { KpiCards } from "./KpiCards"
import { StatusFilterPills } from "./StatusFilterPills"
import { PatientQueueTable } from "./PatientQueueTable"
import { CheckInDialog } from "./CheckInDialog"
import { CheckInIncompleteDialog } from "./CheckInIncompleteDialog"
import { WalkInVisitDialog } from "./WalkInVisitDialog"
import type { DashboardFilters, ReceptionistDashboardRow, ReceptionistStatus } from "@/features/receptionist/types/receptionist.types"

export function ReceptionistDashboard() {
  const { t } = useTranslation("receptionist")
  const [filters, setFilters] = useState<DashboardFilters>({
    page: 1,
    pageSize: 10,
  })
  const [search, setSearch] = useState("")
  const [checkInRow, setCheckInRow] = useState<ReceptionistDashboardRow | null>(null)
  const [checkInIncompleteRow, setCheckInIncompleteRow] = useState<ReceptionistDashboardRow | null>(null)
  const [walkInOpen, setWalkInOpen] = useState(false)

  const dashboardQuery = useReceptionistDashboard({
    ...filters,
    search: search || undefined,
  })
  const kpiQuery = useReceptionistKpi()
  const { refresh, isRefreshing } = useManualRefresh()

  const hasError = dashboardQuery.isError || kpiQuery.isError

  const handleStatusFilter = useCallback(
    (status?: ReceptionistStatus) => {
      setFilters((prev) => ({ ...prev, status, page: 1 }))
    },
    [],
  )

  const handleCheckIn = useCallback(
    (row: ReceptionistDashboardRow) => {
      // Determine if patient is complete or incomplete/guest
      const isIncomplete = row.isGuestBooking || (!row.birthYear && !row.patientCode)
      if (isIncomplete) {
        setCheckInIncompleteRow(row)
      } else {
        setCheckInRow(row)
      }
    },
    [],
  )

  const handleActionMenu = useCallback(
    (_row: ReceptionistDashboardRow) => {
      // Action menu is now handled directly by RowActionMenu component in the table
    },
    [],
  )

  return (
    <div className="space-y-6">
      {/* Title */}
      <h1 className="text-xl font-semibold">Dashboard</h1>

      {/* Connection lost banner */}
      {hasError && (
        <div className="flex items-center gap-2 rounded-md border border-amber-300 bg-amber-50 px-4 py-2 text-sm text-amber-800">
          <IconAlertTriangle className="h-4 w-4 shrink-0" />
          <span>{t("dashboard.connectionLost")}</span>
        </div>
      )}

      {/* Action bar */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-2">
          <Input
            type="search"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={t("dashboard.searchPlaceholder")}
            className="w-64"
          />
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                onClick={refresh}
                disabled={isRefreshing}
              >
                <IconRefresh
                  className={`h-4 w-4 ${isRefreshing ? "animate-spin" : ""}`}
                />
              </Button>
            </TooltipTrigger>
            <TooltipContent>{t("dashboard.refreshData")}</TooltipContent>
          </Tooltip>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" asChild>
            <Link to={"/appointments/new" as string}>
              <IconCalendar className="mr-2 h-4 w-4" />
              {t("dashboard.bookAppointment")}
            </Link>
          </Button>
          <Button asChild>
            <Link to={"/patients/intake" as string}>
              <IconUserPlus className="mr-2 h-4 w-4" />
              {t("dashboard.newPatient")}
            </Link>
          </Button>
        </div>
      </div>

      {/* KPI Cards */}
      <KpiCards kpi={kpiQuery.data} isLoading={kpiQuery.isLoading} />

      {/* Status Filter Pills */}
      <StatusFilterPills
        value={filters.status}
        onChange={handleStatusFilter}
        counts={kpiQuery.data}
      />

      {/* Patient Queue Table */}
      <PatientQueueTable
        data={dashboardQuery.data}
        isLoading={dashboardQuery.isLoading}
        onCheckIn={handleCheckIn}
        onActionMenu={handleActionMenu}
        filters={filters}
        onFiltersChange={setFilters}
      />

      {/* Check-in dialogs */}
      {checkInRow && (
        <CheckInDialog
          open={!!checkInRow}
          onOpenChange={(open) => !open && setCheckInRow(null)}
          row={checkInRow}
        />
      )}
      {checkInIncompleteRow && (
        <CheckInIncompleteDialog
          open={!!checkInIncompleteRow}
          onOpenChange={(open) => !open && setCheckInIncompleteRow(null)}
          row={checkInIncompleteRow}
        />
      )}

      {/* Walk-in dialog */}
      <WalkInVisitDialog
        open={walkInOpen}
        onOpenChange={setWalkInOpen}
      />
    </div>
  )
}
