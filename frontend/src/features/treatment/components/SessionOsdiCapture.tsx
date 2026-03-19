import { useState, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { QRCodeSVG } from "qrcode.react"
import { IconClipboard, IconLoader2 } from "@tabler/icons-react"
import { toast } from "sonner"
import { Button } from "@/shared/components/Button"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/Tabs"
import { useRegisterOsdiToken } from "@/features/treatment/api/treatment-api"
import { OsdiQuestionnaire, calculateOsdi } from "@/features/clinical/components/OsdiQuestionnaire"

// -- Props --

interface SessionOsdiCaptureProps {
  packageId: string
  sessionNumber?: number
  osdiScore: number | null
  onOsdiScoreChange: (score: number | null) => void
  osdiSeverity: string | null
  onOsdiAnswersChange?: (answers: (number | null)[]) => void
}

// -- Component --

export function SessionOsdiCapture({
  packageId,
  sessionNumber,
  osdiScore,
  onOsdiScoreChange,
  onOsdiAnswersChange,
}: SessionOsdiCaptureProps) {
  const { t } = useTranslation("treatment")
  const [qrUrl, setQrUrl] = useState<string | null>(null)
  const [submitted, setSubmitted] = useState(false)
  const registerToken = useRegisterOsdiToken()

  const handleOsdiSubmit = useCallback(
    (answers: (number | null)[]) => {
      const result = calculateOsdi(answers)
      onOsdiScoreChange(result.score)
      onOsdiAnswersChange?.(answers)
      setSubmitted(true)
    },
    [onOsdiScoreChange, onOsdiAnswersChange],
  )

  const handleGenerateQr = useCallback(() => {
    const token = crypto.randomUUID()
    const baseUrl = window.location.origin

    // Register token with backend (now creates DB-backed record via Clinical module)
    registerToken.mutate(
      { packageId, sessionNumber, token },
      {
        onSuccess: (response) => {
          // Use the URL returned from backend response
          const url = `${baseUrl}${response.url}`
          setQrUrl(url)
        },
        onError: () => {
          toast.error(t("osdi.qrError"))
        },
      },
    )
  }, [packageId, sessionNumber, registerToken, t])

  const handleCopyLink = useCallback(() => {
    if (qrUrl) {
      navigator.clipboard.writeText(qrUrl)
      toast.success(t("osdi.linkCopied"))
    }
  }, [qrUrl, t])

  return (
    <Tabs defaultValue="inline" className="w-full">
      <TabsList className="grid w-full grid-cols-2">
        <TabsTrigger value="inline">{t("osdi.inlineTab")}</TabsTrigger>
        <TabsTrigger value="self-fill">{t("osdi.selfFillTab")}</TabsTrigger>
      </TabsList>

      {/* Inline mode - full 12-question OSDI questionnaire */}
      <TabsContent value="inline" className="space-y-3 mt-3">
        {submitted && osdiScore !== null ? (
          <div className="text-center py-4">
            <p className="text-sm text-muted-foreground mb-2">
              {t("osdi.submitted")}
            </p>
            <span className="text-2xl font-bold tabular-nums">
              {osdiScore.toFixed(1)}
            </span>
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="ml-2"
              onClick={() => {
                setSubmitted(false)
                onOsdiScoreChange(null)
              }}
            >
              {t("osdi.retake")}
            </Button>
          </div>
        ) : (
          <OsdiQuestionnaire onSubmit={handleOsdiSubmit} />
        )}
      </TabsContent>

      {/* Patient self-fill mode */}
      <TabsContent value="self-fill" className="space-y-3 mt-3">
        {registerToken.isPending ? (
          <div className="flex flex-col items-center gap-2 py-6">
            <IconLoader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            <p className="text-sm text-muted-foreground">
              {t("osdi.generatingQr")}
            </p>
          </div>
        ) : !qrUrl ? (
          <div className="text-center py-4">
            <p className="text-sm text-muted-foreground mb-3">
              {t("osdi.generateQrDesc")}
            </p>
            <Button
              type="button"
              variant="outline"
              onClick={handleGenerateQr}
            >
              {t("osdi.generateQr")}
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
              {t("osdi.scanQrInstructions")}
            </p>
          </div>
        )}
      </TabsContent>
    </Tabs>
  )
}
