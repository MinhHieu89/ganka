import { useState } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import {
  IconPlus,
  IconTrash,
  IconAlertTriangle,
} from "@tabler/icons-react"
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
import {
  useRemoveAllergy,
  type PatientDto,
  type AllergySeverity,
} from "@/features/patient/api/patient-api"
import { AllergyForm } from "@/features/patient/components/AllergyForm"

interface PatientAllergyTabProps {
  patient: PatientDto
}

const severityBadgeClass: Record<AllergySeverity, string> = {
  Severe: "bg-red-500 text-white hover:bg-red-500",
  Moderate: "bg-orange-500 text-white hover:bg-orange-500",
  Mild: "bg-muted text-muted-foreground hover:bg-muted",
}

export function PatientAllergyTab({ patient }: PatientAllergyTabProps) {
  const { t } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const [addOpen, setAddOpen] = useState(false)
  const removeAllergyMutation = useRemoveAllergy()

  const handleRemove = async (allergyId: string) => {
    try {
      await removeAllergyMutation.mutateAsync({
        patientId: patient.id,
        allergyId,
      })
      toast.success(t("removeAllergy"))
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : tCommon("status.error"),
      )
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="font-medium">{t("allergies")}</h3>
        <Button size="sm" onClick={() => setAddOpen(true)}>
          <IconPlus className="h-4 w-4 mr-1" />
          {t("addAllergy")}
        </Button>
      </div>

      {patient.allergies.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center border border-dashed">
          <IconAlertTriangle className="h-8 w-8 text-muted-foreground/40 mb-3" />
          <p className="text-sm text-muted-foreground">{t("noAllergies")}</p>
        </div>
      ) : (
        <div className="space-y-2">
          {patient.allergies.map((allergy) => (
            <div
              key={allergy.id}
              className="flex items-center justify-between p-3 border"
            >
              <div className="flex items-center gap-3">
                <span className="font-medium text-sm">{allergy.name}</span>
                <Badge
                  variant="outline"
                  className={severityBadgeClass[allergy.severity]}
                >
                  {t(allergy.severity.toLowerCase())}
                </Badge>
              </div>

              <AlertDialog>
                <AlertDialogTrigger asChild>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-destructive hover:text-destructive"
                  >
                    <IconTrash className="h-4 w-4" />
                  </Button>
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>{t("removeAllergy")}</AlertDialogTitle>
                    <AlertDialogDescription>
                      {t("confirmRemoveAllergy")}
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>
                      {tCommon("buttons.cancel")}
                    </AlertDialogCancel>
                    <AlertDialogAction onClick={() => handleRemove(allergy.id)}>
                      {tCommon("buttons.confirm")}
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            </div>
          ))}
        </div>
      )}

      <AllergyForm
        open={addOpen}
        onClose={() => setAddOpen(false)}
        patientId={patient.id}
      />
    </div>
  )
}
