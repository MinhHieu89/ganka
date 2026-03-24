import { useState, useMemo } from "react"
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
import { NumberInput } from "@/shared/components/NumberInput"
import { Button } from "@/shared/components/Button"
import { Checkbox } from "@/shared/components/Checkbox"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import {
  type LensCatalogItemDto,
  LENS_MATERIAL_MAP,
  LENS_COATING_MAP,
  LENS_COATING_BITS,
  LENS_TYPE_OPTIONS,
  decodeCoatings,
  encodeCoatings,
} from "@/features/optical/api/optical-api"
import {
  useCreateLensCatalogItem,
  useUpdateLensCatalogItem,
  useAdjustLensStock,
} from "@/features/optical/api/optical-queries"

// ---- Catalog Item schema ----

function createCatalogSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    brand: z.string().min(1, v.required).max(100),
    name: z.string().min(1, v.required).max(200),
    lensType: z.string().min(1, v.required),
    material: z.number().min(0),
    availableCoatings: z.number().min(0),
    sellingPrice: z.number().positive(v.mustBePositive),
    costPrice: z.number().positive(v.mustBePositive),
    preferredSupplierId: z.string().nullable().optional(),
  })
}

type CatalogFormValues = z.infer<ReturnType<typeof createCatalogSchema>>

// ---- Stock Adjustment schema ----

function createStockSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    sph: z
      .number()
      .min(-20, v.between(-20, 20))
      .max(20, v.between(-20, 20))
      .refine((val) => Math.abs((val * 4) % 1) < 0.01, v.mustBeNonNegative),
    cyl: z
      .number()
      .min(-10, v.between(-10, 0))
      .max(0, v.between(-10, 0))
      .refine((val) => Math.abs((val * 4) % 1) < 0.01, v.mustBeNonNegative),
    add: z
      .number()
      .min(0, v.between(0, 4))
      .max(4, v.between(0, 4))
      .refine((val) => Math.abs((val * 4) % 1) < 0.01, v.mustBeNonNegative)
      .nullable()
      .optional(),
    quantityChange: z.number().int(v.mustBeInteger).refine((val) => val !== 0, v.cannotBeZero),
    minStockLevel: z.number().int(v.mustBeInteger).min(0, v.mustBeNonNegative),
  })
}

type StockFormValues = z.infer<ReturnType<typeof createStockSchema>>

// ---- Props ----

interface LensFormDialogProps {
  mode: "create" | "edit" | "stock"
  lens?: LensCatalogItemDto
  open: boolean
  onOpenChange: (open: boolean) => void
}

// ---- Catalog Item Form ----

