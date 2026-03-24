import { useState, useMemo, useCallback, useEffect } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { createValidationMessages } from "@/shared/lib/validation"
import { toast } from "sonner"
import { format } from "date-fns"
import { useTranslation } from "react-i18next"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  getPaginationRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { IconCheck, IconX, IconLoader2 } from "@tabler/icons-react"
import { Link } from "@tanstack/react-router"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Input } from "@/shared/components/Input"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Skeleton } from "@/shared/components/Skeleton"
import { DataTable } from "@/shared/components/DataTable"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { formatVND } from "@/shared/lib/format-vnd"
import { useAuthStore } from "@/shared/stores/authStore"
import {
  usePendingCancellations,
  useApproveCancellation,
  useRejectCancellation,
} from "@/features/treatment/api/treatment-api"
import type { TreatmentPackageDto } from "@/features/treatment/api/treatment-types"

// -- Treatment type badge styles --

const TREATMENT_TYPE_STYLES: Record<string, string> = {
  IPL: "border-violet-500 text-violet-700 dark:text-violet-400",
  LLLT: "border-blue-500 text-blue-700 dark:text-blue-400",
  LidCare: "border-emerald-500 text-emerald-700 dark:text-emerald-400",
}

// -- Approve schema --

function createApproveSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    deductionPercent: z.coerce
      .number()
      .min(0, v.mustBeNonNegative)
      .max(100, v.percentMax100),
  })
}

type ApproveFormValues = z.infer<ReturnType<typeof createApproveSchema>>

// -- Reject schema --

function createRejectSchema(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    rejectionReason: z.string().min(1, v.reasonRequired),
  })
}

type RejectFormValues = z.infer<ReturnType<typeof createRejectSchema>>

// -- Helpers --

function computeRefundAmount(pkg: TreatmentPackageDto, deductionPercent: number): number {
  const totalCost =
    pkg.pricingMode === "PerPackage"
      ? pkg.packagePrice
      : pkg.sessionPrice * pkg.totalSessions

  const remainingRatio =
    pkg.totalSessions > 0 ? pkg.sessionsRemaining / pkg.totalSessions : 0
  const remainingValue = totalCost * remainingRatio
  const deduction = remainingValue * (deductionPercent / 100)

  return Math.round(remainingValue - deduction)
}

function getDefaultDeductionPercent(pkg: TreatmentPackageDto): number {
  return pkg.cancellationRequest?.deductionPercent ?? 15
}

// -- Column helper --

const columnHelper = createColumnHelper<TreatmentPackageDto>()

// -- Approve Dialog --

interface ApproveDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  pkg: TreatmentPackageDto | null
}

