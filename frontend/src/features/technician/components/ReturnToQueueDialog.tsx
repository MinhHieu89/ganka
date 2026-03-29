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

interface ReturnToQueueDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  patientName: string
  onConfirm: () => void
  isLoading: boolean
}

export function ReturnToQueueDialog({
  open,
  onOpenChange,
  patientName,
  onConfirm,
  isLoading,
}: ReturnToQueueDialogProps) {
  const { t } = useTranslation("technician")

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{t("dialog.returnTitle")}</AlertDialogTitle>
          <AlertDialogDescription>
            {t("dialog.returnMessage", { name: patientName })}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>{t("dialog.returnCancel")}</AlertDialogCancel>
          <AlertDialogAction onClick={onConfirm} disabled={isLoading}>
            {t("dialog.returnConfirm")}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
