import { useTranslation } from "react-i18next"
import { IconAlertTriangle } from "@tabler/icons-react"
import { Alert, AlertDescription, AlertTitle } from "@/shared/components/Alert"
import { Button } from "@/shared/components/Button"
import type { MissingFieldInfo } from "@/features/patient/api/patient-api"

interface PatientFieldWarningProps {
  missingFields: MissingFieldInfo[]
  onUpdateProfile: () => void
}

const fieldNameKeyMap: Record<string, string> = {
  Address: "fieldWarning.address",
  Cccd: "fieldWarning.cccd",
}

export function PatientFieldWarning({
  missingFields,
  onUpdateProfile,
}: PatientFieldWarningProps) {
  const { t } = useTranslation("patient")

  if (missingFields.length === 0) return null

  return (
    <Alert className="border-amber-500/50 bg-amber-50 text-amber-900 dark:border-amber-500/30 dark:bg-amber-950/50 dark:text-amber-200 [&>svg]:text-amber-600 dark:[&>svg]:text-amber-400">
      <IconAlertTriangle className="h-4 w-4" />
      <AlertTitle>{t("fieldWarning.title")}</AlertTitle>
      <AlertDescription>
        <p className="mb-2">{t("fieldWarning.description")}</p>
        <ul className="mb-3 list-inside list-disc text-sm">
          {missingFields.map((field) => (
            <li key={field.fieldName}>
              {t(fieldNameKeyMap[field.fieldName] ?? field.fieldName)}
            </li>
          ))}
        </ul>
        <Button
          variant="outline"
          size="sm"
          className="border-amber-500/50 hover:bg-amber-100 dark:border-amber-500/30 dark:hover:bg-amber-900/50"
          onClick={onUpdateProfile}
        >
          {t("fieldWarning.updateProfile")}
        </Button>
      </AlertDescription>
    </Alert>
  )
}
