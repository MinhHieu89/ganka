import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { Skeleton } from "@/shared/components/Skeleton"
import { IconChevronRight, IconArrowLeft } from "@tabler/icons-react"
import { Link } from "@tanstack/react-router"
import type { ActiveVisitDto } from "../api/clinical-api"
import { ALLOWED_REVERSALS } from "../api/clinical-api"

/** Stage number to i18n key mapping */
const STAGE_LABELS: Record<number, string> = {
  0: "workflow.stages.reception",
  1: "workflow.stages.refractionVa",
  2: "workflow.stages.doctorExam",
  3: "workflow.stages.diagnostics",
  4: "workflow.stages.doctorReads",
  5: "workflow.stages.rx",
  6: "workflow.stages.cashier",
  7: "workflow.stages.pharmacyOptical",
  8: "workflow.done",
}

const MAX_STAGE = 8

/** Status number to badge variant mapping */
const STATUS_VARIANT: Record<number, "outline" | "default" | "secondary" | "destructive"> = {
  0: "outline",    // Draft
  1: "default",    // Signed
  2: "secondary",  // Amended
  3: "destructive", // Cancelled
}

const STATUS_LABELS: Record<number, string> = {
  0: "visit.status.draft",
  1: "visit.status.signed",
  2: "visit.status.amended",
}

type SortKey = "patientName" | "doctorName" | "currentStage" | "waitMinutes" | "visitDate" | "status"
type SortDir = "asc" | "desc"

interface WorkflowTableViewProps {
  visits: ActiveVisitDto[] | undefined
  isLoading: boolean
  onAdvanceStage: (visitId: string, newStage: number) => void
  onReverseStage?: (visitId: string, currentStage: number, targetStage: number) => void
}

function formatWaitTime(minutes: number): string {
  if (minutes < 60) return `${minutes}m`
  const hours = Math.floor(minutes / 60)
  const mins = minutes % 60
  return `${hours}h${mins > 0 ? `${mins}m` : ""}`
}

function formatVisitTime(dateStr: string): string {
  const date = new Date(dateStr)
  return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
}

