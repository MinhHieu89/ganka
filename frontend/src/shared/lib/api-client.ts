import createClient from "openapi-fetch"
import type { Middleware } from "openapi-fetch"
import { useAuthStore } from "@/shared/stores/authStore"

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5255"

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    const token = useAuthStore.getState().accessToken
    if (token) {
      request.headers.set("Authorization", `Bearer ${token}`)
    }
    return request
  },
}

// Typed openapi-fetch client
// Usage: import { api } from '@/shared/lib/api-client'
// When OpenAPI types are generated, update the generic:
//   import type { paths } from '@/generated/api-types'
//   export const api = createClient<paths>({ baseUrl: API_URL })
export const api = createClient({ baseUrl: API_URL, credentials: "include" })
api.use(authMiddleware)
