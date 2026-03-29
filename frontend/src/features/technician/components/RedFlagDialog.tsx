import { useState } from "react"
import { useTranslation } from "react-i18next"
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogFooter,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogCancel,
} from "@/shared/components/AlertDialog"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { Input } from "@/shared/components/Input"
import { Button } from "@/shared/components/Button"
import { Label } from "@/shared/components/Label"

interface RedFlagDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  patientName: string
  onConfirm: (reason: string) => void
  isLoading: boolean
}

const REASON_KEYS = ["eyePain", "visionLoss", "unilateral", "other"] as const
type ReasonKey = (typeof REASON_KEYS)[number]

export function RedFlagDialog({
  open,
  onOpenChange,
  patientName,
  onConfirm,
  isLoading,
}: RedFlagDialogProps) {
  const { t } = useTranslation("technician")
  const [selectedReason, setSelectedReason] = useState<ReasonKey | "">("")
  const [customReason, setCustomReason] = useState("")
  const [error, setError] = useState("")

  const handleConfirm = () => {
    if (!selectedReason) {
      setError(t("dialog.redFlagReasonRequired"))
      return
    }
    if (selectedReason === "other" && !customReason.trim()) {
      setError(t("dialog.redFlagCustomRequired"))
      return
    }
    setError("")

    const reason =
      selectedReason === "other"
        ? customReason.trim()
        : t(`dialog.redFlagReasons.${selectedReason}`)

    onConfirm(reason)
  }

  const handleOpenChange = (nextOpen: boolean) => {
    if (!nextOpen) {
      setSelectedReason("")
      setCustomReason("")
      setError("")
    }
    onOpenChange(nextOpen)
  }

  return (
    <AlertDialog open={open} onOpenChange={handleOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{t("dialog.redFlagTitle")}</AlertDialogTitle>
          <AlertDialogDescription>
            {patientName}
          </AlertDialogDescription>
        </AlertDialogHeader>

        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>{t("dialog.redFlagReasonLabel")}</Label>
            <Select
              value={selectedReason}
              onValueChange={(val) => {
                setSelectedReason(val as ReasonKey)
                setError("")
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder={t("dialog.redFlagReasonLabel")} />
              </SelectTrigger>
              <SelectContent>
                {REASON_KEYS.map((key) => (
                  <SelectItem key={key} value={key}>
                    {t(`dialog.redFlagReasons.${key}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {selectedReason === "other" && (
            <Input
              value={customReason}
              onChange={(e) => {
                setCustomReason(e.target.value)
                setError("")
              }}
            />
          )}

          {error && (
            <p className="text-sm text-destructive">{error}</p>
          )}
        </div>

        <AlertDialogFooter>
          <AlertDialogCancel>{t("dialog.redFlagCancel")}</AlertDialogCancel>
          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={isLoading}
          >
            {t("dialog.redFlagConfirm")}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
