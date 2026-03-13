import { useEffect, useMemo } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"
import type { TFunction } from "i18next"
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
import { Switch } from "@/shared/components/Switch"
import { Label } from "@/shared/components/Label"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  type ServiceCatalogItemDto,
  useCreateServiceCatalogItem,
  useUpdateServiceCatalogItem,
} from "@/features/billing/api/service-catalog-api"

/**
 * Schema factory: accepts translation function so Zod validation messages
 * are displayed in the user's current language (CR-15).
 *
 * isActive is included in both create and edit schemas. For create mode it
 * defaults to true and the field is not rendered, but keeping a single schema
 * simplifies the code without side effects (CR-22).
 */
const createServiceCatalogSchema = (t: TFunction) =>
  z.object({
    code: z.string().min(1, t("validation.required")).max(50),
    name: z.string().min(1, t("validation.required")).max(200),
    nameVi: z.string().min(1, t("validation.required")).max(200),
    price: z.coerce.number().positive(t("validation.mustBePositive")),
    description: z.string().max(500).optional().or(z.literal("")),
    // isActive: present in both create/edit. Defaults to true for create mode.
    isActive: z.boolean().default(true),
  })

type FormValues = z.infer<ReturnType<typeof createServiceCatalogSchema>>

interface ServiceCatalogFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  editItem: ServiceCatalogItemDto | null
}

export function ServiceCatalogFormDialog({
  open,
  onOpenChange,
  editItem,
}: ServiceCatalogFormDialogProps) {
  const { t } = useTranslation("billing")
  const { t: tCommon } = useTranslation("common")
  const createMutation = useCreateServiceCatalogItem()
  const updateMutation = useUpdateServiceCatalogItem()
  const isEdit = !!editItem

  const schema = useMemo(() => createServiceCatalogSchema(t), [t])

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      code: "",
      name: "",
      nameVi: "",
      price: 0,
      description: "",
      isActive: true,
    },
  })

  useEffect(() => {
    if (open) {
      if (editItem) {
        form.reset({
          code: editItem.code,
          name: editItem.name,
          nameVi: editItem.nameVi,
          price: editItem.price,
          description: editItem.description ?? "",
          isActive: editItem.isActive,
        })
      } else {
        form.reset({
          code: "",
          name: "",
          nameVi: "",
          price: 0,
          description: "",
          isActive: true,
        })
      }
    }
  }, [open, editItem, form])

  const onSubmit = async (values: FormValues) => {
    try {
      if (isEdit) {
        await updateMutation.mutateAsync({
          id: editItem.id,
          name: values.name,
          nameVi: values.nameVi,
          price: values.price,
          isActive: values.isActive,
          description: values.description || null,
        })
        toast.success(t("serviceCatalog.updated"))
        onOpenChange(false)
      } else {
        await createMutation.mutateAsync({
          code: values.code,
          name: values.name,
          nameVi: values.nameVi,
          price: values.price,
          description: values.description || null,
        })
        toast.success(t("serviceCatalog.created"))
        onOpenChange(false)
      }
    } catch {
      // Error handled by React Query's onError or global error handler
    }
  }

  const isPending = createMutation.isPending || updateMutation.isPending

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {isEdit
              ? t("serviceCatalog.editService")
              : t("serviceCatalog.addService")}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <Field>
            <FieldLabel>{t("serviceCatalog.code")}</FieldLabel>
            <Input
              {...form.register("code")}
              disabled={isEdit}
            />
            <FieldError>{form.formState.errors.code?.message}</FieldError>
          </Field>

          <Field>
            <FieldLabel>{t("serviceCatalog.name")}</FieldLabel>
            <Input {...form.register("name")} />
            <FieldError>{form.formState.errors.name?.message}</FieldError>
          </Field>

          <Field>
            <FieldLabel>{t("serviceCatalog.nameVi")}</FieldLabel>
            <Input {...form.register("nameVi")} />
            <FieldError>{form.formState.errors.nameVi?.message}</FieldError>
          </Field>

          <Field>
            <FieldLabel>{t("serviceCatalog.price")}</FieldLabel>
            <Input
              type="number"
              {...form.register("price", { valueAsNumber: true })}
            />
            <FieldError>{form.formState.errors.price?.message}</FieldError>
          </Field>

          <Field>
            <FieldLabel>{t("serviceCatalog.description")}</FieldLabel>
            <Textarea {...form.register("description")} rows={3} />
            <FieldError>
              {form.formState.errors.description?.message}
            </FieldError>
          </Field>

          {isEdit && (
            <div className="flex items-center space-x-2">
              <Controller
                control={form.control}
                name="isActive"
                render={({ field }) => (
                  <Switch
                    id="is-active"
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                )}
              />
              <Label htmlFor="is-active">{t("serviceCatalog.active")}</Label>
            </div>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              {tCommon("cancel")}
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending && (
                <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              {tCommon("save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
