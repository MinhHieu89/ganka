import { useEffect } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
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

const catalogSchema = z.object({
  brand: z.string().min(1, "required").max(100),
  name: z.string().min(1, "required").max(200),
  lensType: z.string().min(1, "required"),
  material: z.number().min(0),
  availableCoatings: z.number().min(0),
  sellingPrice: z.number().positive("must be > 0"),
  costPrice: z.number().positive("must be > 0"),
  preferredSupplierId: z.string().nullable().optional(),
})

type CatalogFormValues = z.infer<typeof catalogSchema>

// ---- Stock Adjustment schema ----

const stockSchema = z.object({
  sph: z
    .number()
    .min(-20, "min −20.00")
    .max(20, "max +20.00")
    .refine((v) => Math.abs((v * 4) % 1) < 0.01, "must be a 0.25 step"),
  cyl: z
    .number()
    .min(-10, "min −10.00")
    .max(0, "max 0.00")
    .refine((v) => Math.abs((v * 4) % 1) < 0.01, "must be a 0.25 step"),
  add: z
    .number()
    .min(0, "min 0.00")
    .max(4, "max +4.00")
    .refine((v) => Math.abs((v * 4) % 1) < 0.01, "must be a 0.25 step")
    .nullable()
    .optional(),
  quantityChange: z.number().int("must be integer").refine((v) => v !== 0, "cannot be zero"),
  minStockLevel: z.number().int("must be integer").min(0, "min 0"),
})

type StockFormValues = z.infer<typeof stockSchema>

// ---- Props ----

interface LensFormDialogProps {
  mode: "create" | "edit" | "stock"
  lens?: LensCatalogItemDto
  open: boolean
  onOpenChange: (open: boolean) => void
}

// ---- Helpers ----

function getErrorMessage(error: { message?: string } | undefined, tRequired = "Required"): string | undefined {
  if (!error?.message) return undefined
  if (error.message === "required") return tRequired
  return error.message
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
  const createMutation = useCreateLensCatalogItem()
  const updateMutation = useUpdateLensCatalogItem()

  const form = useForm<CatalogFormValues>({
    resolver: zodResolver(catalogSchema),
    defaultValues: {
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

  useEffect(() => {
    if (!lens) {
      form.reset({
        brand: "",
        name: "",
        lensType: "single_vision",
        material: 0,
        availableCoatings: 0,
        sellingPrice: 0,
        costPrice: 0,
        preferredSupplierId: null,
      })
    } else {
      form.reset({
        brand: lens.brand,
        name: lens.name,
        lensType: lens.lensType,
        material: lens.material,
        availableCoatings: lens.availableCoatings,
        sellingPrice: lens.sellingPrice,
        costPrice: lens.costPrice,
        preferredSupplierId: lens.preferredSupplierId,
      })
    }
  }, [lens, form])

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
        toast.success("Lens catalog item created")
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
        toast.success("Lens catalog item updated")
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
              <FieldLabel htmlFor={field.name}>Brand</FieldLabel>
              <Input {...field} id={field.name} aria-invalid={fieldState.invalid || undefined} />
              {fieldState.error && (
                <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          name="name"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>Name</FieldLabel>
              <Input {...field} id={field.name} aria-invalid={fieldState.invalid || undefined} />
              {fieldState.error && (
                <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
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
              <FieldLabel>Lens Type</FieldLabel>
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {LENS_TYPE_OPTIONS.map((opt) => (
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
          name="material"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel>Material</FieldLabel>
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
                      {label}
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
      </div>

      <div>
        <FieldLabel className="mb-2 block">Available Coatings</FieldLabel>
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
              {LENS_COATING_MAP[bit]}
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
              <FieldLabel htmlFor={field.name}>Selling Price</FieldLabel>
              <Input
                {...field}
                id={field.name}
                type="number"
                min={0}
                step={1000}
                onChange={(e) => field.onChange(Number(e.target.value))}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          name="costPrice"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>Cost Price</FieldLabel>
              <Input
                {...field}
                id={field.name}
                type="number"
                min={0}
                step={1000}
                onChange={(e) => field.onChange(Number(e.target.value))}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />
      </div>

      <DialogFooter>
        <Button type="button" variant="outline" onClick={onClose}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting && <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />}
          Save
        </Button>
      </DialogFooter>
    </form>
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
  const adjustMutation = useAdjustLensStock()

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
      })
      toast.success(
        data.quantityChange > 0
          ? `Added ${data.quantityChange} unit(s) to stock`
          : `Removed ${Math.abs(data.quantityChange)} unit(s) from stock`,
      )
      onClose()
    } catch {
      // onError in mutation handles toast
    }
  }

  return (
    <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
      <div className="rounded-md bg-muted/50 px-3 py-2 text-sm">
        <span className="font-medium">{lens.brand} {lens.name}</span>
        <span className="text-muted-foreground ml-2">— add or adjust stock for a power combination</span>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <Controller
          name="sph"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>SPH</FieldLabel>
              <Input
                {...field}
                id={field.name}
                type="number"
                step={0.25}
                min={-20}
                max={20}
                onChange={(e) => field.onChange(Number(e.target.value))}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          name="cyl"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>CYL</FieldLabel>
              <Input
                {...field}
                id={field.name}
                type="number"
                step={0.25}
                min={-10}
                max={0}
                onChange={(e) => field.onChange(Number(e.target.value))}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          name="add"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>ADD (optional)</FieldLabel>
              <Input
                id={field.name}
                type="number"
                step={0.25}
                min={0}
                max={4}
                value={field.value ?? ""}
                onChange={(e) =>
                  field.onChange(e.target.value === "" ? null : Number(e.target.value))
                }
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <Controller
          name="quantityChange"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>
                Quantity Change
                <span className="text-xs text-muted-foreground ml-1">(+ to add, − to remove)</span>
              </FieldLabel>
              <Input
                {...field}
                id={field.name}
                type="number"
                step={1}
                onChange={(e) => field.onChange(Number(e.target.value))}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          name="minStockLevel"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>Min Stock Level</FieldLabel>
              <Input
                {...field}
                id={field.name}
                type="number"
                step={1}
                min={0}
                onChange={(e) => field.onChange(Number(e.target.value))}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />
      </div>

      <DialogFooter>
        <Button type="button" variant="outline" onClick={onClose}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting && <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />}
          Adjust Stock
        </Button>
      </DialogFooter>
    </form>
  )
}

// ---- Main Dialog ----

export function LensFormDialog({ mode, lens, open, onOpenChange }: LensFormDialogProps) {
  const handleClose = () => onOpenChange(false)

  const title =
    mode === "create"
      ? "Add Lens"
      : mode === "edit"
        ? "Edit Lens"
        : `Adjust Stock — ${lens?.brand ?? ""} ${lens?.name ?? ""}`

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        {mode === "stock" && lens ? (
          <StockAdjustmentForm lens={lens} onClose={handleClose} />
        ) : (
          <CatalogItemForm mode={mode === "create" ? "create" : "edit"} lens={lens} onClose={handleClose} />
        )}
      </DialogContent>
    </Dialog>
  )
}
