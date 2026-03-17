import { useEffect, useRef, useState } from "react"
import { useTranslation } from "react-i18next"
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
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"

interface ApprovalPinDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onApprove: (pin: string) => void
  title: string
  description: string
  isPending?: boolean
  error?: string | null
}

export function ApprovalPinDialog({
  open,
  onOpenChange,
  onApprove,
  title,
  description,
  isPending = false,
  error = null,
}: ApprovalPinDialogProps) {
  const { t } = useTranslation("billing")
  const { t: tCommon } = useTranslation("common")
  const [pin, setPin] = useState("")
  const [localError, setLocalError] = useState<string | null>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  // Reset state when dialog opens/closes
  useEffect(() => {
    if (open) {
      setPin("")
      setLocalError(null)
      // Auto-focus the PIN input after dialog animation
      setTimeout(() => inputRef.current?.focus(), 100)
    }
  }, [open])

  // Sync external error
  useEffect(() => {
    if (error) {
      setLocalError(error)
    }
  }, [error])

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setLocalError(null)

    if (!pin || pin.length < 4 || pin.length > 6) {
      setLocalError(t("pinLength"))
      return
    }

    if (!/^\d+$/.test(pin)) {
      setLocalError(t("pinDigitsOnly"))
      return
    }

    onApprove(pin)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <Field data-invalid={localError ? true : undefined}>
            <FieldLabel htmlFor="approval-pin">{t("managerPin")}</FieldLabel>
            <Input
              ref={inputRef}
              id="approval-pin"
              type="password"
              inputMode="numeric"
              maxLength={6}
              value={pin}
              onChange={(e) => {
                // Only allow numeric input
                const val = e.target.value.replace(/\D/g, "")
                setPin(val)
                if (localError) setLocalError(null)
              }}
              aria-invalid={localError ? true : undefined}
              autoComplete="off"
            />
            {localError && <FieldError>{localError}</FieldError>}
          </Field>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isPending}
            >
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={isPending || !pin}>
              {isPending && (
                <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              {t("confirm")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
