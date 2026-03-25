import { useState, useEffect } from "react"
import { usePatientVisitHistory } from "../api/clinical-api"
import { VisitTimeline } from "./VisitTimeline"
import { VisitHistoryDetail } from "./VisitHistoryDetail"

interface VisitHistoryTabProps {
  patientId: string
}

export function VisitHistoryTab({ patientId }: VisitHistoryTabProps) {
  const { data: visits, isLoading } = usePatientVisitHistory(patientId)
  const [selectedVisitId, setSelectedVisitId] = useState<string | null>(null)

  // Auto-select most recent visit when data loads
  useEffect(() => {
    if (visits && visits.length > 0 && !selectedVisitId) {
      setSelectedVisitId(visits[0].visitId)
    }
  }, [visits, selectedVisitId])

  return (
    <div className="flex gap-4" style={{ height: "calc(100vh - 280px)" }}>
      {/* Left: Timeline (300px fixed) */}
      <div className="w-[300px] min-w-[300px] flex-shrink-0">
        <VisitTimeline
          visits={visits}
          isLoading={isLoading}
          selectedVisitId={selectedVisitId}
          onSelectVisit={setSelectedVisitId}
        />
      </div>
      {/* Right: Detail panel (remaining space) */}
      <div className="flex-1 min-w-0">
        <VisitHistoryDetail visitId={selectedVisitId} />
      </div>
    </div>
  )
}
