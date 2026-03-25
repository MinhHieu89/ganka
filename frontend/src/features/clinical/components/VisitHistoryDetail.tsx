import { useTranslation } from "react-i18next"
import { Badge } from "@/shared/components/Badge"
import { ScrollArea } from "@/shared/components/ScrollArea"
import { Skeleton } from "@/shared/components/Skeleton"
import { useVisitById } from "../api/clinical-api"
import { RefractionSection } from "./RefractionSection"
import { DryEyeSection } from "./DryEyeSection"
import { DiagnosisSection } from "./DiagnosisSection"
import { DrugPrescriptionSection } from "./DrugPrescriptionSection"
import { OpticalPrescriptionSection } from "./OpticalPrescriptionSection"
import { ExaminationNotesSection } from "./ExaminationNotesSection"

const STATUS_VARIANT: Record<number, "outline" | "default" | "secondary" | "destructive"> = {
  0: "outline",
  1: "default",
  2: "secondary",
  3: "destructive",
}

const STATUS_KEY: Record<number, string> = {
  0: "draft",
  1: "signed",
  2: "amended",
  3: "cancelled",
}

interface VisitHistoryDetailProps {
  visitId: string | null
}

export function VisitHistoryDetail({ visitId }: VisitHistoryDetailProps) {
  const { t } = useTranslation("clinical")
  const { data: visit, isLoading } = useVisitById(visitId ?? undefined)

  if (!visitId) {
    return (
      <div className="flex items-center justify-center h-full text-muted-foreground">
        {t("patient.visitHistory.selectVisit")}
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="space-y-4 p-4">
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} className="h-32 w-full" />
        ))}
      </div>
    )
  }

  if (!visit) {
    return (
      <div className="text-center py-8 text-destructive">
        {t("visit.notFound")}
      </div>
    )
  }

  const statusKey = STATUS_KEY[visit.status] ?? "draft"

  return (
    <ScrollArea className="h-full">
      <div className="space-y-4 p-4">
        {/* Visit header */}
        <div className="flex items-center justify-between">
          <div>
            <h3 className="font-semibold">
              {new Date(visit.visitDate).toLocaleDateString()}
            </h3>
            <p className="text-sm text-muted-foreground">{visit.doctorName}</p>
          </div>
          <Badge variant={STATUS_VARIANT[visit.status] ?? "outline"}>
            {t(`visit.status.${statusKey}`)}
          </Badge>
        </div>

        {/* Sections in read-only mode (disabled=true) */}
        <RefractionSection
          visitId={visit.id}
          refractions={visit.refractions}
          disabled
        />

        <DryEyeSection
          visitId={visit.id}
          patientId={visit.patientId}
          dryEyeAssessments={visit.dryEyeAssessments ?? []}
          disabled
        />

        <ExaminationNotesSection
          visitId={visit.id}
          initialNotes={visit.examinationNotes}
          disabled
        />

        <DiagnosisSection
          visitId={visit.id}
          diagnoses={visit.diagnoses}
          doctorId={visit.doctorId}
          disabled
        />

        <DrugPrescriptionSection
          visitId={visit.id}
          patientId={visit.patientId}
          prescriptions={visit.drugPrescriptions ?? []}
          disabled
        />

        <OpticalPrescriptionSection
          visitId={visit.id}
          prescriptions={visit.opticalPrescriptions ?? []}
          refractions={visit.refractions}
          disabled
        />

        {visit.signedAt && (
          <div className="text-xs text-muted-foreground border-t pt-2">
            {t("visit.signedAt", {
              date: new Date(visit.signedAt).toLocaleString(),
            })}
          </div>
        )}
      </div>
    </ScrollArea>
  )
}
