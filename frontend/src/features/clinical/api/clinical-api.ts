import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"

// -- Types matching backend Clinical.Contracts.Dtos --

export interface ActiveVisitDto {
  id: string
  patientId: string
  patientName: string
  doctorName: string
  currentStage: number
  visitDate: string
  hasAllergies: boolean
  waitMinutes: number
}

export interface RefractionDto {
  id: string
  visitId: string
  type: number
  odSph: number | null
  odCyl: number | null
  odAxis: number | null
  odAdd: number | null
  odPd: number | null
  osSph: number | null
  osCyl: number | null
  osAxis: number | null
  osAdd: number | null
  osPd: number | null
  ucvaOd: number | null
  ucvaOs: number | null
  bcvaOd: number | null
  bcvaOs: number | null
  iopOd: number | null
  iopOs: number | null
  iopMethod: number | null
  axialLengthOd: number | null
  axialLengthOs: number | null
}

export interface VisitDiagnosisDto {
  id: string
  icd10Code: string
  descriptionEn: string
  descriptionVi: string
  laterality: number
  role: number
  sortOrder: number
}

export interface VisitAmendmentDto {
  id: string
  amendedById: string
  amendedByName: string
  reason: string
  fieldChangesJson: string
  amendedAt: string
}

export interface DryEyeAssessmentDto {
  id: string
  visitId: string
  odTbut: number | null
  osTbut: number | null
  odSchirmer: number | null
  osSchirmer: number | null
  odMeibomianGrading: number | null
  osMeibomianGrading: number | null
  odTearMeniscus: number | null
  osTearMeniscus: number | null
  odStaining: number | null
  osStaining: number | null
  osdiScore: number | null
  osdiSeverity: number | null
}

export interface OsdiHistoryDto {
  visitId: string
  visitDate: string
  osdiScore: number
  severity: number
}

export interface OsdiHistoryResponse {
  items: OsdiHistoryDto[]
}

export interface DryEyeComparisonVisitData {
  visitId: string
  visitDate: string
  assessment: DryEyeAssessmentDto | null
}

export interface DryEyeComparisonDto {
  visit1: DryEyeComparisonVisitData
  visit2: DryEyeComparisonVisitData
}

export interface OsdiLinkResponse {
  token: string
  url: string
  expiresAt: string
}

export interface VisitDetailDto {
  id: string
  patientId: string
  patientName: string
  doctorId: string
  doctorName: string
  currentStage: number
  status: number
  visitDate: string
  examinationNotes: string | null
  refractions: RefractionDto[]
  diagnoses: VisitDiagnosisDto[]
  amendments: VisitAmendmentDto[]
  dryEyeAssessments: DryEyeAssessmentDto[]
  signedAt: string | null
  signedById: string | null
  appointmentId: string | null
}

export interface Icd10SearchResultDto {
  code: string
  descriptionEn: string
  descriptionVi: string
  category: string
  requiresLaterality: boolean
  isFavorite: boolean
}

export interface CreateVisitCommand {
  patientId: string
  patientName: string
  doctorId: string
  doctorName: string
  hasAllergies: boolean
  appointmentId?: string | null
}

// -- Query key factory --

// -- Medical Image types --

export interface MedicalImageDto {
  id: string
  visitId: string
  type: number
  eyeTag: number | null
  fileName: string
  url: string
  contentType: string
  fileSize: number
  description: string | null
  createdAt: string
}

export interface ImageComparisonResponse {
  visit1Images: MedicalImageDto[]
  visit2Images: MedicalImageDto[]
}

// ImageType enum matching backend Clinical.Domain.Enums.ImageType
export const ImageType = {
  Fluorescein: 0,
  Meibography: 1,
  OCT: 2,
  SpecularMicroscopy: 3,
  Topography: 4,
  Video: 5,
} as const

export type ImageTypeValue = (typeof ImageType)[keyof typeof ImageType]

// EyeTag enum matching backend Clinical.Domain.Enums.EyeTag
export const EyeTag = {
  OD: 0,
  OS: 1,
  OU: 2,
} as const

export type EyeTagValue = (typeof EyeTag)[keyof typeof EyeTag]

