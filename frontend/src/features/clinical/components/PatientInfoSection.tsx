import { useTranslation } from "react-i18next"
import { Link } from "@tanstack/react-router"
import { IconCalendar, IconUser, IconStethoscope, IconLink, IconProgress } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { VisitSection } from "./VisitSection"
import type { VisitDetailDto } from "../api/clinical-api"

const STAGE_LABELS = [
  "reception",
  "refractionVa",
  "doctorExam",
  "diagnostics",
  "doctorReads",
  "rx",
  "cashier",
  "pharmacyOptical",
] as const

interface PatientInfoSectionProps {
  visit: VisitDetailDto
}

export function PatientInfoSection({ visit }: PatientInfoSectionProps) {
  const { t } = useTranslation("clinical")

  const stageLabel =
    visit.currentStage >= 0 && visit.currentStage < STAGE_LABELS.length
      ? t(`workflow.stages.${STAGE_LABELS[visit.currentStage]}`)
      : String(visit.currentStage)

  const visitDate = new Date(visit.visitDate).toLocaleDateString(undefined, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  })

  return (
    <VisitSection title={t("visit.patientInfo")}>
      <div className="grid grid-cols-2 gap-x-6 gap-y-3 text-sm">
        <div className="flex items-center gap-2">
          <IconUser className="h-4 w-4 text-muted-foreground" />
          <span className="text-muted-foreground">{t("visit.patientName")}:</span>
          <Link
            to="/patients/$patientId"
            params={{ patientId: visit.patientId }}
            target="_blank"
            rel="noopener noreferrer"
            className="font-medium text-primary underline-offset-4 hover:underline"
          >
            {visit.patientName}
          </Link>
        </div>
        <div className="flex items-center gap-2">
          <IconStethoscope className="h-4 w-4 text-muted-foreground" />
          <span className="text-muted-foreground">{t("visit.doctorName")}:</span>
          <span className="font-medium">{visit.doctorName}</span>
        </div>
        <div className="flex items-center gap-2">
          <IconCalendar className="h-4 w-4 text-muted-foreground" />
          <span className="text-muted-foreground">{t("visit.visitDate")}:</span>
          <span className="font-medium">{visitDate}</span>
        </div>
        <div className="flex items-center gap-2">
          <IconProgress className="h-4 w-4 text-muted-foreground" />
          <span className="text-muted-foreground">{t("visit.stage")}:</span>
          <Badge variant="outline">{stageLabel}</Badge>
        </div>
        {visit.appointmentId && (
          <div className="flex items-center gap-2 col-span-2">
            <IconLink className="h-4 w-4 text-muted-foreground" />
            <span className="text-muted-foreground">
              {t("visit.appointmentLinked")}
            </span>
          </div>
        )}
      </div>
    </VisitSection>
  )
}
