import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconLoader2 } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Textarea } from "@/shared/components/Textarea"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { useAmendVisit } from "../api/clinical-api"

interface AmendmentDialogProps {
  visitId: string
  open: boolean
  onClose: () => void
}

export function AmendmentDialog({ visitId, open, onClose }: AmendmentDialogProps) {
  const { t } = useTranslation("clinical")
  const { t: tCommon } = useTranslation("common")
  const amendMutation = useAmendVisit()

  const schema = z.object({
    reason: z.string().min(10, t("visit.amendReasonMin")),
  })

  type FormValues = z.infer<typeof schema>

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { reason: "" },
  })

  const handleSubmit = async (data: FormValues) => {
    try {
      await amendMutation.mutateAsync({
        visitId,
        reason: data.reason,
        fieldChangesJson: "[]",
      })
      toast.success(t("visit.amendmentCreated"))
      form.reset()
      onClose()
    } catch (err) {
      toast.error(
        err instanceof Error ? err.message : tCommon("status.error"),
      )
    }
  }

  const handleOpenChange = (v: boolean) => {
    if (!v) {
      onClose()
      form.reset()
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("visit.amend")}</DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          <Field data-invalid={form.formState.errors.reason ? true : undefined}>
            <FieldLabel>{t("visit.amendReason")}</FieldLabel>
            <Textarea
              className="min-h-[100px]"
              {...form.register("reason")}
              aria-invalid={!!form.formState.errors.reason}
            />
            {form.formState.errors.reason && (
              <FieldError>{form.formState.errors.reason.message}</FieldError>
            )}
          </Field>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={amendMutation.isPending}>
              {amendMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {t("visit.amendConfirm")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
