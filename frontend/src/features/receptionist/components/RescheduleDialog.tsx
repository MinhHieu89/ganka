import { useState } from "react"
import { toast } from "sonner"
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
import { TimeSlotGrid } from "./booking/TimeSlotGrid"
import type { ReceptionistDashboardRow } from "@/features/receptionist/types/receptionist.types"

interface RescheduleDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  row: ReceptionistDashboardRow
}

function formatDateTime(isoString: string | null): string {
  if (!isoString) return ""
  const date = new Date(isoString)
  return `${date.toLocaleDateString("vi-VN")} luc ${date.toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit", hour12: false })}`
}

function formatDateToISO(date: Date): string {
  return date.toISOString().split("T")[0]
}

export function RescheduleDialog({
  open,
  onOpenChange,
  row,
}: RescheduleDialogProps) {
  const [selectedDate, setSelectedDate] = useState<Date | undefined>(undefined)
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null)

  const dateStr = selectedDate ? formatDateToISO(selectedDate) : ""
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
          toast.success(`Da doi lich hen cho ${row.patientName}`)
          onOpenChange(false)
        },
        onError: (error) => {
          if (error.message === "DOUBLE_BOOKING") {
            toast.error("Lich da bi trung. Vui long chon slot khac.")
          } else {
            toast.error("Khong the doi lich. Vui long thu lai.")
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
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold">
            Doi lich hen
          </DialogTitle>
          <DialogDescription className="sr-only">
            Chon ngay va gio moi cho lich hen
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Old schedule with strikethrough */}
          {row.appointmentTime && (
            <div className="space-y-1">
              <Label className="text-muted-foreground">Lich hen hien tai</Label>
              <div className="text-sm line-through text-muted-foreground">
                {formatDateTime(row.appointmentTime)}
              </div>
            </div>
          )}

          {/* Calendar for new date */}
          <div className="space-y-2">
            <Label>Chon ngay moi</Label>
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
            <div className="space-y-2">
              <Label>Chon gio</Label>
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
            Huy
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={!selectedSlot || reschedule.isPending}
            style={{ backgroundColor: "#534AB7", color: "white" }}
            className="hover:opacity-90"
          >
            {reschedule.isPending ? "Dang xu ly..." : "Xac nhan doi lich"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
