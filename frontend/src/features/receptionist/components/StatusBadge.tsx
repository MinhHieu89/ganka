import { Badge } from "@/shared/components/Badge"
import type { ReceptionistStatus } from "@/features/receptionist/types/receptionist.types"

const statusConfig: Record<
  ReceptionistStatus,
  { label: string; bgVar: string; textVar: string }
> = {
  not_arrived: {
    label: "Chua den",
    bgVar: "var(--status-not-arrived-bg)",
    textVar: "var(--status-not-arrived-text)",
  },
  waiting: {
    label: "Cho kham",
    bgVar: "var(--status-waiting-bg)",
    textVar: "var(--status-waiting-text)",
  },
  examining: {
    label: "Dang kham",
    bgVar: "var(--status-examining-bg)",
    textVar: "var(--status-examining-text)",
  },
  completed: {
    label: "Hoan thanh",
    bgVar: "var(--status-completed-bg)",
    textVar: "var(--status-completed-text)",
  },
}

interface StatusBadgeProps {
  status: ReceptionistStatus
}

export function StatusBadge({ status }: StatusBadgeProps) {
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
      {config.label}
    </Badge>
  )
}
