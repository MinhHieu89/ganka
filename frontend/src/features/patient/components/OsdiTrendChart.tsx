import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  ReferenceArea,
} from "recharts"
import { format } from "date-fns"
import { vi } from "date-fns/locale"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { useOsdiHistory } from "@/features/clinical/api/clinical-api"

const SEVERITY_BANDS = [
  { y1: 0, y2: 12, fill: "#dcfce7", label: "Normal" },
  { y1: 12, y2: 22, fill: "#fef9c3", label: "Mild" },
  { y1: 22, y2: 32, fill: "#fed7aa", label: "Moderate" },
  { y1: 32, y2: 100, fill: "#fecaca", label: "Severe" },
] as const

const SEVERITY_LABELS: Record<number, string> = {
  0: "Normal",
  1: "Mild",
  2: "Moderate",
  3: "Severe",
}

interface OsdiTrendChartProps {
  patientId: string
}

export function OsdiTrendChart({ patientId }: OsdiTrendChartProps) {
  const { t, i18n } = useTranslation("clinical")
  const { data: response, isLoading } = useOsdiHistory(patientId)
  const isVietnamese = i18n.language === "vi"

  const chartData = useMemo(() => {
    if (!response?.items) return []
    return response.items.map((item) => ({
      visitDate: format(
        new Date(item.visitDate),
        "dd/MM/yyyy",
        isVietnamese ? { locale: vi } : undefined,
      ),
      score: item.osdiScore,
      severity: item.severity,
    }))
  }, [response, isVietnamese])

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-base">{t("osdi.trendChart")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-[300px] flex items-center justify-center text-muted-foreground">
            ...
          </div>
        </CardContent>
      </Card>
    )
  }

  if (!chartData.length) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-base">{t("osdi.trendChart")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-[300px] flex items-center justify-center text-muted-foreground">
            {t("osdi.noOsdiData")}
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{t("osdi.trendChart")}</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={chartData} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
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
              dataKey="visitDate"
              tick={{ fontSize: 12 }}
              interval="preserveStartEnd"
            />
            <YAxis
              domain={[0, 100]}
              tick={{ fontSize: 12 }}
              width={40}
            />
            <Tooltip
              content={({ active, payload }) => {
                if (!active || !payload?.length) return null
                const data = payload[0].payload as (typeof chartData)[number]
                const severityLabel = SEVERITY_LABELS[data.severity] ?? "Unknown"
                return (
                  <div className="rounded-lg border bg-background p-2 shadow-md">
                    <p className="text-xs text-muted-foreground">{data.visitDate}</p>
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
