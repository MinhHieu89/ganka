import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import { IconCircleCheck, IconAlertTriangle } from "@tabler/icons-react"
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
import type { ReceptionistDashboardRow } from "@/features/receptionist/types/receptionist.types"

interface CheckInIncompleteDialogProps {
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

export function CheckInIncompleteDialog({
  open,
  onOpenChange,
  row,
}: CheckInIncompleteDialogProps) {
  const { t } = useTranslation(["scheduling", "common"])
  const navigate = useNavigate()

  const notAvailable = t("scheduling:checkIn.notAvailable")

  const handleCheckInAndComplete = () => {
    onOpenChange(false)

    if (row.isGuestBooking) {
      const params = new URLSearchParams()
      if (row.patientName) params.set("guestName", row.patientName)
      if (row.appointmentId) params.set("appointmentId", row.appointmentId)
      navigate({ to: `/patients/intake?${params.toString()}` as string })
    } else if (row.patientId) {
      navigate({
        to: `/patients/intake?patientId=${row.patientId}` as string,
      })
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-xl font-semibold">
            <IconCircleCheck className="h-5 w-5 text-primary" />
            {t("scheduling:checkIn.incompleteTitle")}
          </DialogTitle>
          <DialogDescription className="sr-only">
            {t("scheduling:checkIn.incompleteDescription")}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Patient header: avatar + name/code + appointment time */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <Avatar className="h-12 w-12">
                <AvatarFallback
                  className="text-sm font-semibold"
                  style={{
                    backgroundColor: "var(--avatar-incomplete-bg)",
                    color: "var(--avatar-incomplete-text)",
                  }}
                >
                  {getInitials(row.patientName)}
                </AvatarFallback>
              </Avatar>
              <div>
                <div className="text-sm font-semibold">{row.patientName}</div>
                {row.patientCode && (
                  <div className="font-mono text-xs text-muted-foreground">
                    {row.patientCode}
                  </div>
                )}
              </div>
            </div>
            {row.appointmentTime && (
              <div className="text-right">
                <div className="text-xs text-muted-foreground">
                  {t("scheduling:checkIn.appointmentAt")}
                </div>
                <div className="text-sm font-semibold text-primary">
                  {formatAppointmentTime(row.appointmentTime)}
                </div>
              </div>
            )}
          </div>

          {/* Patient details card */}
          <div className="rounded-md border bg-muted/50 p-4 space-y-3 text-sm">
            {/* Row 1: Birth year + Gender */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <div className="text-xs text-muted-foreground">
                  {t("scheduling:checkIn.birthYear")}
                </div>
                {row.birthYear
                  ? <div className="font-semibold">{row.birthYear}</div>
                  : <div className="italic text-muted-foreground">{notAvailable}</div>}
              </div>
              <div>
                <div className="text-xs text-muted-foreground">
                  {t("scheduling:checkIn.gender")}
                </div>
                <div className="italic text-muted-foreground">{notAvailable}</div>
              </div>
            </div>

            {/* Row 2: Phone + Occupation */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <div className="text-xs text-muted-foreground">
                  {t("scheduling:checkIn.phone")}
                </div>
                {row.guestPhone
                  ? <div className="font-semibold">{row.guestPhone}</div>
                  : <div className="italic text-muted-foreground">{notAvailable}</div>}
              </div>
              <div>
                <div className="text-xs text-muted-foreground">
                  {t("scheduling:checkIn.occupation")}
                </div>
                <div className="italic text-muted-foreground">{notAvailable}</div>
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

          {/* Amber warning banner */}
          <div
            className="flex items-start gap-2 rounded-md px-3 py-2.5 text-sm"
            style={{
              backgroundColor: "#FAEEDA",
              color: "#633806",
            }}
          >
            <IconAlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
            <div>
              <div className="font-semibold">
                {t("scheduling:checkIn.incompleteWarning").split("--")[0].trim()}
              </div>
              <div>
                {t("scheduling:checkIn.incompleteWarning").split("--")[1]?.trim()}
              </div>
            </div>
          </div>
        </div>

        <DialogFooter className="gap-2 sm:justify-end">
          <Button variant="ghost" onClick={() => onOpenChange(false)}>
            {t("scheduling:checkIn.cancel")}
          </Button>
          <Button
            onClick={handleCheckInAndComplete}
            style={{ backgroundColor: "#534AB7", color: "white" }}
            className="hover:opacity-90"
          >
            {t("scheduling:checkIn.confirmAndComplete")} &rarr;
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
