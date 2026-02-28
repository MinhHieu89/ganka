import { useState, useCallback } from "react"
import {
  useAuditLogsQuery,
  useExportAuditLogsMutation,
  type AuditLogQuery,
} from "@/features/audit/api/audit-api"

export interface AuditLogFilters {
  userId: string
  actionType: string
  entityName: string
  dateFrom: string
  dateTo: string
}

const DEFAULT_PAGE_SIZE = 50

const emptyFilters: AuditLogFilters = {
  userId: "",
  actionType: "",
  entityName: "",
  dateFrom: "",
  dateTo: "",
}

export function useAuditLogs() {
  const [filters, setFilters] = useState<AuditLogFilters>(emptyFilters)
  const [appliedFilters, setAppliedFilters] = useState<AuditLogFilters>(emptyFilters)
  const [cursorStack, setCursorStack] = useState<
    Array<{ timestamp: string; id: string }>
  >([])
  const [currentCursor, setCurrentCursor] = useState<
    { timestamp: string; id: string } | undefined
  >(undefined)

  const queryParams: AuditLogQuery = {
    userId: appliedFilters.userId || undefined,
    actionType: appliedFilters.actionType || undefined,
    entityName: appliedFilters.entityName || undefined,
    dateFrom: appliedFilters.dateFrom || undefined,
    dateTo: appliedFilters.dateTo || undefined,
    cursorTimestamp: currentCursor?.timestamp,
    cursorId: currentCursor?.id,
    pageSize: DEFAULT_PAGE_SIZE,
  }

  const query = useAuditLogsQuery(queryParams)
  const exportMutation = useExportAuditLogsMutation()

  const applyFilters = useCallback(() => {
    setAppliedFilters({ ...filters })
    setCursorStack([])
    setCurrentCursor(undefined)
  }, [filters])

  const clearFilters = useCallback(() => {
    setFilters(emptyFilters)
    setAppliedFilters(emptyFilters)
    setCursorStack([])
    setCurrentCursor(undefined)
  }, [])

  const goToNextPage = useCallback(() => {
    if (query.data?.nextCursor) {
      if (currentCursor) {
        setCursorStack((prev) => [...prev, currentCursor])
      }
      setCurrentCursor(query.data.nextCursor)
    }
  }, [query.data?.nextCursor, currentCursor])

  const goToPreviousPage = useCallback(() => {
    setCursorStack((prev) => {
      const newStack = [...prev]
      const prevCursor = newStack.pop()
      setCurrentCursor(prevCursor)
      return newStack
    })
  }, [])

  const exportToCsv = useCallback(() => {
    exportMutation.mutate({
      userId: appliedFilters.userId || undefined,
      actionType: appliedFilters.actionType || undefined,
      entityName: appliedFilters.entityName || undefined,
      dateFrom: appliedFilters.dateFrom || undefined,
      dateTo: appliedFilters.dateTo || undefined,
    })
  }, [exportMutation, appliedFilters])

  const hasNextPage = !!query.data?.nextCursor
  const hasPreviousPage = cursorStack.length > 0 || !!currentCursor
  const currentPage = cursorStack.length + (currentCursor ? 1 : 0) + 1

  return {
    // Data
    data: query.data?.data ?? [],
    isLoading: query.isLoading,
    isError: query.isError,
    error: query.error,

    // Filters
    filters,
    setFilters,
    applyFilters,
    clearFilters,

    // Pagination
    hasNextPage,
    hasPreviousPage,
    currentPage,
    goToNextPage,
    goToPreviousPage,

    // Export
    exportToCsv,
    isExporting: exportMutation.isPending,
  }
}