function CatalogItemForm({
  mode,
  lens,
  onClose,
}: {
  mode: "create" | "edit"
  lens?: LensCatalogItemDto
  onClose: () => void
}) {
  const { t } = useTranslation("optical")
  const { t: tCommon } = useTranslation("common")
  const createMutation = useCreateLensCatalogItem()
  const updateMutation = useUpdateLensCatalogItem()

  const catalogSchema = useMemo(() => createCatalogSchema(tCommon), [tCommon])
  const form = useForm<CatalogFormValues>({
    resolver: zodResolver(catalogSchema),
    defaultValues: lens
      ? {
          brand: lens.brand,
          name: lens.name,
          lensType: lens.lensType,
          material: lens.material,
          availableCoatings: lens.availableCoatings,
          sellingPrice: lens.sellingPrice,
          costPrice: lens.costPrice,
          preferredSupplierId: lens.preferredSupplierId,
        }
      : {
          brand: "",
          name: "",
          lensType: "single_vision",
          material: 0,
          availableCoatings: 0,
          sellingPrice: 0,
          costPrice: 0,
          preferredSupplierId: null,
        },
  })

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  const handleSubmit = async (data: CatalogFormValues) => {
    try {
      if (mode === "create") {
        await createMutation.mutateAsync({
          brand: data.brand,
          name: data.name,
          lensType: data.lensType,
          material: data.material,
          availableCoatings: data.availableCoatings,
          sellingPrice: data.sellingPrice,
          costPrice: data.costPrice,
          preferredSupplierId: data.preferredSupplierId ?? null,
        })
        toast.success(t("lenses.created"))
      } else if (lens) {
        await updateMutation.mutateAsync({
          id: lens.id,
          brand: data.brand,
          name: data.name,
          lensType: data.lensType,
          material: data.material,
          availableCoatings: data.availableCoatings,
          sellingPrice: data.sellingPrice,
          costPrice: data.costPrice,
          preferredSupplierId: data.preferredSupplierId ?? null,
          isActive: lens.isActive,
        })
        toast.success(t("lenses.updated"))
      }
      onClose()
    } catch {
      // onError in mutation handles toast
    }
  }

  const coatingValue = form.watch("availableCoatings")
  const currentCoatings = decodeCoatings(coatingValue)

  const toggleCoating = (bit: number) => {
    const updated = currentCoatings.includes(bit)
      ? currentCoatings.filter((b) => b !== bit)
      : [...currentCoatings, bit]
    form.setValue("availableCoatings", encodeCoatings(updated))
  }

  return (
    <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <Controller
          name="brand"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>{t("lenses.brand")}</FieldLabel>
              <Input {...field} id={field.name} aria-invalid={fieldState.invalid || undefined} />
              {fieldState.error && (
                <FieldError>{fieldState.error?.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          name="name"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>{t("lenses.name")}</FieldLabel>
              <Input {...field} id={field.name} aria-invalid={fieldState.invalid || undefined} />
              {fieldState.error && (
                <FieldError>{fieldState.error?.message}</FieldError>
              )}
            </Field>
          )}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <Controller
          name="lensType"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel>{t("lenses.lensType")}</FieldLabel>
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {LENS_TYPE_OPTIONS.map((opt) => (
                    <SelectItem key={opt.value} value={opt.value}>
                      {t(opt.label)}
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
          name="material"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel>{t("lenses.material")}</FieldLabel>
              <Select
                value={String(field.value)}
                onValueChange={(v) => field.onChange(Number(v))}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {Object.entries(LENS_MATERIAL_MAP).map(([value, label]) => (
                    <SelectItem key={value} value={value}>
                      {t(label)}
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
      </div>

      <div>
        <FieldLabel className="mb-2 block">{t("lenses.coatings")}</FieldLabel>
        <div className="grid grid-cols-2 gap-2">
          {LENS_COATING_BITS.map((bit) => (
            <label
              key={bit}
              className="flex items-center gap-2 cursor-pointer text-sm"
            >
              <Checkbox
                checked={currentCoatings.includes(bit)}
                onCheckedChange={() => toggleCoating(bit)}
              />
              {t(LENS_COATING_MAP[bit])}
            </label>
          ))}
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <Controller
          name="sellingPrice"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>{t("lenses.sellingPrice")}</FieldLabel>
              <NumberInput
                {...field}
                id={field.name}
                min={0}
                step={1000}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{fieldState.error?.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          name="costPrice"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>{t("lenses.costPrice")}</FieldLabel>
              <NumberInput
                {...field}
                id={field.name}
                min={0}
                step={1000}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{fieldState.error?.message}</FieldError>
              )}
            </Field>
          )}
        />
      </div>

      <DialogFooter>
        <Button type="button" variant="outline" onClick={onClose}>
          {t("common.cancel")}
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting && <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />}
          {t("common.save")}
        </Button>
      </DialogFooter>
    </form>
  )
}

// ---- Diopter Input (handles negative typing) ----

function DioptrField({
  name,
  label,
  control,
  min,
  max,
  optional,
}: {
  name: "sph" | "cyl" | "add"
  label: string
  control: ReturnType<typeof useForm<StockFormValues>>["control"]
  min: number
  max: number
  optional?: boolean
}) {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => {
        const [raw, setRaw] = useState(
          field.value == null ? "" : String(field.value),
        )
        return (
          <Field data-invalid={fieldState.invalid || undefined}>
            <FieldLabel htmlFor={field.name}>
              {label}
              {optional && " (optional)"}
            </FieldLabel>
            <Input
              id={field.name}
              inputMode="decimal"
              value={raw}
              onChange={(e) => {
                const v = e.target.value
                setRaw(v)
                if (v === "" || v === "-" || v === "-0" || v === "-0.") return
                const num = parseFloat(v)
                if (!Number.isNaN(num)) {
                  field.onChange(optional && v === "" ? null : num)
                }
              }}
              onBlur={() => {
                field.onBlur()
                if (raw === "" && optional) {
                  field.onChange(null)
                  return
                }
                const num = parseFloat(raw)
                if (Number.isNaN(num)) {
                  setRaw("0")
                  field.onChange(optional ? null : 0)
                } else {
                  const clamped = Math.max(min, Math.min(max, num))
                  const stepped = Math.round(clamped * 4) / 4
                  setRaw(String(stepped))
                  field.onChange(stepped)
                }
              }}
              aria-invalid={fieldState.invalid || undefined}
            />
            {fieldState.error && (
              <FieldError>{fieldState.error?.message}</FieldError>
            )}
          </Field>
        )
      }}
    />
  )
}

// ---- Stock Adjustment Form ----

function StockAdjustmentForm({
  lens,
  onClose,
}: {
  lens: LensCatalogItemDto
  onClose: () => void
}) {
  const { t } = useTranslation("optical")
  const { t: tCommon } = useTranslation("common")
  const adjustMutation = useAdjustLensStock()

  const stockSchema = useMemo(() => createStockSchema(tCommon), [tCommon])
  const form = useForm<StockFormValues>({
    resolver: zodResolver(stockSchema),
    defaultValues: {
      sph: 0,
      cyl: 0,
      add: null,
      quantityChange: 1,
      minStockLevel: 2,
    },
  })

  const isSubmitting = adjustMutation.isPending

  const handleSubmit = async (data: StockFormValues) => {
    try {
      await adjustMutation.mutateAsync({
        lensCatalogItemId: lens.id,
        sph: data.sph,
        cyl: data.cyl,
        add: data.add ?? null,
        quantityChange: data.quantityChange,
        reason: "Manual stock adjustment",
        minStockLevel: data.minStockLevel,
      })
      toast.success(t("lenses.stockAdjusted"))
      onClose()
    } catch {
      // onError in mutation handles toast
    }
  }

  return (
    <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
      <div className="rounded-md bg-muted/50 px-3 py-2 text-sm">
        <span className="font-medium">{lens.brand} {lens.name}</span>
        <span className="text-muted-foreground ml-2">— {t("lenses.stockAdjustHint")}</span>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <DioptrField name="sph" label="SPH" control={form.control} min={-20} max={20} />
        <DioptrField name="cyl" label="CYL" control={form.control} min={-10} max={0} />
        <DioptrField name="add" label="ADD" control={form.control} min={0} max={4} optional />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <Controller
          name="quantityChange"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>
                {t("lenses.quantityChange")}
                <span className="text-xs text-muted-foreground ml-1">({t("lenses.quantityChangeHint")})</span>
              </FieldLabel>
              <NumberInput
                {...field}
                id={field.name}
                step={1}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{fieldState.error?.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          name="minStockLevel"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>{t("lenses.minStock")}</FieldLabel>
              <NumberInput
                {...field}
                id={field.name}
                step={1}
                min={0}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{fieldState.error?.message}</FieldError>
              )}
            </Field>
          )}
        />
      </div>

      <DialogFooter>
        <Button type="button" variant="outline" onClick={onClose}>
          {t("common.cancel")}
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting && <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />}
          {t("lenses.adjustStock")}
        </Button>
      </DialogFooter>
    </form>
  )
}

// ---- Main Dialog ----

export function LensFormDialog({ mode, lens, open, onOpenChange }: LensFormDialogProps) {
  const { t } = useTranslation("optical")
  const handleClose = () => onOpenChange(false)

  const title =
    mode === "create"
      ? t("lenses.addLens")
      : mode === "edit"
        ? t("lenses.editLens")
        : `${t("lenses.adjustStock")} — ${lens?.brand ?? ""} ${lens?.name ?? ""}`

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        {mode === "stock" && lens ? (
          <StockAdjustmentForm lens={lens} onClose={handleClose} />
        ) : (
          <CatalogItemForm key={lens?.id ?? "create"} mode={mode === "create" ? "create" : "edit"} lens={lens} onClose={handleClose} />
        )}
      </DialogContent>
    </Dialog>
  )
}
