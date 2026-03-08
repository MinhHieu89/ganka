import { useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link } from "@tanstack/react-router"
import { IconArrowLeft } from "@tabler/icons-react"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/Tabs"
import { Skeleton } from "@/shared/components/Skeleton"
import { Button } from "@/shared/components/Button"
import { useRecentPatientsStore } from "@/shared/stores/recentPatientsStore"
import { usePatientById } from "@/features/patient/api/patient-api"
import { PatientProfileHeader } from "@/features/patient/components/PatientProfileHeader"
import { PatientOverviewTab } from "@/features/patient/components/PatientOverviewTab"
import { PatientAllergyTab } from "@/features/patient/components/PatientAllergyTab"
import { PatientAppointmentTab } from "@/features/patient/components/PatientAppointmentTab"
import { PatientDryEyeTab } from "@/features/patient/components/PatientDryEyeTab"
import { PatientPrescriptionsTab } from "@/features/pharmacy/components/PatientPrescriptionsTab"
import { PrescriptionHistoryTab } from "@/features/optical/components/PrescriptionHistoryTab"

interface PatientProfilePageProps {
  patientId: string
}

export function PatientProfilePage({ patientId }: PatientProfilePageProps) {
  const { t } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const { t: tClinical } = useTranslation("clinical")
  const { t: tPharmacy } = useTranslation("pharmacy")
  const { t: tOptical } = useTranslation("optical")
  const addRecent = useRecentPatientsStore((s) => s.addRecent)
  const [isEditing, setIsEditing] = useState(false)

  const patientQuery = usePatientById(patientId)
  const patient = patientQuery.data

  // Update recent patients store when patient loads
  useEffect(() => {
    if (patient) {
      addRecent({
        id: patient.id,
        fullName: patient.fullName,
        patientCode: patient.patientCode ?? "",
        phone: patient.phone,
      })
    }
  }, [patient, addRecent])

  // Loading state
  if (patientQuery.isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Skeleton className="h-20 w-20 rounded-full" />
          <div className="space-y-2">
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-4 w-64" />
          </div>
        </div>
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    )
  }

  // Error state
  if (patientQuery.isError || !patient) {
    const errorMessage = patientQuery.error?.message ?? ""
    const isAuthError =
      errorMessage.includes("401") ||
      errorMessage.toLowerCase().includes("unauthorized")

    return (
      <div className="flex flex-col items-center justify-center py-24 text-center">
        <p className="text-lg font-medium mb-2">
          {isAuthError ? tCommon("status.sessionExpired") : t("notFound")}
        </p>
        <p className="text-sm text-muted-foreground mb-4">
          {isAuthError
            ? tCommon("status.sessionExpiredDetail")
            : tCommon("status.error")}
        </p>
        {isAuthError ? (
          <Button
            variant="outline"
            onClick={() => window.location.reload()}
          >
            {tCommon("buttons.refresh")}
          </Button>
        ) : (
          <Link to={"/patients" as string}>
            <Button variant="outline">
              <IconArrowLeft className="h-4 w-4 mr-1" />
              {tCommon("buttons.back")}
            </Button>
          </Link>
        )}
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Back link */}
      <Link
        to={"/patients" as string}
        className="inline-flex items-center text-sm text-muted-foreground hover:text-foreground transition-colors"
      >
        <IconArrowLeft className="h-4 w-4 mr-1" />
        {t("title")}
      </Link>

      {/* Header */}
      <PatientProfileHeader
        patient={patient}
        onEdit={() => setIsEditing(true)}
      />

      {/* Tabs */}
      <Tabs defaultValue="overview">
        <TabsList>
          <TabsTrigger value="overview">{t("overview")}</TabsTrigger>
          <TabsTrigger value="allergies">{t("allergies")}</TabsTrigger>
          <TabsTrigger value="appointments">{t("appointments")}</TabsTrigger>
          <TabsTrigger value="prescriptions">{tPharmacy("queue.prescriptions.tab")}</TabsTrigger>
          <TabsTrigger value="dry-eye">{tClinical("dryEye.tab")}</TabsTrigger>
          <TabsTrigger value="optical-history">{tOptical("prescriptionHistory.tab")}</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="mt-4">
          <PatientOverviewTab
            patient={patient}
            isEditing={isEditing}
            onEditToggle={setIsEditing}
          />
        </TabsContent>

        <TabsContent value="allergies" className="mt-4">
          <PatientAllergyTab patient={patient} />
        </TabsContent>

        <TabsContent value="appointments" className="mt-4">
          <PatientAppointmentTab patientId={patient.id} />
        </TabsContent>

        <TabsContent value="prescriptions" className="mt-4">
          <PatientPrescriptionsTab patientId={patient.id} />
        </TabsContent>

        <TabsContent value="dry-eye" className="mt-4">
          <PatientDryEyeTab patientId={patient.id} />
        </TabsContent>

        <TabsContent value="optical-history" className="mt-4">
          <PrescriptionHistoryTab patientId={patient.id} />
        </TabsContent>
      </Tabs>
    </div>
  )
}
