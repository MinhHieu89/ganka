import { useTranslation } from "react-i18next"
import { Badge } from "@/shared/components/Badge"
import type { ReceptionistSource } from "@/features/receptionist/types/receptionist.types"

const sourceConfig: Record<
  ReceptionistSource,
  { labelKey: string; bgVar: string; textVar: string }
> = {
  appointment: {
    labelKey: "source.appointment",
    bgVar: "var(--source-appointment-bg)",
    textVar: "var(--source-appointment-text)",
  },
  walkin: {
    labelKey: "source.walkIn",
    bgVar: "var(--source-walkin-bg)",
    textVar: "var(--source-walkin-text)",
  },
}

interface SourceBadgeProps {
  source: ReceptionistSource
}

export function SourceBadge({ source }: SourceBadgeProps) {
  const { t } = useTranslation("receptionist")
  const config = sourceConfig[source]
  return (
    <Badge
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
