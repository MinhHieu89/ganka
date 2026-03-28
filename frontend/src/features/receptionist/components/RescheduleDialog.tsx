import { useState } from "react"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"
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
import { Calendar } from "@/shared/components/ui/calendar"
import { useAvailableSlots } from "@/features/receptionist/api/receptionist-api"
import { useRescheduleAppointment } from "@/features/scheduling/api/scheduling-api"
import { toLocalDateString } from "@/shared/lib/format-date"
import { TimeSlotGrid } from "./booking/TimeSlotGrid"
import type { ReceptionistDashboardRow } from "@/features/receptionist/types/receptionist.types"

interface RescheduleDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  row: ReceptionistDashboardRow
}

function formatDateTime(isoString: string | null, atLabel: string): string {
  if (!isoString) return ""
  const date = new Date(isoString)
  return `${date.toLocaleDateString("vi-VN")} ${atLabel} ${date.toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit", hour12: false })}`
}

export function RescheduleDialog({
  open,
  onOpenChange,
  row,
}: RescheduleDialogProps) {
  const { t } = useTranslation("scheduling")
  const { t: tCommon } = useTranslation("common")
  const [selectedDate, setSelectedDate] = useState<Date | undefined>(undefined)
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null)

  const dateStr = selectedDate ? toLocalDateString(selectedDate) : ""
  const slotsQuery = useAvailableSlots(dateStr)
  const reschedule = useRescheduleAppointment()

  const handleConfirm = () => {
    if (!row.appointmentId || !selectedSlot) return

    reschedule.mutate(
      {
        appointmentId: row.appointmentId,
        newStartTime: selectedSlot,
      },
      {
        onSuccess: () => {
          toast.success(t("rescheduleDialog.successToast", { name: row.patientName }))
          onOpenChange(false)
        },
        onError: (error) => {
          if (error.message === "DOUBLE_BOOKING") {
            toast.error(t("rescheduleDialog.doubleBooking"))
          } else {
            toast.error(t("rescheduleDialog.errorToast"))
          }
        },
      },
    )
  }

  const handleClose = () => {
    setSelectedDate(undefined)
    setSelectedSlot(null)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-3xl">
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold">
            {t("rescheduleDialog.title")}
          </DialogTitle>
          <DialogDescription className="sr-only">
            {t("rescheduleDialog.description")}
          </DialogDescription>
        </DialogHeader>

        {/* Old schedule with strikethrough */}
        {row.appointmentTime && (
          <div className="space-y-1">
            <Label>{t("rescheduleDialog.currentSchedule")}</Label>
            <div className="text-sm line-through text-muted-foreground">
              {formatDateTime(row.appointmentTime, t("rescheduleDialog.at"))}
            </div>
          </div>
        )}

        <div className="flex gap-8 py-2">
          {/* Calendar for new date */}
          <div className="space-y-4 shrink-0">
            <Label>{t("rescheduleDialog.selectNewDate")}</Label>
            <Calendar
              mode="single"
              selected={selectedDate}
              onSelect={(date) => {
                setSelectedDate(date ?? undefined)
                setSelectedSlot(null)
              }}
              disabled={(date) => date < new Date(new Date().setHours(0, 0, 0, 0))}
              className="rounded-md border"
            />
          </div>

          {/* Time slot grid */}
          {selectedDate && (
            <div className="space-y-4 min-w-0 flex-1">
              <Label>{t("rescheduleDialog.selectTime")}</Label>
              <TimeSlotGrid
                slots={slotsQuery.data ?? []}
                selectedSlot={selectedSlot}
                onSelectSlot={setSelectedSlot}
                isLoading={slotsQuery.isLoading}
              />
            </div>
          )}
        </div>

        <DialogFooter className="gap-2 sm:justify-end">
          <Button variant="ghost" onClick={handleClose}>
            {tCommon("buttons.cancel")}
          </Button>
          <Button
            variant="default"
            onClick={handleConfirm}
            disabled={!selectedSlot || reschedule.isPending}
          >
            {reschedule.isPending ? tCommon("status.processing") : t("rescheduleDialog.confirm")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
