import { useState } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconLoader2, IconShieldCheck } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/AlertDialog"
import { useSignOffVisit, type VisitDetailDto } from "../api/clinical-api"
import { AmendmentDialog } from "./AmendmentDialog"

interface SignOffSectionProps {
  visitId: string
  visit: VisitDetailDto
}

export function SignOffSection({ visitId, visit }: SignOffSectionProps) {
  const { t } = useTranslation("clinical")
  const { t: tCommon } = useTranslation("common")
  const signOffMutation = useSignOffVisit()
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [amendOpen, setAmendOpen] = useState(false)

  const handleSignOff = () => {
    signOffMutation.mutate(visitId, {
      onSuccess: () => {
        toast.success(t("visit.signedOff"))
        setConfirmOpen(false)
      },
      onError: (err) => {
        toast.error(err instanceof Error ? err.message : tCommon("status.error"))
      },
    })
  }

  const isSigned = visit.status === 1

  const signedAt = visit.signedAt
    ? new Date(visit.signedAt).toLocaleString(undefined, {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
      })
    : null

  return (
    <div className="space-y-3">
      {isSigned ? (
        <div className="flex items-center justify-between p-4 border rounded-lg">
          <div className="flex items-center gap-2">
            <IconShieldCheck className="h-5 w-5 text-primary" />
            <Badge variant="default">{t("visit.status.signed")}</Badge>
            {signedAt && (
              <span className="text-sm text-muted-foreground">
                {t("visit.signedAt", { date: signedAt })}
              </span>
            )}
          </div>
          <Button variant="outline" onClick={() => setAmendOpen(true)}>
            {t("visit.amend")}
          </Button>
        </div>
      ) : (
        <Button
          className="w-full"
          size="lg"
          onClick={() => setConfirmOpen(true)}
          disabled={signOffMutation.isPending}
        >
          {signOffMutation.isPending && (
            <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
          )}
          {t("visit.signOffButton")}
        </Button>
      )}

      {/* Sign-off confirmation dialog */}
      <AlertDialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t("visit.signOffTitle")}</AlertDialogTitle>
            <AlertDialogDescription>
              {t("visit.signOffConfirm")}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{tCommon("buttons.cancel")}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleSignOff}
              disabled={signOffMutation.isPending}
            >
              {signOffMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {tCommon("buttons.confirm")}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Amendment dialog */}
      <AmendmentDialog
        visitId={visitId}
        visit={visit}
        open={amendOpen}
        onClose={() => setAmendOpen(false)}
      />
    </div>
  )
}
