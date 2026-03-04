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
  refractionType: number
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

export const clinicalKeys = {
  all: ["clinical"] as const,
  visits: () => [...clinicalKeys.all, "visits"] as const,
  visit: (id: string) => [...clinicalKeys.visits(), id] as const,
  activeVisits: () => [...clinicalKeys.all, "active-visits"] as const,
  icd10Search: (query: string) => [...clinicalKeys.all, "icd10", query] as const,
  doctorFavorites: (doctorId: string) =>
    [...clinicalKeys.all, "icd10-favorites", doctorId] as const,
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
    refetchInterval: 5000,
    refetchIntervalInBackground: false,
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
