import { useState } from "react"
import { useForm, FormProvider } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useNavigate } from "@tanstack/react-router"
import { toast } from "sonner"
import { IconLoader2 } from "@tabler/icons-react"
import {
  Breadcrumb,
  BreadcrumbList,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbSeparator,
  BreadcrumbPage,
} from "@/shared/components/Breadcrumb"
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
}

export function PatientIntakeForm({
  patientId,
  defaultValues,
  mode,
}: PatientIntakeFormProps) {
  const navigate = useNavigate()
  const [isSaving, setIsSaving] = useState(false)
  const [isSavingAndAdvancing, setIsSavingAndAdvancing] = useState(false)

  const registerMutation = useRegisterFromIntakeMutation()
  const updateMutation = useUpdateFromIntakeMutation()
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
        toast.success(`Da cap nhat ho so cho ${data.fullName}`)
      } else {
        await registerMutation.mutateAsync(data)
        toast.success(`Da tao ho so cho ${data.fullName}`)
      }
      navigate({ to: "/dashboard" })
    } catch {
      toast.error("Luu that bai, vui long thu lai")
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
        const result = await registerMutation.mutateAsync(data)
        newPatientId = result.id
      }

      // Create walk-in visit
      const visitResult = await createWalkInVisit.mutateAsync({
        patientId: newPatientId,
        reason: data.reason,
      })

      // Advance to PreExam stage (stage 1)
      await advanceStage.mutateAsync({
        visitId: visitResult.id,
        newStage: 1,
      })

      toast.success(`Da tao ho so va chuyen tien kham cho ${data.fullName}`)
      navigate({ to: "/dashboard" })
    } catch {
      toast.error("Luu that bai, vui long thu lai")
    } finally {
      setIsSavingAndAdvancing(false)
    }
  }

  const isSubmitting = isSaving || isSavingAndAdvancing

  return (
    <div className="flex flex-col gap-6 p-6">
      {/* Breadcrumb */}
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem>
            <BreadcrumbLink href="/dashboard">Dashboard</BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbPage>
              {mode === "edit"
                ? "Cap nhat ho so"
                : "Tiep nhan benh nhan moi"}
            </BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      {/* Form */}
      <FormProvider {...form}>
        <form className="flex flex-col gap-6">
          <PersonalInfoSection />
          <ExamInfoSection />
          <MedicalHistorySection />
          <LifestyleSection />

          {/* Footer buttons */}
          <div className="flex items-center justify-between border-t pt-4">
            {/* Left: Cancel */}
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button type="button" variant="outline" disabled={isSubmitting}>
                  Huy nhap lieu
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Xac nhan huy</AlertDialogTitle>
                  <AlertDialogDescription>
                    Ban co chac muon huy? Du lieu chua luu se mat.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Quay lai</AlertDialogCancel>
                  <AlertDialogAction
                    onClick={() => navigate({ to: "/dashboard" })}
                  >
                    Huy nhap lieu
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
                Luu ho so
              </Button>
              <Button
                type="button"
                disabled={isSubmitting}
                onClick={form.handleSubmit(handleSaveAndAdvance)}
              >
                {isSavingAndAdvancing && (
                  <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                Luu & Chuyen tien kham
              </Button>
            </div>
          </div>
        </form>
      </FormProvider>
    </div>
  )
}
