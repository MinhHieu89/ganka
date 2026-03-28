import { toast } from "sonner"
import { useTranslation } from "react-i18next"
import { IconCircleCheck, IconInfoCircle } from "@tabler/icons-react"
import { useNavigate } from "@tanstack/react-router"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Avatar, AvatarFallback } from "@/shared/components/Avatar"
import { Skeleton } from "@/shared/components/Skeleton"
import { useCheckInMutation } from "@/features/receptionist/api/receptionist-api"
import { usePatientById } from "@/features/patient/api/patient-api"
import { usePatientVisitHistory } from "@/features/clinical/api/clinical-api"
import type { ReceptionistDashboardRow } from "@/features/receptionist/types/receptionist.types"

interface CheckInDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  row: ReceptionistDashboardRow
}

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/)
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

function formatAppointmentTime(dateStr: string | null): string {
  if (!dateStr) return "—"
  try {
    return new Date(dateStr).toLocaleTimeString("vi-VN", {
      hour: "2-digit",
      minute: "2-digit",
      hour12: false,
    })
  } catch {
    return "—"
  }
}

function formatGender(gender: string | null, t: (key: string) => string): string | null {
  if (!gender) return null
  const map: Record<string, string> = {
    Male: t("patient:male"),
    Female: t("patient:female"),
    Other: t("patient:other"),
  }
  return map[gender] ?? null
}

function formatVisitDate(dateStr: string): string {
  try {
    return new Date(dateStr).toLocaleDateString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    })
  } catch {
    return dateStr
  }
}

export function CheckInDialog({ open, onOpenChange, row }: CheckInDialogProps) {
  const { t } = useTranslation(["scheduling", "patient", "common"])
  const checkIn = useCheckInMutation()
  const navigate = useNavigate()

  const patientQuery = usePatientById(row.patientId ?? undefined)
  const visitHistoryQuery = usePatientVisitHistory(row.patientId ?? undefined)

  const patient = patientQuery.data
  const lastVisit = visitHistoryQuery.data?.[0] ?? null

  const birthYear = patient?.dateOfBirth
    ? new Date(patient.dateOfBirth).getFullYear()
    : row.birthYear

  const handleConfirm = () => {
    if (!row.appointmentId) return

    checkIn.mutate(row.appointmentId, {
      onSuccess: () => {
        toast.success(t("scheduling:checkIn.successToast", { name: row.patientName }))
        onOpenChange(false)
      },
      onError: (err: Error) => {
        toast.error(err.message || t("scheduling:checkIn.errorGeneric"))
      },
    })
  }

  const handleEditInfo = () => {
    onOpenChange(false)
    navigate({
      to: "/patients/intake" as string,
      search: { patientId: row.patientId } as never,
    })
  }

  const isLoading = patientQuery.isLoading || visitHistoryQuery.isLoading

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-xl font-semibold">
            <IconCircleCheck className="h-5 w-5 text-primary" />
            {t("scheduling:checkIn.title")}
          </DialogTitle>
          <DialogDescription className="sr-only">
            {t("scheduling:checkIn.description")}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Patient header: avatar + name + appointment time */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <Avatar className="h-12 w-12">
                <AvatarFallback
                  className="text-sm font-semibold"
                  style={{
                    backgroundColor: "var(--avatar-complete-bg)",
                    color: "var(--avatar-complete-text)",
                  }}
                >
                  {getInitials(row.patientName)}
                </AvatarFallback>
              </Avatar>
              <div>
                <div className="text-sm font-semibold">{row.patientName}</div>
                {(patient?.patientCode ?? row.patientCode) && (
                  <div className="font-mono text-xs text-muted-foreground">
                    {patient?.patientCode ?? row.patientCode}
                  </div>
                )}
              </div>
            </div>
            <div className="text-right">
              <div className="text-xs text-muted-foreground">
                {t("scheduling:checkIn.appointmentAt")}
              </div>
              <div className="text-sm font-semibold text-primary">
                {formatAppointmentTime(row.appointmentTime)}
              </div>
            </div>
          </div>

          {/* Patient details card */}
          {isLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-24 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
          ) : (
            <>
              <div className="rounded-md border bg-muted/50 p-4 space-y-3 text-sm">
                {/* Row 1: Birth year + Gender */}
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <div className="text-xs text-muted-foreground">
                      {t("scheduling:checkIn.birthYear")}
                    </div>
                    {birthYear
                      ? <div className="font-semibold">{birthYear}</div>
                      : <div className="italic text-muted-foreground">{t("scheduling:checkIn.notAvailable")}</div>}
                  </div>
                  <div>
                    <div className="text-xs text-muted-foreground">
                      {t("scheduling:checkIn.gender")}
                    </div>
                    {formatGender(patient?.gender ?? null, t)
                      ? <div className="font-semibold">{formatGender(patient?.gender ?? null, t)}</div>
                      : <div className="italic text-muted-foreground">{t("scheduling:checkIn.notAvailable")}</div>}
                  </div>
                </div>

                {/* Row 2: Phone + Occupation */}
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <div className="text-xs text-muted-foreground">
                      {t("scheduling:checkIn.phone")}
                    </div>
                    {patient?.phone
                      ? <div className="font-semibold">{patient.phone}</div>
                      : <div className="italic text-muted-foreground">{t("scheduling:checkIn.notAvailable")}</div>}
                  </div>
                  <div>
                    <div className="text-xs text-muted-foreground">
                      {t("scheduling:checkIn.occupation")}
                    </div>
                    <div className="italic text-muted-foreground">{t("scheduling:checkIn.notAvailable")}</div>
                  </div>
                </div>
              </div>

              {/* Reason for visit */}
              {row.reason && (
                <div className="rounded-md border bg-muted/50 p-4 text-sm">
                  <div className="text-xs text-muted-foreground">
                    {t("scheduling:checkIn.reason")}
                  </div>
                  <div className="font-semibold">{row.reason}</div>
                </div>
              )}

              {/* Last visit */}
              {lastVisit && (
                <div className="rounded-md border bg-muted/50 p-4 text-sm">
                  <div className="text-xs text-muted-foreground">
                    {t("scheduling:checkIn.lastVisit")}
                  </div>
                  <div className="font-semibold">
                    {formatVisitDate(lastVisit.visitDate)}
                    {lastVisit.primaryDiagnosisText &&
                      ` — ${lastVisit.primaryDiagnosisText}`}
                    {lastVisit.doctorName && ` — ${lastVisit.doctorName}`}
                  </div>
                </div>
              )}
            </>
          )}

          {/* Info note */}
          <div className="flex items-start gap-2 rounded-md border border-blue-200 bg-blue-50 px-3 py-2.5 text-sm text-blue-800 dark:border-blue-800 dark:bg-blue-950 dark:text-blue-200">
            <IconInfoCircle className="mt-0.5 h-4 w-4 shrink-0" />
            <span>{t("scheduling:checkIn.infoNote")}</span>
          </div>
        </div>

        <DialogFooter className="flex-row justify-between sm:justify-between">
          <Button variant="outline" onClick={handleEditInfo}>
            {t("scheduling:checkIn.editInfo")}
          </Button>
          <div className="flex gap-2">
            <Button variant="ghost" onClick={() => onOpenChange(false)}>
              {t("scheduling:checkIn.cancel")}
            </Button>
            <Button
              onClick={handleConfirm}
              disabled={checkIn.isPending || isLoading}
            >
              {checkIn.isPending
                ? t("common:status.processing")
                : t("scheduling:checkIn.confirm")}
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
