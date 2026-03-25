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
  3: "workflow.stages.imaging",
  4: "workflow.stages.doctorReviewsResults",
  5: "workflow.stages.prescription",
  6: "workflow.stages.cashier",
  7: "workflow.stages.pharmacy",
  8: "workflow.stages.opticalCenter",
  9: "workflow.stages.opticalLab",
  10: "workflow.stages.returnGlasses",
  99: "workflow.done",
}

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

  // Forward shortcut: ONLY on stage 0 (Reception)
  const isReception = visit.currentStage === 0
  const stageLabel = t(stageI18nKeys[visit.currentStage] ?? "workflow.stages.reception")

  const handleCardClick = () => {
    navigate({ to: "/visits/$visitId" as string, params: { visitId: visit.id } } as never)
  }

  const handleAdvance = (e: React.MouseEvent) => {
    e.stopPropagation()
    if (onAdvance && isReception) {
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
            className="font-bold text-[14px] leading-tight truncate flex-1 text-primary hover:underline cursor-pointer"
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

        {/* Assigned doctor (muted) + appointment time (right-aligned) */}
        <div className="flex items-center justify-between text-xs text-muted-foreground">
          <span className="truncate">{visit.doctorName}</span>
          <span className="shrink-0">{visitTime}</span>
        </div>

        {/* Elapsed wait timer with amber status dot */}
        <div className="flex items-center gap-1.5">
          <span className="bg-amber-400 rounded-full w-1.5 h-1.5 animate-pulse shrink-0" />
          <span className="text-xs text-muted-foreground">{waitTimeLabel}</span>
        </div>

        {/* Stage pill badges */}
        <div className="flex items-center flex-wrap gap-1">
          {visit.refractionSkipped && (
            <Badge variant="secondary" className="text-[10px] px-1.5 py-0 bg-amber-100 text-amber-700 border-amber-200">
              {"Đã bỏ qua"}
            </Badge>
          )}
          {visit.status === 1 && (
            <Badge variant="secondary" className="text-[10px] px-1.5 py-0 bg-green-100 text-green-700 border-green-200">
              {"Đã ký duyệt"}
            </Badge>
          )}
          {visit.currentStage > 6 && (
            <Badge variant="secondary" className="text-[10px] px-1.5 py-0 bg-green-100 text-green-700 border-green-200">
              {"Đã thanh toán"}
            </Badge>
          )}
        </div>
      </div>

      {/* Forward shortcut: ONLY on Reception (stage 0) */}
      {isReception && !isDone && onAdvance ? (
        <Button
          variant="ghost"
          size="sm"
          className="w-full h-7 text-xs"
          onClick={handleAdvance}
        >
          {"Chuyển tiếp"}
          <IconChevronRight className="ml-1 h-3 w-3" />
        </Button>
      ) : !isDone && !isReception ? (
        <p className="text-[10px] text-muted-foreground text-center">
          {"Nhấn để xem chi tiết \u2192"}
        </p>
      ) : null}
    </Card>
  )
}