export const IMAGE_TYPE_LABELS: Record<number, { en: string; vi: string }> = {
  [ImageType.Fluorescein]: { en: "Fluorescein", vi: "Fluorescein" },
  [ImageType.Meibography]: { en: "Meibography", vi: "Meibography" },
  [ImageType.OCT]: { en: "OCT", vi: "OCT" },
  [ImageType.SpecularMicroscopy]: { en: "Specular Microscopy", vi: "Kính hiển vi phản xạ gương" },
  [ImageType.Topography]: { en: "Topography", vi: "Địa hình giác mạc" },
  [ImageType.Video]: { en: "Video", vi: "Video" },
}

export const EYE_TAG_LABELS: Record<number, { en: string; vi: string }> = {
  [EyeTag.OD]: { en: "OD", vi: "MP" },
  [EyeTag.OS]: { en: "OS", vi: "MT" },
  [EyeTag.OU]: { en: "OU", vi: "2M" },
}

// Allowed content types matching backend AllowedContentTypes
export const ALLOWED_IMAGE_TYPES = [
  "image/jpeg", "image/png", "image/tiff", "image/bmp", "image/webp",
]
export const ALLOWED_VIDEO_TYPES = [
  "video/mp4", "video/quicktime", "video/x-msvideo",
]
export const ALL_ALLOWED_TYPES = [...ALLOWED_IMAGE_TYPES, ...ALLOWED_VIDEO_TYPES]

// Size limits
export const MAX_IMAGE_SIZE = 50 * 1024 * 1024 // 50MB
export const MAX_VIDEO_SIZE = 200 * 1024 * 1024 // 200MB
export const MAX_FILES_PER_BATCH = 20

export const clinicalKeys = {
  all: ["clinical"] as const,
  visits: () => [...clinicalKeys.all, "visits"] as const,
  visit: (id: string) => [...clinicalKeys.visits(), id] as const,
  activeVisits: () => [...clinicalKeys.all, "active-visits"] as const,
  icd10Search: (query: string) => [...clinicalKeys.all, "icd10", query] as const,
  doctorFavorites: (doctorId: string) =>
    [...clinicalKeys.all, "icd10-favorites", doctorId] as const,
  osdiHistory: (patientId: string) =>
    [...clinicalKeys.all, "osdi-history", patientId] as const,
  dryEyeComparison: (patientId: string, visitId1: string, visitId2: string) =>
    [...clinicalKeys.all, "dry-eye-comparison", patientId, visitId1, visitId2] as const,
  visitImages: (visitId: string) =>
    [...clinicalKeys.all, "visit-images", visitId] as const,
  imageComparison: (patientId: string, visitId1: string, visitId2: string, imageType: number) =>
    [...clinicalKeys.all, "image-comparison", patientId, visitId1, visitId2, imageType] as const,
}

// -- API functions --

async function getActiveVisits(): Promise<ActiveVisitDto[]> {
  const { data, error } = await api.GET("/api/clinical/active" as never)
  if (error) throw new Error("Failed to fetch active visits")
  return (data as ActiveVisitDto[]) ?? []
}

async function getVisitById(visitId: string): Promise<VisitDetailDto> {
  const { data, error } = await api.GET(
    `/api/clinical/${visitId}` as never,
  )
  if (error) {
    const err = error as Record<string, unknown>
    if (err.errors) throw new Error(JSON.stringify(err))
    throw new Error(
      (err.detail as string) || (err.title as string) || "Failed to fetch visit",
    )
  }
  return data as VisitDetailDto
}

async function createVisit(command: CreateVisitCommand): Promise<string> {
  const { data, error, response } = await api.POST("/api/clinical" as never, {
    body: command,
  } as never)
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create visit")
  }
  return (data as { id: string }).id
}

async function signOffVisit(visitId: string): Promise<void> {
  const { error, response } = await api.PUT(
    `/api/clinical/${visitId}/sign-off` as never,
    {} as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to sign off visit")
  }
}

