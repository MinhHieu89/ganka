import { useEffect, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { createValidationMessages } from "@/shared/lib/validation"
import { toast } from "sonner"
import { IconLoader2, IconTag } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Input } from "@/shared/components/Input"
import { NumberInput } from "@/shared/components/NumberInput"
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
import type { ComboPackageDto } from "@/features/optical/api/optical-api"
import {
  useCreateComboPackage,
  useUpdateComboPackage,
  useFrames,
  useLensCatalog,
} from "@/features/optical/api/optical-queries"

const NONE_VALUE = "__none__"

function createComboSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z
    .object({
      name: z.string().min(1, v.required).max(200),
      description: z.string().max(500).optional().or(z.literal("")),
      frameId: z.string().optional(),
      lensCatalogItemId: z.string().optional(),
      comboPrice: z.coerce.number().positive(v.mustBePositive),
      originalTotalPrice: z.coerce
        .number()
        .positive()
        .optional()
        .or(z.literal(0))
        .transform((val) => (val === 0 ? undefined : val)),
    })
    .refine(
      (data) => {
        if (data.originalTotalPrice && data.originalTotalPrice > 0) {
          return data.comboPrice < data.originalTotalPrice
        }
        return true
      },
      {
        message: "Combo price must be less than the original total price to show savings",
        path: ["comboPrice"],
      },
    )
}

type ComboFormValues = z.infer<ReturnType<typeof createComboSchema>>

interface ComboPackageFormProps {
  combo?: ComboPackageDto
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess?: () => void
}

