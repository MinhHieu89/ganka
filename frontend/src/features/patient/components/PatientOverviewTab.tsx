import { useState, useEffect } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { vi, enUS } from "date-fns/locale"
import { toast } from "sonner"
import { IconEdit, IconLoader2, IconX } from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { DatePicker } from "@/shared/components/DatePicker"
import {
  useUpdatePatient,
  usePatientFieldValidation,
  type PatientDto,
} from "@/features/patient/api/patient-api"
import { PatientFieldWarning } from "@/features/patient/components/PatientFieldWarning"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import { ServerValidationAlert } from "@/shared/components/ServerValidationAlert"

interface PatientOverviewTabProps {
  patient: PatientDto
  isEditing: boolean
  onEditToggle: (editing: boolean) => void
}

export function PatientOverviewTab({
  patient,
  isEditing,
  onEditToggle,
}: PatientOverviewTabProps) {
  const { t, i18n } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const updateMutation = useUpdatePatient()
  const { data: fieldValidation } = usePatientFieldValidation(patient.id)
  const [nonFieldError, setNonFieldError] = useState<string | null>(null)

  const locale = i18n.language === "vi" ? vi : enUS
  const dateFormat = i18n.language === "vi" ? "dd/MM/yyyy" : "MM/dd/yyyy"

  const schema = z.object({
    fullName: z.string().min(1, tCommon("validation.required")),
    phone: z
      .string()
      .min(1, tCommon("validation.required"))
      .regex(/^0\d{9,10}$/, t("invalidPhone")),
    dateOfBirth: z.date().nullable().optional(),
    gender: z.enum(["Male", "Female", "Other"]).nullable().optional(),
    address: z.string().optional(),
    cccd: z.string().optional(),
  })

  type FormValues = z.infer<typeof schema>

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      fullName: patient.fullName,
      phone: patient.phone,
      dateOfBirth: patient.dateOfBirth
        ? new Date(patient.dateOfBirth)
        : null,
      gender: patient.gender,
      address: patient.address ?? "",
      cccd: patient.cccd ?? "",
    },
  })

  // Reset form values when patient data changes (ensures DOB displays correctly)
  useEffect(() => {
    form.reset({
      fullName: patient.fullName,
      phone: patient.phone,
      dateOfBirth: patient.dateOfBirth ? new Date(patient.dateOfBirth) : null,
      gender: patient.gender,
      address: patient.address ?? "",
      cccd: patient.cccd ?? "",
    })
  }, [patient.id]) // eslint-disable-line react-hooks/exhaustive-deps

  const handleSubmit = async (data: FormValues) => {
    try {
      await updateMutation.mutateAsync({
        patientId: patient.id,
        fullName: data.fullName,
        phone: data.phone,
        dateOfBirth: data.dateOfBirth
          ? data.dateOfBirth.toISOString()
          : null,
        gender: data.gender,
        address: data.address || null,
        cccd: data.cccd || null,
      })
      toast.success(tCommon("status.success"))
      onEditToggle(false)
    } catch (error) {
      const nonFieldErrors = handleServerValidationError(error, form.setError)
      if (nonFieldErrors.length > 0) {
        setNonFieldError(nonFieldErrors[0])
      }
    }
  }

  const handleCancel = () => {
    form.reset({
      fullName: patient.fullName,
      phone: patient.phone,
      dateOfBirth: patient.dateOfBirth
        ? new Date(patient.dateOfBirth)
        : null,
      gender: patient.gender,
      address: patient.address ?? "",
      cccd: patient.cccd ?? "",
    })
    setNonFieldError(null)
    onEditToggle(false)
  }

  if (!isEditing) {
    return (
      <div className="space-y-4">
        {fieldValidation && !fieldValidation.isValid && (
          <PatientFieldWarning
            missingFields={fieldValidation.missingFields}
            onUpdateProfile={() => onEditToggle(true)}
          />
        )}
        <div className="grid gap-4 md:grid-cols-2">
        {/* Personal Info */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-base">{t("personalInfo")}</CardTitle>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEditToggle(true)}
            >
              <IconEdit className="h-4 w-4 mr-1" />
              {t("edit")}
            </Button>
          </CardHeader>
          <CardContent className="space-y-3">
            <InfoRow label={t("fullName")} value={patient.fullName} />
            <InfoRow label={t("phone")} value={patient.phone} />
            <InfoRow
              label={t("dateOfBirth")}
              value={
                patient.dateOfBirth
                  ? format(new Date(patient.dateOfBirth), dateFormat, {
                      locale,
                    })
                  : "---"
              }
            />
            <InfoRow
              label={t("gender")}
              value={
                patient.gender
                  ? patient.gender === "Male"
                    ? t("male")
                    : patient.gender === "Female"
                      ? t("female")
                      : t("other")
                  : "---"
              }
            />
            <InfoRow label={t("address")} value={patient.address ?? "---"} />
            <InfoRow label={t("cccd")} value={patient.cccd ?? "---"} />
          </CardContent>
        </Card>

        {/* System Info */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t("systemInfo")}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <InfoRow
              label={t("patientCode")}
              value={patient.patientCode ?? "---"}
              mono
            />
            <InfoRow
              label={t("patientType")}
              value={
                patient.patientType === "Medical"
                  ? t("medicalPatient")
                  : t("walkInCustomer")
              }
            />
            <InfoRow
              label={t("createdAt")}
              value={format(new Date(patient.createdAt), dateFormat, {
                locale,
              })}
            />
            <InfoRow
              label={t("status")}
              value={patient.isActive ? t("active") : t("inactive")}
            />
          </CardContent>
        </Card>
      </div>
      </div>
    )
  }

  // Edit mode
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle className="text-base">{t("edit")}</CardTitle>
        <Button variant="ghost" size="sm" onClick={handleCancel}>
          <IconX className="h-4 w-4" />
        </Button>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={form.handleSubmit(handleSubmit)}
          className="space-y-4"
        >
          <ServerValidationAlert
            error={nonFieldError}
            onDismiss={() => setNonFieldError(null)}
          />
          <div className="grid gap-4 md:grid-cols-2">
          <Controller
            name="fullName"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel required htmlFor={field.name}>{t("fullName")}</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          <Controller
            name="phone"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel required htmlFor={field.name}>{t("phone")}</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          <Controller
            name="dateOfBirth"
            control={form.control}
            render={({ field }) => (
              <Field>
                <FieldLabel>{t("dateOfBirth")}</FieldLabel>
                <DatePicker
                  value={field.value ?? undefined}
                  onChange={field.onChange}
                  placeholder={t("dateOfBirth")}
                  className="w-full"
                />
              </Field>
            )}
          />

          <Controller
            name="gender"
            control={form.control}
            render={({ field }) => (
              <Field>
                <FieldLabel>{t("gender")}</FieldLabel>
                <Select
                  value={field.value ?? ""}
                  onValueChange={field.onChange}
                >
                  <SelectTrigger>
                    <SelectValue placeholder={t("gender")} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Male">{t("male")}</SelectItem>
                    <SelectItem value="Female">{t("female")}</SelectItem>
                    <SelectItem value="Other">{t("other")}</SelectItem>
                  </SelectContent>
                </Select>
              </Field>
            )}
          />

          <Controller
            name="address"
            control={form.control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor={field.name}>{t("address")}</FieldLabel>
                <Input {...field} id={field.name} />
              </Field>
            )}
          />

          <Controller
            name="cccd"
            control={form.control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor={field.name}>{t("cccd")}</FieldLabel>
                <Input {...field} id={field.name} />
              </Field>
            )}
          />

          <div className="md:col-span-2 flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={handleCancel}>
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={updateMutation.isPending}>
              {updateMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {tCommon("buttons.save")}
            </Button>
          </div>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

function InfoRow({
  label,
  value,
  mono = false,
}: {
  label: string
  value: string
  mono?: boolean
}) {
  return (
    <div className="flex items-center justify-between text-sm">
      <span className="text-muted-foreground">{label}</span>
      <span className={mono ? "font-mono" : "font-medium"}>{value}</span>
    </div>
  )
}
