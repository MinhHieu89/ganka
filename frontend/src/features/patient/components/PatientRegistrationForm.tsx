import { useState } from "react"
import { useForm, Controller, useFieldArray } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import { toast } from "sonner"
import {
  IconLoader2,
  IconPlus,
  IconTrash,
  IconStethoscope,
  IconWalk,
} from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Input } from "@/shared/components/Input"
import { Button } from "@/shared/components/Button"
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
  useRegisterPatient,
  type PatientType,
  type Gender,
  type AllergySeverity,
} from "@/features/patient/api/patient-api"

interface PatientRegistrationFormProps {
  open: boolean
  onClose: () => void
}

export function PatientRegistrationForm({
  open,
  onClose,
}: PatientRegistrationFormProps) {
  const { t } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const navigate = useNavigate()
  const registerMutation = useRegisterPatient()
  const [patientType, setPatientType] = useState<PatientType>("Medical")

  const schema = z.object({
    fullName: z.string().min(1, tCommon("validation.required")),
    phone: z
      .string()
      .min(1, tCommon("validation.required"))
      .regex(/^(0[0-9]{9}|(\+84)[0-9]{9})$/, t("invalidPhone") || "Invalid phone number"),
    dateOfBirth: z.date().nullable().optional(),
    gender: z.enum(["Male", "Female", "Other"]).nullable().optional(),
    address: z.string().optional(),
    cccd: z.string().optional(),
    allergies: z
      .array(
        z.object({
          name: z.string().min(1, tCommon("validation.required")),
          severity: z.enum(["Mild", "Moderate", "Severe"]),
        }),
      )
      .optional(),
  })

  type FormValues = z.infer<typeof schema>

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      fullName: "",
      phone: "",
      dateOfBirth: null,
      gender: null,
      address: "",
      cccd: "",
      allergies: [],
    },
  })

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "allergies",
  })

  const handleSubmit = async (data: FormValues) => {
    try {
      const patientId = await registerMutation.mutateAsync({
        fullName: data.fullName,
        phone: data.phone,
        patientType,
        dateOfBirth: data.dateOfBirth
          ? data.dateOfBirth.toISOString()
          : null,
        gender: (patientType === "Medical" ? data.gender : null) as
          | Gender
          | null,
        address: patientType === "Medical" ? data.address : null,
        cccd: patientType === "Medical" ? data.cccd : null,
        allergies:
          data.allergies && data.allergies.length > 0
            ? data.allergies.map((a) => ({
                name: a.name,
                severity: a.severity as AllergySeverity,
              }))
            : null,
      })
      onClose()
      form.reset()
      navigate({
        to: "/patients/$patientId" as string,
        params: { patientId } as never,
      })
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : tCommon("status.error"),
      )
    }
  }

  const handleOpenChange = (v: boolean) => {
    if (!v) {
      onClose()
      form.reset()
      setPatientType("Medical")
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{t("register")}</DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Patient Type Toggle */}
          <div className="flex gap-2">
            <Button
              type="button"
              variant={patientType === "Medical" ? "default" : "outline"}
              className="flex-1"
              onClick={() => setPatientType("Medical")}
            >
              <IconStethoscope className="h-4 w-4 mr-2" />
              {t("medicalPatient")}
            </Button>
            <Button
              type="button"
              variant={patientType === "WalkIn" ? "default" : "outline"}
              className="flex-1"
              onClick={() => setPatientType("WalkIn")}
            >
              <IconWalk className="h-4 w-4 mr-2" />
              {t("walkInCustomer")}
            </Button>
          </div>

          {/* Full Name */}
          <Controller
            name="fullName"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>{t("fullName")}</FieldLabel>
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

          {/* Phone */}
          <Controller
            name="phone"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>{t("phone")}</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  placeholder="0901234567"
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Medical-only fields */}
          {patientType === "Medical" && (
            <>
              {/* Date of Birth */}
              <Controller
                name="dateOfBirth"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid || undefined}>
                    <FieldLabel>{t("dateOfBirth")}</FieldLabel>
                    <DatePicker
                      value={field.value ?? undefined}
                      onChange={field.onChange}
                      placeholder={t("dateOfBirth")}
                      className="w-full"
                    />
                    {fieldState.error && (
                      <FieldError>{fieldState.error.message}</FieldError>
                    )}
                  </Field>
                )}
              />

              {/* Gender */}
              <Controller
                name="gender"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid || undefined}>
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
                    {fieldState.error && (
                      <FieldError>{fieldState.error.message}</FieldError>
                    )}
                  </Field>
                )}
              />

              {/* Address */}
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

              {/* CCCD */}
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
            </>
          )}

          {/* Allergies section */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <FieldLabel>{t("allergies")}</FieldLabel>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() =>
                  append({ name: "", severity: "Mild" })
                }
              >
                <IconPlus className="h-3 w-3 mr-1" />
                {t("addAllergy")}
              </Button>
            </div>

            {fields.map((item, index) => (
              <div
                key={item.id}
                className="flex items-start gap-2 p-3 border bg-muted/30"
              >
                <div className="flex-1 space-y-2">
                  <Controller
                    name={`allergies.${index}.name`}
                    control={form.control}
                    render={({ field, fieldState }) => (
                      <Field data-invalid={fieldState.invalid || undefined}>
                        <Input
                          {...field}
                          placeholder={t("allergyName")}
                          aria-invalid={fieldState.invalid || undefined}
                        />
                        {fieldState.error && (
                          <FieldError>{fieldState.error.message}</FieldError>
                        )}
                      </Field>
                    )}
                  />

                  <Controller
                    name={`allergies.${index}.severity`}
                    control={form.control}
                    render={({ field }) => (
                      <Select
                        value={field.value}
                        onValueChange={field.onChange}
                      >
                        <SelectTrigger>
                          <SelectValue placeholder={t("severity")} />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Mild">{t("mild")}</SelectItem>
                          <SelectItem value="Moderate">
                            {t("moderate")}
                          </SelectItem>
                          <SelectItem value="Severe">
                            {t("severe")}
                          </SelectItem>
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => remove(index)}
                  className="text-destructive hover:text-destructive mt-1"
                >
                  <IconTrash className="h-4 w-4" />
                </Button>
              </div>
            ))}
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={registerMutation.isPending}>
              {registerMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {t("register")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
