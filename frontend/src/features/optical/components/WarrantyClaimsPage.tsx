import { useState } from "react"
import { useTranslation } from "react-i18next"
import {
  useReactTable,
  getCoreRowModel,
  getExpandedRowModel,
  type ColumnDef,
  type ExpandedState,
} from "@tanstack/react-table"
import { IconPlus, IconCheck, IconX, IconLoader2, IconChevronDown, IconChevronRight, IconDots } from "@tabler/icons-react"
import { toast } from "sonner"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { DataTable } from "@/shared/components/DataTable"
import { Skeleton } from "@/shared/components/Skeleton"
import { Tabs, TabsList, TabsTrigger } from "@/shared/components/Tabs"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/AlertDialog"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/shared/components/DropdownMenu"
import { useWarrantyClaims, useApproveWarrantyClaim } from "@/features/optical/api/optical-queries"
import type { WarrantyClaimDto } from "@/features/optical/api/optical-api"
import { WARRANTY_RESOLUTION_MAP, WARRANTY_APPROVAL_STATUS_MAP } from "@/features/optical/api/optical-api"
import { WarrantyClaimForm } from "./WarrantyClaimForm"
import { WarrantyDocumentUpload } from "./WarrantyDocumentUpload"
import { format } from "date-fns"

type ApprovalFilter = "All" | "Pending" | "Approved" | "Rejected"

const APPROVAL_FILTER_STATUS: Record<ApprovalFilter, number | undefined> = {
  All: undefined,
  Pending: 0,
  Approved: 1,
  Rejected: 2,
}

function ResolutionBadge({ resolution }: { resolution: number }) {
  const { t } = useTranslation("optical")
  const label = t(WARRANTY_RESOLUTION_MAP[resolution] ?? "Unknown")
  const variantMap: Record<number, "default" | "secondary" | "outline"> = {
    0: "default",   // Replace
    1: "secondary",  // Repair
    2: "outline",    // Discount
  }
  return <Badge variant={variantMap[resolution] ?? "outline"}>{label}</Badge>
}

function ApprovalStatusBadge({ approvalStatus }: { approvalStatus: number }) {
  const { t } = useTranslation("optical")
  const label = t(WARRANTY_APPROVAL_STATUS_MAP[approvalStatus] ?? "Unknown")
  if (approvalStatus === 1) {
    return (
      <Badge className="bg-green-100 text-green-800 border-green-200 hover:bg-green-100">
        {label}
      </Badge>
    )
  }
  if (approvalStatus === 2) {
    return (
      <Badge className="bg-red-100 text-red-800 border-red-200 hover:bg-red-100">
        {label}
      </Badge>
    )
  }
  return (
    <Badge className="bg-yellow-100 text-yellow-800 border-yellow-200 hover:bg-yellow-100">
      {label}
    </Badge>
  )
}

interface ApprovalDialogState {
  claimId: string
  approve: boolean
}

function ClaimDetailsSubRow({ claim }: { claim: WarrantyClaimDto }) {
  const { t } = useTranslation("optical")
  return (
    <div className="bg-muted/30 px-6 py-4 space-y-3 border-t">
      <div className="grid grid-cols-2 gap-4 text-sm">
        <div>
          <span className="text-muted-foreground font-medium">{t("warranty.assessmentNotes")}:</span>
          <p className="mt-1">{claim.assessmentNotes ?? "—"}</p>
        </div>
        {claim.approvalNotes && (
          <div>
            <span className="text-muted-foreground font-medium">{t("warranty.approvedBy")}:</span>
            <p className="mt-1">{claim.approvalNotes}</p>
          </div>
        )}
      </div>
      {(claim.documentUrls?.length ?? 0) > 0 && (
        <div>
          <span className="text-muted-foreground font-medium text-sm">{t("warranty.documents")}:</span>
          <WarrantyDocumentUpload
            claimId={claim.id}
            existingDocuments={claim.documentUrls}
            readonly
          />
        </div>
      )}
    </div>
  )
}

