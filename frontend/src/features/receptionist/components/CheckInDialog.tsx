import { toast } from "sonner"
import { useTranslation } from "react-i18next"
import { IconUser } from "@tabler/icons-react"
import { useNavigate } from "@tanstack/react-router"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Avatar, AvatarFallback } from "@/shared/components/Avatar"
import { useCheckInMutation } from "@/features/receptionist/api/receptionist-api"
import type { ReceptionistDashboardRow } from "@/features/receptionist/types/receptionist.types"

interface CheckInDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  row: ReceptionistDashboardRow
}

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/)
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

export function CheckInDialog({ open, onOpenChange, row }: CheckInDialogProps) {
  const { t } = useTranslation("scheduling")
  const { t: tCommon } = useTranslation("common")
  const { t: tPatient } = useTranslation("patient")
  const checkIn = useCheckInMutation()
  const navigate = useNavigate()

  const handleConfirm = () => {
    if (!row.appointmentId) return

    checkIn.mutate(row.appointmentId, {
      onSuccess: () => {
        toast.success(t("checkIn.successToast", { name: row.patientName }))
        onOpenChange(false)
      },
      onError: () => {
        toast.error(t("checkIn.alreadyExamining"))
      },
    })
  }

  const handleEditInfo = () => {
    onOpenChange(false)
    navigate({ to: `/patients/${row.patientId}/edit` as string })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold">
            {t("checkIn.title")}
          </DialogTitle>
          <DialogDescription className="sr-only">
            {t("checkIn.description")}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Patient avatar + name */}
          <div className="flex items-center gap-3">
            <Avatar className="h-12 w-12">
              <AvatarFallback
                className="text-sm font-semibold"
                style={{
                  backgroundColor: "var(--avatar-complete-bg)",
                  color: "var(--avatar-complete-text)",
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

          {/* Patient info card */}
          <div className="rounded-md border bg-muted/50 p-4 space-y-2 text-sm">
            <div className="flex justify-between">
              <span className="text-muted-foreground">{tPatient("fullName")}</span>
              <span className="font-medium">{row.patientName}</span>
            </div>
            {row.patientCode && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">{tPatient("patientCode")}</span>
                <span className="font-mono">{row.patientCode}</span>
              </div>
            )}
            {row.birthYear && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">{tPatient("birthYear")}</span>
                <span>{row.birthYear}</span>
              </div>
            )}
          </div>

          {/* Blue info note */}
          <div
            className="flex items-start gap-2 rounded-md px-3 py-2.5 text-sm"
            style={{
              backgroundColor: "#E6F1FB",
              color: "#0C447C",
            }}
          >
            <IconUser className="mt-0.5 h-4 w-4 shrink-0" />
            <span>
              {t("checkIn.infoNote")}
            </span>
          </div>
        </div>

        <DialogFooter className="flex-col gap-2 sm:flex-row sm:justify-between">
          <Button variant="outline" onClick={handleEditInfo}>
            {tCommon("buttons.edit")}
          </Button>
          <div className="flex gap-2">
            <Button variant="ghost" onClick={() => onOpenChange(false)}>
              {tCommon("buttons.back")}
            </Button>
            <Button
              onClick={handleConfirm}
              disabled={checkIn.isPending}
              style={{ backgroundColor: "#534AB7", color: "white" }}
              className="hover:opacity-90"
            >
              {checkIn.isPending ? tCommon("status.processing") : t("checkIn.confirm")}
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
