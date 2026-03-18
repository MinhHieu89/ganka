import { useState } from "react"
import { useTranslation } from "react-i18next"
import {
  IconPrinter,
  IconArrowsSort,
  IconCheck,
  IconArrowUp,
  IconArrowDown,
  IconQuestionMark,
} from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import { useDiscrepancyReport } from "../api/optical-queries"
import type { StocktakingItemDto } from "../api/optical-api"
import { cn } from "@/shared/lib/utils"

interface DiscrepancyReportProps {
  sessionId: string
}

type SortKey = "barcode" | "frameName" | "physicalCount" | "systemCount" | "discrepancy"
type SortDir = "asc" | "desc"

type DiscrepancyCategory = "match" | "over" | "under" | "missing"

function getCategory(item: StocktakingItemDto): DiscrepancyCategory {
  if (!item.frameId) return "missing"
  if (item.discrepancy === 0) return "match"
  if (item.discrepancy > 0) return "over"
  return "under"
}

function getCategoryLabel(category: DiscrepancyCategory): string {
  // Labels will be overridden by translations in the component
  switch (category) {
    case "match": return "OK"
    case "over": return "Over"
    case "under": return "Under"
    case "missing": return "Missing"
  }
}

function getCategoryBadgeVariant(category: DiscrepancyCategory): "default" | "secondary" | "destructive" | "outline" {
  switch (category) {
    case "match": return "default"
    case "over": return "secondary"
    case "under": return "destructive"
    case "missing": return "outline"
  }
}

function getCategoryRowClass(category: DiscrepancyCategory): string {
  switch (category) {
    case "match": return "bg-green-50 dark:bg-green-950/20"
    case "over": return "bg-yellow-50 dark:bg-yellow-950/20"
    case "under": return "bg-red-50 dark:bg-red-950/20"
    case "missing": return "bg-gray-50 dark:bg-gray-950/20"
  }
}

