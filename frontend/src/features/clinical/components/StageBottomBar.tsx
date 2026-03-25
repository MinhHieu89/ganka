import type { ReactNode } from "react"
import { Button } from "@/shared/components/Button"

interface BottomBarButton {
  label: string
  onClick: () => void
  disabled?: boolean
}

interface StageBottomBarProps {
  saveDraftButton?: BottomBarButton
  secondaryButton?: BottomBarButton & { className?: string }
  primaryButton?: BottomBarButton
  validationMessage?: ReactNode
}

export function StageBottomBar({
  saveDraftButton,
  secondaryButton,
  primaryButton,
  validationMessage,
}: StageBottomBarProps) {
  return (
    <div className="p-4">
      {validationMessage && <div className="mb-3">{validationMessage}</div>}
      <div className="flex items-center justify-between">
        <div>
          {saveDraftButton && (
            <Button
              variant="outline"
              onClick={saveDraftButton.onClick}
              disabled={saveDraftButton.disabled}
            >
              {saveDraftButton.label}
            </Button>
          )}
        </div>
        <div className="flex items-center gap-3">
          {secondaryButton && (
            <Button
              variant="outline"
              className={secondaryButton.className}
              onClick={secondaryButton.onClick}
              disabled={secondaryButton.disabled}
            >
              {secondaryButton.label}
            </Button>
          )}
          {primaryButton && (
            <Button
              onClick={primaryButton.onClick}
              disabled={primaryButton.disabled}
            >
              {primaryButton.label}
            </Button>
          )}
        </div>
      </div>
    </div>
  )
}
