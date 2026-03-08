import { format } from "date-fns"
import { IconHistory } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Badge } from "@/shared/components/Badge"
import { Card, CardContent } from "@/shared/components/Card"

// -- Version entry type --

export interface PackageVersionEntry {
  versionNumber: number
  changedAt: string
  changedByName: string
  reason: string
  changes: string
  previousValues?: string | null
  newValues?: string | null
}

// -- Props --

interface VersionHistoryDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  versions: PackageVersionEntry[]
  packageName?: string
}

// -- Component --

export function VersionHistoryDialog({
  open,
  onOpenChange,
  versions,
  packageName,
}: VersionHistoryDialogProps) {
  const sortedVersions = [...versions].sort(
    (a, b) => b.versionNumber - a.versionNumber,
  )

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <IconHistory className="h-5 w-5" />
            Lịch sử thay đổi
          </DialogTitle>
          {packageName && (
            <DialogDescription>{packageName}</DialogDescription>
          )}
        </DialogHeader>

        {sortedVersions.length === 0 ? (
          <div className="py-8 text-center text-muted-foreground">
            Chưa có thay đổi nào được ghi nhận
          </div>
        ) : (
          <div className="space-y-3">
            {sortedVersions.map((version) => (
              <Card key={version.versionNumber}>
                <CardContent className="pt-4 pb-3 space-y-2">
                  {/* Header row */}
                  <div className="flex items-center justify-between">
                    <Badge variant="outline">
                      Phiên bản {version.versionNumber}
                    </Badge>
                    <span className="text-xs text-muted-foreground">
                      {format(
                        new Date(version.changedAt),
                        "dd/MM/yyyy HH:mm",
                      )}
                    </span>
                  </div>

                  {/* Changed by */}
                  <div className="text-sm">
                    <span className="text-muted-foreground">Người thay đổi:</span>{" "}
                    <span className="font-medium">{version.changedByName}</span>
                  </div>

                  {/* Reason */}
                  <div className="text-sm">
                    <span className="text-muted-foreground">Lý do:</span>{" "}
                    {version.reason}
                  </div>

                  {/* Change description */}
                  <div className="text-sm">
                    <span className="text-muted-foreground">Nội dung thay đổi:</span>{" "}
                    {version.changes}
                  </div>

                  {/* Optional JSON diff */}
                  {(version.previousValues || version.newValues) && (
                    <details className="text-xs">
                      <summary className="cursor-pointer text-muted-foreground hover:text-foreground transition-colors">
                        Chi tiết kỹ thuật
                      </summary>
                      <div className="mt-2 grid grid-cols-2 gap-2">
                        {version.previousValues && (
                          <div>
                            <div className="font-medium text-muted-foreground mb-1">
                              Trước
                            </div>
                            <pre className="p-2 bg-muted rounded text-xs whitespace-pre-wrap break-all">
                              {formatJson(version.previousValues)}
                            </pre>
                          </div>
                        )}
                        {version.newValues && (
                          <div>
                            <div className="font-medium text-muted-foreground mb-1">
                              Sau
                            </div>
                            <pre className="p-2 bg-muted rounded text-xs whitespace-pre-wrap break-all">
                              {formatJson(version.newValues)}
                            </pre>
                          </div>
                        )}
                      </div>
                    </details>
                  )}
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}

// -- Helpers --

function formatJson(value: string): string {
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}
