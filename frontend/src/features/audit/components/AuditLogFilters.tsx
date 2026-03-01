import { useTranslation } from "react-i18next"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { IconFilter, IconFilterOff } from "@tabler/icons-react"
import type { AuditLogFilters as FilterState } from "@/features/audit/hooks/useAuditLogs"

const AUDIT_ACTION_TYPES = [
  "Created",
  "Updated",
  "Deleted",
  "Login",
  "LoginFailed",
  "Logout",
  "ViewRecord",
] as const

interface AuditLogFiltersProps {
  filters: FilterState
  onFiltersChange: (filters: FilterState) => void
  onApply: () => void
  onClear: () => void
}

export function AuditLogFilters({
  filters,
  onFiltersChange,
  onApply,
  onClear,
}: AuditLogFiltersProps) {
  const { t } = useTranslation("audit")

  const updateFilter = (key: keyof FilterState, value: string) => {
    onFiltersChange({ ...filters, [key]: value })
  }

  const hasActiveFilters =
    filters.userId ||
    filters.actionType ||
    filters.dateFrom ||
    filters.dateTo

  return (
    <div className="flex flex-col gap-4 p-4 border bg-card">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {/* User filter */}
        <div className="space-y-2">
          <Label htmlFor="filter-user">{t("filters.user")}</Label>
          <Input
            id="filter-user"
            placeholder={t("filters.user")}
            value={filters.userId}
            onChange={(e) => updateFilter("userId", e.target.value)}
          />
        </div>

        {/* Action type filter */}
        <div className="space-y-2">
          <Label htmlFor="filter-action">{t("filters.actionType")}</Label>
          <Select
            value={filters.actionType}
            onValueChange={(value) => updateFilter("actionType", value)}
          >
            <SelectTrigger id="filter-action">
              <SelectValue placeholder={t("filters.actionType")} />
            </SelectTrigger>
            <SelectContent>
              {AUDIT_ACTION_TYPES.map((action) => (
                <SelectItem key={action} value={action}>
                  {t(`actions.${action.toLowerCase()}`, action)}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Date from */}
        <div className="space-y-2">
          <Label htmlFor="filter-date-from">{t("filters.from")}</Label>
          <Input
            id="filter-date-from"
            type="date"
            value={filters.dateFrom}
            onChange={(e) => updateFilter("dateFrom", e.target.value)}
          />
        </div>

        {/* Date to */}
        <div className="space-y-2">
          <Label htmlFor="filter-date-to">{t("filters.to")}</Label>
          <Input
            id="filter-date-to"
            type="date"
            value={filters.dateTo}
            onChange={(e) => updateFilter("dateTo", e.target.value)}
          />
        </div>
      </div>

      <div className="flex items-center gap-2">
        <Button onClick={onApply} size="sm">
          <IconFilter className="h-4 w-4" />
          {t("filter")}
        </Button>
        {hasActiveFilters && (
          <Button onClick={onClear} variant="outline" size="sm">
            <IconFilterOff className="h-4 w-4" />
            {t("filters.clear")}
          </Button>
        )}
      </div>
    </div>
  )
}