export function ComboPackageForm({
  combo,
  open,
  onOpenChange,
  onSuccess,
}: ComboPackageFormProps) {
  const { t } = useTranslation("optical")
  const { t: tCommon } = useTranslation("common")
  const isEdit = !!combo

  const createMutation = useCreateComboPackage()
  const updateMutation = useUpdateComboPackage()

  const { data: framesResult } = useFrames({ includeInactive: false })
  const { data: lenses } = useLensCatalog(false)

  const frames = framesResult?.items ?? []

  const comboSchema = useMemo(() => createComboSchema(tCommon), [tCommon])
  const form = useForm<ComboFormValues>({
    resolver: zodResolver(comboSchema),
    defaultValues: {
      name: "",
      description: "",
      frameId: undefined,
      lensCatalogItemId: undefined,
      comboPrice: 0,
      originalTotalPrice: undefined,
    },
  })

  const watchedFrameId = form.watch("frameId")
  const watchedLensId = form.watch("lensCatalogItemId")
  const watchedComboPrice = form.watch("comboPrice")
  const watchedOriginalPrice = form.watch("originalTotalPrice")

  // Auto-populate originalTotalPrice when frame + lens are both selected
  const autoOriginalPrice = useMemo(() => {
    const frame = frames.find((f) => f.id === watchedFrameId)
    const lens = lenses?.find((l) => l.id === watchedLensId)
    if (frame && lens) {
      return frame.sellingPrice + lens.sellingPrice
    }
    return null
  }, [watchedFrameId, watchedLensId, frames, lenses])

  // Set originalTotalPrice when frame + lens combination changes (only if user hasn't manually set it)
  useEffect(() => {
    if (autoOriginalPrice !== null && !isEdit) {
      form.setValue("originalTotalPrice", autoOriginalPrice)
    }
  }, [autoOriginalPrice, form, isEdit])

  // Calculate savings for preview
  const savingsAmount =
    watchedOriginalPrice && watchedOriginalPrice > 0 && watchedComboPrice > 0
      ? watchedOriginalPrice - watchedComboPrice
      : null
  const savingsPct =
    savingsAmount && watchedOriginalPrice
      ? Math.round((savingsAmount / watchedOriginalPrice) * 100)
      : null
  const showSavingsPreview =
    savingsAmount !== null && savingsAmount > 0 && savingsPct !== null && savingsPct > 0

  useEffect(() => {
    if (open) {
      if (isEdit && combo) {
        form.reset({
          name: combo.name,
          description: combo.description ?? "",
          frameId: combo.frameId ?? undefined,
          lensCatalogItemId: combo.lensCatalogItemId ?? undefined,
          comboPrice: combo.comboPrice,
          originalTotalPrice: combo.originalTotalPrice ?? undefined,
        })
      } else {
        form.reset({
          name: "",
          description: "",
          frameId: undefined,
          lensCatalogItemId: undefined,
          comboPrice: 0,
          originalTotalPrice: undefined,
        })
      }
    }
  }, [open, isEdit, combo, form])

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  const handleSubmit = async (data: ComboFormValues) => {
    const input = {
      name: data.name,
      description: data.description || null,
      frameId: data.frameId || null,
      lensCatalogItemId: data.lensCatalogItemId || null,
      comboPrice: data.comboPrice,
      originalTotalPrice: data.originalTotalPrice ?? null,
    }

    try {
      if (isEdit && combo) {
        await updateMutation.mutateAsync({ id: combo.id, ...input })
        toast.success(t("combos.updated"))
      } else {
        await createMutation.mutateAsync(input)
        toast.success(t("combos.created"))
      }
      onOpenChange(false)
      onSuccess?.()
    } catch {
      // onError in mutation handles toast
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {isEdit ? t("combos.editCombo") : t("combos.addCombo")}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Name */}
          <Controller
            name="name"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>{t("combos.name")}</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  placeholder={t("combos.namePlaceholder")}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error?.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Description */}
          <Controller
            name="description"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>{t("combos.description")}</FieldLabel>
                <AutoResizeTextarea
                  {...field}
                  id={field.name}
                  rows={2}
                  placeholder={t("combos.descriptionPlaceholder")}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error?.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Frame select */}
          <Controller
            name="frameId"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor="frameId">{t("combos.frame")}</FieldLabel>
                <Select
                  value={field.value ?? NONE_VALUE}
                  onValueChange={(v) => field.onChange(v === NONE_VALUE ? undefined : v)}
                >
                  <SelectTrigger id="frameId" aria-invalid={fieldState.invalid || undefined}>
                    <SelectValue placeholder={t("combos.selectFrame")} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value={NONE_VALUE}>
                      <span className="text-muted-foreground">{t("combos.anyFrame")}</span>
                    </SelectItem>
                    {frames.map((frame) => (
                      <SelectItem key={frame.id} value={frame.id}>
                        {frame.brand} {frame.model} — {frame.color}
                        <span className="ml-2 text-muted-foreground text-xs">
                          {new Intl.NumberFormat("vi-VN").format(frame.sellingPrice)} ₫
                        </span>
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

          {/* Lens select */}
          <Controller
            name="lensCatalogItemId"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor="lensCatalogItemId">{t("combos.lens")}</FieldLabel>
                <Select
                  value={field.value ?? NONE_VALUE}
                  onValueChange={(v) => field.onChange(v === NONE_VALUE ? undefined : v)}
                >
                  <SelectTrigger
                    id="lensCatalogItemId"
                    aria-invalid={fieldState.invalid || undefined}
                  >
                    <SelectValue placeholder={t("combos.selectLens")} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value={NONE_VALUE}>
                      <span className="text-muted-foreground">{t("combos.anyLens")}</span>
                    </SelectItem>
                    {(lenses ?? []).map((lens) => (
                      <SelectItem key={lens.id} value={lens.id}>
                        {lens.name}
                        <span className="ml-2 text-muted-foreground text-xs">
                          {new Intl.NumberFormat("vi-VN").format(lens.sellingPrice)} ₫
                        </span>
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

          {/* Prices row */}
          <div className="grid grid-cols-2 gap-3">
            {/* Combo Price */}
            <Controller
              name="comboPrice"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>{t("combos.comboPrice")} (₫)</FieldLabel>
                  <NumberInput
                    {...field}
                    id={field.name}
                    min={1}
                    step={1000}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Original Total Price */}
            <Controller
              name="originalTotalPrice"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor="originalTotalPrice">
                    {t("combos.originalPrice")} (₫)
                    {autoOriginalPrice !== null && (
                      <span className="ml-1 text-xs text-muted-foreground font-normal">
                        (auto)
                      </span>
                    )}
                  </FieldLabel>
                  <NumberInput
                    {...field}
                    id="originalTotalPrice"
                    min={0}
                    step={1000}
                    value={field.value ?? ""}
                    onChange={(value) => field.onChange(value)}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error?.message}</FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          {/* Savings preview */}
          {showSavingsPreview && (
            <div className="flex items-center gap-2 rounded-md bg-green-50 dark:bg-green-950/30 border border-green-200 dark:border-green-800 px-3 py-2">
              <IconTag className="h-4 w-4 text-green-600 dark:text-green-400 shrink-0" />
              <span className="text-sm text-green-700 dark:text-green-400">
                Save{" "}
                <strong>
                  {new Intl.NumberFormat("vi-VN").format(savingsAmount!)} ₫
                </strong>{" "}
                ({savingsPct}% off)
              </span>
            </div>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              {t("common.cancel")}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />}
              {isEdit ? t("common.save") : t("combos.addCombo")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
