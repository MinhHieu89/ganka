import { createFileRoute, redirect } from "@tanstack/react-router"
import { useAuthStore } from "@/shared/stores/authStore"
import { CancellationApprovalQueue } from "@/features/treatment/components/CancellationApprovalQueue"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"

export const Route = createFileRoute("/_authenticated/treatments/approvals")({
  beforeLoad: () => {
    const { user } = useAuthStore.getState()
    const hasPermission =
      user?.permissions?.includes("Treatment.Manage") ||
      user?.permissions?.includes("Admin")

    if (!hasPermission) {
      toast.error("You do not have permission to manage treatment approvals")
      throw redirect({ to: "/dashboard" })
    }
  },
  component: ApprovalsPage,
})

function ApprovalsPage() {
  const { t } = useTranslation("treatment")

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{t("approvalQueue.pageTitle")}</h1>
        <p className="text-sm text-muted-foreground mt-0.5">
          {t("approvalQueue.pageDescription")}
        </p>
      </div>

      <CancellationApprovalQueue />
    </div>
  )
}
