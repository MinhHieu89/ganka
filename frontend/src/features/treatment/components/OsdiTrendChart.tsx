import { useMemo } from "react"
import { format } from "date-fns"
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  ReferenceArea,
} from "recharts"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import type { TreatmentSessionDto } from "../api/treatment-types"

// Severity zones matching OSDI scoring thresholds
const SEVERITY_BANDS = [
  { y1: 0, y2: 12, fill: "#dcfce7", label: "Normal" },
  { y1: 12, y2: 22, fill: "#fef9c3", label: "Mild" },
  { y1: 22, y2: 32, fill: "#fed7aa", label: "Moderate" },
  { y1: 32, y2: 100, fill: "#fecaca", label: "Severe" },
] as const

const SEVERITY_LABELS: Record<string, string> = {
  Normal: "Normal",
  Mild: "Mild",
  Moderate: "Moderate",
  Severe: "Severe",
}

function getSeverityLabel(score: number): string {
  if (score <= 12) return "Normal"
  if (score <= 22) return "Mild"
  if (score <= 32) return "Moderate"
  return "Severe"
}

interface ChartDataPoint {
  sessionLabel: string
  sessionDate: string
  score: number
  severity: string
}

interface OsdiTrendChartProps {
  sessions: TreatmentSessionDto[]
}

export function OsdiTrendChart({ sessions }: OsdiTrendChartProps) {
  const chartData = useMemo<ChartDataPoint[]>(() => {
    return sessions
      .filter((s) => s.osdiScore != null)
      .map((s) => {
        const date = s.completedAt ?? s.scheduledAt ?? s.createdAt
        return {
          sessionLabel: `#${s.sessionNumber}`,
          sessionDate: format(new Date(date), "dd/MM/yyyy"),
          score: Number(s.osdiScore),
          severity:
            s.osdiSeverity ?? getSeverityLabel(Number(s.osdiScore)),
        }
      })
  }, [sessions])

  // No data or insufficient data
  if (chartData.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-base">OSDI Score Trend</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-[250px] flex items-center justify-center text-muted-foreground">
            No OSDI scores recorded yet
          </div>
        </CardContent>
      </Card>
    )
  }

  if (chartData.length === 1) {
    const point = chartData[0]
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-base">OSDI Score Trend</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-[250px] flex flex-col items-center justify-center gap-2">
            <span className="text-4xl font-bold tabular-nums">
              {point.score.toFixed(1)}
            </span>
            <span className="text-sm text-muted-foreground">
              {point.severity} - Session {point.sessionLabel} ({point.sessionDate})
            </span>
            <span className="text-xs text-muted-foreground">
              More sessions needed for trend chart
            </span>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">OSDI Score Trend</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart
            data={chartData}
            margin={{ top: 5, right: 20, bottom: 5, left: 0 }}
          >
            {/* Severity background bands */}
            {SEVERITY_BANDS.map((band) => (
              <ReferenceArea
                key={band.label}
                y1={band.y1}
                y2={band.y2}
                fill={band.fill}
                fillOpacity={0.4}
              />
            ))}
            <XAxis
              dataKey="sessionLabel"
              tick={{ fontSize: 12 }}
              interval="preserveStartEnd"
            />
            <YAxis domain={[0, 100]} tick={{ fontSize: 12 }} width={40} />
            <Tooltip
              content={({ active, payload }) => {
                if (!active || !payload?.length) return null
                const data = payload[0].payload as ChartDataPoint
                const severityLabel =
                  SEVERITY_LABELS[data.severity] ?? data.severity
                return (
                  <div className="rounded-lg border bg-background p-2 shadow-md">
                    <p className="text-xs text-muted-foreground">
                      Session {data.sessionLabel} - {data.sessionDate}
                    </p>
                    <p className="text-sm font-bold">
                      OSDI: {data.score.toFixed(1)}
                    </p>
                    <p className="text-xs">{severityLabel}</p>
                  </div>
                )
              }}
            />
            <Line
              type="monotone"
              dataKey="score"
              stroke="hsl(var(--primary))"
              strokeWidth={2}
              dot={{ r: 4, fill: "hsl(var(--primary))" }}
              activeDot={{ r: 6 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}
