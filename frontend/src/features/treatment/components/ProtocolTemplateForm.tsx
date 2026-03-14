import { useEffect, useMemo } from "react"
import { useForm, Controller, useFieldArray } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import { IconLoader2, IconPlus, IconTrash } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Input } from "@/shared/components/Input"
import { Button } from "@/shared/components/Button"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import {
  useCreateProtocolTemplate,
  useUpdateProtocolTemplate,
} from "@/features/treatment/api/treatment-api"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import type { TreatmentProtocolDto } from "@/features/treatment/api/treatment-types"

// -- Zod schema --

const protocolTemplateSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be at most 200 characters"),
  treatmentType: z.string().min(1, "Treatment type is required"),
  defaultSessionCount: z
    .number({ invalid_type_error: "Required" })
    .int()
    .min(1, "Must be between 1 and 6")
    .max(6, "Must be between 1 and 6"),
  pricingMode: z.string().min(1, "Pricing mode is required"),
  defaultPackagePrice: z
    .number({ invalid_type_error: "Required" })
    .min(0, "Must be >= 0"),
  defaultSessionPrice: z
    .number({ invalid_type_error: "Required" })
    .min(0, "Must be >= 0"),
  minIntervalDays: z
    .number({ invalid_type_error: "Required" })
    .int()
    .min(1, "Must be >= 1"),
  maxIntervalDays: z
    .number({ invalid_type_error: "Required" })
    .int()
    .min(1, "Must be >= 1"),
  cancellationDeductionPercent: z
    .number({ invalid_type_error: "Required" })
    .min(10, "Must be between 10 and 20")
    .max(20, "Must be between 10 and 20"),
  description: z.string().optional(),
  // IPL parameters
  iplEnergy: z.number().optional(),
  iplPulseCount: z.number().optional(),
  iplSpotSize: z.string().optional(),
  iplTreatmentZones: z.array(z.object({ value: z.string() })).optional(),
  // LLLT parameters
  llltWavelength: z.number().optional(),
  llltPower: z.number().optional(),
  llltDuration: z.number().optional(),
  llltTreatmentArea: z.string().optional(),
  // Lid Care parameters
  lidCareProcedureSteps: z.array(z.object({ value: z.string() })).optional(),
  lidCareProducts: z.array(z.object({ value: z.string() })).optional(),
  lidCareDuration: z.number().optional(),
}).refine((data) => data.maxIntervalDays >= data.minIntervalDays, {
  message: "Max interval must be >= min interval",
  path: ["maxIntervalDays"],
})

type ProtocolTemplateFormValues = z.infer<typeof protocolTemplateSchema>

// -- Type maps --

const TREATMENT_TYPE_OPTIONS = [
  { value: "IPL", label: "IPL" },
  { value: "LLLT", label: "LLLT" },
  { value: "LidCare", label: "Lid Care" },
] as const

const TREATMENT_TYPE_ENUM: Record<string, number> = { IPL: 0, LLLT: 1, LidCare: 2 }
const PRICING_MODE_ENUM: Record<string, number> = { PerSession: 0, PerPackage: 1 }

const PRICING_MODE_OPTIONS = [
  { value: "PerSession", label: "Per Session" },
  { value: "PerPackage", label: "Per Package" },
] as const

// -- Helper to build/parse parameters JSON --

interface IplParams {
  energy?: number
  pulseCount?: number
  spotSize?: string
  treatmentZones?: string[]
}

interface LlltParams {
  wavelength?: number
  power?: number
  duration?: number
  treatmentArea?: string
}

interface LidCareParams {
  procedureSteps?: string[]
  products?: string[]
  duration?: number
}

