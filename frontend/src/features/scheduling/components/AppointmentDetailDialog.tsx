import { useState } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { format } from "date-fns"
import { vi, enUS } from "date-fns/locale"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { useCancelAppointment } from "@/features/scheduling/api/scheduling-api"
import {
  AppointmentStatus,
  CancellationReason,
} from "@/features/scheduling/hooks/useAppointments"
import { IconLoader2 } from "@tabler/icons-react"

interface AppointmentInfo {
  appointmentId: string
  patientName: string
  doctorName: string
  appointmentTypeName: string
  appointmentTypeNameVi: string
  status: number
  notes?: string | null
  start: Date
  end: Date
}

interface AppointmentDetailDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  appointment: AppointmentInfo | null
}

function getStatusVariant(
  status: number,
): "default" | "secondary" | "destructive" | "outline" {
  switch (status) {
    case AppointmentStatus.Confirmed:
      return "default"
    case AppointmentStatus.Pending:
      return "secondary"
    case AppointmentStatus.Cancelled:
      return "destructive"
    case AppointmentStatus.Completed:
      return "outline"
    default:
      return "secondary"
  }
}

function getStatusKey(status: number): string {
  switch (status) {
    case AppointmentStatus.Confirmed:
      return "confirmed"
    case AppointmentStatus.Pending:
      return "pending"
    case AppointmentStatus.Cancelled:
      return "cancelled"
    case AppointmentStatus.Completed:
      return "completed"
    default:
      return "pending"
  }
}

export function AppointmentDetailDialog({
  open,
  onOpenChange,
  appointment,
}: AppointmentDetailDialogProps) {
  const { t, i18n } = useTranslation("scheduling")
  const { t: tCommon } = useTranslation("common")
  const cancelMutation = useCancelAppointment()

  const [showCancelForm, setShowCancelForm] = useState(false)
  const [cancelReason, setCancelReason] = useState<string>("")
  const [cancelNote, setCancelNote] = useState("")

  const locale = i18n.language === "vi" ? vi : enUS

  if (!appointment) return null

  const handleCancel = () => {
    if (!cancelReason) return

    cancelMutation.mutate(
      {
        appointmentId: appointment.appointmentId,
        cancellationReason: parseInt(cancelReason, 10),
        cancellationNote: cancelNote || null,
      },
      {
        onSuccess: () => {
          toast.success(t("cancelAppointment"))
          setShowCancelForm(false)
          setCancelReason("")
          setCancelNote("")
          onOpenChange(false)
        },
        onError: () => {
          toast.error(t("cancelAppointment") + " failed")
        },
      },
    )
  }

  const canCancel =
    appointment.status === AppointmentStatus.Confirmed ||
    appointment.status === AppointmentStatus.Pending

  return (
    <Dialog
      open={open}
      onOpenChange={(o) => {
        if (!o) {
          setShowCancelForm(false)
          setCancelReason("")
          setCancelNote("")
        }
        onOpenChange(o)
      }}
    >
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("appointmentDetails")}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Status badge */}
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">{t("status.title")}:</span>
            <Badge variant={getStatusVariant(appointment.status)}>
              {t(`status.${getStatusKey(appointment.status)}`)}
            </Badge>
          </div>

          {/* Info grid */}
          <div className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-2 text-sm">
            <span className="text-muted-foreground">{t("patient")}:</span>
            <span className="font-medium">{appointment.patientName}</span>

            <span className="text-muted-foreground">{t("doctor")}:</span>
            <span className="font-medium">{appointment.doctorName}</span>

            <span className="text-muted-foreground">{t("appointmentType")}:</span>
            <span>{i18n.language === "vi" ? appointment.appointmentTypeNameVi : appointment.appointmentTypeName}</span>

            <span className="text-muted-foreground">{t("date")}:</span>
            <span>{format(appointment.start, "EEEE, dd/MM/yyyy", { locale })}</span>

            <span className="text-muted-foreground">{t("time")}:</span>
            <span>
              {format(appointment.start, "HH:mm")} - {format(appointment.end, "HH:mm")}
            </span>

            {appointment.notes && (
              <>
                <span className="text-muted-foreground">{t("notes")}:</span>
                <span>{appointment.notes}</span>
              </>
            )}
          </div>

          {/* Cancel form */}
          {showCancelForm && (
            <div className="space-y-3 border-t pt-3">
              <p className="text-sm font-medium">{t("cancellationReason.title")}</p>
              <Select value={cancelReason} onValueChange={setCancelReason}>
                <SelectTrigger>
                  <SelectValue placeholder={t("cancellationReason.title")} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={String(CancellationReason.PatientNoShow)}>
                    {t("cancellationReason.patientNoShow")}
                  </SelectItem>
                  <SelectItem value={String(CancellationReason.PatientRequest)}>
                    {t("cancellationReason.patientRequest")}
                  </SelectItem>
                  <SelectItem value={String(CancellationReason.DoctorUnavailable)}>
                    {t("cancellationReason.doctorUnavailable")}
                  </SelectItem>
                  <SelectItem value={String(CancellationReason.Other)}>
                    {t("cancellationReason.other")}
                  </SelectItem>
                </SelectContent>
              </Select>
              <AutoResizeTextarea
                value={cancelNote}
                onChange={(e) => setCancelNote(e.target.value)}
                className="min-h-[60px]"
              />
              <div className="flex gap-2 justify-end">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setShowCancelForm(false)}
                >
                  {tCommon("buttons.cancel")}
                </Button>
                <Button
                  variant="destructive"
                  size="sm"
                  disabled={!cancelReason || cancelMutation.isPending}
                  onClick={handleCancel}
                >
                  {cancelMutation.isPending && (
                    <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
                  )}
                  {t("cancelAppointment")}
                </Button>
              </div>
            </div>
          )}
        </div>

        {!showCancelForm && (
          <DialogFooter>
            {canCancel && (
              <Button
                variant="destructive"
                onClick={() => setShowCancelForm(true)}
              >
                {t("cancelAppointment")}
              </Button>
            )}
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              {tCommon("buttons.close")}
            </Button>
          </DialogFooter>
        )}
      </DialogContent>
    </Dialog>
  )
}
