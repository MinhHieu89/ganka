import { useForm } from "react-hook-form"
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
import { Button } from "@/shared/components/Button"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { useAmendVisit, type VisitDetailDto } from "../api/clinical-api"

/**
 * Baseline snapshot of the visit's signed state at amendment initiation.
 * Stored as the initial fieldChangesJson value and later compared against
 * the current state at re-sign time to compute the actual before/after diff.
 */
export interface VisitBaseline {
  examinationNotes: string | null
  refractions: Array<{
    type: number
    odSph: number | null; odCyl: number | null; odAxis: number | null
    osSph: number | null; osCyl: number | null; osAxis: number | null
    odAdd: number | null; odPd: number | null
    osAdd: number | null; osPd: number | null
    ucvaOd: number | null; ucvaOs: number | null
    bcvaOd: number | null; bcvaOs: number | null
    iopOd: number | null; iopOs: number | null
    axialLengthOd: number | null; axialLengthOs: number | null
  }>
  diagnoses: Array<{ icd10Code: string; laterality: number }>
}

/**
 * Builds a baseline snapshot of the visit's current signed state.
 * This captures the "before" values at amendment initiation time.
 * The actual diff is computed later at re-sign time in SignOffSection.
 */
export function buildBaselineSnapshot(visit: VisitDetailDto): string {
  const baseline: VisitBaseline = {
    examinationNotes: visit.examinationNotes ?? null,
    refractions: visit.refractions.map((r) => ({
      type: r.type,
      odSph: r.odSph, odCyl: r.odCyl, odAxis: r.odAxis,
      osSph: r.osSph, osCyl: r.osCyl, osAxis: r.osAxis,
      odAdd: r.odAdd, odPd: r.odPd,
      osAdd: r.osAdd, osPd: r.osPd,
      ucvaOd: r.ucvaOd, ucvaOs: r.ucvaOs,
      bcvaOd: r.bcvaOd, bcvaOs: r.bcvaOs,
      iopOd: r.iopOd, iopOs: r.iopOs,
      axialLengthOd: r.axialLengthOd, axialLengthOs: r.axialLengthOs,
    })),
    diagnoses: visit.diagnoses.map((d) => ({
      icd10Code: d.icd10Code,
      laterality: d.laterality,
    })),
  }
  return JSON.stringify(baseline)
}

interface AmendmentDialogProps {
  visitId: string
  visit: VisitDetailDto
  open: boolean
  onClose: () => void
}

export function AmendmentDialog({ visitId, visit, open, onClose }: AmendmentDialogProps) {
  const { t } = useTranslation("clinical")
  const { t: tCommon } = useTranslation("common")
  const amendMutation = useAmendVisit()

  const schema = z.object({
    reason: z.string().min(10, t("visit.amendReasonMin")),
  })

  type FormValues = z.infer<typeof schema>

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { reason: "" },
  })

  const handleSubmit = async (data: FormValues) => {
    try {
      const fieldChangesJson = buildBaselineSnapshot(visit)
      await amendMutation.mutateAsync({
        visitId,
        reason: data.reason,
        fieldChangesJson,
      })
      toast.success(t("visit.amendmentCreated"))
      form.reset()
      onClose()
    } catch (err) {
      toast.error(
        err instanceof Error ? err.message : tCommon("status.error"),
      )
    }
  }

  const handleOpenChange = (v: boolean) => {
    if (!v) {
      onClose()
      form.reset()
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("visit.amend")}</DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          <Field data-invalid={form.formState.errors.reason ? true : undefined}>
            <FieldLabel>{t("visit.amendReason")}</FieldLabel>
            <AutoResizeTextarea
              className="min-h-[100px]"
              {...form.register("reason")}
              aria-invalid={!!form.formState.errors.reason}
            />
            {form.formState.errors.reason && (
              <FieldError>{form.formState.errors.reason.message}</FieldError>
            )}
          </Field>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={amendMutation.isPending}>
              {amendMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {t("visit.amendConfirm")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
