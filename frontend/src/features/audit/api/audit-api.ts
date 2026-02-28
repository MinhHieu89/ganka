import { useQuery, useMutation } from "@tanstack/react-query"
import { useAuthStore } from "@/shared/stores/authStore"

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000"

export interface AuditLogQuery {
  userId?: string
  actionType?: string
  entityName?: string
  dateFrom?: string
  dateTo?: string
  cursorTimestamp?: string
  cursorId?: string
  pageSize?: number
}

export interface AuditChangeDto {
  propertyName: string
  oldValue: string | null
  newValue: string | null
}

export interface AuditLogDto {
  id: string
  timestamp: string
  userEmail: string
  entityName: string
  entityId: string
  action: string
  changes: AuditChangeDto[]
}

export interface AuditLogResponse {
  data: AuditLogDto[]
  nextCursor: { timestamp: string; id: string } | null
  pageSize: number
}

function getAuthHeaders(): Record<string, string> {
  const token = useAuthStore.getState().accessToken
  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function fetchAuditLogs(params: AuditLogQuery): Promise<AuditLogResponse> {
  const searchParams = new URLSearchParams()
  if (params.userId) searchParams.set("userId", params.userId)
  if (params.actionType) searchParams.set("actionType", params.actionType)
  if (params.entityName) searchParams.set("entityName", params.entityName)
  if (params.dateFrom) searchParams.set("dateFrom", params.dateFrom)
  if (params.dateTo) searchParams.set("dateTo", params.dateTo)
  if (params.cursorTimestamp) searchParams.set("cursorTimestamp", params.cursorTimestamp)
  if (params.cursorId) searchParams.set("cursorId", params.cursorId)
  if (params.pageSize) searchParams.set("pageSize", String(params.pageSize))

  const response = await fetch(
    `${API_URL}/api/admin/audit-logs?${searchParams.toString()}`,
    { headers: { ...getAuthHeaders() } }
  )

  if (!response.ok) {
    throw new Error(`Failed to fetch audit logs: ${response.statusText}`)
  }

  return response.json()
}

async function exportAuditLogs(params: Omit<AuditLogQuery, "cursorTimestamp" | "cursorId" | "pageSize">): Promise<Blob> {
  const searchParams = new URLSearchParams()
  if (params.userId) searchParams.set("userId", params.userId)
  if (params.actionType) searchParams.set("actionType", params.actionType)
  if (params.entityName) searchParams.set("entityName", params.entityName)
  if (params.dateFrom) searchParams.set("dateFrom", params.dateFrom)
  if (params.dateTo) searchParams.set("dateTo", params.dateTo)

  const response = await fetch(
    `${API_URL}/api/admin/audit-logs/export?${searchParams.toString()}`,
    { headers: { ...getAuthHeaders() } }
  )

  if (!response.ok) {
    throw new Error(`Failed to export audit logs: ${response.statusText}`)
  }

  return response.blob()
}

export function useAuditLogsQuery(params: AuditLogQuery) {
  return useQuery({
    queryKey: ["audit-logs", params],
    queryFn: () => fetchAuditLogs(params),
    placeholderData: (previousData) => previousData,
  })
}

export function useExportAuditLogsMutation() {
  return useMutation({
    mutationFn: exportAuditLogs,
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const a = document.createElement("a")
      a.href = url
      a.download = `audit-logs-${new Date().toISOString().slice(0, 10)}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
    },
  })
}
