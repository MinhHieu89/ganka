import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Skeleton } from "@/shared/components/Skeleton"
import {
  IconCalendar,
  IconClock,
  IconStethoscope,
  IconCircleCheck,
} from "@tabler/icons-react"
import type { ReceptionistKpi } from "@/features/receptionist/types/receptionist.types"

interface KpiCardsProps {
  kpi: ReceptionistKpi | undefined
  isLoading: boolean
}

const kpiConfig = [
  {
    key: "todayAppointments" as const,
    label: "Lich hen hom nay",
    color: "var(--kpi-appointments)",
    icon: IconCalendar,
    subKey: "notArrived" as const,
    subLabel: "chua den",
  },
  {
    key: "waiting" as const,
    label: "Cho kham",
    color: "var(--kpi-waiting)",
    icon: IconClock,
    subKey: null,
    subLabel: null,
  },
  {
    key: "examining" as const,
    label: "Dang kham",
    color: "var(--kpi-examining)",
    icon: IconStethoscope,
    subKey: null,
    subLabel: null,
  },
  {
    key: "completed" as const,
    label: "Hoan thanh",
    color: "var(--kpi-completed)",
    icon: IconCircleCheck,
    subKey: null,
    subLabel: null,
  },
]

export function KpiCards({ kpi, isLoading }: KpiCardsProps) {
  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {kpiConfig.map((item) => {
        const Icon = item.icon
        const value = kpi?.[item.key]
        const subValue = item.subKey ? kpi?.[item.subKey] : null
        return (
          <Card key={item.key}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-semibold">
                {item.label}
              </CardTitle>
              <Icon className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <Skeleton className="h-8 w-16" />
              ) : (
                <div className="flex items-baseline gap-2">
                  <span
                    className="text-[28px] font-semibold tabular-nums"
                    style={{ color: item.color }}
                    aria-live="polite"
                  >
                    {value ?? 0}
                  </span>
                  {subValue != null && (
                    <span className="text-sm text-muted-foreground">
                      {subValue} {item.subLabel}
                    </span>
                  )}
                </div>
              )}
            </CardContent>
          </Card>
        )
      })}
    </div>
  )
}
