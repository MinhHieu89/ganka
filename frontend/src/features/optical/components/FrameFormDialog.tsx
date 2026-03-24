import { useEffect, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
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
import { NumberInput } from "@/shared/components/NumberInput"
import { Button } from "@/shared/components/Button"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import {
  type FrameDto,
  FRAME_MATERIAL_MAP,
  FRAME_TYPE_MAP,
  FRAME_GENDER_MAP,
} from "@/features/optical/api/optical-api"
import {
  useCreateFrame,
  useUpdateFrame,
  opticalKeys,
} from "@/features/optical/api/optical-queries"
import { useQueryClient } from "@tanstack/react-query"

function createFrameFormSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    brand: z.string().min(1, v.required).max(100),
    model: z.string().min(1, v.required).max(100),
    color: z.string().min(1, v.required).max(100),
    lensWidth: z
      .number({ invalid_type_error: v.required })
      .int()
      .min(40, v.between(40, 65))
      .max(65, v.between(40, 65)),
    bridgeWidth: z
      .number({ invalid_type_error: v.required })
      .int()
      .min(12, v.between(12, 24))
      .max(24, v.between(12, 24)),
    templeLength: z
      .number({ invalid_type_error: v.required })
      .int()
      .min(120, v.between(120, 155))
      .max(155, v.between(120, 155)),
    material: z.number({ invalid_type_error: v.required }).min(0),
    frameType: z.number({ invalid_type_error: v.required }).min(0),
    gender: z.number({ invalid_type_error: v.required }).min(0),
    sellingPrice: z
      .number({ invalid_type_error: v.required })
      .positive(v.mustBePositive),
    costPrice: z
      .number({ invalid_type_error: v.required })
      .positive(v.mustBePositive),
    barcode: z
      .string()
      .regex(/^\d{13}$/, v.exactDigits(13))
      .optional()
      .or(z.literal("")),
    stockQuantity: z.number().int().min(0).optional(),
  })
}

type FrameFormValues = z.infer<ReturnType<typeof createFrameFormSchema>>

