import { useEffect, useState } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import { IconLoader2, IconAlertTriangle, IconInfoCircle, IconShieldCheck } from "@tabler/icons-react"
import { format, differenceInDays } from "date-fns"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { Textarea } from "@/shared/components/Textarea"
import { Input } from "@/shared/components/Input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { RadioGroup, RadioGroupItem } from "@/shared/components/RadioGroup"
import { Label } from "@/shared/components/Label"
import { Badge } from "@/shared/components/Badge"
import { useCreateWarrantyClaim, useDeliveredOrders } from "@/features/optical/api/optical-queries"
import type { DeliveredOrderSummaryDto } from "@/features/optical/api/optical-api"

const EMPTY_ORDERS: DeliveredOrderSummaryDto[] = []

type ResolutionType = "Replace" | "Repair" | "Discount"

const RESOLUTION_TYPES: { value: ResolutionType; label: string; description: string }[] = [
  {
    value: "Replace",
    label: "Replace",
    description: "Replace the defective product with a new one",
  },
  {
    value: "Repair",
    label: "Repair",
    description: "Repair the defective product",
  },
  {
    value: "Discount",
    label: "Discount",
    description: "Provide a discount on future purchase or refund",
  },
]

const RESOLUTION_TYPE_MAP: Record<ResolutionType, number> = {
  Replace: 0,
  Repair: 1,
  Discount: 2,
}

const warrantyClaimSchema = z
  .object({
    glassesOrderId: z.string().min(1, "Please select a glasses order"),
    resolutionType: z.enum(["Replace", "Repair", "Discount"] as const),
    assessmentNotes: z
      .string()
      .min(10, "Assessment notes must be at least 10 characters")
      .max(2000, "Assessment notes too long"),
    discountAmount: z.number().positive("Discount amount must be positive").optional().nullable(),
  })
  .refine(
    (data) => {
      if (data.resolutionType === "Discount") {
        return data.discountAmount != null && data.discountAmount > 0
      }
      return true
    },
    {
      message: "Discount amount is required when resolution is Discount",
      path: ["discountAmount"],
    },
  )

type WarrantyClaimFormValues = z.infer<typeof warrantyClaimSchema>

interface WarrantyClaimFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

function WarrantyInfoPanel({
  order,
}: {
  order: DeliveredOrderSummaryDto | undefined
}) {
  if (!order) return null

  const deliveredAt = order.deliveredAt ? new Date(order.deliveredAt) : null
  const warrantyExpiresAt = order.warrantyExpiresAt ? new Date(order.warrantyExpiresAt) : null
  const daysRemaining = order.daysRemainingInWarranty

  if (!deliveredAt || !warrantyExpiresAt) return null

  return (
    <div
      className={`rounded-lg border p-3 text-sm space-y-2 ${
        order.isUnderWarranty
          ? "bg-blue-50 border-blue-200"
          : "bg-red-50 border-red-200"
      }`}
    >
      <div className="flex items-center gap-2 font-medium">
        {order.isUnderWarranty ? (
          <IconShieldCheck className="h-4 w-4 text-blue-600" />
        ) : (
          <IconAlertTriangle className="h-4 w-4 text-red-600" />
        )}
        <span className={order.isUnderWarranty ? "text-blue-800" : "text-red-800"}>
          {order.isUnderWarranty ? "Under Warranty" : "Warranty Expired"}
        </span>
      </div>
      <div className="grid grid-cols-2 gap-3 text-xs">
        <div>
          <span className="text-muted-foreground">Delivery Date:</span>
          <div className="font-medium">{format(deliveredAt, "dd/MM/yyyy")}</div>
        </div>
        <div>
          <span className="text-muted-foreground">Warranty Expires:</span>
          <div className="font-medium">{format(warrantyExpiresAt, "dd/MM/yyyy")}</div>
        </div>
      </div>
      {order.isUnderWarranty && daysRemaining != null && (
        <div className="text-xs text-blue-700">
          <strong>{daysRemaining}</strong> day{daysRemaining !== 1 ? "s" : ""} remaining
        </div>
      )}
      {!order.isUnderWarranty && (
        <div className="text-xs text-red-700">
          Warranty expired{" "}
          {warrantyExpiresAt && Math.abs(differenceInDays(warrantyExpiresAt, new Date()))} days ago
        </div>
      )}
    </div>
  )
}

