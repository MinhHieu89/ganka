import { useRef, useCallback, useMemo } from "react"
import FullCalendar from "@fullcalendar/react"
import timeGridPlugin from "@fullcalendar/timegrid"
import dayGridPlugin from "@fullcalendar/daygrid"
import interactionPlugin from "@fullcalendar/interaction"
import momentTimezonePlugin from "@fullcalendar/moment-timezone"
import type { EventClickArg, DateSelectArg, EventDropArg, EventContentArg } from "@fullcalendar/core"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { useAppointmentsForCalendar } from "@/features/scheduling/hooks/useAppointments"
import { useRescheduleAppointment } from "@/features/scheduling/api/scheduling-api"
import { Skeleton } from "@/shared/components/Skeleton"

interface AppointmentCalendarProps {
  doctorId: string | undefined
  onSlotClick: (info: DateSelectArg) => void
  onEventClick: (info: EventClickArg) => void
}

function renderEventContent(lang: string) {
  return function EventContent({ event, timeText }: EventContentArg) {
    const props = event.extendedProps
    const typeName =
      lang === "vi"
        ? props.appointmentTypeNameVi || props.appointmentTypeName
        : props.appointmentTypeName
    const tooltipText = `${props.patientName}\n${typeName}\n${timeText}`

    return (
      <div className="w-full overflow-hidden px-0.5 leading-tight" title={tooltipText}>
        <div className="truncate text-[11px] font-semibold">
          {props.patientName}
        </div>
        <div className="truncate text-[10px] opacity-85">
          {typeName}
        </div>
      </div>
    )
  }
}

export function AppointmentCalendar({
  doctorId,
  onSlotClick,
  onEventClick,
}: AppointmentCalendarProps) {
  const { t, i18n } = useTranslation("scheduling")
  const calendarRef = useRef<FullCalendar>(null)
  const { events, businessHours, isLoading, handleDatesSet } =
    useAppointmentsForCalendar(doctorId)
  const reschedule = useRescheduleAppointment()
  const eventContentRenderer = useMemo(() => renderEventContent(i18n.language), [i18n.language])

  const handleEventDrop = useCallback(
    (info: EventDropArg) => {
      const appointmentId = info.event.id
      const newStart = info.event.start
      if (!appointmentId || !newStart) {
        info.revert()
        return
      }

      reschedule.mutate(
        {
          appointmentId,
          newStartTime: newStart.toISOString(),
        },
        {
          onError: (error) => {
            info.revert()
            if (error.message === "DOUBLE_BOOKING") {
              toast.error(t("slotAlreadyBooked"))
            } else if (error.message === "VALIDATION_ERROR") {
              toast.error(t("outsideClinicHours"))
            } else {
              toast.error(t("reschedule") + " failed")
            }
          },
          onSuccess: () => {
            toast.success(t("confirmReschedule"))
          },
        },
      )
    },
    [reschedule, t],
  )

  if (isLoading && !events.length) {
    return (
      <div className="space-y-4 p-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-[600px] w-full" />
      </div>
    )
  }

  return (
    <div className="appointment-calendar">
      <FullCalendar
        ref={calendarRef}
        plugins={[timeGridPlugin, dayGridPlugin, interactionPlugin, momentTimezonePlugin]}
        initialView="timeGridWeek"
        headerToolbar={{
          left: "prev,next today",
          center: "title",
          right: "timeGridWeek,timeGridDay",
        }}
        slotMinTime="08:00:00"
        slotMaxTime="21:00:00"
        slotDuration="00:30:00"
        slotLabelInterval="00:30:00"
        selectable
        editable
        eventOverlap={false}
        selectOverlap={false}
        businessHours={businessHours}
        selectConstraint="businessHours"
        select={onSlotClick}
        eventClick={onEventClick}
        eventDrop={handleEventDrop}
        eventContent={eventContentRenderer}
        events={events}
        datesSet={handleDatesSet}
        timeZone="Asia/Ho_Chi_Minh"
        locale="vi"
        firstDay={1}
        allDaySlot={false}
        nowIndicator
        height="auto"
        expandRows
        stickyHeaderDates
        dayHeaderFormat={{ weekday: "short", day: "numeric", month: "numeric" }}
        slotLabelFormat={{
          hour: "2-digit",
          minute: "2-digit",
          hour12: false,
        }}
        eventTimeFormat={{
          hour: "2-digit",
          minute: "2-digit",
          hour12: false,
        }}
        buttonText={{
          today: t("status.title") === "Status" ? "Today" : "H.nay",
          week: t("weekView"),
          day: t("dayView"),
        }}
      />
      <style>{`
        .appointment-calendar .fc {
          --fc-border-color: var(--border);
          --fc-today-bg-color: color-mix(in oklch, var(--accent) 15%, transparent);
          --fc-now-indicator-color: var(--primary);
          --fc-non-business-color: color-mix(in oklch, var(--muted) 50%, transparent);
          --fc-page-bg-color: transparent;
          font-family: var(--font-sans, 'Be Vietnam Pro', sans-serif);
          border-radius: var(--radius);
          overflow: hidden;
        }
        .appointment-calendar .fc .fc-toolbar-title {
          font-size: 1.125rem;
          font-weight: 600;
        }
        .appointment-calendar .fc .fc-button {
          background-color: var(--secondary);
          border-color: var(--border);
          color: var(--secondary-foreground);
          font-size: 0.8125rem;
          font-weight: 500;
          padding: 0.25rem 0.625rem;
          border-radius: calc(var(--radius) - 2px);
        }
        .appointment-calendar .fc .fc-button:hover {
          background-color: var(--accent);
        }
        .appointment-calendar .fc .fc-button-active {
          background-color: var(--primary) !important;
          color: var(--primary-foreground) !important;
        }
        .appointment-calendar .fc .fc-col-header-cell {
          padding: 0.5rem 0;
          font-weight: 500;
          font-size: 0.8125rem;
        }
        .appointment-calendar .fc .fc-timegrid-slot {
          height: 2rem;
        }
        .appointment-calendar .fc .fc-timegrid-slot-label {
          font-size: 0.75rem;
          color: var(--muted-foreground);
        }
        .appointment-calendar .fc .fc-event {
          border-radius: calc(var(--radius) - 4px);
          font-size: 0.75rem;
          padding: 1px 2px;
          cursor: pointer;
          overflow: hidden;
        }
        .appointment-calendar .fc .fc-event-main {
          overflow: hidden;
        }
        .appointment-calendar .fc .fc-event:hover {
          opacity: 0.9;
        }
        .appointment-calendar .fc .fc-highlight {
          background-color: color-mix(in oklch, var(--primary) 10%, transparent);
        }
      `}</style>
    </div>
  )
}
