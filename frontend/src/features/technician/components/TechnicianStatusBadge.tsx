import { useTranslation } from "react-i18next"
import { Badge } from "@/shared/components/Badge"
import type { TechnicianStatus } from "@/features/technician/types/technician.types"

const statusStyles: Record<TechnicianStatus, { bg: string; color: string }> = {
  waiting: {
    bg: "var(--tech-status-waiting-bg)",
    color: "var(--tech-status-waiting-text)",
  },
  in_progress: {
    bg: "var(--tech-status-in-progress-bg)",
    color: "var(--tech-status-in-progress-text)",
  },
  red_flag: {
    bg: "var(--tech-status-red-flag-bg)",
    color: "var(--tech-status-red-flag-text)",
  },
  completed: {
    bg: "var(--tech-status-completed-bg)",
    color: "var(--tech-status-completed-text)",
  },
}

const statusKeys: Record<TechnicianStatus, string> = {
  waiting: "status.waiting",
  in_progress: "status.inProgress",
  red_flag: "status.redFlag",
  completed: "status.completed",
}

interface TechnicianStatusBadgeProps {
  status: TechnicianStatus
}

export function TechnicianStatusBadge({ status }: TechnicianStatusBadgeProps) {
  const { t } = useTranslation("technician")
  const style = statusStyles[status]

  return (
    <Badge
      role="status"
      variant="outline"
      className="border-0 font-normal"
      style={{
        backgroundColor: style.bg,
        color: style.color,
      }}
    >
      {t(statusKeys[status])}
    </Badge>
  )
}
