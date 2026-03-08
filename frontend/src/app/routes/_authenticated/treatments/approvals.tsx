import { createFileRoute } from "@tanstack/react-router"
import { CancellationApprovalQueue } from "@/features/treatment/components/CancellationApprovalQueue"

export const Route = createFileRoute("/_authenticated/treatments/approvals")({
  component: ApprovalsPage,
})

function ApprovalsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Phe duyet huy phac do</h1>
        <p className="text-sm text-muted-foreground mt-0.5">
          Quan ly cac yeu cau huy phac do dieu tri can phe duyet
        </p>
      </div>

      <CancellationApprovalQueue />
    </div>
  )
}
