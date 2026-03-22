import { useCallback, useState, useEffect } from "react"
import { useForm, useFieldArray, Controller, useWatch } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import {
  IconPlus,
  IconTrash,
  IconLoader2,
  IconUser,
  IconUserOff,
  IconAlertTriangle,
} from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { Separator } from "@/shared/components/Separator"
import { cn } from "@/shared/lib/utils"
import {
  useCreateOtcSale,
  useDrugAvailableStock,
} from "@/features/pharmacy/api/pharmacy-queries"
import type { DrugCatalogItemDto } from "@/features/pharmacy/api/pharmacy-api"
import { DrugCombobox } from "@/features/pharmacy/components/DrugCombobox"

// ---- Schema ----

const lineSchema = z.object({
  drugCatalogItemId: z.string().min(1, "required"),
  drugName: z.string().min(1),
  quantity: z.coerce.number().int().min(1, "must be >= 1"),
  unitPrice: z.coerce.number().min(0, "must be >= 0"),
})

const otcSaleSchema = z.object({
  isAnonymous: z.boolean(),
  customerName: z.string().max(200).optional().or(z.literal("")),
  notes: z.string().max(500).optional().or(z.literal("")),
  lines: z.array(lineSchema).min(1, "At least one item required"),
})

type OtcSaleValues = z.infer<typeof otcSaleSchema>

// ---- Stock warning component ----

interface StockWarningProps {
  drugCatalogItemId: string | undefined
  quantity: number
}

function StockWarning({ drugCatalogItemId, quantity }: StockWarningProps) {
  const { t } = useTranslation("pharmacy")
  const { data } = useDrugAvailableStock(drugCatalogItemId)

  if (!drugCatalogItemId || data === undefined) return null

  const availableStock = data.availableStock

  if (quantity <= availableStock) return null

  const isOutOfStock = availableStock === 0

  return (
    <p
      className={cn(
        "text-xs flex items-center gap-1 mt-0.5",
        isOutOfStock ? "text-red-600" : "text-yellow-600",
      )}
    >
      <IconAlertTriangle className="h-3 w-3 shrink-0" />
      {isOutOfStock
        ? t("otcSale.outOfStock")
        : t("otcSale.onlyInStock", { count: availableStock })}
    </p>
  )
}

// ---- Line item row with stock check ----

interface LineItemRowProps {
  index: number
  control: ReturnType<typeof useForm<OtcSaleValues>>["control"]
  onRemove: () => void
  onDrugSelect: (drug: DrugCatalogItemDto, index: number) => void
  canRemove: boolean
  getError: (err: { message?: string } | undefined) => string | undefined
}

