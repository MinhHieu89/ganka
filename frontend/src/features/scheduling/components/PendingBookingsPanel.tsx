import { useState } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { format } from "date-fns"
import { vi, enUS } from "date-fns/locale"
import {
  usePendingSelfBookings,
  useApproveSelfBooking,
  useRejectSelfBooking,
  useAppointmentTypes,
  type SelfBookingRequestDto,
} from "@/features/scheduling/api/scheduling-api"
import { DoctorSelector, useDoctors } from "@/features/scheduling/components/DoctorSelector"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import {
  Field,
  FieldLabel,
} from "@/shared/components/Field"
import { DatePicker } from "@/shared/components/DatePicker"
import { Textarea } from "@/shared/components/Textarea"
import { Badge } from "@/shared/components/Badge"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Skeleton } from "@/shared/components/Skeleton"
import {
  IconCheck,
  IconX,
  IconPhone,
  IconMail,
  IconCalendar,
  IconLoader2,
  IconInbox,
} from "@tabler/icons-react"

/** Generate 30-minute time slots from 08:00 to 19:30 */
function generateTimeSlots(): string[] {
  const slots: string[] = []
  for (let h = 8; h < 20; h++) {
    slots.push(`${String(h).padStart(2, "0")}:00`)
    if (h < 20) {
      slots.push(`${String(h).padStart(2, "0")}:30`)
    }
  }
  return slots
}

