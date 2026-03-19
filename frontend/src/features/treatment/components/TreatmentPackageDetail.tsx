import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate } from "@tanstack/react-router"
import { format } from "date-fns"
import {
  IconArrowLeft,
  IconPlayerPlay,
  IconPlayerPause,
  IconHistory,
  IconAlertCircle,
} from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Skeleton } from "@/shared/components/Skeleton"
import { cn } from "@/shared/lib/utils"
import { useTreatmentPackage } from "../api/treatment-api"
import type { TreatmentPackageDto } from "../api/treatment-types"
import { TreatmentSessionCard } from "./TreatmentSessionCard"
import { OsdiTrendChart } from "./OsdiTrendChart"
import { TreatmentSessionForm } from "./TreatmentSessionForm"
import { ModifyPackageDialog } from "./ModifyPackageDialog"
import { CancellationRequestDialog } from "./CancellationRequestDialog"
import { SwitchTreatmentDialog } from "./SwitchTreatmentDialog"
import { VersionHistoryDialog } from "./VersionHistoryDialog"
import type { TreatmentType } from "../api/treatment-types"

// -- Status badge styling --

const STATUS_VARIANT: Record<
  string,
  "default" | "secondary" | "outline" | "destructive"
> = {
  Active: "default",
  Paused: "secondary",
  PendingCancellation: "outline",
  Cancelled: "destructive",
  Switched: "outline",
  Completed: "default",
}

const TREATMENT_TYPE_COLOR: Record<string, string> = {
  IPL: "bg-violet-100 text-violet-800 border-violet-300",
  LLLT: "bg-blue-100 text-blue-800 border-blue-300",
  LidCare: "bg-emerald-100 text-emerald-800 border-emerald-300",
}

// -- Progress bar --

function ProgressBar({
  completed,
  total,
}: {
  completed: number
  total: number
}) {
  const pct = total > 0 ? Math.round((completed / total) * 100) : 0
  return (
    <div className="flex items-center gap-3">
      <div className="flex-1 h-2.5 bg-muted rounded-full overflow-hidden">
        <div
          className={cn(
            "h-full rounded-full transition-all",
            pct === 100 ? "bg-green-500" : "bg-primary",
          )}
          style={{ width: `${pct}%` }}
        />
      </div>
      <span className="text-sm font-medium tabular-nums whitespace-nowrap">
        {completed}/{total}
      </span>
    </div>
  )
}

// -- Pricing display --

function PricingInfo({ pkg }: { pkg: TreatmentPackageDto }) {
  const { t } = useTranslation("treatment")
  if (pkg.pricingMode === "PerPackage") {
    return (
      <div className="text-sm">
        <span className="text-muted-foreground">{t("fields.packagePrice")}:</span>{" "}
        <span className="font-medium">
          {pkg.packagePrice.toLocaleString()} VND
        </span>
      </div>
    )
  }
  return (
    <div className="text-sm">
      <span className="text-muted-foreground">{t("fields.sessionPrice")}:</span>{" "}
      <span className="font-medium">
        {pkg.sessionPrice.toLocaleString()} VND
      </span>
    </div>
  )
}

// -- Cancellation info --

