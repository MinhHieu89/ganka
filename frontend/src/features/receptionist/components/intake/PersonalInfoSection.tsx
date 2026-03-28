import { useState, useEffect, useDeferredValue } from "react"
import { Controller, useFormContext } from "react-hook-form"
import { useTranslation } from "react-i18next"
import { IconChevronDown, IconAlertTriangle } from "@tabler/icons-react"
import { Link } from "@tanstack/react-router"
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/shared/components/Collapsible"
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
import { usePatientSearch } from "@/features/patient/hooks/usePatientSearch"
import type { IntakeFormValues } from "@/features/receptionist/schemas/intake-form.schema"

export function PersonalInfoSection() {
  const { control } = useFormContext<IntakeFormValues>()
  const { t } = useTranslation("patient")
  const [open, setOpen] = useState(true)

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger className="flex w-full items-center justify-between rounded-lg border bg-card p-4 text-left hover:bg-accent/50 transition-colors">
        <h2 className="text-xl font-semibold">{t("intake.personal.title")}</h2>
        <IconChevronDown
          className={`h-5 w-5 text-muted-foreground transition-transform ${open ? "rotate-180" : ""}`}
        />
      </CollapsibleTrigger>
      <CollapsibleContent className="rounded-lg border border-t-0 p-4">
        <div className="grid gap-4 lg:grid-cols-3">
          {/* Họ và tên - spans 2 cols */}
          <div className="lg:col-span-2">
            <Controller
              name="fullName"
              control={control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel required htmlFor="fullName">
                    {t("fullName")}
                  </FieldLabel>
                  <Input
                    {...field}
                    id="fullName"
                    autoFocus
                    maxLength={200}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error.message}</FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          {/* Giới tính */}
          <Controller
            name="gender"
            control={control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel required>{t("gender")}</FieldLabel>
                <Select value={field.value ?? ""} onValueChange={field.onChange}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="0">{t("male")}</SelectItem>
                    <SelectItem value="1">{t("female")}</SelectItem>
                    <SelectItem value="2">{t("other")}</SelectItem>
                  </SelectContent>
                </Select>
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Số điện thoại with duplicate check */}
          <PhoneFieldWithDuplicateCheck control={control} />

          {/* Ngày sinh */}
          <Controller
            name="dateOfBirth"
            control={control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel required>{t("dateOfBirth")}</FieldLabel>
                <DatePicker
                  value={field.value ? new Date(field.value) : undefined}
                  onChange={(date) =>
                    field.onChange(date ? date.toISOString().split("T")[0] : "")
                  }
                  className="w-full"
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Địa chỉ */}
          <Controller
            name="address"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="address">{t("address")}</FieldLabel>
                <Input {...field} id="address" maxLength={500} />
              </Field>
            )}
          />

          {/* Số CCCD */}
          <Controller
            name="cccd"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="cccd">{t("cccd")}</FieldLabel>
                <Input {...field} id="cccd" />
              </Field>
            )}
          />

          {/* Email */}
          <Controller
            name="email"
            control={control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor="email">{t("email")}</FieldLabel>
                <Input {...field} id="email" type="email" />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Nghề nghiệp */}
          <Controller
            name="occupation"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="occupation">{t("occupation")}</FieldLabel>
                <Input {...field} id="occupation" maxLength={200} />
              </Field>
            )}
          />
        </div>
      </CollapsibleContent>
    </Collapsible>
  )
}

function PhoneFieldWithDuplicateCheck({
  control,
}: {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  control: any
}) {
  const { t } = useTranslation("patient")
  const [phoneForSearch, setPhoneForSearch] = useState("")
  const deferredPhone = useDeferredValue(phoneForSearch)

  const { data: duplicates } = usePatientSearch(deferredPhone, {
    enabled: /^0\d{9,10}$/.test(deferredPhone),
  })

  const match = duplicates && duplicates.length > 0 ? duplicates[0] : null

  return (
    <Controller
      name="phone"
      control={control}
      render={({ field, fieldState }) => {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        useEffect(() => {
          const timer = setTimeout(() => {
            setPhoneForSearch(field.value || "")
          }, 500)
          return () => clearTimeout(timer)
        }, [field.value])

        return (
          <Field data-invalid={fieldState.invalid || undefined}>
            <FieldLabel required htmlFor="phone">
              {t("phone")}
            </FieldLabel>
            <Input
              {...field}
              id="phone"
              aria-invalid={fieldState.invalid || undefined}
            />
            {fieldState.error && (
              <FieldError>{fieldState.error.message}</FieldError>
            )}
            {match && !fieldState.error && (
              <div className="flex items-center gap-2 rounded-md bg-amber-50 border border-amber-200 px-3 py-2 text-sm text-amber-800">
                <IconAlertTriangle className="h-4 w-4 shrink-0" />
                <span>
                  {t("intake.phoneDuplicate", { phone: field.value, name: match.fullName, code: match.patientCode })}
                </span>
                <Link
                  to="/patients/$patientId"
                  params={{ patientId: match.id }}
                  className="ml-auto shrink-0 font-medium underline hover:no-underline"
                >
                  {t("intake.openOldRecord")}
                </Link>
              </div>
            )}
          </Field>
        )
      }}
    />
  )
}
