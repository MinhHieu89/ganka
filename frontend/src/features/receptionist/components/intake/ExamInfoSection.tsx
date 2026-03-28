import { Controller, useFormContext } from "react-hook-form"
import { useTranslation } from "react-i18next"
import { IconStethoscope } from "@tabler/icons-react"
import { Textarea } from "@/shared/components/Textarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import type { IntakeFormValues } from "@/features/receptionist/schemas/intake-form.schema"

const MAX_REASON_LENGTH = 500

export function ExamInfoSection() {
  const { control, watch } = useFormContext<IntakeFormValues>()
  const { t } = useTranslation("patient")
  const reasonValue = watch("reason") ?? ""

  return (
    <section>
      <h2 className="flex items-center gap-2 border-b pb-3 text-xl font-semibold">
        <IconStethoscope className="h-5 w-5 text-primary" />
        {t("intake.exam.title")}
      </h2>
      <div className="pt-4">
        <Controller
          name="reason"
          control={control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor="reason" required>
                {t("intake.exam.reason")}
              </FieldLabel>
              <Textarea
                {...field}
                id="reason"
                maxLength={MAX_REASON_LENGTH}
                rows={3}
                placeholder={t("intake.exam.reasonPlaceholder")}
                aria-invalid={fieldState.invalid || undefined}
              />
              <div className="flex items-center justify-between">
                {fieldState.error ? (
                  <FieldError>{fieldState.error.message}</FieldError>
                ) : (
                  <span className="text-xs text-muted-foreground">
                    {t("intake.exam.reasonHelp")}
                  </span>
                )}
                <span className="text-xs text-muted-foreground">
                  {reasonValue.length}/{MAX_REASON_LENGTH}
                </span>
              </div>
            </Field>
          )}
        />
      </div>
    </section>
  )
}
