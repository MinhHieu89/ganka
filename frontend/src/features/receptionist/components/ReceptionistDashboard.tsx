import { useState, useCallback } from "react"
import { Link } from "@tanstack/react-router"
import { IconCalendar, IconUserPlus, IconRefresh, IconAlertTriangle } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Tooltip, TooltipContent, TooltipTrigger } from "@/shared/components/Tooltip"
import { useReceptionistDashboard, useReceptionistKpi, useCheckInMutation } from "@/features/receptionist/api/receptionist-api"
import { useManualRefresh } from "@/features/receptionist/hooks/useReceptionistPolling"
import { KpiCards } from "./KpiCards"
import { StatusFilterPills } from "./StatusFilterPills"
import { PatientQueueTable } from "./PatientQueueTable"
import type { DashboardFilters, ReceptionistDashboardRow, ReceptionistStatus } from "@/features/receptionist/types/receptionist.types"

export function ReceptionistDashboard() {
  const [filters, setFilters] = useState<DashboardFilters>({
    page: 1,
    pageSize: 10,
  })
  const [search, setSearch] = useState("")

  const dashboardQuery = useReceptionistDashboard({
    ...filters,
    search: search || undefined,
  })
  const kpiQuery = useReceptionistKpi()
  const checkInMutation = useCheckInMutation()
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
      if (row.appointmentId) {
        checkInMutation.mutate(row.appointmentId)
      }
    },
    [checkInMutation],
  )

  const handleActionMenu = useCallback(
    (_row: ReceptionistDashboardRow) => {
      // Action menu will be implemented in plan 14-06
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
          <span>Mat ket noi. Du lieu co the chua cap nhat.</span>
        </div>
      )}

      {/* Action bar */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-2">
          <Input
            type="search"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Tim theo SBT hoac ten BN..."
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
            <TooltipContent>Cap nhat du lieu</TooltipContent>
          </Tooltip>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" asChild>
            <Link to={"/appointments/new" as string}>
              <IconCalendar className="mr-2 h-4 w-4" />
              Dat lich hen
            </Link>
          </Button>
          <Button asChild>
            <Link to={"/receptionist/intake" as string}>
              <IconUserPlus className="mr-2 h-4 w-4" />
              Tiep nhan BN moi
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
    </div>
  )
}
