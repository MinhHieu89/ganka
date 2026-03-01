import { useState, useEffect, useMemo } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { format } from "date-fns"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
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
import { usePatientSearch } from "@/features/patient/hooks/usePatientSearch"
import {
  useBookAppointment,
  useAppointmentTypes,
  type AppointmentTypeDto,
} from "@/features/scheduling/api/scheduling-api"
import { DoctorSelector } from "@/features/scheduling/components/DoctorSelector"
import { IconSearch, IconLoader2 } from "@tabler/icons-react"

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

  const schema = useMemo(
    () =>
      z.object({
        patientId: z.string().min(1, t("selectPatient")),
        patientName: z.string().min(1),
        doctorId: z.string().min(1, t("selectDoctor")),
        doctorName: z.string().min(1),
        appointmentTypeId: z.string().min(1, t("appointmentType")),
        startTime: z.string().min(1, t("startTime")),
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
      startTime: defaultStartTime
        ? format(defaultStartTime, "yyyy-MM-dd'T'HH:mm")
        : "",
      notes: "",
    },
  })

  // Reset form when dialog opens with new defaults
  useEffect(() => {
    if (open) {
      form.reset({
        patientId: "",
        patientName: "",
        doctorId: defaultDoctorId ?? "",
        doctorName: "",
        appointmentTypeId: "",
        startTime: defaultStartTime
          ? format(defaultStartTime, "yyyy-MM-dd'T'HH:mm")
          : "",
        notes: "",
      })
    }
  }, [open, defaultDoctorId, defaultStartTime, form])

  // -- Patient search state --
  const [patientSearchTerm, setPatientSearchTerm] = useState("")
  const [patientPopoverOpen, setPatientPopoverOpen] = useState(false)
  const { data: patientResults, isLoading: isSearchingPatients } =
    usePatientSearch(patientSearchTerm, { enabled: patientSearchTerm.length >= 2 })

  const selectedType = appointmentTypes?.find(
    (t) => t.id === form.watch("appointmentTypeId"),
  )

  const onSubmit = (values: FormValues) => {
    bookAppointment.mutate(
      {
        patientId: values.patientId,
        patientName: values.patientName,
        doctorId: values.doctorId,
        doctorName: values.doctorName,
        startTime: new Date(values.startTime).toISOString(),
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
            toast.error(t("slotAlreadyBooked"))
          } else if (error.message === "VALIDATION_ERROR") {
            toast.error(t("outsideClinicHours"))
          } else {
            toast.error(error.message)
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
                  onChange={(id) => {
                    field.onChange(id)
                    // We don't have doctor name in the selector callback directly
                    // Set it from the trigger display text after selection
                    form.setValue("doctorName", id)
                  }}
                />
              )}
            />
            {form.formState.errors.doctorId && (
              <FieldError>{form.formState.errors.doctorId.message}</FieldError>
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

          {/* Start Time */}
          <Field>
            <FieldLabel>{t("startTime")}</FieldLabel>
            <Input
              type="datetime-local"
              {...form.register("startTime")}
            />
            {form.formState.errors.startTime && (
              <FieldError>{form.formState.errors.startTime.message}</FieldError>
            )}
          </Field>

          {/* Notes */}
          <Field>
            <FieldLabel>{t("notes")}</FieldLabel>
            <textarea
              {...form.register("notes")}
              className="flex min-h-[80px] w-full border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              placeholder={t("notes")}
            />
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
