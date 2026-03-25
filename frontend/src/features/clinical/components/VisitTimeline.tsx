import { useTranslation } from "react-i18next"
import { IconClipboardOff } from "@tabler/icons-react"
import { ScrollArea } from "@/shared/components/ScrollArea"
import { Skeleton } from "@/shared/components/Skeleton"
import { cn } from "@/shared/lib/utils"
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
      <div className="relative pl-8 space-y-3">
        {/* Skeleton timeline line */}
        <div className="absolute left-3 top-2 bottom-2 w-0.5 bg-border" />
        {[1, 2, 3].map((i) => (
          <div key={i} className="relative">
            {/* Skeleton dot */}
            <div className="absolute left-[-20px] top-4 w-3 h-3 rounded-full bg-muted border-2 border-background" />
            <Skeleton className="h-20 w-full" />
          </div>
        ))}
      </div>
    )
  }

  if (!visits || visits.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-full py-12 text-muted-foreground">
        <IconClipboardOff className="h-10 w-10 mb-3 opacity-40" />
        <p className="font-semibold text-sm">{t("patient.visitHistory.empty.title")}</p>
        <p className="text-xs mt-1">{t("patient.visitHistory.empty.description")}</p>
      </div>
    )
  }

  return (
    <ScrollArea className="h-full">
      <div className="relative pl-8 pr-2" role="listbox" aria-label="Visit history">
        {/* Vertical timeline connector line */}
        <div className="absolute left-3 top-2 bottom-2 w-0.5 bg-border" />

        <div className="space-y-2">
          {visits.map((visit) => {
            const isSelected = visit.visitId === selectedVisitId
            return (
              <div key={visit.visitId} className="relative transition-all duration-200">
                {/* Timeline dot */}
                <div
                  className={cn(
                    "absolute left-[-20px] top-4 w-3 h-3 rounded-full border-2 border-background z-10",
                    isSelected ? "bg-primary" : "bg-muted-foreground/40",
                  )}
                />
                <VisitTimelineCard
                  visit={visit}
                  isSelected={isSelected}
                  onClick={() => onSelectVisit(visit.visitId)}
                />
              </div>
            )
          })}
        </div>
      </div>
    </ScrollArea>
  )
}
