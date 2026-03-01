import { useTranslation } from "react-i18next"
import { useQuery } from "@tanstack/react-query"
import { useNavigate } from "@tanstack/react-router"
import { format } from "date-fns"
import { vi, enUS } from "date-fns/locale"
import {
  IconCalendar,
  IconCalendarPlus,
  IconClock,
} from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { api } from "@/shared/lib/api-client"

interface AppointmentDto {
  id: string
  date: string
  startTime: string
  endTime: string
  doctorName: string
  type: string
  status: string
}

interface PatientAppointmentTabProps {
  patientId: string
}

export function PatientAppointmentTab({
  patientId,
}: PatientAppointmentTabProps) {
  const { t, i18n } = useTranslation("patient")
  const { t: tScheduling } = useTranslation("scheduling")
  const navigate = useNavigate()
  const locale = i18n.language === "vi" ? vi : enUS
  const dateFormat = i18n.language === "vi" ? "dd/MM/yyyy" : "MM/dd/yyyy"

  const appointmentsQuery = useQuery({
    queryKey: ["appointments", "by-patient", patientId],
    queryFn: async (): Promise<AppointmentDto[]> => {
      const { data, error } = await api.GET(
        `/api/appointments/by-patient/${patientId}` as never,
        {},
      )
      if (error) {
        // Scheduling API may not be available yet
        return []
      }
      return (data as AppointmentDto[]) ?? []
    },
    retry: false,
  })

  const appointments = appointmentsQuery.data ?? []
  const now = new Date()

  const upcoming = appointments.filter(
    (a) => new Date(a.date) >= now,
  )
  const past = appointments.filter(
    (a) => new Date(a.date) < now,
  )

  const statusColor: Record<string, string> = {
    Confirmed: "bg-green-500 text-white hover:bg-green-500",
    Pending: "bg-yellow-500 text-white hover:bg-yellow-500",
    Cancelled: "bg-red-500 text-white hover:bg-red-500",
    Completed: "bg-blue-500 text-white hover:bg-blue-500",
  }

  const renderAppointment = (appointment: AppointmentDto) => (
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
            {format(new Date(appointment.date), dateFormat, { locale })}
          </p>
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <IconClock className="h-3 w-3" />
            <span>
              {appointment.startTime} - {appointment.endTime}
            </span>
            <span>&middot;</span>
            <span>{appointment.doctorName}</span>
          </div>
        </div>
      </div>
      <div className="flex items-center gap-2">
        <Badge variant="secondary">{appointment.type}</Badge>
        <Badge
          variant="outline"
          className={statusColor[appointment.status] ?? ""}
        >
          {appointment.status}
        </Badge>
      </div>
    </div>
  )

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h3 className="font-medium">{t("appointments")}</h3>
        <Button
          size="sm"
          onClick={() =>
            navigate({ to: "/appointments" as string } as never)
          }
        >
          <IconCalendarPlus className="h-4 w-4 mr-1" />
          {t("bookAppointment")}
        </Button>
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
