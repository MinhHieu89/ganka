import { useCallback } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { IconRefresh } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import { Textarea } from "@/shared/components/Textarea"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import {
  Collapsible,
  CollapsibleTrigger,
  CollapsibleContent,
} from "@/shared/components/Collapsible"
import { IconChevronDown } from "@tabler/icons-react"
import type { RefractionDto } from "../api/clinical-api"

const optionalDecimal = z
  .union([z.string(), z.number(), z.null()])
  .transform((v) => {
    if (v === null || v === "" || v === undefined) return null
    const n = Number(v)
    return isNaN(n) ? null : n
  })
  .nullable()
  .optional()

const optionalInt = z
  .union([z.string(), z.number(), z.null()])
  .transform((v) => {
    if (v === null || v === "" || v === undefined) return null
    const n = Number(v)
    return isNaN(n) ? null : Math.round(n)
  })
  .nullable()
  .optional()

const opticalRxSchema = z.object({
  // Distance Rx
  odSph: optionalDecimal,
  odCyl: optionalDecimal,
  odAxis: optionalInt,
  odAdd: optionalDecimal,
  osSph: optionalDecimal,
  osCyl: optionalDecimal,
  osAxis: optionalInt,
  osAdd: optionalDecimal,
  // Near Rx (overrides for bifocal/progressive)
  nearOdSph: optionalDecimal,
  nearOdCyl: optionalDecimal,
  nearOdAxis: optionalInt,
  nearOsSph: optionalDecimal,
  nearOsCyl: optionalDecimal,
  nearOsAxis: optionalInt,
  // PD
  farPd: optionalDecimal,
  nearPd: optionalDecimal,
  // Lens type: 0=SingleVision, 1=Bifocal, 2=Progressive, 3=Reading
  lensType: z.coerce.number().min(0).max(3).default(0),
  // Notes
  notes: z.string().nullable().optional(),
})

export type OpticalPrescriptionFormData = z.infer<typeof opticalRxSchema>

interface OpticalPrescriptionFormProps {
  onSubmit: (data: OpticalPrescriptionFormData) => void
  onCancel?: () => void
  defaultValues?: Partial<OpticalPrescriptionFormData>
  disabled?: boolean
  refractions?: RefractionDto[]
  isSubmitting?: boolean
}

function toFormValue(v: number | null | undefined): string {
  if (v === null || v === undefined) return ""
  return String(v)
}

const DISTANCE_FIELDS = [
  { key: "Sph", label: "sph", min: -30, max: 30, step: 0.25, unit: "D" },
  { key: "Cyl", label: "cyl", min: -10, max: 10, step: 0.25, unit: "D" },
  { key: "Axis", label: "axis", min: 0, max: 180, step: 1, unit: "deg" },
  { key: "Add", label: "add", min: 0.25, max: 4, step: 0.25, unit: "D" },
] as const

const NEAR_FIELDS = [
  { key: "Sph", label: "sph", min: -30, max: 30, step: 0.25, unit: "D" },
  { key: "Cyl", label: "cyl", min: -10, max: 10, step: 0.25, unit: "D" },
  { key: "Axis", label: "axis", min: 0, max: 180, step: 1, unit: "deg" },
] as const

const LENS_TYPES = [
  { value: "0", key: "singleVision" },
  { value: "1", key: "bifocal" },
  { value: "2", key: "progressive" },
  { value: "3", key: "reading" },
] as const