export function WorkflowTableView({
  visits,
  isLoading,
  onAdvanceStage,
  onReverseStage,
}: WorkflowTableViewProps) {
  const { t } = useTranslation("clinical")
  const navigate = useNavigate()

  const [sortKey, setSortKey] = useState<SortKey>("waitMinutes")
  const [sortDir, setSortDir] = useState<SortDir>("desc")
  const [stageFilter, setStageFilter] = useState<string>("all")

  const handleSort = (key: SortKey) => {
    if (sortKey === key) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"))
    } else {
      setSortKey(key)
      setSortDir("asc")
    }
  }

  const getAriaSortValue = (key: SortKey): "ascending" | "descending" | "none" => {
    if (sortKey !== key) return "none"
    return sortDir === "asc" ? "ascending" : "descending"
  }

  const filteredAndSorted = useMemo(() => {
    if (!visits) return []
    let result = [...visits]

    // Apply stage filter
    if (stageFilter !== "all") {
      const stage = parseInt(stageFilter, 10)
      result = result.filter((v) => v.currentStage === stage)
    }

    // Apply sorting
    result.sort((a, b) => {
      let cmp = 0
      switch (sortKey) {
        case "patientName":
          cmp = a.patientName.localeCompare(b.patientName)
          break
        case "doctorName":
          cmp = a.doctorName.localeCompare(b.doctorName)
          break
        case "currentStage":
          cmp = a.currentStage - b.currentStage
          break
        case "waitMinutes":
          cmp = a.waitMinutes - b.waitMinutes
          break
        case "visitDate":
          cmp = new Date(a.visitDate).getTime() - new Date(b.visitDate).getTime()
          break
        case "status":
          cmp = a.status - b.status
          break
      }
      return sortDir === "asc" ? cmp : -cmp
    })

    return result
  }, [visits, stageFilter, sortKey, sortDir])

  if (isLoading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    )
  }

  const handleRowClick = (visitId: string) => {
    navigate({ to: "/visits/$visitId" as string, params: { visitId } } as never)
  }

  return (
    <div className="space-y-4">
      {/* Filter bar */}
      <div className="flex items-center gap-3">
        <Select value={stageFilter} onValueChange={setStageFilter}>
          <SelectTrigger className="w-[200px]">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t("workflow.table.allStages")}</SelectItem>
            {Object.entries(STAGE_LABELS).map(([stage, labelKey]) => (
              <SelectItem key={stage} value={stage}>
                {t(labelKey)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead
                className="cursor-pointer select-none"
                onClick={() => handleSort("patientName")}
                aria-sort={getAriaSortValue("patientName")}
              >
                {t("workflow.table.patient")}
              </TableHead>
              <TableHead
                className="cursor-pointer select-none"
                onClick={() => handleSort("doctorName")}
                aria-sort={getAriaSortValue("doctorName")}
              >
                {t("workflow.table.doctor")}
              </TableHead>
              <TableHead
                className="cursor-pointer select-none"
                onClick={() => handleSort("currentStage")}
                aria-sort={getAriaSortValue("currentStage")}
              >
                {t("workflow.table.stage")}
              </TableHead>
              <TableHead
                className="cursor-pointer select-none"
                onClick={() => handleSort("waitMinutes")}
                aria-sort={getAriaSortValue("waitMinutes")}
              >
                {t("workflow.table.waitTime")}
              </TableHead>
              <TableHead
                className="cursor-pointer select-none"
                onClick={() => handleSort("visitDate")}
                aria-sort={getAriaSortValue("visitDate")}
              >
                {t("workflow.table.visitTime")}
              </TableHead>
              <TableHead
                className="cursor-pointer select-none"
                onClick={() => handleSort("status")}
                aria-sort={getAriaSortValue("status")}
              >
                {t("workflow.table.status")}
              </TableHead>
              <TableHead>{t("workflow.table.actions")}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredAndSorted.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="text-center py-12">
                  <div className="space-y-1">
                    <p className="text-sm font-medium text-muted-foreground">
                      {t("workflow.table.emptyState.title")}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {t("workflow.table.emptyState.description")}
                    </p>
                  </div>
                </TableCell>
              </TableRow>
            ) : (
              filteredAndSorted.map((visit) => {
                const canAdvance = visit.currentStage < MAX_STAGE && !visit.isCompleted
                return (
                  <TableRow
                    key={visit.id}
                    className="cursor-pointer hover:bg-muted/50"
                    onClick={() => handleRowClick(visit.id)}
                  >
                    <TableCell>
                      <Link
                        to="/patients/$patientId"
                        params={{ patientId: visit.patientId }}
                        className="text-primary hover:underline"
                        onClick={(e) => e.stopPropagation()}
                      >
                        {visit.patientName}
                      </Link>
                    </TableCell>
                    <TableCell>{visit.doctorName}</TableCell>
                    <TableCell>
                      <Badge variant="secondary" className="text-xs">
                        {t(STAGE_LABELS[visit.currentStage] ?? "workflow.stages.reception")}
                      </Badge>
                    </TableCell>
                    <TableCell>{formatWaitTime(visit.waitMinutes)}</TableCell>
                    <TableCell>{formatVisitTime(visit.visitDate)}</TableCell>
                    <TableCell>
                      <Badge variant={STATUS_VARIANT[visit.status] ?? "outline"} className="text-xs">
                        {t(STATUS_LABELS[visit.status] ?? "visit.status.draft")}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        {ALLOWED_REVERSALS[visit.currentStage] && !visit.isCompleted && (
                          ALLOWED_REVERSALS[visit.currentStage].length > 1 ? (
                            <Select
                              onValueChange={(value) => {
                                onReverseStage?.(
                                  visit.id,
                                  visit.currentStage,
                                  parseInt(value, 10),
                                )
                              }}
                            >
                              <SelectTrigger
                                className="h-8 w-8 p-0 border-0 bg-transparent [&>svg:last-child]:hidden"
                                onClick={(e) => e.stopPropagation()}
                              >
                                <IconArrowLeft className="h-4 w-4" />
                              </SelectTrigger>
                              <SelectContent>
                                {ALLOWED_REVERSALS[visit.currentStage].map(
                                  (targetStage) => (
                                    <SelectItem
                                      key={targetStage}
                                      value={targetStage.toString()}
                                    >
                                      {t(
                                        STAGE_LABELS[targetStage] ??
                                          "workflow.stages.reception",
                                      )}
                                    </SelectItem>
                                  ),
                                )}
                              </SelectContent>
                            </Select>
                          ) : (
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0"
                              onClick={(e) => {
                                e.stopPropagation()
                                onReverseStage?.(
                                  visit.id,
                                  visit.currentStage,
                                  ALLOWED_REVERSALS[visit.currentStage][0],
                                )
                              }}
                            >
                              <IconArrowLeft className="h-4 w-4" />
                            </Button>
                          )
                        )}
                        <Button
                          variant="ghost"
                          size="sm"
                          className="h-8 w-8 p-0"
                          disabled={!canAdvance}
                          onClick={(e) => {
                            e.stopPropagation()
                            if (canAdvance) {
                              onAdvanceStage(visit.id, visit.currentStage + 1)
                            }
                          }}
                          aria-label={t("card.advanceStage")}
                        >
                          <IconChevronRight className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                )
              })
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  )
}
