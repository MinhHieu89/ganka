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
import { Field, FieldLabel } from "@/shared/components/Field"
import type { IntakeFormValues } from "@/features/receptionist/schemas/intake-form.schema"

export function MedicalHistorySection() {
  const { control } = useFormContext<IntakeFormValues>()
  const { t } = useTranslation("patient")
  const [open, setOpen] = useState(true)

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger className="flex w-full items-center justify-between rounded-lg border bg-card p-4 text-left hover:bg-accent/50 transition-colors">
        <h2 className="text-xl font-semibold">{t("intake.history.title")}</h2>
        <IconChevronDown
          className={`h-5 w-5 text-muted-foreground transition-transform ${open ? "rotate-180" : ""}`}
        />
      </CollapsibleTrigger>
      <CollapsibleContent className="rounded-lg border border-t-0 p-4">
        <div className="flex flex-col gap-4">
          {/* Tiền sử bệnh mắt */}
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
                />
              </Field>
            )}
          />

          {/* Tiền sử bệnh toàn thân */}
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
                />
              </Field>
            )}
          />

          {/* Thuốc đang dùng */}
          <Controller
            name="currentMedications"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="currentMedications">
                  {t("intake.history.medications")}
                </FieldLabel>
                <Textarea
                  {...field}
                  id="currentMedications"
                  maxLength={2000}
                  rows={2}
                />
              </Field>
            )}
          />
        </div>
      </CollapsibleContent>
    </Collapsible>
  )
}
