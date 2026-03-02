import { useState, useEffect, useMemo } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { format, addDays, getDay } from "date-fns"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Textarea } from "@/shared/components/Textarea"
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
import { DatePicker } from "@/shared/components/DatePicker"
import {
  usePublicAppointmentTypes,
  usePublicSchedule,
  type SubmitBookingCommand,
  type PublicAppointmentTypeDto,
} from "@/features/booking/api/booking-api"
import { IconLoader2 } from "@tabler/icons-react"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import { ServerValidationAlert } from "@/shared/components/ServerValidationAlert"

interface BookingFormProps {
  onSubmit: (data: SubmitBookingCommand) => void
  isSubmitting: boolean
  error?: Error | null
}

// Vietnamese phone regex: 0[3|5|7|8|9]xxxxxxxx
const VN_PHONE_REGEX = /^(0[35789])\d{8}$/

export function BookingForm({ onSubmit, isSubmitting, error }: BookingFormProps) {
  const { t, i18n } = useTranslation("scheduling")
  const { t: tCommon } = useTranslation("common")

  const { data: appointmentTypes } = usePublicAppointmentTypes()
  const { data: clinicSchedule } = usePublicSchedule()
  const [nonFieldError, setNonFieldError] = useState<string | null>(null)

  // Parse structured server validation errors from the error prop
  useEffect(() => {
    if (error) {
      const nonFieldErrors = handleServerValidationError(error, form.setError)
      if (nonFieldErrors.length > 0) {
        setNonFieldError(nonFieldErrors[0])
      }
    } else {
      setNonFieldError(null)
    }
  }, [error]) // eslint-disable-line react-hooks/exhaustive-deps

  // Days when clinic is open (for date picker filter)
  const openDays = useMemo(() => {
    if (!clinicSchedule) return new Set<number>()
    return new Set(clinicSchedule.filter((s) => s.isOpen).map((s) => s.dayOfWeek))
  }, [clinicSchedule])

  const schema = useMemo(
    () =>
      z.object({
        patientName: z
          .string()
          .min(1, tCommon("validation.required"))
          .min(2, tCommon("validation.minLength", { min: 2 })),
        phone: z
          .string()
          .min(1, tCommon("validation.required"))
          .regex(VN_PHONE_REGEX, t("selfBooking.phone")),
        email: z
          .string()
          .email(tCommon("validation.invalidEmail"))
          .optional()
          .or(z.literal("")),
        preferredDate: z.date({ required_error: tCommon("validation.required") }),
        preferredTimeSlot: z.string().optional(),
        appointmentTypeId: z.string().min(1, tCommon("validation.required")),
        preferredDoctorId: z.string().optional(),
        notes: z.string().optional(),
      }),
    [t, tCommon],
  )

  type FormValues = z.infer<typeof schema>

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      patientName: "",
      phone: "",
      email: "",
      preferredDate: undefined,
      preferredTimeSlot: "",
      appointmentTypeId: "",
      preferredDoctorId: "",
      notes: "",
    },
  })

  const handleSubmit = (values: FormValues) => {
    onSubmit({
      patientName: values.patientName,
      phone: values.phone,
      email: values.email || null,
      preferredDate: format(values.preferredDate, "yyyy-MM-dd'T'00:00:00"),
      preferredTimeSlot: values.preferredTimeSlot || null,
      appointmentTypeId: values.appointmentTypeId,
      preferredDoctorId: values.preferredDoctorId || null,
      notes: values.notes || null,
    })
  }

  // Disable dates: past dates, closed days, more than 30 days out
  const isDateDisabled = (date: Date): boolean => {
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    if (date < today) return true
    if (date > addDays(today, 30)) return true
    const day = getDay(date) // 0=Sunday
    return !openDays.has(day)
  }

  return (
    <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-5">
      {/* Patient Name */}
      <Field>
        <FieldLabel>{t("selfBooking.fullName")} *</FieldLabel>
        <Input
          {...form.register("patientName")}
        />
        {form.formState.errors.patientName && (
          <FieldError>{form.formState.errors.patientName.message}</FieldError>
        )}
      </Field>

      {/* Phone */}
      <Field>
        <FieldLabel>{t("selfBooking.phone")} *</FieldLabel>
        <Input
          {...form.register("phone")}
          type="tel"
        />
        {form.formState.errors.phone && (
          <FieldError>{form.formState.errors.phone.message}</FieldError>
        )}
      </Field>

      {/* Email (optional) */}
      <Field>
        <FieldLabel>Email</FieldLabel>
        <Input
          {...form.register("email")}
          type="email"
        />
        {form.formState.errors.email && (
          <FieldError>{form.formState.errors.email.message}</FieldError>
        )}
      </Field>

      {/* Appointment Type */}
      <Field>
        <FieldLabel>{t("appointmentType")} *</FieldLabel>
        <Controller
          control={form.control}
          name="appointmentTypeId"
          render={({ field }) => (
            <Select value={field.value} onValueChange={field.onChange}>
              <SelectTrigger>
                <SelectValue placeholder={t("appointmentType")} />
              </SelectTrigger>
              <SelectContent>
                {(appointmentTypes ?? []).map((type: PublicAppointmentTypeDto) => (
                  <SelectItem key={type.id} value={type.id}>
                    <span className="flex items-center gap-2">
                      <span
                        className="inline-block h-2.5 w-2.5 shrink-0"
                        style={{ backgroundColor: type.color }}
                      />
                      {i18n.language === "vi" ? type.nameVi : type.name}
                      {" "}({type.defaultDurationMinutes} {t("minutes")})
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

      {/* Preferred Date */}
      <Field>
        <FieldLabel>{t("selfBooking.preferredDate")} *</FieldLabel>
        <Controller
          control={form.control}
          name="preferredDate"
          render={({ field }) => (
            <DatePicker
              value={field.value}
              onChange={field.onChange}
              className="w-full"
            />
          )}
        />
        {form.formState.errors.preferredDate && (
          <FieldError>{form.formState.errors.preferredDate.message}</FieldError>
        )}
      </Field>

      {/* Preferred Time Slot */}
      <Field>
        <FieldLabel>{t("selfBooking.preferredTime")}</FieldLabel>
        <Controller
          control={form.control}
          name="preferredTimeSlot"
          render={({ field }) => (
            <Select value={field.value} onValueChange={field.onChange}>
              <SelectTrigger>
                <SelectValue placeholder={t("selfBooking.preferredTime")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="morning">
                  {i18n.language === "vi" ? "Buổi sáng (8:00 - 12:00)" : "Morning (8:00 - 12:00)"}
                </SelectItem>
                <SelectItem value="afternoon">
                  {i18n.language === "vi" ? "Buổi chiều (13:00 - 17:00)" : "Afternoon (13:00 - 17:00)"}
                </SelectItem>
                <SelectItem value="evening">
                  {i18n.language === "vi" ? "Buổi tối (17:00 - 20:00)" : "Evening (17:00 - 20:00)"}
                </SelectItem>
              </SelectContent>
            </Select>
          )}
        />
      </Field>

      {/* Notes */}
      <Field>
        <FieldLabel>{t("notes")}</FieldLabel>
        <Textarea
          {...form.register("notes")}
        />
      </Field>

      {/* Error message */}
      {error && error.message === "RATE_LIMITED" && (
        <div className="text-sm text-destructive p-3 bg-destructive/10 border border-destructive/20 rounded-md">
          {t("selfBooking.maxPendingReached")}
        </div>
      )}
      {error && error.message === "VALIDATION_ERROR" && (
        <div className="text-sm text-destructive p-3 bg-destructive/10 border border-destructive/20 rounded-md">
          {tCommon("status.error")}
        </div>
      )}
      <ServerValidationAlert
        error={nonFieldError}
        onDismiss={() => setNonFieldError(null)}
      />

      {/* Submit */}
      <Button type="submit" className="w-full" disabled={isSubmitting}>
        {isSubmitting && <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />}
        {t("selfBooking.submit")}
      </Button>
    </form>
  )
}
