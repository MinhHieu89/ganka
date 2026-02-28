import createClient from "openapi-fetch"

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000"

// Typed openapi-fetch client
// Usage: import { api } from '@/shared/lib/api-client'
// When OpenAPI types are generated, update the generic:
//   import type { paths } from '@/generated/api-types'
//   export const api = createClient<paths>({ baseUrl: API_URL })
export const api = createClient({ baseUrl: API_URL })
