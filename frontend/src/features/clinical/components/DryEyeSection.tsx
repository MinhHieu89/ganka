import { useTranslation } from "react-i18next"
import { VisitSection } from "./VisitSection"
import { DryEyeForm } from "./DryEyeForm"
import { OsdiSection } from "./OsdiSection"
import type { DryEyeAssessmentDto } from "../api/clinical-api"

interface DryEyeSectionProps {
  visitId: string
  patientId: string
  dryEyeAssessments: DryEyeAssessmentDto[]
  disabled: boolean
}

export function DryEyeSection({
  visitId,
  patientId,
  dryEyeAssessments,
  disabled,
}: DryEyeSectionProps) {
  const { t } = useTranslation("clinical")

  // A visit typically has at most one dry eye assessment
  const assessment = dryEyeAssessments[0]

  return (
    <VisitSection title={t("dryEye.title")} defaultOpen={true}>
      <DryEyeForm visitId={visitId} data={assessment} disabled={disabled} />
      <OsdiSection
        visitId={visitId}
        patientId={patientId}
        assessment={assessment}
        disabled={disabled}
      />
    </VisitSection>
  )
}