function ApproveDialog({ open, onOpenChange, pkg }: ApproveDialogProps) {
  const { t } = useTranslation("treatment")
  const { t: tCommon } = useTranslation("common")
  const user = useAuthStore((s) => s.user)
  const approveMutation = useApproveCancellation()

  const defaultDeduction = pkg ? getDefaultDeductionPercent(pkg) : 15

  const approveSchema = useMemo(() => createApproveSchema(tCommon), [tCommon])
  const form = useForm<ApproveFormValues>({
    resolver: zodResolver(approveSchema),
    defaultValues: {
      deductionPercent: defaultDeduction,
    },
  })

  // Reset form when dialog opens with new package
  const watchDeduction = form.watch("deductionPercent")
  const refundAmount = pkg ? computeRefundAmount(pkg, watchDeduction || 0) : 0

  const handleSubmit = async (data: ApproveFormValues) => {
    if (!pkg || !user) return

    try {
      await approveMutation.mutateAsync({
        packageId: pkg.id,
        managerId: user.id,
        deductionPercent: data.deductionPercent,
      })
      toast.success(t("approvalQueue.approveSuccess"))
      onOpenChange(false)
      form.reset()
    } catch {
      toast.error(t("approvalQueue.approveError"))
    }
  }

  // Reset form values when pkg changes
  const handleOpenChange = useCallback(
    (isOpen: boolean) => {
      if (isOpen && pkg) {
        form.reset({
          deductionPercent: getDefaultDeductionPercent(pkg),
        })
      }
      onOpenChange(isOpen)
    },
    [form, onOpenChange, pkg],
  )

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("approvalQueue.approveDialogTitle")}</DialogTitle>
          <DialogDescription>
            {t("approvalQueue.approveDialogDescription")}{" "}
            <span className="font-medium">{pkg?.patientName}</span>
          </DialogDescription>
        </DialogHeader>

        {pkg && (
          <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
            {/* Package summary */}
            <div className="rounded-md border p-3 space-y-1 text-sm">
              <div>
                <span className="text-muted-foreground">{t("approvalQueue.protocol")}:</span>{" "}
                {pkg.protocolTemplateName}
              </div>
              <div>
                <span className="text-muted-foreground">{t("approvalQueue.type")}:</span>{" "}
                <Badge variant="outline" className={`text-xs ${TREATMENT_TYPE_STYLES[pkg.treatmentType] ?? ""}`}>
                  {pkg.treatmentType}
                </Badge>
              </div>
              <div>
                <span className="text-muted-foreground">{t("approvalQueue.progress")}:</span>{" "}
                {pkg.sessionsCompleted}/{pkg.totalSessions} {t("approvalQueue.sessions")}
              </div>
              <div>
                <span className="text-muted-foreground">{t("approvalQueue.cancelReason")}:</span>{" "}
                {pkg.cancellationRequest?.reason ?? "--"}
              </div>
            </div>

            {/* Deduction Percent */}
            <Controller
              name="deductionPercent"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>
                    {t("approvalQueue.deductionPercentLabel")}
                  </FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="number"
                    min={0}
                    max={100}
                    step={1}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{fieldState.error.message}</FieldError>
                  )}
                </Field>
              )}
            />

            {/* Calculated refund amount */}
            <div className="rounded-md bg-muted p-3 text-sm">
              <span className="text-muted-foreground">{t("approvalQueue.estimatedRefund")}:</span>{" "}
              <span className="font-semibold text-base">{formatVND(refundAmount)}</span>
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                disabled={approveMutation.isPending}
              >
                {t("approvalQueue.cancel")}
              </Button>
              <Button type="submit" disabled={approveMutation.isPending}>
                {approveMutation.isPending && (
                  <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                {t("approvalQueue.approveButton")}
              </Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  )
}

// -- Reject Dialog --

interface RejectDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  pkg: TreatmentPackageDto | null
}

