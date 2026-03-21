import { useState, useEffect, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import { IconLoader2, IconAlertTriangle } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Input } from "@/shared/components/Input"
import { Button } from "@/shared/components/Button"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { Label } from "@/shared/components/Label"
import { Checkbox } from "@/shared/components/Checkbox"
import { Separator } from "@/shared/components/Separator"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Alert, AlertDescription, AlertTitle } from "@/shared/components/Alert"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import { useAuthStore } from "@/shared/stores/authStore"
import { useRecordSession } from "@/features/treatment/api/treatment-api"
import type {
  TreatmentType,
  ConsumableInput,
  IntervalWarning,
  RecordTreatmentSessionCommand,
} from "@/features/treatment/api/treatment-types"
import { ConsumableSelector } from "./ConsumableSelector"
import { SessionOsdiCapture } from "./SessionOsdiCapture"

// -- OSDI severity calculation --

function calculateOsdiSeverity(score: number | null): string | null {
  if (score === null || score === undefined) return null
  if (score <= 12) return "Normal"
  if (score <= 22) return "Mild"
  if (score <= 32) return "Moderate"
  return "Severe"
}

// -- Zod schema --

const sessionFormSchema = z.object({
  // IPL parameters
  iplEnergy: z.coerce.number().min(0).nullable().optional(),
  iplPulseCount: z.coerce.number().int().min(0).nullable().optional(),
  iplSpotSize: z.coerce.string().nullable().optional(),
  iplTreatmentZones: z.array(z.string()).optional(),
  // LLLT parameters
  llltWavelength: z.coerce.number().min(0).nullable().optional(),
  llltPower: z.coerce.number().min(0).nullable().optional(),
  llltDuration: z.coerce.number().min(0).nullable().optional(),
  llltTreatmentArea: z.string().nullable().optional(),
  // Lid Care parameters
  lidCareDuration: z.coerce.number().min(0).nullable().optional(),
  lidCareProductsUsed: z.array(z.string()).optional(),
  lidCareProcedureSteps: z.record(z.string(), z.boolean()).optional(),
  // Common
  osdiScore: z.coerce.number().min(0).max(100).nullable().optional(),
  clinicalNotes: z.string().nullable().optional(),
  intervalOverrideReason: z.string().nullable().optional(),
  visitId: z.string().nullable().optional(),
})

type SessionFormValues = z.infer<typeof sessionFormSchema>

// -- Default IPL treatment zones --

const IPL_TREATMENT_ZONES = [
  "Upper eyelid",
  "Lower eyelid",
  "Periorbital",
  "Cheek",
  "Nose bridge",
  "Forehead",
]

// -- Default Lid Care procedure steps --

const LID_CARE_STEPS = [
  "Warm compress applied",
  "Lid margin cleaning",
  "Meibomian gland expression",
  "Debridement",
  "Antibiotic ointment applied",
]

// -- Parse default parameters --

interface IplDefaults {
  energy?: number
  pulseCount?: number
  spotSize?: string
  treatmentZones?: string[]
}

interface LlltDefaults {
  wavelength?: number
  power?: number
  duration?: number
  treatmentArea?: string
}

interface LidCareDefaults {
  procedureSteps?: string[]
  productsUsed?: string[]
  duration?: number
}

function parseDefaultParameters(
  json: string | null | undefined,
  type: TreatmentType,
): IplDefaults | LlltDefaults | LidCareDefaults | null {
  if (!json) return null
  try {
    return JSON.parse(json) as IplDefaults | LlltDefaults | LidCareDefaults
  } catch {
    return null
  }
}

// -- Serialize parameters to JSON --

function serializeParameters(
  values: SessionFormValues,
  type: TreatmentType,
): string {
  switch (type) {
    case "IPL":
      return JSON.stringify({
        energy: values.iplEnergy ?? null,
        pulseCount: values.iplPulseCount ?? null,
        spotSize: values.iplSpotSize ?? null,
        treatmentZones: values.iplTreatmentZones ?? [],
      })
    case "LLLT":
      return JSON.stringify({
        wavelength: values.llltWavelength ?? null,
        power: values.llltPower ?? null,
        duration: values.llltDuration ?? null,
        treatmentArea: values.llltTreatmentArea ?? null,
      })
    case "LidCare": {
      const checkedSteps = Object.entries(values.lidCareProcedureSteps ?? {})
        .filter(([, checked]) => checked)
        .map(([step]) => step)
      return JSON.stringify({
        procedureSteps: checkedSteps,
        productsUsed: values.lidCareProductsUsed ?? [],
        duration: values.lidCareDuration ?? null,
      })
    }
    default:
      return "{}"
  }
}

