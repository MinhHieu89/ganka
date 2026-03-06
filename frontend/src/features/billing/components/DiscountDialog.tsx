import { useEffect, useMemo, useState } from "react"
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
  useApplyDiscount,
  useApproveDiscount,
  type InvoiceLineItemDto,
} from "@/features/billing/api/billing-api"
import { ApprovalPinDialog } from "./ApprovalPinDialog"

// -- Schema --

const discountSchema = z
  .object({
    scope: z.enum(["invoice", "lineItem"]),
    lineItemId: z.string().optional(),
    discountType: z.enum(["percentage", "fixed"]),
    value: z.coerce.number().positive("Gia tri phai lon hon 0"),
    reason: z.string().min(1, "Ly do la bat buoc"),
  })
  .superRefine((data, ctx) => {
    if (data.scope === "lineItem" && !data.lineItemId) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Vui long chon dich vu",
        path: ["lineItemId"],
      })
    }
    if (data.discountType === "percentage" && data.value > 100) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Phan tram khong duoc vuot qua 100",
        path: ["value"],
      })
    }
  })

type DiscountFormValues = z.infer<typeof discountSchema>

// -- Props --

interface DiscountDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  invoiceId: string
  subTotal: number
  lineItems: InvoiceLineItemDto[]
  onSuccess?: () => void
}

