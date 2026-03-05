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
import { Textarea } from "@/shared/components/Textarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { useAmendVisit, type VisitDetailDto } from "../api/clinical-api"

/**
 * Builds a snapshot of the visit's current signed state at the moment amendment
 * is initiated. This captures what existed before the amendment for audit purposes.
 *
 * NOTE: Full before/after diff would require a "finalize amendment" endpoint that
 * compares the snapshot baseline to the final state after edits. The current approach
 * captures the signed state at amendment initiation time. (Future enhancement)
 */
function buildFieldChangesSnapshot(visit: VisitDetailDto): string {
  const changes: Array<{ fieldName: string; oldValue: string; newValue: string }> = []

  if (visit.examinationNotes) {
    changes.push({
      fieldName: "examinationNotes",
      oldValue: visit.examinationNotes,
      newValue: "pending_amendment",
    })
  }

  if (visit.refractions.length > 0) {
    for (const r of visit.refractions) {
      const typeLabel =
        r.type === 0
          ? "manifest"
          : r.type === 1
            ? "autorefraction"
            : "cycloplegic"
      changes.push({
        fieldName: `refraction.${typeLabel}`,
        oldValue: JSON.stringify({
          odSph: r.odSph,
          odCyl: r.odCyl,
          odAxis: r.odAxis,
          osSph: r.osSph,
          osCyl: r.osCyl,
          osAxis: r.osAxis,
        }),
        newValue: "pending_amendment",
      })
    }
  }

  if (visit.diagnoses.length > 0) {
    changes.push({
      fieldName: "diagnoses",
      oldValue: JSON.stringify(
        visit.diagnoses.map((d) => ({
          code: d.icd10Code,
          laterality: d.laterality,
        })),
      ),
      newValue: "pending_amendment",
    })
  }

  // If no specific data to snapshot, provide a generic baseline
  if (changes.length === 0) {
    changes.push({
      fieldName: "visit",
      oldValue: "signed_state",
      newValue: "amendment_initiated",
    })
  }

  return JSON.stringify(changes)
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
      const fieldChangesJson = buildFieldChangesSnapshot(visit)
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
            <Textarea
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
