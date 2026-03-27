import { useState } from "react"
import { Controller, useFormContext } from "react-hook-form"
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
  const [open, setOpen] = useState(true)

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger className="flex w-full items-center justify-between rounded-lg border bg-card p-4 text-left hover:bg-accent/50 transition-colors">
        <h2 className="text-xl font-semibold">Tien su benh</h2>
        <IconChevronDown
          className={`h-5 w-5 text-muted-foreground transition-transform ${open ? "rotate-180" : ""}`}
        />
      </CollapsibleTrigger>
      <CollapsibleContent className="rounded-lg border border-t-0 p-4">
        <div className="flex flex-col gap-4">
          {/* Tien su benh mat */}
          <Controller
            name="ocularHistory"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="ocularHistory">
                  Tien su benh mat
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

          {/* Tien su benh toan than */}
          <Controller
            name="systemicHistory"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="systemicHistory">
                  Tien su benh toan than
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

          {/* Thuoc dang dung */}
          <Controller
            name="currentMedications"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="currentMedications">
                  Thuoc dang dung
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
