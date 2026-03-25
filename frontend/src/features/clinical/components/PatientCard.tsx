import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate } from "@tanstack/react-router"
import { useSortable } from "@dnd-kit/sortable"
import { CSS } from "@dnd-kit/utilities"
import { IconAlertTriangle, IconChevronRight } from "@tabler/icons-react"
import { Card } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/shared/components/Tooltip"
import type { ActiveVisitDto } from "@/features/clinical/api/clinical-api"

/** Map backend WorkflowStage int to i18n key */
const stageI18nKeys: Record<number, string> = {
  0: "workflow.stages.reception",
  1: "workflow.stages.refractionVa",
  2: "workflow.stages.doctorExam",
  3: "workflow.stages.diagnostics",
  4: "workflow.stages.doctorReads",
  5: "workflow.stages.rx",
  6: "workflow.stages.cashier",
  7: "workflow.stages.pharmacyOptical",
  8: "workflow.done",
}

const MAX_STAGE = 8

interface PatientCardProps {
  visit: ActiveVisitDto
  onAdvance?: (visitId: string, nextStage: number) => void
  isDragOverlay?: boolean
  isDone?: boolean
}

export function PatientCard({
  visit,
  onAdvance,
  isDragOverlay,
  isDone = false,
}: PatientCardProps) {
  const { t } = useTranslation("clinical")
  const navigate = useNavigate()

  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: visit.id,
    disabled: isDragOverlay || isDone,
  })

  const style = useMemo(
    () => ({
      transform: CSS.Transform.toString(transform),
      transition,
      opacity: isDragging ? 0.4 : 1,
    }),
    [transform, transition, isDragging],
  )

  const visitTime = useMemo(() => {
    const date = new Date(visit.visitDate)
    return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
  }, [visit.visitDate])

  const waitTimeLabel = useMemo(() => {
    if (visit.waitMinutes < 60) return `${visit.waitMinutes}m`
    const hours = Math.floor(visit.waitMinutes / 60)
    const mins = visit.waitMinutes % 60
    return `${hours}h${mins > 0 ? `${mins}m` : ""}`
  }, [visit.waitMinutes])

  const waitTimeBadgeVariant = useMemo(() => {
    if (visit.waitMinutes >= 60) return "destructive" as const
    if (visit.waitMinutes >= 30) return "secondary" as const
    return "outline" as const
  }, [visit.waitMinutes])

  const stageLabel = t(stageI18nKeys[visit.currentStage] ?? "workflow.stages.reception")
  const canAdvance = visit.currentStage < MAX_STAGE

  const handleCardClick = () => {
    navigate({ to: "/visits/$visitId" as string, params: { visitId: visit.id } } as never)
  }

  const handleAdvance = (e: React.MouseEvent) => {
    e.stopPropagation()
    if (onAdvance && canAdvance) {
      onAdvance(visit.id, visit.currentStage + 1)
    }
  }

  return (
    <Card
      ref={isDragOverlay ? undefined : setNodeRef}
      style={isDragOverlay ? undefined : style}
      className="cursor-grab active:cursor-grabbing min-h-12 p-3 space-y-2 select-none"
      {...(isDragOverlay ? {} : { ...attributes, ...listeners })}
    >
      {/* Card body - clickable to navigate */}
      <div
        className="space-y-1.5 cursor-pointer"
        onClick={handleCardClick}
        role="button"
        tabIndex={0}
        onKeyDown={(e) => {
          if (e.key === "Enter") handleCardClick()
        }}
      >
        {/* Patient name + allergy warning */}
        <div className="flex items-center gap-1.5">
          <Link
            to="/patients/$patientId"
            params={{ patientId: visit.patientId }}
            className="font-semibold text-sm leading-tight truncate flex-1 text-primary hover:underline cursor-pointer"
            onClick={(e: React.MouseEvent) => e.stopPropagation()}
          >
            {visit.patientName}
          </Link>
          {visit.hasAllergies && (
            <Tooltip>
              <TooltipTrigger asChild>
                <IconAlertTriangle className="h-4 w-4 text-amber-500 shrink-0" />
              </TooltipTrigger>
              <TooltipContent>{t("card.allergyWarning")}</TooltipContent>
            </Tooltip>
          )}
        </div>

        {/* Doctor name + time */}
        <div className="flex items-center justify-between text-xs text-muted-foreground">
          <span className="truncate">{visit.doctorName}</span>
          <span className="shrink-0">{visitTime}</span>
        </div>

        {/* Stage badge + wait time */}
        <div className="flex items-center justify-between gap-1.5">
          <Badge variant="secondary" className="text-[10px] px-1.5 py-0">
            {stageLabel}
          </Badge>
          <Badge variant={waitTimeBadgeVariant} className="text-[10px] px-1.5 py-0">
            {waitTimeLabel}
          </Badge>
        </div>
      </div>

      {/* Action button */}
      {canAdvance && (
        <Button
          variant="ghost"
          size="sm"
          className="w-full h-7 text-xs"
          onClick={handleAdvance}
        >
          {t("card.advanceStage")}
          <IconChevronRight className="ml-1 h-3 w-3" />
        </Button>
      )}
    </Card>
  )
}
