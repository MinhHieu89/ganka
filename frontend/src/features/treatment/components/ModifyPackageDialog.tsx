import { useEffect } from "react"
import { useForm, Controller } from "react-hook-form"
import { useTranslation } from "react-i18next"
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
  DialogDescription,
} from "@/shared/components/Dialog"
import { Input } from "@/shared/components/Input"
import { Button } from "@/shared/components/Button"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { Separator } from "@/shared/components/Separator"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import { useModifyPackage } from "@/features/treatment/api/treatment-api"
import type { TreatmentPackageDto } from "@/features/treatment/api/treatment-types"

// -- Schema --

function createModifyPackageSchema(sessionsCompleted: number, t: (key: string, opts?: Record<string, unknown>) => string) {
  return z.object({
    totalSessions: z.coerce
      .number()
      .int()
      .min(1)
      .max(6)
      .nullable()
      .optional()
      .refine(
        (val) => val == null || val >= sessionsCompleted,
        t("modify.sessionMinError", { count: sessionsCompleted }),
      ),
    parametersJson: z.string().nullable().optional(),
    minIntervalDays: z.coerce.number().int().min(1).nullable().optional(),
    reason: z.string().min(1, t("modify.reasonRequired")),
  })
}

type ModifyPackageFormValues = z.infer<ReturnType<typeof createModifyPackageSchema>>

// -- Props --

interface ModifyPackageDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  package_: TreatmentPackageDto
}

// -- Component --

export function ModifyPackageDialog({
  open,
  onOpenChange,
  package_,
}: ModifyPackageDialogProps) {
  const { t } = useTranslation("treatment")
  const modifyMutation = useModifyPackage(package_.id)
  const schema = createModifyPackageSchema(package_.sessionsCompleted, t)

  const form = useForm<ModifyPackageFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      totalSessions: package_.totalSessions,
      parametersJson: package_.parametersJson || null,
      minIntervalDays: package_.minIntervalDays,
      reason: "",
    },
  })

  // Reset form when dialog opens with current package values
  useEffect(() => {
    if (open) {
      form.reset({
        totalSessions: package_.totalSessions,
        parametersJson: package_.parametersJson || null,
        minIntervalDays: package_.minIntervalDays,
        reason: "",
      })
    }
  }, [open, package_, form])

  const handleSubmit = async (data: ModifyPackageFormValues) => {
    try {
      await modifyMutation.mutateAsync({
        totalSessions: data.totalSessions,
        parametersJson: data.parametersJson,
        minIntervalDays: data.minIntervalDays,
        reason: data.reason,
      })
      toast.success(t("modify.success"))
      onOpenChange(false)
    } catch (error) {
      handleServerValidationError(error, form.setError)
    }
  }

  const isSubmitting = modifyMutation.isPending

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{t("modifyPackage")}</DialogTitle>
          <DialogDescription>
            {t("modify.dialogDescription")}
          </DialogDescription>
        </DialogHeader>

        {/* Current values summary */}
        <div className="text-xs text-muted-foreground rounded-md bg-muted/50 p-3 space-y-1">
          <div className="font-medium text-sm text-foreground mb-1">
            {t("modify.currentValues")}
          </div>
          <div>
            <span className="font-medium">{t("fields.treatmentType")}:</span>{" "}
            {package_.treatmentType}
          </div>
          <div>
            <span className="font-medium">{t("modify.totalSessions")}:</span>{" "}
            {package_.totalSessions}
          </div>
          <div>
            <span className="font-medium">{t("modify.completed")}:</span>{" "}
            {package_.sessionsCompleted}/{package_.totalSessions}
          </div>
          <div>
            <span className="font-medium">{t("modify.minInterval")}:</span>{" "}
            {package_.minIntervalDays}
          </div>
        </div>

        <Separator />

        <form
          onSubmit={form.handleSubmit(handleSubmit)}
          className="space-y-4"
        >
          <div className="grid grid-cols-2 gap-4">
            {/* Total sessions */}
            <Controller
              name="totalSessions"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor="totalSessions">
                    {t("modify.totalSessions")}
                  </FieldLabel>
                  <Input
                    {...field}
                    id="totalSessions"
                    type="number"
                    min={1}
                    max={6}
                    value={field.value ?? ""}
                    onChange={(e) =>
                      field.onChange(
                        e.target.value ? Number(e.target.value) : null,
                      )
                    }
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error.message}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Min interval days */}
            <Controller
              name="minIntervalDays"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor="minIntervalDays">
                    {t("modify.minInterval")}
                  </FieldLabel>
                  <Input
                    {...field}
                    id="minIntervalDays"
                    type="number"
                    min={1}
                    value={field.value ?? ""}
                    onChange={(e) =>
                      field.onChange(
                        e.target.value ? Number(e.target.value) : null,
                      )
                    }
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error.message}</FieldError>
                  )}
                </Field>
              )}
            />
          </div>

          {/* Parameters JSON */}
          <Controller
            name="parametersJson"
            control={form.control}
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="parametersJson">
                  {t("modify.parametersJson")}
                </FieldLabel>
                <AutoResizeTextarea
                  {...field}
                  id="parametersJson"
                  value={field.value ?? ""}
                  onChange={(e) => field.onChange(e.target.value || null)}
                  rows={3}
                  className="font-mono text-xs"
                />
              </Field>
            )}
          />

          {/* Reason - required */}
          <Controller
            name="reason"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor="reason">
                  {t("modify.changeReason")} *
                </FieldLabel>
                <AutoResizeTextarea
                  {...field}
                  id="reason"
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
            >
              {t("modify.cancel")}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {t("modify.submit")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
