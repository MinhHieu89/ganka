import { useEffect } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconLoader2, IconArrowRight } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Textarea } from "@/shared/components/Textarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { useAdjustStock } from "@/features/pharmacy/api/pharmacy-queries"

// Stock adjustment reason enum values (must match backend AdjustmentReason enum)
export const ADJUSTMENT_REASON = {
  Correction: 0,
  WriteOff: 1,
  Damage: 2,
  Expired: 3,
  Other: 4,
} as const

export type AdjustmentReason = (typeof ADJUSTMENT_REASON)[keyof typeof ADJUSTMENT_REASON]

// ---- Props ----

export interface BatchInfo {
  drugCatalogItemId: string
  drugName: string
  currentQuantity: number
}

interface StockAdjustmentDialogProps {
  batch: BatchInfo | null
  open: boolean
  onOpenChange: (open: boolean) => void
}

// ---- Schema ----

const adjustmentSchema = z.object({
  quantityChange: z.coerce
    .number()
    .int("Must be a whole number")
    .refine((v) => v !== 0, { message: "Change cannot be zero" }),
  reason: z.coerce.number().int(),
  notes: z.string().max(500).optional().or(z.literal("")),
})

type AdjustmentValues = z.infer<typeof adjustmentSchema>

// ---- Dialog component ----

export function StockAdjustmentDialog({
  batch,
  open,
  onOpenChange,
}: StockAdjustmentDialogProps) {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")
  const adjustStock = useAdjustStock()

  const form = useForm<AdjustmentValues>({
    resolver: zodResolver(adjustmentSchema),
    defaultValues: {
      quantityChange: 0,
      reason: ADJUSTMENT_REASON.Correction,
      notes: "",
    },
  })

  // Reset form when dialog opens or batch changes
  useEffect(() => {
    if (open) {
      form.reset({
        quantityChange: 0,
        reason: ADJUSTMENT_REASON.Correction,
        notes: "",
      })
    }
  }, [open, form])

  const quantityChange = form.watch("quantityChange")
  const newQuantity = batch ? batch.currentQuantity + (Number(quantityChange) || 0) : 0
  const isInvalid = newQuantity < 0

  const handleSubmit = async (data: AdjustmentValues) => {
    if (!batch) return
    if (isInvalid) {
      form.setError("quantityChange", {
        message: t("stockAdjust.errorBelowZero"),
      })
      return
    }

    try {
      await adjustStock.mutateAsync({
        drugCatalogItemId: batch.drugCatalogItemId,
        quantity: data.quantityChange,
        reason: data.reason,
        notes: data.notes || null,
      })
      toast.success(t("stockAdjust.adjusted"))
      onOpenChange(false)
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

  if (!batch) return null

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("stockAdjust.title")}</DialogTitle>
          <p className="text-sm text-muted-foreground">{batch.drugName}</p>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Current → New preview */}
          <div className="flex items-center gap-3 rounded-lg border bg-muted/30 px-4 py-3">
            <div className="text-center">
              <p className="text-xs text-muted-foreground">{t("stockAdjust.current")}</p>
              <p className="text-xl font-bold">{batch.currentQuantity}</p>
            </div>
            <IconArrowRight className="h-5 w-5 text-muted-foreground shrink-0" />
            <div className="text-center">
              <p className="text-xs text-muted-foreground">{t("stockAdjust.new")}</p>
              <p
                className={`text-xl font-bold ${
                  isInvalid
                    ? "text-destructive"
                    : newQuantity !== batch.currentQuantity
                    ? "text-primary"
                    : ""
                }`}
              >
                {newQuantity}
              </p>
            </div>
          </div>

          {isInvalid && (
            <p className="text-sm text-destructive">{t("stockAdjust.errorBelowZero")}</p>
          )}

          {/* Quantity change */}
          <Controller
            name="quantityChange"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>{t("stockAdjust.quantityChange")}</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  type="number"
                  step={1}
                  aria-invalid={fieldState.invalid || undefined}
                />
                <p className="text-xs text-muted-foreground">
                  {t("stockAdjust.quantityChangeHint")}
                </p>
                {fieldState.error && (
                  <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Reason */}
          <Controller
            name="reason"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel>{t("stockAdjust.reason")}</FieldLabel>
                <Select
                  value={String(field.value)}
                  onValueChange={(v) => field.onChange(Number(v))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value={String(ADJUSTMENT_REASON.Correction)}>
                      {t("stockAdjust.reasonCorrection")}
                    </SelectItem>
                    <SelectItem value={String(ADJUSTMENT_REASON.WriteOff)}>
                      {t("stockAdjust.reasonWriteOff")}
                    </SelectItem>
                    <SelectItem value={String(ADJUSTMENT_REASON.Damage)}>
                      {t("stockAdjust.reasonDamage")}
                    </SelectItem>
                    <SelectItem value={String(ADJUSTMENT_REASON.Expired)}>
                      {t("stockAdjust.reasonExpired")}
                    </SelectItem>
                    <SelectItem value={String(ADJUSTMENT_REASON.Other)}>
                      {t("stockAdjust.reasonOther")}
                    </SelectItem>
                  </SelectContent>
                </Select>
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
                <FieldLabel htmlFor={field.name}>{t("stockAdjust.notes")}</FieldLabel>
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

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={adjustStock.isPending}
            >
              {tCommon("buttons.cancel")}
            </Button>
            <Button
              type="submit"
              disabled={adjustStock.isPending || isInvalid}
            >
              {adjustStock.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {t("stockAdjust.confirm")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
