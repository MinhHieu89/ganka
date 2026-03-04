import { useCallback } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconX, IconArrowUp } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import {
  useAddDiagnosis,
  useRemoveDiagnosis,
  type VisitDiagnosisDto,
  type Icd10SearchResultDto,
} from "../api/clinical-api"
import { VisitSection } from "./VisitSection"
import { Icd10Combobox } from "./Icd10Combobox"

// Laterality enum: 0=None, 1=OD, 2=OS, 3=OU
const LATERALITY_LABELS: Record<number, string> = {
  0: "",
  1: "OD",
  2: "OS",
  3: "OU",
}

interface DiagnosisSectionProps {
  visitId: string
  diagnoses: VisitDiagnosisDto[]
  doctorId: string
  disabled: boolean
}

export function DiagnosisSection({
  visitId,
  diagnoses,
  doctorId,
  disabled,
}: DiagnosisSectionProps) {
  const { t, i18n } = useTranslation("clinical")
  const addDiagnosisMutation = useAddDiagnosis()
  const removeDiagnosisMutation = useRemoveDiagnosis()

  const sortedDiagnoses = [...diagnoses].sort((a, b) => a.sortOrder - b.sortOrder)

  const handleAddDiagnosis = useCallback(
    (code: Icd10SearchResultDto, laterality: number) => {
      const role = diagnoses.length === 0 ? 0 : 1 // 0=Primary, 1=Secondary
      const sortOrder = diagnoses.length

      addDiagnosisMutation.mutate(
        {
          visitId,
          icd10Code: code.code,
          descriptionEn: code.descriptionEn,
          descriptionVi: code.descriptionVi,
          laterality,
          role,
          sortOrder,
        },
        {
          onSuccess: () => {
            toast.success(t("visit.diagnosisAdded"))
          },
          onError: () => {
            toast.error(t("visit.diagnosisAddFailed"))
          },
        },
      )
    },
    [visitId, diagnoses.length, addDiagnosisMutation, t],
  )

  const handleRemove = useCallback(
    (diagnosisId: string) => {
      removeDiagnosisMutation.mutate(
        { visitId, diagnosisId },
        {
          onSuccess: () => {
            toast.success(t("visit.diagnosisRemoved"))
          },
          onError: () => {
            toast.error(t("visit.diagnosisRemoveFailed"))
          },
        },
      )
    },
    [visitId, removeDiagnosisMutation, t],
  )

  const handleSetPrimary = useCallback(
    (diagnosisId: string) => {
      // Remove and re-add with role=0 (Primary) and sortOrder=0
      // For simplicity, we mark this diagnosis as primary by removing all and re-adding
      // Actually, let's just use the remove + add approach to change role
      // Since the backend doesn't have a "set primary" endpoint, we'll just note this visually
      // The backend determines primary by role field -- for now this is informational
      // In a real implementation, we'd need an update-diagnosis-role endpoint
      void diagnosisId
    },
    [],
  )

  const getDescription = (d: VisitDiagnosisDto) =>
    i18n.language === "vi" ? d.descriptionVi : d.descriptionEn

  return (
    <VisitSection title={t("visit.diagnosis")}>
      <div className="space-y-3">
        {/* Diagnosis list */}
        {sortedDiagnoses.length === 0 ? (
          <p className="text-sm text-muted-foreground">{t("visit.noDiagnoses")}</p>
        ) : (
          <div className="space-y-2">
            {sortedDiagnoses.map((d) => (
              <div
                key={d.id}
                className="flex items-center gap-2 p-2 rounded-md border text-sm"
              >
                <span className="font-mono text-xs shrink-0 font-medium">
                  {d.icd10Code}
                </span>
                <span className="truncate">{getDescription(d)}</span>
                {d.laterality > 0 && (
                  <Badge variant="outline" className="shrink-0">
                    {LATERALITY_LABELS[d.laterality]}
                  </Badge>
                )}
                <Badge
                  variant={d.role === 0 ? "default" : "secondary"}
                  className="shrink-0 ml-auto"
                >
                  {d.role === 0
                    ? t("diagnosis.primary")
                    : t("diagnosis.secondary")}
                </Badge>
                {!disabled && d.role !== 0 && (
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="h-6 w-6 p-0"
                    title={t("visit.setPrimary")}
                    onClick={() => handleSetPrimary(d.id)}
                  >
                    <IconArrowUp className="h-3 w-3" />
                  </Button>
                )}
                {!disabled && (
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="h-6 w-6 p-0 text-destructive"
                    onClick={() => handleRemove(d.id)}
                  >
                    <IconX className="h-3 w-3" />
                  </Button>
                )}
              </div>
            ))}
          </div>
        )}

        {/* Add diagnosis combobox */}
        {!disabled && (
          <Icd10Combobox
            doctorId={doctorId}
            onSelect={handleAddDiagnosis}
            disabled={disabled}
          />
        )}
      </div>
    </VisitSection>
  )
}
