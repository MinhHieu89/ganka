import { createFileRoute, redirect } from "@tanstack/react-router"
import { z } from "zod"
import { useAuthStore } from "@/shared/stores/authStore"
import { LoginPage } from "@/features/auth/components/LoginPage"

const loginSearchSchema = z.object({
  redirect: z.string().optional(),
})

export const Route = createFileRoute("/login")({
  validateSearch: loginSearchSchema,
  beforeLoad: () => {
    const { isAuthenticated } = useAuthStore.getState()
    if (isAuthenticated) {
      throw redirect({ to: "/dashboard" })
    }
  },
  component: LoginRoute,
})

function LoginRoute() {
  const { redirect: redirectTo } = Route.useSearch()

  return <LoginPage redirectTo={redirectTo} />
}
