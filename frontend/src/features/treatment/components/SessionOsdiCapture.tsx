import { useState, useMemo, useCallback } from "react"
import { QRCodeSVG } from "qrcode.react"
import { IconClipboard } from "@tabler/icons-react"
import { toast } from "sonner"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/Tabs"
import { cn } from "@/shared/lib/utils"

// -- Severity config reused from OsdiQuestionnaire --

const SEVERITY_CONFIG = {
  Normal: {
    color: "bg-green-100 text-green-800 border-green-300",
  },
  Mild: {
    color: "bg-yellow-100 text-yellow-800 border-yellow-300",
  },
  Moderate: {
    color: "bg-orange-100 text-orange-800 border-orange-300",
  },
  Severe: {
    color: "bg-red-100 text-red-800 border-red-300",
  },
} as const

type SeverityKey = keyof typeof SEVERITY_CONFIG

// -- Props --

interface SessionOsdiCaptureProps {
  osdiScore: number | null
  onOsdiScoreChange: (score: number | null) => void
  osdiSeverity: string | null
}

// -- Component --

export function SessionOsdiCapture({
  osdiScore,
  onOsdiScoreChange,
  osdiSeverity,
}: SessionOsdiCaptureProps) {
  const [qrUrl, setQrUrl] = useState<string | null>(null)

  const severityConfig = osdiSeverity
    ? SEVERITY_CONFIG[osdiSeverity as SeverityKey]
    : null

  const handleScoreChange = useCallback(
    (value: string) => {
      if (value === "") {
        onOsdiScoreChange(null)
        return
      }
      const num = Number(value)
      if (!isNaN(num) && num >= 0 && num <= 100) {
        onOsdiScoreChange(num)
      }
    },
    [onOsdiScoreChange],
  )

  const handleGenerateQr = useCallback(() => {
    // Generate a public OSDI page URL with a token
    // The token will be linked back to the session on the backend
    const token = crypto.randomUUID()
    const baseUrl = window.location.origin
    const url = `${baseUrl}/osdi/self-fill?token=${token}`
    setQrUrl(url)
  }, [])

  const handleCopyLink = useCallback(() => {
    if (qrUrl) {
      navigator.clipboard.writeText(qrUrl)
      toast.success("Link đã được sao chép")
    }
  }, [qrUrl])

  return (
    <Tabs defaultValue="inline" className="w-full">
      <TabsList className="grid w-full grid-cols-2">
        <TabsTrigger value="inline">Nhập trực tiếp</TabsTrigger>
        <TabsTrigger value="self-fill">Bệnh nhân tự điền</TabsTrigger>
      </TabsList>

      {/* Inline mode */}
      <TabsContent value="inline" className="space-y-3 mt-3">
        <div className="flex items-center gap-4">
          <div className="flex-1">
            <Label htmlFor="osdiScoreInline" className="text-sm">
              OSDI Score (0-100)
            </Label>
            <Input
              id="osdiScoreInline"
              type="number"
              min={0}
              max={100}
              step="0.1"
              value={osdiScore ?? ""}
              onChange={(e) => handleScoreChange(e.target.value)}
              className="mt-1"
            />
          </div>

          {/* Severity badge */}
          {osdiSeverity && severityConfig && (
            <div className="pt-5">
              <Badge
                variant="outline"
                className={cn("border", severityConfig.color)}
              >
                {osdiSeverity}
              </Badge>
            </div>
          )}
        </div>

        {/* Severity guide */}
        <div className="text-xs text-muted-foreground space-y-0.5">
          <div>0-12: Normal | 13-22: Mild | 23-32: Moderate | 33-100: Severe</div>
        </div>
      </TabsContent>

      {/* Patient self-fill mode */}
      <TabsContent value="self-fill" className="space-y-3 mt-3">
        {!qrUrl ? (
          <div className="text-center py-4">
            <p className="text-sm text-muted-foreground mb-3">
              Tạo mã QR để bệnh nhân tự điền bảng câu hỏi OSDI
            </p>
            <Button
              type="button"
              variant="outline"
              onClick={handleGenerateQr}
            >
              Tạo QR Link
            </Button>
          </div>
        ) : (
          <div className="flex flex-col items-center gap-3 p-4 rounded-lg border bg-background">
            <QRCodeSVG value={qrUrl} size={180} level="M" />
            <div className="flex items-center gap-2">
              <span className="text-xs text-muted-foreground truncate max-w-xs">
                {qrUrl}
              </span>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={handleCopyLink}
              >
                <IconClipboard className="h-4 w-4" />
              </Button>
            </div>
            <p className="text-xs text-muted-foreground">
              Bệnh nhân quét mã QR hoặc mở link để điền bảng OSDI
            </p>
          </div>
        )}
      </TabsContent>
    </Tabs>
  )
}
