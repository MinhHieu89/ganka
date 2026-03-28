import { useState } from "react"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import { IconAlertTriangle } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Label } from "@/shared/components/Label"
import { Textarea } from "@/shared/components/Textarea"
import { Checkbox } from "@/shared/components/Checkbox"
import { Avatar, AvatarFallback } from "@/shared/components/Avatar"
import { useMarkNoShowMutation } from "@/features/receptionist/api/receptionist-api"
import type { ReceptionistDashboardRow } from "@/features/receptionist/types/receptionist.types"

interface NoShowDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  row: ReceptionistDashboardRow
}

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/)
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

export function NoShowDialog({ open, onOpenChange, row }: NoShowDialogProps) {
  const { t } = useTranslation("receptionist")
  const { t: tCommon } = useTranslation("common")
  const [note, setNote] = useState("")
  const [rebook, setRebook] = useState(false)
  const navigate = useNavigate()
  const markNoShow = useMarkNoShowMutation()

  const handleConfirm = () => {
    if (!row.appointmentId) return

    markNoShow.mutate(row.appointmentId, {
      onSuccess: () => {
        toast.success(t("noShow.successToast", { name: row.patientName }))
        handleClose()

        if (rebook && row.patientId) {
          navigate({
            to: `/appointments/new?patientId=${row.patientId}` as string,
          })
        }
      },
      onError: () => {
        toast.error(t("noShow.errorToast"))
      },
    })
  }

  const handleClose = () => {
    setNote("")
    setRebook(false)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold">
            {t("noShow.title")}
          </DialogTitle>
          <DialogDescription className="sr-only">
            {t("noShow.description")}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Patient avatar with no-show styling (amber) */}
          <div className="flex items-center gap-3">
            <Avatar className="h-12 w-12">
              <AvatarFallback
                className="text-sm font-semibold"
                style={{
                  backgroundColor: "#FAEEDA",
                  color: "#BA7517",
                }}
              >
                {getInitials(row.patientName)}
              </AvatarFallback>
            </Avatar>
            <div>
              <div className="text-sm font-semibold">{row.patientName}</div>
              {row.patientCode && (
                <div className="font-mono text-xs text-muted-foreground">
                  {row.patientCode}
                </div>
              )}
            </div>
          </div>

          {/* Amber warning */}
          <div
            className="flex items-start gap-2 rounded-md px-3 py-2.5 text-sm"
            style={{
              backgroundColor: "#FAEEDA",
              color: "#633806",
            }}
          >
            <IconAlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
            <span>
              {t("noShow.warning")}
            </span>
          </div>

          {/* Note textarea */}
          <div className="space-y-2">
            <Label>{t("noShow.notes")}</Label>
            <Textarea
              value={note}
              onChange={(e) => setNote(e.target.value)}
              placeholder={t("noShow.notesPlaceholder")}
              rows={2}
              className="resize-none"
            />
          </div>

          {/* Rebook checkbox */}
          <div className="flex items-center gap-2">
            <Checkbox
              id="rebook-noshow"
              checked={rebook}
              onCheckedChange={(checked) => setRebook(checked === true)}
            />
            <Label htmlFor="rebook-noshow" className="cursor-pointer">
              {t("noShow.rebook")}
            </Label>
          </div>
        </div>

        <DialogFooter className="gap-2 sm:justify-end">
          <Button variant="ghost" onClick={handleClose}>
            {tCommon("buttons.cancel")}
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={markNoShow.isPending}
            style={{ backgroundColor: "#BA7517", color: "white" }}
            className="hover:opacity-90"
          >
            {markNoShow.isPending ? tCommon("status.processing") : t("noShow.confirm")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
