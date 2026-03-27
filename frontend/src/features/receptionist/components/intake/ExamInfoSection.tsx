import { useState } from "react"
import { Controller, useFormContext } from "react-hook-form"
import { IconChevronDown } from "@tabler/icons-react"
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/shared/components/Collapsible"
import { Textarea } from "@/shared/components/Textarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import type { IntakeFormValues } from "@/features/receptionist/schemas/intake-form.schema"

const MAX_REASON_LENGTH = 500

export function ExamInfoSection() {
  const { control, watch } = useFormContext<IntakeFormValues>()
  const [open, setOpen] = useState(true)
  const reasonValue = watch("reason") ?? ""

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger className="flex w-full items-center justify-between rounded-lg border bg-card p-4 text-left hover:bg-accent/50 transition-colors">
        <h2 className="text-xl font-semibold">Thong tin kham</h2>
        <IconChevronDown
          className={`h-5 w-5 text-muted-foreground transition-transform ${open ? "rotate-180" : ""}`}
        />
      </CollapsibleTrigger>
      <CollapsibleContent className="rounded-lg border border-t-0 p-4">
        <Controller
          name="reason"
          control={control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor="reason">Ly do kham</FieldLabel>
              <Textarea
                {...field}
                id="reason"
                maxLength={MAX_REASON_LENGTH}
                rows={3}
                aria-invalid={fieldState.invalid || undefined}
              />
              <div className="flex items-center justify-between">
                {fieldState.error ? (
                  <FieldError>{fieldState.error.message}</FieldError>
                ) : (
                  <span />
                )}
                <span className="text-xs text-muted-foreground">
                  {reasonValue.length}/{MAX_REASON_LENGTH}
                </span>
              </div>
            </Field>
          )}
        />
      </CollapsibleContent>
    </Collapsible>
  )
}