export function DiscountDialog({
  open,
  onOpenChange,
  invoiceId,
  subTotal,
  lineItems,
  onSuccess,
}: DiscountDialogProps) {
  const [pinDialogOpen, setPinDialogOpen] = useState(false)
  const [pendingDiscountId, setPendingDiscountId] = useState<string | null>(null)
  const [pinError, setPinError] = useState<string | null>(null)

  const applyDiscount = useApplyDiscount()
  const approveDiscount = useApproveDiscount()

  const form = useForm<DiscountFormValues>({
    resolver: zodResolver(discountSchema),
    defaultValues: {
      scope: "invoice",
      lineItemId: undefined,
      discountType: "percentage",
      value: 0,
      reason: "",
    },
  })

  const watchScope = form.watch("scope")
  const watchDiscountType = form.watch("discountType")
  const watchValue = form.watch("value")
  const watchLineItemId = form.watch("lineItemId")

  // Reset form when dialog opens
  useEffect(() => {
    if (open) {
      form.reset({
        scope: "invoice",
        lineItemId: undefined,
        discountType: "percentage",
        value: 0,
        reason: "",
      })
      setPendingDiscountId(null)
      setPinError(null)
    }
  }, [open, form])

  // Calculate the base amount for discount validation and preview
  const baseAmount = useMemo(() => {
    if (watchScope === "lineItem" && watchLineItemId) {
      const item = lineItems.find((li) => li.id === watchLineItemId)
      return item?.lineTotal ?? 0
    }
    return subTotal
  }, [watchScope, watchLineItemId, lineItems, subTotal])

  // Live preview of calculated discount in VND
  const previewAmount = useMemo(() => {
    const val = Number(watchValue) || 0
    if (watchDiscountType === "percentage") {
      return Math.round((baseAmount * Math.min(val, 100)) / 100)
    }
    return Math.min(val, baseAmount)
  }, [watchDiscountType, watchValue, baseAmount])

  const handleSubmit = async (data: DiscountFormValues) => {
    // Validate fixed amount does not exceed base
    if (data.discountType === "fixed" && data.value > baseAmount) {
      form.setError("value", {
        message: `Khong duoc vuot qua ${formatVnd(baseAmount)}`,
      })
      return
    }

    try {
      const result = await applyDiscount.mutateAsync({
        invoiceId,
        invoiceLineItemId:
          data.scope === "lineItem" ? data.lineItemId ?? null : null,
        type: data.discountType === "percentage" ? 0 : 1,
        value: data.value,
        reason: data.reason,
      })
      setPendingDiscountId(result.id)
      setPinDialogOpen(true)
    } catch {
      // Error handled by mutation onError
    }
  }

  const handlePinApprove = async (pin: string) => {
    if (!pendingDiscountId) return
    setPinError(null)

    try {
      await approveDiscount.mutateAsync({
        discountId: pendingDiscountId,
        managerPin: pin,
      })
      setPinDialogOpen(false)
      toast.success("Giam gia da duoc ap dung")
      onOpenChange(false)
      onSuccess?.()
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "PIN khong hop le"
      setPinError(message)
    }
  }

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Ap dung giam gia</DialogTitle>
          </DialogHeader>

          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className="space-y-4"
          >
            {/* Discount scope */}
            <Controller
              name="scope"
              control={form.control}
              render={({ field }) => (
                <Field>
                  <FieldLabel>Pham vi</FieldLabel>
                  <RadioGroup
                    value={field.value}
                    onValueChange={field.onChange}
                    className="flex gap-4"
                  >
                    <div className="flex items-center gap-2">
                      <RadioGroupItem value="invoice" id="scope-invoice" />
                      <Label htmlFor="scope-invoice">Toan bo hoa don</Label>
                    </div>
                    <div className="flex items-center gap-2">
                      <RadioGroupItem value="lineItem" id="scope-line" />
                      <Label htmlFor="scope-line">Theo dich vu</Label>
                    </div>
                  </RadioGroup>
                </Field>
              )}
            />

            {/* Line item selector (only when per-item scope) */}
            {watchScope === "lineItem" && (
              <Controller
                name="lineItemId"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid || undefined}>
                    <FieldLabel>Dich vu</FieldLabel>
                    <Select
                      value={field.value ?? ""}
                      onValueChange={field.onChange}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Chon dich vu" />
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

            {/* Discount type toggle */}
            <Controller
              name="discountType"
              control={form.control}
              render={({ field }) => (
                <Field>
                  <FieldLabel>Loai giam gia</FieldLabel>
                  <div className="flex gap-1 rounded-md border p-1">
                    <button
                      type="button"
                      className={`flex-1 rounded px-3 py-1.5 text-sm font-medium transition-colors ${
                        field.value === "percentage"
                          ? "bg-primary text-primary-foreground"
                          : "hover:bg-muted"
                      }`}
                      onClick={() => field.onChange("percentage")}
                    >
                      %
                    </button>
                    <button
                      type="button"
                      className={`flex-1 rounded px-3 py-1.5 text-sm font-medium transition-colors ${
                        field.value === "fixed"
                          ? "bg-primary text-primary-foreground"
                          : "hover:bg-muted"
                      }`}
                      onClick={() => field.onChange("fixed")}
                    >
                      VND
                    </button>
                  </div>
                </Field>
              )}
            />

            {/* Value input */}
            <Controller
              name="value"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>Gia tri</FieldLabel>
                  <div className="relative">
                    <Input
                      {...field}
                      id={field.name}
                      type="number"
                      min={0}
                      max={
                        watchDiscountType === "percentage" ? 100 : baseAmount
                      }
                      step={watchDiscountType === "percentage" ? 1 : 1000}
                      aria-invalid={fieldState.invalid || undefined}
                      className="pr-14"
                    />
                    <span className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">
                      {watchDiscountType === "percentage" ? "%" : "VND"}
                    </span>
                  </div>
                  {fieldState.error && (
                    <FieldError>{fieldState.error.message}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Live preview */}
            {previewAmount > 0 && (
              <div className="rounded-md bg-muted/50 px-3 py-2 text-sm">
                Giam:{" "}
                <span className="font-semibold text-destructive">
                  -{formatVnd(previewAmount)}
                </span>
                {watchDiscountType === "percentage" && (
                  <span className="text-muted-foreground">
                    {" "}
                    ({watchValue}% cua {formatVnd(baseAmount)})
                  </span>
                )}
              </div>
            )}

            {/* Reason */}
            <Controller
              name="reason"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>Ly do</FieldLabel>
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
                Huy
              </Button>
              <Button type="submit" disabled={applyDiscount.isPending}>
                {applyDiscount.isPending && (
                  <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                Ap dung giam gia
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
        title="Xac nhan giam gia"
        description="Nhap PIN quan ly de phe duyet giam gia"
        isPending={approveDiscount.isPending}
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
