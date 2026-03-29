import { useTranslation } from "react-i18next"
import {
  IconDotsVertical,
  IconCircleCheck,
  IconEye,
  IconPlayerPlay,

  IconArrowBackUp,
  IconAlertTriangle,
} from "@tabler/icons-react"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/DropdownMenu"
import { Button } from "@/shared/components/Button"
import type { TechnicianDashboardRow } from "@/features/technician/types/technician.types"

interface TechnicianActionMenuProps {
  row: TechnicianDashboardRow
  onAction: (action: string, row: TechnicianDashboardRow) => void
}

export function TechnicianActionMenu({
  row,
  onAction,
}: TechnicianActionMenuProps) {
  const { t } = useTranslation("technician")

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8"
          onClick={(e) => e.stopPropagation()}
        >
          <IconDotsVertical className="h-4 w-4" />
          <span className="sr-only">{t("table.actions")}</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="min-w-[200px]">
        {row.status === "waiting" && (
          <DropdownMenuItem onClick={() => onAction("accept", row)}>
            <IconPlayerPlay className="h-4 w-4 mr-2" />
            {t("actions.accept")}
          </DropdownMenuItem>
        )}

        {row.status === "in_progress" && (
          <>
            <DropdownMenuItem onClick={() => onAction("continue", row)}>
              <IconPlayerPlay className="h-4 w-4 mr-2" />
              {t("actions.continue")}
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => onAction("complete", row)}>
              <IconCircleCheck className="h-4 w-4 mr-2" />
              {t("actions.complete")}
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => onAction("returnToQueue", row)}>
              <IconArrowBackUp className="h-4 w-4 mr-2" />
              {t("actions.returnToQueue")}
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              onClick={() => onAction("redFlag", row)}
              style={{ color: "var(--tech-action-destructive)" }}
            >
              <IconAlertTriangle className="h-4 w-4 mr-2" />
              {t("actions.redFlag")}
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={() => onAction("viewResults", row)}>
              <IconEye className="h-4 w-4 mr-2" />
              {t("actions.viewResults")}
            </DropdownMenuItem>
          </>
        )}

        {row.status === "completed" && (
          <DropdownMenuItem onClick={() => onAction("viewResults", row)}>
            <IconEye className="h-4 w-4 mr-2" />
            {t("actions.viewResults")}
          </DropdownMenuItem>
        )}

        {row.status === "red_flag" && (
          <DropdownMenuItem onClick={() => onAction("viewResults", row)}>
            <IconEye className="h-4 w-4 mr-2" />
            {t("actions.viewResults")}
          </DropdownMenuItem>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