export function DiscrepancyReport({ sessionId }: DiscrepancyReportProps) {
  const { t } = useTranslation("optical")
  const { data: report, isLoading, error } = useDiscrepancyReport(sessionId)
  const [sortKey, setSortKey] = useState<SortKey>("discrepancy")
  const [sortDir, setSortDir] = useState<SortDir>("desc")

  const handleSort = (key: SortKey) => {
    if (sortKey === key) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"))
    } else {
      setSortKey(key)
      setSortDir("desc")
    }
  }

  const handlePrint = () => {
    window.print()
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-24 rounded-lg" />
          ))}
        </div>
        <Skeleton className="h-64 rounded-lg" />
      </div>
    )
  }

  if (error || !report) {
    return (
      <Card>
        <CardContent className="py-12 text-center text-muted-foreground">
          <p>{t("common.noData")}</p>
        </CardContent>
      </Card>
    )
  }

  const items = report.items ?? []

  // Prefer backend-calculated summary counts, fall back to client calculation
  const totalScanned = report.totalScanned ?? items.length
  const overCount = report.overCount ?? items.filter((i) => getCategory(i) === "over").length
  const underCount = report.underCount ?? items.filter((i) => getCategory(i) === "under").length
  const missingCount = report.missingFromSystem ?? items.filter((i) => getCategory(i) === "missing").length
  const matches = totalScanned - overCount - underCount - missingCount

  // Sort items
  const sorted = [...items].sort((a, b) => {
    let av: string | number
    let bv: string | number

    switch (sortKey) {
      case "barcode":
        av = a.barcode
        bv = b.barcode
        break
      case "frameName":
        av = a.frameName ?? ""
        bv = b.frameName ?? ""
        break
      case "physicalCount":
        av = a.physicalCount
        bv = b.physicalCount
        break
      case "systemCount":
        av = a.systemCount
        bv = b.systemCount
        break
      case "discrepancy":
        av = Math.abs(a.discrepancy)
        bv = Math.abs(b.discrepancy)
        break
      default:
        return 0
    }

    if (av < bv) return sortDir === "asc" ? -1 : 1
    if (av > bv) return sortDir === "asc" ? 1 : -1
    return 0
  })

  const SortIcon = ({ columnKey }: { columnKey: SortKey }) => {
    if (sortKey !== columnKey) {
      return <IconArrowsSort className="ml-1 inline h-3.5 w-3.5 opacity-30" />
    }
    return sortDir === "asc" ? (
      <IconArrowUp className="ml-1 inline h-3.5 w-3.5" />
    ) : (
      <IconArrowDown className="ml-1 inline h-3.5 w-3.5" />
    )
  }

  return (
    <div className="space-y-4">
      {/* Summary Cards */}
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-5">
        <SummaryCard
          title={t("stocktaking.itemsScanned")}
          value={totalScanned}
          className="sm:col-span-1"
        />
        <SummaryCard
          title="OK"
          value={matches}
          icon={<IconCheck className="h-4 w-4 text-green-600" />}
          valueClass="text-green-600"
          className="bg-green-50 dark:bg-green-950/20"
        />
        <SummaryCard
          title={t("stocktaking.over")}
          value={overCount}
          icon={<IconArrowUp className="h-4 w-4 text-yellow-600" />}
          valueClass="text-yellow-600"
          className="bg-yellow-50 dark:bg-yellow-950/20"
        />
        <SummaryCard
          title={t("stocktaking.under")}
          value={underCount}
          icon={<IconArrowDown className="h-4 w-4 text-red-600" />}
          valueClass="text-red-600"
          className="bg-red-50 dark:bg-red-950/20"
        />
        <SummaryCard
          title={t("stocktaking.missing")}
          value={missingCount}
          icon={<IconQuestionMark className="h-4 w-4 text-muted-foreground" />}
          valueClass="text-muted-foreground"
          className="bg-gray-50 dark:bg-gray-950/20"
        />
      </div>

      {/* Report Table */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between pb-3">
          <CardTitle className="text-base">
            {t("stocktaking.discrepancyReport")}
            {report.sessionName && (
              <span className="text-muted-foreground ml-2 text-sm font-normal">
                — {report.sessionName}
              </span>
            )}
          </CardTitle>
          <Button variant="outline" size="sm" onClick={handlePrint}>
            <IconPrinter className="mr-1.5 h-4 w-4" />
            {t("stocktaking.exportReport")}
          </Button>
        </CardHeader>
        <CardContent className="p-0">
          {sorted.length === 0 ? (
            <div className="text-muted-foreground py-12 text-center text-sm">
              {t("stocktaking.empty")}
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead
                      className="cursor-pointer select-none whitespace-nowrap"
                      onClick={() => handleSort("barcode")}
                    >
                      {t("stocktaking.barcode")} <SortIcon columnKey="barcode" />
                    </TableHead>
                    <TableHead
                      className="cursor-pointer select-none"
                      onClick={() => handleSort("frameName")}
                    >
                      {t("stocktaking.frame")} <SortIcon columnKey="frameName" />
                    </TableHead>
                    <TableHead
                      className="cursor-pointer select-none text-right"
                      onClick={() => handleSort("physicalCount")}
                    >
                      {t("stocktaking.physicalCount")} <SortIcon columnKey="physicalCount" />
                    </TableHead>
                    <TableHead
                      className="cursor-pointer select-none text-right"
                      onClick={() => handleSort("systemCount")}
                    >
                      {t("stocktaking.systemCount")} <SortIcon columnKey="systemCount" />
                    </TableHead>
                    <TableHead
                      className="cursor-pointer select-none text-right"
                      onClick={() => handleSort("discrepancy")}
                    >
                      {t("stocktaking.discrepancy")} <SortIcon columnKey="discrepancy" />
                    </TableHead>
                    <TableHead>{t("stocktaking.status")}</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {sorted.map((item) => {
                    const category = getCategory(item)
                    return (
                      <TableRow
                        key={item.id}
                        className={getCategoryRowClass(category)}
                      >
                        <TableCell className="font-mono text-sm">
                          {item.barcode}
                        </TableCell>
                        <TableCell className="text-sm">
                          {item.frameName ?? (
                            <span className="text-muted-foreground italic">Unknown</span>
                          )}
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {item.physicalCount}
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {item.systemCount}
                        </TableCell>
                        <TableCell className="text-right">
                          <span
                            className={cn(
                              "font-medium",
                              item.discrepancy === 0 && "text-green-600",
                              item.discrepancy > 0 && "text-yellow-600",
                              item.discrepancy < 0 && "text-red-600",
                            )}
                          >
                            {item.discrepancy > 0 ? `+${item.discrepancy}` : item.discrepancy}
                          </span>
                        </TableCell>
                        <TableCell>
                          <Badge variant={getCategoryBadgeVariant(category)}>
                            {getCategoryLabel(category)}
                          </Badge>
                        </TableCell>
                      </TableRow>
                    )
                  })}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

interface SummaryCardProps {
  title: string
  value: number
  icon?: React.ReactNode
  valueClass?: string
  className?: string
}

function SummaryCard({ title, value, icon, valueClass, className }: SummaryCardProps) {
  return (
    <Card className={cn("", className)}>
      <CardContent className="p-4">
        <div className="flex items-start justify-between">
          <div>
            <p className="text-muted-foreground text-xs font-medium">{title}</p>
            <p className={cn("mt-1 text-2xl font-bold", valueClass)}>{value}</p>
          </div>
          {icon && <div className="mt-0.5">{icon}</div>}
        </div>
      </CardContent>
    </Card>
  )
}
