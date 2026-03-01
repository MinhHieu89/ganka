import { useTranslation } from "react-i18next"
import { IconAlertTriangle } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Tooltip, TooltipContent, TooltipTrigger } from "@/shared/components/Tooltip"
import type { AllergyDto, AllergySeverity } from "@/features/patient/api/patient-api"

interface AllergyAlertProps {
  allergies: AllergyDto[]
  compact?: boolean
}

const severityColor: Record<AllergySeverity, string> = {
  Severe: "bg-red-100 text-red-800 border-red-200 dark:bg-red-900/30 dark:text-red-300 dark:border-red-800",
  Moderate: "bg-orange-100 text-orange-800 border-orange-200 dark:bg-orange-900/30 dark:text-orange-300 dark:border-orange-800",
  Mild: "bg-muted text-muted-foreground border-border",
}

const severityBadgeVariant: Record<AllergySeverity, string> = {
  Severe: "bg-red-500 text-white hover:bg-red-500",
  Moderate: "bg-orange-500 text-white hover:bg-orange-500",
  Mild: "bg-muted text-muted-foreground hover:bg-muted",
}

export function AllergyAlert({ allergies, compact = false }: AllergyAlertProps) {
  const { t } = useTranslation("patient")

  if (!allergies || allergies.length === 0) return null

  if (compact) {
    return (
      <Tooltip>
        <TooltipTrigger asChild>
          <Badge variant="outline" className="gap-1 cursor-help border-orange-300 text-orange-700 dark:border-orange-700 dark:text-orange-400">
            <IconAlertTriangle className="h-3 w-3" />
            {t("compact_allergies", { count: allergies.length })}
          </Badge>
        </TooltipTrigger>
        <TooltipContent>
          <div className="space-y-1">
            {allergies.map((a) => (
              <div key={a.id} className="flex items-center gap-2 text-xs">
                <span>{a.name}</span>
                <span className="text-muted-foreground">({t(a.severity.toLowerCase())})</span>
              </div>
            ))}
          </div>
        </TooltipContent>
      </Tooltip>
    )
  }

  return (
    <div className="flex items-start gap-3 p-3 bg-orange-50 border border-orange-200 dark:bg-orange-950/30 dark:border-orange-800">
      <IconAlertTriangle className="h-5 w-5 text-orange-600 dark:text-orange-400 shrink-0 mt-0.5" />
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-orange-800 dark:text-orange-300 mb-1">
          {t("allergyAlert")}
        </p>
        <div className="flex flex-wrap gap-1.5">
          {allergies.map((allergy) => (
            <Badge
              key={allergy.id}
              variant="outline"
              className={severityBadgeVariant[allergy.severity]}
            >
              {allergy.name} ({t(allergy.severity.toLowerCase())})
            </Badge>
          ))}
        </div>
      </div>
    </div>
  )
}
