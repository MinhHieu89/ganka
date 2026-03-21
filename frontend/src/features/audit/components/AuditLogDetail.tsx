import { useTranslation } from "react-i18next"
import { Button } from "@/shared/components/Button"
import { IconCopy, IconCheck } from "@tabler/icons-react"
import { useState, useCallback } from "react"
import type { AuditLogDto } from "@/features/audit/api/audit-api"

interface AuditLogDetailProps {
  log: AuditLogDto
}

export function AuditLogDetail({ log }: AuditLogDetailProps) {
  const { t } = useTranslation("audit")
  const [copied, setCopied] = useState(false)

  const handleCopy = useCallback(async () => {
    const record = {
      id: log.id,
      timestamp: log.timestamp,
      user: log.userEmail,
      entity: log.entityName,
      entityId: log.entityId,
      action: log.action,
      changes: log.changes,
    }
    try {
      await navigator.clipboard.writeText(JSON.stringify(record, null, 2))
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch {
      // Fallback for environments without clipboard API
    }
  }, [log])

  const formatTimestamp = (ts: string) => {
    return new Date(ts).toLocaleString(undefined, {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
    })
  }

  return (
    <div className="p-4 bg-muted/30 border-t space-y-4">
      {/* Header info */}
      <div className="flex items-start justify-between">
        <div className="grid grid-cols-2 gap-x-8 gap-y-2 text-sm">
          <div>
            <span className="font-medium text-muted-foreground">
              {t("timestamp")}:
            </span>{" "}
            {formatTimestamp(log.timestamp)}
          </div>
          <div>
            <span className="font-medium text-muted-foreground">
              {t("actor")}:
            </span>{" "}
            {log.userEmail}
          </div>
          <div>
            <span className="font-medium text-muted-foreground">
              {t("entity")}:
            </span>{" "}
            {log.entityName}
          </div>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={handleCopy}
          className="shrink-0"
        >
          {copied ? (
            <IconCheck className="h-4 w-4 text-green-600" />
          ) : (
            <IconCopy className="h-4 w-4" />
          )}
          {copied ? t("copied", "Copied") : t("copyRecord", "Copy")}
        </Button>
      </div>

      {/* Changes table - filter out ID/GUID fields */}
      {log.changes.filter(
        (c) =>
          !c.propertyName.endsWith("Id") &&
          c.propertyName !== "Id"
      ).length > 0 ? (
        <div className="border bg-background rounded-lg overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="px-3 py-2 text-left font-medium text-muted-foreground">
                  {t("field", "Field")}
                </th>
                <th className="px-3 py-2 text-left font-medium text-muted-foreground">
                  {t("oldValue", "Old Value")}
                </th>
                <th className="px-3 py-2 text-left font-medium text-muted-foreground">
                  {t("newValue", "New Value")}
                </th>
              </tr>
            </thead>
            <tbody>
              {log.changes.filter(
                (c) =>
                  !c.propertyName.endsWith("Id") &&
                  c.propertyName !== "Id"
              ).map((change, idx) => (
                <tr key={idx} className="border-b last:border-0">
                  <td className="px-3 py-2 font-mono text-xs font-medium">
                    {change.propertyName}
                  </td>
                  <td className="px-3 py-2">
                    {change.oldValue != null ? (
                      <span className="line-through text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950/30 px-1.5 py-0.5 text-xs font-mono">
                        {change.oldValue}
                      </span>
                    ) : (
                      <span className="text-muted-foreground text-xs italic">
                        --
                      </span>
                    )}
                  </td>
                  <td className="px-3 py-2">
                    {change.newValue != null ? (
                      <span className="text-green-600 dark:text-green-400 bg-green-50 dark:bg-green-950/30 px-1.5 py-0.5 text-xs font-mono">
                        {change.newValue}
                      </span>
                    ) : (
                      <span className="text-muted-foreground text-xs italic">
                        --
                      </span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <p className="text-sm text-muted-foreground italic">
          {t("noChanges", "No field-level changes recorded")}
        </p>
      )}
    </div>
  )
}
