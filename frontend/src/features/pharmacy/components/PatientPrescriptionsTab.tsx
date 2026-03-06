import { useState } from "react"
import { useTranslation } from "react-i18next"
import { Link } from "@tanstack/react-router"
import { IconExternalLink, IconChevronDown, IconChevronUp, IconHistory } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Skeleton } from "@/shared/components/Skeleton"
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/shared/components/Collapsible"
import { Separator } from "@/shared/components/Separator"
import { PharmacyQueueTable } from "@/features/pharmacy/components/PharmacyQueueTable"
import { useDispensingHistory } from "@/features/pharmacy/api/pharmacy-queries"

interface PatientPrescriptionsTabProps {
  patientId: string
}

export function PatientPrescriptionsTab({ patientId }: PatientPrescriptionsTabProps) {
  const { t } = useTranslation("pharmacy")
  const [historyOpen, setHistoryOpen] = useState(false)

  const { data: historyResult, isLoading: historyLoading } = useDispensingHistory(1, patientId)
  const historyItems = historyResult?.items ?? []
  const historyTotal = historyResult?.totalCount ?? 0

  return (
    <div className="space-y-6">
      {/* Pending prescriptions section */}
      <div className="space-y-3">
        <div className="flex items-center justify-between gap-2">
          <h3 className="text-sm font-semibold text-foreground">
            {t("queue.pendingPrescriptions")}
          </h3>
          <Button variant="ghost" size="sm" className="h-7 text-xs gap-1" asChild>
            <Link to={"/pharmacy/queue" as string}>
              <IconExternalLink className="h-3.5 w-3.5" />
              {t("queue.viewFullQueue")}
            </Link>
          </Button>
        </div>

        {/* PharmacyQueueTable filtered to this patient */}
        <PharmacyQueueTable patientId={patientId} />
      </div>

      <Separator />

      {/* Dispensing history section (collapsible) */}
      <Collapsible open={historyOpen} onOpenChange={setHistoryOpen}>
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <IconHistory className="h-4 w-4 text-muted-foreground" />
            <h3 className="text-sm font-semibold text-foreground">
              {t("queue.dispensingHistory")}
            </h3>
            {historyTotal > 0 && (
              <Badge variant="secondary" className="text-xs">
                {historyTotal}
              </Badge>
            )}
          </div>
          <CollapsibleTrigger asChild>
            <Button variant="ghost" size="sm" className="h-7 w-7 p-0">
              {historyOpen ? (
                <IconChevronUp className="h-4 w-4" />
              ) : (
                <IconChevronDown className="h-4 w-4" />
              )}
            </Button>
          </CollapsibleTrigger>
        </div>

        <CollapsibleContent className="mt-3">
          {historyLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-8 w-full" />
              <Skeleton className="h-8 w-full" />
              <Skeleton className="h-8 w-3/4" />
            </div>
          ) : historyItems.length === 0 ? (
            <p className="text-sm text-muted-foreground py-4 text-center">
              {t("queue.historyEmpty")}
            </p>
          ) : (
            <div className="border rounded-lg overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="text-left px-3 py-2 font-medium text-muted-foreground">
                      {t("queue.historyDispensedAt")}
                    </th>
                    <th className="text-left px-3 py-2 font-medium text-muted-foreground">
                      {t("queue.historyDispensedBy")}
                    </th>
                    <th className="text-right px-3 py-2 font-medium text-muted-foreground">
                      {t("queue.historyLines")}
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {historyItems.map((record) => (
                    <tr key={record.id} className="border-t">
                      <td className="px-3 py-2">
                        {new Date(record.dispensedAt).toLocaleString("vi-VN", {
                          year: "numeric",
                          month: "2-digit",
                          day: "2-digit",
                          hour: "2-digit",
                          minute: "2-digit",
                        })}
                      </td>
                      <td className="px-3 py-2 text-muted-foreground">
                        {record.dispensedByName}
                      </td>
                      <td className="px-3 py-2 text-right">
                        <Badge variant="outline" className="text-xs">
                          {record.lineCount}
                        </Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CollapsibleContent>
      </Collapsible>
    </div>
  )
}
