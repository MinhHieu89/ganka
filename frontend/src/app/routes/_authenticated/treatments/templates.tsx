import { createFileRoute, redirect } from "@tanstack/react-router"
import { useAuthStore } from "@/shared/stores/authStore"
import { ProtocolTemplateList } from "@/features/treatment/components/ProtocolTemplateList"
import { toast } from "sonner"

export const Route = createFileRoute("/_authenticated/treatments/templates")({
  beforeLoad: () => {
    const { user } = useAuthStore.getState()
    const hasPermission =
      user?.permissions?.includes("Treatment.View") ||
      user?.permissions?.includes("Admin")

    if (!hasPermission) {
      toast.error("You do not have permission to view treatment templates")
      throw redirect({ to: "/dashboard" })
    }
  },
  component: ProtocolTemplateList,
})
