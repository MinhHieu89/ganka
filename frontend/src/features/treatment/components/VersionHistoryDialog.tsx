import { format } from "date-fns"
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

// -- Component --

export function VersionHistoryDialog({
  open,
  onOpenChange,
  packageId,
  packageName,
}: VersionHistoryDialogProps) {
  const { data: versions, isLoading } = usePackageVersions(
    open ? packageId : undefined,
  )

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <IconHistory className="h-5 w-5" />
            Lich su thay doi
          </DialogTitle>
          {packageName && (
            <DialogDescription>{packageName}</DialogDescription>
          )}
        </DialogHeader>

        {isLoading ? (
          <div className="flex flex-col items-center gap-2 py-8">
            <IconLoader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            <p className="text-sm text-muted-foreground">Dang tai...</p>
          </div>
        ) : !versions || versions.length === 0 ? (
          <div className="py-8 text-center text-muted-foreground">
            Chua co thay doi nao duoc ghi nhan
          </div>
        ) : (
          <div className="space-y-3">
            {versions.map((version) => (
              <Card key={version.versionNumber}>
                <CardContent className="pt-4 pb-3 space-y-2">
                  {/* Header row */}
                  <div className="flex items-center justify-between">
                    <Badge variant="outline">
                      Phien ban {version.versionNumber}
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
                    <span className="text-muted-foreground">Ly do:</span>{" "}
                    {version.reason}
                  </div>

                  {/* Change description */}
                  <div className="text-sm">
                    <span className="text-muted-foreground">Noi dung thay doi:</span>{" "}
                    {version.changeDescription}
                  </div>

                  {/* Optional JSON diff */}
                  {(version.previousJson || version.currentJson) && (
                    <details className="text-xs">
                      <summary className="cursor-pointer text-muted-foreground hover:text-foreground transition-colors">
                        Chi tiet ky thuat
                      </summary>
                      <div className="mt-2 grid grid-cols-2 gap-2">
                        {version.previousJson && (
                          <div>
                            <div className="font-medium text-muted-foreground mb-1">
                              Truoc
                            </div>
                            <pre className="p-2 bg-muted rounded text-xs whitespace-pre-wrap break-all">
                              {formatJson(version.previousJson)}
                            </pre>
                          </div>
                        )}
                        {version.currentJson && (
                          <div>
                            <div className="font-medium text-muted-foreground mb-1">
                              Sau
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
