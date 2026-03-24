import { useEffect, useMemo, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { createValidationMessages } from "@/shared/lib/validation"
import { toast } from "sonner"
import { IconLoader2 } from "@tabler/icons-react"
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
import {
  TreatmentParameterFields,
  buildParametersJson,
  parseParametersJson,
} from "./TreatmentParameterFields"

// -- Zod schema --

function createProtocolTemplateSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
  name: z.string().min(1, v.required).max(200, v.maxLength(200)),
  treatmentType: z.string().min(1, v.required),
  defaultSessionCount: z
    .number({ invalid_type_error: v.required })
    .int()
    .min(1, v.between(1, 6))
    .max(6, v.between(1, 6)),
  pricingMode: z.string().min(1, v.required),
  defaultPackagePrice: z
    .number({ invalid_type_error: v.required })
    .min(0, v.mustBeNonNegative),
  defaultSessionPrice: z
    .number({ invalid_type_error: v.required })
    .min(0, v.mustBeNonNegative),
  minIntervalDays: z
    .number({ invalid_type_error: v.required })
    .int()
    .min(1, v.minValue(1)),
  maxIntervalDays: z
    .number({ invalid_type_error: v.required })
    .int()
    .min(1, v.minValue(1)),
  cancellationDeductionPercent: z
    .number({ invalid_type_error: v.required })
    .min(10, v.between(10, 20))
    .max(20, v.between(10, 20)),
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
    message: v.mustBeNonNegative,
    path: ["maxIntervalDays"],
  })
}

type ProtocolTemplateFormValues = z.infer<ReturnType<typeof createProtocolTemplateSchema>>

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
  const { t } = useTranslation("treatment")
  const { t: tCommon } = useTranslation("common")
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

  const protocolTemplateSchema = useMemo(() => createProtocolTemplateSchema(tCommon), [tCommon])
  const form = useForm<ProtocolTemplateFormValues>({
    resolver: zodResolver(protocolTemplateSchema),
    defaultValues,
  })

  const treatmentType = form.watch("treatmentType")

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
      } as ProtocolTemplateFormValues)
    } else if (open && !isEdit) {
      form.reset(defaultValues)
    }
  }, [open, isEdit, initialData, form, defaultValues])

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  const handleSubmit = async (data: ProtocolTemplateFormValues) => {
    const parametersJson = buildParametersJson(data.treatmentType, data as unknown as Record<string, unknown>)

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
        toast.success(t("templateForm.updateSuccess"))
      } else {
        await createMutation.mutateAsync(command)
        toast.success(t("templateForm.createSuccess"))
      }
      onOpenChange(false)
    } catch (error) {
      const nonFieldErrors = handleServerValidationError(error, form.setError)
      if (nonFieldErrors.length > 0) {
        toast.error(nonFieldErrors.join(", "))
      }
    }
  }

  // Bridge between shared component and react-hook-form
  const paramValues = form.watch() as unknown as Record<string, unknown>

  const handleParamChange = useCallback(
    (field: string, value: unknown) => {
      form.setValue(field as keyof ProtocolTemplateFormValues, value as never, {
        shouldDirty: true,
      })
    },
    [form],
  )

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {isEdit ? t("editTemplate") : t("createTemplate")}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Name */}
          <Controller
            name="name"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel required htmlFor={field.name}>{t("fields.name")}</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error?.message}</FieldError>
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
                  <FieldLabel required>{t("fields.treatmentType")}</FieldLabel>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {TREATMENT_TYPE_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>
                          {t(`treatmentType.${opt.value}`)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="defaultSessionCount"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel required htmlFor={field.name}>{t("fields.sessionCount")} (1-6)</FieldLabel>
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
                    <FieldError>{fieldState.error?.message}</FieldError>
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
                  <FieldLabel required>{t("fields.pricingMode")}</FieldLabel>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {PRICING_MODE_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>
                          {t(`pricingMode.${opt.value}`)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="defaultPackagePrice"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel required htmlFor={field.name}>{t("fields.packagePrice")} (VND)</FieldLabel>
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
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="defaultSessionPrice"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel required htmlFor={field.name}>{t("fields.sessionPrice")} (VND)</FieldLabel>
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
                    <FieldError>{fieldState.error?.message}</FieldError>
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
                  <FieldLabel required htmlFor={field.name}>{t("fields.minInterval")}</FieldLabel>
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
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="maxIntervalDays"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel required htmlFor={field.name}>{t("fields.maxInterval")}</FieldLabel>
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
                    <FieldError>{fieldState.error?.message}</FieldError>
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
                <FieldLabel required htmlFor={field.name}>
                  {t("fields.deductionPercent")} (10-20)
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
                  <FieldError>{fieldState.error?.message}</FieldError>
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
                <FieldLabel htmlFor={field.name}>{t("fields.description")}</FieldLabel>
                <AutoResizeTextarea
                  {...field}
                  id={field.name}
                  rows={3}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error?.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Treatment-type-specific parameters (shared component) */}
          <TreatmentParameterFields
            treatmentType={treatmentType}
            values={paramValues}
            onChange={handleParamChange}
          />

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              {t("templateForm.cancel")}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {isEdit ? t("templateForm.update") : t("templateForm.create")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
