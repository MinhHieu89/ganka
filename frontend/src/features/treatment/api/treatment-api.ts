import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { api } from "@/shared/lib/api-client"
import { treatmentKeys } from "./treatment-queries"
import type {
  TreatmentProtocolDto,
  TreatmentPackageDto,
  TreatmentSessionDto,
  RecordSessionResponse,
  TreatmentType,
  CreateProtocolTemplateCommand,
  UpdateProtocolTemplateCommand,
  CreateTreatmentPackageCommand,
  RecordTreatmentSessionCommand,
  ModifyTreatmentPackageCommand,
  SwitchTreatmentTypeCommand,
  RequestCancellationCommand,
  ApproveCancellationCommand,
  RejectCancellationCommand,
  PauseTreatmentPackageCommand,
} from "./treatment-types"

// -- API functions --

async function getProtocolTemplates(
  type?: TreatmentType,
  includeInactive?: boolean,
): Promise<TreatmentProtocolDto[]> {
  const query: Record<string, unknown> = {}
  if (type != null) {
    const typeMap: Record<TreatmentType, number> = { IPL: 0, LLLT: 1, LidCare: 2 }
    query.treatmentType = typeMap[type]
  }
  if (includeInactive != null) query.includeInactive = includeInactive
  const { data, error } = await api.GET("/api/treatments/protocols" as never, {
    params: { query },
  } as never)
  if (error) throw new Error("Failed to fetch protocol templates")
  return (data as TreatmentProtocolDto[]) ?? []
}

async function getProtocolTemplate(id: string): Promise<TreatmentProtocolDto> {
  const { data, error } = await api.GET(
    `/api/treatments/protocols/${id}` as never,
  )
  if (error) throw new Error("Failed to fetch protocol template")
  return data as TreatmentProtocolDto
}

async function getActiveTreatments(): Promise<TreatmentPackageDto[]> {
  const { data, error } = await api.GET(
    "/api/treatments/packages" as never,
  )
  if (error) throw new Error("Failed to fetch active treatments")
  return (data as TreatmentPackageDto[]) ?? []
}

async function getTreatmentPackage(id: string): Promise<TreatmentPackageDto> {
  const { data, error } = await api.GET(
    `/api/treatments/packages/${id}` as never,
  )
  if (error) throw new Error("Failed to fetch treatment package")
  return data as TreatmentPackageDto
}

async function getPatientTreatments(
  patientId: string,
): Promise<TreatmentPackageDto[]> {
  const { data, error } = await api.GET(
    `/api/treatments/patients/${patientId}/packages` as never,
  )
  if (error) throw new Error("Failed to fetch patient treatments")
  return (data as TreatmentPackageDto[]) ?? []
}

async function getDueSoonSessions(): Promise<TreatmentPackageDto[]> {
  const { data, error } = await api.GET(
    "/api/treatments/packages/due-soon" as never,
  )
  if (error) throw new Error("Failed to fetch due-soon sessions")
  return (data as TreatmentPackageDto[]) ?? []
}

async function getTreatmentSessions(
  packageId: string,
): Promise<TreatmentSessionDto[]> {
  const { data, error } = await api.GET(
    `/api/treatments/packages/${packageId}/sessions` as never,
  )
  if (error) throw new Error("Failed to fetch treatment sessions")
  return (data as TreatmentSessionDto[]) ?? []
}

async function getPendingCancellations(): Promise<TreatmentPackageDto[]> {
  const { data, error } = await api.GET(
    "/api/treatments/cancellations/pending" as never,
  )
  if (error) throw new Error("Failed to fetch pending cancellations")
  return (data as TreatmentPackageDto[]) ?? []
}

async function createProtocolTemplate(
  command: CreateProtocolTemplateCommand,
): Promise<TreatmentProtocolDto> {
  const { data, error, response } = await api.POST(
    "/api/treatments/protocols" as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create protocol template")
  }
  return data as TreatmentProtocolDto
}

async function updateProtocolTemplate(
  id: string,
  command: Omit<UpdateProtocolTemplateCommand, "id">,
): Promise<TreatmentProtocolDto> {
  const { data, error, response } = await api.PUT(
    `/api/treatments/protocols/${id}` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update protocol template")
  }
  return data as TreatmentProtocolDto
}

async function createTreatmentPackage(
  command: CreateTreatmentPackageCommand,
): Promise<TreatmentPackageDto> {
  const { data, error, response } = await api.POST(
    "/api/treatments/packages" as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to create treatment package")
  }
  return data as TreatmentPackageDto
}

async function recordSession(
  packageId: string,
  command: Omit<RecordTreatmentSessionCommand, "packageId">,
): Promise<RecordSessionResponse> {
  const { data, error, response } = await api.POST(
    `/api/treatments/packages/${packageId}/sessions` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to record treatment session")
  }
  return data as RecordSessionResponse
}

async function modifyPackage(
  packageId: string,
  command: Omit<ModifyTreatmentPackageCommand, "packageId">,
): Promise<TreatmentPackageDto> {
  const { data, error, response } = await api.PUT(
    `/api/treatments/packages/${packageId}/modify` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to modify treatment package")
  }
  return data as TreatmentPackageDto
}

async function switchTreatmentType(
  packageId: string,
  command: Omit<SwitchTreatmentTypeCommand, "packageId">,
): Promise<TreatmentPackageDto> {
  const { data, error, response } = await api.POST(
    `/api/treatments/packages/${packageId}/switch` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to switch treatment type")
  }
  return data as TreatmentPackageDto
}

