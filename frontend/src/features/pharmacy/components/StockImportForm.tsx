import { useCallback, useState } from "react"
import { useForm, useFieldArray, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { toast } from "sonner"
import {
  IconPlus,
  IconTrash,
  IconLoader2,
  IconSearch,
} from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { DatePicker } from "@/shared/components/DatePicker"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
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
import { cn } from "@/shared/lib/utils"
import {
  useSuppliers,
  useDrugCatalogList,
  useCreateStockImport,
} from "@/features/pharmacy/api/pharmacy-queries"

// ---- Schema ----

const lineSchema = z.object({
  drugCatalogItemId: z.string().min(1, "required"),
  drugName: z.string().min(1),
  batchNumber: z.string().min(1, "required").max(100),
  expiryDate: z.date({ required_error: "required" }),
  quantity: z.coerce.number().int().min(1, "must be >= 1"),
  purchasePrice: z.coerce.number().min(0, "must be >= 0"),
})

const stockImportSchema = z.object({
  supplierId: z.string().min(1, "required"),
  invoiceNumber: z.string().max(100).optional().or(z.literal("")),
  importDate: z.date({ required_error: "required" }),
  notes: z.string().max(500).optional().or(z.literal("")),
  lines: z.array(lineSchema).min(1, "At least one line item required"),
})

type StockImportValues = z.infer<typeof stockImportSchema>

// ---- Drug search combobox ----

interface DrugComboboxProps {
  value: string
  onSelect: (id: string, name: string) => void
  disabled?: boolean
}

function DrugCombobox({ value, onSelect, disabled }: DrugComboboxProps) {
  const { t } = useTranslation("pharmacy")
  const [open, setOpen] = useState(false)
  const { data: drugs } = useDrugCatalogList()

  const selectedDrug = drugs?.find((d) => d.id === value)

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
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
            t("stockImport.selectDrug")
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-72 p-0" align="start">
        <Command>
          <CommandInput placeholder={t("catalog.search")} />
          <CommandEmpty>{t("catalog.empty")}</CommandEmpty>
          <CommandGroup className="max-h-48 overflow-y-auto">
            {(drugs ?? []).map((drug) => (
              <CommandItem
                key={drug.id}
                value={drug.nameVi || drug.name}
                keywords={[drug.name, drug.genericName ?? ""].filter(Boolean)}
                onSelect={() => {
                  onSelect(drug.id, drug.nameVi || drug.name)
                  setOpen(false)
                }}
              >
                <div>
                  <div className="text-sm font-medium">{drug.nameVi || drug.name}</div>
                  <div className="text-xs text-muted-foreground">{drug.genericName}</div>
                </div>
              </CommandItem>
            ))}
          </CommandGroup>
        </Command>
      </PopoverContent>
    </Popover>
  )
}

// ---- Main form ----

interface StockImportFormProps {
  onSuccess?: () => void
}