interface FrameFormDialogProps {
  mode: "create" | "edit"
  frame?: FrameDto
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function FrameFormDialog({
  mode,
  frame,
  open,
  onOpenChange,
}: FrameFormDialogProps) {
  const queryClient = useQueryClient()
  const { t } = useTranslation("optical")
  const { t: tCommon } = useTranslation("common")
  const createMutation = useCreateFrame()
  const updateMutation = useUpdateFrame()

  const schema = useMemo(() => createFrameFormSchema(tCommon), [tCommon])
  const form = useForm<FrameFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      brand: "",
      model: "",
      color: "",
      lensWidth: 52,
      bridgeWidth: 18,
      templeLength: 140,
      material: 0,
      frameType: 0,
      gender: 2,
      sellingPrice: 0,
      costPrice: 0,
      barcode: "",
      stockQuantity: 0,
    },
  })

  useEffect(() => {
    if (open && mode === "edit" && frame) {
      form.reset({
        brand: frame.brand,
        model: frame.model,
        color: frame.color,
        lensWidth: frame.lensWidth,
        bridgeWidth: frame.bridgeWidth,
        templeLength: frame.templeLength,
        material: frame.material,
        frameType: frame.frameType,
        gender: frame.gender,
        sellingPrice: frame.sellingPrice,
        costPrice: frame.costPrice,
        barcode: frame.barcode ?? "",
        stockQuantity: frame.stockQuantity,
      })
    } else if (open && mode === "create") {
      form.reset({
        brand: "",
        model: "",
        color: "",
        lensWidth: 52,
        bridgeWidth: 18,
        templeLength: 140,
        material: 0,
        frameType: 0,
        gender: 2,
        sellingPrice: 0,
        costPrice: 0,
        barcode: "",
        stockQuantity: 0,
      })
    }
  }, [open, mode, frame, form])

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  const handleSubmit = async (data: FrameFormValues) => {
    const command = {
      brand: data.brand,
      model: data.model,
      color: data.color,
      lensWidth: data.lensWidth,
      bridgeWidth: data.bridgeWidth,
      templeLength: data.templeLength,
      material: data.material,
      frameType: data.frameType,
      gender: data.gender,
      sellingPrice: data.sellingPrice,
      costPrice: data.costPrice,
      barcode: data.barcode || null,
      stockQuantity: data.stockQuantity ?? 0,
    }

    try {
      if (mode === "create") {
        await createMutation.mutateAsync(command)
        toast.success(t("frames.created"))
      } else if (frame) {
        await updateMutation.mutateAsync({ id: frame.id, ...command, stockQuantity: data.stockQuantity ?? frame.stockQuantity, isActive: frame.isActive })
        toast.success(t("frames.updated"))
      }
      await queryClient.invalidateQueries({ queryKey: opticalKeys.frames.all() })
      onOpenChange(false)
    } catch {
      // onError callback in mutation handles toast
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {mode === "create" ? t("frames.addFrame") : t("frames.editFrame")}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Brand and Model */}
          <div className="grid grid-cols-2 gap-4">
            <Controller
              name="brand"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>{t("frames.brand")}</FieldLabel>
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

            <Controller
              name="model"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>{t("frames.model")}</FieldLabel>
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
          </div>

          {/* Color */}
          <Controller
            name="color"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>{t("frames.color")}</FieldLabel>
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

          {/* Size fields */}
          <div className="space-y-1">
            <p className="text-sm font-medium">{t("frames.size")}</p>
            <div className="grid grid-cols-3 gap-4">
              <Controller
                name="lensWidth"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid || undefined}>
                    <FieldLabel htmlFor={field.name}>{t("frames.lensWidth")}</FieldLabel>
                    <NumberInput
                      {...field}
                      id={field.name}
                      min={40}
                      max={65}
                      aria-invalid={fieldState.invalid || undefined}
                    />
                    {fieldState.error && (
                      <FieldError>{fieldState.error?.message}</FieldError>
                    )}
                  </Field>
                )}
              />

              <Controller
                name="bridgeWidth"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid || undefined}>
                    <FieldLabel htmlFor={field.name}>{t("frames.bridgeWidth")}</FieldLabel>
                    <NumberInput
                      {...field}
                      id={field.name}
                      min={12}
                      max={24}
                      aria-invalid={fieldState.invalid || undefined}
                    />
                    {fieldState.error && (
                      <FieldError>{fieldState.error?.message}</FieldError>
                    )}
                  </Field>
                )}
              />

              <Controller
                name="templeLength"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid || undefined}>
                    <FieldLabel htmlFor={field.name}>{t("frames.templeLength")}</FieldLabel>
                    <NumberInput
                      {...field}
                      id={field.name}
                      min={120}
                      max={155}
                      aria-invalid={fieldState.invalid || undefined}
                    />
                    {fieldState.error && (
                      <FieldError>{fieldState.error?.message}</FieldError>
                    )}
                  </Field>
                )}
              />
            </div>
          </div>

          {/* Material, Frame Type, Gender */}
          <div className="grid grid-cols-3 gap-4">
            <Controller
              name="material"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel>{t("frames.material")}</FieldLabel>
                  <Select
                    value={String(field.value)}
                    onValueChange={(v) => field.onChange(Number(v))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {Object.entries(FRAME_MATERIAL_MAP).map(([value, label]) => (
                        <SelectItem key={value} value={value}>
                          {t(label)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="frameType"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel>{t("frames.frameType")}</FieldLabel>
                  <Select
                    value={String(field.value)}
                    onValueChange={(v) => field.onChange(Number(v))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {Object.entries(FRAME_TYPE_MAP).map(([value, label]) => (
                        <SelectItem key={value} value={value}>
                          {t(label)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="gender"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel>{t("frames.gender")}</FieldLabel>
                  <Select
                    value={String(field.value)}
                    onValueChange={(v) => field.onChange(Number(v))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {Object.entries(FRAME_GENDER_MAP).map(([value, label]) => (
                        <SelectItem key={value} value={value}>
                          {t(label)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          {/* Pricing */}
          <div className="grid grid-cols-2 gap-4">
            <Controller
              name="sellingPrice"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>{t("frames.sellingPrice")} (VND)</FieldLabel>
                  <NumberInput
                    {...field}
                    id={field.name}
                    min={0}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="costPrice"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>{t("frames.costPrice")} (VND)</FieldLabel>
                  <NumberInput
                    {...field}
                    id={field.name}
                    min={0}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          {/* Barcode and Stock */}
          <div className="grid grid-cols-2 gap-4">
            <Controller
              name="barcode"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>
                    {t("frames.barcode")} (EAN-13)
                  </FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    inputMode="numeric"
                    maxLength={13}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="stockQuantity"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>{t("frames.stock")}</FieldLabel>
                  <NumberInput
                    {...field}
                    id={field.name}
                    min={0}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              {t("common.cancel")}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {t("common.save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
