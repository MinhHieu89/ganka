import { createFileRoute, redirect } from "@tanstack/react-router"
import { useAuthStore } from "@/shared/stores/authStore"
import { TreatmentsPage } from "@/features/treatment/components/TreatmentsPage"
import { toast } from "sonner"

export const Route = createFileRoute("/_authenticated/treatments/")({
  beforeLoad: () => {
    const { user } = useAuthStore.getState()
    const hasPermission =
      user?.permissions?.includes("Treatment.View") ||
      user?.permissions?.includes("Admin")

    if (!hasPermission) {
      toast.error("You do not have permission to view treatments")
      throw redirect({ to: "/dashboard" })
    }
  },
  component: TreatmentsPage,
})
