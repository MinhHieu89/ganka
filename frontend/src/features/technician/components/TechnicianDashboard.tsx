import { useState, useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import { toast } from "sonner"
import {
  useTechnicianDashboard,
  useTechnicianKpi,
  useAcceptOrder,
  useCompleteOrder,
  useReturnToQueue,
  useRedFlagOrder,
} from "@/features/technician/api/technician-api"
import type {
  TechnicianStatus,
  TechnicianDashboardFilters,
  TechnicianDashboardRow,
} from "@/features/technician/types/technician.types"
import { TechnicianKpiCards } from "./TechnicianKpiCards"
import { TechnicianBanner } from "./TechnicianBanner"
import { TechnicianToolbar } from "./TechnicianToolbar"
import { TechnicianQueueTable } from "./TechnicianQueueTable"
import { RedFlagDialog } from "./RedFlagDialog"
import { ReturnToQueueDialog } from "./ReturnToQueueDialog"
import { PausePatientDialog } from "./PausePatientDialog"
import { PatientResultsPanel } from "./PatientResultsPanel"

export function TechnicianDashboard() {
  const { t } = useTranslation("technician")
  const navigate = useNavigate()

  // Filter state
  const [statusFilter, setStatusFilter] = useState<TechnicianStatus | undefined>()
  const [search, setSearch] = useState("")
  const [page] = useState(1)
  const [pageSize] = useState(50)

  const filters: TechnicianDashboardFilters = useMemo(
    () => ({
      status: statusFilter,
      search: search || undefined,
      page,
      pageSize,
    }),
    [statusFilter, search, page, pageSize],
  )

  // Data hooks
  const dashboardQuery = useTechnicianDashboard(filters)
  const kpiQuery = useTechnicianKpi()

  // Separate unfiltered query to always find in-progress patient for the banner
  const allQuery = useTechnicianDashboard(
    useMemo(() => ({ status: "in_progress" as TechnicianStatus, page: 1, pageSize: 1 }), []),
  )

  // Mutations
  const acceptOrder = useAcceptOrder()
  const completeOrder = useCompleteOrder()
  const returnToQueue = useReturnToQueue()
  const redFlagOrder = useRedFlagOrder()

  // Derive current in-progress patient from unfiltered query
  const currentPatient = useMemo(
    () => allQuery.data?.find((r) => r.status === "in_progress") ?? null,
    [allQuery.data],
  )

  // Filter counts from KPI data
  const filterCounts = useMemo(
    () => ({
      waiting: kpiQuery.data?.waiting ?? 0,
      inProgress: kpiQuery.data?.inProgress ?? 0,
      completed: kpiQuery.data?.completed ?? 0,
      redFlag: kpiQuery.data?.redFlag ?? 0,
    }),
    [kpiQuery.data],
  )

  // Dialog state
  const [redFlagOpen, setRedFlagOpen] = useState(false)
  const [returnOpen, setReturnOpen] = useState(false)
  const [pauseOpen, setPauseOpen] = useState(false)
  const [selectedRow, setSelectedRow] = useState<TechnicianDashboardRow | null>(null)
  const [pendingAcceptRow, setPendingAcceptRow] = useState<TechnicianDashboardRow | null>(null)
  const [resultsOpen, setResultsOpen] = useState(false)
  const [resultsRow, setResultsRow] = useState<TechnicianDashboardRow | null>(null)

  // Navigate to pre-exam stub page
  const navigateToPreExam = useCallback(() => {
    navigate({ to: "/technician/pre-exam" as string })
  }, [navigate])

  // Action handler
  const handleAction = useCallback(
    (action: string, row: TechnicianDashboardRow) => {
      switch (action) {
        case "accept":
          if (currentPatient) {
            // Already measuring someone - ask to pause
            setPendingAcceptRow(row)
            setPauseOpen(true)
          } else {
            acceptOrder.mutate(row.orderId, {
              onSuccess: () => navigateToPreExam(),
              onError: (err) => {
                const message = err instanceof Error ? err.message : ""
                if (message.includes("already") || message.includes("accepted")) {
                  toast.error(t("error.acceptRace", { name: row.technicianName ?? "" }))
                } else {
                  toast.error(t("error.network"))
                }
              },
            })
          }
          break

        case "continue":
          navigateToPreExam()
          break

        case "complete":
          completeOrder.mutate(row.orderId, {
            onSuccess: () => {
              toast.success(t("toast.completeSuccess", { name: row.patientName }))
            },
            onError: () => toast.error(t("error.network")),
          })
          break

        case "returnToQueue":
          setSelectedRow(row)
          setReturnOpen(true)
          break

        case "redFlag":
          setSelectedRow(row)
          setRedFlagOpen(true)
          break

        case "viewResults":
          setResultsRow(row)
          setResultsOpen(true)
          break
      }
    },
    [currentPatient, acceptOrder, completeOrder, navigateToPreExam, t],
  )

  // Dialog confirmations
  const handleReturnConfirm = useCallback(() => {
    if (!selectedRow) return
    returnToQueue.mutate(selectedRow.orderId, {
      onSuccess: () => {
        toast.success(t("toast.returnSuccess", { name: selectedRow.patientName }))
        setReturnOpen(false)
        setSelectedRow(null)
      },
      onError: () => toast.error(t("error.network")),
    })
  }, [selectedRow, returnToQueue, t])

  const handleRedFlagConfirm = useCallback(
    (reason: string) => {
      if (!selectedRow) return
      redFlagOrder.mutate(
        { orderId: selectedRow.orderId, reason },
        {
          onSuccess: () => {
            toast.success(
              t("toast.redFlagSuccess", { name: selectedRow.patientName }),
            )
            setRedFlagOpen(false)
            setSelectedRow(null)
          },
          onError: () => toast.error(t("error.network")),
        },
      )
    },
    [selectedRow, redFlagOrder, t],
  )

  const handlePauseConfirm = useCallback(() => {
    if (!pendingAcceptRow) return
    // Accept the new patient (backend handles pausing current)
    acceptOrder.mutate(pendingAcceptRow.orderId, {
      onSuccess: () => {
        setPauseOpen(false)
        setPendingAcceptRow(null)
        navigateToPreExam()
      },
      onError: (err) => {
        const message = err instanceof Error ? err.message : ""
        if (message.includes("already") || message.includes("accepted")) {
          toast.error(
            t("error.acceptRace", { name: pendingAcceptRow.technicianName ?? "" }),
          )
        } else {
          toast.error(t("error.network"))
        }
        setPauseOpen(false)
        setPendingAcceptRow(null)
      },
    })
  }, [pendingAcceptRow, acceptOrder, navigateToPreExam, t])

  return (
    <div className="space-y-6">
      {/* Page title */}
      <h1 className="text-[22px] font-bold">{t("pageTitle")}</h1>

      {/* KPI Cards */}
      <TechnicianKpiCards kpi={kpiQuery.data} isLoading={kpiQuery.isLoading} />

      {/* In-progress banner */}
      <TechnicianBanner
        currentPatient={currentPatient}
        onContinue={navigateToPreExam}
      />

      {/* Toolbar: filters + search */}
      <TechnicianToolbar
        activeFilter={statusFilter}
        onFilterChange={setStatusFilter}
        search={search}
        onSearchChange={setSearch}
        counts={filterCounts}
      />

      {/* Queue table */}
      <TechnicianQueueTable
        rows={dashboardQuery.data ?? []}
        isLoading={dashboardQuery.isLoading}
        onAction={handleAction}
      />

      {/* Dialogs */}
      {selectedRow && (
        <>
          <RedFlagDialog
            open={redFlagOpen}
            onOpenChange={setRedFlagOpen}
            patientName={selectedRow.patientName}
            onConfirm={handleRedFlagConfirm}
            isLoading={redFlagOrder.isPending}
          />
          <ReturnToQueueDialog
            open={returnOpen}
            onOpenChange={setReturnOpen}
            patientName={selectedRow.patientName}
            onConfirm={handleReturnConfirm}
            isLoading={returnToQueue.isPending}
          />
        </>
      )}
      {currentPatient && (
        <PausePatientDialog
          open={pauseOpen}
          onOpenChange={setPauseOpen}
          currentPatientName={currentPatient.patientName}
          onConfirm={handlePauseConfirm}
          isLoading={acceptOrder.isPending}
        />
      )}

      {/* Patient results slide-over panel */}
      <PatientResultsPanel
        open={resultsOpen}
        onOpenChange={setResultsOpen}
        row={resultsRow}
      />
    </div>
  )
}
