import { useState, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { QRCodeSVG } from "qrcode.react"
import { IconClipboard, IconQrcode } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import {
  Collapsible,
  CollapsibleTrigger,
  CollapsibleContent,
} from "@/shared/components/Collapsible"
import { cn } from "@/shared/lib/utils"
import { useGenerateOsdiLink, useSubmitOsdiInline } from "../api/clinical-api"
import type { DryEyeAssessmentDto, OsdiLinkResponse } from "../api/clinical-api"
import { OsdiQuestionnaire, SEVERITY_CONFIG } from "./OsdiQuestionnaire"
import { OsdiAnswersSection } from "./OsdiAnswersSection"

type SeverityKey = keyof typeof SEVERITY_CONFIG

function getSeverityKey(severityValue: number | null | undefined): SeverityKey {
  if (severityValue === null || severityValue === undefined) return "normal"
  switch (severityValue) {
    case 0: return "normal"
    case 1: return "mild"
    case 2: return "moderate"
    case 3: return "severe"
    default: return "normal"
  }
}

interface OsdiSectionProps {
  visitId: string
  patientId: string
  assessment: DryEyeAssessmentDto | undefined
  disabled: boolean
}

export function OsdiSection({ visitId, patientId, assessment, disabled }: OsdiSectionProps) {
  const { t } = useTranslation("clinical")
  const [showQuestionnaire, setShowQuestionnaire] = useState(false)
  const [osdiLink, setOsdiLink] = useState<OsdiLinkResponse | null>(null)
  const [showQrCode, setShowQrCode] = useState(false)

  const generateLinkMutation = useGenerateOsdiLink()
  const submitOsdiMutation = useSubmitOsdiInline()

  const osdiScore = assessment?.osdiScore
  const osdiSeverity = assessment?.osdiSeverity
  const severityKey = getSeverityKey(osdiSeverity)
  const severityConfig = SEVERITY_CONFIG[severityKey]

  const handleGenerateLink = useCallback(() => {
    generateLinkMutation.mutate(visitId, {
      onSuccess: (data) => {
        const fullUrl = data.url.startsWith("http")
          ? data.url
          : `${window.location.origin}${data.url}`
        setOsdiLink({ ...data, url: fullUrl })
        setShowQrCode(true)
        toast.success(t("osdi.linkGenerated"))
      },
      onError: () => {
        toast.error(t("osdi.linkFailed"))
      },
    })
  }, [visitId, generateLinkMutation, t])

  const handleCopyLink = useCallback(() => {
    if (osdiLink?.url) {
      navigator.clipboard.writeText(osdiLink.url)
      toast.success(t("osdi.linkCopied"))
    }
  }, [osdiLink, t])

  const handleQuestionnaireSubmit = useCallback(
    (answers: (number | null)[]) => {
      submitOsdiMutation.mutate(
        { visitId, answers },
        {
          onSuccess: () => {
            toast.success(t("osdi.submitted"))
            setShowQuestionnaire(false)
          },
          onError: () => {
            toast.error(t("osdi.submitFailed"))
          },
        },
      )
    },
    [visitId, submitOsdiMutation, t],
  )

  return (
    <div className="space-y-4 border-t pt-4">
      <div className="flex items-center justify-between">
        <h4 className="text-sm font-semibold">{t("osdi.title")}</h4>
        <div className="flex items-center gap-2">
          {!disabled && (
            <>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setShowQuestionnaire(!showQuestionnaire)}
              >
                {showQuestionnaire ? t("osdi.hideQuestionnaire") : t("osdi.fillQuestionnaire")}
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={handleGenerateLink}
                disabled={generateLinkMutation.isPending}
              >
                <IconQrcode className="h-4 w-4 mr-1" />
                {t("osdi.generateLink")}
              </Button>
            </>
          )}
        </div>
      </div>

      {/* Score display */}
      {osdiScore !== null && osdiScore !== undefined ? (
        <div className="flex items-center gap-4 p-3 rounded-lg border bg-muted/30">
          <span className="text-3xl font-bold tabular-nums">
            {Number(osdiScore).toFixed(1)}
          </span>
          <div className="flex flex-col gap-1">
            <Badge className={cn("border", severityConfig.color)} variant="outline">
              {t(severityConfig.label)}
            </Badge>
            <span className="text-xs text-muted-foreground">
              OSDI {t("osdi.score")}
            </span>
          </div>
        </div>
      ) : (
        <div className="text-sm text-muted-foreground p-3 rounded-lg border border-dashed">
          {t("osdi.noScore")}
        </div>
      )}

      {/* OSDI Answers detail */}
      {osdiScore !== null && osdiScore !== undefined && (
        <OsdiAnswersSection visitId={visitId} />
      )}

      {/* QR Code section */}
      {showQrCode && osdiLink && (
        <div className="flex flex-col items-center gap-3 p-4 rounded-lg border bg-background">
          <QRCodeSVG value={osdiLink.url} size={180} level="M" />
          <div className="flex items-center gap-2">
            <span className="text-xs text-muted-foreground truncate max-w-xs">
              {osdiLink.url}
            </span>
            <Button variant="ghost" size="sm" onClick={handleCopyLink}>
              <IconClipboard className="h-4 w-4" />
            </Button>
          </div>
          <span className="text-xs text-muted-foreground">
            {t("osdi.linkExpires", {
              date: new Date(osdiLink.expiresAt).toLocaleString(),
            })}
          </span>
        </div>
      )}

      {/* Inline questionnaire */}
      <Collapsible open={showQuestionnaire} onOpenChange={setShowQuestionnaire}>
        <CollapsibleContent>
          <div className="pt-2">
            <OsdiQuestionnaire
              onSubmit={handleQuestionnaireSubmit}
              isSubmitting={submitOsdiMutation.isPending}
              disabled={disabled}
            />
          </div>
        </CollapsibleContent>
      </Collapsible>
    </div>
  )
}
