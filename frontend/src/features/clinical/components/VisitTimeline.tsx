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
      <div className="relative pl-7 space-y-3">
        {/* Skeleton timeline line */}
        <div className="absolute left-3.5 top-0 bottom-0 w-0.5 bg-border" />
        {[1, 2, 3].map((i) => (
          <div key={i} className="relative">
            {/* Skeleton dot */}
            <div className="absolute -left-4.75 top-1/2 -translate-y-1/2 w-3 h-3 rounded-full bg-muted border-2 border-background" />
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
      <div className="relative pl-7 pr-2 py-1" role="listbox" aria-label="Visit history">
        {/* Vertical timeline connector line — centered at left 14.5px (left-[14px] + half of w-0.5) */}
        <div className="absolute left-[14px] top-0 bottom-0 w-0.5 bg-border" />

        <div className="space-y-2">
          {visits.map((visit) => {
            const isSelected = visit.visitId === selectedVisitId
            return (
              <div key={visit.visitId} className="relative transition-all duration-200">
                {/* Timeline dot — centered on line and vertically centered on card */}
                <div
                  className={cn(
                    "absolute left-[-19px] top-1/2 -translate-y-1/2 w-3 h-3 rounded-full border-2 border-background z-10",
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
