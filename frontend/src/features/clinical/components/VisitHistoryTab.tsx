import { useState, useEffect } from "react"
import { useTranslation } from "react-i18next"
import { IconHistory } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Separator } from "@/shared/components/Separator"
import { usePatientVisitHistory } from "../api/clinical-api"
import { VisitTimeline } from "./VisitTimeline"
import { VisitHistoryDetail } from "./VisitHistoryDetail"

interface VisitHistoryTabProps {
  patientId: string
}

export function VisitHistoryTab({ patientId }: VisitHistoryTabProps) {
  const { t } = useTranslation("clinical")
  const { data: visits, isLoading } = usePatientVisitHistory(patientId)
  const [selectedVisitId, setSelectedVisitId] = useState<string | null>(null)

  // Auto-select most recent visit when data loads
  useEffect(() => {
    if (visits && visits.length > 0 && !selectedVisitId) {
      setSelectedVisitId(visits[0].visitId)
    }
  }, [visits, selectedVisitId])

  const visitCount = visits?.length ?? 0

  return (
    <div className="flex flex-col min-h-[600px]">
      {/* Header */}
      <div className="flex items-center gap-2 mb-4">
        <IconHistory className="h-5 w-5 text-muted-foreground" />
        <h2 className="text-lg font-semibold">{t("patient.visitHistory.title")}</h2>
        {visitCount > 0 && (
          <Badge variant="secondary" className="text-xs">
            {visitCount} {visitCount === 1 ? t("patient.visitHistory.visit") : t("patient.visitHistory.visits")}
          </Badge>
        )}
      </div>

      {/* Content: 2-column layout */}
      <div className="flex flex-1 min-h-0">
        {/* Left: Timeline (300px fixed) */}
        <div className="w-[300px] min-w-[300px] flex-shrink-0">
          <VisitTimeline
            visits={visits}
            isLoading={isLoading}
            selectedVisitId={selectedVisitId}
            onSelectVisit={setSelectedVisitId}
          />
        </div>

        {/* Vertical divider */}
        <Separator orientation="vertical" className="mx-3" />

        {/* Right: Detail panel (remaining space) */}
        <div className="flex-1 min-w-0">
          <VisitHistoryDetail visitId={selectedVisitId} />
        </div>
      </div>
    </div>
  )
}
