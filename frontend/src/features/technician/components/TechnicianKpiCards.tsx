import { useTranslation } from "react-i18next"
import { Card, CardContent } from "@/shared/components/Card"
import { Skeleton } from "@/shared/components/Skeleton"
import {
  IconClock,
  IconEye,
  IconCircleCheck,
  IconAlertTriangle,
} from "@tabler/icons-react"
import type { TechnicianKpi } from "@/features/technician/types/technician.types"

interface TechnicianKpiCardsProps {
  kpi: TechnicianKpi | undefined
  isLoading: boolean
}

const kpiConfig = [
  {
    key: "waiting" as const,
    labelKey: "kpi.waiting",
    subKey: "kpi.waitingSub",
    color: "var(--tech-kpi-waiting)",
    icon: IconClock,
  },
  {
    key: "inProgress" as const,
    labelKey: "kpi.inProgress",
    subKey: "kpi.inProgressSub",
    color: "var(--tech-kpi-in-progress)",
    icon: IconEye,
  },
  {
    key: "completed" as const,
    labelKey: "kpi.completed",
    subKey: "kpi.completedSub",
    color: "var(--tech-kpi-completed)",
    icon: IconCircleCheck,
  },
  {
    key: "redFlag" as const,
    labelKey: "kpi.redFlag",
    subKey: "kpi.redFlagSub",
    color: "var(--tech-kpi-red-flag)",
    icon: IconAlertTriangle,
  },
]

export function TechnicianKpiCards({ kpi, isLoading }: TechnicianKpiCardsProps) {
  const { t } = useTranslation("technician")

  if (isLoading) {
    return (
      <div className="grid grid-cols-4 gap-3">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-[88px] w-full" />
        ))}
      </div>
    )
  }

  return (
    <div className="grid grid-cols-4 gap-3">
      {kpiConfig.map((item) => {
        const Icon = item.icon
        const value = kpi?.[item.key] ?? 0
        return (
          <Card key={item.key}>
            <CardContent className="p-4">
              <div className="flex items-center justify-between">
                <span className="text-xs">{t(item.labelKey)}</span>
                <Icon className="h-4 w-4 text-muted-foreground" />
              </div>
              <div
                className="text-2xl font-bold tabular-nums mt-1"
                style={{ color: item.color }}
                aria-live="polite"
              >
                {value}
              </div>
              <span className="text-xs text-muted-foreground">
                {t(item.subKey)}
              </span>
            </CardContent>
          </Card>
        )
      })}
    </div>
  )
}
