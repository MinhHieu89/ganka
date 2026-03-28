import { Controller, useFormContext } from "react-hook-form"
import { useTranslation } from "react-i18next"
import { IconCirclePlus } from "@tabler/icons-react"
import { Textarea } from "@/shared/components/Textarea"
import { Input } from "@/shared/components/Input"
import { Field, FieldLabel } from "@/shared/components/Field"
import type { IntakeFormValues } from "@/features/receptionist/schemas/intake-form.schema"

export function MedicalHistorySection() {
  const { control } = useFormContext<IntakeFormValues>()
  const { t } = useTranslation("patient")

  return (
    <section>
      <h2 className="flex items-center gap-2 border-b pb-3 text-xl font-semibold">
        <IconCirclePlus className="h-5 w-5 text-primary" />
        {t("intake.history.title")}
        <span className="text-sm font-normal text-muted-foreground">
          ({t("intake.optional")})
        </span>
      </h2>
      <div className="grid gap-4 pt-4 lg:grid-cols-2">
        <Controller
          name="ocularHistory"
          control={control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="ocularHistory">
                {t("intake.history.ocular")}
              </FieldLabel>
              <Textarea
                {...field}
                id="ocularHistory"
                maxLength={2000}
                rows={3}
                placeholder={t("intake.history.ocularPlaceholder")}
              />
            </Field>
          )}
        />

        <Controller
          name="systemicHistory"
          control={control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="systemicHistory">
                {t("intake.history.systemic")}
              </FieldLabel>
              <Textarea
                {...field}
                id="systemicHistory"
                maxLength={2000}
                rows={3}
                placeholder={t("intake.history.systemicPlaceholder")}
              />
            </Field>
          )}
        />

        <Controller
          name="currentMedications"
          control={control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="currentMedications">
                {t("intake.history.medications")}
              </FieldLabel>
              <Input
                {...field}
                id="currentMedications"
                maxLength={2000}
                placeholder={t("intake.history.medicationsPlaceholder")}
              />
            </Field>
          )}
        />

        <Controller
          name="allergies"
          control={control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="allergies">
                {t("intake.history.allergies")}
              </FieldLabel>
              <Input
                {...field}
                id="allergies"
                maxLength={2000}
                placeholder={t("intake.history.allergiesPlaceholder")}
              />
            </Field>
          )}
        />
      </div>
    </section>
  )
}
