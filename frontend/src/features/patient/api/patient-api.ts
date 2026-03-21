import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"

// ---- Types matching backend Patient.Contracts.Dtos ----

export type PatientType = "Medical" | "WalkIn"
export type Gender = "Male" | "Female" | "Other"
export type AllergySeverity = "Mild" | "Moderate" | "Severe"

// Backend returns enums as integers — map to string names
const patientTypeMap: Record<number, PatientType> = { 0: "Medical", 1: "WalkIn" }
const genderMap: Record<number, Gender> = { 0: "Male", 1: "Female", 2: "Other" }
const severityMap: Record<number, AllergySeverity> = { 0: "Mild", 1: "Moderate", 2: "Severe" }

function normalizePatient(raw: Record<string, unknown>): PatientDto {
  const p = raw as unknown as PatientDto
  if (typeof raw.patientType === "number") {
    (p as Record<string, unknown>).patientType = patientTypeMap[raw.patientType as number] ?? "Medical"
  }
  if (typeof raw.gender === "number") {
    (p as Record<string, unknown>).gender = genderMap[raw.gender as number] ?? null
  }
  if (Array.isArray(p.allergies)) {
    for (const a of p.allergies) {
      if (typeof (a as Record<string, unknown>).severity === "number") {
        (a as Record<string, unknown>).severity = severityMap[(a as Record<string, unknown>).severity as number] ?? "Mild"
      }
    }
  }
  return p
}

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
  search?: string | null
}

// ---- Field validation types (matches backend PatientFieldValidationResult) ----

export interface MissingFieldInfo {
  fieldName: string
  requiredForContext: string
}

export interface PatientFieldValidationResult {
  isValid: boolean
  missingFields: MissingFieldInfo[]
}

// ---- Bilingual allergy catalog (matches backend AllergyCatalogSeeder) ----

export const ALLERGY_CATALOG_BILINGUAL = [
  // Ophthalmic Drug
  { en: "Atropine", vi: "Atropine", category: "Ophthalmic Drug" },
  { en: "Fluorescein", vi: "Fluorescein", category: "Ophthalmic Drug" },
  { en: "Tropicamide", vi: "Tropicamide", category: "Ophthalmic Drug" },
  { en: "Proparacaine", vi: "Proparacaine", category: "Ophthalmic Drug" },
  { en: "Tetracaine", vi: "Tetracaine", category: "Ophthalmic Drug" },
  { en: "Timolol", vi: "Timolol", category: "Ophthalmic Drug" },
  { en: "Latanoprost", vi: "Latanoprost", category: "Ophthalmic Drug" },
  { en: "Brimonidine", vi: "Brimonidine", category: "Ophthalmic Drug" },
  { en: "Dorzolamide", vi: "Dorzolamide", category: "Ophthalmic Drug" },
  { en: "Cyclopentolate", vi: "Cyclopentolate", category: "Ophthalmic Drug" },
  { en: "Pilocarpine", vi: "Pilocarpine", category: "Ophthalmic Drug" },
  { en: "Prednisolone Acetate", vi: "Prednisolone Acetate", category: "Ophthalmic Drug" },
  // General Drug
  { en: "Penicillin", vi: "Penicillin", category: "General Drug" },
  { en: "Sulfonamides", vi: "Sulfonamid", category: "General Drug" },
  { en: "Aspirin", vi: "Aspirin", category: "General Drug" },
  { en: "Ibuprofen", vi: "Ibuprofen", category: "General Drug" },
  { en: "Codeine", vi: "Codein", category: "General Drug" },
  { en: "Amoxicillin", vi: "Amoxicillin", category: "General Drug" },
  { en: "Cephalosporins", vi: "Cephalosporin", category: "General Drug" },
  // Material
  { en: "Latex", vi: "Cao su (Latex)", category: "Material" },
  { en: "Contact Lens Solution", vi: "Dung dịch kính áp tròng", category: "Material" },
  { en: "Adhesive Tape", vi: "Băng dính y tế", category: "Material" },
  // Environmental
  { en: "Dust", vi: "Bụi", category: "Environmental" },
  { en: "Pollen", vi: "Phấn hoa", category: "Environmental" },
  { en: "Animal Dander", vi: "Lông động vật", category: "Environmental" },
  { en: "Mold", vi: "Nấm mốc", category: "Environmental" },
] as const

// ---- Enum denormalization: frontend strings → backend integers ----

const patientTypeToInt: Record<string, number> = { Medical: 0, WalkIn: 1 }
const genderToInt: Record<string, number> = { Male: 0, Female: 1, Other: 2 }
const severityToInt: Record<string, number> = { Mild: 0, Moderate: 1, Severe: 2 }

function denormalizeForApi(data: Record<string, unknown>): Record<string, unknown> {
  const out = { ...data }
  if (typeof out.patientType === "string") out.patientType = patientTypeToInt[out.patientType] ?? 0
  if (typeof out.gender === "string") out.gender = genderToInt[out.gender] ?? null
  if (Array.isArray(out.allergies)) {
    out.allergies = (out.allergies as Record<string, unknown>[]).map((a) => ({
      ...a,
      severity: typeof a.severity === "string" ? severityToInt[a.severity as string] ?? 0 : a.severity,
    }))
  }
  return out
}

