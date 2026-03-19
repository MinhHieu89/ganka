import { useTranslation } from "react-i18next"
import { IconPlus, IconTrash } from "@tabler/icons-react"
import { Input } from "@/shared/components/Input"
import { Button } from "@/shared/components/Button"
import { Field, FieldLabel } from "@/shared/components/Field"

// -- Parameter type interfaces --

export interface IplParams {
  energy?: number
  pulseCount?: number
  spotSize?: string
  treatmentZones?: string[]
}

export interface LlltParams {
  wavelength?: number
  power?: number
  duration?: number
  treatmentArea?: string
}

export interface LidCareParams {
  procedureSteps?: string[]
  products?: string[]
  duration?: number
}

// -- Helper functions to build/parse parameters JSON --

export function buildParametersJson(
  treatmentType: string,
  values: Record<string, unknown>,
): string | null {
  switch (treatmentType) {
    case "IPL": {
      const params: IplParams = {}
      if (values.iplEnergy != null) params.energy = values.iplEnergy as number
      if (values.iplPulseCount != null) params.pulseCount = values.iplPulseCount as number
      if (values.iplSpotSize) params.spotSize = values.iplSpotSize as string
      const zones = Array.isArray(values.iplTreatmentZones)
        ? (values.iplTreatmentZones as Array<string | { value: string }>)
            .map((z) => (typeof z === "string" ? z : z.value))
            .filter(Boolean)
        : []
      if (zones.length > 0) params.treatmentZones = zones
      return Object.keys(params).length > 0 ? JSON.stringify(params) : null
    }
    case "LLLT": {
      const params: LlltParams = {}
      if (values.llltWavelength != null) params.wavelength = values.llltWavelength as number
      if (values.llltPower != null) params.power = values.llltPower as number
      if (values.llltDuration != null) params.duration = values.llltDuration as number
      if (values.llltTreatmentArea) params.treatmentArea = values.llltTreatmentArea as string
      return Object.keys(params).length > 0 ? JSON.stringify(params) : null
    }
    case "LidCare": {
      const params: LidCareParams = {}
      const steps = Array.isArray(values.lidCareProcedureSteps)
        ? (values.lidCareProcedureSteps as Array<string | { value: string }>)
            .map((s) => (typeof s === "string" ? s : s.value))
            .filter(Boolean)
        : []
      if (steps.length > 0) params.procedureSteps = steps
      const products = Array.isArray(values.lidCareProducts)
        ? (values.lidCareProducts as Array<string | { value: string }>)
            .map((p) => (typeof p === "string" ? p : p.value))
            .filter(Boolean)
        : []
      if (products.length > 0) params.products = products
      if (values.lidCareDuration != null) params.duration = values.lidCareDuration as number
      return Object.keys(params).length > 0 ? JSON.stringify(params) : null
    }
    default:
      return null
  }
}

export function parseParametersJson(
  treatmentType: string,
  json: string | null | undefined,
): Record<string, unknown> {
  if (!json) return {}
  try {
    const parsed = JSON.parse(json) as Record<string, unknown>
    switch (treatmentType) {
      case "IPL": {
        const p = parsed as IplParams
        return {
          iplEnergy: p.energy,
          iplPulseCount: p.pulseCount,
          iplSpotSize: p.spotSize ?? "",
          iplTreatmentZones: (p.treatmentZones ?? []).map((v) => ({ value: v })),
        }
      }
      case "LLLT": {
        const p = parsed as LlltParams
        return {
          llltWavelength: p.wavelength,
          llltPower: p.power,
          llltDuration: p.duration,
          llltTreatmentArea: p.treatmentArea ?? "",
        }
      }
      case "LidCare": {
        const p = parsed as LidCareParams
        return {
          lidCareProcedureSteps: (p.procedureSteps ?? []).map((v) => ({ value: v })),
          lidCareProducts: (p.products ?? []).map((v) => ({ value: v })),
          lidCareDuration: p.duration,
        }
      }
      default:
        return {}
    }
  } catch {
    return {}
  }
}

// -- Shared component --

interface TreatmentParameterFieldsProps {
  treatmentType: string
  values: Record<string, unknown>
  onChange: (field: string, value: unknown) => void
  disabled?: boolean
}

