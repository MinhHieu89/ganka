import { useCallback, useState, useEffect } from "react"
import { useForm, useFieldArray, Controller, useWatch } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import {
  IconPlus,
  IconTrash,
  IconSearch,
  IconLoader2,
  IconUser,
  IconUserOff,
} from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Textarea } from "@/shared/components/Textarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
} from "@/shared/components/Command"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/shared/components/Popover"
import { Separator } from "@/shared/components/Separator"
import { cn } from "@/shared/lib/utils"
import {
  useDrugCatalogList,
  useCreateOtcSale,
} from "@/features/pharmacy/api/pharmacy-queries"
import type { DrugCatalogItemDto } from "@/features/pharmacy/api/pharmacy-api"

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

// ---- Drug search combobox ----

interface DrugComboboxProps {
  value: string
  onSelect: (drug: DrugCatalogItemDto) => void
  disabled?: boolean
}

function DrugCombobox({ value, onSelect, disabled }: DrugComboboxProps) {
  const { t } = useTranslation("pharmacy")
  const [open, setOpen] = useState(false)
  const [search, setSearch] = useState("")
  const { data: drugs } = useDrugCatalogList()

  const selectedDrug = drugs?.find((d) => d.id === value)

  const filtered = drugs?.filter((d) => {
    if (!search) return true
    const s = search.toLowerCase()
    return (
      d.nameVi.toLowerCase().includes(s) ||
      d.name.toLowerCase().includes(s) ||
      d.genericName.toLowerCase().includes(s)
    )
  })

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <div>
          <Button
            type="button"
            variant="outline"
            role="combobox"
            aria-expanded={open}
            disabled={disabled}
            className={cn(
              "w-full justify-start text-left font-normal",
              !selectedDrug && "text-muted-foreground",
            )}
          >
            <IconSearch className="h-4 w-4 mr-2 shrink-0" />
            {selectedDrug ? (
              <span className="truncate">{selectedDrug.nameVi || selectedDrug.name}</span>
            ) : (
              t("otcSale.selectDrug")
            )}
          </Button>
        </div>
      </PopoverTrigger>
      <PopoverContent className="w-80 p-0" align="start">
        <Command shouldFilter={false}>
          <CommandInput
            placeholder={t("catalog.search")}
            value={search}
            onValueChange={setSearch}
          />
          <CommandEmpty>{t("catalog.empty")}</CommandEmpty>
          <CommandGroup className="max-h-52 overflow-y-auto">
            {(filtered ?? []).map((drug) => (
              <CommandItem
                key={drug.id}
                value={drug.id}
                onSelect={() => {
                  onSelect(drug)
                  setOpen(false)
                  setSearch("")
                }}
              >
                <div>
                  <div className="text-sm font-medium">{drug.nameVi || drug.name}</div>
                  <div className="text-xs text-muted-foreground">
                    {drug.genericName}
                    {drug.sellingPrice != null && drug.sellingPrice > 0 && (
                      <span className="ml-2">{drug.sellingPrice.toLocaleString("vi-VN")} ₫/{drug.unit}</span>
                    )}
                  </div>
                </div>
              </CommandItem>
            ))}
          </CommandGroup>
        </Command>
      </PopoverContent>
    </Popover>
  )
}

// ---- Line item row ----

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
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")

  return (
    <div className="grid grid-cols-[1fr_90px_130px_40px] gap-2 items-start">
      {/* Drug selector */}
      <Controller
        name={`lines.${index}.drugCatalogItemId`}
        control={control}
        render={({ field: f, fieldState }) => (
          <Field data-invalid={fieldState.invalid || undefined}>
            <DrugCombobox
              value={f.value}
              onSelect={(drug) => onDrugSelect(drug, index)}
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
      <Button
        type="button"
        variant="ghost"
        size="sm"
        className="mt-0.5 h-9 w-9 p-0 text-destructive hover:text-destructive"
        onClick={onRemove}
        disabled={!canRemove}
        title={tCommon("buttons.remove")}
      >
        <IconTrash className="h-4 w-4" />
      </Button>
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

  // Reset customer name when toggling to anonymous
  useEffect(() => {
    if (isAnonymous) {
      form.setValue("customerName", "")
    }
  }, [isAnonymous, form])

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
        lines: data.lines.map((line) => ({
          drugCatalogItemId: line.drugCatalogItemId,
          drugName: line.drugName,
          quantity: line.quantity,
          unitPrice: line.unitPrice,
        })),
      })
      toast.success(t("otcSale.created"))
      form.reset()
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
        <div className="grid grid-cols-[1fr_90px_130px_40px] gap-2 text-xs font-medium text-muted-foreground px-1">
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
            <Textarea
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
        <Button type="submit" disabled={createOtcSale.isPending} className="min-w-36">
          {createOtcSale.isPending && (
            <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
          )}
          {createOtcSale.isPending ? t("otcSale.submitting") : t("otcSale.submit")}
        </Button>
      </div>
    </form>
  )
}