// ---- API functions ----

async function registerPatient(
  data: RegisterPatientCommand,
): Promise<string> {
  const body = denormalizeForApi(data as unknown as Record<string, unknown>)
  const res = await api.POST("/api/patients" as never, {
    body: body as never,
  })
  if (res.error) {
    const err = res.error as Record<string, unknown>
    if (err.errors) {
      // Throw full RFC 7807 body for structured validation handling
      throw new Error(JSON.stringify(err))
    }
    throw new Error(
      (err.detail as string) || (err.title as string) || "Failed to register patient",
    )
  }
  return (res.data as { id: string }).id
}

async function getPatientById(patientId: string): Promise<PatientDto> {
  const res = await api.GET(`/api/patients/${patientId}` as never, {})
  if (res.error) {
    const err = res.error as Record<string, unknown>
    if (err.errors) {
      throw new Error(JSON.stringify(err))
    }
    throw new Error(
      (err.detail as string) || (err.title as string) || "Failed to fetch patient",
    )
  }
  return normalizePatient(res.data as Record<string, unknown>)
}

async function updatePatient(data: UpdatePatientCommand): Promise<void> {
  const body = denormalizeForApi(data as unknown as Record<string, unknown>)
  // patientId is in the URL path; remove from body to avoid deserialization issues
  delete (body as Record<string, unknown>).patientId
  delete (body as Record<string, unknown>).PatientId
  const res = await api.PUT(
    `/api/patients/${data.patientId}` as never,
    {
      body: body as never,
    },
  )
  if (res.error) {
    const err = res.error as Record<string, unknown>
    if (err.errors) {
      throw new Error(JSON.stringify(err))
    }
    throw new Error(
      (err.detail as string) || (err.title as string) || "Failed to update patient",
    )
  }
}

async function deactivatePatient(patientId: string): Promise<void> {
  const res = await api.POST(
    `/api/patients/${patientId}/deactivate` as never,
    {},
  )
  if (res.error) {
    const err = res.error as Record<string, unknown>
    if (err.errors) {
      throw new Error(JSON.stringify(err))
    }
    throw new Error(
      (err.detail as string) || (err.title as string) || "Failed to deactivate patient",
    )
  }
}

async function reactivatePatient(patientId: string): Promise<void> {
  const res = await api.POST(
    `/api/patients/${patientId}/reactivate` as never,
    {},
  )
  if (res.error) {
    const err = res.error as Record<string, unknown>
    if (err.errors) {
      throw new Error(JSON.stringify(err))
    }
    throw new Error(
      (err.detail as string) || (err.title as string) || "Failed to reactivate patient",
    )
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
  if (params.search) query.search = params.search

  const res = await api.GET("/api/patients" as never, {
    params: { query } as never,
  })
  if (res.error) {
    const err = res.error as Record<string, unknown>
    if (err.errors) {
      throw new Error(JSON.stringify(err))
    }
    throw new Error(
      (err.detail as string) || (err.title as string) || "Failed to fetch patients",
    )
  }
  const data = res.data as PagedResult<PatientDto>
  data.items = data.items.map((item) => normalizePatient(item as unknown as Record<string, unknown>))
  return data
}

async function addAllergy(data: AddAllergyCommand): Promise<string> {
  const res = await api.POST(
    `/api/patients/${data.patientId}/allergies` as never,
    {
      body: { name: data.name, severity: severityToInt[data.severity] ?? 0 } as never,
    },
  )
  if (res.error) {
    const err = res.error as Record<string, unknown>
    if (err.errors) {
      throw new Error(JSON.stringify(err))
    }
    throw new Error(
      (err.detail as string) || (err.title as string) || "Failed to add allergy",
    )
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
    const err = res.error as Record<string, unknown>
    if (err.errors) {
      throw new Error(JSON.stringify(err))
    }
    throw new Error(
      (err.detail as string) || (err.title as string) || "Failed to remove allergy",
    )
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
    let message = "Failed to upload photo"
    try {
      const errorBody = await res.json()
      if (errorBody.detail) message = errorBody.detail
      else if (errorBody.title) message = errorBody.title
    } catch {
      // Response is not JSON, use status text
      if (res.status === 413) message = "File is too large"
      else if (res.status === 415) message = "Unsupported file type"
      else if (res.status === 401) message = "Not authenticated. Please log in again."
    }
    throw new Error(message)
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

// ---- Field validation ----

async function getPatientFieldValidation(
  patientId: string,
): Promise<PatientFieldValidationResult> {
  const res = await api.GET(
    `/api/patients/${patientId}/field-validation` as never,
    {},
  )
  if (res.error) {
    const err = res.error as Record<string, unknown>
    if (err.errors) {
      throw new Error(JSON.stringify(err))
    }
    throw new Error(
      (err.detail as string) ||
        (err.title as string) ||
        "Failed to validate patient fields",
    )
  }
  return res.data as PatientFieldValidationResult
}

export function usePatientFieldValidation(patientId: string | undefined) {
  return useQuery({
    queryKey: ["patients", patientId, "field-validation"],
    queryFn: () => getPatientFieldValidation(patientId!),
    enabled: !!patientId,
  })
}
