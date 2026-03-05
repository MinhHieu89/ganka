import { useEffect, useCallback, useRef } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import { useUpdateDryEye } from "../api/clinical-api"
import type { DryEyeAssessmentDto } from "../api/clinical-api"

const optionalNumber = z
  .union([z.string(), z.number(), z.null()])
  .transform((v) => {
    if (v === null || v === "" || v === undefined) return null
    const n = Number(v)
    return isNaN(n) ? null : n
  })
  .nullable()
  .optional()

const dryEyeSchema = z.object({
  odTbut: optionalNumber,
  osTbut: optionalNumber,
  odSchirmer: optionalNumber,
  osSchirmer: optionalNumber,
  odMeibomianGrading: optionalNumber,
  osMeibomianGrading: optionalNumber,
  odTearMeniscus: optionalNumber,
  osTearMeniscus: optionalNumber,
  odStaining: optionalNumber,
  osStaining: optionalNumber,
})

type DryEyeFormValues = z.infer<typeof dryEyeSchema>

interface DryEyeField {
  key: string
  label: string
  min: number
  max: number
  step: number
  unit?: string
}

const DRY_EYE_FIELDS: DryEyeField[] = [
  { key: "Tbut", label: "tbut", min: 0, max: 30, step: 1, unit: "s" },
  { key: "Schirmer", label: "schirmer", min: 0, max: 35, step: 1, unit: "mm" },
  { key: "MeibomianGrading", label: "meibomianGrading", min: 0, max: 3, step: 1 },
  { key: "TearMeniscus", label: "tearMeniscus", min: 0, max: 2, step: 0.1, unit: "mm" },
  { key: "Staining", label: "staining", min: 0, max: 5, step: 1 },
]

function toFormValue(v: number | null | undefined): string {
  if (v === null || v === undefined) return ""
  return String(v)
}

interface DryEyeFormProps {
  visitId: string
  data: DryEyeAssessmentDto | undefined
  disabled: boolean
}

export function DryEyeForm({ visitId, data, disabled }: DryEyeFormProps) {
  const { t } = useTranslation("clinical")
  const updateMutation = useUpdateDryEye()
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const form = useForm<DryEyeFormValues>({
    resolver: zodResolver(dryEyeSchema),
    defaultValues: {
      odTbut: data?.odTbut ?? null,
      osTbut: data?.osTbut ?? null,
      odSchirmer: data?.odSchirmer ?? null,
      osSchirmer: data?.osSchirmer ?? null,
      odMeibomianGrading: data?.odMeibomianGrading ?? null,
      osMeibomianGrading: data?.osMeibomianGrading ?? null,
      odTearMeniscus: data?.odTearMeniscus ?? null,
      osTearMeniscus: data?.osTearMeniscus ?? null,
      odStaining: data?.odStaining ?? null,
      osStaining: data?.osStaining ?? null,
    },
  })

  // Reset form when data changes (e.g., after mutation invalidation)
  useEffect(() => {
    if (data) {
      form.reset({
        odTbut: data.odTbut ?? null,
        osTbut: data.osTbut ?? null,
        odSchirmer: data.odSchirmer ?? null,
        osSchirmer: data.osSchirmer ?? null,
        odMeibomianGrading: data.odMeibomianGrading ?? null,
        osMeibomianGrading: data.osMeibomianGrading ?? null,
        odTearMeniscus: data.odTearMeniscus ?? null,
        osTearMeniscus: data.osTearMeniscus ?? null,
        odStaining: data.odStaining ?? null,
        osStaining: data.osStaining ?? null,
      })
    }
  }, [data, form])

  const saveData = useCallback(
    (values: DryEyeFormValues) => {
      const hasData = Object.values(values).some(
        (v) => v !== null && v !== undefined,
      )
      if (!hasData) return

      updateMutation.mutate(
        {
          visitId,
          ...values,
        },
        {
          onSuccess: () => {
            toast.success(t("dryEye.saved"))
          },
          onError: () => {
            toast.error(t("dryEye.saveFailed"))
          },
        },
      )
    },
    [visitId, updateMutation, t],
  )

  const handleBlur = useCallback(() => {
    if (disabled) return
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      const values = form.getValues()
      const parsed = dryEyeSchema.safeParse(values)
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
    name: keyof DryEyeFormValues,
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
      <div className="grid grid-cols-[80px_1fr_1fr] gap-x-3 gap-y-1 items-center">
        {/* Header row */}
        <div />
        <Label className="text-center font-semibold text-sm">
          {t("refraction.od")}
        </Label>
        <Label className="text-center font-semibold text-sm">
          {t("refraction.os")}
        </Label>

        {/* Dry eye fields */}
        {DRY_EYE_FIELDS.map((field) => (
          <div key={field.key} className="contents">
            <Label className="text-xs text-muted-foreground">
              {t(`dryEye.${field.label}`)}
            </Label>
            {renderNumberInput(`od${field.key}` as keyof DryEyeFormValues, field)}
            {renderNumberInput(`os${field.key}` as keyof DryEyeFormValues, field)}
          </div>
        ))}
      </div>
    </div>
  )
}
