import { useEffect, useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useForm, Controller } from "react-hook-form"
import { z } from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Label } from "@/shared/components/Label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import type { AllergyDto } from "@/features/patient/api/patient-api"
import type {
  DrugCatalogItemDto,
  PrescriptionItemInput,
} from "../api/prescription-api"
import { DrugCombobox } from "./DrugCombobox"
import { DrugAllergyWarning } from "./DrugAllergyWarning"

// -- DrugForm enum mapping (matches Pharmacy.Domain.Enums.DrugForm) --
const DRUG_FORM_OPTIONS = [
  { value: 0, labelKey: "drugForm.eyeDrops" },
  { value: 1, labelKey: "drugForm.tablet" },
  { value: 2, labelKey: "drugForm.capsule" },
  { value: 3, labelKey: "drugForm.ointment" },
  { value: 4, labelKey: "drugForm.injection" },
  { value: 5, labelKey: "drugForm.gel" },
  { value: 6, labelKey: "drugForm.solution" },
  { value: 7, labelKey: "drugForm.suspension" },
  { value: 8, labelKey: "drugForm.cream" },
  { value: 9, labelKey: "drugForm.spray" },
] as const

// -- DrugRoute enum mapping (matches Pharmacy.Domain.Enums.DrugRoute) --
const DRUG_ROUTE_OPTIONS = [
  { value: 0, labelKey: "drugRoute.topical" },
  { value: 1, labelKey: "drugRoute.oral" },
  { value: 2, labelKey: "drugRoute.intramuscular" },
  { value: 3, labelKey: "drugRoute.intravenous" },
  { value: 4, labelKey: "drugRoute.subconjunctival" },
  { value: 5, labelKey: "drugRoute.intravitreal" },
  { value: 6, labelKey: "drugRoute.periocular" },
] as const

// -- Frequency options --
const FREQUENCY_OPTIONS = [
  { value: "1 lan/ngay", labelEn: "1 time/day", labelVi: "1 l\u1ea7n/ng\u00e0y" },
  { value: "2 lan/ngay", labelEn: "2 times/day", labelVi: "2 l\u1ea7n/ng\u00e0y" },
  { value: "3 lan/ngay", labelEn: "3 times/day", labelVi: "3 l\u1ea7n/ng\u00e0y" },
  { value: "4 lan/ngay", labelEn: "4 times/day", labelVi: "4 l\u1ea7n/ng\u00e0y" },
  { value: "6 lan/ngay", labelEn: "6 times/day", labelVi: "6 l\u1ea7n/ng\u00e0y" },
  { value: "moi 2 gio", labelEn: "Every 2 hours", labelVi: "M\u1ed7i 2 gi\u1edd" },
  { value: "moi 4 gio", labelEn: "Every 4 hours", labelVi: "M\u1ed7i 4 gi\u1edd" },
  { value: "moi 6 gio", labelEn: "Every 6 hours", labelVi: "M\u1ed7i 6 gi\u1edd" },
  { value: "truoc an", labelEn: "Before meals", labelVi: "Tr\u01b0\u1edbc \u0103n" },
  { value: "sau an", labelEn: "After meals", labelVi: "Sau \u0103n" },
  { value: "khi can", labelEn: "As needed", labelVi: "Khi c\u1ea7n" },
] as const

interface DrugPrescriptionFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSubmit: (item: PrescriptionItemInput) => void
  editingItem?: PrescriptionItemInput | null
  patientAllergies?: AllergyDto[]
}

