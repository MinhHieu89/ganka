import { useRef, useState } from "react"
import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { vi, enUS } from "date-fns/locale"
import {
  IconEdit,
  IconUserOff,
  IconUserCheck,
  IconCamera,
} from "@tabler/icons-react"
import { Avatar, AvatarImage, AvatarFallback } from "@/shared/components/Avatar"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/components/AlertDialog"
import { AllergyAlert } from "@/features/patient/components/AllergyAlert"
import {
  useDeactivatePatient,
  useReactivatePatient,
  useUploadPatientPhoto,
  type PatientDto,
} from "@/features/patient/api/patient-api"
import { toast } from "sonner"

interface PatientProfileHeaderProps {
  patient: PatientDto
  onEdit: () => void
}

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/)
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase()
  return (
    parts[0].charAt(0).toUpperCase() +
    parts[parts.length - 1].charAt(0).toUpperCase()
  )
}

function calculateAge(dateOfBirth: string): number {
  const dob = new Date(dateOfBirth)
  const now = new Date()
  let age = now.getFullYear() - dob.getFullYear()
  const monthDiff = now.getMonth() - dob.getMonth()
  if (monthDiff < 0 || (monthDiff === 0 && now.getDate() < dob.getDate())) {
    age--
  }
  return age
}

export function PatientProfileHeader({
  patient,
  onEdit,
}: PatientProfileHeaderProps) {
  const { t, i18n } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const deactivateMutation = useDeactivatePatient()
  const reactivateMutation = useReactivatePatient()
  const uploadPhotoMutation = useUploadPatientPhoto()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [confirmOpen, setConfirmOpen] = useState(false)

  const locale = i18n.language === "vi" ? vi : enUS
  const dateFormat = i18n.language === "vi" ? "dd/MM/yyyy" : "MM/dd/yyyy"

  const handleDeactivate = async () => {
    try {
      await deactivateMutation.mutateAsync(patient.id)
      toast.success(t("deactivate"))
      setConfirmOpen(false)
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : tCommon("status.error"),
      )
    }
  }

  const handleReactivate = async () => {
    try {
      await reactivateMutation.mutateAsync(patient.id)
      toast.success(t("reactivate"))
      setConfirmOpen(false)
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : tCommon("status.error"),
      )
    }
  }

  const handlePhotoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    try {
      await uploadPhotoMutation.mutateAsync({ patientId: patient.id, file })
      toast.success(t("uploadPhoto"))
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : tCommon("status.error"),
      )
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row items-start gap-4">
        {/* Photo / Avatar */}
        <div className="relative group shrink-0">
          <Avatar className="h-20 w-20">
            {patient.photoUrl && (
              <AvatarImage src={patient.photoUrl} alt={patient.fullName} />
            )}
            <AvatarFallback className="text-lg bg-primary/10 text-primary">
              {getInitials(patient.fullName)}
            </AvatarFallback>
          </Avatar>
          <button
            type="button"
            className="absolute inset-0 flex items-center justify-center bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer"
            onClick={() => fileInputRef.current?.click()}
          >
            <IconCamera className="h-5 w-5 text-white" />
          </button>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={handlePhotoUpload}
          />
        </div>

        {/* Patient Info */}
        <div className="flex-1 min-w-0 space-y-1">
          <div className="flex items-center gap-2 flex-wrap">
            <h1 className="text-2xl font-semibold tracking-tight">
              {patient.fullName}
            </h1>
            <Badge
              variant={
                patient.patientType === "Medical" ? "default" : "secondary"
              }
            >
              {patient.patientType === "Medical"
                ? t("medicalPatient")
                : t("walkInCustomer")}
            </Badge>
            {!patient.isActive && (
              <Badge variant="outline" className="text-destructive border-destructive">
                {t("inactive")}
              </Badge>
            )}
          </div>

          {patient.patientCode && (
            <p className="text-sm font-mono text-muted-foreground">
              {patient.patientCode}
            </p>
          )}

          <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-sm text-muted-foreground">
            {patient.dateOfBirth && (
              <span>
                {format(new Date(patient.dateOfBirth), dateFormat, { locale })}{" "}
                ({calculateAge(patient.dateOfBirth)} {t("age")})
              </span>
            )}
            {patient.gender && (
              <span>
                {patient.gender === "Male"
                  ? t("male")
                  : patient.gender === "Female"
                    ? t("female")
                    : t("other")}
              </span>
            )}
            {patient.phone && <span>{patient.phone}</span>}
          </div>
        </div>

        {/* Actions */}
        <div className="flex items-center gap-2 shrink-0">
          <Button variant="outline" size="sm" onClick={onEdit}>
            <IconEdit className="h-4 w-4 mr-1" />
            {t("edit")}
          </Button>

          <AlertDialog open={confirmOpen} onOpenChange={setConfirmOpen}>
            <AlertDialogTrigger asChild>
              {patient.isActive ? (
                <Button variant="outline" size="sm" className="text-destructive">
                  <IconUserOff className="h-4 w-4 mr-1" />
                  {t("deactivate")}
                </Button>
              ) : (
                <Button variant="outline" size="sm">
                  <IconUserCheck className="h-4 w-4 mr-1" />
                  {t("reactivate")}
                </Button>
              )}
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>
                  {patient.isActive ? t("deactivate") : t("reactivate")}
                </AlertDialogTitle>
                <AlertDialogDescription>
                  {patient.isActive
                    ? t("confirmDeactivate")
                    : t("confirmReactivate")}
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>{tCommon("buttons.cancel")}</AlertDialogCancel>
                <AlertDialogAction
                  onClick={patient.isActive ? handleDeactivate : handleReactivate}
                >
                  {tCommon("buttons.confirm")}
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      </div>

      {/* Allergy Alert Banner */}
      <AllergyAlert allergies={patient.allergies} />
    </div>
  )
}
