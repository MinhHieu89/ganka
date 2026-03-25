import { useState, useMemo, useCallback } from "react"
import { useTranslation } from "react-i18next"
import {
  DndContext,
  DragOverlay,
  PointerSensor,
  TouchSensor,
  useSensor,
  useSensors,
  closestCorners,
  type DragStartEvent,
  type DragEndEvent,
} from "@dnd-kit/core"
import { Skeleton } from "@/shared/components/Skeleton"
import { Card, CardContent } from "@/shared/components/Card"
import {
  useActiveVisits,
  useAdvanceStage,
  useCreateVisit,
  isReversalAllowed,
  type ActiveVisitDto,
  type CreateVisitCommand,
} from "@/features/clinical/api/clinical-api"
import { KanbanColumn } from "@/features/clinical/components/KanbanColumn"
import { PatientCard } from "@/features/clinical/components/PatientCard"
import { NewVisitDialog } from "@/features/clinical/components/NewVisitDialog"
import { WorkflowToolbar } from "@/features/clinical/components/WorkflowToolbar"
import { WorkflowTableView } from "@/features/clinical/components/WorkflowTableView"
import { StageReversalDialog } from "@/features/clinical/components/StageReversalDialog"
import { type ViewMode, getStoredViewMode, storeViewMode } from "@/features/clinical/components/ViewToggle"

// -- Kanban column definitions --
// Each workflow stage gets its own column (11 stages + Done)
// NO CashierGlasses — single combined payment at Cashier

interface ColumnDef {
  id: string
  titleKey: string
  stages: number[]
  colorAccent: string
  alwaysVisible: boolean
}

const KANBAN_COLUMNS: ColumnDef[] = [
  { id: "reception", titleKey: "workflow.stages.reception", stages: [0], colorAccent: "stone", alwaysVisible: true },
  { id: "refraction-va", titleKey: "workflow.stages.refractionVa", stages: [1], colorAccent: "blue", alwaysVisible: true },
  { id: "doctor-exam", titleKey: "workflow.stages.doctorExam", stages: [2], colorAccent: "emerald", alwaysVisible: true },
  { id: "imaging", titleKey: "workflow.stages.imaging", stages: [3], colorAccent: "cyan", alwaysVisible: true },
  { id: "doctor-reviews", titleKey: "workflow.stages.doctorReviewsResults", stages: [4], colorAccent: "teal", alwaysVisible: true },
  { id: "prescription", titleKey: "workflow.stages.prescription", stages: [5], colorAccent: "amber", alwaysVisible: true },
  { id: "cashier", titleKey: "workflow.stages.cashier", stages: [6], colorAccent: "orange", alwaysVisible: true },
  { id: "pharmacy", titleKey: "workflow.stages.pharmacy", stages: [7], colorAccent: "violet", alwaysVisible: false },
  { id: "optical-center", titleKey: "workflow.stages.opticalCenter", stages: [8], colorAccent: "pink", alwaysVisible: false },
  { id: "optical-lab", titleKey: "workflow.stages.opticalLab", stages: [9], colorAccent: "indigo", alwaysVisible: false },
  { id: "return-glasses", titleKey: "workflow.stages.returnGlasses", stages: [10], colorAccent: "emerald", alwaysVisible: false },
  { id: "done", titleKey: "workflow.done", stages: [], colorAccent: "muted", alwaysVisible: true },
]

/** Map a stage number to its column ID */
function getColumnForStage(stage: number, isCompleted?: boolean): string {
  if (isCompleted || stage === 99) return "done"
  for (const col of KANBAN_COLUMNS) {
    if (col.stages.includes(stage)) return col.id
  }
  return "reception"
}

/** Find the column definition by column ID */
function getColumnDef(columnId: string): ColumnDef | undefined {
  return KANBAN_COLUMNS.find((c) => c.id === columnId)
}

