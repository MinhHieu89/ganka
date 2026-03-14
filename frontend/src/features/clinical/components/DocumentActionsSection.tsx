import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconFileText, IconPrinter } from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Label } from "@/shared/components/Label"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { PrintButton } from "./PrintButton"
import {
  generateReferralLetterPdf,
  generateConsentFormPdf,
} from "../api/document-api"

interface DocumentActionsSectionProps {
  visitId: string
}

export function DocumentActionsSection({ visitId }: DocumentActionsSectionProps) {
  const { t } = useTranslation("clinical")

  const [referralOpen, setReferralOpen] = useState(false)
  const [referralReason, setReferralReason] = useState("")
  const [referralTo, setReferralTo] = useState("")

  const [consentOpen, setConsentOpen] = useState(false)
  const [procedureType, setProcedureType] = useState("")

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <IconFileText className="h-5 w-5" />
          {t("documents.title", "Documents")}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="flex flex-wrap gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setReferralOpen(true)}
          >
            <IconPrinter className="h-4 w-4" />
            {t("prescription.printReferral")}
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setConsentOpen(true)}
          >
            <IconPrinter className="h-4 w-4" />
            {t("prescription.printConsent")}
          </Button>
        </div>

        {/* Referral Letter Dialog */}
        <Dialog open={referralOpen} onOpenChange={setReferralOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>{t("prescription.printReferral")}</DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label>{t("prescription.referralTo")}</Label>
                <Input
                  value={referralTo}
                  onChange={(e) => setReferralTo(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label>{t("prescription.referralReason")}</Label>
                <AutoResizeTextarea
                  value={referralReason}
                  onChange={(e) => setReferralReason(e.target.value)}
                  rows={3}
                />
              </div>
            </div>
            <DialogFooter>
              <PrintButton
                label={t("prescription.printReferral")}
                icon={<IconPrinter className="h-4 w-4" />}
                disabled={!referralReason.trim() || !referralTo.trim()}
                onClick={() =>
                  generateReferralLetterPdf(visitId, referralReason, referralTo)
                }
              />
            </DialogFooter>
          </DialogContent>
        </Dialog>

        {/* Consent Form Dialog */}
        <Dialog open={consentOpen} onOpenChange={setConsentOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>{t("prescription.printConsent")}</DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label>{t("prescription.procedureType")}</Label>
                <Input
                  value={procedureType}
                  onChange={(e) => setProcedureType(e.target.value)}
                />
              </div>
            </div>
            <DialogFooter>
              <PrintButton
                label={t("prescription.printConsent")}
                icon={<IconPrinter className="h-4 w-4" />}
                disabled={!procedureType.trim()}
                onClick={() =>
                  generateConsentFormPdf(visitId, procedureType)
                }
              />
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </CardContent>
    </Card>
  )
}
