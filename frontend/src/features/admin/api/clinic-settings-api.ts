import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"
import { useAuthStore } from "@/shared/stores/authStore"

// ---- Types matching backend Shared.Application.Interfaces.IClinicSettingsService DTOs ----

export interface ClinicSettingsDto {
  id: string
  clinicName: string
  clinicNameVi: string | null
  address: string
  phone: string | null
  fax: string | null
  licenseNumber: string | null
  tagline: string | null
  logoBlobUrl: string | null
  email: string | null
  website: string | null
}

export interface UpdateClinicSettingsCommand {
  clinicName: string
  address: string
  clinicNameVi?: string | null
  phone?: string | null
  fax?: string | null
  licenseNumber?: string | null
  tagline?: string | null
  email?: string | null
  website?: string | null
}

// ---- Query key factory ----

export const clinicSettingsKeys = {
  all: ["clinic-settings"] as const,
}

// ---- API functions ----

async function getClinicSettings(): Promise<ClinicSettingsDto | null> {
  const res = await api.GET("/api/settings/clinic" as never, {})
  if (res.response.status === 404) {
    return null
  }
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to fetch clinic settings")
  }
  return res.data as ClinicSettingsDto
}

async function updateClinicSettings(
  data: UpdateClinicSettingsCommand,
): Promise<void> {
  const res = await api.PUT("/api/settings/clinic" as never, {
    body: data as never,
  })
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(
      err.detail || err.title || "Failed to update clinic settings",
    )
  }
}

async function uploadClinicLogo(file: File): Promise<string> {
  const formData = new FormData()
  formData.append("file", file)

  const baseUrl = import.meta.env.VITE_API_URL ?? "http://localhost:5255"
  const token = useAuthStore.getState().accessToken

  const res = await fetch(`${baseUrl}/api/settings/clinic/logo`, {
    method: "POST",
    body: formData,
    headers: {
      Authorization: `Bearer ${token}`,
    },
  })

  if (!res.ok) {
    let message = "Failed to upload clinic logo"
    try {
      const errorBody = await res.json()
      if (errorBody.detail) message = errorBody.detail
      else if (errorBody.title) message = errorBody.title
    } catch {
      // Response is not JSON, use status text
      if (res.status === 413) message = "File is too large (max 5MB)"
      else if (res.status === 415) message = "Unsupported file type. Only JPEG, PNG, and WebP are allowed."
      else if (res.status === 401) message = "Not authenticated. Please log in again."
    }
    throw new Error(message)
  }

  const data = await res.json()
  return data.url ?? data.logoUrl ?? ""
}

// ---- TanStack Query hooks ----

export function useClinicSettings() {
  return useQuery({
    queryKey: clinicSettingsKeys.all,
    queryFn: getClinicSettings,
  })
}

export function useUpdateClinicSettings() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: updateClinicSettings,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clinicSettingsKeys.all })
    },
  })
}

export function useUploadClinicLogo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: uploadClinicLogo,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clinicSettingsKeys.all })
    },
  })
}
