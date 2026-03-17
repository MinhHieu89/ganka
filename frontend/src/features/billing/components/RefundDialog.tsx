import { useEffect, useMemo, useState } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
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
import { Label } from "@/shared/components/Label"
import { RadioGroup, RadioGroupItem } from "@/shared/components/RadioGroup"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  useRequestRefund,
  useApproveRefund,
  useProcessRefund,
  type InvoiceLineItemDto,
} from "@/features/billing/api/billing-api"
import { ApprovalPinDialog } from "./ApprovalPinDialog"

// -- Schema factory --

function createRefundSchema(t: (key: string) => string) {
  return z
    .object({
      scope: z.enum(["full", "partial"]),
      lineItemId: z.string().optional(),
      amount: z.coerce.number().positive(t("validation.mustBePositive")),
      reason: z.string().min(1, t("validation.reasonRequired")),
    })
    .superRefine((data, ctx) => {
      if (data.scope === "partial" && !data.lineItemId) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: t("validation.selectService"),
          path: ["lineItemId"],
        })
      }
    })
}

type RefundFormValues = z.infer<ReturnType<typeof createRefundSchema>>

// -- Props --

interface RefundDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  invoiceId: string
  totalAmount: number
  lineItems: InvoiceLineItemDto[]
  onSuccess?: () => void
}

export function RefundDialog({
  open,
  onOpenChange,
  invoiceId,
  totalAmount,
  lineItems,
  onSuccess,
}: RefundDialogProps) {
  const { t } = useTranslation("billing")
  const { t: tCommon } = useTranslation("common")
  const [pinDialogOpen, setPinDialogOpen] = useState(false)
  const [pendingRefundId, setPendingRefundId] = useState<string | null>(null)
  const [pinError, setPinError] = useState<string | null>(null)

  const requestRefundMutation = useRequestRefund()
  const approveRefundMutation = useApproveRefund()
  const processRefundMutation = useProcessRefund()

  const form = useForm<RefundFormValues>({
    resolver: zodResolver(createRefundSchema(t)),
    defaultValues: {
      scope: "full",
      lineItemId: undefined,
      amount: totalAmount,
      reason: "",
    },
  })

  const watchScope = form.watch("scope")
  const watchLineItemId = form.watch("lineItemId")

  // Reset form when dialog opens
  useEffect(() => {
    if (open) {
      form.reset({
        scope: "full",
        lineItemId: undefined,
        amount: totalAmount,
        reason: "",
      })
      setPendingRefundId(null)
      setPinError(null)
    }
  }, [open, form, totalAmount])

  // Update amount when scope or line item changes
  const maxAmount = useMemo(() => {
    if (watchScope === "partial" && watchLineItemId) {
      const item = lineItems.find((li) => li.id === watchLineItemId)
      return item?.lineTotal ?? totalAmount
    }
    return totalAmount
  }, [watchScope, watchLineItemId, lineItems, totalAmount])

  // When scope or line item changes, reset amount to max
  useEffect(() => {
    form.setValue("amount", maxAmount)
  }, [maxAmount, form])

  const handleSubmit = async (data: RefundFormValues) => {
    if (data.amount > maxAmount) {
      form.setError("amount", {
        message: t("exceedsMaxAmount", { max: formatVnd(maxAmount) }),
      })
      return
    }

    try {
      const result = await requestRefundMutation.mutateAsync({
        invoiceId,
        invoiceLineItemId:
          data.scope === "partial" ? data.lineItemId ?? null : null,
        amount: data.amount,
        reason: data.reason,
      })
      setPendingRefundId(result.id)
      setPinDialogOpen(true)
    } catch {
      // Error handled by mutation onError
    }
  }

  const handlePinApprove = async (pin: string) => {
    if (!pendingRefundId) return
    setPinError(null)

    try {
      // Step 1: Approve the refund with manager PIN
      await approveRefundMutation.mutateAsync({
        refundId: pendingRefundId,
        managerPin: pin,
      })

      // Step 2: Process the refund automatically after approval
      await processRefundMutation.mutateAsync({
        refundId: pendingRefundId,
      })

      setPinDialogOpen(false)
      toast.success(t("refundProcessed"))
      onOpenChange(false)
      onSuccess?.()
    } catch (err) {
      const message =
        err instanceof Error ? err.message : t("invalidPin")
      setPinError(message)
    }
  }

  const isPending =
    requestRefundMutation.isPending ||
    approveRefundMutation.isPending ||
    processRefundMutation.isPending

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t("requestRefund")}</DialogTitle>
          </DialogHeader>

          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className="space-y-4"
          >
            {/* Refund scope */}
            <Controller
              name="scope"
              control={form.control}
              render={({ field }) => (
                <Field>
                  <FieldLabel>{t("scope")}</FieldLabel>
                  <RadioGroup
                    value={field.value}
                    onValueChange={field.onChange}
                    className="flex gap-4"
                  >
                    <div className="flex items-center gap-2">
                      <RadioGroupItem value="full" id="refund-full" />
                      <Label htmlFor="refund-full">{t("fullRefund")}</Label>
                    </div>
                    <div className="flex items-center gap-2">
                      <RadioGroupItem value="partial" id="refund-partial" />
                      <Label htmlFor="refund-partial">{t("partialRefund")}</Label>
                    </div>
                  </RadioGroup>
                </Field>
              )}
            />

            {/* Line item selector (partial mode) */}
            {watchScope === "partial" && (
              <Controller
                name="lineItemId"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid || undefined}>
                    <FieldLabel>{t("service")}</FieldLabel>
                    <Select
                      value={field.value ?? ""}
                      onValueChange={field.onChange}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder={t("selectService")} />
                      </SelectTrigger>
                      <SelectContent>
                        {lineItems.map((item) => (
                          <SelectItem key={item.id} value={item.id}>
                            {item.descriptionVi || item.description} -{" "}
                            {formatVnd(item.lineTotal)}
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
            )}

            {/* Amount */}
            <Controller
              name="amount"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>{t("refundAmount")}</FieldLabel>
                  <div className="relative">
                    <Input
                      {...field}
                      id={field.name}
                      type="number"
                      min={0}
                      max={maxAmount}
                      step={1000}
                      aria-invalid={fieldState.invalid || undefined}
                      className="pr-14"
                    />
                    <span className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">
                      VND
                    </span>
                  </div>
                  {fieldState.error && (
                    <FieldError>{fieldState.error.message}</FieldError>
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
                  <FieldLabel htmlFor={field.name}>{t("refundReason")}</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error.message}</FieldError>
                  )}
                </Field>
              )}
            />

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
              >
                {tCommon("buttons.cancel")}
              </Button>
              <Button
                type="submit"
                disabled={requestRefundMutation.isPending}
                variant="destructive"
              >
                {requestRefundMutation.isPending && (
                  <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                {t("requestRefund")}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Manager PIN approval dialog */}
      <ApprovalPinDialog
        open={pinDialogOpen}
        onOpenChange={(open) => {
          setPinDialogOpen(open)
          if (!open) setPinError(null)
        }}
        onApprove={handlePinApprove}
        title={t("confirmRefund")}
        description={t("enterPinForRefund")}
        isPending={isPending}
        error={pinError}
      />
    </>
  )
}

// -- Helper --

function formatVnd(amount: number): string {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(amount)
}
