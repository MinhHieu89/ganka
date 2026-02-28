import { createFileRoute, redirect } from "@tanstack/react-router"
import { AppShell } from "@/shared/components/AppShell"
import { useAuthStore } from "@/shared/stores/authStore"

export const Route = createFileRoute("/_authenticated")({
  beforeLoad: ({ location }) => {
    const { isAuthenticated } = useAuthStore.getState()
    if (!isAuthenticated) {
      throw redirect({
        to: "/login",
        search: {
          redirect: location.href,
        },
      })
    }
  },
  component: AppShell,
})
