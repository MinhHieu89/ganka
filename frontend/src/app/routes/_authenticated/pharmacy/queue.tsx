import { createFileRoute } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { IconClock, IconRefresh } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { PharmacyQueueTable } from "@/features/pharmacy/components/PharmacyQueueTable"
import { usePendingPrescriptions } from "@/features/pharmacy/api/pharmacy-queries"

export const Route = createFileRoute("/_authenticated/pharmacy/queue")({
  component: PharmacyQueuePage,
})

function PharmacyQueuePage() {
  const { t } = useTranslation("pharmacy")
  const { data: prescriptions } = usePendingPrescriptions()

  const pendingCount = prescriptions?.length ?? 0

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-start justify-between gap-4">
        <div className="space-y-1">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold">{t("queue.title")}</h1>
            {pendingCount > 0 && (
              <Badge variant="destructive" className="text-sm px-2 py-0.5">
                {pendingCount}
              </Badge>
            )}
          </div>
          <p className="text-sm text-muted-foreground">{t("queue.subtitle")}</p>
        </div>

        {/* Auto-refresh info */}
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground shrink-0 mt-1">
          <IconRefresh className="h-3.5 w-3.5" />
          <span>{t("queue.autoRefresh")}</span>
        </div>
      </div>

      {/* Pending count summary */}
      {pendingCount > 0 && (
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <IconClock className="h-4 w-4" />
          <span>
            {t("queue.pendingCount", { count: pendingCount })}
          </span>
        </div>
      )}

      {/* Queue table */}
      <PharmacyQueueTable />
    </div>
  )
}
