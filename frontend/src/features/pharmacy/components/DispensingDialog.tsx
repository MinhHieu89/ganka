import { useState, useEffect, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconAlertTriangle, IconLoader2, IconPackage } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Alert, AlertDescription, AlertTitle } from "@/shared/components/Alert"
import { Textarea } from "@/shared/components/Textarea"
import { Separator } from "@/shared/components/Separator"
import type { PendingPrescriptionDto, PendingPrescriptionItemDto } from "@/features/pharmacy/api/pharmacy-api"
import { useDrugBatches } from "@/features/pharmacy/api/pharmacy-queries"
import { useDispenseDrugs } from "@/features/pharmacy/api/pharmacy-queries"

// ---- Types ----

interface LineState {
  prescriptionItemId: string
  drugCatalogItemId: string | null
  drugName: string
  quantity: number
  unit: string
  dosage: string | null
  isOffCatalog: boolean
  skip: boolean
}

// ---- Batch suggestion sub-component ----

interface BatchSuggestionProps {
  drugCatalogItemId: string
  requiredQuantity: number
}

function BatchSuggestion({ drugCatalogItemId, requiredQuantity }: BatchSuggestionProps) {
  const { t } = useTranslation("pharmacy")
  const { data: batches, isLoading } = useDrugBatches(drugCatalogItemId)

  const suggestions = useMemo(() => {
    if (!batches) return []
    // FEFO: sort by expiry date ascending, take available batches
    const available = batches
      .filter((b) => !b.isExpired && b.currentQuantity > 0)
      .sort((a, b) => new Date(a.expiryDate).getTime() - new Date(b.expiryDate).getTime())

    // Show what FEFO would allocate
    const result: Array<{ batchNumber: string; qty: number; expiryDate: string; isNearExpiry: boolean }> = []
    let remaining = requiredQuantity
    for (const batch of available) {
      if (remaining <= 0) break
      const take = Math.min(batch.currentQuantity, remaining)
      result.push({
        batchNumber: batch.batchNumber,
        qty: take,
        expiryDate: batch.expiryDate,
        isNearExpiry: batch.isNearExpiry,
      })
      remaining -= take
    }
    return result
  }, [batches, requiredQuantity])

  if (isLoading) {
    return (
      <div className="flex items-center gap-1 text-xs text-muted-foreground">
        <IconLoader2 className="h-3 w-3 animate-spin" />
        <span>{t("queue.loadingBatches")}</span>
      </div>
    )
  }

  if (suggestions.length === 0) {
    return (
      <span className="text-xs text-destructive font-medium">
        {t("queue.noBatchesAvailable")}
      </span>
    )
  }

  return (
    <div className="space-y-1">
      <p className="text-xs text-muted-foreground font-medium">{t("queue.fefoSuggestion")}:</p>
      <div className="flex flex-wrap gap-1">
        {suggestions.map((s) => (
          <Badge
            key={s.batchNumber}
            variant="outline"
            className={`text-xs font-mono ${s.isNearExpiry ? "border-yellow-500 text-yellow-700 dark:text-yellow-400" : ""}`}
          >
            {s.batchNumber} × {s.qty} ({new Date(s.expiryDate).toLocaleDateString("vi-VN")})
          </Badge>
        ))}
      </div>
    </div>
  )
}

// ---- Line row component ----

interface LineRowProps {
  line: LineState
  onToggleSkip: (prescriptionItemId: string) => void
}

function LineRow({ line, onToggleSkip }: LineRowProps) {
  const { t } = useTranslation("pharmacy")

  return (
    <div
      className={`rounded-lg border p-3 space-y-2 transition-colors ${
        line.skip ? "bg-muted/40 border-border opacity-60" : "bg-card border-border"
      }`}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="font-medium text-sm">{line.drugName}</span>
            {line.isOffCatalog && (
              <Badge variant="outline" className="text-xs">
                {t("queue.offCatalog")}
              </Badge>
            )}
            {line.skip && (
              <Badge variant="secondary" className="text-xs">
                {t("queue.skipped")}
              </Badge>
            )}
          </div>
          <div className="flex items-center gap-3 mt-0.5">
            <span className="text-sm text-muted-foreground">
              {t("queue.quantity")}: {line.quantity} {line.unit}
            </span>
            {line.dosage && (
              <span className="text-xs text-muted-foreground">{line.dosage}</span>
            )}
          </div>
        </div>

        {/* Action toggle - off-catalog always skipped, can't toggle */}
        {!line.isOffCatalog ? (
          <Button
            type="button"
            variant={line.skip ? "outline" : "default"}
            size="sm"
            className="shrink-0"
            onClick={() => onToggleSkip(line.prescriptionItemId)}
          >
            {line.skip ? t("queue.dispense") : t("queue.skip")}
          </Button>
        ) : (
          <Badge variant="secondary" className="text-xs shrink-0 self-start mt-1">
            {t("queue.autoSkipped")}
          </Badge>
        )}
      </div>

      {/* FEFO batch suggestion for catalog drugs that will be dispensed */}
      {!line.skip && !line.isOffCatalog && line.drugCatalogItemId && (
        <div className="pt-1 border-t border-border/50">
          <BatchSuggestion
            drugCatalogItemId={line.drugCatalogItemId}
            requiredQuantity={line.quantity}
          />
        </div>
      )}
    </div>
  )
}

// ---- Main dialog component ----

interface DispensingDialogProps {
  prescription: PendingPrescriptionDto | null
  open: boolean
  onOpenChange: (open: boolean) => void
}

