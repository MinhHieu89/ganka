import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts"
import { format } from "date-fns"
import { vi } from "date-fns/locale"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Button } from "@/shared/components/Button"
import { useDryEyeMetricHistory } from "@/features/clinical/api/clinical-api"
import type { MetricTimeSeries } from "@/features/clinical/api/clinical-api"

const TIME_RANGES = [
  { value: "3m", labelKey: "dryEye.timeRange.3m" },
  { value: "6m", labelKey: "dryEye.timeRange.6m" },
  { value: "1y", labelKey: "dryEye.timeRange.1y" },
  { value: "all", labelKey: "dryEye.timeRange.all" },
] as const

interface DryEyeMetricChartsProps {
  patientId: string
}

export function DryEyeMetricCharts({ patientId }: DryEyeMetricChartsProps) {
  const { t, i18n } = useTranslation("clinical")
  const [timeRange, setTimeRange] = useState("all")
  const { data: response, isLoading } = useDryEyeMetricHistory(patientId, timeRange)
  const isVietnamese = i18n.language === "vi"

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-base">{t("dryEye.metricCharts")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-[200px] flex items-center justify-center text-muted-foreground">
            ...
          </div>
        </CardContent>
      </Card>
    )
  }

  const hasData = response?.metrics?.some((m) => m.dataPoints.length > 0) ?? false

  if (!hasData) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-base">{t("dryEye.metricCharts")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-[200px] flex items-center justify-center text-muted-foreground">
            {t("dryEye.noMetricData")}
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="space-y-4">
      {/* Time range selector */}
      <div className="flex items-center justify-between">
        <h3 className="text-base font-semibold">{t("dryEye.metricCharts")}</h3>
        <div className="flex items-center gap-1">
          {TIME_RANGES.map((range) => (
            <Button
              key={range.value}
              variant={timeRange === range.value ? "default" : "outline"}
              size="sm"
              className="h-7 text-xs px-2"
              onClick={() => setTimeRange(range.value)}
            >
              {t(range.labelKey)}
            </Button>
          ))}
        </div>
      </div>

      {/* Metric charts */}
      {response!.metrics.map((metric) => (
        <MetricChart
          key={metric.metricName}
          metric={metric}
          isVietnamese={isVietnamese}
          t={t}
        />
      ))}
    </div>
  )
}

function MetricChart({
  metric,
  isVietnamese,
  t,
}: {
  metric: MetricTimeSeries
  isVietnamese: boolean
  t: (key: string) => string
}) {
  const chartData = useMemo(() => {
    return metric.dataPoints.map((dp) => ({
      visitDate: format(
        new Date(dp.visitDate),
        "dd/MM/yyyy",
        isVietnamese ? { locale: vi } : undefined,
      ),
      od: dp.odValue,
      os: dp.osValue,
    }))
  }, [metric.dataPoints, isVietnamese])

  if (chartData.length === 0) return null

  const metricLabelKey = `dryEye.metric.${metric.metricName}`

  return (
    <Card>
      <CardHeader className="py-3 px-4">
        <CardTitle className="text-sm">{t(metricLabelKey)}</CardTitle>
      </CardHeader>
      <CardContent className="px-2 pb-3">
        <ResponsiveContainer width="100%" height={200}>
          <LineChart data={chartData} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
            <XAxis
              dataKey="visitDate"
              tick={{ fontSize: 11 }}
              interval="preserveStartEnd"
            />
            <YAxis tick={{ fontSize: 11 }} width={40} />
            <Tooltip
              content={({ active, payload }) => {
                if (!active || !payload?.length) return null
                const data = payload[0].payload as (typeof chartData)[number]
                return (
                  <div className="rounded-lg border bg-background p-2 shadow-md">
                    <p className="text-xs text-muted-foreground">{data.visitDate}</p>
                    <p className="text-sm">
                      <span className="font-medium" style={{ color: "hsl(var(--chart-1))" }}>
                        OD: {data.od !== null && data.od !== undefined ? data.od : "N/A"}
                      </span>
                    </p>
                    <p className="text-sm">
                      <span className="font-medium" style={{ color: "hsl(var(--chart-2))" }}>
                        OS: {data.os !== null && data.os !== undefined ? data.os : "N/A"}
                      </span>
                    </p>
                  </div>
                )
              }}
            />
            <Line
              type="monotone"
              dataKey="od"
              name="OD"
              stroke="hsl(var(--chart-1))"
              strokeWidth={2}
              dot={{ r: 3, fill: "hsl(var(--chart-1))" }}
              activeDot={{ r: 5 }}
              connectNulls
            />
            <Line
              type="monotone"
              dataKey="os"
              name="OS"
              stroke="hsl(var(--chart-2))"
              strokeWidth={2}
              dot={{ r: 3, fill: "hsl(var(--chart-2))" }}
              activeDot={{ r: 5 }}
              connectNulls
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}
