import { useState, useMemo } from "react"
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
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/components/ui/alert-dialog"
import { Input } from "@/shared/components/ui/input"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Button } from "@/shared/components/ui/button"
import { Field, FieldLabel, FieldError } from "@/shared/components/ui/field"
import { useCloseShift } from "../api/shift-api"
import { formatVND } from "@/shared/lib/format-vnd"
import { cn } from "@/shared/lib/utils"

const closeShiftSchema = z.object({
  actualCashCount: z.coerce
    .number({ invalid_type_error: "Vui long nhap so" })
    .min(0, "So tien phai >= 0"),
  managerNote: z.string().optional(),
})

type CloseShiftFormValues = z.infer<typeof closeShiftSchema>

interface ShiftCloseDialogProps {
  shiftId: string
  expectedCash: number
  onSuccess?: () => void
  children?: React.ReactNode
}

export function ShiftCloseDialog({
  shiftId,
  expectedCash,
  onSuccess,
  children,
}: ShiftCloseDialogProps) {
  const { t } = useTranslation("billing")
  const [open, setOpen] = useState(false)
  const closeShiftMutation = useCloseShift()

  const {
    control,
    handleSubmit,
    watch,
    reset,
    formState: { errors },
  } = useForm<CloseShiftFormValues>({
    resolver: zodResolver(closeShiftSchema),
    defaultValues: {
      actualCashCount: 0,
      managerNote: "",
    },
  })

  const actualCashCount = watch("actualCashCount")

  const discrepancy = useMemo(() => {
    const actual = typeof actualCashCount === "number" ? actualCashCount : 0
    return actual - expectedCash
  }, [actualCashCount, expectedCash])

  const discrepancyLabel = useMemo(() => {
    if (discrepancy === 0) return t("matches")
    if (discrepancy > 0) return `${t("surplus")} ${formatVND(discrepancy)}`
    return `${t("deficit")} ${formatVND(Math.abs(discrepancy))}`
  }, [discrepancy, t])

  const discrepancyColor = useMemo(() => {
    if (discrepancy === 0) return "text-green-600"
    if (discrepancy > 0) return "text-green-600"
    return "text-red-600"
  }, [discrepancy])

  const hasDiscrepancy = discrepancy !== 0

  const doClose = (values: CloseShiftFormValues) => {
    closeShiftMutation.mutate(
      {
        shiftId,
        actualCashCount: values.actualCashCount,
        managerNote: values.managerNote || null,
      },
      {
        onSuccess: () => {
          toast.success(t("shiftClosed"))
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
        {children ?? (
          <Button variant="destructive">{t("closeShift")}</Button>
        )}
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("closeShift")}</DialogTitle>
          <DialogDescription>
            {t("expectedCash")}: {formatVND(expectedCash)}
          </DialogDescription>
        </DialogHeader>

        <form
          id="close-shift-form"
          onSubmit={handleSubmit((values) => {
            // If there is no discrepancy, close immediately.
            // If there is discrepancy, the AlertDialog confirmation will handle it.
            if (!hasDiscrepancy) {
              doClose(values)
            }
          })}
          className="space-y-4"
        >
          {/* Expected cash display */}
          <div className="rounded-lg border bg-muted/50 p-4 text-center">
            <p className="text-sm text-muted-foreground">{t("expectedCash")}</p>
            <p className="text-2xl font-bold">{formatVND(expectedCash)}</p>
          </div>

          <Field data-invalid={!!errors.actualCashCount}>
            <FieldLabel>{t("actualCashCount")}</FieldLabel>
            <Controller
              control={control}
              name="actualCashCount"
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
            {errors.actualCashCount && (
              <FieldError>{errors.actualCashCount.message}</FieldError>
            )}
          </Field>

          {/* Discrepancy display */}
          <div
            className={cn(
              "rounded-lg border p-3 text-center font-semibold text-lg",
              discrepancyColor,
            )}
          >
            {discrepancyLabel}
          </div>

          {/* Manager note (highlighted when discrepancy exists) */}
          <Field>
            <FieldLabel
              className={cn(hasDiscrepancy && "text-yellow-600 font-semibold")}
            >
              {t("managerNote")}
              {hasDiscrepancy && " *"}
            </FieldLabel>
            <Controller
              control={control}
              name="managerNote"
              render={({ field }) => (
                <AutoResizeTextarea
                  className={cn(
                    hasDiscrepancy && "border-yellow-400 bg-yellow-50/50",
                  )}
                  rows={3}
                  {...field}
                />
              )}
            />
          </Field>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => setOpen(false)}
            >
              {t("buttons.cancel", { ns: "common" })}
            </Button>

            {hasDiscrepancy ? (
              <AlertDialog>
                <AlertDialogTrigger asChild>
                  <Button
                    type="button"
                    variant="destructive"
                    disabled={closeShiftMutation.isPending}
                  >
                    {closeShiftMutation.isPending ? "..." : t("closeShift")}
                  </Button>
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>{t("closeShift")}</AlertDialogTitle>
                    <AlertDialogDescription>
                      {t("confirmClose")}
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>
                      {t("buttons.cancel", { ns: "common" })}
                    </AlertDialogCancel>
                    <AlertDialogAction
                      onClick={handleSubmit(doClose)}
                      disabled={closeShiftMutation.isPending}
                    >
                      {t("closeShift")}
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            ) : (
              <Button
                type="submit"
                variant="destructive"
                disabled={closeShiftMutation.isPending}
              >
                {closeShiftMutation.isPending ? "..." : t("closeShift")}
              </Button>
            )}
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
