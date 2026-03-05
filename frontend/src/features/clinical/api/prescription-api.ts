import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { api } from "@/shared/lib/api-client"
import { clinicalKeys } from "./clinical-api"

// -- Types matching backend DTOs --

export interface DrugCatalogItemDto {
  id: string
  name: string
  nameVi: string
  genericName: string
  form: number
  strength: string | null
  route: number
  unit: string
  defaultDosageTemplate: string | null
  isActive: boolean
}

export interface PrescriptionItemInput {
  drugCatalogItemId: string | null
  drugName: string
  genericName: string | null
  strength: string | null
  form: number
  route: number
  dosage: string | null
  dosageOverride: string | null
  quantity: number
  unit: string
  frequency: string | null
  durationDays: number | null
  hasAllergyWarning: boolean
}

// -- Query key factories --

export const prescriptionKeys = {
  drug: (visitId: string) => ["prescriptions", "drug", visitId] as const,
  optical: (visitId: string) => ["prescriptions", "optical", visitId] as const,
  drugCatalogSearch: (term: string) =>
    ["drug-catalog", "search", term] as const,
  checkAllergy: (patientId: string, drugName: string, genericName: string) =>
    ["check-allergy", patientId, drugName, genericName] as const,
}

// -- Drug Catalog Search --

async function searchDrugCatalog(
  term: string,
): Promise<DrugCatalogItemDto[]> {
  const { data, error } = await api.GET("/api/pharmacy/drugs/search" as never, {
    params: { query: { term } },
  } as never)
  if (error) throw new Error("Failed to search drug catalog")
  return (data as DrugCatalogItemDto[]) ?? []
}

export function useDrugCatalogSearch(term: string) {
  return useQuery({
    queryKey: prescriptionKeys.drugCatalogSearch(term),
    queryFn: () => searchDrugCatalog(term),
    enabled: term.length >= 2,
  })
}

// -- Drug Allergy Check --

export interface AllergyCheckResult {
  id: string
  name: string
  severity: string
}

async function checkDrugAllergy(
  visitId: string,
  patientId: string,
  drugName: string,
  genericName: string | null,
): Promise<AllergyCheckResult[]> {
  const params: Record<string, string> = { patientId, drugName }
  if (genericName) params.genericName = genericName

  const { data, error } = await api.GET(
    `/api/clinical/${visitId}/check-drug-allergy` as never,
    { params: { query: params } } as never,
  )
  if (error) throw new Error("Failed to check drug allergy")
  return (data as AllergyCheckResult[]) ?? []
}

export function useCheckDrugAllergy(
  visitId: string | undefined,
  patientId: string | undefined,
  drugName: string | undefined,
  genericName: string | null | undefined,
) {
  return useQuery({
    queryKey: prescriptionKeys.checkAllergy(
      patientId ?? "",
      drugName ?? "",
      genericName ?? "",
    ),
    queryFn: () =>
      checkDrugAllergy(visitId!, patientId!, drugName!, genericName ?? null),
    enabled: !!visitId && !!patientId && !!drugName && drugName.length > 0,
  })
}

// -- Drug Prescription CRUD --

async function addDrugPrescription(
  visitId: string,
  notes: string | null,
  items: PrescriptionItemInput[],
): Promise<string> {
  const { data, error, response } = await api.POST(
    `/api/clinical/${visitId}/drug-prescriptions` as never,
    {
      body: { visitId, notes, items },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to add drug prescription")
  }
  return (data as { id: string }).id
}

async function updateDrugPrescription(
  visitId: string,
  prescriptionId: string,
  notes: string | null,
): Promise<void> {
  const { error, response } = await api.PUT(
    `/api/clinical/${visitId}/drug-prescriptions/${prescriptionId}` as never,
    {
      body: { visitId, prescriptionId, notes },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update drug prescription")
  }
}

async function removeDrugPrescription(
  visitId: string,
  prescriptionId: string,
): Promise<void> {
  const { error, response } = await api.DELETE(
    `/api/clinical/${visitId}/drug-prescriptions/${prescriptionId}` as never,
    {} as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to remove drug prescription")
  }
}

export function useAddDrugPrescription() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      notes,
      items,
    }: {
      visitId: string
      notes: string | null
      items: PrescriptionItemInput[]
    }) => addDrugPrescription(visitId, notes, items),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
    onError: () => {
      toast.error("Failed to add drug prescription")
    },
  })
}

