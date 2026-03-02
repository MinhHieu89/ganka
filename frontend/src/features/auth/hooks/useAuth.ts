import { useCallback, useEffect, useRef } from "react"
import { useNavigate } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { useAuthStore } from "@/shared/stores/authStore"
import {
  useLoginMutation,
  useRefreshMutation,
  useLogoutMutation,
} from "@/features/auth/api/auth-api"

export function useAuth() {
  const { i18n } = useTranslation()
  const navigate = useNavigate()
  const { user, accessToken, isAuthenticated, setAuth, clearAuth } =
    useAuthStore()
  const loginMutation = useLoginMutation()
  const refreshMutation = useRefreshMutation()
  const logoutMutation = useLogoutMutation()
  const refreshTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  // Schedule token refresh at 80% of lifetime
  const scheduleRefresh = useCallback(
    (expiresAt: string) => {
      if (refreshTimerRef.current) {
        clearTimeout(refreshTimerRef.current)
      }

      const expiresMs = new Date(expiresAt).getTime()
      const nowMs = Date.now()
      const lifetime = expiresMs - nowMs
      const refreshAt = lifetime * 0.8

      if (refreshAt <= 0) return

      refreshTimerRef.current = setTimeout(async () => {
        try {
          const response = await refreshMutation.mutateAsync()
          setAuth(
            {
              id: response.user.id,
              email: response.user.email,
              fullName: response.user.fullName,
              permissions: response.user.permissions,
              preferredLanguage: response.user.preferredLanguage,
            },
            response.accessToken,
          )
          scheduleRefresh(response.expiresAt)
        } catch {
          clearAuth()
          navigate({ to: "/login" })
        }
      }, refreshAt)
    },
    [refreshMutation, setAuth, clearAuth, navigate],
  )

  // Cleanup timer on unmount
  useEffect(() => {
    return () => {
      if (refreshTimerRef.current) {
        clearTimeout(refreshTimerRef.current)
      }
    }
  }, [])

  const login = useCallback(
    async (email: string, password: string, rememberMe: boolean) => {
      const response = await loginMutation.mutateAsync({
        email,
        password,
        rememberMe,
      })

      setAuth(
        {
          id: response.user.id,
          email: response.user.email,
          fullName: response.user.fullName,
          permissions: response.user.permissions,
          preferredLanguage: response.user.preferredLanguage,
        },
        response.accessToken,
      )

      // Sync UI language to user's preferred language
      if (
        response.user.preferredLanguage &&
        i18n.language !== response.user.preferredLanguage
      ) {
        i18n.changeLanguage(response.user.preferredLanguage)
      }

      // Schedule automatic token refresh
      scheduleRefresh(response.expiresAt)

      return response
    },
    [loginMutation, setAuth, i18n, scheduleRefresh],
  )

  const logout = useCallback(async () => {
    try {
      await logoutMutation.mutateAsync()
    } catch {
      // Even if server logout fails, clear local state
    }
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current)
    }
    clearAuth()
    navigate({ to: "/login" })
  }, [logoutMutation, clearAuth, navigate])

  const refresh = useCallback(async () => {
    try {
      const response = await refreshMutation.mutateAsync()
      setAuth(
        {
          id: response.user.id,
          email: response.user.email,
          fullName: response.user.fullName,
          permissions: response.user.permissions,
          preferredLanguage: response.user.preferredLanguage,
        },
        response.accessToken,
      )
      scheduleRefresh(response.expiresAt)
      return response
    } catch {
      clearAuth()
      navigate({ to: "/login" })
      toast.error("Session expired. Please log in again.")
      return null
    }
  }, [refreshMutation, setAuth, clearAuth, navigate, scheduleRefresh])

  return {
    user,
    accessToken,
    isAuthenticated,
    login,
    logout,
    refresh,
    isLoggingIn: loginMutation.isPending,
    loginError: loginMutation.error,
  }
}
