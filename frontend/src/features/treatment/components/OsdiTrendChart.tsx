import type { TreatmentSessionDto } from "../api/treatment-types"

interface OsdiTrendChartProps {
  sessions: TreatmentSessionDto[]
}

// Stub -- fully implemented in Task 2
export function OsdiTrendChart({ sessions }: OsdiTrendChartProps) {
  return <div data-sessions={sessions.length} />
}
