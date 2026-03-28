import { useState } from "react"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"
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
import { Label } from "@/shared/components/Label"
import { Textarea } from "@/shared/components/Textarea"
import { Avatar, AvatarFallback } from "@/shared/components/Avatar"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { useCancelAppointment } from "@/features/scheduling/api/scheduling-api"
import { receptionistKeys } from "@/features/receptionist/api/receptionist-api"
import { useQueryClient } from "@tanstack/react-query"
import type { ReceptionistDashboardRow } from "@/features/receptionist/types/receptionist.types"

interface CancelAppointmentDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  row: ReceptionistDashboardRow
}

const CANCEL_REASON_KEYS = [
  { value: "0", labelKey: "cancelDialog.reasons.patientRequest" },
  { value: "1", labelKey: "cancelDialog.reasons.changeClinic" },
  { value: "2", labelKey: "cancelDialog.reasons.wrongBooking" },
  { value: "3", labelKey: "cancelDialog.reasons.other" },
] as const

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/)
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

export function CancelAppointmentDialog({
  open,
  onOpenChange,
  row,
}: CancelAppointmentDialogProps) {
  const { t } = useTranslation("scheduling")
  const { t: tCommon } = useTranslation("common")
  const [reason, setReason] = useState("")
  const [note, setNote] = useState("")
  const queryClient = useQueryClient()
  const cancelAppointment = useCancelAppointment()

  const handleConfirm = () => {
    if (!row.appointmentId || !reason) return

    cancelAppointment.mutate(
      {
        appointmentId: row.appointmentId,
        cancellationReason: Number(reason),
        cancellationNote: note || undefined,
      },
      {
        onSuccess: () => {
          toast.success(t("cancelDialog.successToast", { name: row.patientName }))
          queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
          handleClose()
        },
        onError: () => {
          toast.error(t("cancelDialog.errorToast"))
        },
      },
    )
  }

  const handleClose = () => {
    setReason("")
    setNote("")
    onOpenChange(false)
  }

  const isOtherReason = reason === "3"

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold">{t("cancelDialog.title")}</DialogTitle>
          <DialogDescription className="sr-only">
            {t("cancelDialog.description")}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Patient avatar with cancel styling */}
          <div className="flex items-center gap-3">
            <Avatar className="h-12 w-12">
              <AvatarFallback
                className="text-sm font-semibold"
                style={{
                  backgroundColor: "var(--avatar-cancel-bg)",
                  color: "var(--avatar-cancel-text)",
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

          {/* Red warning */}
          <div
            className="flex items-start gap-2 rounded-md px-3 py-2.5 text-sm"
            style={{
              backgroundColor: "#FDE8E4",
              color: "#A32D2D",
            }}
          >
            <IconAlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
            <span>
              {t("cancelDialog.warning")}
            </span>
          </div>

          {/* Reason dropdown */}
          <div className="space-y-2">
            <Label>
              {t("cancelDialog.reasonLabel")} <span className="text-destructive">*</span>
            </Label>
            <Select value={reason} onValueChange={setReason}>
              <SelectTrigger>
                <SelectValue placeholder={t("cancelDialog.reasonPlaceholder")} />
              </SelectTrigger>
              <SelectContent>
                {CANCEL_REASON_KEYS.map((r) => (
                  <SelectItem key={r.value} value={r.value}>
                    {t(r.labelKey)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Note field when "Khác" selected */}
          {isOtherReason && (
            <div className="space-y-2">
              <Label>{t("cancelDialog.notes")}</Label>
              <Textarea
                value={note}
                onChange={(e) => setNote(e.target.value)}
                rows={2}
                className="resize-none"
              />
            </div>
          )}
        </div>

        <DialogFooter className="gap-2 sm:justify-end">
          <Button variant="ghost" onClick={handleClose}>
            {tCommon("buttons.cancel")}
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={!reason || cancelAppointment.isPending}
            style={{ backgroundColor: "#A32D2D", color: "white" }}
            className="hover:opacity-90"
          >
            {cancelAppointment.isPending
              ? tCommon("status.processing")
              : t("cancelDialog.confirm")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
