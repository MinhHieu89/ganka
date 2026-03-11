import { createFileRoute } from "@tanstack/react-router"
import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconHistory, IconChevronDown, IconChevronRight } from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Skeleton } from "@/shared/components/Skeleton"
import { useDispensingHistory } from "@/features/pharmacy/api/pharmacy-queries"
import type { DispensingRecordDto } from "@/features/pharmacy/api/pharmacy-api"

export const Route = createFileRoute("/_authenticated/pharmacy/dispensing-history")({
  component: DispensingHistoryPage,
})

function DispensingHistoryPage() {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")
  const [page, setPage] = useState(1)
  const [expandedId, setExpandedId] = useState<string | null>(null)

  const { data, isLoading } = useDispensingHistory(page)
  const items = data?.items ?? []
  const totalCount = data?.totalCount ?? 0
  const pageSize = 20
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

  const toggleExpand = (id: string) => {
    setExpandedId((prev) => (prev === id ? null : id))
  }

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="space-y-1">
        <div className="flex items-center gap-3">
          <IconHistory className="h-6 w-6 text-muted-foreground" />
          <h1 className="text-2xl font-bold">{t("queue.dispensingHistory")}</h1>
        </div>
        <p className="text-sm text-muted-foreground">
          {t("queue.dispensingHistorySubtitle")}
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">
            {t("queue.dispensingHistory")}
            {totalCount > 0 && (
              <Badge variant="secondary" className="ml-2 text-xs">
                {totalCount}
              </Badge>
            )}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-10 w-full" />
              ))}
            </div>
          ) : items.length === 0 ? (
            <p className="text-sm text-muted-foreground py-8 text-center">
              {t("queue.historyEmpty")}
            </p>
          ) : (
            <>
              <div className="border rounded-lg overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="w-10 px-3 py-2" />
                      <th className="text-left px-3 py-2 font-medium text-muted-foreground">
                        {t("queue.historyPatientName")}
                      </th>
                      <th className="text-left px-3 py-2 font-medium text-muted-foreground">
                        {t("queue.historyDispensedAt")}
                      </th>
                      <th className="text-right px-3 py-2 font-medium text-muted-foreground">
                        {t("queue.historyLines")}
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {items.map((record) => (
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

              {/* Pagination */}
              <div className="flex items-center justify-between mt-4">
                <p className="text-sm text-muted-foreground">
                  {tCommon("table.page")} {page} / {totalPages}
                </p>
                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={page <= 1}
                  >
                    {tCommon("buttons.previous")}
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    disabled={page >= totalPages}
                  >
                    {tCommon("buttons.next")}
                  </Button>
                </div>
              </div>
            </>
          )}
        </CardContent>
      </Card>
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
  const { t } = useTranslation("pharmacy")
  const lineCount = record.lines?.length ?? 0

  return (
    <>
      <tr
        className="border-t cursor-pointer hover:bg-muted/30 transition-colors"
        onClick={onToggle}
      >
        <td className="px-3 py-2">
          <Button variant="ghost" size="sm" className="h-7 w-7 p-0">
            {expanded ? (
              <IconChevronDown className="h-4 w-4" />
            ) : (
              <IconChevronRight className="h-4 w-4" />
            )}
          </Button>
        </td>
        <td className="px-3 py-2">
          <span className="font-medium">{record.patientName}</span>
        </td>
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
          <td colSpan={4} className="px-3 py-2 bg-muted/20">
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
                        {t("queue.skipped")}
                      </Badge>
                    )}
                  </div>
                </div>
              ))}
              {record.overrideReason && (
                <p className="text-xs text-yellow-600 mt-1">
                  {t("queue.overrideReasonLabel")}: {record.overrideReason}
                </p>
              )}
            </div>
          </td>
        </tr>
      )}
    </>
  )
}