function initLineStates(items: PendingPrescriptionItemDto[]): LineState[] {
  return items.map((item) => ({
    prescriptionItemId: item.prescriptionItemId,
    drugCatalogItemId: item.drugCatalogItemId,
    drugName: item.drugName,
    quantity: item.quantity,
    unit: item.unit,
    dosage: item.dosage,
    isOffCatalog: item.isOffCatalog,
    // Off-catalog items start as skipped (no batch deduction)
    skip: item.isOffCatalog,
  }))
}

export function DispensingDialog({
  prescription,
  open,
  onOpenChange,
}: DispensingDialogProps) {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")
  const dispenseMutation = useDispenseDrugs()

  const [overrideReason, setOverrideReason] = useState("")
  const [lineStates, setLineStates] = useState<LineState[]>([])

  // Reset state when dialog opens/prescription changes
  useEffect(() => {
    if (open && prescription) {
      setLineStates(initLineStates(prescription.items))
      setOverrideReason("")
    }
  }, [open, prescription])

  const toggleSkip = (prescriptionItemId: string) => {
    setLineStates((prev) =>
      prev.map((l) =>
        l.prescriptionItemId === prescriptionItemId ? { ...l, skip: !l.skip } : l,
      ),
    )
  }

  const allSkipped = lineStates.length > 0 && lineStates.every((l) => l.skip)

  // Require override reason when prescription is expired
  const canDispense = useMemo(() => {
    if (!prescription) return false
    if (allSkipped) return false
    if (prescription.isExpired && !overrideReason.trim()) return false
    return true
  }, [prescription, allSkipped, overrideReason])

  const handleDispense = async () => {
    if (!prescription) return

    const input = {
      prescriptionId: prescription.prescriptionId,
      visitId: prescription.visitId,
      patientId: prescription.patientId,
      patientName: prescription.patientName,
      prescribedAt: prescription.prescribedAt,
      overrideReason: prescription.isExpired ? overrideReason.trim() : null,
      lines: lineStates.map((l) => ({
        prescriptionItemId: l.prescriptionItemId,
        drugCatalogItemId: l.drugCatalogItemId,
        drugName: l.drugName,
        quantity: l.quantity,
        isOffCatalog: l.isOffCatalog,
        skip: l.skip,
        manualBatches: null,
      })),
    }

    try {
      await dispenseMutation.mutateAsync(input)
      toast.success(t("queue.dispensed"))
      onOpenChange(false)
    } catch {
      // onError in mutation handles toast
    }
  }

  if (!prescription) return null

  const prescribedDate = new Date(prescription.prescribedAt).toLocaleDateString("vi-VN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  })

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <IconPackage className="h-5 w-5" />
            {t("queue.dispensingTitle")}
          </DialogTitle>
        </DialogHeader>

        {/* Patient info header */}
        <div className="rounded-lg border bg-muted/30 p-3 space-y-1">
          <div className="flex items-center justify-between gap-2">
            <span className="font-semibold text-base">{prescription.patientName}</span>
            {prescription.prescriptionCode && (
              <span className="font-mono text-sm text-muted-foreground">
                {prescription.prescriptionCode}
              </span>
            )}
          </div>
          <div className="flex items-center gap-4 text-sm text-muted-foreground">
            <span>{t("queue.prescribedAt")}: {prescribedDate}</span>
            {prescription.isExpired ? (
              <Badge variant="destructive" className="text-xs">
                {t("queue.expired")}
              </Badge>
            ) : prescription.daysRemaining <= 2 ? (
              <Badge
                variant="outline"
                className="text-xs border-yellow-500 text-yellow-700 dark:text-yellow-400"
              >
                {t("queue.daysLeft", { days: prescription.daysRemaining })}
              </Badge>
            ) : (
              <span>
                {t("queue.daysLeft", { days: prescription.daysRemaining })}
              </span>
            )}
          </div>
        </div>

        {/* 7-day expiry warning banner */}
        {prescription.isExpired && (
          <Alert variant="destructive">
            <IconAlertTriangle className="h-4 w-4" />
            <AlertTitle>{t("queue.expiredWarningTitle")}</AlertTitle>
            <AlertDescription>
              <p className="mb-3">{t("queue.expiredWarningDesc")}</p>
              <div className="space-y-1">
                <label
                  htmlFor="override-reason"
                  className="text-sm font-medium text-destructive"
                >
                  {t("queue.overrideReasonLabel")} *
                </label>
                <Textarea
                  id="override-reason"
                  value={overrideReason}
                  onChange={(e) => setOverrideReason(e.target.value)}
                  className="bg-background text-foreground border-destructive/50 min-h-[80px] text-sm"
                  maxLength={500}
                />
              </div>
            </AlertDescription>
          </Alert>
        )}

        <Separator />

        {/* Prescription items */}
        <div className="space-y-2">
          <p className="text-sm font-medium text-muted-foreground">
            {t("queue.items")} ({lineStates.length})
          </p>
          {lineStates.map((line) => (
            <LineRow
              key={line.prescriptionItemId}
              line={line}
              onToggleSkip={toggleSkip}
            />
          ))}
        </div>

        {allSkipped && (
          <Alert>
            <AlertDescription>
              {t("queue.allSkippedWarning")}
            </AlertDescription>
          </Alert>
        )}

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={dispenseMutation.isPending}
          >
            {tCommon("buttons.cancel")}
          </Button>
          <Button
            type="button"
            disabled={!canDispense || dispenseMutation.isPending}
            onClick={handleDispense}
          >
            {dispenseMutation.isPending && (
              <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
            )}
            {t("queue.confirmDispense")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