// -- Props --

interface TreatmentSessionFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  packageId: string
  treatmentType: TreatmentType
  defaultParametersJson?: string | null
  lastSessionDate?: string | null
  minIntervalDays?: number
}

// -- Component --

export function TreatmentSessionForm({
  open,
  onOpenChange,
  packageId,
  treatmentType,
  defaultParametersJson,
  lastSessionDate,
  minIntervalDays,
}: TreatmentSessionFormProps) {
  const { t } = useTranslation("treatment")
  const user = useAuthStore((s) => s.user)
  const recordMutation = useRecordSession(packageId)

  const [consumables, setConsumables] = useState<ConsumableInput[]>([])
  const [osdiScore, setOsdiScore] = useState<number | null>(null)
  const [intervalWarning, setIntervalWarning] =
    useState<IntervalWarning | null>(null)
  const [lidCareProducts, setLidCareProducts] = useState<string[]>([])
  const [newProduct, setNewProduct] = useState("")

  const osdiSeverity = useMemo(
    () => calculateOsdiSeverity(osdiScore),
    [osdiScore],
  )

  // Parse default parameters for pre-populating form
  const defaults = useMemo(
    () => parseDefaultParameters(defaultParametersJson, treatmentType),
    [defaultParametersJson, treatmentType],
  )

  const form = useForm<SessionFormValues>({
    resolver: zodResolver(sessionFormSchema),
    defaultValues: {
      iplEnergy: null,
      iplPulseCount: null,
      iplSpotSize: null,
      iplTreatmentZones: [],
      llltWavelength: null,
      llltPower: null,
      llltDuration: null,
      llltTreatmentArea: null,
      lidCareDuration: null,
      lidCareProductsUsed: [],
      lidCareProcedureSteps: {},
      osdiScore: null,
      clinicalNotes: null,
      intervalOverrideReason: null,
      visitId: null,
    },
  })

  // Reset form when dialog opens and populate defaults
  useEffect(() => {
    if (!open) return

    setConsumables([])
    setOsdiScore(null)
    setIntervalWarning(null)
    setLidCareProducts([])
    setNewProduct("")

    const resetValues: SessionFormValues = {
      iplEnergy: null,
      iplPulseCount: null,
      iplSpotSize: null,
      iplTreatmentZones: [],
      llltWavelength: null,
      llltPower: null,
      llltDuration: null,
      llltTreatmentArea: null,
      lidCareDuration: null,
      lidCareProductsUsed: [],
      lidCareProcedureSteps: {},
      osdiScore: null,
      clinicalNotes: null,
      intervalOverrideReason: null,
      visitId: null,
    }

    if (defaults) {
      switch (treatmentType) {
        case "IPL": {
          const d = defaults as IplDefaults
          resetValues.iplEnergy = d.energy ?? null
          resetValues.iplPulseCount = d.pulseCount ?? null
          resetValues.iplSpotSize = d.spotSize ?? null
          resetValues.iplTreatmentZones = d.treatmentZones ?? []
          break
        }
        case "LLLT": {
          const d = defaults as LlltDefaults
          resetValues.llltWavelength = d.wavelength ?? null
          resetValues.llltPower = d.power ?? null
          resetValues.llltDuration = d.duration ?? null
          resetValues.llltTreatmentArea = d.treatmentArea ?? null
          break
        }
        case "LidCare": {
          const d = defaults as LidCareDefaults
          resetValues.lidCareDuration = d.duration ?? null
          const stepMap: Record<string, boolean> = {}
          for (const step of d.procedureSteps ?? []) {
            stepMap[step] = true
          }
          resetValues.lidCareProcedureSteps = stepMap
          setLidCareProducts(d.productsUsed ?? [])
          resetValues.lidCareProductsUsed = d.productsUsed ?? []
          break
        }
      }
    }

    form.reset(resetValues)

    // Proactive interval warning (client-side check on dialog open)
    if (lastSessionDate && minIntervalDays) {
      const daysSinceLast = Math.floor(
        (Date.now() - new Date(lastSessionDate).getTime()) / (1000 * 60 * 60 * 24)
      )
      if (daysSinceLast < minIntervalDays) {
        setIntervalWarning({ daysSinceLast, minIntervalDays })
      }
    }
  }, [open, defaults, treatmentType, form, lastSessionDate, minIntervalDays])

  const isSubmitting = recordMutation.isPending

  const handleSubmit = async (data: SessionFormValues) => {
    if (!user) return

    try {
      const parametersJson = serializeParameters(data, treatmentType)

      const command: Omit<RecordTreatmentSessionCommand, "packageId"> = {
        parametersJson,
        osdiScore: osdiScore,
        osdiSeverity: osdiSeverity,
        clinicalNotes: data.clinicalNotes || null,
        performedById: user.id,
        visitId: data.visitId || null,
        scheduledAt: null,
        intervalOverrideReason: data.intervalOverrideReason || null,
        consumables,
      }

      const result = await recordMutation.mutateAsync(command)

      if (result.warning) {
        setIntervalWarning(result.warning)
        toast.success(t("sessionForm.successWithWarning"))
      } else {
        toast.success(t("sessionForm.success"))
        onOpenChange(false)
      }
    } catch (error) {
      handleServerValidationError(error, form.setError)
    }
  }

  // Handle zone toggle for IPL
  const handleZoneToggle = (zone: string, checked: boolean) => {
    const current = form.getValues("iplTreatmentZones") ?? []
    if (checked) {
      form.setValue("iplTreatmentZones", [...current, zone])
    } else {
      form.setValue(
        "iplTreatmentZones",
        current.filter((z) => z !== zone),
      )
    }
  }

  // Handle lid care procedure step toggle
  const handleStepToggle = (step: string, checked: boolean) => {
    const current = form.getValues("lidCareProcedureSteps") ?? {}
    form.setValue("lidCareProcedureSteps", { ...current, [step]: checked })
  }

  // Handle adding lid care product
  const handleAddProduct = () => {
    const trimmed = newProduct.trim()
    if (!trimmed || lidCareProducts.includes(trimmed)) return
    const updated = [...lidCareProducts, trimmed]
    setLidCareProducts(updated)
    form.setValue("lidCareProductsUsed", updated)
    setNewProduct("")
  }

  const handleRemoveProduct = (product: string) => {
    const updated = lidCareProducts.filter((p) => p !== product)
    setLidCareProducts(updated)
    form.setValue("lidCareProductsUsed", updated)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{t("sessionForm.title")}</DialogTitle>
        </DialogHeader>

        <form
          onSubmit={form.handleSubmit(handleSubmit)}
          className="space-y-6"
        >
          {/* Section 1: Device Parameters */}
          <div className="space-y-3">
            <h4 className="text-sm font-semibold">{t("sessionForm.deviceParameters")}</h4>

            {treatmentType === "IPL" && (
              <IplParameterFields
                form={form}
                zones={form.watch("iplTreatmentZones") ?? []}
                onZoneToggle={handleZoneToggle}
              />
            )}

            {treatmentType === "LLLT" && <LlltParameterFields form={form} />}

            {treatmentType === "LidCare" && (
              <LidCareParameterFields
                form={form}
                steps={form.watch("lidCareProcedureSteps") ?? {}}
                onStepToggle={handleStepToggle}
                products={lidCareProducts}
                newProduct={newProduct}
                onNewProductChange={setNewProduct}
                onAddProduct={handleAddProduct}
                onRemoveProduct={handleRemoveProduct}
              />
            )}
          </div>

          <Separator />

          {/* Section 2: OSDI */}
          <div className="space-y-3">
            <h4 className="text-sm font-semibold">{t("fields.osdiScore")}</h4>
            <SessionOsdiCapture
              packageId={packageId}
              osdiScore={osdiScore}
              onOsdiScoreChange={setOsdiScore}
              osdiSeverity={osdiSeverity}
            />
          </div>

          <Separator />

          {/* Section 3: Clinical Notes */}
          <div className="space-y-3">
            <h4 className="text-sm font-semibold">{t("sessionForm.clinicalNotes")}</h4>
            <Controller
              name="clinicalNotes"
              control={form.control}
              render={({ field }) => (
                <AutoResizeTextarea
                  {...field}
                  value={field.value ?? ""}
                  onChange={(e) =>
                    field.onChange(e.target.value || null)
                  }
                  rows={3}
                />
              )}
            />
          </div>

          <Separator />

          {/* Section 4: Consumables */}
          <div className="space-y-3">
            <h4 className="text-sm font-semibold">{t("sessionForm.consumables")}</h4>
            <ConsumableSelector
              value={consumables}
              onChange={setConsumables}
            />
          </div>

          {/* Section 5: Interval Warning */}
          {intervalWarning && (
            <>
              <Separator />
              <Alert variant="destructive" className="border-yellow-300 bg-yellow-50 text-yellow-900 [&>svg]:text-yellow-600">
                <IconAlertTriangle className="h-4 w-4" />
                <AlertTitle>{t("sessionForm.intervalWarningTitle")}</AlertTitle>
                <AlertDescription>
                  {t("intervalWarning", { days: intervalWarning.daysSinceLast, minDays: intervalWarning.minIntervalDays })}
                </AlertDescription>
              </Alert>
              <Controller
                name="intervalOverrideReason"
                control={form.control}
                render={({ field }) => (
                  <Field>
                    <FieldLabel>{t("sessionForm.intervalOverrideReason")}</FieldLabel>
                    <AutoResizeTextarea
                      {...field}
                      value={field.value ?? ""}
                      onChange={(e) =>
                        field.onChange(e.target.value || null)
                      }
                      rows={2}
                    />
                  </Field>
                )}
              />
            </>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              {t("sessionForm.cancel")}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {t("sessionForm.submit")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// -- IPL Parameter Fields --

function IplParameterFields({
  form,
  zones,
  onZoneToggle,
}: {
  form: ReturnType<typeof useForm<SessionFormValues>>
  zones: string[]
  onZoneToggle: (zone: string, checked: boolean) => void
}) {
  const { t } = useTranslation("treatment")
  return (
    <div className="space-y-3">
      <div className="grid grid-cols-3 gap-3">
        <Controller
          name="iplEnergy"
          control={form.control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="iplEnergy">
                {t("ipl.energy")}
              </FieldLabel>
              <Input
                {...field}
                id="iplEnergy"
                type="number"
                step="0.1"
                min={0}
                value={field.value ?? ""}
                onChange={(e) =>
                  field.onChange(
                    e.target.value ? Number(e.target.value) : null,
                  )
                }
              />
            </Field>
          )}
        />

        <Controller
          name="iplPulseCount"
          control={form.control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="iplPulseCount">
                {t("ipl.pulseCount")}
              </FieldLabel>
              <Input
                {...field}
                id="iplPulseCount"
                type="number"
                min={0}
                value={field.value ?? ""}
                onChange={(e) =>
                  field.onChange(
                    e.target.value ? Number(e.target.value) : null,
                  )
                }
              />
            </Field>
          )}
        />

        <Controller
          name="iplSpotSize"
          control={form.control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="iplSpotSize">
                {t("ipl.spotSize")}
              </FieldLabel>
              <Input
                {...field}
                id="iplSpotSize"
                value={field.value ?? ""}
                onChange={(e) =>
                  field.onChange(e.target.value || null)
                }
              />
            </Field>
          )}
        />
      </div>

      <div>
        <Label className="text-sm">{t("ipl.treatmentZones")}</Label>
        <div className="grid grid-cols-3 gap-2 mt-2">
          {IPL_TREATMENT_ZONES.map((zone) => (
            <label
              key={zone}
              className="flex items-center gap-2 text-sm cursor-pointer"
            >
              <Checkbox
                checked={zones.includes(zone)}
                onCheckedChange={(checked) =>
                  onZoneToggle(zone, checked === true)
                }
              />
              {t(`ipl.zones.${zone}`, zone)}
            </label>
          ))}
        </div>
      </div>
    </div>
  )
}

// -- LLLT Parameter Fields --

function LlltParameterFields({
  form,
}: {
  form: ReturnType<typeof useForm<SessionFormValues>>
}) {
  const { t } = useTranslation("treatment")
  return (
    <div className="space-y-3">
      <div className="grid grid-cols-3 gap-3">
        <Controller
          name="llltWavelength"
          control={form.control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="llltWavelength">
                {t("lllt.wavelength")}
              </FieldLabel>
              <Input
                {...field}
                id="llltWavelength"
                type="number"
                min={0}
                value={field.value ?? ""}
                onChange={(e) =>
                  field.onChange(
                    e.target.value ? Number(e.target.value) : null,
                  )
                }
              />
            </Field>
          )}
        />

        <Controller
          name="llltPower"
          control={form.control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="llltPower">
                {t("lllt.power")}
              </FieldLabel>
              <Input
                {...field}
                id="llltPower"
                type="number"
                min={0}
                value={field.value ?? ""}
                onChange={(e) =>
                  field.onChange(
                    e.target.value ? Number(e.target.value) : null,
                  )
                }
              />
            </Field>
          )}
        />

        <Controller
          name="llltDuration"
          control={form.control}
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="llltDuration">
                {t("lllt.duration")}
              </FieldLabel>
              <Input
                {...field}
                id="llltDuration"
                type="number"
                min={0}
                value={field.value ?? ""}
                onChange={(e) =>
                  field.onChange(
                    e.target.value ? Number(e.target.value) : null,
                  )
                }
              />
            </Field>
          )}
        />
      </div>

      <Controller
        name="llltTreatmentArea"
        control={form.control}
        render={({ field }) => (
          <Field>
            <FieldLabel htmlFor="llltTreatmentArea">
              {t("lllt.treatmentArea")}
            </FieldLabel>
            <Input
              {...field}
              id="llltTreatmentArea"
              value={field.value ?? ""}
              onChange={(e) =>
                field.onChange(e.target.value || null)
              }
            />
          </Field>
        )}
      />
    </div>
  )
}

