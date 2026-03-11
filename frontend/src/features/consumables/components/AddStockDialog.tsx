import { useEffect } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import { format } from "date-fns"
import { IconLoader2 } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Input } from "@/shared/components/Input"
import { Textarea } from "@/shared/components/Textarea"
import { Button } from "@/shared/components/Button"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { DatePicker } from "@/shared/components/DatePicker"
import { type ConsumableItemDto } from "@/features/consumables/api/consumables-api"
import { useAddConsumableStock } from "@/features/consumables/api/consumables-queries"

// Tracking mode enum matching backend ConsumableTrackingMode
const TRACKING_MODE_EXPIRY = 0
const TRACKING_MODE_SIMPLE = 1

// ---- Schemas ----

const simpleStockSchema = z.object({
  quantity: z.coerce.number().int().min(1, "Số lượng phải >= 1"),
  notes: z.string().max(500).optional().or(z.literal("")),
})

const expiryStockSchema = z.object({
  quantity: z.coerce.number().int().min(1, "Số lượng phải >= 1"),
  batchNumber: z.string().min(1, "Bắt buộc").max(100),
  expiryDate: z.date({ required_error: "Bắt buộc" }),
  notes: z.string().max(500).optional().or(z.literal("")),
})

type SimpleStockValues = z.infer<typeof simpleStockSchema>
type ExpiryStockValues = z.infer<typeof expiryStockSchema>

// ---- Props ----

interface AddStockDialogProps {
  item: ConsumableItemDto | null
  open: boolean
  onOpenChange: (open: boolean) => void
}

// ---- Simple Stock Form ----

function SimpleStockForm({
  item,
  open,
  onOpenChange,
}: {
  item: ConsumableItemDto
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const addStock = useAddConsumableStock()

  const form = useForm<SimpleStockValues>({
    resolver: zodResolver(simpleStockSchema),
    defaultValues: { quantity: 1, notes: "" },
  })

  useEffect(() => {
    if (open) {
      form.reset({ quantity: 1, notes: "" })
    }
  }, [open, form])

  const handleSubmit = async (data: SimpleStockValues) => {
    try {
      await addStock.mutateAsync({
        id: item.id,
        quantity: data.quantity,
        notes: data.notes || null,
      })
      toast.success("Đã thêm hàng vào kho")
      onOpenChange(false)
    } catch {
      // onError in mutation handles toast
    }
  }

  return (
    <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
      <Controller
        name="quantity"
        control={form.control}
        render={({ field, fieldState }) => (
          <Field data-invalid={fieldState.invalid || undefined}>
            <FieldLabel htmlFor={field.name}>Số lượng</FieldLabel>
            <Input
              {...field}
              id={field.name}
              type="number"
              min={1}
              step={1}
              aria-invalid={fieldState.invalid || undefined}
            />
            {fieldState.error && (
              <FieldError>{fieldState.error.message}</FieldError>
            )}
          </Field>
        )}
      />

      <Controller
        name="notes"
        control={form.control}
        render={({ field, fieldState }) => (
          <Field data-invalid={fieldState.invalid || undefined}>
            <FieldLabel htmlFor={field.name}>Ghi chú</FieldLabel>
            <Textarea
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
        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
          Huỷ
        </Button>
        <Button type="submit" disabled={addStock.isPending}>
          {addStock.isPending && <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />}
          Thêm hàng
        </Button>
      </DialogFooter>
    </form>
  )
}

// ---- Expiry Tracked Form ----

function ExpiryTrackedForm({
  item,
  open,
  onOpenChange,
}: {
  item: ConsumableItemDto
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const addStock = useAddConsumableStock()

  const form = useForm<ExpiryStockValues>({
    resolver: zodResolver(expiryStockSchema),
    defaultValues: {
      quantity: 1,
      batchNumber: "",
      notes: "",
    },
  })

  useEffect(() => {
    if (open) {
      form.reset({
        quantity: 1,
        batchNumber: "",
        notes: "",
      })
    }
  }, [open, form])

  const handleSubmit = async (data: ExpiryStockValues) => {
    try {
      await addStock.mutateAsync({
        id: item.id,
        quantity: data.quantity,
        batchNumber: data.batchNumber,
        expiryDate: format(data.expiryDate, "yyyy-MM-dd"),
        notes: data.notes || null,
      })
      toast.success("Đã thêm lô hàng vào kho")
      onOpenChange(false)
    } catch {
      // onError in mutation handles toast
    }
  }

  const today = new Date()
  const maxDate = new Date(today.getFullYear() + 10, today.getMonth(), today.getDate())

  return (
    <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
      {/* Batch Number */}
      <Controller
        name="batchNumber"
        control={form.control}
        render={({ field, fieldState }) => (
          <Field data-invalid={fieldState.invalid || undefined}>
            <FieldLabel htmlFor={field.name}>Số lô</FieldLabel>
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

      {/* Expiry Date */}
      <Controller
        name="expiryDate"
        control={form.control}
        render={({ field, fieldState }) => (
          <Field data-invalid={fieldState.invalid || undefined}>
            <FieldLabel>Hạn sử dụng</FieldLabel>
            <DatePicker
              value={field.value}
              onChange={field.onChange}
              fromDate={today}
              toDate={maxDate}
              placeholder="Chọn ngày hết hạn"
              className="w-full"
            />
            {fieldState.error && (
              <FieldError>{fieldState.error.message}</FieldError>
            )}
          </Field>
        )}
      />

      {/* Quantity */}
      <Controller
        name="quantity"
        control={form.control}
        render={({ field, fieldState }) => (
          <Field data-invalid={fieldState.invalid || undefined}>
            <FieldLabel htmlFor={field.name}>Số lượng</FieldLabel>
            <Input
              {...field}
              id={field.name}
              type="number"
              min={1}
              step={1}
              aria-invalid={fieldState.invalid || undefined}
            />
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
            <Textarea
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
        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
          Huỷ
        </Button>
        <Button type="submit" disabled={addStock.isPending}>
          {addStock.isPending && <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />}
          Thêm lô hàng
        </Button>
      </DialogFooter>
    </form>
  )
}

// ---- Main Dialog ----

export function AddStockDialog({ item, open, onOpenChange }: AddStockDialogProps) {
  if (!item) return null

  const isExpiryTracked = item.trackingMode === TRACKING_MODE_EXPIRY

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Thêm hàng vào kho</DialogTitle>
          <p className="text-sm text-muted-foreground">{item.nameVi || item.name}</p>
        </DialogHeader>

        {isExpiryTracked ? (
          <ExpiryTrackedForm item={item} open={open} onOpenChange={onOpenChange} />
        ) : (
          <SimpleStockForm item={item} open={open} onOpenChange={onOpenChange} />
        )}
      </DialogContent>
    </Dialog>
  )
}

export { TRACKING_MODE_SIMPLE, TRACKING_MODE_EXPIRY }
