import { useState, useEffect, useCallback, useDeferredValue, useMemo } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { createValidationMessages } from "@/shared/lib/validation"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"
import { IconLoader2, IconSearch, IconUser } from "@tabler/icons-react"
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { Separator } from "@/shared/components/Separator"
import { formatVND } from "@/shared/lib/format-vnd"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import {
  useProtocolTemplates,
  useCreateTreatmentPackage,
} from "@/features/treatment/api/treatment-api"
import { usePatientSearch } from "@/features/patient/hooks/usePatientSearch"
import type {
  TreatmentProtocolDto,
  CreateTreatmentPackageCommand,
} from "@/features/treatment/api/treatment-types"
import {
  TreatmentParameterFields,
  buildParametersJson,
  parseParametersJson,
} from "./TreatmentParameterFields"

// -- Pricing mode enum values matching backend --
const PRICING_MODE_PER_SESSION = 0
const PRICING_MODE_PER_PACKAGE = 1

// -- Schema --

function createPackageFormSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    protocolTemplateId: z.string().min(1, v.selectRequired),
    patientId: z.string().min(1, v.selectRequired),
  patientName: z.string().min(1),
  totalSessions: z.coerce.number().int().min(1).max(6).nullable().optional(),
  pricingMode: z.coerce.number().int().min(0).max(1).nullable().optional(),
  packagePrice: z.coerce.number().min(0).nullable().optional(),
  sessionPrice: z.coerce.number().min(0).nullable().optional(),
  minIntervalDays: z.coerce.number().int().min(1).nullable().optional(),
  parametersJson: z.string().nullable().optional(),
    visitId: z.string().nullable().optional(),
  })
}

type PackageFormValues = z.infer<ReturnType<typeof createPackageFormSchema>>

// -- Props --

interface TreatmentPackageFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  visitId?: string
  patientId?: string
  patientName?: string
}

// -- Component --

