import { useState } from "react"
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
            <span className="sr-only">Menu hanh dong</span>
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="min-w-[200px]">
          {row.status === "not_arrived" && (
            <>
              <DropdownMenuItem onClick={handleViewProfile}>
                Xem ho so
              </DropdownMenuItem>
              <DropdownMenuItem onClick={handleEditInfo}>
                Sua thong tin
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => setActiveDialog("reschedule")}
                style={{ color: "#534AB7" }}
              >
                Doi lich hen
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => setActiveDialog("no-show")}
                style={{ color: "#BA7517" }}
              >
                Danh dau khong den
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => setActiveDialog("cancel-appointment")}
                style={{ color: "#A32D2D" }}
              >
                Huy hen
              </DropdownMenuItem>
            </>
          )}

          {row.status === "waiting" && (
            <>
              <DropdownMenuItem onClick={handleViewProfile}>
                Xem ho so
              </DropdownMenuItem>
              <DropdownMenuItem onClick={handleEditInfo}>
                Sua thong tin
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => setActiveDialog("cancel-visit")}
                style={{ color: "#A32D2D" }}
              >
                Huy luot kham
              </DropdownMenuItem>
            </>
          )}

          {row.status === "examining" && (
            <DropdownMenuItem onClick={handleViewProfile}>
              Xem ho so
            </DropdownMenuItem>
          )}

          {row.status === "completed" && (
            <DropdownMenuItem onClick={handleViewProfile}>
              Xem ho so
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
