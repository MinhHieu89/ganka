import { useState, useEffect } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconLoader2 } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Checkbox } from "@/shared/components/Checkbox"
import { Label } from "@/shared/components/Label"
import {
  Field,
  FieldLabel,
  FieldError,
} from "@/shared/components/Field"
import { Separator } from "@/shared/components/Separator"
import { formatVND } from "@/shared/lib/format-vnd"
import {
  useRecordPayment,
  type RecordPaymentInput,
} from "@/features/billing/api/billing-api"
import {
  PaymentMethodSelector,
  methodRequiresReference,
  methodIsCard,
  getCardType,
} from "./PaymentMethodSelector"

// -- Zod schema for payment form (uses i18n keys as defaults, overridden at render) --

function createPaymentFormSchema(t: (key: string) => string, maxAmount: number) {
  return z
    .object({
      method: z.number({ required_error: t("validation.paymentMethodRequired") }).min(0),
      amount: z.coerce
        .number({ required_error: t("validation.required") })
        .positive(t("validation.mustBePositive"))
        .max(maxAmount, t("amountExceedsBalance")),
      referenceNumber: z.string().optional().nullable(),
      cardLast4: z.string().optional().nullable(),
      notes: z.string().max(500).optional().nullable(),
      treatmentPackageId: z.string().optional().nullable(),
      isSplitPayment: z.boolean().default(false),
      splitSequence: z.coerce.number().optional().nullable(),
    })
    .superRefine((data, ctx) => {
      if (
        methodRequiresReference(data.method) &&
        (!data.referenceNumber || data.referenceNumber.trim() === "")
      ) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: t("validation.refNumberRequired"),
          path: ["referenceNumber"],
        })
      }
      if (methodIsCard(data.method)) {
        if (!data.cardLast4 || data.cardLast4.trim() === "") {
          ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: t("validation.cardLast4Required"),
            path: ["cardLast4"],
          })
        } else if (!/^\d{4}$/.test(data.cardLast4.trim())) {
          ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: t("validation.cardLast4Format"),
            path: ["cardLast4"],
          })
        }
      }
      if (data.isSplitPayment && (data.splitSequence == null || data.splitSequence < 1)) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: t("validation.splitSequenceRequired"),
          path: ["splitSequence"],
        })
      }
    })
}

type PaymentFormValues = z.infer<ReturnType<typeof createPaymentFormSchema>>

interface PaymentFormProps {
  invoiceId: string
  balanceDue: number
  onSuccess?: () => void
  open: boolean
  onOpenChange: (open: boolean) => void
  /** Optional: pre-fill treatment package context */
  treatmentPackageId?: string | null
}

