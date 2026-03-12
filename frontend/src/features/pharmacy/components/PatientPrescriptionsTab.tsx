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
import type { DispensingRecordDto } from "@/features/pharmacy/api/pharmacy-api"

interface PatientPrescriptionsTabProps {
  patientId: string
}

export function PatientPrescriptionsTab({ patientId }: PatientPrescriptionsTabProps) {
  const { t } = useTranslation("pharmacy")
  const [historyOpen, setHistoryOpen] = useState(true)
  const [expandedId, setExpandedId] = useState<string | null>(null)

  const { data: historyResult, isLoading: historyLoading } = useDispensingHistory(1, patientId)
  const historyItems = historyResult?.items ?? []
  const historyTotal = historyResult?.totalCount ?? 0

  const toggleExpand = (id: string) => {
    setExpandedId((prev) => (prev === id ? null : id))
  }

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

      {/* Dispensing history section (collapsible, expanded by default) */}
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
                    <th className="text-right px-3 py-2 font-medium text-muted-foreground">
                      {t("queue.historyLines")}
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {historyItems.map((record) => (
                    <HistoryRow
                      key={record.id}
                      record={record}
                      expanded={expandedId === record.id}
                      onToggle={() => toggleExpand(record.id)}
                    />
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

function HistoryRow({
  record,
  expanded,
  onToggle,
}: {
  record: DispensingRecordDto
  expanded: boolean
  onToggle: () => void
}) {
  const lineCount = record.lines?.length ?? 0

  return (
    <>
      <tr
        className="border-t cursor-pointer hover:bg-muted/30 transition-colors"
        onClick={onToggle}
      >
        <td className="px-3 py-2">
          {new Date(record.dispensedAt).toLocaleString("vi-VN", {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
          })}
        </td>
        <td className="px-3 py-2 text-right">
          <Badge variant="outline" className="text-xs">
            {lineCount}
          </Badge>
        </td>
      </tr>
      {expanded && record.lines && record.lines.length > 0 && (
        <tr>
          <td colSpan={2} className="px-3 py-2 bg-muted/20">
            <div className="space-y-1">
              {record.lines.map((line) => (
                <div
                  key={line.id}
                  className="flex items-center justify-between text-xs"
                >
                  <span className="text-foreground">{line.drugName}</span>
                  <div className="flex items-center gap-2">
                    <span className="text-muted-foreground">
                      x{line.quantity} {line.unit}
                    </span>
                    {line.status === 1 && (
                      <Badge variant="outline" className="text-xs text-yellow-600">
                        Bỏ qua
                      </Badge>
                    )}
                  </div>
                </div>
              ))}
              {record.overrideReason && (
                <p className="text-xs text-yellow-600 mt-1">
                  Ghi đè: {record.overrideReason}
                </p>
              )}
            </div>
          </td>
        </tr>
      )}
    </>
  )
}
