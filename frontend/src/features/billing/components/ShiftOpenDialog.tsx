import { useState } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  DialogTrigger,
} from "@/shared/components/ui/dialog"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select"
import { Input } from "@/shared/components/ui/input"
import { Button } from "@/shared/components/ui/button"
import { Field, FieldLabel, FieldError } from "@/shared/components/ui/field"
import { useOpenShift, useShiftTemplates } from "../api/shift-api"

function createOpenShiftSchema(t: (key: string) => string) {
  return z.object({
    shiftTemplateId: z.string().nullable().optional(),
    openingBalance: z.coerce
      .number({ invalid_type_error: t("validation.enterNumber") })
      .min(0, t("validation.balanceMinZero")),
  })
}

type OpenShiftFormValues = z.infer<ReturnType<typeof createOpenShiftSchema>>

interface ShiftOpenDialogProps {
  onSuccess?: () => void
  lastClosingBalance?: number | null
  children?: React.ReactNode
}

export function ShiftOpenDialog({
  onSuccess,
  lastClosingBalance,
  children,
}: ShiftOpenDialogProps) {
  const { t } = useTranslation("billing")
  const [open, setOpen] = useState(false)
  const { data: templates } = useShiftTemplates()
  const openShiftMutation = useOpenShift()

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<OpenShiftFormValues>({
    resolver: zodResolver(createOpenShiftSchema(t)),
    defaultValues: {
      shiftTemplateId: null,
      openingBalance: lastClosingBalance ?? 0,
    },
  })

  const onSubmit = (values: OpenShiftFormValues) => {
    openShiftMutation.mutate(
      {
        shiftTemplateId: values.shiftTemplateId || null,
        openingBalance: values.openingBalance,
      },
      {
        onSuccess: () => {
          toast.success(t("shiftOpened"))
          setOpen(false)
          reset()
          onSuccess?.()
        },
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        {children ?? <Button>{t("openShift")}</Button>}
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("openShift")}</DialogTitle>
          <DialogDescription>
            {t("openingBalance")}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Field>
            <FieldLabel>{t("shiftTemplate")}</FieldLabel>
            <Controller
              control={control}
              name="shiftTemplateId"
              render={({ field }) => (
                <Select
                  value={field.value ?? "none"}
                  onValueChange={(v) =>
                    field.onChange(v === "none" ? null : v)
                  }
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">--</SelectItem>
                    {templates
                      ?.filter((t) => t.isActive)
                      .map((tmpl) => (
                        <SelectItem key={tmpl.id} value={tmpl.id}>
                          {tmpl.nameVi ?? tmpl.name}
                        </SelectItem>
                      ))}
                  </SelectContent>
                </Select>
              )}
            />
          </Field>

          <Field data-invalid={!!errors.openingBalance}>
            <FieldLabel required>{t("openingBalance")}</FieldLabel>
            <Controller
              control={control}
              name="openingBalance"
              render={({ field }) => (
                <Input
                  type="number"
                  min={0}
                  step={1000}
                  {...field}
                  onChange={(e) => field.onChange(e.target.valueAsNumber || 0)}
                />
              )}
            />
            {errors.openingBalance && (
              <FieldError>{errors.openingBalance.message}</FieldError>
            )}
          </Field>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => setOpen(false)}
            >
              {t("buttons.cancel", { ns: "common" })}
            </Button>
            <Button type="submit" disabled={openShiftMutation.isPending}>
              {openShiftMutation.isPending ? "..." : t("openShift")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