export function PaymentForm({
  invoiceId,
  balanceDue,
  onSuccess,
  open,
  onOpenChange,
  treatmentPackageId,
}: PaymentFormProps) {
  const { t } = useTranslation("billing")
  const { t: tCommon } = useTranslation("common")
  const recordPayment = useRecordPayment()
  const [showTreatmentFields, setShowTreatmentFields] = useState(
    !!treatmentPackageId,
  )

  const {
    control,
    handleSubmit,
    watch,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<PaymentFormValues>({
    resolver: zodResolver(createPaymentFormSchema(t, balanceDue)),
    defaultValues: {
      method: undefined as unknown as number,
      amount: balanceDue,
      referenceNumber: null,
      cardLast4: null,
      notes: null,
      treatmentPackageId: treatmentPackageId ?? null,
      isSplitPayment: false,
      splitSequence: null,
    },
  })

  // Reset form with updated balanceDue when dialog opens
  useEffect(() => {
    if (open) {
      reset({
        method: undefined as unknown as number,
        amount: balanceDue,
        referenceNumber: null,
        cardLast4: null,
        notes: null,
        treatmentPackageId: treatmentPackageId ?? null,
        isSplitPayment: false,
        splitSequence: null,
      })
    }
  }, [open, balanceDue, reset, treatmentPackageId])

  const selectedMethod = watch("method")
  const enteredAmount = watch("amount")
  const isSplitPayment = watch("isSplitPayment")

  const remainingAfterPayment =
    typeof enteredAmount === "number" ? balanceDue - enteredAmount : balanceDue

  const onSubmit = async (values: PaymentFormValues) => {
    const input: RecordPaymentInput = {
      invoiceId,
      method: values.method,
      amount: values.amount,
      referenceNumber: methodRequiresReference(values.method)
        ? values.referenceNumber?.trim() || null
        : null,
      cardLast4: methodIsCard(values.method)
        ? values.cardLast4?.trim() || null
        : null,
      cardType: getCardType(values.method),
      notes: values.notes?.trim() || null,
      treatmentPackageId:
        showTreatmentFields && values.treatmentPackageId
          ? values.treatmentPackageId
          : null,
      isSplitPayment: showTreatmentFields ? values.isSplitPayment : false,
      splitSequence:
        showTreatmentFields && values.isSplitPayment
          ? values.splitSequence
          : null,
    }

    try {
      await recordPayment.mutateAsync(input)
      toast.success(t("paymentSuccessful"))
      reset()
      onOpenChange(false)
      onSuccess?.()
    } catch {
      // Error already handled by mutation onError toast
    }
  }

  const handleClose = () => {
    reset()
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{t("collectPayment")}</DialogTitle>
          <DialogDescription>
            {t("amountToPay")}: {formatVND(balanceDue)}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          {/* Payment Method Selector */}
          <Field>
            <FieldLabel>{t("paymentMethod")} *</FieldLabel>
            <Controller
              name="method"
              control={control}
              render={({ field }) => (
                <PaymentMethodSelector
                  value={field.value}
                  onChange={field.onChange}
                  disabled={isSubmitting}
                />
              )}
            />
            {errors.method && (
              <FieldError>{errors.method.message}</FieldError>
            )}
          </Field>

          {/* Amount */}
          <Field>
            <FieldLabel>{t("amountVnd")} *</FieldLabel>
            <Controller
              name="amount"
              control={control}
              render={({ field }) => (
                <Input
                  type="number"
                  min={0}
                  step={1000}
                  disabled={isSubmitting}
                  {...field}
                  onChange={(e) => field.onChange(Number(e.target.value))}
                />
              )}
            />
            {errors.amount && (
              <FieldError>{errors.amount.message}</FieldError>
            )}
            {typeof enteredAmount === "number" && enteredAmount > 0 && (
              <p className="text-xs text-muted-foreground">
                {t("remainingAfterPayment")}:{" "}
                <span
                  className={
                    remainingAfterPayment <= 0
                      ? "text-green-600 font-medium"
                      : "text-red-600 font-medium"
                  }
                >
                  {formatVND(Math.max(0, remainingAfterPayment))}
                </span>
              </p>
            )}
          </Field>

          {/* Reference Number -- shown for BankTransfer, QR methods */}
          {selectedMethod != null && methodRequiresReference(selectedMethod) && (
            <Field>
              <FieldLabel>{t("referenceNumber")} *</FieldLabel>
              <Controller
                name="referenceNumber"
                control={control}
                render={({ field }) => (
                  <Input
                    disabled={isSubmitting}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.value)}
                  />
                )}
              />
              {errors.referenceNumber && (
                <FieldError>{errors.referenceNumber.message}</FieldError>
              )}
            </Field>
          )}

          {/* Card Last 4 -- shown for Card methods */}
          {selectedMethod != null && methodIsCard(selectedMethod) && (
            <>
              <Field>
                <FieldLabel>{t("cardLast4Label")} *</FieldLabel>
                <Controller
                  name="cardLast4"
                  control={control}
                  render={({ field }) => (
                    <Input
                      maxLength={4}
                      disabled={isSubmitting}
                      value={field.value ?? ""}
                      onChange={(e) =>
                        field.onChange(e.target.value.replace(/\D/g, ""))
                      }
                    />
                  )}
                />
                {errors.cardLast4 && (
                  <FieldError>{errors.cardLast4.message}</FieldError>
                )}
              </Field>
              <Field>
                <FieldLabel>{t("cardType")}</FieldLabel>
                <Input
                  value={getCardType(selectedMethod) ?? ""}
                  disabled
                  readOnly
                />
              </Field>
            </>
          )}

          {/* Notes */}
          <Field>
            <FieldLabel>{t("notes")}</FieldLabel>
            <Controller
              name="notes"
              control={control}
              render={({ field }) => (
                <AutoResizeTextarea
                  rows={2}
                  disabled={isSubmitting}
                  value={field.value ?? ""}
                  onChange={(e) => field.onChange(e.target.value)}
                />
              )}
            />
            {errors.notes && (
              <FieldError>{errors.notes.message}</FieldError>
            )}
          </Field>

          <Separator />

          {/* Treatment Package Fields */}
          <div className="flex items-center gap-2">
            <Checkbox
              id="showTreatmentFields"
              checked={showTreatmentFields}
              onCheckedChange={(checked) =>
                setShowTreatmentFields(checked === true)
              }
              disabled={isSubmitting}
            />
            <Label htmlFor="showTreatmentFields" className="text-sm">
              {t("treatmentPackagePayment")}
            </Label>
          </div>

          {showTreatmentFields && (
            <div className="flex flex-col gap-3 rounded-md border p-3">
              <Field>
                <FieldLabel>{t("treatmentPackageId")}</FieldLabel>
                <Controller
                  name="treatmentPackageId"
                  control={control}
                  render={({ field }) => (
                    <Input
                      disabled={isSubmitting}
                      value={field.value ?? ""}
                      onChange={(e) => field.onChange(e.target.value)}
                    />
                  )}
                />
              </Field>

              <div className="flex items-center gap-2">
                <Controller
                  name="isSplitPayment"
                  control={control}
                  render={({ field }) => (
                    <Checkbox
                      id="isSplitPayment"
                      checked={field.value}
                      onCheckedChange={(checked) =>
                        field.onChange(checked === true)
                      }
                      disabled={isSubmitting}
                    />
                  )}
                />
                <Label htmlFor="isSplitPayment" className="text-sm">
                  {t("splitPayment")}
                </Label>
              </div>

              {isSplitPayment && (
                <Field>
                  <FieldLabel>{t("splitSequence")} *</FieldLabel>
                  <Controller
                    name="splitSequence"
                    control={control}
                    render={({ field }) => (
                      <div className="flex gap-2">
                        <Button
                          type="button"
                          variant={field.value === 1 ? "default" : "outline"}
                          size="sm"
                          onClick={() => field.onChange(1)}
                          disabled={isSubmitting}
                        >
                          {t("splitTime1")}
                        </Button>
                        <Button
                          type="button"
                          variant={field.value === 2 ? "default" : "outline"}
                          size="sm"
                          onClick={() => field.onChange(2)}
                          disabled={isSubmitting}
                        >
                          {t("splitTime2")}
                        </Button>
                      </div>
                    )}
                  />
                  {errors.splitSequence && (
                    <FieldError>{errors.splitSequence.message}</FieldError>
                  )}
                </Field>
              )}
            </div>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={handleClose}
              disabled={isSubmitting}
            >
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              {t("confirmPayment")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
