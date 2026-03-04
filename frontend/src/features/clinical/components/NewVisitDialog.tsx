import { useState, useDeferredValue } from "react"
import { useTranslation } from "react-i18next"
import { IconSearch, IconUser } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import { DoctorSelector } from "@/features/scheduling/components/DoctorSelector"
import { usePatientSearch } from "@/features/patient/hooks/usePatientSearch"
import { usePatientById } from "@/features/patient/api/patient-api"
import type { CreateVisitCommand } from "@/features/clinical/api/clinical-api"

interface NewVisitDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onCreateVisit: (command: CreateVisitCommand) => void
  isCreating: boolean
}

export function NewVisitDialog({
  open,
  onOpenChange,
  onCreateVisit,
  isCreating,
}: NewVisitDialogProps) {
  const { t } = useTranslation("clinical")

  const [searchTerm, setSearchTerm] = useState("")
  const deferredSearch = useDeferredValue(searchTerm)
  const { data: searchResults } = usePatientSearch(deferredSearch, {
    enabled: deferredSearch.length >= 2,
  })

  const [selectedPatientId, setSelectedPatientId] = useState<string>()
  const [selectedDoctorId, setSelectedDoctorId] = useState<string>()
  const [selectedDoctorName, setSelectedDoctorName] = useState<string>("")

  const { data: selectedPatient } = usePatientById(selectedPatientId)

  const handleSelectPatient = (id: string) => {
    setSelectedPatientId(id)
    setSearchTerm("")
  }

  const handleSubmit = () => {
    if (!selectedPatient || !selectedDoctorId) return

    const hasAllergies = (selectedPatient.allergies?.length ?? 0) > 0

    onCreateVisit({
      patientId: selectedPatient.id,
      patientName: selectedPatient.fullName,
      doctorId: selectedDoctorId,
      doctorName: selectedDoctorName,
      hasAllergies,
    })

    // Reset after success (dialog closes via parent)
    setSelectedPatientId(undefined)
    setSelectedDoctorId(undefined)
    setSelectedDoctorName("")
    setSearchTerm("")
  }

  const handleOpenChange = (open: boolean) => {
    if (!open) {
      setSelectedPatientId(undefined)
      setSelectedDoctorId(undefined)
      setSelectedDoctorName("")
      setSearchTerm("")
    }
    onOpenChange(open)
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("newVisit.walkIn")}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Patient search */}
          <div className="space-y-2">
            <Label>{t("newVisit.selectPatient")}</Label>
            {selectedPatient ? (
              <div className="flex items-center gap-2 p-2 border rounded-md">
                <IconUser className="h-4 w-4 text-muted-foreground" />
                <div className="flex-1">
                  <div className="text-sm font-medium">
                    {selectedPatient.fullName}
                  </div>
                  <div className="text-xs text-muted-foreground">
                    {selectedPatient.phone}
                    {selectedPatient.patientCode && ` - ${selectedPatient.patientCode}`}
                  </div>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setSelectedPatientId(undefined)}
                >
                  &times;
                </Button>
              </div>
            ) : (
              <div className="space-y-1">
                <div className="relative">
                  <IconSearch className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                  <Input
                    className="pl-8"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                </div>
                {searchResults && searchResults.length > 0 && (
                  <div className="border rounded-md max-h-48 overflow-y-auto">
                    {searchResults.map((p) => (
                      <button
                        key={p.id}
                        type="button"
                        className="w-full text-left px-3 py-2 text-sm hover:bg-accent transition-colors"
                        onClick={() => handleSelectPatient(p.id)}
                      >
                        <div className="font-medium">{p.fullName}</div>
                        <div className="text-xs text-muted-foreground">
                          {p.phone}
                          {p.patientCode && ` - ${p.patientCode}`}
                        </div>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Doctor selector */}
          <div className="space-y-2">
            <Label>{t("newVisit.selectDoctor")}</Label>
            <DoctorSelector
              value={selectedDoctorId}
              onChange={(id, name) => {
                setSelectedDoctorId(id)
                setSelectedDoctorName(name)
              }}
            />
          </div>

          {/* Create button */}
          <Button
            className="w-full"
            onClick={handleSubmit}
            disabled={!selectedPatient || !selectedDoctorId || isCreating}
          >
            {t("newVisit.create")}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}
