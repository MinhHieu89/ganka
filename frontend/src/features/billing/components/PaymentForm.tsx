import { useState } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
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
import { Textarea } from "@/shared/components/Textarea"
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

// -- Zod schema for payment form --

const paymentFormSchema = z
  .object({
    method: z.number({ required_error: "Payment method is required" }).min(0),
    amount: z.coerce
      .number({ required_error: "Amount is required" })
      .positive("Amount must be greater than 0"),
    referenceNumber: z.string().optional().nullable(),
    cardLast4: z.string().optional().nullable(),
    notes: z.string().max(500).optional().nullable(),
    treatmentPackageId: z.string().optional().nullable(),
    isSplitPayment: z.boolean().default(false),
    splitSequence: z.coerce.number().optional().nullable(),
  })
  .superRefine((data, ctx) => {
    // Reference number required for bank transfer and QR methods
    if (
      methodRequiresReference(data.method) &&
      (!data.referenceNumber || data.referenceNumber.trim() === "")
    ) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Reference number is required for this payment method",
        path: ["referenceNumber"],
      })
    }
    // Card last 4 required for card methods
    if (methodIsCard(data.method)) {
      if (!data.cardLast4 || data.cardLast4.trim() === "") {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: "Card last 4 digits are required",
          path: ["cardLast4"],
        })
      } else if (!/^\d{4}$/.test(data.cardLast4.trim())) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: "Must be exactly 4 digits",
          path: ["cardLast4"],
        })
      }
    }
    // Split payment requires split sequence
    if (data.isSplitPayment && (data.splitSequence == null || data.splitSequence < 1)) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Split sequence is required for split payments",
        path: ["splitSequence"],
      })
    }
  })

type PaymentFormValues = z.infer<typeof paymentFormSchema>

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
    resolver: zodResolver(paymentFormSchema),
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

  const selectedMethod = watch("method")
  const enteredAmount = watch("amount")
  const isSplitPayment = watch("isSplitPayment")

  const remainingAfterPayment =
    typeof enteredAmount === "number" ? balanceDue - enteredAmount : balanceDue

  const onSubmit = async (values: PaymentFormValues) => {
    if (values.amount > balanceDue) {
      toast.error("Amount cannot exceed balance due")
      return
    }

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
      toast.success("Thanh toan thanh cong")
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
          <DialogTitle>Thu tien</DialogTitle>
          <DialogDescription>
            So tien can thanh toan: {formatVND(balanceDue)}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          {/* Payment Method Selector */}
          <Field>
            <FieldLabel>Phuong thuc thanh toan *</FieldLabel>
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
            <FieldLabel>So tien (VND) *</FieldLabel>
            <Controller
              name="amount"
              control={control}
              render={({ field }) => (
                <Input
                  type="number"
                  min={0}
                  max={balanceDue}
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
                Con lai sau thanh toan:{" "}
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
              <FieldLabel>Ma giao dich *</FieldLabel>
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
                <FieldLabel>4 so cuoi the *</FieldLabel>
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
                <FieldLabel>Loai the</FieldLabel>
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
            <FieldLabel>Ghi chu</FieldLabel>
            <Controller
              name="notes"
              control={control}
              render={({ field }) => (
                <Textarea
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
              Thanh toan goi dieu tri
            </Label>
          </div>

          {showTreatmentFields && (
            <div className="flex flex-col gap-3 rounded-md border p-3">
              <Field>
                <FieldLabel>Ma goi dieu tri</FieldLabel>
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
                  Chia thanh toan (50/50)
                </Label>
              </div>

              {isSplitPayment && (
                <Field>
                  <FieldLabel>Lan thanh toan *</FieldLabel>
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
                          Lan 1
                        </Button>
                        <Button
                          type="button"
                          variant={field.value === 2 ? "default" : "outline"}
                          size="sm"
                          onClick={() => field.onChange(2)}
                          disabled={isSubmitting}
                        >
                          Lan 2
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
              Huy
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              Xac nhan thanh toan
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
