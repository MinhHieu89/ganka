import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"

// ---- Types matching backend Patient.Contracts.Dtos ----

export type PatientType = "Medical" | "WalkIn"
export type Gender = "Male" | "Female" | "Other"
export type AllergySeverity = "Mild" | "Moderate" | "Severe"

export interface AllergyDto {
  id: string
  name: string
  severity: AllergySeverity
}

export interface PatientDto {
  id: string
  fullName: string
  phone: string
  patientCode: string | null
  patientType: PatientType
  dateOfBirth: string | null
  gender: Gender | null
  address: string | null
  cccd: string | null
  photoUrl: string | null
  isActive: boolean
  createdAt: string
  allergies: AllergyDto[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface RegisterPatientCommand {
  fullName: string
  phone: string
  dateOfBirth?: string | null
  gender?: Gender | null
  patientType: PatientType
  address?: string | null
  cccd?: string | null
  allergies?: AllergyInput[] | null
}

export interface AllergyInput {
  name: string
  severity: AllergySeverity
}

export interface UpdatePatientCommand {
  patientId: string
  fullName: string
  phone: string
  dateOfBirth?: string | null
  gender?: Gender | null
  address?: string | null
  cccd?: string | null
}

export interface AddAllergyCommand {
  patientId: string
  name: string
  severity: AllergySeverity
}

export interface GetPatientListParams {
  page?: number
  pageSize?: number
  gender?: Gender | null
  hasAllergies?: boolean | null
  dateFrom?: string | null
  dateTo?: string | null
}

// ---- API functions ----

async function registerPatient(
  data: RegisterPatientCommand,
): Promise<string> {
  const res = await api.POST("/api/patients" as never, {
    body: data as never,
  })
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to register patient")
  }
  return res.data as string
}

async function getPatientById(patientId: string): Promise<PatientDto> {
  const res = await api.GET(`/api/patients/${patientId}` as never, {})
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to fetch patient")
  }
  return res.data as PatientDto
}

async function updatePatient(data: UpdatePatientCommand): Promise<void> {
  const res = await api.PUT(
    `/api/patients/${data.patientId}` as never,
    {
      body: data as never,
    },
  )
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to update patient")
  }
}

async function deactivatePatient(patientId: string): Promise<void> {
  const res = await api.POST(
    `/api/patients/${patientId}/deactivate` as never,
    {},
  )
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to deactivate patient")
  }
}

async function reactivatePatient(patientId: string): Promise<void> {
  const res = await api.POST(
    `/api/patients/${patientId}/reactivate` as never,
    {},
  )
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to reactivate patient")
  }
}

async function getPatientList(
  params: GetPatientListParams,
): Promise<PagedResult<PatientDto>> {
  const query: Record<string, unknown> = {}
  if (params.page) query.page = params.page
  if (params.pageSize) query.pageSize = params.pageSize
  if (params.gender) query.gender = params.gender
  if (params.hasAllergies !== undefined && params.hasAllergies !== null)
    query.hasAllergies = params.hasAllergies
  if (params.dateFrom) query.dateFrom = params.dateFrom
  if (params.dateTo) query.dateTo = params.dateTo

  const res = await api.GET("/api/patients" as never, {
    params: { query } as never,
  })
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to fetch patients")
  }
  return res.data as PagedResult<PatientDto>
}

async function addAllergy(data: AddAllergyCommand): Promise<string> {
  const res = await api.POST(
    `/api/patients/${data.patientId}/allergies` as never,
    {
      body: { name: data.name, severity: data.severity } as never,
    },
  )
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to add allergy")
  }
  return res.data as string
}

async function removeAllergy(
  patientId: string,
  allergyId: string,
): Promise<void> {
  const res = await api.DELETE(
    `/api/patients/${patientId}/allergies/${allergyId}` as never,
    {},
  )
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to remove allergy")
  }
}

async function uploadPatientPhoto(
  patientId: string,
  file: File,
): Promise<string> {
  const formData = new FormData()
  formData.append("file", file)

  const res = await fetch(
    `${import.meta.env.VITE_API_URL ?? "http://localhost:5255"}/api/patients/${patientId}/photo`,
    {
      method: "POST",
      body: formData,
      headers: {
        Authorization: `Bearer ${(await import("@/shared/stores/authStore")).useAuthStore.getState().accessToken}`,
      },
    },
  )
  if (!res.ok) {
    throw new Error("Failed to upload photo")
  }
  return (await res.json()) as string
}

// ---- TanStack Query hooks ----

export function useRegisterPatient() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: registerPatient,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["patients"] })
    },
  })
}

export function usePatientById(patientId: string | undefined) {
  return useQuery({
    queryKey: ["patients", patientId],
    queryFn: () => getPatientById(patientId!),
    enabled: !!patientId,
  })
}

export function useUpdatePatient() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: updatePatient,
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["patients", variables.patientId],
      })
      queryClient.invalidateQueries({ queryKey: ["patients"] })
    },
  })
}

export function useDeactivatePatient() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: deactivatePatient,
    onSuccess: (_data, patientId) => {
      queryClient.invalidateQueries({ queryKey: ["patients", patientId] })
      queryClient.invalidateQueries({ queryKey: ["patients"] })
    },
  })
}

export function useReactivatePatient() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: reactivatePatient,
    onSuccess: (_data, patientId) => {
      queryClient.invalidateQueries({ queryKey: ["patients", patientId] })
      queryClient.invalidateQueries({ queryKey: ["patients"] })
    },
  })
}

export function usePatientList(params: GetPatientListParams) {
  return useQuery({
    queryKey: ["patients", "list", params],
    queryFn: () => getPatientList(params),
    placeholderData: (prev: PagedResult<PatientDto> | undefined) => prev,
  })
}

export function useAddAllergy() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: addAllergy,
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["patients", variables.patientId],
      })
    },
  })
}

export function useRemoveAllergy() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      patientId,
      allergyId,
    }: {
      patientId: string
      allergyId: string
    }) => removeAllergy(patientId, allergyId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["patients", variables.patientId],
      })
    },
  })
}

export function useUploadPatientPhoto() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      patientId,
      file,
    }: {
      patientId: string
      file: File
    }) => uploadPatientPhoto(patientId, file),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["patients", variables.patientId],
      })
    },
  })
}