async function amendVisit(
  visitId: string,
  reason: string,
  fieldChangesJson: string,
): Promise<void> {
  const { error, response } = await api.POST(
    `/api/clinical/${visitId}/amend` as never,
    {
      body: { visitId, reason, fieldChangesJson },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to amend visit")
  }
}

async function advanceWorkflowStage(
  visitId: string,
  newStage: number,
): Promise<void> {
  const { error, response } = await api.PUT(
    `/api/clinical/${visitId}/stage` as never,
    {
      body: { visitId, newStage },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to advance workflow stage")
  }
}

async function updateVisitNotes(
  visitId: string,
  notes: string | null,
): Promise<void> {
  const { error, response } = await api.PUT(
    `/api/clinical/${visitId}/notes` as never,
    {
      body: { visitId, notes },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update visit notes")
  }
}

async function updateRefraction(
  visitId: string,
  data: {
    refractionType: number
    odSph?: number | null
    odCyl?: number | null
    odAxis?: number | null
    odAdd?: number | null
    odPd?: number | null
    osSph?: number | null
    osCyl?: number | null
    osAxis?: number | null
    osAdd?: number | null
    osPd?: number | null
    ucvaOd?: number | null
    ucvaOs?: number | null
    bcvaOd?: number | null
    bcvaOs?: number | null
    iopOd?: number | null
    iopOs?: number | null
    iopMethod?: number | null
    axialLengthOd?: number | null
    axialLengthOs?: number | null
  },
): Promise<void> {
  const { error, response } = await api.PUT(
    `/api/clinical/${visitId}/refraction` as never,
    {
      body: { visitId, ...data },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update refraction")
  }
}

async function addVisitDiagnosis(
  visitId: string,
  data: {
    icd10Code: string
    descriptionEn: string
    descriptionVi: string
    laterality: number
    role: number
    sortOrder: number
  },
): Promise<void> {
  const { error, response } = await api.POST(
    `/api/clinical/${visitId}/diagnoses` as never,
    {
      body: { visitId, ...data },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to add diagnosis")
  }
}

async function removeVisitDiagnosis(
  visitId: string,
  diagnosisId: string,
): Promise<void> {
  const { error, response } = await api.DELETE(
    `/api/clinical/${visitId}/diagnoses/${diagnosisId}` as never,
    {} as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to remove diagnosis")
  }
}

async function searchIcd10Codes(
  term: string,
): Promise<Icd10SearchResultDto[]> {
  const { data, error } = await api.GET("/api/clinical/icd10/search" as never, {
    params: { query: { term } },
  } as never)
  if (error) throw new Error("Failed to search ICD-10 codes")
  return (data as Icd10SearchResultDto[]) ?? []
}

async function toggleIcd10Favorite(
  doctorId: string,
  icd10Code: string,
): Promise<void> {
  const { error, response } = await api.POST(
    "/api/clinical/icd10/favorites/toggle" as never,
    {
      body: { doctorId, icd10Code },
    } as never,
  )
  if (error || !response.ok) throw new Error("Failed to toggle favorite")
}

async function getDoctorFavorites(
  doctorId: string,
): Promise<Icd10SearchResultDto[]> {
  const { data, error } = await api.GET(
    "/api/clinical/icd10/favorites" as never,
    {
      params: { query: { doctorId } },
    } as never,
  )
  if (error) throw new Error("Failed to fetch doctor favorites")
  return (data as Icd10SearchResultDto[]) ?? []
}

// -- TanStack Query hooks --

export function useActiveVisits() {
  return useQuery({
    queryKey: clinicalKeys.activeVisits(),
    queryFn: getActiveVisits,
    refetchInterval: 30_000,
    refetchIntervalInBackground: false,
    retry: 1,
  })
}

export function useVisitById(visitId: string | undefined) {
  return useQuery({
    queryKey: clinicalKeys.visit(visitId ?? ""),
    queryFn: () => getVisitById(visitId!),
    enabled: !!visitId,
  })
}

export function useIcd10Search(term: string) {
  return useQuery({
    queryKey: clinicalKeys.icd10Search(term),
    queryFn: () => searchIcd10Codes(term),
    enabled: term.length >= 2,
  })
}

export function useDoctorFavorites(doctorId: string | undefined) {
  return useQuery({
    queryKey: clinicalKeys.doctorFavorites(doctorId ?? ""),
    queryFn: () => getDoctorFavorites(doctorId!),
    enabled: !!doctorId,
  })
}

export function useCreateVisit() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: createVisit,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clinicalKeys.activeVisits() })
    },
  })
}

export function useAdvanceStage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      newStage,
    }: {
      visitId: string
      newStage: number
    }) => advanceWorkflowStage(visitId, newStage),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clinicalKeys.activeVisits() })
    },
  })
}

