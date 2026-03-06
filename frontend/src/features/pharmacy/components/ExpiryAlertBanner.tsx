import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconAlertTriangle, IconChevronDown, IconChevronUp } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/shared/components/Collapsible"
import { useExpiryAlerts } from "@/features/pharmacy/api/pharmacy-queries"

type ThresholdDays = 30 | 60 | 90

export function ExpiryAlertBanner() {
  const { t } = useTranslation("pharmacy")
  const [days, setDays] = useState<ThresholdDays>(30)
  const [open, setOpen] = useState(false)

  const { data: alerts, isLoading } = useExpiryAlerts(days)

  if (isLoading) return null

  const count = alerts?.length ?? 0
  if (count === 0 && days === 30) {
    // Check 60-day window before hiding completely — only hide if truly no 30-day alerts
    // Show a subtle "no alerts" state for user confidence
  }

  const thresholds: ThresholdDays[] = [30, 60, 90]

  function getBannerStyle() {
    if (count === 0) return "bg-muted/40 border border-border"
    if (days === 30) return "bg-destructive/10 border border-destructive/40"
    if (days === 60) return "bg-yellow-50 border border-yellow-300 dark:bg-yellow-950/30 dark:border-yellow-700"
    return "bg-muted border border-border"
  }

  function getIconStyle() {
    if (count === 0) return "text-muted-foreground"
    if (days === 30) return "text-destructive"
    if (days === 60) return "text-yellow-600 dark:text-yellow-400"
    return "text-muted-foreground"
  }

  function getTextStyle() {
    if (count === 0) return "text-muted-foreground"
    if (days === 30) return "text-destructive"
    if (days === 60) return "text-yellow-700 dark:text-yellow-400"
    return "text-foreground"
  }

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <div className={`rounded-lg p-3 ${getBannerStyle()}`}>
        <div className="flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 flex-1 min-w-0">
            <IconAlertTriangle className={`h-4 w-4 shrink-0 ${getIconStyle()}`} />
            <span className={`text-sm font-medium ${getTextStyle()}`}>
              {t("alerts.expiryTitle")}
            </span>
            {count > 0 ? (
              <Badge
                variant={days === 30 ? "destructive" : "outline"}
                className={`text-xs shrink-0 ${days === 60 ? "border-yellow-500 text-yellow-700 dark:text-yellow-400" : ""}`}
              >
                {count}
              </Badge>
            ) : (
              <span className="text-xs text-muted-foreground">{t("alerts.expiryNone")}</span>
            )}
          </div>

          {/* Threshold selector */}
          <div className="flex items-center gap-1 shrink-0">
            {thresholds.map((d) => (
              <Button
                key={d}
                variant={days === d ? "default" : "outline"}
                size="sm"
                className="h-6 text-xs px-2"
                onClick={() => setDays(d)}
              >
                {t(`alerts.days${d}`)}
              </Button>
            ))}
          </div>

          {count > 0 && (
            <CollapsibleTrigger asChild>
              <Button variant="ghost" size="sm" className="h-7 w-7 p-0 shrink-0">
                {open ? (
                  <IconChevronUp className="h-4 w-4" />
                ) : (
                  <IconChevronDown className="h-4 w-4" />
                )}
              </Button>
            </CollapsibleTrigger>
          )}
        </div>

        {count > 0 && (
          <CollapsibleContent>
            <div className="mt-3 space-y-1 max-h-48 overflow-y-auto">
              <div className="grid grid-cols-4 gap-2 text-xs font-medium text-muted-foreground pb-1 border-b">
                <span>{t("alerts.drugName")}</span>
                <span>{t("alerts.batchNumber")}</span>
                <span>{t("alerts.expiryDate")}</span>
                <span>{t("alerts.daysLeft", { days: "" }).trim()}</span>
              </div>
              {alerts?.map((alert) => (
                <div
                  key={`${alert.drugCatalogItemId}-${alert.batchNumber}`}
                  className="grid grid-cols-4 gap-2 text-xs py-0.5"
                >
                  <span className="font-medium truncate">{alert.drugName}</span>
                  <span className="font-mono text-muted-foreground truncate">{alert.batchNumber}</span>
                  <span
                    className={
                      alert.daysUntilExpiry <= 0
                        ? "text-destructive"
                        : alert.daysUntilExpiry <= 30
                          ? "text-destructive font-medium"
                          : alert.daysUntilExpiry <= 60
                            ? "text-yellow-600 dark:text-yellow-400"
                            : ""
                    }
                  >
                    {new Date(alert.expiryDate).toLocaleDateString("vi-VN")}
                  </span>
                  <span
                    className={
                      alert.daysUntilExpiry <= 30 ? "text-destructive font-medium" : "text-muted-foreground"
                    }
                  >
                    {t("alerts.daysLeft", { days: alert.daysUntilExpiry })}
                  </span>
                </div>
              ))}
            </div>
          </CollapsibleContent>
        )}
      </div>
    </Collapsible>
  )
}
