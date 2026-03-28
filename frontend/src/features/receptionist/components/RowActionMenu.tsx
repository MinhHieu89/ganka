import { useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import { IconDotsVertical } from "@tabler/icons-react"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/DropdownMenu"
import { Button } from "@/shared/components/Button"
import type { ReceptionistDashboardRow } from "@/features/receptionist/types/receptionist.types"
import { RescheduleDialog } from "./RescheduleDialog"
import { CancelAppointmentDialog } from "./CancelAppointmentDialog"
import { NoShowDialog } from "./NoShowDialog"
import { CancelVisitDialog } from "./CancelVisitDialog"

interface RowActionMenuProps {
  row: ReceptionistDashboardRow
  onCheckIn: () => void
}

type DialogType = "reschedule" | "cancel-appointment" | "no-show" | "cancel-visit" | null

export function RowActionMenu({ row, onCheckIn }: RowActionMenuProps) {
  const { t } = useTranslation("receptionist")
  const navigate = useNavigate()
  const [activeDialog, setActiveDialog] = useState<DialogType>(null)

  const handleViewProfile = () => {
    if (row.patientId) {
      navigate({ to: `/patients/${row.patientId}` as string })
    }
  }

  const handleEditInfo = () => {
    if (row.patientId) {
      navigate({ to: `/patients/${row.patientId}/edit` as string })
    }
  }

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={(e) => e.stopPropagation()}
          >
            <IconDotsVertical className="h-4 w-4" />
            <span className="sr-only">{t("actionMenu.label")}</span>
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="min-w-[200px]">
          {row.status === "not_arrived" && (
            <>
              <DropdownMenuItem onClick={handleViewProfile}>
                {t("actionMenu.viewRecord")}
              </DropdownMenuItem>
              <DropdownMenuItem onClick={handleEditInfo}>
                {t("actionMenu.editInfo")}
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => setActiveDialog("reschedule")}
                style={{ color: "#534AB7" }}
              >
                {t("actionMenu.reschedule")}
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => setActiveDialog("no-show")}
                style={{ color: "#BA7517" }}
              >
                {t("actionMenu.markNoShow")}
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => setActiveDialog("cancel-appointment")}
                style={{ color: "#A32D2D" }}
              >
                {t("actionMenu.cancelAppointment")}
              </DropdownMenuItem>
            </>
          )}

          {row.status === "waiting" && (
            <>
              <DropdownMenuItem onClick={handleViewProfile}>
                {t("actionMenu.viewRecord")}
              </DropdownMenuItem>
              <DropdownMenuItem onClick={handleEditInfo}>
                {t("actionMenu.editInfo")}
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => setActiveDialog("cancel-visit")}
                style={{ color: "#A32D2D" }}
              >
                {t("actionMenu.cancelVisit")}
              </DropdownMenuItem>
            </>
          )}

          {row.status === "examining" && (
            <DropdownMenuItem onClick={handleViewProfile}>
              {t("actionMenu.viewRecord")}
            </DropdownMenuItem>
          )}

          {row.status === "completed" && (
            <DropdownMenuItem onClick={handleViewProfile}>
              {t("actionMenu.viewRecord")}
            </DropdownMenuItem>
          )}
        </DropdownMenuContent>
      </DropdownMenu>

      {/* Dialogs */}
      {activeDialog === "reschedule" && (
        <RescheduleDialog
          open
          onOpenChange={(open) => !open && setActiveDialog(null)}
          row={row}
        />
      )}
      {activeDialog === "cancel-appointment" && (
        <CancelAppointmentDialog
          open
          onOpenChange={(open) => !open && setActiveDialog(null)}
          row={row}
        />
      )}
      {activeDialog === "no-show" && (
        <NoShowDialog
          open
          onOpenChange={(open) => !open && setActiveDialog(null)}
          row={row}
        />
      )}
      {activeDialog === "cancel-visit" && (
        <CancelVisitDialog
          open
          onOpenChange={(open) => !open && setActiveDialog(null)}
          row={row}
        />
      )}
    </>
  )
}
