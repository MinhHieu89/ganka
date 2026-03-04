import { useEffect, useCallback, useRef } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { useUpdateRefraction } from "../api/clinical-api"
import type { RefractionDto } from "../api/clinical-api"

const optionalNumber = z
  .union([z.string(), z.number(), z.null()])
  .transform((v) => {
    if (v === null || v === "" || v === undefined) return null
    const n = Number(v)
    return isNaN(n) ? null : n
  })
  .nullable()
  .optional()

const refractionSchema = z.object({
  odSph: optionalNumber,
  odCyl: optionalNumber,
  odAxis: optionalNumber,
  odAdd: optionalNumber,
  odPd: optionalNumber,
  osSph: optionalNumber,
  osCyl: optionalNumber,
  osAxis: optionalNumber,
  osAdd: optionalNumber,
  osPd: optionalNumber,
  ucvaOd: optionalNumber,
  ucvaOs: optionalNumber,
  bcvaOd: optionalNumber,
  bcvaOs: optionalNumber,
  iopOd: optionalNumber,
  iopOs: optionalNumber,
  iopMethod: optionalNumber,
  axialLengthOd: optionalNumber,
  axialLengthOs: optionalNumber,
})

type RefractionFormValues = z.infer<typeof refractionSchema>

interface RefractionField {
  key: string
  label: string
  min?: number
  max?: number
  step?: number
  unit?: string
}

const EYE_FIELDS: RefractionField[] = [
  { key: "Sph", label: "sph", min: -30, max: 30, step: 0.25, unit: "D" },
  { key: "Cyl", label: "cyl", min: -10, max: 10, step: 0.25, unit: "D" },
  { key: "Axis", label: "axis", min: 1, max: 180, step: 1, unit: "deg" },
  { key: "Add", label: "add", min: 0.25, max: 4, step: 0.25, unit: "D" },
  { key: "Pd", label: "pd", min: 20, max: 80, step: 0.5, unit: "mm" },
]

const SHARED_FIELDS: { key: string; label: string; min: number; max: number; step: number; unit?: string }[] = [
  { key: "ucva", label: "ucva", min: 0.01, max: 2.0, step: 0.01 },
  { key: "bcva", label: "bcva", min: 0.01, max: 2.0, step: 0.01 },
  { key: "iop", label: "iop", min: 1, max: 60, step: 1, unit: "mmHg" },
  { key: "axialLength", label: "axialLength", min: 15.0, max: 40.0, step: 0.01, unit: "mm" },
]

const IOP_METHODS = [
  { value: "0", label: "goldmann" },
  { value: "1", label: "nonContact" },
  { value: "2", label: "icare" },
  { value: "3", label: "tonopen" },
  { value: "4", label: "other" },
]

function toFormValue(v: number | null | undefined): string {
  if (v === null || v === undefined) return ""
  return String(v)
}

interface RefractionFormProps {
  visitId: string
  refractionType: number
  data: RefractionDto | undefined
  disabled: boolean
}

