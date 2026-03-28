import { useState } from "react"
import { toast } from "sonner"
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
import { Label } from "@/shared/components/Label"
import { Textarea } from "@/shared/components/Textarea"
import { Checkbox } from "@/shared/components/Checkbox"
import { Avatar, AvatarFallback } from "@/shared/components/Avatar"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { useCancelVisitMutation } from "@/features/receptionist/api/receptionist-api"
import type { ReceptionistDashboardRow } from "@/features/receptionist/types/receptionist.types"

interface CancelVisitDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  row: ReceptionistDashboardRow
}

const CANCEL_VISIT_REASONS = [
  { value: "BN không muốn chờ, bỏ về", labelKey: "cancelVisit.reasons.leftWithoutWaiting" },
  { value: "BN muốn đổi sang ngày khác", labelKey: "cancelVisit.reasons.changeDateRequest" },
  { value: "Lễ tân check-in nhầm người", labelKey: "cancelVisit.reasons.wrongCheckIn" },
  { value: "BN chuyển sang phòng khám khác", labelKey: "cancelVisit.reasons.changeClinic" },
  { value: "Khác", labelKey: "cancelVisit.reasons.other" },
] as const

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/)
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

export function CancelVisitDialog({
  open,
  onOpenChange,
  row,
}: CancelVisitDialogProps) {
  const { t } = useTranslation("receptionist")
  const { t: tCommon } = useTranslation("common")
  const [reason, setReason] = useState("")
  const [note, setNote] = useState("")
  const [rebook, setRebook] = useState(false)
  const navigate = useNavigate()
  const cancelVisit = useCancelVisitMutation()

  const handleConfirm = () => {
    if (!row.visitId || !reason) return

    const fullReason = reason === "Khác" && note ? `${reason}: ${note}` : reason

    cancelVisit.mutate(
      {
        visitId: row.visitId,
        reason: fullReason,
      },
      {
        onSuccess: () => {
          toast.success(t("cancelVisit.successToast", { name: row.patientName }))
          handleClose()

          if (rebook && row.patientId) {
            navigate({
              to: `/appointments/new?patientId=${row.patientId}` as string,
            })
          }
        },
        onError: (error) => {
          if (
            error.message.includes("409") ||
            error.message.includes("conflict")
          ) {
            toast.error(t("cancelVisit.alreadyExamining"))
          } else {
            toast.error(t("cancelVisit.errorToast"))
          }
        },
      },
    )
  }

  const handleClose = () => {
    setReason("")
    setNote("")
    setRebook(false)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold">
            {t("cancelVisit.title")}
          </DialogTitle>
          <DialogDescription className="sr-only">
            {t("cancelVisit.description")}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Patient avatar with cancel styling (red) */}
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
              {t("cancelVisit.warning")}
            </span>
          </div>

          {/* Reason dropdown */}
          <div className="space-y-2">
            <Label>
              {t("cancelVisit.reasonLabel")} <span className="text-destructive">*</span>
            </Label>
            <Select value={reason} onValueChange={setReason}>
              <SelectTrigger>
                <SelectValue placeholder={t("cancelVisit.reasonPlaceholder")} />
              </SelectTrigger>
              <SelectContent>
                {CANCEL_VISIT_REASONS.map((r) => (
                  <SelectItem key={r.value} value={r.value}>
                    {t(r.labelKey)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Note field when "Khác" selected */}
          {reason === "Khác" && (
            <div className="space-y-2">
              <Label>{t("cancelVisit.notes")}</Label>
              <Textarea
                value={note}
                onChange={(e) => setNote(e.target.value)}
                rows={2}
                className="resize-none"
              />
            </div>
          )}

          {/* Rebook checkbox */}
          <div className="flex items-center gap-2">
            <Checkbox
              id="rebook-cancel-visit"
              checked={rebook}
              onCheckedChange={(checked) => setRebook(checked === true)}
            />
            <Label htmlFor="rebook-cancel-visit" className="cursor-pointer">
              {t("cancelVisit.rebook")}
            </Label>
          </div>
        </div>

        <DialogFooter className="gap-2 sm:justify-end">
          <Button variant="ghost" onClick={handleClose}>
            {tCommon("buttons.cancel")}
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={!reason || cancelVisit.isPending}
            style={{ backgroundColor: "#A32D2D", color: "white" }}
            className="hover:opacity-90"
          >
            {cancelVisit.isPending
              ? tCommon("status.processing")
              : t("cancelVisit.confirm")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