export function DrugPrescriptionForm({
  open,
  onOpenChange,
  onSubmit,
  editingItem,
  patientAllergies,
}: DrugPrescriptionFormProps) {
  const { t, i18n } = useTranslation("clinical")

  const schema = useMemo(
    () =>
      z.object({
        drugCatalogItemId: z.string().nullable(),
        drugName: z.string().min(1, t("prescription.drugName") + " required"),
        genericName: z.string().nullable(),
        strength: z.string().nullable(),
        form: z.number(),
        route: z.number(),
        doseAmount: z.string(),
        frequency: z.string(),
        durationDays: z.union([z.number().int().positive(), z.nan()]).nullable(),
        dosageOverride: z.string().nullable(),
        quantity: z.number().int().positive(t("prescription.quantity") + " > 0"),
        unit: z.string().min(1, t("prescription.unit") + " required"),
        isOffCatalog: z.boolean(),
        hasAllergyWarning: z.boolean(),
      }),
    [t],
  )

  type FormValues = z.infer<typeof schema>

  const {
    control,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      drugCatalogItemId: null,
      drugName: "",
      genericName: null,
      strength: null,
      form: 0,
      route: 0,
      doseAmount: "",
      frequency: "",
      durationDays: null,
      dosageOverride: null,
      quantity: 1,
      unit: "",
      isOffCatalog: false,
      hasAllergyWarning: false,
    },
  })

  const watchDrugName = watch("drugName")
  const watchGenericName = watch("genericName")
  const watchDoseAmount = watch("doseAmount")
  const watchFrequency = watch("frequency")
  const watchDurationDays = watch("durationDays")

  // Auto-generate dosage text from structured fields
  const autoDosage = useMemo(() => {
    const parts: string[] = []
    if (watchDoseAmount) parts.push(watchDoseAmount)
    if (watchFrequency) {
      const freq = FREQUENCY_OPTIONS.find((f) => f.value === watchFrequency)
      if (freq) {
        parts.push(i18n.language === "vi" ? freq.labelVi : freq.labelEn)
      } else {
        parts.push(watchFrequency)
      }
    }
    if (watchDurationDays && !isNaN(watchDurationDays)) {
      parts.push(
        `${watchDurationDays} ${t("prescription.durationUnit")}`,
      )
    }
    return parts.join(", ")
  }, [watchDoseAmount, watchFrequency, watchDurationDays, i18n.language, t])

  // Pre-fill form when editing
  useEffect(() => {
    if (open && editingItem) {
      reset({
        drugCatalogItemId: editingItem.drugCatalogItemId,
        drugName: editingItem.drugName,
        genericName: editingItem.genericName,
        strength: editingItem.strength,
        form: editingItem.form,
        route: editingItem.route,
        doseAmount: editingItem.doseAmount ?? "",
        frequency: editingItem.frequency ?? "",
        durationDays: editingItem.durationDays,
        dosageOverride: editingItem.dosageOverride,
        quantity: editingItem.quantity,
        unit: editingItem.unit,
        isOffCatalog: !editingItem.drugCatalogItemId,
        hasAllergyWarning: editingItem.hasAllergyWarning,
      })
    } else if (open && !editingItem) {
      reset({
        drugCatalogItemId: null,
        drugName: "",
        genericName: null,
        strength: null,
        form: 0,
        route: 0,
        doseAmount: "",
        frequency: "",
        durationDays: null,
        dosageOverride: null,
        quantity: 1,
        unit: "",
        isOffCatalog: false,
        hasAllergyWarning: false,
      })
    }
  }, [open, editingItem, reset])

  const handleDrugSelect = useCallback(
    (drug: DrugCatalogItemDto, hasAllergyWarning: boolean) => {
      setValue("drugCatalogItemId", drug.id)
      setValue("drugName", drug.name)
      setValue("genericName", drug.genericName)
      setValue("strength", drug.strength)
      setValue("form", drug.form)
      setValue("route", drug.route)
      setValue("unit", drug.unit)
      setValue("isOffCatalog", false)
      setValue("hasAllergyWarning", hasAllergyWarning)
    },
    [setValue],
  )

  const handleOffCatalog = useCallback(
    (drugName: string) => {
      setValue("drugCatalogItemId", null)
      setValue("drugName", drugName)
      setValue("genericName", null)
      setValue("strength", null)
      setValue("isOffCatalog", true)
      // Check allergy for off-catalog drug name
      if (patientAllergies && patientAllergies.length > 0) {
        const dn = drugName.toLowerCase()
        const hasWarning = patientAllergies.some((a) => {
          const an = a.name.toLowerCase()
          return dn.includes(an) || an.includes(dn)
        })
        setValue("hasAllergyWarning", hasWarning)
      }
    },
    [setValue, patientAllergies],
  )

  const onFormSubmit = useCallback(
    (values: FormValues) => {
      const item: PrescriptionItemInput = {
        drugCatalogItemId: values.drugCatalogItemId,
        drugName: values.drugName,
        genericName: values.genericName,
        strength: values.strength,
        form: values.form,
        route: values.route,
        doseAmount: values.doseAmount || null,
        dosage: values.dosageOverride?.trim() ? null : autoDosage || null,
        dosageOverride: values.dosageOverride?.trim() || null,
        quantity: values.quantity,
        unit: values.unit,
        frequency: values.frequency || null,
        durationDays:
          values.durationDays && !isNaN(values.durationDays)
            ? values.durationDays
            : null,
        hasAllergyWarning: values.hasAllergyWarning,
      }
      onSubmit(item)
      onOpenChange(false)
    },
    [autoDosage, onSubmit, onOpenChange],
  )

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {editingItem
              ? t("prescription.editDrug")
              : t("prescription.addDrug")}
          </DialogTitle>
        </DialogHeader>

        <form
          onSubmit={handleSubmit(onFormSubmit)}
          className="space-y-4"
        >
          {/* Drug selection */}
          {!editingItem && (
            <div className="space-y-1.5">
              <Label>{t("prescription.drugName")}</Label>
              <DrugCombobox
                onSelect={handleDrugSelect}
                patientAllergies={patientAllergies}
                onOffCatalog={handleOffCatalog}
              />
              {watchDrugName && (
                <p className="text-sm font-medium">{watchDrugName}</p>
              )}
            </div>
          )}

          {/* Allergy warning */}
          {watchDrugName && patientAllergies && patientAllergies.length > 0 && (
            <DrugAllergyWarning
              allergies={patientAllergies}
              drugName={watchDrugName}
              genericName={watchGenericName}
            />
          )}

          {/* Drug name for edit mode */}
          {editingItem && (
            <div className="space-y-1.5">
              <Label>{t("prescription.drugName")}</Label>
              <Controller
                control={control}
                name="drugName"
                render={({ field }) => (
                  <Input {...field} disabled={!!editingItem.drugCatalogItemId} />
                )}
              />
              {errors.drugName && (
                <p className="text-destructive text-sm">
                  {errors.drugName.message}
                </p>
              )}
            </div>
          )}

          {/* Strength */}
          <div className="space-y-1.5">
            <Label>{t("prescription.strength")}</Label>
            <Controller
              control={control}
              name="strength"
              render={({ field }) => (
                <Input
                  value={field.value ?? ""}
                  onChange={(e) =>
                    field.onChange(e.target.value || null)
                  }
                />
              )}
            />
          </div>

          {/* Form + Route row */}
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>{t("prescription.form")}</Label>
              <Controller
                control={control}
                name="form"
                render={({ field }) => (
                  <Select
                    value={String(field.value)}
                    onValueChange={(v) => field.onChange(Number(v))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {DRUG_FORM_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={String(opt.value)}>
                          {t(opt.labelKey)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
            </div>
            <div className="space-y-1.5">
              <Label>{t("prescription.route")}</Label>
              <Controller
                control={control}
                name="route"
                render={({ field }) => (
                  <Select
                    value={String(field.value)}
                    onValueChange={(v) => field.onChange(Number(v))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {DRUG_ROUTE_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={String(opt.value)}>
                          {t(opt.labelKey)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
            </div>
          </div>

          {/* Structured dosage: Dose amount + Frequency + Duration */}
          <div className="grid grid-cols-3 gap-3">
            <div className="space-y-1.5">
              <Label>{t("prescription.dosage")}</Label>
              <Controller
                control={control}
                name="doseAmount"
                render={({ field }) => <Input {...field} />}
              />
            </div>
            <div className="space-y-1.5">
              <Label>{t("prescription.frequency")}</Label>
              <Controller
                control={control}
                name="frequency"
                render={({ field }) => (
                  <Select
                    value={field.value || ""}
                    onValueChange={field.onChange}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {FREQUENCY_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>
                          {i18n.language === "vi" ? opt.labelVi : opt.labelEn}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
            </div>
            <div className="space-y-1.5">
              <Label>{t("prescription.duration")}</Label>
              <Controller
                control={control}
                name="durationDays"
                render={({ field }) => (
                  <Input
                    type="number"
                    min={1}
                    value={
                      field.value != null && !isNaN(field.value)
                        ? field.value
                        : ""
                    }
                    onChange={(e) => {
                      const v = e.target.value
                      field.onChange(v ? parseInt(v, 10) : null)
                    }}
                  />
                )}
              />
            </div>
          </div>

          {/* Auto-generated dosage preview */}
          {autoDosage && (
            <p className="text-xs text-muted-foreground">
              {autoDosage}
            </p>
          )}

          {/* Dosage Override (free-text) */}
          <div className="space-y-1.5">
            <Label>{t("prescription.dosageOverride")}</Label>
            <Controller
              control={control}
              name="dosageOverride"
              render={({ field }) => (
                <AutoResizeTextarea
                  value={field.value ?? ""}
                  onChange={(e) =>
                    field.onChange(e.target.value || null)
                  }
                  rows={2}
                />
              )}
            />
          </div>

          {/* Quantity + Unit row */}
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>{t("prescription.quantity")}</Label>
              <Controller
                control={control}
                name="quantity"
                render={({ field }) => (
                  <Input
                    type="number"
                    min={1}
                    value={field.value}
                    onChange={(e) => field.onChange(parseInt(e.target.value, 10) || 1)}
                  />
                )}
              />
              {errors.quantity && (
                <p className="text-destructive text-sm">
                  {errors.quantity.message}
                </p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label>{t("prescription.unit")}</Label>
              <Controller
                control={control}
                name="unit"
                render={({ field }) => <Input {...field} />}
              />
              {errors.unit && (
                <p className="text-destructive text-sm">
                  {errors.unit.message}
                </p>
              )}
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t("images.cancel")}
            </Button>
            <Button type="submit" disabled={!watchDrugName}>
              {editingItem ? t("prescription.editDrug") : t("prescription.addDrug")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
