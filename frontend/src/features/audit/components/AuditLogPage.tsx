import { useTranslation } from "react-i18next"
import { Button } from "@/shared/components/Button"
import { IconDownload, IconLoader2 } from "@tabler/icons-react"
import { useAuditLogs } from "@/features/audit/hooks/useAuditLogs"
import { AuditLogFilters } from "./AuditLogFilters"
import { AuditLogTable } from "./AuditLogTable"

export function AuditLogPage() {
  const { t } = useTranslation("audit")
  const {
    data,
    isLoading,
    filters,
    setFilters,
    applyFilters,
    clearFilters,
    hasNextPage,
    hasPreviousPage,
    currentPage,
    goToNextPage,
    goToPreviousPage,
    exportToCsv,
    isExporting,
  } = useAuditLogs()

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("title")}</h1>
        </div>
        <Button
          onClick={exportToCsv}
          disabled={isExporting}
          variant="outline"
        >
          {isExporting ? (
            <IconLoader2 className="h-4 w-4 animate-spin" />
          ) : (
            <IconDownload className="h-4 w-4" />
          )}
          {t("exportCsv", "Export CSV")}
        </Button>
      </div>

      {/* Filters */}
      <AuditLogFilters
        filters={filters}
        onFiltersChange={setFilters}
        onApply={applyFilters}
        onClear={clearFilters}
      />

      {/* Table */}
      <AuditLogTable
        data={data}
        isLoading={isLoading}
        hasNextPage={hasNextPage}
        hasPreviousPage={hasPreviousPage}
        currentPage={currentPage}
        onNextPage={goToNextPage}
        onPreviousPage={goToPreviousPage}
      />
    </div>
  )
}