export function useSignOffVisit() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: signOffVisit,
    onSuccess: (_data, visitId) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(visitId),
      })
      queryClient.invalidateQueries({ queryKey: clinicalKeys.activeVisits() })
    },
  })
}

export function useAmendVisit() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      reason,
      fieldChangesJson,
    }: {
      visitId: string
      reason: string
      fieldChangesJson: string
    }) => amendVisit(visitId, reason, fieldChangesJson),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
  })
}

export function useUpdateNotes() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      notes,
    }: {
      visitId: string
      notes: string | null
    }) => updateVisitNotes(visitId, notes),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
  })
}

export function useUpdateRefraction() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      ...data
    }: {
      visitId: string
      refractionType: number
      odSph?: number | null
      odCyl?: number | null
      odAxis?: number | null
      odAdd?: number | null
      odPd?: number | null
      osSph?: number | null
      osCyl?: number | null
      osAxis?: number | null
      osAdd?: number | null
      osPd?: number | null
      ucvaOd?: number | null
      ucvaOs?: number | null
      bcvaOd?: number | null
      bcvaOs?: number | null
      iopOd?: number | null
      iopOs?: number | null
      iopMethod?: number | null
      axialLengthOd?: number | null
      axialLengthOs?: number | null
    }) => updateRefraction(visitId, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
  })
}

export function useAddDiagnosis() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      ...data
    }: {
      visitId: string
      icd10Code: string
      descriptionEn: string
      descriptionVi: string
      laterality: number
      role: number
      sortOrder: number
    }) => addVisitDiagnosis(visitId, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
  })
}

export function useRemoveDiagnosis() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      diagnosisId,
    }: {
      visitId: string
      diagnosisId: string
    }) => removeVisitDiagnosis(visitId, diagnosisId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
  })
}

export function useToggleIcd10Favorite() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      doctorId,
      icd10Code,
    }: {
      doctorId: string
      icd10Code: string
    }) => toggleIcd10Favorite(doctorId, icd10Code),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.doctorFavorites(variables.doctorId),
      })
      queryClient.invalidateQueries({ queryKey: clinicalKeys.all })
    },
  })
}

// -- Dry Eye API functions --

async function updateDryEye(
  visitId: string,
  data: {
    odTbut?: number | null
    osTbut?: number | null
    odSchirmer?: number | null
    osSchirmer?: number | null
    odMeibomianGrading?: number | null
    osMeibomianGrading?: number | null
    odTearMeniscus?: number | null
    osTearMeniscus?: number | null
    odStaining?: number | null
    osStaining?: number | null
  },
): Promise<void> {
  const { error, response } = await api.PUT(
    `/api/clinical/${visitId}/dry-eye` as never,
    {
      body: { visitId, ...data },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update dry eye assessment")
  }
}

async function getOsdiHistory(patientId: string): Promise<OsdiHistoryResponse> {
  const { data, error } = await api.GET(
    `/api/clinical/osdi-history/${patientId}` as never,
  )
  if (error) throw new Error("Failed to fetch OSDI history")
  return data as OsdiHistoryResponse
}

async function getDryEyeComparison(
  patientId: string,
  visitId1: string,
  visitId2: string,
): Promise<DryEyeComparisonDto> {
  const { data, error } = await api.GET(
    "/api/clinical/dry-eye-comparison" as never,
    {
      params: { query: { patientId, visitId1, visitId2 } },
    } as never,
  )
  if (error) throw new Error("Failed to fetch dry eye comparison")
  return data as DryEyeComparisonDto
}

async function generateOsdiLink(visitId: string): Promise<OsdiLinkResponse> {
  const { data, error, response } = await api.POST(
    `/api/clinical/${visitId}/osdi-link` as never,
    {} as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to generate OSDI link")
  }
  return data as OsdiLinkResponse
}

// -- Dry Eye hooks --

export function useUpdateDryEye() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      ...data
    }: {
      visitId: string
      odTbut?: number | null
      osTbut?: number | null
      odSchirmer?: number | null
      osSchirmer?: number | null
      odMeibomianGrading?: number | null
      osMeibomianGrading?: number | null
      odTearMeniscus?: number | null
      osTearMeniscus?: number | null
      odStaining?: number | null
      osStaining?: number | null
    }) => updateDryEye(visitId, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
  })
}

