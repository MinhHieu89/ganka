import { useState } from "react"
import { useTranslation } from "react-i18next"
import { Link } from "@tanstack/react-router"
import {
  IconClock,
  IconChevronDown,
  IconChevronUp,
  IconPlayerRecord,
} from "@tabler/icons-react"
import { differenceInDays, format } from "date-fns"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { useDueSoonSessions } from "@/features/treatment/api/treatment-api"
import type { TreatmentPackageDto } from "@/features/treatment/api/treatment-types"

const COLLAPSED_LIMIT = 5

export function DueSoonSection() {
  const { t } = useTranslation("treatment")
  const { data: packages, isLoading } = useDueSoonSessions()
  const [expanded, setExpanded] = useState(false)

  if (isLoading) {
    return (
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="flex items-center gap-2 text-base">
            <IconClock className="h-5 w-5 text-orange-500" />
            {t("dueSoon.title")}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-14 w-full" />
            ))}
          </div>
        </CardContent>
      </Card>
    )
  }

  if (!packages || packages.length === 0) {
    return (
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="flex items-center gap-2 text-base">
            <IconClock className="h-5 w-5 text-muted-foreground" />
            {t("dueSoon.title")}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground text-center py-4">
            {t("dueSoon.empty")}
          </p>
        </CardContent>
      </Card>
    )
  }

  const visibleItems = expanded
    ? packages
    : packages.slice(0, COLLAPSED_LIMIT)
  const hasMore = packages.length > COLLAPSED_LIMIT

  return (
    <Card className="border-orange-200 dark:border-orange-800/50 bg-orange-50/30 dark:bg-orange-950/10">
      <CardHeader className="pb-3">
        <CardTitle className="flex items-center gap-2 text-base">
          <IconClock className="h-5 w-5 text-orange-500" />
          {t("dueSoon.title")}
          <Badge variant="secondary" className="ml-1">
            {packages.length}
          </Badge>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-2">
          {visibleItems.map((pkg) => (
            <DueSoonItem key={pkg.id} pkg={pkg} />
          ))}
        </div>

        {hasMore && (
          <Button
            variant="ghost"
            size="sm"
            className="w-full mt-3"
            onClick={() => setExpanded(!expanded)}
          >
            {expanded ? (
              <>
                <IconChevronUp className="h-4 w-4 mr-1" />
                {t("dueSoon.showLess")}
              </>
            ) : (
              <>
                <IconChevronDown className="h-4 w-4 mr-1" />
                {t("dueSoon.showMore", { count: packages.length - COLLAPSED_LIMIT })}
              </>
            )}
          </Button>
        )}
      </CardContent>
    </Card>
  )
}

function DueSoonItem({ pkg }: { pkg: TreatmentPackageDto }) {
  const { t } = useTranslation("treatment")
  const daysSinceLast = pkg.lastSessionDate
    ? differenceInDays(new Date(), new Date(pkg.lastSessionDate))
    : null

  const isOverdue =
    pkg.nextDueDate != null && new Date(pkg.nextDueDate) < new Date()

  return (
    <div className="flex items-center justify-between p-3 rounded-lg border bg-background hover:bg-muted/50 transition-colors">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <Link
            to="/patients/$patientId"
            params={{ patientId: pkg.patientId }}
            className="font-medium text-sm hover:underline truncate"
            onClick={(e) => e.stopPropagation()}
          >
            {pkg.patientName}
          </Link>
          <TreatmentTypeBadge type={pkg.treatmentType} />
          {isOverdue && (
            <Badge variant="destructive" className="text-xs">
              {t("dueSoon.overdue")}
            </Badge>
          )}
        </div>
        <p className="text-xs text-muted-foreground mt-0.5">
          {daysSinceLast != null
            ? t("dueSoon.daysSinceLast", { days: daysSinceLast })
            : t("dueSoon.noSessions")}
          {" - "}
          {pkg.sessionsCompleted}/{pkg.totalSessions} {t("dueSoon.sessions")}
          {pkg.nextDueDate && (
            <>
              {" - "}
              {t("dueSoon.dueDate", { date: format(new Date(pkg.nextDueDate), "dd/MM/yyyy") })}
            </>
          )}
        </p>
      </div>
      <Link
        to="/treatments/$packageId"
        params={{ packageId: pkg.id }}
        onClick={(e) => e.stopPropagation()}
      >
        <Button variant="outline" size="sm">
          <IconPlayerRecord className="h-4 w-4 mr-1" />
          {t("dueSoon.record")}
        </Button>
      </Link>
    </div>
  )
}

function TreatmentTypeBadge({ type }: { type: string }) {
  const colorMap: Record<string, string> = {
    IPL: "border-violet-500 text-violet-700 dark:text-violet-400",
    LLLT: "border-blue-500 text-blue-700 dark:text-blue-400",
    LidCare: "border-emerald-500 text-emerald-700 dark:text-emerald-400",
  }

  return (
    <Badge
      variant="outline"
      className={`text-xs ${colorMap[type] ?? ""}`}
    >
      {type}
    </Badge>
  )
}
