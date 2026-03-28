import { useState, useCallback, useMemo, useEffect } from "react"
import { useNavigate } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { vi, enUS } from "date-fns/locale"
import type { Locale } from "date-fns"
import { toast } from "sonner"
import { IconSearch, IconInfoCircle, IconAlertTriangle } from "@tabler/icons-react"
import { Button } from "@/shared/components/ui/button"
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
import { useBookAppointment, useClinicSchedule } from "@/features/scheduling/api/scheduling-api"
import { useIsLargeScreen } from "@/shared/hooks/use-mobile"
import { TimeSlotGrid } from "./TimeSlotGrid"
import { ConfirmationBar } from "./ConfirmationBar"

const NO_DOCTOR_VALUE = "__no_doctor__"

const localeMap: Record<string, Locale> = {
  vi,
  en: enUS,
}

interface NewAppointmentPageProps {
  initialPatientId?: string
}

export function NewAppointmentPage({ initialPatientId }: NewAppointmentPageProps) {
  const navigate = useNavigate()
  const { t, i18n } = useTranslation("scheduling")
  const { t: tCommon } = useTranslation("common")
  const isLargeScreen = useIsLargeScreen()
  const calendarLocale = localeMap[i18n.language] ?? vi

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
  const { data: clinicSchedule } = useClinicSchedule()

  // Days when clinic is closed (for calendar disabled state)
  const closedDays = useMemo(() => {
    if (!clinicSchedule) return new Set<number>()
    return new Set(clinicSchedule.filter((s) => !s.isOpen).map((s) => s.dayOfWeek))
  }, [clinicSchedule])

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
      toast.error(t("booking.selectSlotError"))
      return
    }

    if (!selectedDate) {
      toast.error(t("booking.selectDateError"))
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
          appointmentTypeId: "00000000-0000-0000-0000-000000000101",
          notes: reason || undefined,
        })
      } else if (isNewPatient) {
        // Guest booking
        if (!guestName.trim()) {
          toast.error(t("booking.nameRequired"))
          return
        }
        if (!guestPhone.trim() || !/^0\d{9,10}$/.test(guestPhone)) {
          toast.error(t("booking.phoneInvalid"))
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
          doctorName: selectedDoctorName,
          date: dateString,
          startTime: selectedSlot,
        })
      } else {
        toast.error(t("booking.selectPatientError"))
        return
      }

      toast.success(t("booking.successToast", { name: patientDisplayName, date: dateString, time: slotTime }))
      navigate({ to: "/dashboard" })
    } catch (err) {
      const message = err instanceof Error ? err.message : ""
      if (message === "DOUBLE_BOOKING") {
        toast.error(t("booking.doubleBooking"))
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
      ? `${t("booking.newPatientLabel")} (${guestPhone || "..."})`
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
      <h1 className="text-xl font-semibold">{t("booking.title")}</h1>

      {/* 2-column layout */}
      <div className="grid gap-6 lg:grid-cols-5">
        {/* Left column: Patient info (40%) */}
        <div className="lg:col-span-2 space-y-4">
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base">{t("booking.patientInfo")}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Patient search */}
              <div className="space-y-2">
                <Label>{t("booking.searchLabel")}</Label>
                <Popover open={searchOpen} onOpenChange={setSearchOpen}>
                  <PopoverTrigger asChild>
                    <div className="relative">
                      <IconSearch className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                      <Input
                        value={searchTerm}
                        onChange={(e) => handleSearchChange(e.target.value)}
                        className="pl-9"
                        placeholder={t("booking.searchPlaceholder")}
                        autoFocus
                      />
                    </div>
                  </PopoverTrigger>
                  {searchTerm.length >= 2 && (
                    <PopoverContent
                      className="w-(--radix-popover-trigger-width) p-0"
                      align="start"
                      onOpenAutoFocus={(e) => e.preventDefault()}
                    >
                      <Command>
                        <CommandList>
                          {searchResults.length === 0 ? (
                            <CommandEmpty>
                              <Button
                                type="button"
                                variant="ghost"
                                className="w-full justify-start"
                                onClick={handleNotFound}
                              >
                                {t("booking.notFound")}
                              </Button>
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
                    {t("booking.found")}{" "}
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
                    {t("booking.notFoundWarning")}
                  </span>
                </div>
              )}

              {/* Guest fields (new patient) */}
              {isNewPatient && !selectedPatient && (
                <div className="space-y-3">
                  <div className="space-y-1.5">
                    <Label>{t("booking.guestName")}</Label>
                    <Input
                      value={guestName}
                      onChange={(e) => setGuestName(e.target.value)}
                      maxLength={200}
                    />
                  </div>
                  <div className="space-y-1.5">
                    <Label>{t("booking.guestPhone")}</Label>
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
                      {t("booking.guestInfo")}
                    </span>
                  </div>
                </div>
              )}

              {/* Doctor selector */}
              <div className="space-y-1.5">
                <Label>{t("booking.doctor")}</Label>
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
                    <SelectItem value={NO_DOCTOR_VALUE}>{t("booking.anyDoctor")}</SelectItem>
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
                <Label>{t("booking.reason")}</Label>
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
              <CardTitle className="text-base">{t("booking.dateTimeTitle")}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Calendar */}
              <Calendar
                mode="single"
                selected={selectedDate}
                onSelect={(date) => setSelectedDate(date ?? undefined)}
                disabled={(date) =>
                  date < today || date > maxDate || closedDays.has(date.getDay())
                }
                locale={calendarLocale}
                numberOfMonths={isLargeScreen ? 2 : 1}
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
