import { useState } from "react"
import { toast } from "sonner"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import { IconSearch, IconUserPlus } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import { Textarea } from "@/shared/components/Textarea"
import { Avatar, AvatarFallback } from "@/shared/components/Avatar"
import { Badge } from "@/shared/components/Badge"
import { Skeleton } from "@/shared/components/Skeleton"
import { useCreateWalkInVisitMutation } from "@/features/receptionist/api/receptionist-api"
import { usePatientById } from "@/features/patient/api/patient-api"
import { usePatientVisitHistory } from "@/features/clinical/api/clinical-api"
import { api } from "@/shared/lib/api-client"

interface PatientResult {
  id: string
  name: string
  code: string | null
  birthYear: number | null
  phone?: string
}

interface WalkInVisitDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/)
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

function formatGender(gender: string | null, t: (key: string) => string): string | null {
  if (!gender) return null
  const map: Record<string, string> = {
    Male: t("patient:male"),
    Female: t("patient:female"),
    Other: t("patient:other"),
  }
  return map[gender] ?? null
}

function formatVisitDate(dateStr: string): string {
  try {
    return new Date(dateStr).toLocaleDateString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    })
  } catch {
    return dateStr
  }
}

function SelectedPatientCard({
  patient,
  onChange,
  t,
  reason,
  onReasonChange,
}: {
  patient: PatientResult
  onChange: () => void
  t: (key: string) => string
  reason: string
  onReasonChange: (v: string) => void
}) {
  const patientQuery = usePatientById(patient.id)
  const visitHistoryQuery = usePatientVisitHistory(patient.id)

  const full = patientQuery.data
  const lastVisit = visitHistoryQuery.data?.[0] ?? null
  const isLoading = patientQuery.isLoading || visitHistoryQuery.isLoading

  const birthYear = full?.dateOfBirth
    ? new Date(full.dateOfBirth).getFullYear()
    : patient.birthYear

  return (
    <div className="space-y-4">
      {/* Header: avatar + name + badge + change button */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Avatar className="h-12 w-12">
            <AvatarFallback
              className="text-sm font-semibold"
              style={{
                backgroundColor: "var(--avatar-complete-bg)",
                color: "var(--avatar-complete-text)",
              }}
            >
              {getInitials(patient.name)}
            </AvatarFallback>
          </Avatar>
          <div>
            <div className="text-sm font-semibold">{patient.name}</div>
            {(full?.patientCode ?? patient.code) && (
              <div className="font-mono text-xs text-muted-foreground">
                {full?.patientCode ?? patient.code}
              </div>
            )}
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="outline">{t("receptionist:walkIn.walkInBadge")}</Badge>
          <Button
            variant="ghost"
            size="sm"
            onClick={onChange}
            className="h-auto px-2 py-1 text-xs"
          >
            {t("receptionist:walkIn.change")}
          </Button>
        </div>
      </div>

      {/* Patient details card */}
      {isLoading ? (
        <div className="space-y-2">
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <>
          <div className="rounded-md border bg-muted/50 p-4 space-y-3 text-sm">
            {/* Row 1: Birth year + Gender */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <div className="text-xs text-muted-foreground">
                  {t("scheduling:checkIn.birthYear")}
                </div>
                {birthYear
                  ? <div className="font-semibold">{birthYear}</div>
                  : <div className="italic text-muted-foreground">{t("scheduling:checkIn.notAvailable")}</div>}
              </div>
              <div>
                <div className="text-xs text-muted-foreground">
                  {t("scheduling:checkIn.gender")}
                </div>
                {formatGender(full?.gender ?? null, t)
                  ? <div className="font-semibold">{formatGender(full?.gender ?? null, t)}</div>
                  : <div className="italic text-muted-foreground">{t("scheduling:checkIn.notAvailable")}</div>}
              </div>
            </div>

            {/* Row 2: Phone + Occupation */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <div className="text-xs text-muted-foreground">
                  {t("scheduling:checkIn.phone")}
                </div>
                {full?.phone
                  ? <div className="font-semibold">{full.phone}</div>
                  : <div className="italic text-muted-foreground">{t("scheduling:checkIn.notAvailable")}</div>}
              </div>
              <div>
                <div className="text-xs text-muted-foreground">
                  {t("scheduling:checkIn.occupation")}
                </div>
                {full?.occupation
                  ? <div className="font-semibold">{full.occupation}</div>
                  : <div className="italic text-muted-foreground">{t("scheduling:checkIn.notAvailable")}</div>}
              </div>
            </div>
          </div>

          {/* Last visit */}
          {lastVisit && (
            <div className="rounded-md border bg-muted/50 p-4 text-sm">
              <div className="text-xs text-muted-foreground">
                {t("scheduling:checkIn.lastVisit")}
              </div>
              <div className="font-semibold">
                {formatVisitDate(lastVisit.visitDate)}
                {lastVisit.primaryDiagnosisText &&
                  ` — ${lastVisit.primaryDiagnosisText}`}
                {lastVisit.doctorName && ` — ${lastVisit.doctorName}`}
              </div>
            </div>
          )}
        </>
      )}

      {/* Reason field */}
      <div className="space-y-2">
        <Label>
          {t("receptionist:walkIn.reason")} <span className="text-destructive">*</span>
        </Label>
        <Textarea
          value={reason}
          onChange={(e) => onReasonChange(e.target.value)}
          placeholder={t("receptionist:walkIn.reasonPlaceholder")}
          rows={2}
          className="resize-none"
        />
      </div>
    </div>
  )
}

export function WalkInVisitDialog({
  open,
  onOpenChange,
}: WalkInVisitDialogProps) {
  const { t } = useTranslation(["receptionist", "scheduling", "patient", "common"])
  const { t: tCommon } = useTranslation("common")
  const navigate = useNavigate()
  const [searchQuery, setSearchQuery] = useState("")
  const [selectedPatient, setSelectedPatient] = useState<PatientResult | null>(
    null,
  )
  const [reason, setReason] = useState("")
  const [searchResults, setSearchResults] = useState<PatientResult[]>([])
  const [isSearching, setIsSearching] = useState(false)

  const createWalkIn = useCreateWalkInVisitMutation()

  const handleSearch = async (query: string) => {
    setSearchQuery(query)
    if (query.length < 2) {
      setSearchResults([])
      return
    }

    setIsSearching(true)
    try {
      const { data } = await api.GET("/api/patients/search" as never, {
        params: { query: { term: query } },
      } as never)
      const patients = Array.isArray(data) ? data : ((data as unknown as Record<string, unknown>)?.data as Record<string, unknown>[]) ?? []
      setSearchResults(
        patients.map((p: Record<string, unknown>) => ({
          id: p.id as string,
          name: (p.fullName ?? p.name ?? "") as string,
          code: (p.patientCode ?? p.code ?? null) as string | null,
          birthYear: (p.birthYear ?? null) as number | null,
          phone: (p.phone ?? "") as string,
        })),
      )
    } catch {
      // Silently fail search
    } finally {
      setIsSearching(false)
    }
  }

  const handleSelectPatient = (patient: PatientResult) => {
    setSelectedPatient(patient)
    setSearchQuery("")
    setSearchResults([])
  }

  const handleConfirm = () => {
    if (!selectedPatient) return

    createWalkIn.mutate(
      {
        patientId: selectedPatient.id,
        reason: reason || undefined,
      },
      {
        onSuccess: () => {
          toast.success(t("walkIn.successToast", { name: selectedPatient.name }))
          handleClose()
        },
        onError: () => {
          toast.error(t("walkIn.errorToast"))
        },
      },
    )
  }

  const handleClose = () => {
    setSearchQuery("")
    setSelectedPatient(null)
    setReason("")
    setSearchResults([])
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold">
            {t("walkIn.title")}
          </DialogTitle>
          <DialogDescription className="sr-only">
            {t("walkIn.description")}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Patient search */}
          {!selectedPatient && (
            <div className="space-y-2">
              <Label>{t("walkIn.searchLabel")}</Label>
              <div className="relative">
                <IconSearch className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  value={searchQuery}
                  onChange={(e) => handleSearch(e.target.value)}
                  placeholder={t("walkIn.searchPlaceholder")}
                  className="pl-9"
                />
              </div>

              {/* Search results — fixed height to prevent dialog resize */}
              {searchQuery.length >= 2 && (
                <div className="h-48 overflow-y-auto rounded-md border">
                  {isSearching ? (
                    <div className="flex h-full items-center justify-center text-sm text-muted-foreground">
                      {t("walkIn.searching")}
                    </div>
                  ) : searchResults.length > 0 ? (
                    searchResults.map((patient) => (
                      <button
                        key={patient.id}
                        type="button"
                        onClick={() => handleSelectPatient(patient)}
                        className="flex w-full items-center gap-3 px-3 py-2 text-left text-sm hover:bg-accent transition-colors"
                      >
                        <Avatar className="h-8 w-8">
                          <AvatarFallback className="text-xs">
                            {getInitials(patient.name)}
                          </AvatarFallback>
                        </Avatar>
                        <div className="min-w-0 flex-1">
                          <div className="font-medium">{patient.name}</div>
                          <div className="flex flex-wrap items-center gap-x-1 text-xs text-muted-foreground">
                            {patient.code && (
                              <span><span className="font-mono">{patient.code}</span></span>
                            )}
                            {patient.birthYear && (
                              <span>· NS: {patient.birthYear}</span>
                            )}
                            {patient.phone && (
                              <span>· SĐT: {patient.phone}</span>
                            )}
                          </div>
                        </div>
                      </button>
                    ))
                  ) : (
                    <div className="flex h-full flex-col items-center justify-center gap-3">
                      <p className="text-sm text-muted-foreground">
                        {t("walkIn.noResults")}
                      </p>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => {
                          handleClose()
                          navigate({ to: "/patients/intake" as string })
                        }}
                      >
                        <IconUserPlus className="mr-2 h-4 w-4" />
                        {t("walkIn.goToIntake")}
                      </Button>
                    </div>
                  )}
                </div>
              )}
            </div>
          )}

          {/* Selected patient info */}
          {selectedPatient && (
            <SelectedPatientCard
              patient={selectedPatient}
              onChange={() => setSelectedPatient(null)}
              t={t}
              reason={reason}
              onReasonChange={setReason}
            />
          )}
        </div>

        <DialogFooter className="gap-2 sm:justify-end">
          <Button variant="ghost" onClick={handleClose}>
            {tCommon("buttons.cancel")}
          </Button>
          {selectedPatient ? (
            <Button
              onClick={handleConfirm}
              disabled={createWalkIn.isPending}
            >
              {createWalkIn.isPending
                ? tCommon("status.processing")
                : t("walkIn.confirm")}
            </Button>
          ) : (
            <Button
              onClick={() => {
                handleClose()
                navigate({ to: "/patients/intake" as string })
              }}
            >
              <IconUserPlus className="mr-2 h-4 w-4" />
              {t("walkIn.goToIntake")}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
