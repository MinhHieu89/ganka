import { useTranslation } from "react-i18next"
import { Card } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { cn } from "@/shared/lib/utils"
import type { PatientVisitHistoryDto } from "../api/clinical-api"

const STATUS_VARIANT: Record<number, "outline" | "default" | "secondary" | "destructive"> = {
  0: "outline",      // Draft
  1: "default",      // Signed
  2: "secondary",    // Amended
  3: "destructive",  // Cancelled
}

const STATUS_KEY: Record<number, string> = {
  0: "draft",
  1: "signed",
  2: "amended",
  3: "cancelled",
}

interface VisitTimelineCardProps {
  visit: PatientVisitHistoryDto
  isSelected: boolean
  onClick: () => void
}

export function VisitTimelineCard({ visit, isSelected, onClick }: VisitTimelineCardProps) {
  const { t } = useTranslation("clinical")

  return (
    <Card
      className={cn(
        "cursor-pointer p-3 space-y-1 transition-shadow",
        isSelected && "ring-2 ring-primary",
      )}
      onClick={onClick}
      role="option"
      aria-selected={isSelected}
    >
      <div className="flex items-center justify-between">
        <span className="text-sm font-semibold">
          {new Date(visit.visitDate).toLocaleDateString()}
        </span>
        <Badge variant={STATUS_VARIANT[visit.status] ?? "outline"}>
          {t(`visit.status.${STATUS_KEY[visit.status] ?? "draft"}`)}
        </Badge>
      </div>
      <p className="text-xs text-muted-foreground">{visit.doctorName}</p>
      {visit.primaryDiagnosisText && (
        <p className="text-xs truncate">{visit.primaryDiagnosisText}</p>
      )}
    </Card>
  )
}