export function OpticalPrescriptionForm({
  onSubmit,
  onCancel,
  defaultValues,
  disabled = false,
  refractions = [],
  isSubmitting = false,
}: OpticalPrescriptionFormProps) {
  const { t } = useTranslation("clinical")
  const { t: tCommon } = useTranslation("common")

  const form = useForm<OpticalPrescriptionFormData>({
    resolver: zodResolver(opticalRxSchema),
    defaultValues: {
      odSph: defaultValues?.odSph ?? null,
      odCyl: defaultValues?.odCyl ?? null,
      odAxis: defaultValues?.odAxis ?? null,
      odAdd: defaultValues?.odAdd ?? null,
      osSph: defaultValues?.osSph ?? null,
      osCyl: defaultValues?.osCyl ?? null,
      osAxis: defaultValues?.osAxis ?? null,
      osAdd: defaultValues?.osAdd ?? null,
      nearOdSph: defaultValues?.nearOdSph ?? null,
      nearOdCyl: defaultValues?.nearOdCyl ?? null,
      nearOdAxis: defaultValues?.nearOdAxis ?? null,
      nearOsSph: defaultValues?.nearOsSph ?? null,
      nearOsCyl: defaultValues?.nearOsCyl ?? null,
      nearOsAxis: defaultValues?.nearOsAxis ?? null,
      farPd: defaultValues?.farPd ?? null,
      nearPd: defaultValues?.nearPd ?? null,
      lensType: defaultValues?.lensType ?? 0,
      notes: defaultValues?.notes ?? null,
    },
  })

  const autoFillFromRefraction = useCallback(() => {
    // Find manifest refractions (type === 0) for OD (eyeTag via data structure)
    // RefractionDto has combined OD/OS fields: odSph, osSph, etc.
    // type 0 = Manifest
    const manifest = refractions.find((r) => r.type === 0)
    if (!manifest) return

    form.setValue("odSph", manifest.odSph)
    form.setValue("odCyl", manifest.odCyl)
    form.setValue("odAxis", manifest.odAxis)
    form.setValue("odAdd", manifest.odAdd)
    form.setValue("osSph", manifest.osSph)
    form.setValue("osCyl", manifest.osCyl)
    form.setValue("osAxis", manifest.osAxis)
    form.setValue("osAdd", manifest.osAdd)

    // PD from manifest
    if (manifest.odPd != null && manifest.osPd != null) {
      // Average of both eyes for far PD
      form.setValue("farPd", Math.round(((manifest.odPd + manifest.osPd) / 2) * 10) / 10)
    } else if (manifest.odPd != null) {
      form.setValue("farPd", manifest.odPd)
    } else if (manifest.osPd != null) {
      form.setValue("farPd", manifest.osPd)
    }
  }, [refractions, form])

  const handleSubmit = form.handleSubmit((data) => {
    onSubmit(data)
  })

  const renderNumberInput = (
    name: keyof OpticalPrescriptionFormData,
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
    <form onSubmit={handleSubmit} className="space-y-4">
      {/* Auto-fill button */}
      {refractions.length > 0 && !disabled && (
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={autoFillFromRefraction}
        >
          <IconRefresh className="h-4 w-4 mr-1" />
          {t("prescription.autoFillRefraction")}
        </Button>
      )}

      {/* Distance Rx */}
      <div>
        <h4 className="text-sm font-medium mb-2">
          {t("prescription.distanceRx")}
        </h4>
        <div className="grid grid-cols-[60px_1fr_1fr_1fr_1fr] gap-x-3 gap-y-1 items-center">
          {/* Header row */}
          <div />
          {DISTANCE_FIELDS.map((f) => (
            <Label key={f.key} className="text-center text-xs text-muted-foreground font-medium">
              {t(`refraction.${f.label}`)}
            </Label>
          ))}

          {/* OD row */}
          <Label className="text-xs font-semibold">{t("refraction.od")}</Label>
          {DISTANCE_FIELDS.map((f) => (
            <div key={`od-${f.key}`}>
              {renderNumberInput(
                `od${f.key}` as keyof OpticalPrescriptionFormData,
                f,
              )}
            </div>
          ))}

          {/* OS row */}
          <Label className="text-xs font-semibold">{t("refraction.os")}</Label>
          {DISTANCE_FIELDS.map((f) => (
            <div key={`os-${f.key}`}>
              {renderNumberInput(
                `os${f.key}` as keyof OpticalPrescriptionFormData,
                f,
              )}
            </div>
          ))}
        </div>
      </div>

      {/* Near Rx (collapsible) */}
      <Collapsible>
        <CollapsibleTrigger asChild>
          <div className="flex items-center gap-2 cursor-pointer">
            <h4 className="text-sm font-medium">{t("prescription.nearRx")}</h4>
            <IconChevronDown className="h-3 w-3 shrink-0 transition-transform [[data-state=open]>&]:rotate-180" />
          </div>
        </CollapsibleTrigger>
        <CollapsibleContent>
          <div className="grid grid-cols-[60px_1fr_1fr_1fr] gap-x-3 gap-y-1 items-center mt-2">
            {/* Header row */}
            <div />
            {NEAR_FIELDS.map((f) => (
              <Label key={f.key} className="text-center text-xs text-muted-foreground font-medium">
                {t(`refraction.${f.label}`)}
              </Label>
            ))}

            {/* OD row */}
            <Label className="text-xs font-semibold">{t("refraction.od")}</Label>
            {NEAR_FIELDS.map((f) => (
              <div key={`near-od-${f.key}`}>
                {renderNumberInput(
                  `nearOd${f.key}` as keyof OpticalPrescriptionFormData,
                  f,
                )}
              </div>
            ))}

            {/* OS row */}
            <Label className="text-xs font-semibold">{t("refraction.os")}</Label>
            {NEAR_FIELDS.map((f) => (
              <div key={`near-os-${f.key}`}>
                {renderNumberInput(
                  `nearOs${f.key}` as keyof OpticalPrescriptionFormData,
                  f,
                )}
              </div>
            ))}
          </div>
        </CollapsibleContent>
      </Collapsible>

      {/* PD */}
      <div className="flex gap-4 items-end">
        <div className="space-y-1">
          <Label className="text-xs text-muted-foreground">
            {t("prescription.farPd")}
          </Label>
          <div className="flex items-center gap-1">
            <Input
              type="number"
              className="h-8 w-24 text-sm tabular-nums"
              min={20}
              max={80}
              step={0.5}
              value={toFormValue(form.watch("farPd") as number | null | undefined)}
              disabled={disabled}
              onChange={(e) => {
                const val = e.target.value
                form.setValue("farPd", val === "" ? null : Number(val))
              }}
            />
            <span className="text-xs text-muted-foreground">mm</span>
          </div>
        </div>
        <div className="space-y-1">
          <Label className="text-xs text-muted-foreground">
            {t("prescription.nearPd")}
          </Label>
          <div className="flex items-center gap-1">
            <Input
              type="number"
              className="h-8 w-24 text-sm tabular-nums"
              min={20}
              max={80}
              step={0.5}
              value={toFormValue(form.watch("nearPd") as number | null | undefined)}
              disabled={disabled}
              onChange={(e) => {
                const val = e.target.value
                form.setValue("nearPd", val === "" ? null : Number(val))
              }}
            />
            <span className="text-xs text-muted-foreground">mm</span>
          </div>
        </div>
      </div>

      {/* Lens Type */}
      <div className="space-y-1">
        <Label className="text-xs text-muted-foreground">
          {t("prescription.lensType")}
        </Label>
        <Select
          value={String(form.watch("lensType") ?? 0)}
          onValueChange={(v) => form.setValue("lensType", Number(v))}
          disabled={disabled}
        >
          <SelectTrigger className="h-8 w-48">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {LENS_TYPES.map((lt) => (
              <SelectItem key={lt.value} value={lt.value}>
                {t(`prescription.${lt.key}`)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Notes */}
      <div className="space-y-1">
        <Label className="text-xs text-muted-foreground">
          {t("prescription.notes")}
        </Label>
        <Textarea
          className="text-sm min-h-[60px]"
          value={form.watch("notes") ?? ""}
          disabled={disabled}
          onChange={(e) => form.setValue("notes", e.target.value || null)}
        />
      </div>

      {/* Action buttons */}
      {!disabled && (
        <div className="flex gap-2 justify-end">
          {onCancel && (
            <Button type="button" variant="outline" size="sm" onClick={onCancel}>
              {tCommon("actions.cancel")}
            </Button>
          )}
          <Button type="submit" size="sm" disabled={isSubmitting}>
            {tCommon("actions.save")}
          </Button>
        </div>
      )}
    </form>
  )
}