export function WorkflowDashboard() {
  const { t } = useTranslation("clinical")
  const { data: activeVisits, isLoading } = useActiveVisits()
  const advanceStageMutation = useAdvanceStage()
  const createVisitMutation = useCreateVisit()

  const [activeCard, setActiveCard] = useState<ActiveVisitDto | null>(null)
  const [newVisitOpen, setNewVisitOpen] = useState(false)
  const [viewMode, setViewMode] = useState<ViewMode>(() => getStoredViewMode())
  const [reversalInfo, setReversalInfo] = useState<{
    visitId: string
    currentStage: number
    targetStage: number
  } | null>(null)

  // Build stageLabels map for reversal dialog display
  const stageLabels = useMemo(() => {
    const labels: Record<number, string> = {}
    for (const col of KANBAN_COLUMNS) {
      if (col.stages.length > 0) {
        labels[col.stages[0]] = t(col.titleKey)
      }
    }
    return labels
  }, [t])

  const handleViewModeChange = useCallback((mode: ViewMode) => {
    setViewMode(mode)
    storeViewMode(mode)
  }, [])

  // Configure drag sensors
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 8 },
    }),
    useSensor(TouchSensor, {
      activationConstraint: { delay: 200, tolerance: 5 },
    }),
  )

  // Group visits by column
  const columnVisits = useMemo(() => {
    const groups: Record<string, ActiveVisitDto[]> = {}
    for (const col of KANBAN_COLUMNS) {
      groups[col.id] = []
    }
    if (!activeVisits) return groups
    for (const visit of activeVisits) {
      const colId = getColumnForStage(visit.currentStage, visit.isCompleted)
      if (groups[colId]) groups[colId].push(visit)
    }
    return groups
  }, [activeVisits])

  // Conditional column visibility: hide optional columns unless patients need them
  const visibleColumns = useMemo(() => {
    return KANBAN_COLUMNS.filter((col) => {
      if (col.alwaysVisible) return true
      // Pharmacy column: visible if any visit has drugTrackStatus !== 0 (NotApplicable)
      if (col.id === "pharmacy") {
        return activeVisits?.some((v) => v.drugTrackStatus !== 0) ?? false
      }
      // Optical columns: visible if any visit has glassesTrackStatus !== 0 (NotApplicable)
      if (["optical-center", "optical-lab", "return-glasses"].includes(col.id)) {
        return activeVisits?.some((v) => v.glassesTrackStatus !== 0) ?? false
      }
      // Fallback: show if any visits are in this column
      return (columnVisits[col.id]?.length ?? 0) > 0
    })
  }, [activeVisits, columnVisits])

  const totalPatients = activeVisits?.length ?? 0

  // Find which column a visit is in
  const findVisitColumn = useCallback(
    (visitId: string): string | null => {
      for (const [colId, visits] of Object.entries(columnVisits)) {
        if (visits.some((v) => v.id === visitId)) return colId
      }
      return null
    },
    [columnVisits],
  )

  const handleDragStart = useCallback(
    (event: DragStartEvent) => {
      const draggedId = event.active.id as string
      const visit = activeVisits?.find((v) => v.id === draggedId) ?? null
      setActiveCard(visit)
    },
    [activeVisits],
  )

  const handleDragEnd = useCallback(
    (event: DragEndEvent) => {
      setActiveCard(null)
      const { active, over } = event
      if (!over) return

      const visitId = active.id as string
      const overId = over.id as string

      // Determine target column
      let targetColumnId: string | null = null

      // Check if dropped on a column directly
      if (visibleColumns.some((c) => c.id === overId)) {
        targetColumnId = overId
      } else {
        // Dropped on a card -- find which column that card is in
        targetColumnId = findVisitColumn(overId)
      }

      if (!targetColumnId) return

      // Find source column
      const sourceColumnId = findVisitColumn(visitId)
      if (sourceColumnId === targetColumnId) return

      // Compute new stage: first stage of target column
      const targetCol = getColumnDef(targetColumnId)
      if (!targetCol || targetCol.stages.length === 0) return
      const newStage = targetCol.stages[0]

      const visit = activeVisits?.find((v) => v.id === visitId)
      if (!visit) return

      if (newStage < visit.currentStage) {
        // Backward drag: check if allowed, then open reversal dialog
        if (isReversalAllowed(visit.currentStage, newStage)) {
          setReversalInfo({
            visitId,
            currentStage: visit.currentStage,
            targetStage: newStage,
          })
        }
        // If not allowed, do nothing (card snaps back)
      } else {
        // Forward: advance directly
        advanceStageMutation.mutate({ visitId, newStage })
      }
    },
    [activeVisits, findVisitColumn, advanceStageMutation, visibleColumns],
  )

  const handleAdvance = useCallback(
    (visitId: string, nextStage: number) => {
      advanceStageMutation.mutate({ visitId, newStage: nextStage })
    },
    [advanceStageMutation],
  )

  const handleCreateVisit = useCallback(
    (command: CreateVisitCommand) => {
      createVisitMutation.mutate(command, {
        onSuccess: () => {
          setNewVisitOpen(false)
        },
      })
    },
    [createVisitMutation],
  )

  // Loading skeleton
  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <Skeleton className="h-6 w-6" />
            <Skeleton className="h-8 w-48" />
          </div>
        </div>
        <div className="overflow-x-auto pb-4">
          <div className="flex gap-4" style={{ minWidth: "1800px" }}>
            {KANBAN_COLUMNS.filter((col) => col.alwaysVisible).map((col) => (
              <Card key={col.id} className="min-w-[200px] w-[200px] shrink-0 border-t-2">
                <CardContent className="p-3 space-y-2">
                  <Skeleton className="h-5 w-24" />
                  <Skeleton className="h-20 w-full" />
                  <Skeleton className="h-20 w-full" />
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {/* Toolbar with title, patient count, view toggle, and new visit button */}
      <WorkflowToolbar
        totalPatients={totalPatients}
        viewMode={viewMode}
        onViewModeChange={handleViewModeChange}
        onNewVisit={() => setNewVisitOpen(true)}
      />

      {/* Conditional rendering: kanban or table view */}
      {viewMode === "kanban" ? (
        <DndContext
          sensors={sensors}
          collisionDetection={closestCorners}
          onDragStart={handleDragStart}
          onDragEnd={handleDragEnd}
        >
          <div className="overflow-x-auto pb-4">
            <div className="flex gap-4" style={{ minWidth: `${visibleColumns.length * 216}px` }}>
              {visibleColumns.map((col) => (
                <KanbanColumn
                  key={col.id}
                  id={col.id}
                  title={t(col.titleKey)}
                  visits={columnVisits[col.id] ?? []}
                  colorAccent={col.colorAccent}
                  onAdvance={handleAdvance}
                  isDone={col.id === "done"}
                />
              ))}
            </div>
          </div>
          <DragOverlay>
            {activeCard ? (
              <PatientCard visit={activeCard} isDragOverlay />
            ) : null}
          </DragOverlay>
        </DndContext>
      ) : (
        <WorkflowTableView
          visits={activeVisits}
          isLoading={isLoading}
          onAdvanceStage={(visitId, newStage) => advanceStageMutation.mutate({ visitId, newStage })}
          onReverseStage={(visitId, currentStage, targetStage) =>
            setReversalInfo({ visitId, currentStage, targetStage })
          }
        />
      )}

      {/* New Visit dialog */}
      <NewVisitDialog
        open={newVisitOpen}
        onOpenChange={setNewVisitOpen}
        onCreateVisit={handleCreateVisit}
        isCreating={createVisitMutation.isPending}
      />

      {/* Stage reversal dialog */}
      <StageReversalDialog
        reversalInfo={reversalInfo}
        onClose={() => setReversalInfo(null)}
        stageLabels={stageLabels}
      />
    </div>
  )
}
