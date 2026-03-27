import { useState } from "react"
import { toast } from "sonner"
import { IconSearch } from "@tabler/icons-react"
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
import { useCreateWalkInVisitMutation } from "@/features/receptionist/api/receptionist-api"

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

export function WalkInVisitDialog({
  open,
  onOpenChange,
}: WalkInVisitDialogProps) {
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
      // Use fetch directly for patient search since we don't have a dedicated hook
      const response = await fetch(
        `/api/patients/search?q=${encodeURIComponent(query)}`,
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("accessToken") ?? ""}`,
          },
        },
      )
      if (response.ok) {
        const data = await response.json()
        const patients = Array.isArray(data) ? data : data?.data ?? []
        setSearchResults(
          patients.map((p: Record<string, unknown>) => ({
            id: p.id as string,
            name: (p.fullName ?? p.name ?? "") as string,
            code: (p.patientCode ?? p.code ?? null) as string | null,
            birthYear: (p.birthYear ?? null) as number | null,
            phone: (p.phone ?? "") as string,
          })),
        )
      }
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
          toast.success(`Da tao luot kham cho ${selectedPatient.name}`)
          handleClose()
        },
        onError: () => {
          toast.error("Khong the tao luot kham. Vui long thu lai.")
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
            Tao luot kham moi
          </DialogTitle>
          <DialogDescription className="sr-only">
            Tim va chon benh nhan de tao luot kham walk-in
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Patient search */}
          {!selectedPatient && (
            <div className="space-y-2">
              <Label>Tim benh nhan</Label>
              <div className="relative">
                <IconSearch className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  value={searchQuery}
                  onChange={(e) => handleSearch(e.target.value)}
                  placeholder="Tim theo SBT hoac ten BN..."
                  className="pl-9"
                />
              </div>

              {/* Search results */}
              {searchResults.length > 0 && (
                <div className="max-h-48 overflow-y-auto rounded-md border">
                  {searchResults.map((patient) => (
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
                      <div>
                        <div className="font-medium">{patient.name}</div>
                        <div className="text-xs text-muted-foreground">
                          {patient.code && (
                            <span className="font-mono">{patient.code}</span>
                          )}
                          {patient.code && patient.birthYear && " - "}
                          {patient.birthYear && <span>{patient.birthYear}</span>}
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              )}

              {isSearching && (
                <div className="text-sm text-muted-foreground text-center py-2">
                  Dang tim kiem...
                </div>
              )}

              {searchQuery.length >= 2 &&
                !isSearching &&
                searchResults.length === 0 && (
                  <div className="text-sm text-muted-foreground text-center py-2">
                    Khong tim thay benh nhan nao
                  </div>
                )}
            </div>
          )}

          {/* Selected patient info */}
          {selectedPatient && (
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <Label>Benh nhan</Label>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setSelectedPatient(null)}
                  className="h-auto px-2 py-1 text-xs"
                >
                  Doi
                </Button>
              </div>
              <div className="flex items-center gap-3 rounded-md border bg-muted/50 p-3">
                <Avatar className="h-10 w-10">
                  <AvatarFallback
                    className="text-sm font-semibold"
                    style={{
                      backgroundColor: "var(--avatar-complete-bg)",
                      color: "var(--avatar-complete-text)",
                    }}
                  >
                    {getInitials(selectedPatient.name)}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <div className="text-sm font-semibold">
                    {selectedPatient.name}
                  </div>
                  <div className="text-xs text-muted-foreground">
                    {selectedPatient.code && (
                      <span className="font-mono">{selectedPatient.code}</span>
                    )}
                    {selectedPatient.code && selectedPatient.birthYear && " - "}
                    {selectedPatient.birthYear && (
                      <span>{selectedPatient.birthYear}</span>
                    )}
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Reason field */}
          <div className="space-y-2">
            <Label>Ly do kham</Label>
            <Textarea
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              rows={2}
              className="resize-none"
            />
          </div>
        </div>

        <DialogFooter className="gap-2 sm:justify-end">
          <Button variant="ghost" onClick={handleClose}>
            Huy
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={!selectedPatient || createWalkIn.isPending}
            style={{ backgroundColor: "#534AB7", color: "white" }}
            className="hover:opacity-90"
          >
            {createWalkIn.isPending
              ? "Dang xu ly..."
              : "Xac nhan tao luot kham"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
