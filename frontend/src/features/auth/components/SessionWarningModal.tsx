import { useTranslation } from "react-i18next"
import { IconClock, IconLogout } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"

interface SessionWarningModalProps {
  open: boolean
  remainingSeconds: number
  onExtend: () => void
  onLogout: () => void
}

export function SessionWarningModal({
  open,
  remainingSeconds,
  onExtend,
  onLogout,
}: SessionWarningModalProps) {
  const { t } = useTranslation("auth")

  const minutes = Math.floor(remainingSeconds / 60)
  const seconds = remainingSeconds % 60

  const timeDisplay =
    minutes > 0
      ? `${minutes}:${seconds.toString().padStart(2, "0")}`
      : `0:${seconds.toString().padStart(2, "0")}`

  return (
    <Dialog open={open} onOpenChange={() => {}}>
      <DialogContent
        className="sm:max-w-md"
        onPointerDownOutside={(e) => e.preventDefault()}
        onEscapeKeyDown={(e) => e.preventDefault()}
        hideCloseButton
      >
        <DialogHeader>
          <div className="flex items-center justify-center mb-2">
            <div className="flex items-center justify-center w-12 h-12 bg-destructive/10 text-destructive">
              <IconClock className="h-6 w-6" />
            </div>
          </div>
          <DialogTitle className="text-center">
            {t("session.expiring")}
          </DialogTitle>
          <DialogDescription className="text-center">
            {t("session.expiringMessage", { minutes: timeDisplay })}
          </DialogDescription>
        </DialogHeader>

        <div className="flex items-center justify-center py-4">
          <div className="text-4xl font-mono font-bold tabular-nums text-destructive">
            {timeDisplay}
          </div>
        </div>

        <DialogFooter className="flex-row gap-2 sm:justify-center">
          <Button variant="outline" onClick={onLogout} className="flex-1">
            <IconLogout className="h-4 w-4 mr-2" />
            {t("session.logout")}
          </Button>
          <Button onClick={onExtend} className="flex-1">
            {t("session.extend")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
