import { useState } from "react"
import { Controller, useFormContext } from "react-hook-form"
import { useTranslation } from "react-i18next"
import { IconChevronDown } from "@tabler/icons-react"
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/shared/components/Collapsible"
import { Textarea } from "@/shared/components/Textarea"
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
  const [open, setOpen] = useState(true)

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger className="flex w-full items-center justify-between rounded-lg border bg-card p-4 text-left hover:bg-accent/50 transition-colors">
        <h2 className="text-xl font-semibold">{t("intake.lifestyle.title")}</h2>
        <IconChevronDown
          className={`h-5 w-5 text-muted-foreground transition-transform ${open ? "rotate-180" : ""}`}
        />
      </CollapsibleTrigger>
      <CollapsibleContent className="rounded-lg border border-t-0 p-4">
        <div className="grid gap-4 lg:grid-cols-2">
          {/* Thoi gian su dung man hinh */}
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
                />
              </Field>
            )}
          />

          {/* Môi trường làm việc */}
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
                    <SelectValue />
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

          {/* Sử dụng kính áp tròng */}
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
                    <SelectValue />
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

          {/* Ghi chú */}
          <Controller
            name="lifestyleNotes"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="lifestyleNotes">{t("intake.lifestyle.notes")}</FieldLabel>
                <Textarea
                  {...field}
                  id="lifestyleNotes"
                  maxLength={2000}
                  rows={3}
                />
              </Field>
            )}
          />
        </div>
      </CollapsibleContent>
    </Collapsible>
  )
}
