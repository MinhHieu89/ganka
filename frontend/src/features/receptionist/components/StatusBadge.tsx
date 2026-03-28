import { useTranslation } from "react-i18next"
import { Badge } from "@/shared/components/Badge"
import type { ReceptionistStatus } from "@/features/receptionist/types/receptionist.types"

const statusConfig: Record<
  ReceptionistStatus,
  { labelKey: string; bgVar: string; textVar: string }
> = {
  not_arrived: {
    labelKey: "status.notArrived",
    bgVar: "var(--status-not-arrived-bg)",
    textVar: "var(--status-not-arrived-text)",
  },
  waiting: {
    labelKey: "status.waiting",
    bgVar: "var(--status-waiting-bg)",
    textVar: "var(--status-waiting-text)",
  },
  examining: {
    labelKey: "status.examining",
    bgVar: "var(--status-examining-bg)",
    textVar: "var(--status-examining-text)",
  },
  completed: {
    labelKey: "status.completed",
    bgVar: "var(--status-completed-bg)",
    textVar: "var(--status-completed-text)",
  },
}

interface StatusBadgeProps {
  status: ReceptionistStatus
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const { t } = useTranslation("receptionist")
  const config = statusConfig[status]
  return (
    <Badge
      role="status"
      className="border-transparent rounded-full"
      style={{
        backgroundColor: config.bgVar,
        color: config.textVar,
      }}
    >
      {t(config.labelKey)}
    </Badge>
  )
}
