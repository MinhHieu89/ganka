import { createFileRoute, redirect } from "@tanstack/react-router"
import { AppShell } from "@/shared/components/AppShell"
import { useAuthStore } from "@/shared/stores/authStore"
import { silentRefresh } from "@/features/auth/api/auth-api"

export const Route = createFileRoute("/_authenticated")({
  beforeLoad: async ({ location }) => {
    const { isAuthenticated, accessToken } = useAuthStore.getState()

    // If already authenticated with access token in memory, proceed
    if (isAuthenticated && accessToken) {
      return
    }

    // Attempt silent refresh using HTTP-only cookie
    try {
      const response = await silentRefresh()
      if (response) {
        useAuthStore.getState().setAuth(
          {
            id: response.user.id,
            email: response.user.email,
            fullName: response.user.fullName,
            roles: response.user.roles,
            permissions: response.user.permissions,
            preferredLanguage: response.user.preferredLanguage,
          },
          response.accessToken,
        )
        return // session silently restored
      }
    } catch {
      // Silent refresh failed -- fall through to login redirect
    }

    // No valid session -- redirect to login
    throw redirect({
      to: "/login",
      search: { redirect: location.href },
    })
  },
  component: AppShell,
})
