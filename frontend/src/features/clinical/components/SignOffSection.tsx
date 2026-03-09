import { useState } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconLoader2, IconShieldCheck } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/AlertDialog"
import { useSignOffVisit, type VisitDetailDto } from "../api/clinical-api"
import { AmendmentDialog, type VisitBaseline } from "./AmendmentDialog"

/**
 * Computes the actual field-level diff between the baseline snapshot
 * (captured at amendment initiation) and the current visit state.
 * Only returns fields that actually changed.
 */
function computeFieldChanges(visit: VisitDetailDto): string {
  const latestAmendment = [...visit.amendments].sort(
    (a, b) => new Date(b.amendedAt).getTime() - new Date(a.amendedAt).getTime(),
  )[0]

  if (!latestAmendment) return "[]"

  let baseline: VisitBaseline
  try {
    const parsed = JSON.parse(latestAmendment.fieldChangesJson)
    // If fieldChangesJson is already an array (old format), skip diff computation
    if (Array.isArray(parsed)) return latestAmendment.fieldChangesJson
    baseline = parsed as VisitBaseline
  } catch {
    return "[]"
  }

  const changes: Array<{ field: string; oldValue: string; newValue: string }> = []

  // Compare examination notes
  const currentNotes = visit.examinationNotes ?? null
  if (baseline.examinationNotes !== currentNotes) {
    changes.push({
      field: "examinationNotes",
      oldValue: baseline.examinationNotes ?? "-",
      newValue: currentNotes ?? "-",
    })
  }

  // Compare refractions by type
  const refractionTypeLabel = (type: number) =>
    type === 0 ? "manifest" : type === 1 ? "autorefraction" : "cycloplegic"

  const refFields = [
    "odSph", "odCyl", "odAxis", "osSph", "osCyl", "osAxis",
    "odAdd", "odPd", "osAdd", "osPd",
    "ucvaOd", "ucvaOs", "bcvaOd", "bcvaOs",
    "iopOd", "iopOs", "axialLengthOd", "axialLengthOs",
  ] as const

  if (baseline.refractions) {
    for (const baseRef of baseline.refractions) {
      const currentRef = visit.refractions.find((r) => r.type === baseRef.type)
      if (!currentRef) {
        changes.push({
          field: `refraction.${refractionTypeLabel(baseRef.type)}`,
          oldValue: JSON.stringify(baseRef),
          newValue: "(removed)",
        })
        continue
      }
      // Compare each field
      for (const f of refFields) {
        const oldVal = baseRef[f]
        const newVal = currentRef[f as keyof typeof currentRef] as number | null
        if (oldVal !== newVal) {
          changes.push({
            field: `refraction.${refractionTypeLabel(baseRef.type)}.${f}`,
            oldValue: oldVal != null ? String(oldVal) : "-",
            newValue: newVal != null ? String(newVal) : "-",
          })
        }
      }
    }

    // Check for new refractions added during amendment
    for (const curRef of visit.refractions) {
      if (!baseline.refractions.find((r) => r.type === curRef.type)) {
        changes.push({
          field: `refraction.${refractionTypeLabel(curRef.type)}`,
          oldValue: "(none)",
          newValue: "(added)",
        })
      }
    }
  }

  // Compare diagnoses
  if (baseline.diagnoses) {
    const baselineDx = new Set(
      baseline.diagnoses.map((d) => `${d.icd10Code}:${d.laterality}`),
    )
    const currentDx = new Set(
      visit.diagnoses.map((d) => `${d.icd10Code}:${d.laterality}`),
    )

    for (const key of baselineDx) {
      if (!currentDx.has(key)) {
        changes.push({ field: "diagnosis", oldValue: key, newValue: "(removed)" })
      }
    }
    for (const key of currentDx) {
      if (!baselineDx.has(key)) {
        changes.push({ field: "diagnosis", oldValue: "(none)", newValue: key })
      }
    }
  }

  return JSON.stringify(changes)
}

interface SignOffSectionProps {
  visitId: string
  visit: VisitDetailDto
}

export function SignOffSection({ visitId, visit }: SignOffSectionProps) {
  const { t } = useTranslation("clinical")
  const { t: tCommon } = useTranslation("common")
  const signOffMutation = useSignOffVisit()
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [amendOpen, setAmendOpen] = useState(false)

  const handleSignOff = () => {
    const isReSign = visit.status === 2 // VisitStatus.Amended
    const fieldChangesJson = isReSign ? computeFieldChanges(visit) : undefined

    signOffMutation.mutate(
      { visitId, fieldChangesJson },
      {
        onSuccess: () => {
          toast.success(t("visit.signedOff"))
          setConfirmOpen(false)
        },
        onError: (err) => {
          toast.error(err instanceof Error ? err.message : tCommon("status.error"))
        },
      },
    )
  }

  const isSigned = visit.status === 1

  const signedAt = visit.signedAt
    ? new Date(visit.signedAt).toLocaleString(undefined, {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
      })
    : null

  return (
    <div className="space-y-3">
      {isSigned ? (
        <div className="flex items-center justify-between p-4 border rounded-lg">
          <div className="flex items-center gap-2">
            <IconShieldCheck className="h-5 w-5 text-primary" />
            <Badge variant="default">{t("visit.status.signed")}</Badge>
            {signedAt && (
              <span className="text-sm text-muted-foreground">
                {t("visit.signedAt", { date: signedAt })}
              </span>
            )}
          </div>
          <Button variant="outline" onClick={() => setAmendOpen(true)}>
            {t("visit.amend")}
          </Button>
        </div>
      ) : (
        <Button
          className="w-full"
          size="lg"
          onClick={() => setConfirmOpen(true)}
          disabled={signOffMutation.isPending}
        >
          {signOffMutation.isPending && (
            <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
          )}
          {t("visit.signOffButton")}
        </Button>
      )}

      {/* Sign-off confirmation dialog */}
      <AlertDialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t("visit.signOffTitle")}</AlertDialogTitle>
            <AlertDialogDescription>
              {t("visit.signOffConfirm")}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{tCommon("buttons.cancel")}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleSignOff}
              disabled={signOffMutation.isPending}
            >
              {signOffMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {tCommon("buttons.confirm")}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Amendment dialog */}
      <AmendmentDialog
        visitId={visitId}
        visit={visit}
        open={amendOpen}
        onClose={() => setAmendOpen(false)}
      />
    </div>
  )
}
