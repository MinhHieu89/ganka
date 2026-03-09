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
import {
  IconStethoscope,
  IconPlus,
} from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
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

// -- Kanban column definitions --
// Groups 8 WorkflowStage values into 5 columns

interface ColumnDef {
  id: string
  titleKey: string
  stages: number[]
  colorAccent: string
}

const KANBAN_COLUMNS: ColumnDef[] = [
  { id: "reception", titleKey: "workflow.reception", stages: [0], colorAccent: "gray" },
  { id: "testing", titleKey: "workflow.testing", stages: [1], colorAccent: "blue" },
  { id: "doctor", titleKey: "workflow.doctor", stages: [2, 3, 4], colorAccent: "green" },
  { id: "processing", titleKey: "workflow.processing", stages: [5, 6], colorAccent: "orange" },
  { id: "done", titleKey: "workflow.done", stages: [7], colorAccent: "muted" },
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
    if (activeVisits) {
      for (const visit of activeVisits) {
        const colId = getColumnForStage(visit.currentStage)
        groups[colId]?.push(visit)
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
      // overId could be a column ID (droppable) or a card ID (sortable)
      let targetColumnId: string | null = null

      // Check if dropped on a column directly
      if (KANBAN_COLUMNS.some((c) => c.id === overId)) {
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
      if (!targetCol) return
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
        <div className="flex gap-4 overflow-x-auto p-1">
          {KANBAN_COLUMNS.map((col) => (
            <Card key={col.id} className="min-w-[220px] w-[220px] shrink-0 border-t-2">
              <CardContent className="p-3 space-y-2">
                <Skeleton className="h-5 w-24" />
                <Skeleton className="h-20 w-full" />
                <Skeleton className="h-20 w-full" />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {/* Page header */}
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <IconStethoscope className="h-6 w-6 text-primary" />
          <h1 className="text-2xl font-semibold tracking-tight">
            {t("workflow.title")}
          </h1>
          <Badge variant="secondary">{totalPatients}</Badge>
        </div>
        <Button onClick={() => setNewVisitOpen(true)}>
          <IconPlus className="mr-2 h-4 w-4" />
          {t("newVisit.title")}
        </Button>
      </div>

      {/* Kanban board -- always render columns regardless of patient count */}
      <DndContext
        sensors={sensors}
        collisionDetection={closestCorners}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
      >
        <div className="flex gap-4 overflow-x-auto p-1">
          {KANBAN_COLUMNS.map((col) => (
            <KanbanColumn
              key={col.id}
              id={col.id}
              title={t(col.titleKey)}
              visits={columnVisits[col.id] ?? []}
              colorAccent={col.colorAccent}
              onAdvance={handleAdvance}
            />
          ))}
        </div>
        <DragOverlay>
          {activeCard ? (
            <PatientCard visit={activeCard} isDragOverlay />
          ) : null}
        </DragOverlay>
      </DndContext>

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
