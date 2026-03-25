import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Textarea } from "@/shared/components/Textarea"
import { Label } from "@/shared/components/Label"
import { useTranslation } from "react-i18next"
import { useState, useEffect } from "react"
import { toast } from "sonner"
import { useReverseStage } from "../api/clinical-api"

interface ReversalInfo {
  visitId: string
  currentStage: number
  targetStage: number
}

interface StageReversalDialogProps {
  reversalInfo: ReversalInfo | null
  onClose: () => void
  stageLabels: Record<number, string>
}

export function StageReversalDialog({
  reversalInfo,
  onClose,
  stageLabels,
}: StageReversalDialogProps) {
  const { t } = useTranslation("clinical")
  const [reason, setReason] = useState("")
  const reverseMutation = useReverseStage()

  useEffect(() => {
    if (reversalInfo) setReason("")
  }, [reversalInfo])

  const handleConfirm = () => {
    if (!reversalInfo || reason.trim().length < 10) return
    reverseMutation.mutate(
      {
        visitId: reversalInfo.visitId,
        targetStage: reversalInfo.targetStage,
        reason: reason.trim(),
      },
      {
        onSuccess: () => {
          toast.success(
            t("workflow.reversal.success", {
              stage: stageLabels[reversalInfo.targetStage],
            }),
          )
          onClose()
        },
        onError: () => {
          toast.error(t("workflow.reversal.error"))
        },
      },
    )
  }

  return (
    <Dialog
      open={!!reversalInfo}
      onOpenChange={(open) => {
        if (!open) onClose()
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("workflow.reversal.title")}</DialogTitle>
          <DialogDescription>
            {stageLabels[reversalInfo?.currentStage ?? 0]} {"\u2192"}{" "}
            {stageLabels[reversalInfo?.targetStage ?? 0]}
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-2">
          <Label htmlFor="reversal-reason">
            {t("workflow.reversal.reasonLabel")}
          </Label>
          <Textarea
            id="reversal-reason"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder={t("workflow.reversal.reasonPlaceholder")}
            autoFocus
            minLength={10}
          />
          {reason.length > 0 && reason.trim().length < 10 && (
            <p className="text-xs text-destructive">
              {t("workflow.reversal.minLength", { min: 10 })}
            </p>
          )}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            {t("workflow.reversal.cancel")}
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={reason.trim().length < 10 || reverseMutation.isPending}
          >
            {reverseMutation.isPending
              ? "..."
              : t("workflow.reversal.confirm")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
