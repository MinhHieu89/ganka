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
  type ActiveVisitDto,
  type CreateVisitCommand,
} from "@/features/clinical/api/clinical-api"
import { KanbanColumn } from "@/features/clinical/components/KanbanColumn"
import { PatientCard } from "@/features/clinical/components/PatientCard"
import { NewVisitDialog } from "@/features/clinical/components/NewVisitDialog"
import { WorkflowToolbar } from "@/features/clinical/components/WorkflowToolbar"
import { WorkflowTableView } from "@/features/clinical/components/WorkflowTableView"
import { type ViewMode, getStoredViewMode, storeViewMode } from "@/features/clinical/components/ViewToggle"

// -- Kanban column definitions --
// Each workflow stage gets its own column (8 stages + Done)

interface ColumnDef {
  id: string
  titleKey: string
  stages: number[]
  colorAccent: string
}

const KANBAN_COLUMNS: ColumnDef[] = [
  { id: "reception", titleKey: "workflow.stages.reception", stages: [0], colorAccent: "stone" },
  { id: "refraction-va", titleKey: "workflow.stages.refractionVa", stages: [1], colorAccent: "blue" },
  { id: "doctor-exam", titleKey: "workflow.stages.doctorExam", stages: [2], colorAccent: "emerald" },
  { id: "diagnostics", titleKey: "workflow.stages.diagnostics", stages: [3], colorAccent: "cyan" },
  { id: "doctor-reads", titleKey: "workflow.stages.doctorReads", stages: [4], colorAccent: "teal" },
  { id: "rx", titleKey: "workflow.stages.rx", stages: [5], colorAccent: "amber" },
  { id: "cashier", titleKey: "workflow.stages.cashier", stages: [6], colorAccent: "orange" },
  { id: "pharmacy-optical", titleKey: "workflow.stages.pharmacyOptical", stages: [7], colorAccent: "violet" },
  { id: "done", titleKey: "workflow.done", stages: [], colorAccent: "muted" },
]

/** Map a stage number to its column ID */
function getColumnForStage(stage: number): string {
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
      if (visit.isCompleted) {
        groups["done"].push(visit)
      } else {
        const colId = getColumnForStage(visit.currentStage)
        if (groups[colId]) groups[colId].push(visit)
      }
    }
    return groups
  }, [activeVisits])

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
      if (KANBAN_COLUMNS.some((c) => c.id === overId)) {
        targetColumnId = overId
      } else {
        // Dropped on a card -- find which column that card is in
        targetColumnId = findVisitColumn(overId)
      }

      if (!targetColumnId) return

      // Don't allow dropping into the done column
      if (targetColumnId === "done") return

      // Find source column
      const sourceColumnId = findVisitColumn(visitId)
      if (sourceColumnId === targetColumnId) return

      // Compute new stage: first stage of target column
      const targetCol = getColumnDef(targetColumnId)
      if (!targetCol || targetCol.stages.length === 0) return
      const newStage = targetCol.stages[0]

      advanceStageMutation.mutate({ visitId, newStage })
    },
    [findVisitColumn, advanceStageMutation],
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
            {KANBAN_COLUMNS.map((col) => (
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
            <div className="flex gap-4" style={{ minWidth: "1800px" }}>
              {KANBAN_COLUMNS.map((col) => (
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
        />
      )}

      {/* New Visit dialog */}
      <NewVisitDialog
        open={newVisitOpen}
        onOpenChange={setNewVisitOpen}
        onCreateVisit={handleCreateVisit}
        isCreating={createVisitMutation.isPending}
      />
    </div>
  )
}
