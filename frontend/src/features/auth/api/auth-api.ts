import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"

// ---- Types matching backend Auth.Contracts.Dtos ----

export interface LoginRequest {
  email: string
  password: string
  rememberMe: boolean
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserDto
}

export interface UserDto {
  id: string
  email: string
  fullName: string
  preferredLanguage: string
  isActive: boolean
  roles: string[]
  permissions: string[]
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface UpdateLanguageRequest {
  language: string
}

// ---- Auth API functions ----

async function login(data: LoginRequest): Promise<LoginResponse> {
  const res = await api.POST("/api/auth/login" as never, {
    body: data as never,
  })
  if (res.error || !res.data) {
    const err = res.error as { detail?: string; title?: string } | undefined
    throw new Error(err?.detail || err?.title || "Login failed")
  }
  return res.data as LoginResponse
}

async function refreshToken(data: RefreshTokenRequest): Promise<LoginResponse> {
  const res = await api.POST("/api/auth/refresh" as never, {
    body: data as never,
  })
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Token refresh failed")
  }
  return res.data as LoginResponse
}

async function logout(): Promise<void> {
  await api.POST("/api/auth/logout" as never, {})
}

async function getMe(): Promise<UserDto> {
  const res = await api.GET("/api/auth/me" as never, {})
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to get user info")
  }
  return res.data as UserDto
}

async function updateLanguage(data: UpdateLanguageRequest): Promise<void> {
  await api.PUT("/api/auth/language" as never, {
    body: data as never,
  })
}

// ---- TanStack Query hooks ----

export function useLoginMutation() {
  return useMutation({
    mutationFn: login,
  })
}

export function useRefreshMutation() {
  return useMutation({
    mutationFn: refreshToken,
  })
}

export function useLogoutMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: logout,
    onSuccess: () => {
      queryClient.clear()
    },
  })
}

export function useGetMeQuery(enabled = true) {
  return useQuery({
    queryKey: ["auth", "me"],
    queryFn: getMe,
    enabled,
    retry: false,
  })
}

export function useUpdateLanguageMutation() {
  return useMutation({
    mutationFn: updateLanguage,
  })
}