export function useOsdiHistory(patientId: string | undefined) {
  return useQuery({
    queryKey: clinicalKeys.osdiHistory(patientId ?? ""),
    queryFn: () => getOsdiHistory(patientId!),
    enabled: !!patientId,
  })
}

export function useDryEyeComparison(
  patientId: string | undefined,
  visitId1: string | undefined,
  visitId2: string | undefined,
) {
  return useQuery({
    queryKey: clinicalKeys.dryEyeComparison(
      patientId ?? "",
      visitId1 ?? "",
      visitId2 ?? "",
    ),
    queryFn: () => getDryEyeComparison(patientId!, visitId1!, visitId2!),
    enabled: !!patientId && !!visitId1 && !!visitId2,
  })
}

export function useGenerateOsdiLink() {
  return useMutation({
    mutationFn: (visitId: string) => generateOsdiLink(visitId),
  })
}

// -- Medical Image API functions (native fetch + FormData, NOT openapi-fetch) --

const API_URL = (import.meta as never as { env: Record<string, string> }).env?.VITE_API_URL ?? "http://localhost:5255"

export async function uploadMedicalImage(
  visitId: string,
  file: File,
  imageType: number,
  eyeTag?: number | null,
): Promise<string> {
  const formData = new FormData()
  formData.append("file", file)
  formData.append("imageType", imageType.toString())
  if (eyeTag != null) {
    formData.append("eyeTag", eyeTag.toString())
  }

  const token = (await import("@/shared/stores/authStore")).useAuthStore.getState().accessToken
  const res = await fetch(`${API_URL}/api/clinical/${visitId}/images`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: formData,
    credentials: "include",
  })
  if (!res.ok) {
    const body = await res.json().catch(() => null)
    throw new Error(body?.detail || body?.title || "Failed to upload image")
  }
  const data = await res.json()
  return data.id ?? data
}

async function getVisitImages(visitId: string): Promise<MedicalImageDto[]> {
  const { data, error } = await api.GET(
    `/api/clinical/${visitId}/images` as never,
  )
  if (error) throw new Error("Failed to fetch visit images")
  return (data as MedicalImageDto[]) ?? []
}

async function deleteImage(imageId: string): Promise<void> {
  const { error, response } = await api.DELETE(
    `/api/clinical/images/${imageId}` as never,
    {} as never,
  )
  if (error || !response.ok) {
    throw new Error("Failed to delete image")
  }
}

async function getImageComparison(
  patientId: string,
  visitId1: string,
  visitId2: string,
  imageType: number,
): Promise<ImageComparisonResponse> {
  const { data, error } = await api.GET(
    "/api/clinical/image-comparison" as never,
    {
      params: {
        query: { patientId, visitId1, visitId2, imageType },
      },
    } as never,
  )
  if (error) throw new Error("Failed to fetch image comparison")
  return data as ImageComparisonResponse
}

// -- Medical Image hooks --

export function useVisitImages(visitId: string | undefined) {
  return useQuery({
    queryKey: clinicalKeys.visitImages(visitId ?? ""),
    queryFn: () => getVisitImages(visitId!),
    enabled: !!visitId,
    staleTime: 1000 * 60 * 30, // 30 min -- SAS URLs expire in 1 hour
  })
}

export function useUploadImage(visitId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      file,
      imageType,
      eyeTag,
    }: {
      file: File
      imageType: number
      eyeTag?: number | null
    }) => uploadMedicalImage(visitId, file, imageType, eyeTag),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visitImages(visitId),
      })
    },
  })
}

export function useDeleteImage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ imageId }: { imageId: string; visitId: string }) =>
      deleteImage(imageId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visitImages(variables.visitId),
      })
    },
  })
}

export function useImageComparison(
  patientId: string | undefined,
  visitId1: string | undefined,
  visitId2: string | undefined,
  imageType: number | undefined,
) {
  return useQuery({
    queryKey: clinicalKeys.imageComparison(
      patientId ?? "",
      visitId1 ?? "",
      visitId2 ?? "",
      imageType ?? -1,
    ),
    queryFn: () =>
      getImageComparison(patientId!, visitId1!, visitId2!, imageType!),
    enabled: !!patientId && !!visitId1 && !!visitId2 && imageType != null,
    staleTime: 1000 * 60 * 30,
  })
}
