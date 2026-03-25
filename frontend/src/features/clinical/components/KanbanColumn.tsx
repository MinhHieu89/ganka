import { useDroppable } from "@dnd-kit/core"
import {
  SortableContext,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable"
import { Card, CardContent, CardHeader } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { PatientCard } from "@/features/clinical/components/PatientCard"
import type { ActiveVisitDto } from "@/features/clinical/api/clinical-api"

/** Color accent classes for the thin top border */
const accentColors: Record<string, string> = {
  stone: "border-t-2 border-t-stone-400",
  blue: "border-t-2 border-t-blue-500",
  emerald: "border-t-2 border-t-emerald-500",
  cyan: "border-t-2 border-t-cyan-500",
  teal: "border-t-2 border-t-teal-500",
  amber: "border-t-2 border-t-amber-500",
  orange: "border-t-2 border-t-orange-500",
  violet: "border-t-2 border-t-violet-500",
  muted: "border-t-2 border-t-muted-foreground/20",
  // Legacy colors for backward compatibility
  gray: "border-t-2 border-t-muted-foreground/40",
  green: "border-t-2 border-t-green-500",
}

interface KanbanColumnProps {
  id: string
  title: string
  visits: ActiveVisitDto[]
  colorAccent: string
  onAdvance: (visitId: string, nextStage: number) => void
  isDone?: boolean
}

export function KanbanColumn({
  id,
  title,
  visits,
  colorAccent,
  onAdvance,
  isDone = false,
}: KanbanColumnProps) {
  const { setNodeRef, isOver } = useDroppable({ id, disabled: isDone })

  return (
    <Card
      className={`flex flex-col min-w-[200px] w-[200px] shrink-0 ${
        accentColors[colorAccent] ?? accentColors.stone
      } ${isOver ? "ring-2 ring-primary/30" : ""}`}
    >
      <CardHeader className="p-3 pb-2">
        <div className="flex items-center justify-between">
          <span className="text-sm font-semibold">{title}</span>
          <Badge variant="secondary" className="text-xs px-1.5 py-0">
            {visits.length}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="flex-1 p-2 pt-0">
        <div
          ref={setNodeRef}
          className="space-y-2 min-h-[60px] max-h-[calc(100vh-220px)] overflow-y-auto"
        >
          <SortableContext
            items={visits.map((v) => v.id)}
            strategy={verticalListSortingStrategy}
          >
            {visits.map((visit) => (
              <PatientCard
                key={visit.id}
                visit={visit}
                onAdvance={isDone ? undefined : onAdvance}
                isDone={isDone}
              />
            ))}
          </SortableContext>
        </div>
      </CardContent>
    </Card>
  )
}
