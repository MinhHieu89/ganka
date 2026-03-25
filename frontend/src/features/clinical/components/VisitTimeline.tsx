import { useTranslation } from "react-i18next"
import { ScrollArea } from "@/shared/components/ScrollArea"
import { Skeleton } from "@/shared/components/Skeleton"
import { VisitTimelineCard } from "./VisitTimelineCard"
import type { PatientVisitHistoryDto } from "../api/clinical-api"

interface VisitTimelineProps {
  visits: PatientVisitHistoryDto[] | undefined
  isLoading: boolean
  selectedVisitId: string | null
  onSelectVisit: (visitId: string) => void
}

export function VisitTimeline({
  visits,
  isLoading,
  selectedVisitId,
  onSelectVisit,
}: VisitTimelineProps) {
  const { t } = useTranslation("clinical")

  if (isLoading) {
    return (
      <div className="space-y-2">
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} className="h-20 w-full" />
        ))}
      </div>
    )
  }

  if (!visits || visits.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        <p className="font-semibold">{t("patient.visitHistory.empty.title")}</p>
        <p className="text-xs mt-1">{t("patient.visitHistory.empty.description")}</p>
      </div>
    )
  }

  return (
    <ScrollArea className="h-full">
      <div className="space-y-2 pr-2" role="listbox" aria-label="Visit history">
        {visits.map((visit) => (
          <VisitTimelineCard
            key={visit.visitId}
            visit={visit}
            isSelected={visit.visitId === selectedVisitId}
            onClick={() => onSelectVisit(visit.visitId)}
          />
        ))}
      </div>
    </ScrollArea>
  )
}
