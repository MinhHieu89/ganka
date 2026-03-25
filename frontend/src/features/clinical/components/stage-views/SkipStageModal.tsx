import { useState } from "react"
import { Button } from "@/shared/components/Button"
import { Textarea } from "@/shared/components/Textarea"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/shared/components/Dialog"
import { cn } from "@/shared/lib/utils"

interface SkipReasonChip {
  value: number
  label: string
}

interface SkipStageModalProps {
  open: boolean
  onClose: () => void
  onConfirm: (reason: number, freeTextNote?: string) => void
  title: string
  reasonChips: SkipReasonChip[]
  isSubmitting?: boolean
}

const MAX_NOTE_LENGTH = 200

// Vietnamese labels
const LABELS = {
  description: "Ch\u1ECDn l\u00FD do b\u1ECF qua b\u01B0\u1EDBc n\u00E0y.",
  cancel: "H\u1EE7y",
  confirm: "X\u00E1c nh\u1EADn b\u1ECF qua",
  submitting: "\u0110ang x\u1EED l\u00FD...",
}

export function SkipStageModal({
  open,
  onClose,
  onConfirm,
  title,
  reasonChips,
  isSubmitting,
}: SkipStageModalProps) {
  const [selectedReason, setSelectedReason] = useState<number | null>(null)
  const [freeTextNote, setFreeTextNote] = useState("")

  function handleConfirm() {
    if (selectedReason === null) return
    onConfirm(selectedReason, freeTextNote.trim() || undefined)
  }

  function handleClose() {
    setSelectedReason(null)
    setFreeTextNote("")
    onClose()
  }

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && handleClose()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{LABELS.description}</DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Reason chips - single-select, mandatory */}
          <div className="flex flex-wrap gap-2">
            {reasonChips.map((chip) => (
              <button
                key={chip.value}
                type="button"
                onClick={() => setSelectedReason(chip.value)}
                className={cn(
                  "px-3 py-1.5 text-sm border transition-colors",
                  selectedReason === chip.value
                    ? "border-primary bg-primary text-primary-foreground"
                    : "border-border bg-background hover:bg-muted",
                )}
              >
                {chip.label}
              </button>
            ))}
          </div>

          {/* Optional free-text note with character counter */}
          <div>
            <Textarea
              value={freeTextNote}
              onChange={(e) => {
                if (e.target.value.length <= MAX_NOTE_LENGTH) {
                  setFreeTextNote(e.target.value)
                }
              }}
              rows={3}
              className="resize-none"
            />
            <div className="flex justify-end mt-1">
              <span className="text-xs text-muted-foreground">
                {freeTextNote.length}/{MAX_NOTE_LENGTH}
              </span>
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose} disabled={isSubmitting}>
            {LABELS.cancel}
          </Button>
          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={selectedReason === null || isSubmitting}
          >
            {isSubmitting ? LABELS.submitting : LABELS.confirm}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
