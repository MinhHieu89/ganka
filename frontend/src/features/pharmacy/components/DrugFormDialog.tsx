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
import { Textarea } from "@/shared/components/Textarea"
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
  type DrugCatalogItemDto,
  DRUG_FORM_MAP,
  DRUG_ROUTE_MAP,
  useCreateDrugCatalogItem,
  useUpdateDrugCatalogItem,
} from "@/features/pharmacy/api/pharmacy-api"

const drugFormSchema = z.object({
  name: z.string().min(1, "required").max(200),
  nameVi: z.string().min(1, "required").max(200),
  genericName: z.string().min(1, "required").max(200),
  form: z.number().min(0),
  strength: z.string().max(50).optional().or(z.literal("")),
  route: z.number().min(0),
  unit: z.string().min(1, "required").max(50),
  defaultDosageTemplate: z.string().max(500).optional().or(z.literal("")),
})

type DrugFormValues = z.infer<typeof drugFormSchema>

interface DrugFormDialogProps {
  mode: "create" | "edit"
  drug?: DrugCatalogItemDto
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function DrugFormDialog({
  mode,
  drug,
  open,
  onOpenChange,
}: DrugFormDialogProps) {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")

  const createMutation = useCreateDrugCatalogItem()
  const updateMutation = useUpdateDrugCatalogItem()

  const form = useForm<DrugFormValues>({
    resolver: zodResolver(drugFormSchema),
    defaultValues: {
      name: "",
      nameVi: "",
      genericName: "",
      form: 0,
      strength: "",
      route: 0,
      unit: "",
      defaultDosageTemplate: "",
    },
  })

  useEffect(() => {
    if (open && mode === "edit" && drug) {
      form.reset({
        name: drug.name,
        nameVi: drug.nameVi,
        genericName: drug.genericName,
        form: drug.form,
        strength: drug.strength ?? "",
        route: drug.route,
        unit: drug.unit,
        defaultDosageTemplate: drug.defaultDosageTemplate ?? "",
      })
    } else if (open && mode === "create") {
      form.reset({
        name: "",
        nameVi: "",
        genericName: "",
        form: 0,
        strength: "",
        route: 0,
        unit: "",
        defaultDosageTemplate: "",
      })
    }
  }, [open, mode, drug, form])

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  const handleSubmit = async (data: DrugFormValues) => {
    const command = {
      name: data.name,
      nameVi: data.nameVi,
      genericName: data.genericName,
      form: data.form,
      strength: data.strength || null,
      route: data.route,
      unit: data.unit,
      defaultDosageTemplate: data.defaultDosageTemplate || null,
    }

    try {
      if (mode === "create") {
        await createMutation.mutateAsync(command)
        toast.success(t("catalog.created"))
      } else if (drug) {
        await updateMutation.mutateAsync({ id: drug.id, ...command })
        toast.success(t("catalog.updated"))
      }
      onOpenChange(false)
    } catch {
      // onError callback in mutation handles toast
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
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {mode === "create" ? t("catalog.addDrug") : t("catalog.editDrug")}
          </DialogTitle>
        </DialogHeader>

        <form
          onSubmit={form.handleSubmit(handleSubmit)}
          className="space-y-4"
        >
          <Controller
            name="name"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>
                  {t("catalog.name")}
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
            name="nameVi"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>
                  {t("catalog.nameVi")}
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
            name="genericName"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>
                  {t("catalog.genericName")}
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

          <div className="grid grid-cols-2 gap-4">
            <Controller
              name="form"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel>{t("catalog.form")}</FieldLabel>
                  <Select
                    value={String(field.value)}
                    onValueChange={(v) => field.onChange(Number(v))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {Object.entries(DRUG_FORM_MAP).map(([value, key]) => (
                        <SelectItem key={value} value={value}>
                          {t(`form.${key}`)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {fieldState.error && (
                    <FieldError>
                      {getErrorMessage(fieldState.error)}
                    </FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="route"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel>{t("catalog.route")}</FieldLabel>
                  <Select
                    value={String(field.value)}
                    onValueChange={(v) => field.onChange(Number(v))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {Object.entries(DRUG_ROUTE_MAP).map(([value, key]) => (
                        <SelectItem key={value} value={value}>
                          {t(`route.${key}`)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {fieldState.error && (
                    <FieldError>
                      {getErrorMessage(fieldState.error)}
                    </FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Controller
              name="strength"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>
                    {t("catalog.strength")}
                  </FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>
                      {getErrorMessage(fieldState.error)}
                    </FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="unit"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>
                    {t("catalog.unit")}
                  </FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>
                      {getErrorMessage(fieldState.error)}
                    </FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          <Controller
            name="defaultDosageTemplate"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>
                  {t("catalog.defaultDosage")}
                </FieldLabel>
                <Textarea
                  {...field}
                  id={field.name}
                  rows={2}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>
                    {getErrorMessage(fieldState.error)}
                  </FieldError>
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