async function pausePackage(
  packageId: string,
  command: Omit<PauseTreatmentPackageCommand, "packageId">,
): Promise<TreatmentPackageDto> {
  const { data, error, response } = await api.POST(
    `/api/treatments/packages/${packageId}/pause` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to pause/resume treatment package")
  }
  return data as TreatmentPackageDto
}

async function requestCancellation(
  packageId: string,
  command: Omit<RequestCancellationCommand, "packageId">,
): Promise<void> {
  const { error, response } = await api.POST(
    `/api/treatments/packages/${packageId}/cancel` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to request cancellation")
  }
}

async function approveCancellation(
  packageId: string,
  command: Omit<ApproveCancellationCommand, "packageId">,
): Promise<void> {
  const { error, response } = await api.POST(
    `/api/treatments/packages/${packageId}/cancel/approve` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to approve cancellation")
  }
}

async function rejectCancellation(
  packageId: string,
  command: Omit<RejectCancellationCommand, "packageId">,
): Promise<void> {
  const { error, response } = await api.POST(
    `/api/treatments/packages/${packageId}/cancel/reject` as never,
    { body: command } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to reject cancellation")
  }
}

// -- TanStack Query hooks --

export function useProtocolTemplates(type?: TreatmentType) {
  return useQuery({
    queryKey: treatmentKeys.templates(),
    queryFn: () => getProtocolTemplates(type),
  })
}

export function useProtocolTemplate(id: string | undefined) {
  return useQuery({
    queryKey: treatmentKeys.template(id ?? ""),
    queryFn: () => getProtocolTemplate(id!),
    enabled: !!id,
  })
}

export function useActiveTreatments() {
  return useQuery({
    queryKey: treatmentKeys.packages(),
    queryFn: getActiveTreatments,
  })
}

export function useTreatmentPackage(id: string | undefined) {
  return useQuery({
    queryKey: treatmentKeys.package(id ?? ""),
    queryFn: () => getTreatmentPackage(id!),
    enabled: !!id,
  })
}

export function usePatientTreatments(patientId: string | undefined) {
  return useQuery({
    queryKey: treatmentKeys.patientPackages(patientId ?? ""),
    queryFn: () => getPatientTreatments(patientId!),
    enabled: !!patientId,
  })
}

export function useDueSoonSessions() {
  return useQuery({
    queryKey: treatmentKeys.dueSoon(),
    queryFn: getDueSoonSessions,
  })
}

export function useTreatmentSessions(packageId: string | undefined) {
  return useQuery({
    queryKey: treatmentKeys.sessions(packageId ?? ""),
    queryFn: () => getTreatmentSessions(packageId!),
    enabled: !!packageId,
  })
}

export function usePendingCancellations() {
  return useQuery({
    queryKey: treatmentKeys.pendingCancellations(),
    queryFn: getPendingCancellations,
  })
}

// -- Mutation hooks --

export function useCreateProtocolTemplate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: CreateProtocolTemplateCommand) =>
      createProtocolTemplate(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: treatmentKeys.templates() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useUpdateProtocolTemplate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      id,
      ...command
    }: { id: string } & Omit<UpdateProtocolTemplateCommand, "id">) =>
      updateProtocolTemplate(id, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: treatmentKeys.templates() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useCreateTreatmentPackage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: CreateTreatmentPackageCommand) =>
      createTreatmentPackage(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: treatmentKeys.packages() })
      queryClient.invalidateQueries({ queryKey: treatmentKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useRecordSession(packageId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (
      command: Omit<RecordTreatmentSessionCommand, "packageId">,
    ) => recordSession(packageId, command),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: treatmentKeys.package(packageId),
      })
      queryClient.invalidateQueries({
        queryKey: treatmentKeys.sessions(packageId),
      })
      queryClient.invalidateQueries({ queryKey: treatmentKeys.dueSoon() })
      queryClient.invalidateQueries({ queryKey: treatmentKeys.packages() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useModifyPackage(packageId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (
      command: Omit<ModifyTreatmentPackageCommand, "packageId">,
    ) => modifyPackage(packageId, command),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: treatmentKeys.package(packageId),
      })
      queryClient.invalidateQueries({ queryKey: treatmentKeys.packages() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useSwitchTreatmentType() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      packageId,
      ...command
    }: SwitchTreatmentTypeCommand) =>
      switchTreatmentType(packageId, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: treatmentKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function usePausePackage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      packageId,
      ...command
    }: PauseTreatmentPackageCommand) =>
      pausePackage(packageId, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: treatmentKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useRequestCancellation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      packageId,
      ...command
    }: RequestCancellationCommand) =>
      requestCancellation(packageId, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: treatmentKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useApproveCancellation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      packageId,
      ...command
    }: ApproveCancellationCommand) =>
      approveCancellation(packageId, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: treatmentKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

export function useRejectCancellation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      packageId,
      ...command
    }: RejectCancellationCommand) =>
      rejectCancellation(packageId, command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: treatmentKeys.all })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}

// Re-export API functions for direct use
export {
  getProtocolTemplates,
  getProtocolTemplate,
  getActiveTreatments,
  getTreatmentPackage,
  getPatientTreatments,
  getDueSoonSessions,
  getTreatmentSessions,
  getPendingCancellations,
  createProtocolTemplate,
  updateProtocolTemplate,
  createTreatmentPackage,
  recordSession,
  modifyPackage,
  switchTreatmentType,
  pausePackage,
  requestCancellation,
  approveCancellation,
  rejectCancellation,
}
