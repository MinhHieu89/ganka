import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { vi } from "date-fns/locale"
import { IconArrowUp, IconArrowDown, IconMinus } from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { cn } from "@/shared/lib/utils"
import {
  useOsdiHistory,
  useDryEyeComparison,
} from "@/features/clinical/api/clinical-api"
import type { DryEyeAssessmentDto } from "@/features/clinical/api/clinical-api"

// Metric config: key matches DTO fields, higherIsBetter drives delta color
interface MetricConfig {
  key: string
  label: string
  unit?: string
  higherIsBetter: boolean
}

const METRICS: MetricConfig[] = [
  { key: "Tbut", label: "TBUT", unit: "s", higherIsBetter: true },
  { key: "Schirmer", label: "Schirmer", unit: "mm", higherIsBetter: true },
  { key: "MeibomianGrading", label: "Meibomian", higherIsBetter: false },
  { key: "TearMeniscus", label: "Tear Meniscus", unit: "mm", higherIsBetter: true },
  { key: "Staining", label: "Staining", higherIsBetter: false },
]

function getMetricValue(assessment: DryEyeAssessmentDto | null, eye: "od" | "os", key: string): number | null {
  if (!assessment) return null
  const fieldKey = `${eye}${key}` as keyof DryEyeAssessmentDto
  const val = assessment[fieldKey]
  return val !== null && val !== undefined ? Number(val) : null
}

function DeltaIndicator({
  oldVal,
  newVal,
  higherIsBetter,
}: {
  oldVal: number | null
  newVal: number | null
  higherIsBetter: boolean
}) {
  const { t } = useTranslation("clinical")

  if (oldVal === null || newVal === null) return null

  const diff = newVal - oldVal
  if (diff === 0) {
    return (
      <span className="flex items-center gap-0.5 text-xs text-muted-foreground">
        <IconMinus className="h-3 w-3" />
      </span>
    )
  }

  const isImprovement = higherIsBetter ? diff > 0 : diff < 0
  const sign = diff > 0 ? "+" : ""

  return (
    <span
      className={cn(
        "flex items-center gap-0.5 text-xs font-medium",
        isImprovement ? "text-green-600" : "text-red-600",
      )}
      title={isImprovement ? t("dryEye.improved") : t("dryEye.worsened")}
    >
      {isImprovement ? (
        <IconArrowUp className="h-3 w-3" />
      ) : (
        <IconArrowDown className="h-3 w-3" />
      )}
      {sign}{diff.toFixed(1)}
    </span>
  )
}

interface DryEyeComparisonPanelProps {
  patientId: string
}