// -- Lid Care Parameter Fields --

function LidCareParameterFields({
  form,
  steps,
  onStepToggle,
  products,
  newProduct,
  onNewProductChange,
  onAddProduct,
  onRemoveProduct,
}: {
  form: ReturnType<typeof useForm<SessionFormValues>>
  steps: Record<string, boolean>
  onStepToggle: (step: string, checked: boolean) => void
  products: string[]
  newProduct: string
  onNewProductChange: (value: string) => void
  onAddProduct: () => void
  onRemoveProduct: (product: string) => void
}) {
  const { t } = useTranslation("treatment")
  return (
    <div className="space-y-4">
      {/* Procedure steps checklist */}
      <div>
        <Label className="text-sm">{t("sessionForm.procedureSteps")}</Label>
        <div className="space-y-2 mt-2">
          {LID_CARE_STEPS.map((step) => (
            <label
              key={step}
              className="flex items-center gap-2 text-sm cursor-pointer"
            >
              <Checkbox
                checked={steps[step] ?? false}
                onCheckedChange={(checked) =>
                  onStepToggle(step, checked === true)
                }
              />
              {t(`lidCare.steps.${step}`, step)}
            </label>
          ))}
        </div>
      </div>

      {/* Products used */}
      <div>
        <Label className="text-sm">{t("sessionForm.productsUsed")}</Label>
        <div className="space-y-2 mt-2">
          {products.map((product) => (
            <div
              key={product}
              className="flex items-center gap-2 text-sm"
            >
              <span className="flex-1">{product}</span>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => onRemoveProduct(product)}
              >
                &times;
              </Button>
            </div>
          ))}
          <div className="flex gap-2">
            <Input
              value={newProduct}
              onChange={(e) => onNewProductChange(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault()
                  onAddProduct()
                }
              }}
              className="flex-1"
            />
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={onAddProduct}
            >
              {t("sessionForm.addProduct")}
            </Button>
          </div>
        </div>
      </div>

      {/* Duration */}
      <Controller
        name="lidCareDuration"
        control={form.control}
        render={({ field }) => (
          <Field>
            <FieldLabel htmlFor="lidCareDuration">
              {t("lidCare.duration")}
            </FieldLabel>
            <Input
              {...field}
              id="lidCareDuration"
              type="number"
              min={0}
              value={field.value ?? ""}
              onChange={(e) =>
                field.onChange(
                  e.target.value ? Number(e.target.value) : null,
                )
              }
            />
          </Field>
        )}
      />
    </div>
  )
}
