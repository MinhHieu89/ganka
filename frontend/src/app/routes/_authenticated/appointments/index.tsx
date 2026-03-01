import { useState, useCallback } from "react"
import { createFileRoute } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import type { DateSelectArg, EventClickArg } from "@fullcalendar/core"
import { IconPlus, IconCalendar } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/Tabs"
import { AppointmentCalendar } from "@/features/scheduling/components/AppointmentCalendar"
import { AppointmentBookingDialog } from "@/features/scheduling/components/AppointmentBookingDialog"
import { AppointmentDetailDialog } from "@/features/scheduling/components/AppointmentDetailDialog"
import { DoctorSelector } from "@/features/scheduling/components/DoctorSelector"
import { PendingBookingsPanel } from "@/features/scheduling/components/PendingBookingsPanel"
import { Badge } from "@/shared/components/Badge"
import { usePendingSelfBookings } from "@/features/scheduling/api/scheduling-api"

export const Route = createFileRoute("/_authenticated/appointments/")({
  component: AppointmentsPage,
})

interface AppointmentInfo {
  appointmentId: string
  patientName: string
  doctorName: string
  appointmentTypeName: string
  status: number
  notes?: string | null
  start: Date
  end: Date
}

function AppointmentsPage() {
  const { t } = useTranslation("scheduling")

  const [selectedDoctorId, setSelectedDoctorId] = useState<string>()
  const [bookingDialogOpen, setBookingDialogOpen] = useState(false)
  const [detailDialogOpen, setDetailDialogOpen] = useState(false)
  const [selectedSlotStart, setSelectedSlotStart] = useState<Date>()
  const [selectedAppointment, setSelectedAppointment] = useState<AppointmentInfo | null>(null)

  const { data: pendingBookings } = usePendingSelfBookings()
  const pendingCount = pendingBookings?.length ?? 0

  const handleSlotClick = useCallback((info: DateSelectArg) => {
    setSelectedSlotStart(info.start)
    setBookingDialogOpen(true)
  }, [])

  const handleEventClick = useCallback((info: EventClickArg) => {
    const ext = info.event.extendedProps
    setSelectedAppointment({
      appointmentId: info.event.id,
      patientName: ext.patientName as string,
      doctorName: ext.doctorName as string,
      appointmentTypeName: ext.appointmentTypeName as string,
      status: ext.status as number,
      notes: ext.notes as string | null,
      start: info.event.start!,
      end: info.event.end!,
    })
    setDetailDialogOpen(true)
  }, [])

  const handleBookAppointmentClick = useCallback(() => {
    setSelectedSlotStart(undefined)
    setBookingDialogOpen(true)
  }, [])

  return (
    <div className="space-y-4">
      {/* Page header */}
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <IconCalendar className="h-6 w-6 text-primary" />
          <h1 className="text-2xl font-semibold tracking-tight">{t("title")}</h1>
        </div>
        <div className="flex items-center gap-3">
          <DoctorSelector
            value={selectedDoctorId}
            onChange={setSelectedDoctorId}
            className="w-[200px]"
          />
          <Button onClick={handleBookAppointmentClick}>
            <IconPlus className="mr-2 h-4 w-4" />
            {t("bookAppointment")}
          </Button>
        </div>
      </div>

      {/* Main content with tabs */}
      <Tabs defaultValue="calendar">
        <TabsList>
          <TabsTrigger value="calendar">{t("calendar")}</TabsTrigger>
          <TabsTrigger value="pending" className="flex items-center gap-2">
            {t("pendingBookings")}
            {pendingCount > 0 && (
              <Badge variant="destructive" className="h-5 min-w-5 px-1.5 text-[10px]">
                {pendingCount}
              </Badge>
            )}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="calendar" className="mt-4">
          <AppointmentCalendar
            doctorId={selectedDoctorId}
            onSlotClick={handleSlotClick}
            onEventClick={handleEventClick}
          />
        </TabsContent>

        <TabsContent value="pending" className="mt-4">
          <PendingBookingsPanel />
        </TabsContent>
      </Tabs>

      {/* Booking dialog */}
      <AppointmentBookingDialog
        open={bookingDialogOpen}
        onOpenChange={setBookingDialogOpen}
        defaultDoctorId={selectedDoctorId}
        defaultStartTime={selectedSlotStart}
      />

      {/* Detail dialog */}
      <AppointmentDetailDialog
        open={detailDialogOpen}
        onOpenChange={setDetailDialogOpen}
        appointment={selectedAppointment}
      />
    </div>
  )
}
