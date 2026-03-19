import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { Link } from "@tanstack/react-router"
import { format } from "date-fns"
import {
  IconPlus,
  IconChevronDown,
  IconChevronRight,
  IconEye,
} from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import {
  Card,
  CardContent,
} from "@/shared/components/Card"
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/shared/components/Collapsible"
import { Skeleton } from "@/shared/components/Skeleton"
import { usePatientTreatments } from "@/features/treatment/api/treatment-api"
import { TreatmentPackageForm } from "./TreatmentPackageForm"
import type { TreatmentPackageDto } from "@/features/treatment/api/treatment-types"

// -- Status styles --

const STATUS_STYLES: Record<string, string> = {
  Active: "border-green-500 text-green-700 dark:text-green-400",
  Paused: "border-yellow-500 text-yellow-700 dark:text-yellow-400",
  PendingCancellation: "border-orange-500 text-orange-700 dark:text-orange-400",
  Completed: "border-blue-500 text-blue-700 dark:text-blue-400",
  Cancelled: "border-red-500 text-red-700 dark:text-red-400",
  Switched: "border-gray-500 text-gray-700 dark:text-gray-400",
}

const TREATMENT_TYPE_STYLES: Record<string, string> = {
  IPL: "border-violet-500 text-violet-700 dark:text-violet-400",
  LLLT: "border-blue-500 text-blue-700 dark:text-blue-400",
  LidCare: "border-emerald-500 text-emerald-700 dark:text-emerald-400",
}

// -- Props --

interface PatientTreatmentsTabProps {
  patientId: string
  patientName?: string
}

// -- Package card --

function PackageCard({ pkg }: { pkg: TreatmentPackageDto }) {
  const { t } = useTranslation("treatment")
  const percent =
    pkg.totalSessions > 0
      ? Math.round((pkg.sessionsCompleted / pkg.totalSessions) * 100)
      : 0
  const statusClassName = STATUS_STYLES[pkg.status] ?? ""
  const typeStyle = TREATMENT_TYPE_STYLES[pkg.treatmentType] ?? ""

  return (
    <Card className="transition-colors hover:bg-accent/50">
      <CardContent className="p-4">
        <div className="flex items-start justify-between gap-3">
          {/* Left content */}
          <div className="flex-1 space-y-3">
            {/* Type & template name */}
            <div className="flex items-center gap-2 flex-wrap">
              <Badge variant="outline" className={`text-xs ${typeStyle}`}>
                {t(`treatmentType.${pkg.treatmentType}`)}
              </Badge>
              <span className="text-sm font-medium">
                {pkg.protocolTemplateName}
              </span>
              <Badge
                variant="outline"
                className={`text-xs ${statusClassName}`}
              >
                {t(`status.${pkg.status}`)}
              </Badge>
            </div>

            {/* Progress bar */}
            <div className="flex items-center gap-3">
              <div className="flex-1 max-w-48 h-2 bg-muted rounded-full overflow-hidden">
                <div
                  className="h-full bg-primary rounded-full transition-all"
                  style={{ width: `${percent}%` }}
                />
              </div>
              <span className="text-xs text-muted-foreground whitespace-nowrap">
                {pkg.sessionsCompleted}/{pkg.totalSessions} {t("patientTab.sessions")}
              </span>
            </div>

            {/* Dates */}
            <div className="flex items-center gap-4 text-xs text-muted-foreground">
              {pkg.lastSessionDate && (
                <span>
                  {t("patientTab.lastSessionLabel")}:{" "}
                  {format(new Date(pkg.lastSessionDate), "dd/MM/yyyy")}
                </span>
              )}
              {pkg.nextDueDate && pkg.status === "Active" && (
                <span>
                  {t("patientTab.nextDueLabel")}:{" "}
                  {format(new Date(pkg.nextDueDate), "dd/MM/yyyy")}
                </span>
              )}
            </div>
          </div>

          {/* Right action */}
          <Link
            to="/treatments/$packageId"
            params={{ packageId: pkg.id }}
            className="shrink-0"
          >
            <Button variant="ghost" size="sm">
              <IconEye className="h-4 w-4 mr-1" />
              {t("patientTab.view")}
            </Button>
          </Link>
        </div>
      </CardContent>
    </Card>
  )
}