function buildParametersJson(
  treatmentType: string,
  values: ProtocolTemplateFormValues,
): string | null {
  switch (treatmentType) {
    case "IPL": {
      const params: IplParams = {}
      if (values.iplEnergy != null) params.energy = values.iplEnergy
      if (values.iplPulseCount != null) params.pulseCount = values.iplPulseCount
      if (values.iplSpotSize) params.spotSize = values.iplSpotSize
      const zones = values.iplTreatmentZones?.map((z) => z.value).filter(Boolean) ?? []
      if (zones.length > 0) params.treatmentZones = zones
      return Object.keys(params).length > 0 ? JSON.stringify(params) : null
    }
    case "LLLT": {
      const params: LlltParams = {}
      if (values.llltWavelength != null) params.wavelength = values.llltWavelength
      if (values.llltPower != null) params.power = values.llltPower
      if (values.llltDuration != null) params.duration = values.llltDuration
      if (values.llltTreatmentArea) params.treatmentArea = values.llltTreatmentArea
      return Object.keys(params).length > 0 ? JSON.stringify(params) : null
    }
    case "LidCare": {
      const params: LidCareParams = {}
      const steps = values.lidCareProcedureSteps?.map((s) => s.value).filter(Boolean) ?? []
      if (steps.length > 0) params.procedureSteps = steps
      const products = values.lidCareProducts?.map((p) => p.value).filter(Boolean) ?? []
      if (products.length > 0) params.products = products
      if (values.lidCareDuration != null) params.duration = values.lidCareDuration
      return Object.keys(params).length > 0 ? JSON.stringify(params) : null
    }
    default:
      return null
  }
}

function parseParametersJson(
  treatmentType: string,
  json: string | null,
): Partial<ProtocolTemplateFormValues> {
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

// -- Component --

interface ProtocolTemplateFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  initialData?: TreatmentProtocolDto
}