function RejectDialog({ open, onOpenChange, pkg }: RejectDialogProps) {
  const { t } = useTranslation("treatment")
  const { t: tCommon } = useTranslation("common")
  const user = useAuthStore((s) => s.user)
  const rejectMutation = useRejectCancellation()

  const rejectSchema = useMemo(() => createRejectSchema(tCommon), [tCommon])
  const form = useForm<RejectFormValues>({
    resolver: zodResolver(rejectSchema),
    defaultValues: {
      rejectionReason: "",
    },
  })

  const handleSubmit = async (data: RejectFormValues) => {
    if (!pkg || !user) return

    try {
      await rejectMutation.mutateAsync({
        packageId: pkg.id,
        managerId: user.id,
        rejectionReason: data.rejectionReason,
      })
      toast.success(t("approvalQueue.rejectSuccess"))
      onOpenChange(false)
      form.reset()
    } catch {
      toast.error(t("approvalQueue.rejectError"))
    }
  }

  const handleOpenChange = useCallback(
    (isOpen: boolean) => {
      if (isOpen) {
        form.reset({ rejectionReason: "" })
      }
      onOpenChange(isOpen)
    },
    [form, onOpenChange],
  )

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("approvalQueue.rejectDialogTitle")}</DialogTitle>
          <DialogDescription>
            {t("approvalQueue.rejectDialogDescription")}{" "}
            <span className="font-medium">{pkg?.patientName}</span>
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          <Controller
            name="rejectionReason"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>{t("approvalQueue.rejectionReasonLabel")}</FieldLabel>
                <AutoResizeTextarea
                  {...field}
                  id={field.name}
                  rows={3}
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
              disabled={rejectMutation.isPending}
            >
              {t("approvalQueue.cancel")}
            </Button>
            <Button
              type="submit"
              variant="destructive"
              disabled={rejectMutation.isPending}
            >
              {rejectMutation.isPending && (
                <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              {t("approvalQueue.rejectButton")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// -- Main component --

export function CancellationApprovalQueue() {
  const { t } = useTranslation("treatment")
  const { data: packages = [], isLoading, isError } = usePendingCancellations()
  const canManage = useAuthStore(
    (s) => s.user?.permissions?.includes("Treatment.Manage") || s.user?.permissions?.includes("Admin"),
  )
  const [sorting, setSorting] = useState<SortingState>([])
  const [approveTarget, setApproveTarget] = useState<TreatmentPackageDto | null>(null)
  const [rejectTarget, setRejectTarget] = useState<TreatmentPackageDto | null>(null)

  const columns = useMemo(
    () => [
      columnHelper.accessor("patientName", {
        header: t("approvalQueue.columns.patient"),
        cell: (info) => (
          <Link
            to="/patients/$patientId"
            params={{ patientId: info.row.original.patientId }}
            className="font-medium text-sm hover:underline text-primary"
            onClick={(e) => e.stopPropagation()}
          >
            {info.getValue()}
          </Link>
        ),
        enableSorting: true,
      }),
      columnHelper.accessor("treatmentType", {
        header: t("approvalQueue.columns.treatmentType"),
        cell: (info) => {
          const type = info.getValue()
          return (
            <Badge
              variant="outline"
              className={`text-xs ${TREATMENT_TYPE_STYLES[type] ?? ""}`}
            >
              {type}
            </Badge>
          )
        },
        enableSorting: false,
      }),
      columnHelper.display({
        id: "progress",
        header: t("approvalQueue.columns.progress"),
        cell: ({ row }) => {
          const pkg = row.original
          return (
            <span className="text-sm">
              {pkg.sessionsCompleted} / {pkg.totalSessions}
            </span>
          )
        },
      }),
      columnHelper.display({
        id: "requestedBy",
        header: t("approvalQueue.columns.requestedBy"),
        cell: ({ row }) => (
          <span className="text-sm">
            {row.original.cancellationRequest?.requestedByName ?? "--"}
          </span>
        ),
      }),
      columnHelper.display({
        id: "requestDate",
        header: t("approvalQueue.columns.requestDate"),
        cell: ({ row }) => {
          const req = row.original.cancellationRequest
          if (!req) return <span className="text-sm text-muted-foreground">--</span>
          return (
            <span className="text-sm text-muted-foreground">
              {format(new Date(req.requestedAt), "dd/MM/yyyy HH:mm")}
            </span>
          )
        },
      }),
      columnHelper.display({
        id: "reason",
        header: t("approvalQueue.columns.reason"),
        cell: ({ row }) => (
          <span className="text-sm max-w-[200px] truncate block">
            {row.original.cancellationRequest?.reason ?? "--"}
          </span>
        ),
      }),
      columnHelper.display({
        id: "deduction",
        header: t("approvalQueue.columns.deduction"),
        cell: ({ row }) => {
          const deduction = row.original.cancellationRequest?.deductionPercent
          return (
            <span className="text-sm font-medium">
              {deduction != null ? `${deduction}%` : "--"}
            </span>
          )
        },
      }),
      ...(canManage
        ? [
            columnHelper.display({
              id: "actions",
              header: "",
              cell: ({ row }) => (
                <div className="flex items-center gap-1">
                  <Button
                    size="sm"
                    variant="outline"
                    className="h-8 text-green-600 hover:text-green-700 hover:bg-green-50"
                    onClick={(e) => {
                      e.stopPropagation()
                      setApproveTarget(row.original)
                    }}
                  >
                    <IconCheck className="h-4 w-4 mr-1" />
                    {t("approvalQueue.approveButton")}
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    className="h-8 text-red-600 hover:text-red-700 hover:bg-red-50"
                    onClick={(e) => {
                      e.stopPropagation()
                      setRejectTarget(row.original)
                    }}
                  >
                    <IconX className="h-4 w-4 mr-1" />
                    {t("approvalQueue.rejectButton")}
                  </Button>
                </div>
              ),
            }),
          ]
        : []),
    ],
    [canManage, t],
  )

  const table = useReactTable({
    data: packages,
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: {
      pagination: { pageSize: 20 },
    },
  })

  if (isError) {
    return (
      <div className="text-center py-12 text-destructive">
        {t("approvalQueue.loadError")}
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    )
  }

  return (
    <>
      <DataTable
        table={table}
        columns={columns}
        emptyMessage={t("approvalQueue.emptyMessage")}
      />

      <ApproveDialog
        open={approveTarget !== null}
        onOpenChange={(open) => {
          if (!open) setApproveTarget(null)
        }}
        pkg={approveTarget}
      />

      <RejectDialog
        open={rejectTarget !== null}
        onOpenChange={(open) => {
          if (!open) setRejectTarget(null)
        }}
        pkg={rejectTarget}
      />
    </>
  )
}
