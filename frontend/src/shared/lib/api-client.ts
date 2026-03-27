import createClient from "openapi-fetch"
import type { Middleware } from "openapi-fetch"
import { useAuthStore } from "@/shared/stores/authStore"
import { silentRefresh } from "@/features/auth/api/auth-api"
import type { LoginResponse } from "@/features/auth/api/auth-api"

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5255"

// Prevent concurrent refresh attempts
let refreshPromise: Promise<LoginResponse | null> | null = null

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    const token = useAuthStore.getState().accessToken
    if (token) {
      request.headers.set("Authorization", `Bearer ${token}`)
    }
    return request
  },
  async onResponse({ response, request }) {
    if (response.status === 401) {
      // Skip refresh for the refresh endpoint itself to avoid infinite loop
      if (request.url.includes("/api/auth/refresh")) {
        return response
      }

      // Avoid concurrent refresh attempts
      if (!refreshPromise) {
        refreshPromise = silentRefresh().finally(() => {
          refreshPromise = null
        })
      }

      const result = await refreshPromise
      if (result) {
        useAuthStore.getState().setAuth(
          {
            id: result.user.id,
            email: result.user.email,
            fullName: result.user.fullName,
            roles: result.user.roles,
            permissions: result.user.permissions,
            preferredLanguage: result.user.preferredLanguage,
          },
          result.accessToken,
        )

        // Retry original request with new token
        const retryRequest = new Request(request.url, {
          method: request.method,
          headers: new Headers(request.headers),
          body: request.method !== "GET" && request.method !== "HEAD" ? await request.clone().text().then(t => t || null) : null,
          credentials: request.credentials,
        })
        retryRequest.headers.set("Authorization", `Bearer ${result.accessToken}`)
        return fetch(retryRequest)
      } else {
        // Refresh failed — redirect to login
        useAuthStore.getState().clearAuth()
        window.location.href = "/login"
      }
    }
    return response
  },
}

// Typed openapi-fetch client
// Usage: import { api } from '@/shared/lib/api-client'
// When OpenAPI types are generated, update the generic:
//   import type { paths } from '@/generated/api-types'
//   export const api = createClient<paths>({ baseUrl: API_URL })
export const api = createClient({ baseUrl: API_URL, credentials: "include" })
api.use(authMiddleware)
