import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { IconAlertTriangle } from "@tabler/icons-react"
import { Alert, AlertTitle, AlertDescription } from "@/shared/components/Alert"
import type { AllergyDto } from "@/features/patient/api/patient-api"

interface DrugAllergyWarningProps {
  allergies: AllergyDto[]
  drugName: string
  genericName?: string | null
}

export function DrugAllergyWarning({
  allergies,
  drugName,
  genericName,
}: DrugAllergyWarningProps) {
  const { t } = useTranslation("clinical")

  // Find matching allergies (case-insensitive, bidirectional match)
  const matchingAllergies = useMemo(() => {
    if (!allergies || allergies.length === 0 || !drugName) return []
    const dn = drugName.toLowerCase()
    const gn = genericName?.toLowerCase() ?? ""
    return allergies.filter((a) => {
      const allergyName = a.name.toLowerCase()
      return (
        dn.includes(allergyName) ||
        allergyName.includes(dn) ||
        (gn && (gn.includes(allergyName) || allergyName.includes(gn)))
      )
    })
  }, [allergies, drugName, genericName])

  if (matchingAllergies.length === 0) return null

  return (
    <Alert
      variant="destructive"
      className="bg-destructive/10 border-destructive"
    >
      <IconAlertTriangle className="h-4 w-4" />
      <AlertTitle>{t("prescription.allergyWarning")}</AlertTitle>
      <AlertDescription>
        <ul className="list-disc list-inside text-sm mt-1">
          {matchingAllergies.map((a) => (
            <li key={a.id}>
              {a.name}{" "}
              <span className="text-xs opacity-75">({a.severity})</span>
            </li>
          ))}
        </ul>
      </AlertDescription>
    </Alert>
  )
}
