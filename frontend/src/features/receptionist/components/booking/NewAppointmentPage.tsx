import { useState, useCallback, useMemo, useEffect } from "react"
import { useNavigate } from "@tanstack/react-router"
import { toast } from "sonner"
import { IconSearch, IconInfoCircle, IconAlertTriangle } from "@tabler/icons-react"
import { Calendar } from "@/shared/components/ui/calendar"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import { Textarea } from "@/shared/components/Textarea"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/shared/components/Command"
import { Popover, PopoverContent, PopoverTrigger } from "@/shared/components/Popover"
import { usePatientSearch } from "@/features/patient/hooks/usePatientSearch"
import type { PatientSearchResult } from "@/features/patient/hooks/usePatientSearch"
import { useDoctors } from "@/features/scheduling/components/DoctorSelector"
import {
  useAvailableSlots,
  useBookGuestMutation,
} from "@/features/receptionist/api/receptionist-api"
import { useBookAppointment } from "@/features/scheduling/api/scheduling-api"
import { TimeSlotGrid } from "./TimeSlotGrid"
import { ConfirmationBar } from "./ConfirmationBar"

const NO_DOCTOR_VALUE = "__no_doctor__"

interface NewAppointmentPageProps {
  initialPatientId?: string
}

export function NewAppointmentPage({ initialPatientId }: NewAppointmentPageProps) {
  const navigate = useNavigate()

  // Patient search state
  const [searchTerm, setSearchTerm] = useState("")
  const [searchOpen, setSearchOpen] = useState(false)
  const [selectedPatient, setSelectedPatient] = useState<PatientSearchResult | null>(null)
  const [isNewPatient, setIsNewPatient] = useState(false)

  // Guest fields
  const [guestName, setGuestName] = useState("")
  const [guestPhone, setGuestPhone] = useState("")

  // Common fields
  const [reason, setReason] = useState("")
  const [selectedDoctorId, setSelectedDoctorId] = useState<string | undefined>(undefined)
  const [selectedDoctorName, setSelectedDoctorName] = useState<string | undefined>(undefined)

  // Date + slot
  const [selectedDate, setSelectedDate] = useState<Date | undefined>(undefined)
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null)

  // Search hook
  const { data: searchResults = [] } = usePatientSearch(searchTerm)
  const { data: doctors = [] } = useDoctors()

  // Date string for API
  const dateString = useMemo(() => {
    if (!selectedDate) return ""
    const y = selectedDate.getFullYear()
    const m = String(selectedDate.getMonth() + 1).padStart(2, "0")
    const d = String(selectedDate.getDate()).padStart(2, "0")
    return `${y}-${m}-${d}`
  }, [selectedDate])

  // Slots query
  const { data: slots = [], isLoading: slotsLoading } = useAvailableSlots(
    dateString,
    selectedDoctorId && selectedDoctorId !== NO_DOCTOR_VALUE ? selectedDoctorId : undefined,
  )

  // Mutations
  const bookGuestMutation = useBookGuestMutation()
  const bookPatientMutation = useBookAppointment()
  const isSubmitting = bookGuestMutation.isPending || bookPatientMutation.isPending

  // Reset slot when date or doctor changes
  useEffect(() => {
    setSelectedSlot(null)
  }, [dateString, selectedDoctorId])

  // Pre-fill support for patientId
  useEffect(() => {
    if (initialPatientId && searchResults.length === 0 && !selectedPatient) {
      // The caller already specified a patient; we'd need to fetch. For now, set search term.
      setSearchTerm(initialPatientId)
    }
  }, [initialPatientId, searchResults.length, selectedPatient])

  const handleSelectPatient = useCallback((patient: PatientSearchResult) => {
    setSelectedPatient(patient)
    setIsNewPatient(false)
    setSearchOpen(false)
    setSearchTerm(patient.fullName)
    setGuestName("")
    setGuestPhone("")
  }, [])

  const handleSearchChange = useCallback((value: string) => {
    setSearchTerm(value)
    if (value.length >= 2) {
      setSearchOpen(true)
    }
    // Reset patient selection when typing
    setSelectedPatient(null)
    setIsNewPatient(false)
  }, [])

  const handleNotFound = useCallback(() => {
    setIsNewPatient(true)
    setSelectedPatient(null)
    setSearchOpen(false)
    // Auto-fill phone if search term looks like a phone number
    if (/^0\d{9,10}$/.test(searchTerm)) {
      setGuestPhone(searchTerm)
    }
  }, [searchTerm])

  const handleSlotSelect = useCallback((startTime: string) => {
    setSelectedSlot((prev) => (prev === startTime ? null : startTime))
  }, [])

  const handleCancel = useCallback(() => {
    navigate({ to: "/dashboard" })
  }, [navigate])

  const handleConfirm = useCallback(async () => {
    // Validation
    if (!selectedSlot) {
      toast.error("Vui long chon khung gio hen")
      return
    }

    if (!selectedDate) {
      toast.error("Vui long chon ngay hen")
      return
    }

    const patientDisplayName = selectedPatient?.fullName || guestName
    const slotTime = new Date(selectedSlot).toLocaleTimeString("vi-VN", {
      hour: "2-digit",
      minute: "2-digit",
      hour12: false,
      timeZone: "Asia/Ho_Chi_Minh",
    })

    try {
      if (selectedPatient) {
        // Existing patient booking
        const doctor = doctors.find((d) => d.id === selectedDoctorId)
        await bookPatientMutation.mutateAsync({
          patientId: selectedPatient.id,
          patientName: selectedPatient.fullName,
          doctorId: selectedDoctorId && selectedDoctorId !== NO_DOCTOR_VALUE
            ? selectedDoctorId
            : doctors[0]?.id ?? "",
          doctorName: doctor?.fullName ?? doctors[0]?.fullName ?? "",
          startTime: selectedSlot,
          appointmentTypeId: "", // Will use default
          notes: reason || undefined,
        })
      } else if (isNewPatient) {
        // Guest booking
        if (!guestName.trim()) {
          toast.error("Vui long nhap ho ten benh nhan")
          return
        }
        if (!guestPhone.trim() || !/^0\d{9,10}$/.test(guestPhone)) {
          toast.error("So dien thoai khong hop le")
          return
        }

        await bookGuestMutation.mutateAsync({
          guestName: guestName.trim(),
          guestPhone: guestPhone.trim(),
          reason: reason || undefined,
          doctorId:
            selectedDoctorId && selectedDoctorId !== NO_DOCTOR_VALUE
              ? selectedDoctorId
              : undefined,
          date: dateString,
          startTime: selectedSlot,
        })
      } else {
        toast.error("Vui long tim benh nhan hoac nhap thong tin BN moi")
        return
      }

      toast.success(`Da dat hen cho ${patientDisplayName} vao ${dateString} luc ${slotTime}`)
      navigate({ to: "/dashboard" })
    } catch (err) {
      const message = err instanceof Error ? err.message : "Dat hen that bai"
      if (message === "DOUBLE_BOOKING") {
        toast.error("Slot nay vua duoc dat. Vui long chon slot khac.")
      } else {
        toast.error(message)
      }
    }
  }, [
    selectedSlot,
    selectedDate,
    selectedPatient,
    guestName,
    guestPhone,
    reason,
    selectedDoctorId,
    dateString,
    doctors,
    isNewPatient,
    bookPatientMutation,
    bookGuestMutation,
    navigate,
  ])

  // Determine display name for confirmation bar
  const displayName = selectedPatient
    ? selectedPatient.fullName
    : isNewPatient && guestName
      ? `BN moi (${guestPhone || "..."})`
      : "..."

  const showConfirmation = selectedDate && selectedSlot

  // Disable past dates
  const today = new Date()
  today.setHours(0, 0, 0, 0)

  // Max 3 months future
  const maxDate = new Date()
  maxDate.setMonth(maxDate.getMonth() + 3)

  return (
    <div className="space-y-6 pb-24">
      {/* Header / Breadcrumb */}
      <div>
        <p className="text-sm text-muted-foreground">
          Dashboard / Dat lich hen
        </p>
        <h1 className="mt-1 text-xl font-semibold">Dat lich hen</h1>
      </div>

      {/* 2-column layout */}
      <div className="grid gap-6 lg:grid-cols-5">
        {/* Left column: Patient info (40%) */}
        <div className="lg:col-span-2 space-y-4">
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base">Thong tin benh nhan</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Patient search */}
              <div className="space-y-2">
                <Label>Tim benh nhan</Label>
                <Popover open={searchOpen} onOpenChange={setSearchOpen}>
                  <PopoverTrigger asChild>
                    <div className="relative">
                      <IconSearch className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                      <Input
                        value={searchTerm}
                        onChange={(e) => handleSearchChange(e.target.value)}
                        className="pl-9"
                        placeholder="Go SDT hoac ten de tim..."
                        autoFocus
                      />
                    </div>
                  </PopoverTrigger>
                  {searchTerm.length >= 2 && (
                    <PopoverContent
                      className="w-[var(--radix-popover-trigger-width)] p-0"
                      align="start"
                      onOpenAutoFocus={(e) => e.preventDefault()}
                    >
                      <Command>
                        <CommandList>
                          {searchResults.length === 0 ? (
                            <CommandEmpty>
                              <button
                                type="button"
                                className="w-full px-2 py-1.5 text-left text-sm"
                                onClick={handleNotFound}
                              >
                                Khong tim thay. Tao hen cho BN moi?
                              </button>
                            </CommandEmpty>
                          ) : (
                            <CommandGroup>
                              {searchResults.map((patient) => (
                                <CommandItem
                                  key={patient.id}
                                  value={patient.id}
                                  onSelect={() => handleSelectPatient(patient)}
                                >
                                  <span className="font-medium">{patient.fullName}</span>
                                  <span className="ml-2 text-xs text-muted-foreground font-mono">
                                    {patient.patientCode}
                                  </span>
                                  <span className="ml-auto text-xs text-muted-foreground">
                                    {patient.phone}
                                  </span>
                                </CommandItem>
                              ))}
                            </CommandGroup>
                          )}
                        </CommandList>
                      </Command>
                    </PopoverContent>
                  )}
                </Popover>
              </div>

              {/* Found patient bar */}
              {selectedPatient && (
                <div className="flex items-center gap-2 rounded-md bg-[#E1F5EE] px-3 py-2 text-sm text-[#085041]">
                  <IconInfoCircle className="h-4 w-4 shrink-0" />
                  <span>
                    Da tim thay:{" "}
                    <strong>{selectedPatient.fullName}</strong>
                    {" — "}
                    <span className="font-mono">{selectedPatient.patientCode}</span>
                  </span>
                </div>
              )}

              {/* Not found bar */}
              {isNewPatient && !selectedPatient && (
                <div className="flex items-center gap-2 rounded-md bg-[#FAEEDA] px-3 py-2 text-sm text-[#633806]">
                  <IconAlertTriangle className="h-4 w-4 shrink-0" />
                  <span>
                    Khong tim thay BN voi SDT nay. Nhap thong tin ben duoi de tao hen cho BN moi.
                  </span>
                </div>
              )}

              {/* Guest fields (new patient) */}
              {isNewPatient && !selectedPatient && (
                <div className="space-y-3">
                  <div className="space-y-1.5">
                    <Label>Ho ten *</Label>
                    <Input
                      value={guestName}
                      onChange={(e) => setGuestName(e.target.value)}
                      maxLength={200}
                    />
                  </div>
                  <div className="space-y-1.5">
                    <Label>So dien thoai *</Label>
                    <Input
                      value={guestPhone}
                      onChange={(e) => setGuestPhone(e.target.value)}
                      type="tel"
                    />
                  </div>

                  {/* Info note */}
                  <div className="flex items-start gap-2 rounded-md bg-[#E6F1FB] px-3 py-2 text-sm text-[#0C447C]">
                    <IconInfoCircle className="mt-0.5 h-4 w-4 shrink-0" />
                    <span>
                      BN moi chi can ten + SDT + ly do kham de dat hen. Thong tin day du se bo sung khi BN den check-in.
                    </span>
                  </div>
                </div>
              )}

              {/* Doctor selector */}
              <div className="space-y-1.5">
                <Label>Bac si chi dinh</Label>
                <Select
                  value={selectedDoctorId ?? NO_DOCTOR_VALUE}
                  onValueChange={(val) => {
                    if (val === NO_DOCTOR_VALUE) {
                      setSelectedDoctorId(undefined)
                      setSelectedDoctorName(undefined)
                    } else {
                      setSelectedDoctorId(val)
                      const doc = doctors.find((d) => d.id === val)
                      setSelectedDoctorName(doc?.fullName)
                    }
                  }}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value={NO_DOCTOR_VALUE}>BS nao trong</SelectItem>
                    {doctors.map((doc) => (
                      <SelectItem key={doc.id} value={doc.id}>
                        {doc.fullName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Reason */}
              <div className="space-y-1.5">
                <Label>Ly do kham</Label>
                <Textarea
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  maxLength={500}
                  rows={3}
                />
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Right column: Calendar + slots (60%) */}
        <div className="lg:col-span-3 space-y-4">
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base">Chon ngay & gio</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Calendar */}
              <Calendar
                mode="single"
                selected={selectedDate}
                onSelect={(date) => setSelectedDate(date ?? undefined)}
                disabled={(date) => date < today || date > maxDate}
                classNames={{
                  today: "underline text-accent-foreground",
                  day: "group/day relative aspect-square h-full w-full select-none p-0 text-center [&:first-child[data-selected=true]_button]:rounded-l-md [&:last-child[data-selected=true]_button]:rounded-r-md",
                }}
                className="w-full"
              />

              {/* Time slot grid */}
              {selectedDate && (
                <div className="pt-2">
                  <TimeSlotGrid
                    slots={slots}
                    selectedSlot={selectedSlot}
                    onSelectSlot={handleSlotSelect}
                    isLoading={slotsLoading}
                  />
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Confirmation bar */}
      {showConfirmation && (
        <ConfirmationBar
          patientName={displayName}
          date={dateString}
          time={selectedSlot!}
          reason={reason}
          doctorName={selectedDoctorName}
          onCancel={handleCancel}
          onConfirm={handleConfirm}
          isSubmitting={isSubmitting}
        />
      )}
    </div>
  )
}