export function WarrantyClaimsPage() {
  const { t } = useTranslation("optical")
  const [activeFilter, setActiveFilter] = useState<ApprovalFilter>("All")
  const [page] = useState(1)
  const [formOpen, setFormOpen] = useState(false)
  const [approvalDialog, setApprovalDialog] = useState<ApprovalDialogState | null>(null)
  const [expanded, setExpanded] = useState<ExpandedState>({})

  const approvalStatusFilter = APPROVAL_FILTER_STATUS[activeFilter]
  const { data, isLoading } = useWarrantyClaims({
    approvalStatusFilter,
    page,
    pageSize: 20,
  })

  const approveMutation = useApproveWarrantyClaim()

  const claims = data?.items ?? []

  const handleApprovalConfirm = async () => {
    if (!approvalDialog) return
    try {
      await approveMutation.mutateAsync({
        id: approvalDialog.claimId,
        approve: approvalDialog.approve,
      })
      toast.success(
        approvalDialog.approve ? t("warranty.approved") : t("warranty.rejected"),
      )
    } catch {
      // error handled in mutation
    } finally {
      setApprovalDialog(null)
    }
  }

  const columns: ColumnDef<WarrantyClaimDto>[] = [
    {
      id: "expand",
      size: 40,
      cell: ({ row }) => (
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7"
          onClick={(e) => {
            e.stopPropagation()
            row.toggleExpanded()
          }}
        >
          {row.getIsExpanded() ? (
            <IconChevronDown className="h-4 w-4" />
          ) : (
            <IconChevronRight className="h-4 w-4" />
          )}
        </Button>
      ),
    },
    {
      accessorKey: "claimDate",
      header: t("warranty.claimDate"),
      cell: ({ row }) => {
        try {
          return format(new Date(row.original.claimDate), "dd/MM/yyyy")
        } catch {
          return row.original.claimDate
        }
      },
    },
    {
      accessorKey: "patientName",
      header: t("warranty.patient"),
    },
    {
      accessorKey: "glassesOrderId",
      header: t("warranty.order"),
      cell: ({ row }) => (
        <span className="font-mono text-sm">{row.original.glassesOrderId.slice(0, 8)}</span>
      ),
    },
    {
      accessorKey: "resolution",
      header: t("warranty.resolution"),
      cell: ({ row }) => <ResolutionBadge resolution={row.original.resolution} />,
    },
    {
      accessorKey: "approvalStatus",
      header: t("warranty.approvalStatus"),
      cell: ({ row }) => <ApprovalStatusBadge approvalStatus={row.original.approvalStatus} />,
    },
    {
      accessorKey: "documentUrls",
      header: t("warranty.documents"),
      cell: ({ row }) => (
        <span className="text-muted-foreground text-sm">
          {row.original.documentUrls?.length ?? 0} file
          {(row.original.documentUrls?.length ?? 0) !== 1 ? "s" : ""}
        </span>
      ),
    },
    {
      id: "actions",
      header: t("common.actions"),
      cell: ({ row }) => {
        const claim = row.original
        // Replace claims (resolution=0) that are Pending (approvalStatus=0) show approve/reject
        const isReplace = claim.resolution === 0
        const isPending = claim.approvalStatus === 0

        if (isReplace && isPending) {
          return (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8"
                  onClick={(e) => e.stopPropagation()}
                  disabled={approveMutation.isPending}
                >
                  <IconDots className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem
                  className="text-green-700 focus:text-green-700"
                  onClick={(e) => {
                    e.stopPropagation()
                    setApprovalDialog({ claimId: claim.id, approve: true })
                  }}
                >
                  <IconCheck className="h-4 w-4 mr-2" />
                  {t("enums.warrantyApproval.approved")}
                </DropdownMenuItem>
                <DropdownMenuItem
                  className="text-red-700 focus:text-red-700"
                  onClick={(e) => {
                    e.stopPropagation()
                    setApprovalDialog({ claimId: claim.id, approve: false })
                  }}
                >
                  <IconX className="h-4 w-4 mr-2" />
                  {t("enums.warrantyApproval.rejected")}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          )
        }
        return null
      },
    },
  ]

  const table = useReactTable({
    data: claims,
    columns,
    state: { expanded },
    onExpandedChange: setExpanded,
    getCoreRowModel: getCoreRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
  })

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("warranty.title")}</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {t("warranty.subtitle")}
          </p>
        </div>
        <Button onClick={() => setFormOpen(true)}>
          <IconPlus className="h-4 w-4 mr-2" />
          {t("warranty.addClaim")}
        </Button>
      </div>

      {/* Filter tabs */}
      <Tabs
        value={activeFilter}
        onValueChange={(v) => setActiveFilter(v as ApprovalFilter)}
      >
        <TabsList>
          <TabsTrigger value="All">{t("common.all")}</TabsTrigger>
          <TabsTrigger value="Pending">{t("warranty.pendingApproval")}</TabsTrigger>
          <TabsTrigger value="Approved">{t("enums.warrantyApproval.approved")}</TabsTrigger>
          <TabsTrigger value="Rejected">{t("enums.warrantyApproval.rejected")}</TabsTrigger>
        </TabsList>
      </Tabs>

      {/* Claims table */}
      {isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      ) : (
        <DataTable
          table={table}
          columns={columns}
          emptyMessage={t("warranty.empty")}
          renderSubRow={(claim) => <ClaimDetailsSubRow claim={claim} />}
        />
      )}

      {/* File claim dialog */}
      <WarrantyClaimForm open={formOpen} onOpenChange={setFormOpen} />

      {/* Approve/Reject confirmation dialog */}
      <AlertDialog
        open={!!approvalDialog}
        onOpenChange={(open) => {
          if (!open) setApprovalDialog(null)
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {approvalDialog?.approve
                ? t("warranty.approved")
                : t("warranty.rejected")}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {approvalDialog?.approve
                ? t("warranty.requiresManagerApproval")
                : t("warranty.rejectedReason")}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t("common.cancel")}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleApprovalConfirm}
              className={
                approvalDialog?.approve
                  ? "bg-green-600 hover:bg-green-700"
                  : "bg-red-600 hover:bg-red-700"
              }
              disabled={approveMutation.isPending}
            >
              {approveMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {approvalDialog?.approve ? t("enums.warrantyApproval.approved") : t("enums.warrantyApproval.rejected")}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}
