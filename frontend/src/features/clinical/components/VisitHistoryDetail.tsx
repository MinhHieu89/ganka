import { useTranslation } from "react-i18next"
import {
  IconCalendar,
  IconStethoscope,
  IconEye,
  IconClipboardHeart,
  IconPill,
  IconGlasses,
  IconNotes,
  IconCheck,
  IconDroplets,
  IconFileOff,
} from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { ScrollArea } from "@/shared/components/ScrollArea"
import { Separator } from "@/shared/components/Separator"
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

function formatFullDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString(undefined, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  })
}

function formatDateTime(dateStr: string): string {
  return new Date(dateStr).toLocaleString(undefined, {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  })
}

interface VisitHistoryDetailProps {
  visitId: string | null
}

export function VisitHistoryDetail({ visitId }: VisitHistoryDetailProps) {
  const { t } = useTranslation("clinical")
  const { data: visit, isLoading } = useVisitById(visitId ?? undefined)

  if (!visitId) {
    return (
      <div className="flex flex-col items-center justify-center h-full text-muted-foreground gap-3">
        <IconFileOff className="h-10 w-10 opacity-40" />
        <p className="text-sm">{t("patient.visitHistory.selectVisit")}</p>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="space-y-4 p-4">
        <Skeleton className="h-24 w-full" />
        <Skeleton className="h-8 w-48" />
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} className="h-32 w-full" />
        ))}
      </div>
    )
  }

  if (!visit) {
    return (
      <div className="flex flex-col items-center justify-center h-full text-destructive gap-2">
        <IconFileOff className="h-8 w-8" />
        <p className="text-sm font-medium">{t("visit.notFound")}</p>
      </div>
    )
  }

  const statusKey = STATUS_KEY[visit.status] ?? "draft"

  return (
    <ScrollArea className="h-full">
      <div className="space-y-4 p-4">
        {/* Visit Header Card */}
        <Card>
          <CardContent className="p-4">
            <div className="flex items-start justify-between">
              <div className="space-y-1.5">
                <div className="flex items-center gap-2 text-muted-foreground">
                  <IconCalendar className="h-4 w-4" />
                  <span className="text-sm font-medium text-foreground">
                    {formatFullDate(visit.visitDate)}
                  </span>
                </div>
                <div className="flex items-center gap-2 text-muted-foreground">
                  <IconStethoscope className="h-4 w-4" />
                  <span className="text-sm">{visit.doctorName}</span>
                </div>
              </div>
              <Badge variant={STATUS_VARIANT[visit.status] ?? "outline"}>
                {t(`visit.status.${statusKey}`)}
              </Badge>
            </div>
          </CardContent>
        </Card>

        <Separator />

        {/* Clinical Sections wrapped in Cards */}
        <SectionWrapper
          icon={<IconEye className="h-4 w-4" />}
          title={t("visit.sections.refraction")}
        >
          <RefractionSection
            visitId={visit.id}
            refractions={visit.refractions}
            disabled
          />
        </SectionWrapper>

        <SectionWrapper
          icon={<IconDroplets className="h-4 w-4" />}
          title={t("visit.sections.dryEye")}
        >
          <DryEyeSection
            visitId={visit.id}
            patientId={visit.patientId}
            dryEyeAssessments={visit.dryEyeAssessments ?? []}
            disabled
          />
        </SectionWrapper>

        <SectionWrapper
          icon={<IconNotes className="h-4 w-4" />}
          title={t("visit.sections.examinationNotes")}
        >
          <ExaminationNotesSection
            visitId={visit.id}
            initialNotes={visit.examinationNotes}
            disabled
          />
        </SectionWrapper>

        <SectionWrapper
          icon={<IconClipboardHeart className="h-4 w-4" />}
          title={t("visit.sections.diagnosis")}
        >
          <DiagnosisSection
            visitId={visit.id}
            diagnoses={visit.diagnoses}
            doctorId={visit.doctorId}
            disabled
          />
        </SectionWrapper>

        <SectionWrapper
          icon={<IconPill className="h-4 w-4" />}
          title={t("visit.sections.drugPrescription")}
        >
          <DrugPrescriptionSection
            visitId={visit.id}
            patientId={visit.patientId}
            prescriptions={visit.drugPrescriptions ?? []}
            disabled
          />
        </SectionWrapper>

        <SectionWrapper
          icon={<IconGlasses className="h-4 w-4" />}
          title={t("visit.sections.opticalPrescription")}
        >
          <OpticalPrescriptionSection
            visitId={visit.id}
            prescriptions={visit.opticalPrescriptions ?? []}
            refractions={visit.refractions}
            disabled
          />
        </SectionWrapper>

        {/* Signed at footer */}
        {visit.signedAt && (
          <Card className="bg-muted/30">
            <CardContent className="p-3 flex items-center gap-2 text-xs text-muted-foreground">
              <IconCheck className="h-4 w-4 text-green-600" />
              <span>
                {t("visit.signedAt", {
                  date: formatDateTime(visit.signedAt),
                })}
              </span>
            </CardContent>
          </Card>
        )}
      </div>
    </ScrollArea>
  )
}

/** Wrapper that provides a Card with icon + title header for each clinical section */
function SectionWrapper({
  icon,
  title,
  children,
}: {
  icon: React.ReactNode
  title: string
  children: React.ReactNode
}) {
  return (
    <Card>
      <CardHeader className="pb-2 pt-3 px-4">
        <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
          {icon}
          <span>{title}</span>
        </CardTitle>
      </CardHeader>
      <CardContent className="px-4 pb-4 pt-0">
        {children}
      </CardContent>
    </Card>
  )
}
