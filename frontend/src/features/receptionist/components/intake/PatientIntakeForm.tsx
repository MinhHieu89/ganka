import { useState } from "react"
import { useForm, FormProvider } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useNavigate } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconLoader2 } from "@tabler/icons-react"
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
  intakeFormSchema,
  type IntakeFormValues,
} from "@/features/receptionist/schemas/intake-form.schema"
import {
  useRegisterFromIntakeMutation,
  useUpdateFromIntakeMutation,
  useCheckInMutation,
  useCreateWalkInVisitMutation,
  useAdvanceStageMutation,
} from "@/features/receptionist/api/receptionist-api"
import { PersonalInfoSection } from "./PersonalInfoSection"
import { ExamInfoSection } from "./ExamInfoSection"
import { MedicalHistorySection } from "./MedicalHistorySection"
import { LifestyleSection } from "./LifestyleSection"

interface PatientIntakeFormProps {
  patientId?: string
  defaultValues?: Partial<IntakeFormValues>
  mode: "create" | "edit"
  appointmentId?: string
}

export function PatientIntakeForm({
  patientId,
  defaultValues,
  mode,
  appointmentId,
}: PatientIntakeFormProps) {
  const navigate = useNavigate()
  const { t } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const [isSaving, setIsSaving] = useState(false)
  const [isSavingAndAdvancing, setIsSavingAndAdvancing] = useState(false)

  const registerMutation = useRegisterFromIntakeMutation()
  const updateMutation = useUpdateFromIntakeMutation()
  const checkInMutation = useCheckInMutation()
  const createWalkInVisit = useCreateWalkInVisitMutation()
  const advanceStage = useAdvanceStageMutation()

  const form = useForm<IntakeFormValues>({
    resolver: zodResolver(intakeFormSchema),
    defaultValues: {
      fullName: "",
      phone: "",
      dateOfBirth: "",
      gender: "",
      address: "",
      cccd: "",
      email: "",
      occupation: "",
      reason: "",
      ocularHistory: "",
      systemicHistory: "",
      currentMedications: "",
      allergies: "",
      screenTimeHours: undefined,
      workEnvironment: undefined,
      contactLensUsage: undefined,
      lifestyleNotes: "",
      ...defaultValues,
    },
  })

  const handleSaveOnly = async (data: IntakeFormValues) => {
    setIsSaving(true)
    try {
      if (mode === "edit" && patientId) {
        await updateMutation.mutateAsync({ patientId, ...data })
        toast.success(t("intake.updateSuccess", { name: data.fullName }))
      } else {
        await registerMutation.mutateAsync({ ...data, appointmentId })
        // Check in the appointment after patient registration
        if (appointmentId) {
          await checkInMutation.mutateAsync(appointmentId)
        }
        toast.success(t("intake.saveSuccess", { name: data.fullName }))
      }
      navigate({ to: "/dashboard" })
    } catch {
      toast.error(t("intake.saveError"))
    } finally {
      setIsSaving(false)
    }
  }

  const handleSaveAndAdvance = async (data: IntakeFormValues) => {
    setIsSavingAndAdvancing(true)
    try {
      let newPatientId: string

      if (mode === "edit" && patientId) {
        await updateMutation.mutateAsync({ patientId, ...data })
        newPatientId = patientId
      } else {
        const result = await registerMutation.mutateAsync({ ...data, appointmentId })
        newPatientId = result.id
      }

      if (appointmentId) {
        // Check in the appointment — this creates a visit via domain event
        await checkInMutation.mutateAsync(appointmentId)
      } else {
        // Walk-in: create visit manually
        const visitResult = await createWalkInVisit.mutateAsync({
          patientId: newPatientId,
          reason: data.reason,
        })

        await advanceStage.mutateAsync({
          visitId: visitResult.id,
          newStage: 1,
        })
      }

      toast.success(t("intake.advanceSuccess", { name: data.fullName }))
      navigate({ to: "/dashboard" })
    } catch {
      toast.error(t("intake.saveError"))
    } finally {
      setIsSavingAndAdvancing(false)
    }
  }

  const isSubmitting = isSaving || isSavingAndAdvancing

  return (
    <div className="flex flex-col gap-6 p-6">
      <FormProvider {...form}>
        <form className="flex flex-col gap-6">
          <PersonalInfoSection patientId={patientId} />
          <ExamInfoSection />
          <MedicalHistorySection />
          <LifestyleSection />

          {/* Footer buttons */}
          <div className="flex items-center justify-between border-t pt-4">
            {/* Left: Cancel */}
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button type="button" variant="outline" disabled={isSubmitting}>
                  {t("intake.cancelButton")}
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>{t("intake.cancelConfirmTitle")}</AlertDialogTitle>
                  <AlertDialogDescription>
                    {t("intake.cancelConfirmMessage")}
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>{tCommon("buttons.back")}</AlertDialogCancel>
                  <AlertDialogAction
                    onClick={() => navigate({ to: "/dashboard" })}
                  >
                    {t("intake.cancelButton")}
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>

            {/* Right: Save buttons */}
            <div className="flex items-center gap-2">
              <Button
                type="button"
                variant="outline"
                disabled={isSubmitting}
                onClick={form.handleSubmit(handleSaveOnly)}
              >
                {isSaving && (
                  <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                {t("intake.saveOnly")}
              </Button>
              <Button
                type="button"
                disabled={isSubmitting}
                onClick={form.handleSubmit(handleSaveAndAdvance)}
              >
                {isSavingAndAdvancing && (
                  <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                {t("intake.saveAndAdvance")} &rarr;
              </Button>
            </div>
          </div>
        </form>
      </FormProvider>
    </div>
  )
}
