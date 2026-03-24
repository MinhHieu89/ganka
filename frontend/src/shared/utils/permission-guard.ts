import { redirect } from "@tanstack/react-router"
import { useAuthStore } from "@/shared/stores/authStore"
import { toast } from "sonner"

/**
 * Route-level permission guard for beforeLoad hooks.
 * Checks if the current user has the required permission.
 * Redirects to /dashboard with toast error if denied (per D-06).
 */
export function requirePermission(permission: string, errorMessage?: string) {
  const { user } = useAuthStore.getState()
  const hasPermission = user?.permissions?.includes(permission) ?? false

  if (!hasPermission) {
    toast.error(errorMessage ?? "You do not have permission to access this page")
    throw redirect({ to: "/dashboard" })
  }
}
