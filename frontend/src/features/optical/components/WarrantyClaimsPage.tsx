import { useState } from "react"
import {
  useReactTable,
  getCoreRowModel,
  type ColumnDef,
} from "@tanstack/react-table"
import { IconPlus, IconCheck, IconX, IconLoader2, IconChevronDown, IconChevronRight } from "@tabler/icons-react"
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

function getResolutionLabel(resolutionType: number): string {
  return WARRANTY_RESOLUTION_MAP[resolutionType] ?? "Unknown"
}

function getApprovalLabel(approvalStatus: number): string {
  return WARRANTY_APPROVAL_STATUS_MAP[approvalStatus] ?? "Unknown"
}

function ResolutionBadge({ resolutionType }: { resolutionType: number }) {
  const label = getResolutionLabel(resolutionType)
  const variantMap: Record<string, "default" | "secondary" | "outline"> = {
    Replace: "default",
    Repair: "secondary",
    Discount: "outline",
  }
  return <Badge variant={variantMap[label] ?? "outline"}>{label}</Badge>
}

function ApprovalStatusBadge({ approvalStatus }: { approvalStatus: number }) {
  const label = getApprovalLabel(approvalStatus)
  if (label === "Approved") {
    return (
      <Badge className="bg-green-100 text-green-800 border-green-200 hover:bg-green-100">
        {label}
      </Badge>
    )
  }
  if (label === "Rejected") {
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
  return (
    <div className="bg-muted/30 px-6 py-4 space-y-3 border-t">
      <div className="grid grid-cols-2 gap-4 text-sm">
        <div>
          <span className="text-muted-foreground font-medium">Assessment Notes:</span>
          <p className="mt-1">{claim.notes ?? "—"}</p>
        </div>
        {claim.approvalNotes && (
          <div>
            <span className="text-muted-foreground font-medium">Approval Notes:</span>
            <p className="mt-1">{claim.approvalNotes}</p>
          </div>
        )}
      </div>
      {claim.documentUrls.length > 0 && (
        <div>
          <span className="text-muted-foreground font-medium text-sm">Supporting Documents:</span>
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
  const [activeFilter, setActiveFilter] = useState<ApprovalFilter>("All")
  const [page] = useState(1)
  const [formOpen, setFormOpen] = useState(false)
  const [approvalDialog, setApprovalDialog] = useState<ApprovalDialogState | null>(null)
  const [expandedClaimId, setExpandedClaimId] = useState<string | null>(null)

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
        approvalDialog.approve ? "Warranty claim approved." : "Warranty claim rejected.",
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
      cell: ({ row }) => {
        const isExpanded = expandedClaimId === row.original.id
        return (
          <Button
            variant="ghost"
            size="icon"
            className="h-7 w-7"
            onClick={(e) => {
              e.stopPropagation()
              setExpandedClaimId(isExpanded ? null : row.original.id)
            }}
          >
            {isExpanded ? (
              <IconChevronDown className="h-4 w-4" />
            ) : (
              <IconChevronRight className="h-4 w-4" />
            )}
          </Button>
        )
      },
    },
    {
      accessorKey: "claimDate",
      header: "Claim Date",
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
      header: "Patient",
    },
    {
      accessorKey: "glassesOrderId",
      header: "Order",
      cell: ({ row }) => (
        <span className="font-mono text-sm">{row.original.glassesOrderId.slice(0, 8)}</span>
      ),
    },
    {
      accessorKey: "resolutionType",
      header: "Resolution",
      cell: ({ row }) => <ResolutionBadge resolutionType={row.original.resolutionType} />,
    },
    {
      accessorKey: "approvalStatus",
      header: "Approval Status",
      cell: ({ row }) => <ApprovalStatusBadge approvalStatus={row.original.approvalStatus} />,
    },
    {
      accessorKey: "documentUrls",
      header: "Documents",
      cell: ({ row }) => (
        <span className="text-muted-foreground text-sm">
          {row.original.documentUrls.length} file
          {row.original.documentUrls.length !== 1 ? "s" : ""}
        </span>
      ),
    },
    {
      id: "actions",
      header: "Actions",
      cell: ({ row }) => {
        const claim = row.original
        // Replace claims (resolutionType=0) that are Pending (approvalStatus=0) show approve/reject
        const isReplace = claim.resolutionType === 0
        const isPending = claim.approvalStatus === 0

        if (isReplace && isPending) {
          return (
            <div className="flex items-center gap-1">
              <Button
                variant="outline"
                size="sm"
                className="h-7 text-green-700 border-green-300 hover:bg-green-50"
                onClick={(e) => {
                  e.stopPropagation()
                  setApprovalDialog({ claimId: claim.id, approve: true })
                }}
                disabled={approveMutation.isPending}
              >
                <IconCheck className="h-3 w-3 mr-1" />
                Approve
              </Button>
              <Button
                variant="outline"
                size="sm"
                className="h-7 text-red-700 border-red-300 hover:bg-red-50"
                onClick={(e) => {
                  e.stopPropagation()
                  setApprovalDialog({ claimId: claim.id, approve: false })
                }}
                disabled={approveMutation.isPending}
              >
                <IconX className="h-3 w-3 mr-1" />
                Reject
              </Button>
            </div>
          )
        }
        return null
      },
    },
  ]

  const table = useReactTable({
    data: claims,
    columns,
    getCoreRowModel: getCoreRowModel(),
  })

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Warranty Claims</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            Manage warranty claims and approvals for delivered glasses orders.
          </p>
        </div>
        <Button onClick={() => setFormOpen(true)}>
          <IconPlus className="h-4 w-4 mr-2" />
          File Claim
        </Button>
      </div>

      {/* Filter tabs */}
      <Tabs
        value={activeFilter}
        onValueChange={(v) => setActiveFilter(v as ApprovalFilter)}
      >
        <TabsList>
          <TabsTrigger value="All">All</TabsTrigger>
          <TabsTrigger value="Pending">Pending Approval</TabsTrigger>
          <TabsTrigger value="Approved">Approved</TabsTrigger>
          <TabsTrigger value="Rejected">Rejected</TabsTrigger>
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
          emptyMessage="No warranty claims found."
          renderSubRow={(claim) =>
            expandedClaimId === claim.id ? <ClaimDetailsSubRow claim={claim} /> : null
          }
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
                ? "Approve Warranty Claim"
                : "Reject Warranty Claim"}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {approvalDialog?.approve
                ? "Are you sure you want to approve this replacement warranty claim? This action will authorize the replacement."
                : "Are you sure you want to reject this replacement warranty claim? This action cannot be undone."}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
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
              {approvalDialog?.approve ? "Approve" : "Reject"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}
