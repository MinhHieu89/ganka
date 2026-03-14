import { useEffect } from "react"
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
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Button } from "@/shared/components/Button"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import type { SupplierDto } from "@/features/pharmacy/api/pharmacy-api"
import {
  useCreateSupplier,
  useUpdateSupplier,
} from "@/features/pharmacy/api/pharmacy-queries"

const supplierSchema = z.object({
  name: z.string().min(1, "required").max(200),
  contactInfo: z.string().max(500).optional().or(z.literal("")),
})

type SupplierFormValues = z.infer<typeof supplierSchema>

interface SupplierFormProps {
  supplier?: SupplierDto
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess?: () => void
}

export function SupplierForm({
  supplier,
  open,
  onOpenChange,
  onSuccess,
}: SupplierFormProps) {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")

  const isEdit = !!supplier

  const createMutation = useCreateSupplier()
  const updateMutation = useUpdateSupplier()

  const form = useForm<SupplierFormValues>({
    resolver: zodResolver(supplierSchema),
    defaultValues: {
      name: "",
      contactInfo: "",
    },
  })

  useEffect(() => {
    if (open) {
      if (isEdit && supplier) {
        form.reset({
          name: supplier.name,
          contactInfo: supplier.contactInfo ?? "",
        })
      } else {
        form.reset({
          name: "",
          contactInfo: "",
        })
      }
    }
  }, [open, isEdit, supplier, form])

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  const handleSubmit = async (data: SupplierFormValues) => {
    const input = {
      name: data.name,
      contactInfo: data.contactInfo || null,
    }

    try {
      if (isEdit && supplier) {
        await updateMutation.mutateAsync({ id: supplier.id, ...input })
        toast.success(t("supplier.updated"))
      } else {
        await createMutation.mutateAsync(input)
        toast.success(t("supplier.created"))
      }
      onOpenChange(false)
      onSuccess?.()
    } catch {
      // onError in mutation handles toast
    }
  }

  const getErrorMessage = (
    error: { message?: string } | undefined,
  ): string | undefined => {
    if (!error?.message) return undefined
    if (error.message === "required") return tCommon("validation.required")
    return error.message
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>
            {isEdit ? t("supplier.editSupplier") : t("supplier.addSupplier")}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          <Controller
            name="name"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>
                  {t("supplier.name")}
                </FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{getErrorMessage(fieldState.error)}</FieldError>
                )}
              </Field>
            )}
          />

          <Controller
            name="contactInfo"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>
                  {t("supplier.contactInfo")}
                </FieldLabel>
                <AutoResizeTextarea
                  {...field}
                  id={field.name}
                  rows={3}
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
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {tCommon("buttons.save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
