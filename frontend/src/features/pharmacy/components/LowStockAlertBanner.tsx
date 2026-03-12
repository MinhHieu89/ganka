import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconPackage, IconChevronDown, IconChevronUp } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/shared/components/Collapsible"
import { useLowStockAlerts } from "@/features/pharmacy/api/pharmacy-queries"

export function LowStockAlertBanner() {
  const { t } = useTranslation("pharmacy")
  const [open, setOpen] = useState(false)
  const { data: alerts, isLoading } = useLowStockAlerts()

  if (isLoading) return null

  const count = alerts?.length ?? 0

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <div
        className={
          count > 0
            ? "rounded-lg p-3 bg-yellow-50 border border-yellow-300 dark:bg-yellow-950/30 dark:border-yellow-700"
            : "rounded-lg p-3 bg-muted/40 border border-border"
        }
      >
        <div className="flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 flex-1 min-w-0">
            <IconPackage
              className={`h-4 w-4 shrink-0 ${count > 0 ? "text-yellow-600 dark:text-yellow-400" : "text-muted-foreground"}`}
            />
            <span
              className={`text-sm font-medium ${count > 0 ? "text-yellow-700 dark:text-yellow-400" : "text-muted-foreground"}`}
            >
              {t("alerts.lowStockTitle")}
            </span>
            {count > 0 ? (
              <Badge
                variant="outline"
                className="text-xs shrink-0 border-yellow-500 text-yellow-700 dark:text-yellow-400"
              >
                {count}
              </Badge>
            ) : (
              <span className="text-xs text-muted-foreground">{t("alerts.lowStockNone")}</span>
            )}
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
              <div className="grid grid-cols-3 gap-2 text-xs font-medium text-muted-foreground pb-1 border-b">
                <span>{t("alerts.drugName")}</span>
                <span>{t("alerts.currentStock")}</span>
                <span>{t("alerts.minRequired")}</span>
              </div>
              {alerts?.map((alert) => (
                <div
                  key={alert.drugCatalogItemId}
                  className="grid grid-cols-3 gap-2 text-xs py-0.5"
                >
                  <span className="font-medium truncate">{alert.drugName}</span>
                  <span
                    className={`font-medium ${alert.totalStock === 0 ? "text-destructive" : "text-yellow-600 dark:text-yellow-400"}`}
                  >
                    {alert.totalStock}
                  </span>
                  <span className="text-muted-foreground">{alert.minStockLevel}</span>
                </div>
              ))}
            </div>
          </CollapsibleContent>
        )}
      </div>
    </Collapsible>
  )
}
