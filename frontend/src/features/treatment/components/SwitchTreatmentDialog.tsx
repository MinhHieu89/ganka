import { useEffect, useMemo, useState } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import { useNavigate } from "@tanstack/react-router"
import { IconLoader2, IconArrowRight, IconAlertTriangle } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Textarea } from "@/shared/components/Textarea"
import { Badge } from "@/shared/components/Badge"
import { Card, CardContent } from "@/shared/components/Card"
import { Separator } from "@/shared/components/Separator"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import {
  useProtocolTemplates,
  useSwitchTreatmentType,
} from "@/features/treatment/api/treatment-api"
import type { TreatmentPackageDto } from "@/features/treatment/api/treatment-types"

// -- Schema --

const switchSchema = z.object({
  newProtocolTemplateId: z.string().min(1, "Vui lòng chọn phác đồ mới"),
  reason: z.string().min(1, "Lý do chuyển đổi là bắt buộc"),
})

type SwitchFormValues = z.infer<typeof switchSchema>

// -- Props --

interface SwitchTreatmentDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  currentPackage: TreatmentPackageDto
}

// -- Component --

export function SwitchTreatmentDialog({
  open,
  onOpenChange,
  currentPackage,
}: SwitchTreatmentDialogProps) {
  const navigate = useNavigate()
  const { data: templates = [] } = useProtocolTemplates()
  const switchMutation = useSwitchTreatmentType()
  const [selectedTemplateName, setSelectedTemplateName] = useState<string | null>(null)

  const form = useForm<SwitchFormValues>({
    resolver: zodResolver(switchSchema),
    defaultValues: {
      newProtocolTemplateId: "",
      reason: "",
    },
  })

  // Reset form when dialog opens
  useEffect(() => {
    if (open) {
      form.reset({
        newProtocolTemplateId: "",
        reason: "",
      })
      setSelectedTemplateName(null)
    }
  }, [open, form])

  // Filter templates to exclude current treatment type
  const availableTemplates = useMemo(() => {
    return templates.filter(
      (t) => t.isActive && t.treatmentType !== currentPackage.treatmentType,
    )
  }, [templates, currentPackage.treatmentType])

  const handleTemplateChange = (templateId: string) => {
    const template = templates.find((t) => t.id === templateId)
    setSelectedTemplateName(template?.name ?? null)
    form.setValue("newProtocolTemplateId", templateId)
  }

  const handleSubmit = async (data: SwitchFormValues) => {
    try {
      const newPackage = await switchMutation.mutateAsync({
        packageId: currentPackage.id,
        newProtocolTemplateId: data.newProtocolTemplateId,
        reason: data.reason,
      })
      toast.success("Chuyển đổi loại điều trị thành công")
      onOpenChange(false)
      // Navigate to the new package
      navigate({
        to: "/treatments/$packageId",
        params: { packageId: newPackage.id },
      })
    } catch (error) {
      handleServerValidationError(error, form.setError)
    }
  }

  const isSubmitting = switchMutation.isPending

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Chuyển đổi loại điều trị</DialogTitle>
          <DialogDescription>
            Phác đồ hiện tại sẽ được đánh dấu &ldquo;Switched&rdquo; và phác đồ
            mới sẽ được tạo với số phiên còn lại.
          </DialogDescription>
        </DialogHeader>

        {/* Current package summary */}
        <Card className="border-amber-200 dark:border-amber-800">
          <CardContent className="pt-4 pb-3 space-y-2">
            <div className="flex items-center gap-2 text-sm font-medium text-amber-700 dark:text-amber-400">
              <IconAlertTriangle className="h-4 w-4" />
              Phác đồ hiện tại
            </div>
            <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
              <div>
                <span className="text-muted-foreground">Loại:</span>{" "}
                <Badge variant="outline">{currentPackage.treatmentType}</Badge>
              </div>
              <div>
                <span className="text-muted-foreground">Trạng thái:</span>{" "}
                <Badge variant="outline">{currentPackage.status}</Badge>
              </div>
              <div>
                <span className="text-muted-foreground">Đã hoàn thành:</span>{" "}
                {currentPackage.sessionsCompleted}/{currentPackage.totalSessions} phiên
              </div>
              <div>
                <span className="text-muted-foreground">Còn lại:</span>{" "}
                {currentPackage.sessionsRemaining} phiên
              </div>
            </div>
          </CardContent>
        </Card>

        <Separator />

        <form
          onSubmit={form.handleSubmit(handleSubmit)}
          className="space-y-4"
        >
          {/* New protocol template select */}
          <Controller
            name="newProtocolTemplateId"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor="newProtocolTemplateId">
                  Phác đồ mới
                </FieldLabel>
                <Select
                  value={field.value}
                  onValueChange={(v) => {
                    field.onChange(v)
                    handleTemplateChange(v)
                  }}
                >
                  <SelectTrigger id="newProtocolTemplateId">
                    <SelectValue placeholder="Chọn phác đồ mới..." />
                  </SelectTrigger>
                  <SelectContent>
                    {availableTemplates.map((t) => (
                      <SelectItem key={t.id} value={t.id}>
                        {t.name} ({t.treatmentType})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Preview section */}
          {selectedTemplateName && (
            <Card className="bg-muted/50">
              <CardContent className="pt-4 pb-3">
                <div className="text-sm font-medium mb-2">Kết quả dự kiến</div>
                <div className="space-y-1.5 text-sm">
                  <div className="flex items-center gap-2">
                    <Badge variant="outline">
                      {currentPackage.treatmentType}
                    </Badge>
                    <IconArrowRight className="h-3.5 w-3.5 text-muted-foreground" />
                    <Badge variant="outline" className="border-primary text-primary">
                      {selectedTemplateName}
                    </Badge>
                  </div>
                  <div className="text-muted-foreground">
                    Phác đồ hiện tại ({currentPackage.protocolTemplateName}) sẽ được
                    đánh dấu &ldquo;Switched&rdquo;
                  </div>
                  <div className="text-muted-foreground">
                    Phác đồ mới ({selectedTemplateName}) sẽ được tạo với{" "}
                    {currentPackage.sessionsRemaining} phiên còn lại
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Reason - required */}
          <Controller
            name="reason"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor="reason">
                  Lý do chuyển đổi *
                </FieldLabel>
                <Textarea
                  {...field}
                  id="reason"
                  rows={2}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Huỷ
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              Chuyển đổi
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
