import { useState, useEffect, useMemo } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
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
  FieldError,
} from "@/shared/components/Field"
import { Popover, PopoverContent, PopoverTrigger } from "@/shared/components/Popover"
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/shared/components/Command"
import { DatePicker } from "@/shared/components/DatePicker"
import { Textarea } from "@/shared/components/Textarea"
import { usePatientSearch } from "@/features/patient/hooks/usePatientSearch"
import {
  useBookAppointment,
  useAppointmentTypes,
  type AppointmentTypeDto,
} from "@/features/scheduling/api/scheduling-api"
import { DoctorSelector, useDoctors } from "@/features/scheduling/components/DoctorSelector"
import { IconSearch, IconLoader2 } from "@tabler/icons-react"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import { ServerValidationAlert } from "@/shared/components/ServerValidationAlert"

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

interface AppointmentBookingDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  defaultDoctorId?: string
  defaultStartTime?: Date
  defaultEndTime?: Date
}

export function AppointmentBookingDialog({
  open,
  onOpenChange,
  defaultDoctorId,
  defaultStartTime,
}: AppointmentBookingDialogProps) {
  const { t } = useTranslation("scheduling")
  const { t: tCommon } = useTranslation("common")

  const bookAppointment = useBookAppointment()
  const { data: appointmentTypes } = useAppointmentTypes()
  const { data: doctors } = useDoctors()
  const [nonFieldError, setNonFieldError] = useState<string | null>(null)

  const schema = useMemo(
    () =>
      z.object({
        patientId: z.string().min(1, t("selectPatient")),
        patientName: z.string().min(1),
        doctorId: z.string().min(1, t("selectDoctor")),
        doctorName: z.string().min(1),
        appointmentTypeId: z.string().min(1, t("appointmentType")),
        startDate: z.date({ required_error: t("date") }),
        startTime: z.string().min(1, t("time")),
        notes: z.string().optional(),
      }),
    [t],
  )

  type FormValues = z.infer<typeof schema>

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      patientId: "",
      patientName: "",
      doctorId: defaultDoctorId ?? "",
      doctorName: "",
      appointmentTypeId: "",
      startDate: defaultStartTime ?? undefined,
      startTime: defaultStartTime
        ? `${String(defaultStartTime.getUTCHours()).padStart(2, "0")}:${String(defaultStartTime.getUTCMinutes()).padStart(2, "0")}`
        : "",
      notes: "",
    },
  })

  // Reset form when dialog opens with new defaults
  useEffect(() => {
    if (open) {
      setNonFieldError(null)
      const doctorName = doctors?.find((d) => d.id === defaultDoctorId)?.fullName ?? ""
      form.reset({
        patientId: "",
        patientName: "",
        doctorId: defaultDoctorId ?? "",
        doctorName,
        appointmentTypeId: "",
        startDate: defaultStartTime ?? undefined,
        startTime: defaultStartTime
          ? `${String(defaultStartTime.getUTCHours()).padStart(2, "0")}:${String(defaultStartTime.getUTCMinutes()).padStart(2, "0")}`
          : "",
        notes: "",
      })
    }
  }, [open, defaultDoctorId, defaultStartTime, form, doctors])

  // -- Patient search state --
  const [patientSearchTerm, setPatientSearchTerm] = useState("")
  const [patientPopoverOpen, setPatientPopoverOpen] = useState(false)
  const { data: patientResults, isLoading: isSearchingPatients } =
    usePatientSearch(patientSearchTerm, { enabled: patientSearchTerm.length >= 2 })

  const selectedType = appointmentTypes?.find(
    (t) => t.id === form.watch("appointmentTypeId"),
  )

  const onSubmit = (values: FormValues) => {
    // Combine date + time into a single ISO datetime
    const startDateTime = new Date(values.startDate)
    const [h, m] = values.startTime.split(":")
    startDateTime.setHours(parseInt(h, 10), parseInt(m, 10), 0, 0)

    bookAppointment.mutate(
      {
        patientId: values.patientId,
        patientName: values.patientName,
        doctorId: values.doctorId,
        doctorName: values.doctorName,
        startTime: startDateTime.toISOString(),
        appointmentTypeId: values.appointmentTypeId,
        notes: values.notes || null,
      },
      {
        onSuccess: () => {
          toast.success(t("newAppointment"))
          onOpenChange(false)
        },
        onError: (error) => {
          if (error.message === "DOUBLE_BOOKING") {
            setNonFieldError(t("slotAlreadyBooked"))
          } else if (error.message === "VALIDATION_ERROR") {
            setNonFieldError(t("outsideClinicHours"))
          } else {
            const nonFieldErrors = handleServerValidationError(error, form.setError)
            if (nonFieldErrors.length > 0) {
              setNonFieldError(nonFieldErrors[0])
            }
          }
        },
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{t("newAppointment")}</DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <ServerValidationAlert
            error={nonFieldError}
            onDismiss={() => setNonFieldError(null)}
          />

          {/* Patient search */}
          <Field>
            <FieldLabel>{t("patient")}</FieldLabel>
            <Popover open={patientPopoverOpen} onOpenChange={setPatientPopoverOpen}>
              <PopoverTrigger asChild>
                <Button
                  variant="outline"
                  role="combobox"
                  className="w-full justify-start text-left font-normal"
                >
                  <IconSearch className="mr-2 h-4 w-4 shrink-0 opacity-50" />
                  {form.watch("patientName") || t("selectPatient")}
                </Button>
              </PopoverTrigger>
              <PopoverContent className="w-[--radix-popover-trigger-width] p-0" align="start">
                <Command shouldFilter={false}>
                  <CommandInput
                    placeholder={t("selectPatient")}
                    value={patientSearchTerm}
                    onValueChange={setPatientSearchTerm}
                  />
                  <CommandList>
                    {isSearchingPatients && (
                      <div className="flex items-center justify-center py-4">
                        <IconLoader2 className="h-4 w-4 animate-spin" />
                      </div>
                    )}
                    <CommandEmpty>
                      {patientSearchTerm.length < 2
                        ? tCommon("search.placeholder")
                        : tCommon("search.noResults")}
                    </CommandEmpty>
                    <CommandGroup>
                      {(patientResults ?? []).map((patient) => (
                        <CommandItem
                          key={patient.id}
                          value={patient.id}
                          onSelect={() => {
                            form.setValue("patientId", patient.id)
                            form.setValue("patientName", patient.fullName)
                            setPatientPopoverOpen(false)
                            setPatientSearchTerm("")
                          }}
                        >
                          <div className="flex flex-col">
                            <span className="font-medium">{patient.fullName}</span>
                            <span className="text-xs text-muted-foreground">
                              {patient.patientCode}
                              {patient.phone ? ` - ${patient.phone}` : ""}
                            </span>
                          </div>
                        </CommandItem>
                      ))}
                    </CommandGroup>
                  </CommandList>
                </Command>
              </PopoverContent>
            </Popover>
            {form.formState.errors.patientId && (
              <FieldError>{form.formState.errors.patientId.message}</FieldError>
            )}
          </Field>

          {/* Doctor */}
          <Field>
            <FieldLabel>{t("doctor")}</FieldLabel>
            <Controller
              control={form.control}
              name="doctorId"
              render={({ field }) => (
                <DoctorSelector
                  value={field.value}
                  onChange={(id, name) => {
                    field.onChange(id)
                    form.setValue("doctorName", name)
                  }}
                />
              )}
            />
            {form.formState.errors.doctorId && (
              <FieldError>{form.formState.errors.doctorId.message}</FieldError>
            )}
            {form.formState.errors.doctorName && (
              <FieldError>{form.formState.errors.doctorName.message}</FieldError>
            )}
          </Field>

          {/* Appointment Type */}
          <Field>
            <FieldLabel>
              {t("appointmentType")}
              {selectedType && (
                <span className="ml-2 text-xs text-muted-foreground font-normal">
                  ({selectedType.defaultDurationMinutes} {t("minutes")})
                </span>
              )}
            </FieldLabel>
            <Controller
              control={form.control}
              name="appointmentTypeId"
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger>
                    <SelectValue placeholder={t("appointmentType")} />
                  </SelectTrigger>
                  <SelectContent>
                    {(appointmentTypes ?? []).map((type: AppointmentTypeDto) => (
                      <SelectItem key={type.id} value={type.id}>
                        <span className="flex items-center gap-2">
                          <span
                            className="inline-block h-2.5 w-2.5 shrink-0"
                            style={{ backgroundColor: type.color }}
                          />
                          {type.nameVi} ({type.defaultDurationMinutes} {t("minutes")})
                        </span>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            {form.formState.errors.appointmentTypeId && (
              <FieldError>{form.formState.errors.appointmentTypeId.message}</FieldError>
            )}
          </Field>

          {/* Date */}
          <Field>
            <FieldLabel>{t("date")}</FieldLabel>
            <Controller
              control={form.control}
              name="startDate"
              render={({ field }) => (
                <DatePicker
                  value={field.value}
                  onChange={field.onChange}
                  fromDate={new Date()}
                  toDate={new Date(Date.now() + 90 * 24 * 60 * 60 * 1000)}
                  className="w-full"
                />
              )}
            />
            {form.formState.errors.startDate && (
              <FieldError>{form.formState.errors.startDate.message}</FieldError>
            )}
          </Field>

          {/* Time */}
          <Field>
            <FieldLabel>{t("time")}</FieldLabel>
            <Controller
              control={form.control}
              name="startTime"
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
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
              )}
            />
            {form.formState.errors.startTime && (
              <FieldError>{form.formState.errors.startTime.message}</FieldError>
            )}
          </Field>

          {/* Notes */}
          <Field>
            <FieldLabel>{t("notes")}</FieldLabel>
            <Textarea {...form.register("notes")} />
          </Field>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={bookAppointment.isPending}>
              {bookAppointment.isPending && (
                <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              {t("bookAppointment")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