export function StockImportForm({ onSuccess }: StockImportFormProps) {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")
  const { data: suppliers } = useSuppliers()
  const createImport = useCreateStockImport()

  const form = useForm<StockImportValues>({
    resolver: zodResolver(stockImportSchema),
    defaultValues: {
      supplierId: "",
      invoiceNumber: "",
      importDate: new Date(),
      notes: "",
      lines: [
        {
          drugCatalogItemId: "",
          drugName: "",
          batchNumber: "",
          expiryDate: undefined,
          quantity: 1,
          purchasePrice: 0,
        },
      ],
    },
  })

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "lines",
  })

  const addLine = useCallback(() => {
    append({
      drugCatalogItemId: "",
      drugName: "",
      batchNumber: "",
      expiryDate: undefined as unknown as Date,
      quantity: 1,
      purchasePrice: 0,
    })
  }, [append])

  const handleSubmit = async (data: StockImportValues) => {
    try {
      await createImport.mutateAsync({
        supplierId: data.supplierId,
        invoiceNumber: data.invoiceNumber || null,
        notes: data.notes || null,
        lines: data.lines.map((line) => ({
          drugCatalogItemId: line.drugCatalogItemId,
          drugName: line.drugName,
          batchNumber: line.batchNumber,
          expiryDate: format(line.expiryDate, "yyyy-MM-dd"),
          quantity: line.quantity,
          purchasePrice: line.purchasePrice,
        })),
      })
      toast.success(t("stockImport.created"))
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
    <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-6">
      {/* Header fields */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        {/* Supplier */}
        <Controller
          name="supplierId"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel>{t("stockImport.supplier")}</FieldLabel>
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger>
                  <SelectValue placeholder={t("stockImport.selectSupplier")} />
                </SelectTrigger>
                <SelectContent>
                  {(suppliers ?? [])
                    .filter((s) => s.isActive)
                    .map((supplier) => (
                      <SelectItem key={supplier.id} value={supplier.id}>
                        {supplier.name}
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

        {/* Invoice number */}
        <Controller
          name="invoiceNumber"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>{t("stockImport.invoiceNumber")}</FieldLabel>
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

        {/* Import date */}
        <Controller
          name="importDate"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel>{t("stockImport.importDate")}</FieldLabel>
              <DatePicker
                value={field.value}
                onChange={field.onChange}
                toDate={new Date()}
              />
              {fieldState.error && (
                <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
              )}
            </Field>
          )}
        />

        {/* Notes */}
        <Controller
          name="notes"
          control={form.control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <FieldLabel htmlFor={field.name}>{t("stockImport.notes")}</FieldLabel>
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
      </div>

      {/* Line items */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold">{t("stockImport.lineItems")}</h3>
          <Button type="button" variant="outline" size="sm" onClick={addLine}>
            <IconPlus className="h-4 w-4 mr-1" />
            {t("stockImport.addLine")}
          </Button>
        </div>

        {/* Column headers */}
        <div className="grid grid-cols-[1fr_120px_140px_80px_120px_40px] gap-2 text-xs font-medium text-muted-foreground px-1">
          <span>{t("stockImport.drug")}</span>
          <span>{t("stockImport.batchNumber")}</span>
          <span>{t("stockImport.expiryDate")}</span>
          <span>{t("stockImport.quantity")}</span>
          <span>{t("stockImport.purchasePrice")}</span>
          <span />
        </div>

        {fields.map((field, index) => (
          <div
            key={field.id}
            className="grid grid-cols-[1fr_120px_140px_80px_120px_40px] gap-2 items-start"
          >
            {/* Drug selector */}
            <Controller
              name={`lines.${index}.drugCatalogItemId`}
              control={form.control}
              render={({ field: f, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <DrugCombobox
                    value={f.value}
                    onSelect={(id, name) => {
                      form.setValue(`lines.${index}.drugCatalogItemId`, id, { shouldValidate: true, shouldDirty: true })
                      form.setValue(`lines.${index}.drugName`, name)
                    }}
                  />
                  {fieldState.error && (
                    <FieldError className="text-xs">{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Batch number */}
            <Controller
              name={`lines.${index}.batchNumber`}
              control={form.control}
              render={({ field: f, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <Input
                    {...f}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError className="text-xs">{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Expiry date */}
            <Controller
              name={`lines.${index}.expiryDate`}
              control={form.control}
              render={({ field: f, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <DatePicker
                    value={f.value}
                    onChange={f.onChange}
                    fromDate={new Date()}
                    toDate={new Date(new Date().setFullYear(new Date().getFullYear() + 10))}
                    className="w-full"
                  />
                  {fieldState.error && (
                    <FieldError className="text-xs">{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Quantity */}
            <Controller
              name={`lines.${index}.quantity`}
              control={form.control}
              render={({ field: f, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <Input
                    {...f}
                    type="number"
                    min={1}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError className="text-xs">{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Purchase price */}
            <Controller
              name={`lines.${index}.purchasePrice`}
              control={form.control}
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
                    <FieldError className="text-xs">{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Remove button */}
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="mt-0.5 h-9 w-9 p-0 text-destructive hover:text-destructive"
              onClick={() => remove(index)}
              disabled={fields.length === 1}
            >
              <IconTrash className="h-4 w-4" />
            </Button>
          </div>
        ))}

        {form.formState.errors.lines?.root && (
          <p className="text-xs text-destructive">
            {form.formState.errors.lines.root.message}
          </p>
        )}
      </div>

      {/* Submit */}
      <div className="flex justify-end">
        <Button type="submit" disabled={createImport.isPending} className="min-w-32">
          {createImport.isPending && (
            <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
          )}
          {createImport.isPending ? t("stockImport.submitting") : t("stockImport.submit")}
        </Button>
      </div>
    </form>
  )
}