// -- Section --

function PackageSection({
  title,
  packages,
  defaultOpen = true,
}: {
  title: string
  packages: TreatmentPackageDto[]
  defaultOpen?: boolean
}) {
  const [open, setOpen] = useState(defaultOpen)

  if (packages.length === 0) return null

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger asChild>
        <button
          type="button"
          className="flex items-center gap-2 text-sm font-semibold text-muted-foreground hover:text-foreground transition-colors w-full text-left py-1"
        >
          {open ? (
            <IconChevronDown className="h-4 w-4" />
          ) : (
            <IconChevronRight className="h-4 w-4" />
          )}
          {title} ({packages.length})
        </button>
      </CollapsibleTrigger>
      <CollapsibleContent className="space-y-2 mt-2">
        {packages.map((pkg) => (
          <PackageCard key={pkg.id} pkg={pkg} />
        ))}
      </CollapsibleContent>
    </Collapsible>
  )
}

// -- Loading skeleton --

function PackageSkeleton() {
  return (
    <div className="space-y-4">
      {Array.from({ length: 3 }).map((_, i) => (
        <Card key={i}>
          <CardContent className="p-4">
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <Skeleton className="h-5 w-12" />
                <Skeleton className="h-5 w-32" />
                <Skeleton className="h-5 w-16" />
              </div>
              <Skeleton className="h-2 w-48" />
              <Skeleton className="h-4 w-40" />
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}

// -- Main component --

export function PatientTreatmentsTab({
  patientId,
  patientName,
}: PatientTreatmentsTabProps) {
  const { t } = useTranslation("treatment")
  const { data: packages = [], isLoading, isError } = usePatientTreatments(patientId)
  const [createDialogOpen, setCreateDialogOpen] = useState(false)

  // Group packages by status
  const { active, completed, other } = useMemo(() => {
    const activeStatuses = ["Active", "Paused", "PendingCancellation"]
    const completedStatuses = ["Completed"]

    return {
      active: packages.filter((p) => activeStatuses.includes(p.status)),
      completed: packages.filter((p) => completedStatuses.includes(p.status)),
      other: packages.filter(
        (p) =>
          !activeStatuses.includes(p.status) &&
          !completedStatuses.includes(p.status),
      ),
    }
  }, [packages])

  if (isError) {
    return (
      <div className="text-center py-12 text-destructive">
        {t("patientTab.loadError")}
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="flex justify-end">
          <Skeleton className="h-9 w-36" />
        </div>
        <PackageSkeleton />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header with create button */}
      <div className="flex items-center justify-end">
        <Button size="sm" onClick={() => setCreateDialogOpen(true)}>
          <IconPlus className="h-4 w-4 mr-1" />
          {t("patientTab.createProtocol")}
        </Button>
      </div>

      {/* Empty state */}
      {packages.length === 0 ? (
        <div className="text-center py-12 border rounded-lg bg-muted/30">
          <p className="text-muted-foreground mb-3">
            {t("patientTab.noProtocols")}
          </p>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setCreateDialogOpen(true)}
          >
            <IconPlus className="h-4 w-4 mr-1" />
            {t("patientTab.createFirst")}
          </Button>
        </div>
      ) : (
        <div className="space-y-6">
          {/* Active section (most prominent) */}
          <PackageSection
            title={t("patientTab.active")}
            packages={active}
            defaultOpen={true}
          />

          {/* Completed section */}
          <PackageSection
            title={t("patientTab.completed")}
            packages={completed}
            defaultOpen={true}
          />

          {/* Cancelled / Switched section (collapsed by default) */}
          <PackageSection
            title={t("patientTab.other")}
            packages={other}
            defaultOpen={false}
          />
        </div>
      )}

      {/* Create package dialog */}
      <TreatmentPackageForm
        open={createDialogOpen}
        onOpenChange={setCreateDialogOpen}
        patientId={patientId}
        patientName={patientName}
      />
    </div>
  )
}