export function RefractionForm({
  visitId,
  refractionType,
  data,
  disabled,
}: RefractionFormProps) {
  const { t } = useTranslation("clinical")
  const updateMutation = useUpdateRefraction()
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const form = useForm<RefractionFormValues>({
    resolver: zodResolver(refractionSchema),
    defaultValues: {
      odSph: data?.odSph ?? null,
      odCyl: data?.odCyl ?? null,
      odAxis: data?.odAxis ?? null,
      odAdd: data?.odAdd ?? null,
      odPd: data?.odPd ?? null,
      osSph: data?.osSph ?? null,
      osCyl: data?.osCyl ?? null,
      osAxis: data?.osAxis ?? null,
      osAdd: data?.osAdd ?? null,
      osPd: data?.osPd ?? null,
      ucvaOd: data?.ucvaOd ?? null,
      ucvaOs: data?.ucvaOs ?? null,
      bcvaOd: data?.bcvaOd ?? null,
      bcvaOs: data?.bcvaOs ?? null,
      iopOd: data?.iopOd ?? null,
      iopOs: data?.iopOs ?? null,
      iopMethod: data?.iopMethod ?? null,
      axialLengthOd: data?.axialLengthOd ?? null,
      axialLengthOs: data?.axialLengthOs ?? null,
    },
  })

  // Reset form when data changes (e.g., after mutation invalidation)
  useEffect(() => {
    if (data) {
      form.reset({
        odSph: data.odSph ?? null,
        odCyl: data.odCyl ?? null,
        odAxis: data.odAxis ?? null,
        odAdd: data.odAdd ?? null,
        odPd: data.odPd ?? null,
        osSph: data.osSph ?? null,
        osCyl: data.osCyl ?? null,
        osAxis: data.osAxis ?? null,
        osAdd: data.osAdd ?? null,
        osPd: data.osPd ?? null,
        ucvaOd: data.ucvaOd ?? null,
        ucvaOs: data.ucvaOs ?? null,
        bcvaOd: data.bcvaOd ?? null,
        bcvaOs: data.bcvaOs ?? null,
        iopOd: data.iopOd ?? null,
        iopOs: data.iopOs ?? null,
        iopMethod: data.iopMethod ?? null,
        axialLengthOd: data.axialLengthOd ?? null,
        axialLengthOs: data.axialLengthOs ?? null,
      })
    }
  }, [data, form])

  const saveData = useCallback(
    (values: RefractionFormValues) => {
      // Only save if at least one field has data
      const hasData = Object.values(values).some(
        (v) => v !== null && v !== undefined,
      )
      if (!hasData) return

      updateMutation.mutate(
        {
          visitId,
          refractionType,
          ...values,
        },
        {
          onSuccess: () => {
            toast.success(t("visit.refractionSaved"))
          },
        },
      )
    },
    [visitId, refractionType, updateMutation, t],
  )

  const handleBlur = useCallback(() => {
    if (disabled) return
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      const values = form.getValues()
      // Parse through schema to get clean values
      const parsed = refractionSchema.safeParse(values)
      if (parsed.success) {
        saveData(parsed.data)
      }
    }, 500)
  }, [disabled, form, saveData])

  useEffect(() => {
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current)
    }
  }, [])

  const renderNumberInput = (
    name: keyof RefractionFormValues,
    field: { min?: number; max?: number; step?: number; unit?: string },
  ) => {
    const value = form.watch(name)
    return (
      <div className="flex items-center gap-1">
        <Input
          type="number"
          className="h-8 text-sm tabular-nums"
          min={field.min}
          max={field.max}
          step={field.step}
          value={toFormValue(value as number | null | undefined)}
          disabled={disabled}
          onChange={(e) => {
            const val = e.target.value
            form.setValue(name, val === "" ? null : Number(val))
          }}
          onBlur={handleBlur}
        />
        {field.unit && (
          <span className="text-xs text-muted-foreground whitespace-nowrap">
            {field.unit}
          </span>
        )}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {/* OD/OS side-by-side grid for eye-specific fields */}
      <div className="grid grid-cols-[80px_1fr_1fr] gap-x-3 gap-y-1 items-center">
        {/* Header row */}
        <div />
        <Label className="text-center font-semibold text-sm">
          {t("refraction.od")}
        </Label>
        <Label className="text-center font-semibold text-sm">
          {t("refraction.os")}
        </Label>

        {/* Eye-specific fields */}
        {EYE_FIELDS.map((field) => (
          <div key={field.key} className="contents">
            <Label className="text-xs text-muted-foreground">
              {t(`refraction.${field.label}`)}
            </Label>
            {renderNumberInput(`od${field.key}` as keyof RefractionFormValues, field)}
            {renderNumberInput(`os${field.key}` as keyof RefractionFormValues, field)}
          </div>
        ))}
      </div>

      {/* Shared context fields below */}
      <div className="grid grid-cols-[80px_1fr_1fr] gap-x-3 gap-y-1 items-center border-t pt-3">
        <div />
        <Label className="text-center text-xs font-medium text-muted-foreground">OD</Label>
        <Label className="text-center text-xs font-medium text-muted-foreground">OS</Label>

        {SHARED_FIELDS.map((field) => {
          const odKey = `${field.key}Od` as keyof RefractionFormValues
          const osKey = `${field.key}Os` as keyof RefractionFormValues
          return (
            <div key={field.key} className="contents">
              <Label className="text-xs text-muted-foreground">
                {t(`refraction.${field.label}`)}
              </Label>
              {renderNumberInput(odKey, field)}
              {renderNumberInput(osKey, field)}
            </div>
          )
        })}
      </div>

      {/* IOP Method */}
      <div className="flex items-center gap-3 border-t pt-3">
        <Label className="text-xs text-muted-foreground whitespace-nowrap">
          {t("refraction.iopMethod")}
        </Label>
        <Select
          value={toFormValue(form.watch("iopMethod") as number | null | undefined)}
          onValueChange={(v) => {
            form.setValue("iopMethod", v === "" ? null : Number(v))
            handleBlur()
          }}
          disabled={disabled}
        >
          <SelectTrigger className="h-8 w-48">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {IOP_METHODS.map((m) => (
              <SelectItem key={m.value} value={m.value}>
                {t(`refraction.iopMethods.${m.label}`)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </div>
  )
}
