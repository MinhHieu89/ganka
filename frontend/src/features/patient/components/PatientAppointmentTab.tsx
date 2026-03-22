import { useState } from "react"
import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { vi, enUS } from "date-fns/locale"
import {
  IconCalendar,
  IconCalendarPlus,
  IconClock,
} from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { AppointmentBookingDialog } from "@/features/scheduling/components/AppointmentBookingDialog"
import {
  useAppointmentsByPatient,
  type AppointmentDto,
} from "@/features/scheduling/api/scheduling-api"

interface PatientAppointmentTabProps {
  patientId: string
  patientName: string
}

export function PatientAppointmentTab({
  patientId,
  patientName,
}: PatientAppointmentTabProps) {
  const { t, i18n } = useTranslation("patient")
  const { t: tScheduling } = useTranslation("scheduling")
  const [bookingDialogOpen, setBookingDialogOpen] = useState(false)
  const locale = i18n.language === "vi" ? vi : enUS
  const dateFormat = i18n.language === "vi" ? "dd/MM/yyyy" : "MM/dd/yyyy"

  const appointmentsQuery = useAppointmentsByPatient(patientId)

  const appointments = appointmentsQuery.data ?? []
  const now = new Date()

  const upcoming = appointments.filter(
    (a) => new Date(a.startTime) >= now,
  )
  const past = appointments.filter(
    (a) => new Date(a.startTime) < now,
  )

  const statusLabels: Record<number, string> = {
    0: tScheduling("status.pending"),
    1: tScheduling("status.confirmed"),
    2: tScheduling("status.completed"),
    3: tScheduling("status.cancelled"),
  }

  const statusColor: Record<number, string> = {
    0: "bg-yellow-500 text-white hover:bg-yellow-500",
    1: "bg-green-500 text-white hover:bg-green-500",
    2: "bg-blue-500 text-white hover:bg-blue-500",
    3: "bg-red-500 text-white hover:bg-red-500",
  }

  const renderAppointment = (appointment: AppointmentDto) => {
    const start = new Date(appointment.startTime)
    const end = new Date(appointment.endTime)
    const timeStr = `${format(start, "HH:mm")} - ${format(end, "HH:mm")}`
    const typeName = i18n.language === "vi"
      ? appointment.appointmentTypeNameVi
      : appointment.appointmentTypeName

    return (
      <div
        key={appointment.id}
        className="flex items-center justify-between p-3 border"
      >
        <div className="flex items-center gap-3">
          <div className="flex size-10 items-center justify-center bg-primary/5 text-primary shrink-0">
            <IconCalendar className="h-5 w-5" />
          </div>
          <div>
            <p className="text-sm font-medium">
              {format(start, dateFormat, { locale })}
            </p>
            <div className="flex items-center gap-2 text-xs text-muted-foreground">
              <IconClock className="h-3 w-3" />
              <span>{timeStr}</span>
              <span>&middot;</span>
              <span>{appointment.doctorName}</span>
            </div>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="secondary">{typeName}</Badge>
          <Badge
            variant="outline"
            className={statusColor[appointment.status] ?? ""}
          >
            {statusLabels[appointment.status] ?? "Unknown"}
          </Badge>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h3 className="font-medium">{t("appointments")}</h3>
        <Button
          size="sm"
          onClick={() => setBookingDialogOpen(true)}
        >
          <IconCalendarPlus className="h-4 w-4 mr-1" />
          {t("bookAppointment")}
        </Button>
        <AppointmentBookingDialog
          open={bookingDialogOpen}
          onOpenChange={setBookingDialogOpen}
          defaultPatientId={patientId}
          defaultPatientName={patientName}
        />
      </div>

      {appointments.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center border border-dashed">
          <IconCalendar className="h-8 w-8 text-muted-foreground/40 mb-3" />
          <p className="text-sm text-muted-foreground">
            {t("noAppointments")}
          </p>
        </div>
      ) : (
        <>
          {upcoming.length > 0 && (
            <div className="space-y-2">
              <h4 className="text-sm font-medium text-muted-foreground">
                {t("upcomingAppointments")}
              </h4>
              {upcoming.map(renderAppointment)}
            </div>
          )}

          {past.length > 0 && (
            <div className="space-y-2">
              <h4 className="text-sm font-medium text-muted-foreground">
                {t("pastAppointments")}
              </h4>
              {past.map(renderAppointment)}
            </div>
          )}
        </>
      )}
    </div>
  )
}