function LineItemRow({
  index,
  control,
  onRemove,
  onDrugSelect,
  canRemove,
  getError,
}: LineItemRowProps) {
  const { t: tCommon } = useTranslation("common")

  // Watch this line's drug and quantity for stock check
  const drugCatalogItemId = useWatch({ control, name: `lines.${index}.drugCatalogItemId` })
  const quantity = useWatch({ control, name: `lines.${index}.quantity` })

  return (
    <div>
      <div className="grid grid-cols-[1fr_80px_120px_40px] gap-2 items-start">
        {/* Drug selector */}
        <Controller
          name={`lines.${index}.drugCatalogItemId`}
          control={control}
          render={({ field: f, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <DrugCombobox
                value={f.value}
                onSelect={(drug) => onDrugSelect(drug, index)}
                renderExtra={(drug) =>
                  drug.sellingPrice != null && drug.sellingPrice > 0 ? (
                    <span className="ml-2">
                      {drug.sellingPrice.toLocaleString("vi-VN")} /{drug.unit}
                    </span>
                  ) : null
                }
              />
              {fieldState.error && (
                <FieldError className="text-xs">{getError(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />

        {/* Quantity */}
        <Controller
          name={`lines.${index}.quantity`}
          control={control}
          render={({ field: f, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <Input
                {...f}
                type="number"
                min={1}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError className="text-xs">{getError(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />

        {/* Unit price */}
        <Controller
          name={`lines.${index}.unitPrice`}
          control={control}
          render={({ field: f, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <Input
                {...f}
                type="number"
                min={0}
                step={100}
                aria-invalid={fieldState.invalid || undefined}
              />
              {fieldState.error && (
                <FieldError className="text-xs">{getError(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />

        {/* Remove */}
        <div className="flex items-center justify-center">
          <Button
            type="button"
            variant="ghost"
            size="sm"
            className="mt-0.5 h-8 w-8 p-0 text-destructive hover:text-destructive"
            onClick={onRemove}
            disabled={!canRemove}
            title={tCommon("buttons.remove")}
          >
            <IconTrash className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Inline stock warning */}
      <StockWarning
        drugCatalogItemId={drugCatalogItemId || undefined}
        quantity={Number(quantity) || 0}
      />
    </div>
  )
}

// ---- Total display ----

function TotalDisplay({ control }: { control: ReturnType<typeof useForm<OtcSaleValues>>["control"] }) {
  const { t } = useTranslation("pharmacy")
  const lines = useWatch({ control, name: "lines" })

  const total = lines.reduce((sum, line) => {
    const qty = Number(line.quantity) || 0
    const price = Number(line.unitPrice) || 0
    return sum + qty * price
  }, 0)

  return (
    <div className="flex items-center justify-between rounded-lg border bg-muted/30 px-4 py-3">
      <span className="text-sm font-medium text-muted-foreground">{t("otcSale.total")}</span>
      <span className="text-lg font-bold">{total.toLocaleString("vi-VN")} ₫</span>
    </div>
  )
}

// ---- Stock checker per line (for submit button) ----

interface StockCheckerProps {
  drugCatalogItemId: string | undefined
  quantity: number
  onStockStatus: (exceeded: boolean) => void
}

function StockChecker({ drugCatalogItemId, quantity, onStockStatus }: StockCheckerProps) {
  const { data } = useDrugAvailableStock(drugCatalogItemId)

  useEffect(() => {
    if (!drugCatalogItemId) {
      onStockStatus(false)
      return
    }
    if (data === undefined) {
      onStockStatus(false)
      return
    }
    onStockStatus(quantity > data.availableStock)
  }, [drugCatalogItemId, quantity, data, onStockStatus])

  return null
}

// ---- Main form ----

interface OtcSaleFormProps {
  onSuccess?: () => void
}

export function OtcSaleForm({ onSuccess }: OtcSaleFormProps) {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")
  const createOtcSale = useCreateOtcSale()

  const form = useForm<OtcSaleValues>({
    resolver: zodResolver(otcSaleSchema),
    defaultValues: {
      isAnonymous: true,
      customerName: "",
      notes: "",
      lines: [
        {
          drugCatalogItemId: "",
          drugName: "",
          quantity: 1,
          unitPrice: 0,
        },
      ],
    },
  })

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "lines",
  })

  const isAnonymous = useWatch({ control: form.control, name: "isAnonymous" })
  const watchedLines = useWatch({ control: form.control, name: "lines" })

  // Track stock exceeded status per line
  const [stockExceeded, setStockExceeded] = useState<Record<number, boolean>>({})

  const handleStockStatus = useCallback((index: number, exceeded: boolean) => {
    setStockExceeded((prev) => {
      if (prev[index] === exceeded) return prev
      return { ...prev, [index]: exceeded }
    })
  }, [])

  const anyStockExceeded = Object.values(stockExceeded).some(Boolean)

  // Reset customer name when toggling to anonymous
  useEffect(() => {
    if (isAnonymous) {
      form.setValue("customerName", "")
    }
  }, [isAnonymous, form])

  // Clean up stock exceeded entries when lines are removed
  useEffect(() => {
    setStockExceeded((prev) => {
      const cleaned: Record<number, boolean> = {}
      for (let i = 0; i < fields.length; i++) {
        if (i in prev) cleaned[i] = prev[i]
      }
      return cleaned
    })
  }, [fields.length])

  const addLine = useCallback(() => {
    append({
      drugCatalogItemId: "",
      drugName: "",
      quantity: 1,
      unitPrice: 0,
    })
  }, [append])

  const handleDrugSelect = useCallback(
    (drug: DrugCatalogItemDto, index: number) => {
      form.setValue(`lines.${index}.drugCatalogItemId`, drug.id, { shouldValidate: true })
      form.setValue(`lines.${index}.drugName`, drug.nameVi || drug.name)
      form.setValue(`lines.${index}.unitPrice`, drug.sellingPrice ?? 0)
    },
    [form],
  )

  const handleSubmit = async (data: OtcSaleValues) => {
    try {
      await createOtcSale.mutateAsync({
        patientId: null,
        customerName: data.isAnonymous ? null : (data.customerName?.trim() || null),
        notes: data.notes?.trim() || null,
        lines: data.lines.map((line) => ({
          drugCatalogItemId: line.drugCatalogItemId,
          drugName: line.drugName,
          quantity: line.quantity,
          unitPrice: line.unitPrice,
        })),
      })
      toast.success(t("otcSale.created"))
      form.reset()
      setStockExceeded({})
      onSuccess?.()
    } catch {
      // onError in mutation handles toast
    }
  }

  const getErrorMessage = (
    error: { message?: string } | undefined,
  ): string | undefined => {
    if (!error?.message) return undefined
    if (error.message === "required") return tCommon("validation.required")
    return error.message
  }

  return (
    <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-5">

      {/* Hidden stock checkers for submit button disabling */}
      {watchedLines.map((line, index) => (
        <StockChecker
          key={`stock-${index}-${line.drugCatalogItemId}`}
          drugCatalogItemId={line.drugCatalogItemId || undefined}
          quantity={Number(line.quantity) || 0}
          onStockStatus={(exceeded) => handleStockStatus(index, exceeded)}
        />
      ))}

      {/* Customer section */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold">{t("otcSale.customerSection")}</h3>
          <div className="flex gap-2">
            <Button
              type="button"
              variant={isAnonymous ? "default" : "outline"}
              size="sm"
              onClick={() => form.setValue("isAnonymous", true)}
            >
              <IconUserOff className="h-4 w-4 mr-1" />
              {t("otcSale.anonymous")}
            </Button>
            <Button
              type="button"
              variant={!isAnonymous ? "default" : "outline"}
              size="sm"
              onClick={() => form.setValue("isAnonymous", false)}
            >
              <IconUser className="h-4 w-4 mr-1" />
              {t("otcSale.namedCustomer")}
            </Button>
          </div>
        </div>

        {!isAnonymous && (
          <Controller
            name="customerName"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>{t("otcSale.customerName")}</FieldLabel>
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
        )}
      </div>

      <Separator />

      {/* Line items */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold">{t("otcSale.items")}</h3>
          <Button type="button" variant="outline" size="sm" onClick={addLine}>
            <IconPlus className="h-4 w-4 mr-1" />
            {t("otcSale.addItem")}
          </Button>
        </div>

        {/* Column headers */}
        <div className="grid grid-cols-[1fr_80px_120px_40px] gap-2 text-xs font-medium text-muted-foreground px-1">
          <span>{t("otcSale.drug")}</span>
          <span>{t("otcSale.quantity")}</span>
          <span>{t("otcSale.unitPrice")}</span>
          <span />
        </div>

        {fields.map((field, index) => (
          <LineItemRow
            key={field.id}
            index={index}
            control={form.control}
            onRemove={() => remove(index)}
            onDrugSelect={handleDrugSelect}
            canRemove={fields.length > 1}
            getError={getErrorMessage}
          />
        ))}

        {form.formState.errors.lines?.root && (
          <p className="text-xs text-destructive">
            {form.formState.errors.lines.root.message}
          </p>
        )}
      </div>

      {/* Total */}
      <TotalDisplay control={form.control} />

      {/* Notes */}
      <Controller
        name="notes"
        control={form.control}
        render={({ field, fieldState }) => (
          <Field data-invalid={fieldState.invalid || undefined}>
            <FieldLabel htmlFor={field.name}>{t("otcSale.notes")}</FieldLabel>
            <AutoResizeTextarea
              {...field}
              id={field.name}
              rows={2}
              aria-invalid={fieldState.invalid || undefined}
            />
            {fieldState.error && (
              <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
            )}
          </Field>
        )}
      />

      {/* Submit */}
      <div className="flex justify-end">
        <Button
          type="submit"
          disabled={createOtcSale.isPending || anyStockExceeded}
          className="min-w-36"
        >
          {createOtcSale.isPending && (
            <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
          )}
          {createOtcSale.isPending ? t("otcSale.submitting") : t("otcSale.submit")}
        </Button>
      </div>
    </form>
  )
}
