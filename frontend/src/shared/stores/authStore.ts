import { create } from "zustand"
import { silentRefresh } from "@/features/auth/api/auth-api"

export interface AuthUser {
  id: string
  email: string
  fullName: string
  roles: string[]
  permissions: string[]
  preferredLanguage: string
}

interface AuthState {
  user: AuthUser | null
  accessToken: string | null
  isAuthenticated: boolean
  isInitializing: boolean
  setAuth: (user: AuthUser, accessToken: string) => void
  clearAuth: () => void
  initialize: () => Promise<void>
}

export const useAuthStore = create<AuthState>()((set, get) => ({
  user: null,
  accessToken: null,
  isAuthenticated: false,
  isInitializing: true,
  setAuth: (user, accessToken) =>
    set({ user, accessToken, isAuthenticated: true }),
  clearAuth: () =>
    set({ user: null, accessToken: null, isAuthenticated: false }),
  initialize: async () => {
    // Skip if already authenticated (e.g. login just happened)
    if (get().isAuthenticated) {
      set({ isInitializing: false })
      return
    }

    set({ isInitializing: true })
    try {
      const response = await silentRefresh()
      if (response) {
        set({
          user: {
            id: response.user.id,
            email: response.user.email,
            fullName: response.user.fullName,
            roles: response.user.roles,
            permissions: response.user.permissions,
            preferredLanguage: response.user.preferredLanguage,
          },
          accessToken: response.accessToken,
          isAuthenticated: true,
          isInitializing: false,
        })
        return
      }
    } catch {
      // Silent refresh failed -- user not remembered or token expired
    }
    set({ isInitializing: false })
  },
}))

/**
 * Hook to check if the current user has a specific role.
 */
export function useHasRole(role: string): boolean {
  const roles = useAuthStore((s) => s.user?.roles ?? [])
  return roles.includes(role)
}