export function ProtocolTemplateForm({
  open,
  onOpenChange,
  initialData,
}: ProtocolTemplateFormProps) {
  const isEdit = !!initialData
  const createMutation = useCreateProtocolTemplate()
  const updateMutation = useUpdateProtocolTemplate()

  const defaultValues = useMemo<ProtocolTemplateFormValues>(
    () => ({
      name: "",
      treatmentType: "IPL",
      defaultSessionCount: 4,
      pricingMode: "PerPackage",
      defaultPackagePrice: 0,
      defaultSessionPrice: 0,
      minIntervalDays: 14,
      maxIntervalDays: 21,
      cancellationDeductionPercent: 15,
      description: "",
      iplEnergy: undefined,
      iplPulseCount: undefined,
      iplSpotSize: "",
      iplTreatmentZones: [],
      llltWavelength: undefined,
      llltPower: undefined,
      llltDuration: undefined,
      llltTreatmentArea: "",
      lidCareProcedureSteps: [],
      lidCareProducts: [],
      lidCareDuration: undefined,
    }),
    [],
  )

  const form = useForm<ProtocolTemplateFormValues>({
    resolver: zodResolver(protocolTemplateSchema),
    defaultValues,
  })

  const treatmentType = form.watch("treatmentType")

  // Field arrays for multi-input fields
  const iplZones = useFieldArray({ control: form.control, name: "iplTreatmentZones" })
  const lidCareSteps = useFieldArray({ control: form.control, name: "lidCareProcedureSteps" })
  const lidCareProducts = useFieldArray({ control: form.control, name: "lidCareProducts" })

  useEffect(() => {
    if (open && isEdit && initialData) {
      const paramValues = parseParametersJson(
        initialData.treatmentType,
        initialData.defaultParametersJson,
      )
      form.reset({
        name: initialData.name,
        treatmentType: initialData.treatmentType,
        defaultSessionCount: initialData.defaultSessionCount,
        pricingMode: initialData.pricingMode,
        defaultPackagePrice: initialData.defaultPackagePrice,
        defaultSessionPrice: initialData.defaultSessionPrice,
        minIntervalDays: initialData.minIntervalDays,
        maxIntervalDays: initialData.maxIntervalDays,
        cancellationDeductionPercent: initialData.cancellationDeductionPercent,
        description: initialData.description ?? "",
        // Reset all params first
        iplEnergy: undefined,
        iplPulseCount: undefined,
        iplSpotSize: "",
        iplTreatmentZones: [],
        llltWavelength: undefined,
        llltPower: undefined,
        llltDuration: undefined,
        llltTreatmentArea: "",
        lidCareProcedureSteps: [],
        lidCareProducts: [],
        lidCareDuration: undefined,
        // Apply parsed params
        ...paramValues,
      })
    } else if (open && !isEdit) {
      form.reset(defaultValues)
    }
  }, [open, isEdit, initialData, form, defaultValues])

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  const handleSubmit = async (data: ProtocolTemplateFormValues) => {
    const parametersJson = buildParametersJson(data.treatmentType, data)

    const command = {
      name: data.name,
      treatmentType: TREATMENT_TYPE_ENUM[data.treatmentType] ?? 0,
      defaultSessionCount: data.defaultSessionCount,
      pricingMode: PRICING_MODE_ENUM[data.pricingMode] ?? 0,
      defaultPackagePrice: data.defaultPackagePrice,
      defaultSessionPrice: data.defaultSessionPrice,
      minIntervalDays: data.minIntervalDays,
      maxIntervalDays: data.maxIntervalDays,
      defaultParametersJson: parametersJson,
      cancellationDeductionPercent: data.cancellationDeductionPercent,
      description: data.description || null,
    }

    try {
      if (isEdit && initialData) {
        await updateMutation.mutateAsync({ id: initialData.id, ...command })
        toast.success("Protocol template updated successfully")
      } else {
        await createMutation.mutateAsync(command)
        toast.success("Protocol template created successfully")
      }
      onOpenChange(false)
    } catch (error) {
      const nonFieldErrors = handleServerValidationError(error, form.setError)
      if (nonFieldErrors.length > 0) {
        toast.error(nonFieldErrors.join(", "))
      }
    }
  }

  const getErrorMessage = (
    error: { message?: string } | undefined,
  ): string | undefined => {
    if (!error?.message) return undefined
    if (error.message === "required") return "This field is required"
    return error.message
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {isEdit ? "Edit Protocol Template" : "Create Protocol Template"}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Name */}
          <Controller
            name="name"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>Name</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Treatment Type and Session Count */}
          <div className="grid grid-cols-2 gap-4">
            <Controller
              name="treatmentType"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel>Treatment Type</FieldLabel>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {TREATMENT_TYPE_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>
                          {opt.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {fieldState.error && (
                    <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="defaultSessionCount"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>Default Session Count (1-6)</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="number"
                    min={1}
                    max={6}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.valueAsNumber)}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          {/* Pricing */}
          <div className="grid grid-cols-3 gap-4">
            <Controller
              name="pricingMode"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel>Pricing Mode</FieldLabel>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {PRICING_MODE_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>
                          {opt.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {fieldState.error && (
                    <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="defaultPackagePrice"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>Package Price (VND)</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="number"
                    min={0}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.valueAsNumber)}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="defaultSessionPrice"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>Session Price (VND)</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="number"
                    min={0}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.valueAsNumber)}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          {/* Interval */}
          <div className="grid grid-cols-2 gap-4">
            <Controller
              name="minIntervalDays"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>Min Interval (days)</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="number"
                    min={1}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.valueAsNumber)}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="maxIntervalDays"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>Max Interval (days)</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="number"
                    min={1}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.valueAsNumber)}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          {/* Cancellation Deduction */}
          <Controller
            name="cancellationDeductionPercent"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>
                  Cancellation Deduction % (10-20)
                </FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  type="number"
                  min={10}
                  max={20}
                  value={field.value ?? ""}
                  onChange={(e) => field.onChange(e.target.valueAsNumber)}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Description */}
          <Controller
            name="description"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>Description</FieldLabel>
                <AutoResizeTextarea
                  {...field}
                  id={field.name}
                  rows={3}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Treatment-type-specific parameters */}
          <div className="border rounded-lg p-4 space-y-4">
            <p className="text-sm font-medium">Default Parameters ({treatmentType})</p>

            {/* IPL Parameters */}
            {treatmentType === "IPL" && (
              <>
                <div className="grid grid-cols-3 gap-4">
                  <Controller
                    name="iplEnergy"
                    control={form.control}
                    render={({ field }) => (
                      <Field>
                        <FieldLabel htmlFor={field.name}>Energy (J/cm2)</FieldLabel>
                        <Input
                          {...field}
                          id={field.name}
                          type="number"
                          min={0}
                          step={0.1}
                          value={field.value ?? ""}
                          onChange={(e) =>
                            field.onChange(
                              e.target.value === "" ? undefined : e.target.valueAsNumber,
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
                        <FieldLabel htmlFor={field.name}>Pulse Count</FieldLabel>
                        <Input
                          {...field}
                          id={field.name}
                          type="number"
                          min={1}
                          value={field.value ?? ""}
                          onChange={(e) =>
                            field.onChange(
                              e.target.value === "" ? undefined : e.target.valueAsNumber,
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
                        <FieldLabel htmlFor={field.name}>Spot Size</FieldLabel>
                        <Input
                          {...field}
                          id={field.name}
                          value={field.value ?? ""}
                        />
                      </Field>
                    )}
                  />
                </div>

                {/* Treatment Zones (multi-input) */}
                <div className="space-y-2">
                  <FieldLabel>Treatment Zones</FieldLabel>
                  {iplZones.fields.map((zoneField, index) => (
                    <div key={zoneField.id} className="flex items-center gap-2">
                      <Controller
                        name={`iplTreatmentZones.${index}.value`}
                        control={form.control}
                        render={({ field }) => (
                          <Input {...field} className="flex-1" />
                        )}
                      />
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        onClick={() => iplZones.remove(index)}
                      >
                        <IconTrash className="h-4 w-4" />
                      </Button>
                    </div>
                  ))}
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => iplZones.append({ value: "" })}
                  >
                    <IconPlus className="h-4 w-4 mr-1" />
                    Add Zone
                  </Button>
                </div>
              </>
            )}

            {/* LLLT Parameters */}
            {treatmentType === "LLLT" && (
              <div className="grid grid-cols-2 gap-4">
                <Controller
                  name="llltWavelength"
                  control={form.control}
                  render={({ field }) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>Wavelength (nm)</FieldLabel>
                      <Input
                        {...field}
                        id={field.name}
                        type="number"
                        min={0}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value === "" ? undefined : e.target.valueAsNumber,
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
                      <FieldLabel htmlFor={field.name}>Power (mW)</FieldLabel>
                      <Input
                        {...field}
                        id={field.name}
                        type="number"
                        min={0}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value === "" ? undefined : e.target.valueAsNumber,
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
                      <FieldLabel htmlFor={field.name}>Duration (min)</FieldLabel>
                      <Input
                        {...field}
                        id={field.name}
                        type="number"
                        min={0}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value === "" ? undefined : e.target.valueAsNumber,
                          )
                        }
                      />
                    </Field>
                  )}
                />
                <Controller
                  name="llltTreatmentArea"
                  control={form.control}
                  render={({ field }) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>Treatment Area</FieldLabel>
                      <Input
                        {...field}
                        id={field.name}
                        value={field.value ?? ""}
                      />
                    </Field>
                  )}
                />
              </div>
            )}

            {/* Lid Care Parameters */}
            {treatmentType === "LidCare" && (
              <>
                {/* Duration */}
                <Controller
                  name="lidCareDuration"
                  control={form.control}
                  render={({ field }) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>Duration (min)</FieldLabel>
                      <Input
                        {...field}
                        id={field.name}
                        type="number"
                        min={0}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value === "" ? undefined : e.target.valueAsNumber,
                          )
                        }
                        className="max-w-xs"
                      />
                    </Field>
                  )}
                />

                {/* Procedure Steps (multi-input checklist) */}
                <div className="space-y-2">
                  <FieldLabel>Procedure Steps</FieldLabel>
                  {lidCareSteps.fields.map((stepField, index) => (
                    <div key={stepField.id} className="flex items-center gap-2">
                      <span className="text-sm text-muted-foreground w-6">{index + 1}.</span>
                      <Controller
                        name={`lidCareProcedureSteps.${index}.value`}
                        control={form.control}
                        render={({ field }) => (
                          <Input {...field} className="flex-1" />
                        )}
                      />
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        onClick={() => lidCareSteps.remove(index)}
                      >
                        <IconTrash className="h-4 w-4" />
                      </Button>
                    </div>
                  ))}
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => lidCareSteps.append({ value: "" })}
                  >
                    <IconPlus className="h-4 w-4 mr-1" />
                    Add Step
                  </Button>
                </div>

                {/* Products (multi-input) */}
                <div className="space-y-2">
                  <FieldLabel>Products</FieldLabel>
                  {lidCareProducts.fields.map((productField, index) => (
                    <div key={productField.id} className="flex items-center gap-2">
                      <Controller
                        name={`lidCareProducts.${index}.value`}
                        control={form.control}
                        render={({ field }) => (
                          <Input {...field} className="flex-1" />
                        )}
                      />
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        onClick={() => lidCareProducts.remove(index)}
                      >
                        <IconTrash className="h-4 w-4" />
                      </Button>
                    </div>
                  ))}
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => lidCareProducts.append({ value: "" })}
                  >
                    <IconPlus className="h-4 w-4 mr-1" />
                    Add Product
                  </Button>
                </div>
              </>
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {isEdit ? "Update" : "Create"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