export function WarrantyClaimForm({ open, onOpenChange }: WarrantyClaimFormProps) {
  const { data: deliveredOrders = EMPTY_ORDERS, isLoading: ordersLoading } = useDeliveredOrders()
  const createMutation = useCreateWarrantyClaim()

  const [selectedOrder, setSelectedOrder] = useState<DeliveredOrderSummaryDto | undefined>()

  const form = useForm<WarrantyClaimFormValues>({
    resolver: zodResolver(warrantyClaimSchema),
    defaultValues: {
      glassesOrderId: "",
      resolutionType: "Repair",
      assessmentNotes: "",
      discountAmount: null,
    },
  })

  const watchedOrderId = form.watch("glassesOrderId")
  const watchedResolution = form.watch("resolutionType")

  useEffect(() => {
    if (watchedOrderId) {
      const order = deliveredOrders.find((o) => o.id === watchedOrderId)
      setSelectedOrder(order)
    } else {
      setSelectedOrder(undefined)
    }
  }, [watchedOrderId, deliveredOrders])

  useEffect(() => {
    if (!open) {
      form.reset()
      setSelectedOrder(undefined)
    }
  }, [open, form])

  const isSubmitting = createMutation.isPending
  const isOutOfWarranty = selectedOrder != null && !selectedOrder.isUnderWarranty
  const canSubmit = !isSubmitting && !isOutOfWarranty && !!selectedOrder

  const handleSubmit = async (data: WarrantyClaimFormValues) => {
    try {
      await createMutation.mutateAsync({
        glassesOrderId: data.glassesOrderId,
        resolutionType: RESOLUTION_TYPE_MAP[data.resolutionType],
        notes: data.assessmentNotes,
      })
      toast.success("Warranty claim filed successfully.")
      onOpenChange(false)
    } catch {
      // error handled by mutation onError
    }
  }

  const getErrorMessage = (error: { message?: string } | undefined): string | undefined => {
    if (!error?.message) return undefined
    if (error.message === "required") return "This field is required"
    return error.message
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>File Warranty Claim</DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Glasses Order selection */}
          <Controller
            name="glassesOrderId"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel>Glasses Order</FieldLabel>
                <Select
                  value={field.value}
                  onValueChange={field.onChange}
                  disabled={ordersLoading}
                >
                  <SelectTrigger>
                    <SelectValue
                      placeholder={
                        ordersLoading ? "Loading orders..." : "Select delivered order"
                      }
                    />
                  </SelectTrigger>
                  <SelectContent>
                    {deliveredOrders.map((order) => (
                      <SelectItem key={order.id} value={order.id}>
                        <span className="font-medium">{order.patientName}</span>
                        <span className="text-muted-foreground ml-2 text-xs">
                          Delivered {order.deliveredAt
                            ? format(new Date(order.deliveredAt), "dd/MM/yyyy")
                            : "—"}
                        </span>
                        {!order.isUnderWarranty && (
                          <Badge variant="destructive" className="ml-2 text-xs">
                            Expired
                          </Badge>
                        )}
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

          {/* Warranty info panel */}
          {selectedOrder && <WarrantyInfoPanel order={selectedOrder} />}

          {/* Out of warranty warning */}
          {isOutOfWarranty && (
            <div className="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-800">
              <IconAlertTriangle className="h-4 w-4 mt-0.5 shrink-0" />
              <span>
                This order is no longer under warranty. Warranty claims can only be filed within
                12 months of delivery.
              </span>
            </div>
          )}

          {/* Resolution Type */}
          <Controller
            name="resolutionType"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel>Resolution Type</FieldLabel>
                <RadioGroup
                  value={field.value}
                  onValueChange={field.onChange}
                  className="space-y-2"
                >
                  {RESOLUTION_TYPES.map((type) => (
                    <div key={type.value} className="flex items-start gap-3">
                      <RadioGroupItem
                        value={type.value}
                        id={`resolution-${type.value}`}
                        className="mt-0.5"
                      />
                      <Label
                        htmlFor={`resolution-${type.value}`}
                        className="cursor-pointer leading-none"
                      >
                        <span className="font-medium">{type.label}</span>
                        <span className="block text-xs text-muted-foreground mt-0.5">
                          {type.description}
                        </span>
                      </Label>
                    </div>
                  ))}
                </RadioGroup>
                {fieldState.error && (
                  <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Manager approval notice for Replace */}
          {watchedResolution === "Replace" && (
            <div className="flex items-start gap-2 rounded-lg border border-blue-200 bg-blue-50 p-3 text-sm text-blue-800">
              <IconInfoCircle className="h-4 w-4 mt-0.5 shrink-0" />
              <span>
                <strong>Requires manager approval.</strong> Replacement claims must be reviewed
                and approved by a manager before proceeding.
              </span>
            </div>
          )}

          {/* Discount Amount (shown only for Discount resolution) */}
          {watchedResolution === "Discount" && (
            <Controller
              name="discountAmount"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel>Discount Amount (VND)</FieldLabel>
                  <Input
                    type="number"
                    min={0}
                    step={1000}
                    value={field.value ?? ""}
                    onChange={(e) => {
                      const val = e.target.value
                      field.onChange(val === "" ? null : Number(val))
                    }}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                  )}
                </Field>
              )}
            />
          )}

          {/* Assessment Notes */}
          <Controller
            name="assessmentNotes"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel>Assessment Notes</FieldLabel>
                <Textarea
                  {...field}
                  rows={4}
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
            >
              Cancel
            </Button>
            <Button type="submit" disabled={!canSubmit}>
              {isSubmitting && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              File Claim
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
