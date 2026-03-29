import { useTranslation } from "react-i18next"
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogFooter,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogCancel,
  AlertDialogAction,
} from "@/shared/components/AlertDialog"

interface PausePatientDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  currentPatientName: string
  onConfirm: () => void
  isLoading: boolean
}

export function PausePatientDialog({
  open,
  onOpenChange,
  currentPatientName,
  onConfirm,
  isLoading,
}: PausePatientDialogProps) {
  const { t } = useTranslation("technician")

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{t("dialog.pauseTitle")}</AlertDialogTitle>
          <AlertDialogDescription>
            {t("dialog.pauseMessage", { name: currentPatientName })}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>{t("dialog.pauseCancel")}</AlertDialogCancel>
          <AlertDialogAction onClick={onConfirm} disabled={isLoading}>
            {t("dialog.pauseConfirm")}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
