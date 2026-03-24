import { useMemo, useCallback } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { createValidationMessages } from "@/shared/lib/validation"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"
import { IconLoader2, IconAlertTriangle } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Badge } from "@/shared/components/Badge"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { formatVND } from "@/shared/lib/format-vnd"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import { useRequestCancellation } from "@/features/treatment/api/treatment-api"
import type { TreatmentPackageDto } from "@/features/treatment/api/treatment-types"

// -- Schema --

function createCancellationSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    reason: z.string().min(1, v.reasonRequired),
  })
}

type CancellationFormValues = z.infer<ReturnType<typeof createCancellationSchema>>

// -- Treatment type badge styles --

const TREATMENT_TYPE_STYLES: Record<string, string> = {
  IPL: "border-violet-500 text-violet-700 dark:text-violet-400",
  LLLT: "border-blue-500 text-blue-700 dark:text-blue-400",
  LidCare: "border-emerald-500 text-emerald-700 dark:text-emerald-400",
}

// -- Props --

interface CancellationRequestDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  package: TreatmentPackageDto | null
}

export function CancellationRequestDialog({
  open,
  onOpenChange,
  package: pkg,
}: CancellationRequestDialogProps) {
  const { t } = useTranslation("treatment")
  const { t: tCommon } = useTranslation("common")
  const requestMutation = useRequestCancellation()

  const cancellationSchema = useMemo(() => createCancellationSchema(tCommon), [tCommon])
  const form = useForm<CancellationFormValues>({
    resolver: zodResolver(cancellationSchema),
    defaultValues: {
      reason: "",
    },
  })

  // Compute estimated refund based on remaining sessions and default deduction
  const refundEstimate = useMemo(() => {
    if (!pkg) return { deductionPercent: 0, remainingValue: 0, deduction: 0, refund: 0 }

    // Default deduction comes from the protocol template
    const deductionPercent = pkg.cancellationRequest?.deductionPercent ?? 15

    const totalCost =
      pkg.pricingMode === "PerPackage"
        ? pkg.packagePrice
        : pkg.sessionPrice * pkg.totalSessions

    const remainingRatio =
      pkg.totalSessions > 0 ? pkg.sessionsRemaining / pkg.totalSessions : 0
    const remainingValue = totalCost * remainingRatio
    const deduction = remainingValue * (deductionPercent / 100)
    const refund = Math.round(remainingValue - deduction)

    return { deductionPercent, remainingValue: Math.round(remainingValue), deduction: Math.round(deduction), refund }
  }, [pkg])

  const handleSubmit = async (data: CancellationFormValues) => {
    if (!pkg) return

    try {
      await requestMutation.mutateAsync({
        packageId: pkg.id,
        reason: data.reason,
      })
      toast.success(t("cancellationDialog.submitSuccess"))
      onOpenChange(false)
      form.reset()
    } catch (error) {
      const nonFieldErrors = handleServerValidationError(
        error,
        form.setError,
      )
      if (nonFieldErrors.length > 0) {
        toast.error(nonFieldErrors[0])
      }
    }
  }

  const handleOpenChange = (isOpen: boolean) => {
    if (isOpen) {
      form.reset({ reason: "" })
    }
    onOpenChange(isOpen)
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("cancellationDialog.title")}</DialogTitle>
          <DialogDescription>
            {t("cancellationDialog.description")}
          </DialogDescription>
        </DialogHeader>

        {pkg && (
          <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
            {/* Package summary */}
            <div className="rounded-md border p-3 space-y-1.5 text-sm">
              <div>
                <span className="text-muted-foreground">{t("cancellationDialog.patient")}:</span>{" "}
                <span className="font-medium">{pkg.patientName}</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-muted-foreground">{t("cancellationDialog.type")}:</span>
                <Badge
                  variant="outline"
                  className={`text-xs ${TREATMENT_TYPE_STYLES[pkg.treatmentType] ?? ""}`}
                >
                  {pkg.treatmentType}
                </Badge>
              </div>
              <div>
                <span className="text-muted-foreground">{t("cancellationDialog.protocol")}:</span>{" "}
                {pkg.protocolTemplateName}
              </div>
              <div>
                <span className="text-muted-foreground">{t("cancellationDialog.progress")}:</span>{" "}
                {pkg.sessionsCompleted}/{pkg.totalSessions} {t("cancellationDialog.sessions")}
                {pkg.sessionsRemaining > 0 && (
                  <span className="text-muted-foreground">
                    {" "}
                    ({t("cancellationDialog.remaining", { count: pkg.sessionsRemaining })})
                  </span>
                )}
              </div>
              <div>
                <span className="text-muted-foreground">{t("cancellationDialog.price")}:</span>{" "}
                {pkg.pricingMode === "PerPackage"
                  ? formatVND(pkg.packagePrice)
                  : `${formatVND(pkg.sessionPrice)} / ${t("cancellationDialog.perSession")}`}
              </div>
            </div>

            {/* Refund estimate */}
            <div className="rounded-md bg-muted p-3 space-y-1 text-sm">
              <div className="font-medium mb-1.5">{t("cancellationDialog.refundEstimate")}</div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">
                  {t("cancellationDialog.remainingValue", { count: pkg.sessionsRemaining })}:
                </span>
                <span>{formatVND(refundEstimate.remainingValue)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">
                  {t("cancellationDialog.defaultDeduction", { percent: refundEstimate.deductionPercent })}:
                </span>
                <span className="text-red-600">-{formatVND(refundEstimate.deduction)}</span>
              </div>
              <div className="flex justify-between border-t pt-1 mt-1">
                <span className="font-medium">{t("cancellationDialog.estimatedRefund")}:</span>
                <span className="font-semibold">{formatVND(refundEstimate.refund)}</span>
              </div>
            </div>

            {/* Reason */}
            <Controller
              name="reason"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel required htmlFor={field.name}>{t("cancellationDialog.reasonLabel")}</FieldLabel>
                  <AutoResizeTextarea
                    {...field}
                    id={field.name}
                    rows={3}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error.message}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Warning */}
            <div className="flex items-start gap-2 rounded-md border border-yellow-300 bg-yellow-50 dark:border-yellow-700 dark:bg-yellow-950 p-3 text-sm">
              <IconAlertTriangle className="h-4 w-4 text-yellow-600 dark:text-yellow-400 mt-0.5 flex-shrink-0" />
              <span className="text-yellow-800 dark:text-yellow-200">
                {t("cancellationDialog.warning")}
              </span>
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                disabled={requestMutation.isPending}
              >
                {t("cancellationDialog.close")}
              </Button>
              <Button
                type="submit"
                variant="destructive"
                disabled={requestMutation.isPending}
              >
                {requestMutation.isPending && (
                  <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                {t("cancellationDialog.submitButton")}
              </Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  )
}
