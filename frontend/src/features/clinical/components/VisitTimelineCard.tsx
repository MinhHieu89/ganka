import { useTranslation } from "react-i18next"
import { IconStethoscope, IconClipboardHeart } from "@tabler/icons-react"
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

function formatVisitDate(dateStr: string): string {
  const date = new Date(dateStr)
  return date.toLocaleDateString(undefined, {
    day: "2-digit",
    month: "short",
    year: "numeric",
  })
}

export function VisitTimelineCard({ visit, isSelected, onClick }: VisitTimelineCardProps) {
  const { t } = useTranslation("clinical")

  return (
    <Card
      className={cn(
        "cursor-pointer p-3 space-y-1.5 transition-all duration-200",
        "hover:bg-muted/50 hover:shadow-sm",
        isSelected && "ring-2 ring-primary bg-primary/5",
      )}
      onClick={onClick}
      role="option"
      aria-selected={isSelected}
    >
      {/* Top row: date + status badge */}
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium">
          {formatVisitDate(visit.visitDate)}
        </span>
        <Badge variant={STATUS_VARIANT[visit.status] ?? "outline"} className="text-[10px] px-1.5 py-0">
          {t(`visit.status.${STATUS_KEY[visit.status] ?? "draft"}`)}
        </Badge>
      </div>

      {/* Middle row: doctor name with icon */}
      <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
        <IconStethoscope className="h-3.5 w-3.5 flex-shrink-0" />
        <span className="truncate">{visit.doctorName}</span>
      </div>

      {/* Bottom row: diagnosis */}
      {(visit.primaryDiagnosisText || visit.diagnosisCount > 0) && (
        <div className="flex items-center gap-1.5 text-xs text-foreground/70">
          <IconClipboardHeart className="h-3.5 w-3.5 flex-shrink-0" />
          <span className="truncate">
            {visit.primaryDiagnosisText
              ? visit.primaryDiagnosisText
              : `${visit.diagnosisCount} ${visit.diagnosisCount === 1 ? t("visit.diagnosis") : t("visit.diagnoses")}`}
          </span>
        </div>
      )}
    </Card>
  )
}
