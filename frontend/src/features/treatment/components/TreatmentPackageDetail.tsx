import { useMemo } from "react"
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
  if (pkg.pricingMode === "PerPackage") {
    return (
      <div className="text-sm">
        <span className="text-muted-foreground">Package price:</span>{" "}
        <span className="font-medium">
          {pkg.packagePrice.toLocaleString()} VND
        </span>
      </div>
    )
  }
  return (
    <div className="text-sm">
      <span className="text-muted-foreground">Per session:</span>{" "}
      <span className="font-medium">
        {pkg.sessionPrice.toLocaleString()} VND
      </span>
    </div>
  )
}

// -- Cancellation info --

function CancellationInfo({ pkg }: { pkg: TreatmentPackageDto }) {
  if (!pkg.cancellationRequest) return null
  const req = pkg.cancellationRequest
  return (
    <Card className="border-red-200 dark:border-red-800">
      <CardHeader className="pb-2">
        <CardTitle className="text-base flex items-center gap-2 text-red-700 dark:text-red-400">
          <IconAlertCircle className="h-4 w-4" />
          Cancellation Request
        </CardTitle>
      </CardHeader>
      <CardContent className="text-sm space-y-1">
        <div>
          <span className="text-muted-foreground">Requested by:</span>{" "}
          {req.requestedByName}
        </div>
        <div>
          <span className="text-muted-foreground">Date:</span>{" "}
          {format(new Date(req.requestedAt), "dd/MM/yyyy HH:mm")}
        </div>
        <div>
          <span className="text-muted-foreground">Reason:</span> {req.reason}
        </div>
        <div>
          <span className="text-muted-foreground">Status:</span>{" "}
          <Badge variant="outline">{req.status}</Badge>
        </div>
        {req.deductionPercent > 0 && (
          <div>
            <span className="text-muted-foreground">Deduction:</span>{" "}
            {req.deductionPercent}%
          </div>
        )}
        {req.refundAmount > 0 && (
          <div>
            <span className="text-muted-foreground">Refund:</span>{" "}
            {req.refundAmount.toLocaleString()} VND
          </div>
        )}
        {req.rejectionReason && (
          <div>
            <span className="text-muted-foreground">Rejection:</span>{" "}
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
  const navigate = useNavigate()
  const { data: pkg, isLoading, error } = useTreatmentPackage(packageId)

  const goBack = () => navigate({ to: "/dashboard" })

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
          Back
        </Button>
        <div className="flex items-center justify-center h-64 text-muted-foreground">
          Treatment package not found
        </div>
      </div>
    )
  }

  const isActive = pkg.status === "Active"
  const isPaused = pkg.status === "Paused"
  const canModify = isActive || isPaused
  const showCancellation =
    pkg.status === "PendingCancellation" || pkg.status === "Cancelled"

  return (
    <div className="space-y-6 p-4 max-w-5xl mx-auto">
      {/* Back button */}
      <Button variant="ghost" size="sm" onClick={goBack}>
        <IconArrowLeft className="h-4 w-4 mr-1" />
        Back
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
                {pkg.treatmentType}
              </Badge>
              <Badge variant={STATUS_VARIANT[pkg.status] ?? "outline"}>
                {pkg.status}
              </Badge>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Patient link */}
          <div className="text-sm">
            <span className="text-muted-foreground">Patient:</span>{" "}
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
              Sessions Progress
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
              <span className="text-muted-foreground">Created:</span>{" "}
              <span className="font-medium">
                {format(new Date(pkg.createdAt), "dd/MM/yyyy")}
              </span>
            </div>
            {pkg.lastSessionDate && (
              <div>
                <span className="text-muted-foreground">Last session:</span>{" "}
                <span className="font-medium">
                  {format(new Date(pkg.lastSessionDate), "dd/MM/yyyy")}
                </span>
              </div>
            )}
            {pkg.nextDueDate && (
              <div>
                <span className="text-muted-foreground">Next due:</span>{" "}
                <span className="font-medium">
                  {format(new Date(pkg.nextDueDate), "dd/MM/yyyy")}
                </span>
              </div>
            )}
          </div>

          {/* Action buttons */}
          <div className="flex flex-wrap gap-2 pt-2 border-t">
            {isActive && (
              <Button size="sm">
                Record Session
              </Button>
            )}
            {canModify && (
              <>
                <Button variant="outline" size="sm">
                  Modify
                </Button>
                <Button variant="outline" size="sm">
                  {isActive ? (
                    <>
                      <IconPlayerPause className="h-4 w-4 mr-1" />
                      Pause
                    </>
                  ) : (
                    <>
                      <IconPlayerPlay className="h-4 w-4 mr-1" />
                      Resume
                    </>
                  )}
                </Button>
                <Button variant="outline" size="sm">
                  Switch Type
                </Button>
                <Button variant="outline" size="sm" className="text-red-600">
                  Request Cancellation
                </Button>
              </>
            )}
            <Button variant="ghost" size="sm">
              <IconHistory className="h-4 w-4 mr-1" />
              View History
            </Button>
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
          <h2 className="text-lg font-semibold mb-3">Sessions</h2>
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
              No sessions recorded yet
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