export function PendingBookingsPanel() {
  const { t, i18n } = useTranslation("scheduling")
  const { t: tCommon } = useTranslation("common")
  const locale = i18n.language === "vi" ? vi : enUS

  const { data: pendingBookings, isLoading } = usePendingSelfBookings()
  const approveMutation = useApproveSelfBooking()
  const rejectMutation = useRejectSelfBooking()
  const { data: doctors } = useDoctors()

  // Approve dialog state
  const [approveTarget, setApproveTarget] = useState<SelfBookingRequestDto | null>(null)
  const [approveDoctorId, setApproveDoctorId] = useState("")
  const [approveDate, setApproveDate] = useState<Date | undefined>(undefined)
  const [approveTime, setApproveTime] = useState("")

  // Reject dialog state
  const [rejectTarget, setRejectTarget] = useState<SelfBookingRequestDto | null>(null)
  const [rejectReason, setRejectReason] = useState("")

  const handleApprove = () => {
    if (!approveTarget || !approveDoctorId || !approveDate || !approveTime) return

    const doctor = doctors?.find((d) => d.id === approveDoctorId)

    // Combine date + time into a single datetime
    const startDateTime = new Date(approveDate)
    const [h, m] = approveTime.split(":")
    startDateTime.setHours(parseInt(h, 10), parseInt(m, 10), 0, 0)

    approveMutation.mutate(
      {
        id: approveTarget.id,
        doctorId: approveDoctorId,
        doctorName: doctor?.fullName ?? "",
        patientName: approveTarget.patientName,
        startTime: startDateTime.toISOString(),
      },
      {
        onSuccess: () => {
          toast.success(t("approve"))
          setApproveTarget(null)
          setApproveDoctorId("")
          setApproveDate(undefined)
          setApproveTime("")
        },
        onError: (error) => {
          if (error.message === "DOUBLE_BOOKING") {
            toast.error(t("slotAlreadyBooked"))
          } else {
            toast.error(error.message)
          }
        },
      },
    )
  }

  const handleReject = () => {
    if (!rejectTarget || !rejectReason.trim()) return

    rejectMutation.mutate(
      {
        id: rejectTarget.id,
        reason: rejectReason,
      },
      {
        onSuccess: () => {
          toast.success(t("reject"))
          setRejectTarget(null)
          setRejectReason("")
        },
        onError: () => {
          toast.error(t("reject") + " failed")
        },
      },
    )
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="h-32 w-full" />
        ))}
      </div>
    )
  }

  if (!pendingBookings || pendingBookings.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <div className="flex size-16 items-center justify-center bg-primary/5 text-primary mb-4">
          <IconInbox className="h-8 w-8" />
        </div>
        <p className="text-sm text-muted-foreground">{t("noAppointments")}</p>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {pendingBookings.map((booking) => (
        <Card key={booking.id}>
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-base">{booking.patientName}</CardTitle>
              <Badge variant="secondary">{booking.referenceNumber}</Badge>
            </div>
          </CardHeader>
          <CardContent>
            <div className="grid gap-2 text-sm">
              <div className="flex items-center gap-4">
                <span className="flex items-center gap-1.5 text-muted-foreground">
                  <IconPhone className="h-3.5 w-3.5" />
                  {booking.phone}
                </span>
                {booking.email && (
                  <span className="flex items-center gap-1.5 text-muted-foreground">
                    <IconMail className="h-3.5 w-3.5" />
                    {booking.email}
                  </span>
                )}
              </div>
              <div className="flex items-center gap-4">
                <span className="flex items-center gap-1.5 text-muted-foreground">
                  <IconCalendar className="h-3.5 w-3.5" />
                  {format(new Date(booking.preferredDate), "EEEE, dd/MM/yyyy", { locale })}
                  {booking.preferredTimeSlot && ` - ${booking.preferredTimeSlot}`}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-muted-foreground">{t("appointmentType")}:</span>
                <span>{booking.appointmentTypeName}</span>
              </div>
              <div className="flex items-center gap-1 text-xs text-muted-foreground">
                {format(new Date(booking.createdAt), "dd/MM/yyyy HH:mm", { locale })}
              </div>
            </div>

            <div className="flex gap-2 mt-3 pt-3 border-t">
              <Button
                size="sm"
                onClick={() => {
                  setApproveTarget(booking)
                  setApproveDate(new Date(booking.preferredDate))
                  setApproveTime("14:00")
                }}
              >
                <IconCheck className="mr-1.5 h-3.5 w-3.5" />
                {t("approve")}
              </Button>
              <Button
                size="sm"
                variant="destructive"
                onClick={() => setRejectTarget(booking)}
              >
                <IconX className="mr-1.5 h-3.5 w-3.5" />
                {t("reject")}
              </Button>
            </div>
          </CardContent>
        </Card>
      ))}

      {/* Approve Dialog */}
      <Dialog
        open={!!approveTarget}
        onOpenChange={(o) => {
          if (!o) setApproveTarget(null)
        }}
      >
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>
              {t("approve")} - {approveTarget?.patientName}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <Field>
              <FieldLabel>{t("doctor")}</FieldLabel>
              <DoctorSelector
                value={approveDoctorId}
                onChange={setApproveDoctorId}
              />
            </Field>
            <Field>
              <FieldLabel>{t("date")}</FieldLabel>
              <DatePicker
                value={approveDate}
                onChange={setApproveDate}
                fromDate={new Date()}
                toDate={new Date(Date.now() + 90 * 24 * 60 * 60 * 1000)}
                className="w-full"
              />
            </Field>
            <Field>
              <FieldLabel>{t("time")}</FieldLabel>
              <Select value={approveTime} onValueChange={setApproveTime}>
                <SelectTrigger>
                  <SelectValue placeholder={t("selectTime")} />
                </SelectTrigger>
                <SelectContent>
                  {generateTimeSlots().map((slot) => (
                    <SelectItem key={slot} value={slot}>
                      {slot}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </Field>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setApproveTarget(null)}
            >
              {tCommon("buttons.cancel")}
            </Button>
            <Button
              onClick={handleApprove}
              disabled={!approveDoctorId || !approveDate || !approveTime || approveMutation.isPending}
            >
              {approveMutation.isPending && (
                <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              {t("approve")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog
        open={!!rejectTarget}
        onOpenChange={(o) => {
          if (!o) setRejectTarget(null)
        }}
      >
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>
              {t("reject")} - {rejectTarget?.patientName}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <Field>
              <FieldLabel>{t("cancellationReason.title")}</FieldLabel>
              <Textarea
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
              />
            </Field>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setRejectTarget(null)}
            >
              {tCommon("buttons.cancel")}
            </Button>
            <Button
              variant="destructive"
              onClick={handleReject}
              disabled={!rejectReason.trim() || rejectMutation.isPending}
            >
              {rejectMutation.isPending && (
                <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              {t("reject")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