export function useUpdateDrugPrescription() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      prescriptionId,
      notes,
    }: {
      visitId: string
      prescriptionId: string
      notes: string | null
    }) => updateDrugPrescription(visitId, prescriptionId, notes),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
    onError: () => {
      toast.error("Failed to update drug prescription")
    },
  })
}

export function useRemoveDrugPrescription() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      prescriptionId,
    }: {
      visitId: string
      prescriptionId: string
    }) => removeDrugPrescription(visitId, prescriptionId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
    onError: () => {
      toast.error("Failed to remove drug prescription")
    },
  })
}

// -- Optical Prescription CRUD --

async function addOpticalPrescription(
  visitId: string,
  data: {
    odSph?: number | null
    odCyl?: number | null
    odAxis?: number | null
    odAdd?: number | null
    osSph?: number | null
    osCyl?: number | null
    osAxis?: number | null
    osAdd?: number | null
    farPd?: number | null
    nearPd?: number | null
    nearOdSph?: number | null
    nearOdCyl?: number | null
    nearOdAxis?: number | null
    nearOsSph?: number | null
    nearOsCyl?: number | null
    nearOsAxis?: number | null
    lensType: number
    notes?: string | null
  },
): Promise<string> {
  const { data: result, error, response } = await api.POST(
    `/api/clinical/${visitId}/optical-prescription` as never,
    {
      body: { visitId, ...data },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to add optical prescription")
  }
  return (result as { id: string }).id
}

async function updateOpticalPrescription(
  visitId: string,
  prescriptionId: string,
  data: {
    odSph?: number | null
    odCyl?: number | null
    odAxis?: number | null
    odAdd?: number | null
    osSph?: number | null
    osCyl?: number | null
    osAxis?: number | null
    osAdd?: number | null
    farPd?: number | null
    nearPd?: number | null
    nearOdSph?: number | null
    nearOdCyl?: number | null
    nearOdAxis?: number | null
    nearOsSph?: number | null
    nearOsCyl?: number | null
    nearOsAxis?: number | null
    lensType: number
    notes?: string | null
  },
): Promise<void> {
  const { error, response } = await api.PUT(
    `/api/clinical/${visitId}/optical-prescription/${prescriptionId}` as never,
    {
      body: { visitId, prescriptionId, ...data },
    } as never,
  )
  if (error || !response.ok) {
    const err = error as Record<string, unknown> | undefined
    if (err?.errors) throw new Error(JSON.stringify(err))
    throw new Error("Failed to update optical prescription")
  }
}

export function useAddOpticalPrescription() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      ...data
    }: {
      visitId: string
      odSph?: number | null
      odCyl?: number | null
      odAxis?: number | null
      odAdd?: number | null
      osSph?: number | null
      osCyl?: number | null
      osAxis?: number | null
      osAdd?: number | null
      farPd?: number | null
      nearPd?: number | null
      nearOdSph?: number | null
      nearOdCyl?: number | null
      nearOdAxis?: number | null
      nearOsSph?: number | null
      nearOsCyl?: number | null
      nearOsAxis?: number | null
      lensType: number
      notes?: string | null
    }) => addOpticalPrescription(visitId, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
    onError: () => {
      toast.error("Failed to add optical prescription")
    },
  })
}

export function useUpdateOpticalPrescription() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      visitId,
      prescriptionId,
      ...data
    }: {
      visitId: string
      prescriptionId: string
      odSph?: number | null
      odCyl?: number | null
      odAxis?: number | null
      odAdd?: number | null
      osSph?: number | null
      osCyl?: number | null
      osAxis?: number | null
      osAdd?: number | null
      farPd?: number | null
      nearPd?: number | null
      nearOdSph?: number | null
      nearOdCyl?: number | null
      nearOdAxis?: number | null
      nearOsSph?: number | null
      nearOsCyl?: number | null
      nearOsAxis?: number | null
      lensType: number
      notes?: string | null
    }) => updateOpticalPrescription(visitId, prescriptionId, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: clinicalKeys.visit(variables.visitId),
      })
    },
    onError: () => {
      toast.error("Failed to update optical prescription")
    },
  })
}