export function DryEyeComparisonPanel({ patientId }: DryEyeComparisonPanelProps) {
  const { t, i18n } = useTranslation("clinical")
  const isVietnamese = i18n.language === "vi"

  const [visitId1, setVisitId1] = useState<string>("")
  const [visitId2, setVisitId2] = useState<string>("")

  // Use OSDI history to get list of visits with dry eye data
  const { data: osdiResponse } = useOsdiHistory(patientId)

  // Build visit options from OSDI history (these visits have dry eye assessments)
  const visitOptions = useMemo(() => {
    if (!osdiResponse?.items) return []
    return osdiResponse.items.map((item) => ({
      id: item.visitId,
      label: format(
        new Date(item.visitDate),
        "dd/MM/yyyy",
        isVietnamese ? { locale: vi } : undefined,
      ),
      date: item.visitDate,
    }))
  }, [osdiResponse, isVietnamese])

  // Fetch comparison data when both visits are selected
  const { data: comparison, isLoading } = useDryEyeComparison(
    patientId,
    visitId1 || undefined,
    visitId2 || undefined,
  )

  const assessment1 = comparison?.visit1?.assessment ?? null
  const assessment2 = comparison?.visit2?.assessment ?? null

  if (visitOptions.length < 2) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-base">{t("dryEye.comparison")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center h-32 text-sm text-muted-foreground">
            {t("dryEye.notEnoughVisits")}
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{t("dryEye.comparison")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Visit selectors */}
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="text-xs text-muted-foreground mb-1 block">
              {t("comparison.visit1")}
            </label>
            <Select value={visitId1} onValueChange={setVisitId1}>
              <SelectTrigger className="h-8">
                <SelectValue placeholder={t("dryEye.selectVisit")} />
              </SelectTrigger>
              <SelectContent>
                {visitOptions.map((opt) => (
                  <SelectItem key={opt.id} value={opt.id} disabled={opt.id === visitId2}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div>
            <label className="text-xs text-muted-foreground mb-1 block">
              {t("comparison.visit2")}
            </label>
            <Select value={visitId2} onValueChange={setVisitId2}>
              <SelectTrigger className="h-8">
                <SelectValue placeholder={t("dryEye.selectVisit")} />
              </SelectTrigger>
              <SelectContent>
                {visitOptions.map((opt) => (
                  <SelectItem key={opt.id} value={opt.id} disabled={opt.id === visitId1}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        {/* Comparison table */}
        {visitId1 && visitId2 ? (
          isLoading ? (
            <div className="flex items-center justify-center h-32 text-muted-foreground">
              ...
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b">
                    <th className="text-left py-2 pr-2 font-medium text-muted-foreground w-24">
                      {t("osdi.score")}
                    </th>
                    <th className="text-center py-2 px-2 font-medium" colSpan={2}>
                      {t("refraction.od")}
                    </th>
                    <th className="text-center py-2 px-2 font-medium" colSpan={2}>
                      {t("refraction.os")}
                    </th>
                  </tr>
                  <tr className="border-b text-xs text-muted-foreground">
                    <th />
                    <th className="text-center py-1 px-2">{t("comparison.visit1")}</th>
                    <th className="text-center py-1 px-2">{t("comparison.visit2")}</th>
                    <th className="text-center py-1 px-2">{t("comparison.visit1")}</th>
                    <th className="text-center py-1 px-2">{t("comparison.visit2")}</th>
                  </tr>
                </thead>
                <tbody>
                  {METRICS.map((metric) => {
                    const odVal1 = getMetricValue(assessment1, "od", metric.key)
                    const odVal2 = getMetricValue(assessment2, "od", metric.key)
                    const osVal1 = getMetricValue(assessment1, "os", metric.key)
                    const osVal2 = getMetricValue(assessment2, "os", metric.key)

                    return (
                      <tr key={metric.key} className="border-b last:border-b-0">
                        <td className="py-2 pr-2 text-xs text-muted-foreground">
                          {metric.label}
                          {metric.unit && (
                            <span className="ml-1 text-[10px]">({metric.unit})</span>
                          )}
                        </td>
                        {/* OD */}
                        <td className="text-center py-2 px-2 tabular-nums">
                          {odVal1 !== null ? odVal1 : "--"}
                        </td>
                        <td className="text-center py-2 px-2">
                          <div className="flex items-center justify-center gap-1">
                            <span className="tabular-nums">
                              {odVal2 !== null ? odVal2 : "--"}
                            </span>
                            <DeltaIndicator
                              oldVal={odVal1}
                              newVal={odVal2}
                              higherIsBetter={metric.higherIsBetter}
                            />
                          </div>
                        </td>
                        {/* OS */}
                        <td className="text-center py-2 px-2 tabular-nums">
                          {osVal1 !== null ? osVal1 : "--"}
                        </td>
                        <td className="text-center py-2 px-2">
                          <div className="flex items-center justify-center gap-1">
                            <span className="tabular-nums">
                              {osVal2 !== null ? osVal2 : "--"}
                            </span>
                            <DeltaIndicator
                              oldVal={osVal1}
                              newVal={osVal2}
                              higherIsBetter={metric.higherIsBetter}
                            />
                          </div>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          )
        ) : (
          <div className="flex items-center justify-center h-32 text-sm text-muted-foreground">
            {t("dryEye.selectBothVisits")}
          </div>
        )}
      </CardContent>
    </Card>
  )
}