export function TreatmentParameterFields({
  treatmentType,
  values,
  onChange,
  disabled,
}: TreatmentParameterFieldsProps) {
  const { t } = useTranslation("treatment")

  const getNum = (field: string): string | number => {
    const v = values[field]
    return v != null ? (v as number) : ""
  }

  const getStr = (field: string): string => {
    const v = values[field]
    return typeof v === "string" ? v : ""
  }

  const handleNumberChange = (field: string, rawValue: string) => {
    onChange(field, rawValue === "" ? undefined : Number(rawValue))
  }

  const handleStringChange = (field: string, rawValue: string) => {
    onChange(field, rawValue)
  }

  // Dynamic list helpers
  const getListValues = (field: string): Array<{ value: string }> => {
    const raw = values[field]
    if (!Array.isArray(raw)) return []
    return raw as Array<{ value: string }>
  }

  const handleListItemChange = (field: string, index: number, newValue: string) => {
    const list = [...getListValues(field)]
    list[index] = { value: newValue }
    onChange(field, list)
  }

  const handleListAdd = (field: string) => {
    const list = [...getListValues(field), { value: "" }]
    onChange(field, list)
  }

  const handleListRemove = (field: string, index: number) => {
    const list = getListValues(field).filter((_, i) => i !== index)
    onChange(field, list)
  }

  return (
    <div className="border rounded-lg p-4 space-y-4">
      <p className="text-sm font-medium">
        {t("fields.parameters")} ({treatmentType})
      </p>

      {/* IPL Parameters */}
      {treatmentType === "IPL" && (
        <>
          <div className="grid grid-cols-3 gap-4">
            <Field>
              <FieldLabel>{t("ipl.energy")}</FieldLabel>
              <Input
                type="number"
                min={0}
                step={0.1}
                value={getNum("iplEnergy")}
                onChange={(e) => handleNumberChange("iplEnergy", e.target.value)}
                disabled={disabled}
              />
            </Field>
            <Field>
              <FieldLabel>{t("ipl.pulseCount")}</FieldLabel>
              <Input
                type="number"
                min={1}
                value={getNum("iplPulseCount")}
                onChange={(e) => handleNumberChange("iplPulseCount", e.target.value)}
                disabled={disabled}
              />
            </Field>
            <Field>
              <FieldLabel>{t("ipl.spotSize")}</FieldLabel>
              <Input
                value={getStr("iplSpotSize")}
                onChange={(e) => handleStringChange("iplSpotSize", e.target.value)}
                disabled={disabled}
              />
            </Field>
          </div>

          {/* Treatment Zones (dynamic list) */}
          <div className="space-y-2">
            <FieldLabel>{t("ipl.treatmentZones")}</FieldLabel>
            {getListValues("iplTreatmentZones").map((item, index) => (
              <div key={index} className="flex items-center gap-2">
                <Input
                  className="flex-1"
                  value={item.value}
                  onChange={(e) =>
                    handleListItemChange("iplTreatmentZones", index, e.target.value)
                  }
                  disabled={disabled}
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => handleListRemove("iplTreatmentZones", index)}
                  disabled={disabled}
                >
                  <IconTrash className="h-4 w-4" />
                </Button>
              </div>
            ))}
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => handleListAdd("iplTreatmentZones")}
              disabled={disabled}
            >
              <IconPlus className="h-4 w-4 mr-1" />
              {t("ipl.treatmentZones")}
            </Button>
          </div>
        </>
      )}

      {/* LLLT Parameters */}
      {treatmentType === "LLLT" && (
        <div className="grid grid-cols-2 gap-4">
          <Field>
            <FieldLabel>{t("lllt.wavelength")}</FieldLabel>
            <Input
              type="number"
              min={0}
              value={getNum("llltWavelength")}
              onChange={(e) => handleNumberChange("llltWavelength", e.target.value)}
              disabled={disabled}
            />
          </Field>
          <Field>
            <FieldLabel>{t("lllt.power")}</FieldLabel>
            <Input
              type="number"
              min={0}
              value={getNum("llltPower")}
              onChange={(e) => handleNumberChange("llltPower", e.target.value)}
              disabled={disabled}
            />
          </Field>
          <Field>
            <FieldLabel>{t("lllt.duration")}</FieldLabel>
            <Input
              type="number"
              min={0}
              value={getNum("llltDuration")}
              onChange={(e) => handleNumberChange("llltDuration", e.target.value)}
              disabled={disabled}
            />
          </Field>
          <Field>
            <FieldLabel>{t("lllt.treatmentArea")}</FieldLabel>
            <Input
              value={getStr("llltTreatmentArea")}
              onChange={(e) => handleStringChange("llltTreatmentArea", e.target.value)}
              disabled={disabled}
            />
          </Field>
        </div>
      )}

      {/* Lid Care Parameters */}
      {treatmentType === "LidCare" && (
        <>
          {/* Duration */}
          <Field>
            <FieldLabel>{t("lidCare.duration")}</FieldLabel>
            <Input
              type="number"
              min={0}
              value={getNum("lidCareDuration")}
              onChange={(e) => handleNumberChange("lidCareDuration", e.target.value)}
              disabled={disabled}
              className="max-w-xs"
            />
          </Field>

          {/* Procedure Steps (dynamic list) */}
          <div className="space-y-2">
            <FieldLabel>{t("lidCare.procedureSteps")}</FieldLabel>
            {getListValues("lidCareProcedureSteps").map((item, index) => (
              <div key={index} className="flex items-center gap-2">
                <span className="text-sm text-muted-foreground w-6">{index + 1}.</span>
                <Input
                  className="flex-1"
                  value={item.value}
                  onChange={(e) =>
                    handleListItemChange("lidCareProcedureSteps", index, e.target.value)
                  }
                  disabled={disabled}
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => handleListRemove("lidCareProcedureSteps", index)}
                  disabled={disabled}
                >
                  <IconTrash className="h-4 w-4" />
                </Button>
              </div>
            ))}
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => handleListAdd("lidCareProcedureSteps")}
              disabled={disabled}
            >
              <IconPlus className="h-4 w-4 mr-1" />
              {t("lidCare.procedureSteps")}
            </Button>
          </div>

          {/* Products (dynamic list) */}
          <div className="space-y-2">
            <FieldLabel>{t("lidCare.productsUsed")}</FieldLabel>
            {getListValues("lidCareProducts").map((item, index) => (
              <div key={index} className="flex items-center gap-2">
                <Input
                  className="flex-1"
                  value={item.value}
                  onChange={(e) =>
                    handleListItemChange("lidCareProducts", index, e.target.value)
                  }
                  disabled={disabled}
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => handleListRemove("lidCareProducts", index)}
                  disabled={disabled}
                >
                  <IconTrash className="h-4 w-4" />
                </Button>
              </div>
            ))}
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => handleListAdd("lidCareProducts")}
              disabled={disabled}
            >
              <IconPlus className="h-4 w-4 mr-1" />
              {t("lidCare.productsUsed")}
            </Button>
          </div>
        </>
      )}
    </div>
  )
}
