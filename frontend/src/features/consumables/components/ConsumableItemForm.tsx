import { useEffect, useMemo } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { createValidationMessages } from "@/shared/lib/validation"
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
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { RadioGroup, RadioGroupItem } from "@/shared/components/RadioGroup"
import { Label } from "@/shared/components/Label"
import { type ConsumableItemDto } from "@/features/consumables/api/consumables-api"
import {
  useCreateConsumableItem,
  useUpdateConsumableItem,
} from "@/features/consumables/api/consumables-queries"

// Tracking mode enum matching backend ConsumableTrackingMode
const TRACKING_MODE_EXPIRY = 0
const TRACKING_MODE_SIMPLE = 1

// ---- Schema ----

function createConsumableItemSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    name: z.string().min(1, v.required).max(200),
    nameVi: z.string().min(1, v.required).max(200),
    unit: z.string().min(1, v.required).max(50),
    trackingMode: z.number().int().min(0).max(1),
    minStockLevel: z.coerce.number().int().min(0, v.mustBeNonNegative),
  })
}

type ConsumableItemValues = z.infer<ReturnType<typeof createConsumableItemSchema>>

// ---- Props ----

interface ConsumableItemFormProps {
  mode: "create" | "edit"
  item?: ConsumableItemDto
  open: boolean
  onOpenChange: (open: boolean) => void
}

// ---- Component ----

export function ConsumableItemForm({
  mode,
  item,
  open,
  onOpenChange,
}: ConsumableItemFormProps) {
  const { t: tCommon } = useTranslation("common")
  const createMutation = useCreateConsumableItem()
  const updateMutation = useUpdateConsumableItem()

  const consumableItemSchema = useMemo(() => createConsumableItemSchema(tCommon), [tCommon])
  const form = useForm<ConsumableItemValues>({
    resolver: zodResolver(consumableItemSchema),
    defaultValues: {
      name: "",
      nameVi: "",
      unit: "",
      trackingMode: TRACKING_MODE_SIMPLE,
      minStockLevel: 0,
    },
  })

  useEffect(() => {
    if (open && mode === "edit" && item) {
      form.reset({
        name: item.name,
        nameVi: item.nameVi,
        unit: item.unit,
        trackingMode: item.trackingMode,
        minStockLevel: item.minStockLevel,
      })
    } else if (open && mode === "create") {
      form.reset({
        name: "",
        nameVi: "",
        unit: "",
        trackingMode: TRACKING_MODE_SIMPLE,
        minStockLevel: 0,
      })
    }
  }, [open, mode, item, form])

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  const handleSubmit = async (data: ConsumableItemValues) => {
    try {
      if (mode === "create") {
        await createMutation.mutateAsync({
          name: data.name,
          nameVi: data.nameVi,
          unit: data.unit,
          trackingMode: data.trackingMode,
          minStockLevel: data.minStockLevel,
        })
        toast.success("Đã thêm vật tư tiêu hao")
      } else if (item) {
        await updateMutation.mutateAsync({
          id: item.id,
          name: data.name,
          nameVi: data.nameVi,
          unit: data.unit,
          minStockLevel: data.minStockLevel,
        })
        toast.success("Đã cập nhật vật tư tiêu hao")
      }
      onOpenChange(false)
    } catch {
      // onError in mutation handles toast
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>
            {mode === "create" ? "Thêm vật tư tiêu hao" : "Chỉnh sửa vật tư"}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Name VI */}
          <Controller
            name="nameVi"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>Tên (Tiếng Việt)</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error?.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Name EN */}
          <Controller
            name="name"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>Tên (Tiếng Anh)</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error?.message}</FieldError>
                )}
              </Field>
            )}
          />

          <div className="grid grid-cols-2 gap-4">
            {/* Unit */}
            <Controller
              name="unit"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>Đơn vị</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Min Stock Level */}
            <Controller
              name="minStockLevel"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>Tồn kho tối thiểu</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="number"
                    min={0}
                    step={1}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          {/* Tracking Mode (only in create mode) */}
          {mode === "create" && (
            <Controller
              name="trackingMode"
              control={form.control}
              render={({ field }) => (
                <Field>
                  <FieldLabel>Phương thức theo dõi</FieldLabel>
                  <RadioGroup
                    value={String(field.value)}
                    onValueChange={(v) => field.onChange(Number(v))}
                    className="mt-1 space-y-2"
                  >
                    <div className="flex items-start gap-3">
                      <RadioGroupItem
                        value={String(TRACKING_MODE_EXPIRY)}
                        id="tracking-expiry"
                        className="mt-0.5"
                      />
                      <div>
                        <Label htmlFor="tracking-expiry" className="font-medium cursor-pointer">
                          Theo dõi hạn sử dụng
                        </Label>
                        <p className="text-xs text-muted-foreground">
                          Theo dõi theo lô với số lô và ngày hết hạn
                        </p>
                      </div>
                    </div>
                    <div className="flex items-start gap-3">
                      <RadioGroupItem
                        value={String(TRACKING_MODE_SIMPLE)}
                        id="tracking-simple"
                        className="mt-0.5"
                      />
                      <div>
                        <Label htmlFor="tracking-simple" className="font-medium cursor-pointer">
                          Theo dõi số lượng đơn giản
                        </Label>
                        <p className="text-xs text-muted-foreground">
                          Chỉ theo dõi tổng số lượng tồn kho
                        </p>
                      </div>
                    </div>
                  </RadioGroup>
                </Field>
              )}
            />
          )}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Huỷ
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />}
              Lưu
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
