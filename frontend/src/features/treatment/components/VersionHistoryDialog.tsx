import { format } from "date-fns"
import { useTranslation } from "react-i18next"
import { IconHistory, IconLoader2 } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Badge } from "@/shared/components/Badge"
import { Card, CardContent } from "@/shared/components/Card"
import { usePackageVersions } from "@/features/treatment/api/treatment-api"

// -- Props --

interface VersionHistoryDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  packageId: string
  packageName?: string
}

// -- Helpers --

function formatJson(value: string): string {
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

function formatValue(val: unknown): string {
  if (val === null || val === undefined) return "-"
  if (typeof val === "object") return JSON.stringify(val)
  return String(val)
}

function toCamelCase(str: string): string {
  return str.charAt(0).toLowerCase() + str.slice(1)
}

function parseAndTranslateChanges(
  previousJson: string | null,
  currentJson: string | null,
  t: (key: string, opts?: Record<string, unknown>) => string,
): { field: string; from: string; to: string }[] {
  if (!previousJson || !currentJson) return []
  try {
    const prev = JSON.parse(previousJson)
    const curr = JSON.parse(currentJson)
    const changes: { field: string; from: string; to: string }[] = []
    const allKeys = new Set([...Object.keys(prev), ...Object.keys(curr)])
    for (const key of allKeys) {
      const prevVal = JSON.stringify(prev[key])
      const currVal = JSON.stringify(curr[key])
      if (prevVal !== currVal) {
        const camelKey = toCamelCase(key)
        changes.push({
          field: t(`history.fields.${camelKey}`, { defaultValue: key }),
          from: formatValue(prev[key]),
          to: formatValue(curr[key]),
        })
      }
    }
    return changes
  } catch {
    return []
  }
}

// -- Component --

export function VersionHistoryDialog({
  open,
  onOpenChange,
  packageId,
  packageName,
}: VersionHistoryDialogProps) {
  const { t } = useTranslation("treatment")
  const { data: versions, isLoading } = usePackageVersions(
    open ? packageId : undefined,
  )

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <IconHistory className="h-5 w-5" />
            {t("versionHistory")}
          </DialogTitle>
          {packageName && (
            <DialogDescription>{packageName}</DialogDescription>
          )}
        </DialogHeader>

        {isLoading ? (
          <div className="flex flex-col items-center gap-2 py-8">
            <IconLoader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            <p className="text-sm text-muted-foreground">{t("history.loading")}</p>
          </div>
        ) : !versions || versions.length === 0 ? (
          <div className="py-8 text-center text-muted-foreground">
            {t("history.noChanges")}
          </div>
        ) : (
          <div className="space-y-3">
            {versions.map((version) => {
              const changes = parseAndTranslateChanges(
                version.previousJson,
                version.currentJson,
                t,
              )

              return (
                <Card key={version.versionNumber}>
                  <CardContent className="pt-4 pb-3 space-y-2">
                    {/* Header row */}
                    <div className="flex items-center justify-between">
                      <Badge variant="outline">
                        {t("history.version")} {version.versionNumber}
                      </Badge>
                      <span className="text-xs text-muted-foreground">
                        {format(
                          new Date(version.changedAt),
                          "dd/MM/yyyy HH:mm",
                        )}
                      </span>
                    </div>

                    {/* Reason */}
                    <div className="text-sm">
                      <span className="text-muted-foreground">{t("history.reason")}</span>{" "}
                      {version.reason}
                    </div>

                    {/* Translated field-by-field diff */}
                    {changes.length > 0 ? (
                      <div className="space-y-1">
                        <span className="text-sm text-muted-foreground">{t("history.changes")}</span>
                        <div className="mt-1 space-y-1">
                          {changes.map((change, i) => (
                            <div key={i} className="flex items-center gap-2 text-sm">
                              <span className="font-medium min-w-[120px]">{change.field}:</span>
                              <span className="text-red-600 line-through">{change.from}</span>
                              <span className="text-muted-foreground">&rarr;</span>
                              <span className="text-green-600">{change.to}</span>
                            </div>
                          ))}
                        </div>
                      </div>
                    ) : (
                      <div className="text-sm">
                        <span className="text-muted-foreground">{t("history.changes")}</span>{" "}
                        {version.changeDescription}
                      </div>
                    )}

                    {/* Technical details (collapsible raw JSON) */}
                    {(version.previousJson || version.currentJson) && (
                      <details className="text-xs">
                        <summary className="cursor-pointer text-muted-foreground hover:text-foreground transition-colors">
                          {t("history.technicalDetails")}
                        </summary>
                        <div className="mt-2 grid grid-cols-2 gap-2">
                          {version.previousJson && (
                            <div>
                              <div className="font-medium text-muted-foreground mb-1">
                                {t("history.before")}
                              </div>
                              <pre className="p-2 bg-muted rounded text-xs whitespace-pre-wrap break-all">
                                {formatJson(version.previousJson)}
                              </pre>
                            </div>
                          )}
                          {version.currentJson && (
                            <div>
                              <div className="font-medium text-muted-foreground mb-1">
                                {t("history.after")}
                              </div>
                              <pre className="p-2 bg-muted rounded text-xs whitespace-pre-wrap break-all">
                                {formatJson(version.currentJson)}
                              </pre>
                            </div>
                          )}
                        </div>
                      </details>
                    )}
                  </CardContent>
                </Card>
              )
            })}
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}
