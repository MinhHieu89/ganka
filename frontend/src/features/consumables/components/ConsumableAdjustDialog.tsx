import { useEffect, useState, useMemo } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { createValidationMessages } from "@/shared/lib/validation"
import { toast } from "sonner"
import { IconLoader2, IconArrowRight } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Input } from "@/shared/components/Input"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Button } from "@/shared/components/Button"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { type ConsumableItemDto } from "@/features/consumables/api/consumables-api"
import {
  useAdjustConsumableStock,
  useConsumableBatches,
} from "@/features/consumables/api/consumables-queries"

// Must match backend ConsumableAdjustmentReason enum
const ADJUSTMENT_REASON = {
  Correction: 0,
  WriteOff: 1,
  Damage: 2,
  Expired: 3,
  Other: 4,
} as const

// Must match backend ConsumableTrackingMode enum
const TRACKING_MODE_EXPIRY = 0

function createConsumableAdjustSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    quantity: z.coerce
      .number()
      .int(v.mustBeInteger)
      .refine((val) => val !== 0, { message: v.cannotBeZero }),
    reason: z.coerce.number().int(),
    notes: z.string().max(500).optional().or(z.literal("")),
  })
}

type AdjustmentValues = z.infer<ReturnType<typeof createConsumableAdjustSchema>>

interface ConsumableAdjustDialogProps {
  item: ConsumableItemDto | null
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function ConsumableAdjustDialog({
  item,
  open,
  onOpenChange,
}: ConsumableAdjustDialogProps) {
  const { t: tCommon } = useTranslation("common")
  const adjustStock = useAdjustConsumableStock()
  const isExpiryTracked = item?.trackingMode === TRACKING_MODE_EXPIRY
  const { data: batches } = useConsumableBatches(
    open && isExpiryTracked ? item?.id ?? null : null,
  )

  const [selectedBatchId, setSelectedBatchId] = useState<string>("")

  const selectedBatch = batches?.find((b) => b.id === selectedBatchId)

  const adjustmentSchema = useMemo(() => createConsumableAdjustSchema(tCommon), [tCommon])
  const form = useForm<AdjustmentValues>({
    resolver: zodResolver(adjustmentSchema),
    defaultValues: {
      quantity: 0,
      reason: ADJUSTMENT_REASON.Correction,
      notes: "",
    },
  })

  useEffect(() => {
    if (open) {
      form.reset({
        quantity: 0,
        reason: ADJUSTMENT_REASON.Correction,
        notes: "",
      })
      setSelectedBatchId("")
    }
  }, [open, form])

  const quantity = form.watch("quantity")
  const currentStock = isExpiryTracked
    ? (selectedBatch?.currentQuantity ?? 0)
    : (item?.currentStock ?? 0)
  const newQuantity = currentStock + (Number(quantity) || 0)
  const isInvalid = newQuantity < 0

  const handleSubmit = async (data: AdjustmentValues) => {
    if (!item) return
    if (isInvalid) {
      form.setError("quantity", { message: "Số lượng mới không được âm" })
      return
    }
    if (isExpiryTracked && !selectedBatchId) {
      toast.error("Vui lòng chọn lô hàng")
      return
    }
    try {
      await adjustStock.mutateAsync({
        id: item.id,
        quantityChange: data.quantity,
        reason: data.reason,
        notes: data.notes || null,
        consumableBatchId: isExpiryTracked ? selectedBatchId : null,
      })
      toast.success("Đã điều chỉnh tồn kho")
      onOpenChange(false)
    } catch {
      // onError in mutation handles toast
    }
  }

  if (!item) return null

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Điều chỉnh tồn kho</DialogTitle>
          <p className="text-sm text-muted-foreground">{item.nameVi || item.name}</p>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Batch selector for ExpiryTracked items */}
          {isExpiryTracked && (
            <Field>
              <FieldLabel>Lô hàng</FieldLabel>
              <Select value={selectedBatchId} onValueChange={setSelectedBatchId}>
                <SelectTrigger>
                  <SelectValue placeholder="Chọn lô hàng..." />
                </SelectTrigger>
                <SelectContent>
                  {batches
                    ?.filter((b) => b.currentQuantity > 0)
                    .map((b) => (
                      <SelectItem key={b.id} value={b.id}>
                        {b.batchNumber} — SL: {b.currentQuantity} — HSD:{" "}
                        {new Date(b.expiryDate).toLocaleDateString("vi-VN")}
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
            </Field>
          )}

          {/* Current → New preview */}
          <div className="flex items-center gap-3 rounded-lg border bg-muted/30 px-4 py-3">
            <div className="text-center">
              <p className="text-xs text-muted-foreground">Hiện tại</p>
              <p className="text-xl font-bold">{currentStock}</p>
            </div>
            <IconArrowRight className="h-5 w-5 text-muted-foreground shrink-0" />
            <div className="text-center">
              <p className="text-xs text-muted-foreground">Sau điều chỉnh</p>
              <p
                className={`text-xl font-bold ${
                  isInvalid
                    ? "text-destructive"
                    : newQuantity !== currentStock
                      ? "text-primary"
                      : ""
                }`}
              >
                {newQuantity}
              </p>
            </div>
          </div>

          {isInvalid && (
            <p className="text-sm text-destructive">Số lượng mới không được âm</p>
          )}

          {/* Quantity change */}
          <Controller
            name="quantity"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>Thay đổi số lượng</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  type="number"
                  step={1}
                  aria-invalid={fieldState.invalid || undefined}
                />
                <p className="text-xs text-muted-foreground">
                  Nhập số dương để tăng, số âm để giảm
                </p>
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
                <FieldLabel>Lý do điều chỉnh</FieldLabel>
                <Select
                  value={String(field.value)}
                  onValueChange={(v) => field.onChange(Number(v))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value={String(ADJUSTMENT_REASON.Correction)}>
                      Hiệu chỉnh
                    </SelectItem>
                    <SelectItem value={String(ADJUSTMENT_REASON.WriteOff)}>
                      Xuất bỏ
                    </SelectItem>
                    <SelectItem value={String(ADJUSTMENT_REASON.Damage)}>
                      Hư hỏng
                    </SelectItem>
                    <SelectItem value={String(ADJUSTMENT_REASON.Expired)}>
                      Hết hạn
                    </SelectItem>
                    <SelectItem value={String(ADJUSTMENT_REASON.Other)}>
                      Khác
                    </SelectItem>
                  </SelectContent>
                </Select>
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
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
                <FieldLabel htmlFor={field.name}>Ghi chú</FieldLabel>
                <AutoResizeTextarea
                  {...field}
                  id={field.name}
                  rows={2}
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
              disabled={adjustStock.isPending}
            >
              Huỷ
            </Button>
            <Button
              type="submit"
              disabled={
                adjustStock.isPending ||
                isInvalid ||
                (isExpiryTracked && !selectedBatchId)
              }
            >
              {adjustStock.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              Xác nhận
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