export function TreatmentPackageForm({
  open,
  onOpenChange,
  visitId,
  patientId: presetPatientId,
  patientName: presetPatientName,
}: TreatmentPackageFormProps) {
  const { t } = useTranslation("treatment")
  const { t: tCommon } = useTranslation("common")
  const { data: templates = [] } = useProtocolTemplates()
  const createMutation = useCreateTreatmentPackage()

  const [selectedTemplate, setSelectedTemplate] =
    useState<TreatmentProtocolDto | null>(null)

  // Structured parameter fields state
  const [paramFields, setParamFields] = useState<Record<string, unknown>>({})

  // Patient search state
  const [patientSearchTerm, setPatientSearchTerm] = useState("")
  const deferredSearch = useDeferredValue(patientSearchTerm)
  const { data: searchResults } = usePatientSearch(deferredSearch, {
    enabled: deferredSearch.length >= 2,
  })
  const [selectedPatientDisplay, setSelectedPatientDisplay] = useState<{
    id: string
    name: string
    phone: string
    code: string
  } | null>(null)

  const packageFormSchema = useMemo(() => createPackageFormSchema(tCommon), [tCommon])
  const form = useForm<PackageFormValues>({
    resolver: zodResolver(packageFormSchema),
    defaultValues: {
      protocolTemplateId: "",
      patientId: presetPatientId ?? "",
      patientName: presetPatientName ?? "",
      totalSessions: null,
      pricingMode: null,
      packagePrice: null,
      sessionPrice: null,
      minIntervalDays: null,
      parametersJson: null,
      visitId: visitId ?? null,
    },
  })

  // Reset form when dialog opens
  useEffect(() => {
    if (open) {
      form.reset({
        protocolTemplateId: "",
        patientId: presetPatientId ?? "",
        patientName: presetPatientName ?? "",
        totalSessions: null,
        pricingMode: null,
        packagePrice: null,
        sessionPrice: null,
        minIntervalDays: null,
        parametersJson: null,
        visitId: visitId ?? null,
      })
      setSelectedTemplate(null)
      setParamFields({})
      if (presetPatientId && presetPatientName) {
        setSelectedPatientDisplay({
          id: presetPatientId,
          name: presetPatientName,
          phone: "",
          code: "",
        })
      } else {
        setSelectedPatientDisplay(null)
      }
      setPatientSearchTerm("")
    }
  }, [open, visitId, presetPatientId, presetPatientName, form])

  // Handle template selection - pre-populate form
  const handleTemplateSelect = (templateId: string) => {
    const template = templates.find((t) => t.id === templateId)
    if (!template) return

    setSelectedTemplate(template)
    form.setValue("protocolTemplateId", templateId)
    form.setValue("totalSessions", template.defaultSessionCount)
    form.setValue(
      "pricingMode",
      template.pricingMode === "PerPackage"
        ? PRICING_MODE_PER_PACKAGE
        : PRICING_MODE_PER_SESSION,
    )
    form.setValue("packagePrice", template.defaultPackagePrice)
    form.setValue("sessionPrice", template.defaultSessionPrice)
    form.setValue("minIntervalDays", template.minIntervalDays)

    // Parse template parameters into structured fields
    const parsed = parseParametersJson(
      template.treatmentType,
      template.defaultParametersJson,
    )
    setParamFields(parsed)
  }

  // Handle parameter field changes from shared component
  const handleParamFieldChange = useCallback(
    (field: string, value: unknown) => {
      setParamFields((prev) => ({ ...prev, [field]: value }))
    },
    [],
  )

  // Handle patient selection
  const handleSelectPatient = (patient: {
    id: string
    fullName: string
    phone: string
    patientCode: string
  }) => {
    form.setValue("patientId", patient.id)
    form.setValue("patientName", patient.fullName)
    setSelectedPatientDisplay({
      id: patient.id,
      name: patient.fullName,
      phone: patient.phone,
      code: patient.patientCode,
    })
    setPatientSearchTerm("")
  }

  const clearPatient = () => {
    form.setValue("patientId", "")
    form.setValue("patientName", "")
    setSelectedPatientDisplay(null)
  }

  const isSubmitting = createMutation.isPending

  const handleSubmit = async (data: PackageFormValues) => {
    try {
      // Convert structured parameter fields back to JSON string for the API
      const parametersJson = selectedTemplate
        ? buildParametersJson(selectedTemplate.treatmentType, paramFields)
        : data.parametersJson

      const command: CreateTreatmentPackageCommand = {
        protocolTemplateId: data.protocolTemplateId,
        patientId: data.patientId,
        patientName: data.patientName,
        totalSessions: data.totalSessions,
        pricingMode: data.pricingMode,
        packagePrice: data.packagePrice,
        sessionPrice: data.sessionPrice,
        minIntervalDays: data.minIntervalDays,
        parametersJson: parametersJson ?? null,
        visitId: data.visitId,
      }
      await createMutation.mutateAsync(command)
      toast.success(t("packageForm.createSuccess"))
      onOpenChange(false)
    } catch (error) {
      handleServerValidationError(error, form.setError)
    }
  }

  const activeTemplates = templates.filter((t) => t.isActive)

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{t("packageForm.title")}</DialogTitle>
        </DialogHeader>

        <form
          onSubmit={form.handleSubmit(handleSubmit)}
          className="space-y-4"
        >
          {/* Step 1: Select protocol template */}
          <Controller
            name="protocolTemplateId"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel required htmlFor="protocolTemplateId">
                  {t("packageForm.selectTemplate")}
                </FieldLabel>
                <Select
                  value={field.value}
                  onValueChange={(v) => {
                    field.onChange(v)
                    handleTemplateSelect(v)
                  }}
                >
                  <SelectTrigger id="protocolTemplateId">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {activeTemplates.map((tmpl) => (
                      <SelectItem key={tmpl.id} value={tmpl.id}>
                        {tmpl.name} ({tmpl.treatmentType})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Template info */}
          {selectedTemplate && (
            <div className="text-xs text-muted-foreground rounded-md bg-muted/50 p-3 space-y-1">
              <div>
                <span className="font-medium">{t("fields.treatmentType")}:</span>{" "}
                {selectedTemplate.treatmentType}
              </div>
              <div>
                <span className="font-medium">{t("fields.sessionCount")}:</span>{" "}
                {selectedTemplate.defaultSessionCount}
              </div>
              <div>
                <span className="font-medium">{t("packageForm.templateDefaults")}:</span>{" "}
                {selectedTemplate.pricingMode === "PerPackage"
                  ? formatVND(selectedTemplate.defaultPackagePrice)
                  : `${formatVND(selectedTemplate.defaultSessionPrice)}/${t("cancellationDialog.perSession")}`}
              </div>
              {selectedTemplate.description && (
                <div>
                  <span className="font-medium">{t("fields.description")}:</span>{" "}
                  {selectedTemplate.description}
                </div>
              )}
            </div>
          )}

          <Separator />

          {/* Step 2: Patient selector (hidden when patient is preset from context) */}
          {!presetPatientId ? (
            <div className="space-y-2">
              <Label>{t("packageForm.selectPatient")}</Label>
              {selectedPatientDisplay ? (
                <div className="flex items-center gap-2 p-2 border rounded-md">
                  <IconUser className="h-4 w-4 text-muted-foreground" />
                  <div className="flex-1">
                    <div className="text-sm font-medium">
                      {selectedPatientDisplay.name}
                    </div>
                    <div className="text-xs text-muted-foreground">
                      {selectedPatientDisplay.phone}
                      {selectedPatientDisplay.code &&
                        ` - ${selectedPatientDisplay.code}`}
                    </div>
                  </div>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={clearPatient}
                  >
                    &times;
                  </Button>
                </div>
              ) : (
                <div className="space-y-1">
                  <div className="relative">
                    <IconSearch className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                    <Input
                      className="pl-8"
                      placeholder={t("packageForm.searchPatient")}
                      value={patientSearchTerm}
                      onChange={(e) => setPatientSearchTerm(e.target.value)}
                    />
                  </div>
                  {searchResults && searchResults.length > 0 && (
                    <div className="border rounded-md max-h-48 overflow-y-auto">
                      {searchResults.map((p) => (
                        <button
                          key={p.id}
                          type="button"
                          className="w-full text-left px-3 py-2 text-sm hover:bg-accent transition-colors"
                          onClick={() => handleSelectPatient(p)}
                        >
                          <div className="font-medium">{p.fullName}</div>
                          <div className="text-xs text-muted-foreground">
                            {p.phone}
                            {p.patientCode && ` - ${p.patientCode}`}
                          </div>
                        </button>
                      ))}
                    </div>
                  )}
                  {form.formState.errors.patientId && (
                    <FieldError>
                      {form.formState.errors.patientId.message}
                    </FieldError>
                  )}
                </div>
              )}
            </div>
          ) : (
            <div className="flex items-center gap-2 p-2 border rounded-md">
              <IconUser className="h-4 w-4 text-muted-foreground" />
              <div className="text-sm font-medium">{presetPatientName}</div>
            </div>
          )}

          {/* Customization fields (all optional, defaults from template) */}
          {selectedTemplate && (
            <>
              <Separator />

              <div className="grid grid-cols-2 gap-4">
                {/* Total sessions */}
                <Controller
                  name="totalSessions"
                  control={form.control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid || undefined}>
                      <FieldLabel htmlFor="totalSessions">
                        {t("packageForm.overrideSessions")}
                      </FieldLabel>
                      <Input
                        {...field}
                        id="totalSessions"
                        type="number"
                        min={1}
                        max={6}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value ? Number(e.target.value) : null,
                          )
                        }
                      />
                      {fieldState.error && (
                        <FieldError>{fieldState.error.message}</FieldError>
                      )}
                    </Field>
                  )}
                />

                {/* Min interval days */}
                <Controller
                  name="minIntervalDays"
                  control={form.control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid || undefined}>
                      <FieldLabel htmlFor="minIntervalDays">
                        {t("fields.minInterval")}
                      </FieldLabel>
                      <Input
                        {...field}
                        id="minIntervalDays"
                        type="number"
                        min={1}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value ? Number(e.target.value) : null,
                          )
                        }
                      />
                      {fieldState.error && (
                        <FieldError>{fieldState.error.message}</FieldError>
                      )}
                    </Field>
                  )}
                />
              </div>

              {/* Pricing mode */}
              <Controller
                name="pricingMode"
                control={form.control}
                render={({ field }) => (
                  <Field>
                    <FieldLabel>{t("packageForm.overridePricing")}</FieldLabel>
                    <Select
                      value={field.value != null ? String(field.value) : ""}
                      onValueChange={(v) => field.onChange(Number(v))}
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value={String(PRICING_MODE_PER_SESSION)}>
                          {t("pricingMode.PerSession")}
                        </SelectItem>
                        <SelectItem value={String(PRICING_MODE_PER_PACKAGE)}>
                          {t("pricingMode.PerPackage")}
                        </SelectItem>
                      </SelectContent>
                    </Select>
                  </Field>
                )}
              />

              <div className="grid grid-cols-2 gap-4">
                {/* Package price */}
                <Controller
                  name="packagePrice"
                  control={form.control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid || undefined}>
                      <FieldLabel htmlFor="packagePrice">
                        {t("fields.packagePrice")}
                      </FieldLabel>
                      <Input
                        {...field}
                        id="packagePrice"
                        type="number"
                        min={0}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value ? Number(e.target.value) : null,
                          )
                        }
                      />
                      {fieldState.error && (
                        <FieldError>{fieldState.error.message}</FieldError>
                      )}
                    </Field>
                  )}
                />

                {/* Session price */}
                <Controller
                  name="sessionPrice"
                  control={form.control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid || undefined}>
                      <FieldLabel htmlFor="sessionPrice">
                        {t("fields.sessionPrice")}
                      </FieldLabel>
                      <Input
                        {...field}
                        id="sessionPrice"
                        type="number"
                        min={0}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value ? Number(e.target.value) : null,
                          )
                        }
                      />
                      {fieldState.error && (
                        <FieldError>{fieldState.error.message}</FieldError>
                      )}
                    </Field>
                  )}
                />
              </div>

              {/* Structured treatment parameters (replaces raw JSON textarea) */}
              <TreatmentParameterFields
                treatmentType={selectedTemplate.treatmentType}
                values={paramFields}
                onChange={handleParamFieldChange}
              />
            </>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              {t("packageForm.cancel")}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {t("packageForm.submit")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
