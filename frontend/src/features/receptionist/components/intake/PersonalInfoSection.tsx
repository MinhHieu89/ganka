import { useState, useEffect, useDeferredValue } from "react"
import { Controller, useFormContext } from "react-hook-form"
import { useTranslation } from "react-i18next"
import { IconUser, IconAlertTriangle } from "@tabler/icons-react"
import { Link } from "@tanstack/react-router"
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
import { toLocalDateString } from "@/shared/lib/format-date"
import { usePatientSearch } from "@/features/patient/hooks/usePatientSearch"
import type { IntakeFormValues } from "@/features/receptionist/schemas/intake-form.schema"

export function PersonalInfoSection() {
  const { control } = useFormContext<IntakeFormValues>()
  const { t } = useTranslation("patient")

  return (
    <section>
      <h2 className="flex items-center gap-2 border-b pb-3 text-xl font-semibold">
        <IconUser className="h-5 w-5 text-primary" />
        {t("intake.personal.title")}
      </h2>
      <div className="grid gap-4 pt-4 lg:grid-cols-3">
        {/* Row 1: Họ và tên (2 cols) + Giới tính (1 col) */}
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
                  placeholder={t("intake.personal.fullNamePlaceholder")}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />
        </div>

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

        {/* Row 2: Ngày sinh + SĐT + Email */}
        <Controller
          name="dateOfBirth"
          control={control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel required>{t("dateOfBirth")}</FieldLabel>
              <DatePicker
                value={field.value ? new Date(field.value) : undefined}
                onChange={(date) =>
                  field.onChange(date ? toLocalDateString(date) : "")
                }
                className="w-full"
              />
              {fieldState.error && (
                <FieldError>{fieldState.error.message}</FieldError>
              )}
            </Field>
          )}
        />

        <PhoneFieldWithDuplicateCheck control={control} />

        <Controller
          name="email"
          control={control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor="email">{t("email")}</FieldLabel>
              <Input
                {...field}
                id="email"
                type="email"
              />
              {fieldState.error && (
                <FieldError>{fieldState.error.message}</FieldError>
              )}
            </Field>
          )}
        />

        {/* Row 3: Địa chỉ (full width) */}
        <div className="lg:col-span-3">
          <Controller
            name="address"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="address">{t("address")}</FieldLabel>
                <Input
                  {...field}
                  id="address"
                  maxLength={500}
                />
              </Field>
            )}
          />
        </div>

        {/* Row 4: Nghề nghiệp + CCCD */}
        <Controller
          name="occupation"
          control={control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="occupation">{t("occupation")}</FieldLabel>
              <Input
                {...field}
                id="occupation"
                maxLength={200}
                placeholder={t("intake.personal.occupationPlaceholder")}
              />
            </Field>
          )}
        />

        <Controller
          name="cccd"
          control={control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="cccd">{t("cccd")}</FieldLabel>
              <Input
                {...field}
                id="cccd"
                maxLength={12}
              />
            </Field>
          )}
        />
      </div>
    </section>
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
          <div className="lg:col-span-1">
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
            </Field>
            {match && !fieldState.error && (
              <div className="mt-2 flex items-center gap-2 rounded-md bg-amber-50 border border-amber-200 px-3 py-2 text-sm text-amber-800">
                <IconAlertTriangle className="h-4 w-4 shrink-0" />
                <span className="flex-1">
                  {t("intake.phoneDuplicate", { phone: field.value, name: match.fullName, code: match.patientCode })}
                </span>
                <Link
                  to="/patients/$patientId"
                  params={{ patientId: match.id }}
                  className="shrink-0 rounded-md border border-amber-300 px-3 py-1 text-xs font-medium hover:bg-amber-100"
                >
                  {t("intake.openOldRecord")}
                </Link>
              </div>
            )}
          </div>
        )
      }}
    />
  )
}
