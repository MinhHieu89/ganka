import { useState, useMemo, useCallback } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import { format } from "date-fns"
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
import { Textarea } from "@/shared/components/Textarea"
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

const approveSchema = z.object({
  managerPin: z
    .string()
    .min(4, "PIN phai co 4-6 ky tu so")
    .max(6, "PIN phai co 4-6 ky tu so")
    .regex(/^\d+$/, "PIN chi chua ky tu so"),
  deductionPercent: z.coerce
    .number()
    .min(0, "Phan tram khau tru khong duoc am")
    .max(100, "Phan tram khau tru khong duoc vuot qua 100"),
})

type ApproveFormValues = z.infer<typeof approveSchema>

// -- Reject schema --

const rejectSchema = z.object({
  rejectionReason: z.string().min(1, "Ly do tu choi la bat buoc"),
})

type RejectFormValues = z.infer<typeof rejectSchema>

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
  const user = useAuthStore((s) => s.user)
  const approveMutation = useApproveCancellation()

  const defaultDeduction = pkg ? getDefaultDeductionPercent(pkg) : 15

  const form = useForm<ApproveFormValues>({
    resolver: zodResolver(approveSchema),
    defaultValues: {
      managerPin: "",
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
        managerPin: data.managerPin,
        deductionPercent: data.deductionPercent,
      })
      toast.success("Da phe duyet huy phac do")
      onOpenChange(false)
      form.reset()
    } catch {
      toast.error("Khong the phe duyet. Vui long kiem tra PIN.")
    }
  }

  // Reset form values when pkg changes
  const handleOpenChange = useCallback(
    (isOpen: boolean) => {
      if (isOpen && pkg) {
        form.reset({
          managerPin: "",
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
          <DialogTitle>Phe duyet huy phac do</DialogTitle>
          <DialogDescription>
            Xac nhan huy phac do dieu tri cho benh nhan{" "}
            <span className="font-medium">{pkg?.patientName}</span>
          </DialogDescription>
        </DialogHeader>

        {pkg && (
          <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
            {/* Package summary */}
            <div className="rounded-md border p-3 space-y-1 text-sm">
              <div>
                <span className="text-muted-foreground">Phac do:</span>{" "}
                {pkg.protocolTemplateName}
              </div>
              <div>
                <span className="text-muted-foreground">Loai:</span>{" "}
                <Badge variant="outline" className={`text-xs ${TREATMENT_TYPE_STYLES[pkg.treatmentType] ?? ""}`}>
                  {pkg.treatmentType}
                </Badge>
              </div>
              <div>
                <span className="text-muted-foreground">Tien trinh:</span>{" "}
                {pkg.sessionsCompleted}/{pkg.totalSessions} phien
              </div>
              <div>
                <span className="text-muted-foreground">Ly do huy:</span>{" "}
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
                    Phan tram khau tru (%)
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
              <span className="text-muted-foreground">So tien hoan du kien:</span>{" "}
              <span className="font-semibold text-base">{formatVND(refundAmount)}</span>
            </div>

            {/* Manager PIN */}
            <Controller
              name="managerPin"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel htmlFor={field.name}>PIN quan ly</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="password"
                    inputMode="numeric"
                    maxLength={6}
                    onChange={(e) => {
                      const val = e.target.value.replace(/\D/g, "")
                      field.onChange(val)
                    }}
                    autoComplete="off"
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
                disabled={approveMutation.isPending}
              >
                Huy
              </Button>
              <Button type="submit" disabled={approveMutation.isPending}>
                {approveMutation.isPending && (
                  <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                Phe duyet
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
  const user = useAuthStore((s) => s.user)
  const rejectMutation = useRejectCancellation()

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
      toast.success("Da tu choi yeu cau huy")
      onOpenChange(false)
      form.reset()
    } catch {
      toast.error("Khong the tu choi yeu cau")
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
          <DialogTitle>Tu choi yeu cau huy</DialogTitle>
          <DialogDescription>
            Tu choi yeu cau huy phac do cho benh nhan{" "}
            <span className="font-medium">{pkg?.patientName}</span>
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          <Controller
            name="rejectionReason"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>Ly do tu choi</FieldLabel>
                <Textarea
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
              Huy
            </Button>
            <Button
              type="submit"
              variant="destructive"
              disabled={rejectMutation.isPending}
            >
              {rejectMutation.isPending && (
                <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              Tu choi
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// -- Main component --

export function CancellationApprovalQueue() {
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
        header: "Benh nhan",
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
        header: "Loai dieu tri",
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
        header: "Tien trinh",
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
        header: "Nguoi yeu cau",
        cell: ({ row }) => (
          <span className="text-sm">
            {row.original.cancellationRequest?.requestedByName ?? "--"}
          </span>
        ),
      }),
      columnHelper.display({
        id: "requestDate",
        header: "Ngay yeu cau",
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
        header: "Ly do",
        cell: ({ row }) => (
          <span className="text-sm max-w-[200px] truncate block">
            {row.original.cancellationRequest?.reason ?? "--"}
          </span>
        ),
      }),
      columnHelper.display({
        id: "deduction",
        header: "Khau tru (%)",
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
                    Duyet
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
                    Tu choi
                  </Button>
                </div>
              ),
            }),
          ]
        : []),
    ],
    [canManage],
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
        Khong the tai danh sach yeu cau huy. Vui long thu lai.
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
        emptyMessage="Khong co yeu cau huy nao dang cho duyet"
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
