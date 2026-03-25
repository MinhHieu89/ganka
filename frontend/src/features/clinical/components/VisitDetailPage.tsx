import { useTranslation } from "react-i18next"
import { Link } from "@tanstack/react-router"
import { IconArrowLeft, IconLoader2 } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { useVisitById } from "../api/clinical-api"
import { useOsdiHub } from "../hooks/use-osdi-hub"
import { PatientInfoSection } from "./PatientInfoSection"
import { RefractionSection } from "./RefractionSection"
import { ExaminationNotesSection } from "./ExaminationNotesSection"
import { DiagnosisSection } from "./DiagnosisSection"
import { SignOffSection } from "./SignOffSection"
import { VisitAmendmentHistory } from "./VisitAmendmentHistory"
import { DryEyeSection } from "./DryEyeSection"
import { MedicalImagesSection } from "./MedicalImagesSection"
import { DrugPrescriptionSection } from "./DrugPrescriptionSection"
import { OpticalPrescriptionSection } from "./OpticalPrescriptionSection"
import { DocumentActionsSection } from "./DocumentActionsSection"

// Visit status enum: 0=Draft, 1=Signed, 2=Amended
const STATUS_MAP: Record<number, { key: string; variant: "default" | "secondary" | "outline" }> = {
  0: { key: "draft", variant: "outline" },
  1: { key: "signed", variant: "default" },
  2: { key: "amended", variant: "secondary" },
}

interface VisitDetailPageProps {
  visitId: string
}

export function VisitDetailPage({ visitId }: VisitDetailPageProps) {
  const { t } = useTranslation("clinical")
  const { data: visit, isLoading, error } = useVisitById(visitId)

  // Connect to OsdiHub for realtime OSDI score updates via SignalR
  useOsdiHub(visitId)

  if (isLoading) {
    return (
      <div className="space-y-4 p-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-48 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  if (error || !visit) {
    return (
      <div className="p-4 space-y-4">
        <Link to="/clinical">
          <Button variant="ghost" size="sm">
            <IconArrowLeft className="h-4 w-4 mr-1" />
            {t("visit.backToClinical")}
          </Button>
        </Link>
        <div className="flex items-center justify-center h-64 text-muted-foreground">
          {t("visit.notFound")}
        </div>
      </div>
    )
  }

  // Signed = read-only (status 1). Draft (0) and Amended (2) = editable.
  const isReadOnly = visit.status === 1

  const statusInfo = STATUS_MAP[visit.status] ?? STATUS_MAP[0]

  return (
    <div className="space-y-4 p-4 max-w-5xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/clinical">
            <Button variant="ghost" size="sm">
              <IconArrowLeft className="h-4 w-4 mr-1" />
              {t("visit.backToClinical")}
            </Button>
          </Link>
          <h1 className="text-xl font-semibold">
            {t("visit.titlePrefix")}{" "}
            <Link
              to="/patients/$patientId"
              params={{ patientId: visit.patientId }}
              className="text-primary hover:underline"
            >
              {visit.patientName}
            </Link>
          </h1>
          <Badge variant={statusInfo.variant}>
            {t(`visit.status.${statusInfo.key}`)}
          </Badge>
        </div>
      </div>

      {/* Sections */}
      <PatientInfoSection visit={visit} />

      <RefractionSection
        visitId={visitId}
        refractions={visit.refractions}
        disabled={isReadOnly}
      />

      <DryEyeSection
        visitId={visitId}
        patientId={visit.patientId}
        dryEyeAssessments={visit.dryEyeAssessments ?? []}
        disabled={isReadOnly}
      />

      <ExaminationNotesSection
        visitId={visitId}
        initialNotes={visit.examinationNotes}
        disabled={isReadOnly}
      />

      <DiagnosisSection
        visitId={visitId}
        diagnoses={visit.diagnoses}
        doctorId={visit.doctorId}
        disabled={isReadOnly}
      />

      <DrugPrescriptionSection
        visitId={visitId}
        patientId={visit.patientId}
        prescriptions={visit.drugPrescriptions ?? []}
        disabled={isReadOnly}
      />

      <OpticalPrescriptionSection
        visitId={visitId}
        prescriptions={visit.opticalPrescriptions ?? []}
        refractions={visit.refractions}
        disabled={isReadOnly}
      />

      <DocumentActionsSection visitId={visitId} />

      <MedicalImagesSection visitId={visitId} patientId={visit.patientId} />

      <VisitAmendmentHistory amendments={visit.amendments} />

      <SignOffSection visitId={visitId} visit={visit} />
    </div>
  )
}
