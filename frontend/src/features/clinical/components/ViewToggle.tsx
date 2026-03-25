import { ToggleGroup, ToggleGroupItem } from "@/shared/components/ToggleGroup"
import { IconLayoutKanban, IconTable } from "@tabler/icons-react"

export type ViewMode = "kanban" | "table"

const STORAGE_KEY = "ganka:workflow-view-mode"

export function getStoredViewMode(): ViewMode {
  const stored = localStorage.getItem(STORAGE_KEY)
  return stored === "table" ? "table" : "kanban"
}

export function storeViewMode(mode: ViewMode) {
  localStorage.setItem(STORAGE_KEY, mode)
}

interface ViewToggleProps {
  value: ViewMode
  onChange: (mode: ViewMode) => void
}

export function ViewToggle({ value, onChange }: ViewToggleProps) {
  return (
    <ToggleGroup
      type="single"
      value={value}
      onValueChange={(v) => {
        if (v) onChange(v as ViewMode)
      }}
      variant="outline"
      size="sm"
    >
      <ToggleGroupItem value="kanban" aria-label="Board view">
        <IconLayoutKanban className="h-4 w-4" />
      </ToggleGroupItem>
      <ToggleGroupItem value="table" aria-label="Table view">
        <IconTable className="h-4 w-4" />
      </ToggleGroupItem>
    </ToggleGroup>
  )
}