function CancellationInfo({ pkg }: { pkg: TreatmentPackageDto }) {
  const { t } = useTranslation("treatment")
  if (!pkg.cancellationRequest) return null
  const req = pkg.cancellationRequest
  return (
    <Card className="border-red-200 dark:border-red-800">
      <CardHeader className="pb-2">
        <CardTitle className="text-base flex items-center gap-2 text-red-700 dark:text-red-400">
          <IconAlertCircle className="h-4 w-4" />
          {t("detail.cancellationRequest")}
        </CardTitle>
      </CardHeader>
      <CardContent className="text-sm space-y-1">
        <div>
          <span className="text-muted-foreground">{t("detail.requestedBy")}:</span>{" "}
          {req.requestedByName}
        </div>
        <div>
          <span className="text-muted-foreground">{t("detail.date")}:</span>{" "}
          {format(new Date(req.requestedAt), "dd/MM/yyyy HH:mm")}
        </div>
        <div>
          <span className="text-muted-foreground">{t("fields.reason")}:</span> {req.reason}
        </div>
        <div>
          <span className="text-muted-foreground">{t("detail.status")}:</span>{" "}
          <Badge variant="outline">{t(`status.${req.status}`)}</Badge>
        </div>
        {req.deductionPercent > 0 && (
          <div>
            <span className="text-muted-foreground">{t("cancellation.deduction")}:</span>{" "}
            {req.deductionPercent}%
          </div>
        )}
        {req.refundAmount > 0 && (
          <div>
            <span className="text-muted-foreground">{t("fields.refundAmount")}:</span>{" "}
            {req.refundAmount.toLocaleString()} VND
          </div>
        )}
        {req.rejectionReason && (
          <div>
            <span className="text-muted-foreground">{t("fields.rejectionReason")}:</span>{" "}
            {req.rejectionReason}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

// -- Main component --

interface TreatmentPackageDetailProps {
  packageId: string
}

export function TreatmentPackageDetail({
  packageId,
}: TreatmentPackageDetailProps) {
  const { t } = useTranslation("treatment")
  const navigate = useNavigate()
  const { data: pkg, isLoading, error } = useTreatmentPackage(packageId)

  // Dialog state
  const [sessionFormOpen, setSessionFormOpen] = useState(false)
  const [modifyDialogOpen, setModifyDialogOpen] = useState(false)
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false)
  const [switchDialogOpen, setSwitchDialogOpen] = useState(false)
  const [historyDialogOpen, setHistoryDialogOpen] = useState(false)

  const goBack = () => navigate({ to: "/treatments" })

  const sortedSessions = useMemo(() => {
    if (!pkg?.sessions) return []
    return [...pkg.sessions].sort((a, b) => a.sessionNumber - b.sessionNumber)
  }, [pkg?.sessions])

  // -- Loading --
  if (isLoading) {
    return (
      <div className="space-y-4 p-4 max-w-5xl mx-auto">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-40 w-full" />
        <Skeleton className="h-64 w-full" />
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <Skeleton className="h-48" />
          <Skeleton className="h-48" />
        </div>
      </div>
    )
  }

  // -- Error / not found --
  if (error || !pkg) {
    return (
      <div className="p-4 space-y-4 max-w-5xl mx-auto">
        <Button variant="ghost" size="sm" onClick={goBack}>
          <IconArrowLeft className="h-4 w-4 mr-1" />
          {t("detail.back")}
        </Button>
        <div className="flex items-center justify-center h-64 text-muted-foreground">
          {t("detail.notFound")}
        </div>
      </div>
    )
  }

  const isActive = pkg.status === "Active"
  const isPaused = pkg.status === "Paused"
  const canModify = isActive || isPaused
  const isTerminal =
    pkg.status === "Completed" ||
    pkg.status === "Cancelled" ||
    pkg.status === "Switched"
  const showCancellation =
    pkg.status === "PendingCancellation" || pkg.status === "Cancelled"

  return (
    <div className="space-y-6 p-4 max-w-5xl mx-auto">
      {/* Back button */}
      <Button variant="ghost" size="sm" onClick={goBack}>
        <IconArrowLeft className="h-4 w-4 mr-1" />
        {t("detail.back")}
      </Button>

      {/* Header Card */}
      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
            <div className="flex items-center gap-3 flex-wrap">
              <CardTitle className="text-xl">
                {pkg.protocolTemplateName}
              </CardTitle>
              <Badge
                variant="outline"
                className={cn(
                  "border",
                  TREATMENT_TYPE_COLOR[pkg.treatmentType] ?? "",
                )}
              >
                {t(`treatmentType.${pkg.treatmentType}`)}
              </Badge>
              <Badge variant={STATUS_VARIANT[pkg.status] ?? "outline"}>
                {t(`status.${pkg.status}`)}
              </Badge>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Patient link */}
          <div className="text-sm">
            <span className="text-muted-foreground">{t("detail.patient")}:</span>{" "}
            <Link
              to="/patients/$patientId"
              params={{ patientId: pkg.patientId }}
              className="font-medium text-primary hover:underline"
            >
              {pkg.patientName}
            </Link>
          </div>

          {/* Progress */}
          <div>
            <div className="text-sm text-muted-foreground mb-1">
              {t("fields.progress")}
            </div>
            <ProgressBar
              completed={pkg.sessionsCompleted}
              total={pkg.totalSessions}
            />
          </div>

          {/* Info grid */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-sm">
            <PricingInfo pkg={pkg} />
            <div>
              <span className="text-muted-foreground">{t("detail.created")}:</span>{" "}
              <span className="font-medium">
                {format(new Date(pkg.createdAt), "dd/MM/yyyy")}
              </span>
            </div>
            {pkg.lastSessionDate && (
              <div>
                <span className="text-muted-foreground">{t("fields.lastSession")}:</span>{" "}
                <span className="font-medium">
                  {format(new Date(pkg.lastSessionDate), "dd/MM/yyyy")}
                </span>
              </div>
            )}
            {pkg.nextDueDate && (
              <div>
                <span className="text-muted-foreground">{t("fields.nextDue")}:</span>{" "}
                <span className="font-medium">
                  {format(new Date(pkg.nextDueDate), "dd/MM/yyyy")}
                </span>
              </div>
            )}
          </div>

          {/* Action buttons */}
          <div className="flex flex-wrap gap-2 pt-2 border-t">
            {isActive && (
              <Button size="sm" onClick={() => setSessionFormOpen(true)}>
                {t("recordSession")}
              </Button>
            )}
            {canModify && (
              <>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setModifyDialogOpen(true)}
                >
                  {t("modifyPackage")}
                </Button>
                <Button variant="outline" size="sm">
                  {isActive ? (
                    <>
                      <IconPlayerPause className="h-4 w-4 mr-1" />
                      {t("detail.pause")}
                    </>
                  ) : (
                    <>
                      <IconPlayerPlay className="h-4 w-4 mr-1" />
                      {t("detail.resume")}
                    </>
                  )}
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setSwitchDialogOpen(true)}
                >
                  {t("switchTreatment")}
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  className="text-red-600"
                  onClick={() => setCancelDialogOpen(true)}
                >
                  {t("requestCancellation")}
                </Button>
              </>
            )}
            {!isTerminal && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setHistoryDialogOpen(true)}
              >
                <IconHistory className="h-4 w-4 mr-1" />
                {t("versionHistory")}
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Cancellation info */}
      {showCancellation && <CancellationInfo pkg={pkg} />}

      {/* OSDI Trend Chart */}
      <OsdiTrendChart sessions={sortedSessions} />

      {/* Sessions grid */}
      {sortedSessions.length > 0 ? (
        <div>
          <h2 className="text-lg font-semibold mb-3">{t("sessionHistory")}</h2>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            {sortedSessions.map((session) => (
              <TreatmentSessionCard
                key={session.id}
                session={session}
                treatmentType={pkg.treatmentType}
              />
            ))}
          </div>
        </div>
      ) : (
        <Card>
          <CardContent className="py-8">
            <div className="text-center text-muted-foreground">
              {t("detail.noSessions")}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Dialogs */}
      <TreatmentSessionForm
        open={sessionFormOpen}
        onOpenChange={setSessionFormOpen}
        packageId={packageId}
        treatmentType={pkg.treatmentType as TreatmentType}
        defaultParametersJson={pkg.parametersJson}
      />

      <ModifyPackageDialog
        open={modifyDialogOpen}
        onOpenChange={setModifyDialogOpen}
        package_={pkg}
      />

      <CancellationRequestDialog
        open={cancelDialogOpen}
        onOpenChange={setCancelDialogOpen}
        package={pkg}
      />

      <SwitchTreatmentDialog
        open={switchDialogOpen}
        onOpenChange={setSwitchDialogOpen}
        currentPackage={pkg}
      />

      <VersionHistoryDialog
        open={historyDialogOpen}
        onOpenChange={setHistoryDialogOpen}
        packageId={pkg.id}
        packageName={pkg.protocolTemplateName}
      />
    </div>
  )
}
