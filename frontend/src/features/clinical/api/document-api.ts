import { useAuthStore } from "@/shared/stores/authStore"

const API_URL =
  (import.meta as never as { env: Record<string, string> }).env?.VITE_API_URL ??
  "http://localhost:5255"

async function fetchPdf(url: string): Promise<Blob> {
  const token = useAuthStore.getState().accessToken
  const res = await fetch(url, {
    headers: { Authorization: `Bearer ${token}` },
    credentials: "include",
  })
  if (!res.ok) {
    throw new Error("Failed to generate PDF")
  }
  return res.blob()
}

export async function generateDrugPrescriptionPdf(
  visitId: string,
): Promise<Blob> {
  return fetchPdf(`${API_URL}/api/clinical/${visitId}/print/drug-rx`)
}

export async function generateOpticalPrescriptionPdf(
  visitId: string,
): Promise<Blob> {
  return fetchPdf(`${API_URL}/api/clinical/${visitId}/print/optical-rx`)
}

export async function generateReferralLetterPdf(
  visitId: string,
  reason: string,
  to: string,
): Promise<Blob> {
  const params = new URLSearchParams({ reason, to })
  return fetchPdf(
    `${API_URL}/api/clinical/${visitId}/print/referral-letter?${params}`,
  )
}

export async function generateConsentFormPdf(
  visitId: string,
  procedureType: string,
): Promise<Blob> {
  const params = new URLSearchParams({ procedureType })
  return fetchPdf(
    `${API_URL}/api/clinical/${visitId}/print/consent-form?${params}`,
  )
}

export async function generatePharmacyLabelPdf(
  prescriptionItemId: string,
): Promise<Blob> {
  return fetchPdf(
    `${API_URL}/api/clinical/prescription-items/${prescriptionItemId}/print/label`,
  )
}
