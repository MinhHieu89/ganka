import { Badge } from "@/shared/components/Badge"
import type { ReceptionistSource } from "@/features/receptionist/types/receptionist.types"

const sourceConfig: Record<
  ReceptionistSource,
  { label: string; bgVar: string; textVar: string }
> = {
  appointment: {
    label: "Hen",
    bgVar: "var(--source-appointment-bg)",
    textVar: "var(--source-appointment-text)",
  },
  walkin: {
    label: "Walk-in",
    bgVar: "var(--source-walkin-bg)",
    textVar: "var(--source-walkin-text)",
  },
}

interface SourceBadgeProps {
  source: ReceptionistSource
}

export function SourceBadge({ source }: SourceBadgeProps) {
  const config = sourceConfig[source]
  return (
    <Badge
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
