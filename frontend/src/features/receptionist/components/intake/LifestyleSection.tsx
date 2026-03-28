import { Controller, useFormContext } from "react-hook-form"
import { useTranslation } from "react-i18next"
import { IconClock } from "@tabler/icons-react"
import { Input } from "@/shared/components/Input"
import { NumberInput } from "@/shared/components/NumberInput"
import { Field, FieldLabel } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import type { IntakeFormValues } from "@/features/receptionist/schemas/intake-form.schema"

export function LifestyleSection() {
  const { control } = useFormContext<IntakeFormValues>()
  const { t } = useTranslation("patient")

  return (
    <section>
      <h2 className="flex items-center gap-2 border-b pb-3 text-xl font-semibold">
        <IconClock className="h-5 w-5 text-primary" />
        {t("intake.lifestyle.title")}
        <span className="text-sm font-normal text-muted-foreground">
          ({t("intake.optional")})
        </span>
      </h2>
      <div className="grid gap-4 pt-4 lg:grid-cols-3">
        <Controller
          name="screenTimeHours"
          control={control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="screenTimeHours">
                {t("intake.lifestyle.screenTime")}
              </FieldLabel>
              <NumberInput
                {...field}
                id="screenTimeHours"
                min={0}
                max={24}
                step={0.5}
                placeholder={t("intake.lifestyle.screenTimePlaceholder")}
              />
            </Field>
          )}
        />

        <Controller
          name="workEnvironment"
          control={control}
          render={({ field }) => (
            <Field>
              <FieldLabel>{t("intake.lifestyle.workEnvironment")}</FieldLabel>
              <Select
                value={field.value ?? ""}
                onValueChange={(v) => field.onChange(v || undefined)}
              >
                <SelectTrigger>
                  <SelectValue placeholder={t("intake.lifestyle.selectPlaceholder")} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="office">{t("intake.lifestyle.workOptions.office")}</SelectItem>
                  <SelectItem value="outdoor">{t("intake.lifestyle.workOptions.outdoor")}</SelectItem>
                  <SelectItem value="mixed">{t("intake.lifestyle.workOptions.factory")}</SelectItem>
                  <SelectItem value="other">{t("intake.lifestyle.workOptions.other")}</SelectItem>
                </SelectContent>
              </Select>
            </Field>
          )}
        />

        <Controller
          name="contactLensUsage"
          control={control}
          render={({ field }) => (
            <Field>
              <FieldLabel>{t("intake.lifestyle.contactLens")}</FieldLabel>
              <Select
                value={field.value ?? ""}
                onValueChange={(v) => field.onChange(v || undefined)}
              >
                <SelectTrigger>
                  <SelectValue placeholder={t("intake.lifestyle.selectPlaceholder")} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">{t("intake.lifestyle.contactLensOptions.none")}</SelectItem>
                  <SelectItem value="soft">{t("intake.lifestyle.contactLensOptions.daily")}</SelectItem>
                  <SelectItem value="rgp">{t("intake.lifestyle.contactLensOptions.occasional")}</SelectItem>
                  <SelectItem value="ortho_k">Ortho-K</SelectItem>
                  <SelectItem value="other">{t("intake.lifestyle.contactLensOptions.other")}</SelectItem>
                </SelectContent>
              </Select>
            </Field>
          )}
        />

        <div className="lg:col-span-3">
          <Controller
            name="lifestyleNotes"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="lifestyleNotes">{t("intake.lifestyle.notes")}</FieldLabel>
                <Input
                  {...field}
                  id="lifestyleNotes"
                  maxLength={2000}
                  placeholder={t("intake.lifestyle.notesPlaceholder")}
                />
              </Field>
            )}
          />
        </div>
      </div>
    </section>
  )
}
