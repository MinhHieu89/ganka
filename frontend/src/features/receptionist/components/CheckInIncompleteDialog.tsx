import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import { IconAlertTriangle } from "@tabler/icons-react"
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

export function CheckInIncompleteDialog({
  open,
  onOpenChange,
  row,
}: CheckInIncompleteDialogProps) {
  const { t } = useTranslation("scheduling")
  const { t: tCommon } = useTranslation("common")
  const { t: tPatient } = useTranslation("patient")
  const navigate = useNavigate()

  const handleCheckInAndComplete = () => {
    onOpenChange(false)

    if (row.isGuestBooking) {
      // Guest booking: navigate to intake form with guest info pre-filled
      const params = new URLSearchParams()
      if (row.patientName) params.set("guestName", row.patientName)
      // Phone is not directly on the row; intake form will handle from appointment
      if (row.appointmentId) params.set("appointmentId", row.appointmentId)
      navigate({ to: `/patients/intake?${params.toString()}` as string })
    } else if (row.patientId) {
      // Existing patient with incomplete info: navigate to intake with patientId
      navigate({
        to: `/patients/intake?patientId=${row.patientId}` as string,
      })
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold">
            {t("checkIn.incompleteTitle")}
          </DialogTitle>
          <DialogDescription className="sr-only">
            {t("checkIn.incompleteDescription")}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Patient avatar with incomplete styling */}
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

          {/* Amber warning banner */}
          <div
            className="flex items-start gap-2 rounded-md px-3 py-2.5 text-sm"
            style={{
              backgroundColor: "#FAEEDA",
              color: "#633806",
            }}
          >
            <IconAlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
            <span>
              {t("checkIn.incompleteWarning")}
            </span>
          </div>

          {/* Patient info (what we have) */}
          <div className="rounded-md border bg-muted/50 p-4 space-y-2 text-sm">
            <div className="flex justify-between">
              <span className="text-muted-foreground">{tPatient("fullName")}</span>
              <span className="font-medium">{row.patientName}</span>
            </div>
            {row.patientCode && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">{tPatient("patientCode")}</span>
                <span className="font-mono">{row.patientCode}</span>
              </div>
            )}
          </div>
        </div>

        <DialogFooter className="gap-2 sm:justify-end">
          <Button variant="ghost" onClick={() => onOpenChange(false)}>
            {tCommon("buttons.back")}
          </Button>
          <Button
            onClick={handleCheckInAndComplete}
            style={{ backgroundColor: "#534AB7", color: "white" }}
            className="hover:opacity-90"
          >
            {t("checkIn.confirmAndComplete")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
