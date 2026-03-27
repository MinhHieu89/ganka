import { useState } from "react"
import { Controller, useFormContext } from "react-hook-form"
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
  const [open, setOpen] = useState(true)

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger className="flex w-full items-center justify-between rounded-lg border bg-card p-4 text-left hover:bg-accent/50 transition-colors">
        <h2 className="text-xl font-semibold">Lifestyle</h2>
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
                  Thoi gian su dung man hinh (gio/ngay)
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

          {/* Moi truong lam viec */}
          <Controller
            name="workEnvironment"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel>Moi truong lam viec</FieldLabel>
                <Select
                  value={field.value ?? ""}
                  onValueChange={(v) => field.onChange(v || undefined)}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="office">Van phong</SelectItem>
                    <SelectItem value="outdoor">Ngoai troi</SelectItem>
                    <SelectItem value="mixed">Nha may</SelectItem>
                    <SelectItem value="other">Khac</SelectItem>
                  </SelectContent>
                </Select>
              </Field>
            )}
          />

          {/* Su dung kinh ap trong */}
          <Controller
            name="contactLensUsage"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel>Su dung kinh ap trong</FieldLabel>
                <Select
                  value={field.value ?? ""}
                  onValueChange={(v) => field.onChange(v || undefined)}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">Khong</SelectItem>
                    <SelectItem value="soft">Hang ngay</SelectItem>
                    <SelectItem value="rgp">Thoi diem</SelectItem>
                    <SelectItem value="ortho_k">Ortho-K</SelectItem>
                    <SelectItem value="other">Khac</SelectItem>
                  </SelectContent>
                </Select>
              </Field>
            )}
          />

          {/* Ghi chu */}
          <Controller
            name="lifestyleNotes"
            control={control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="lifestyleNotes">Ghi chu</FieldLabel>
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
