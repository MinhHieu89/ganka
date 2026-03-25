import { useTranslation } from "react-i18next"
import { IconStethoscope, IconPlus } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { ViewToggle, type ViewMode } from "./ViewToggle"

interface WorkflowToolbarProps {
  totalPatients: number
  viewMode: ViewMode
  onViewModeChange: (mode: ViewMode) => void
  onNewVisit: () => void
}

export function WorkflowToolbar({
  totalPatients,
  viewMode,
  onViewModeChange,
  onNewVisit,
}: WorkflowToolbarProps) {
  const { t } = useTranslation("clinical")

  return (
    <div className="flex items-center justify-between mb-4">
      <div className="flex items-center gap-3">
        <IconStethoscope className="h-6 w-6 text-primary" />
        <h1 className="text-xl font-semibold tracking-tight">
          {t("workflow.title")}
        </h1>
        <Badge variant="secondary">{totalPatients}</Badge>
      </div>
      <div className="flex items-center gap-3">
        <ViewToggle value={viewMode} onChange={onViewModeChange} />
        <Button onClick={onNewVisit}>
          <IconPlus className="mr-2 h-4 w-4" />
          {t("newVisit.title")}
        </Button>
      </div>
    </div>
  )
}
