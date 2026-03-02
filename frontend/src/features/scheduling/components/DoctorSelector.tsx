import { useEffect } from "react"
import { useQuery } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { api } from "@/shared/lib/api-client"

export interface DoctorOption {
  id: string
  fullName: string
}

function useDoctors() {
  return useQuery({
    queryKey: ["doctors"],
    queryFn: async (): Promise<DoctorOption[]> => {
      const { data, error } = await api.GET("/api/admin/users" as never)
      if (error) throw new Error("Failed to fetch doctors")
      // Response is a paginated envelope: { data: UserDto[], totalCount, page, pageSize }
      const envelope = data as { data: Array<{ id: string; fullName: string; roles?: string[] }> }
      const users = envelope.data ?? []
      return users
        .filter(
          (u) =>
            u.roles?.some((r) => r === "Doctor") ?? false,
        )
        .map((u) => ({ id: u.id, fullName: u.fullName }))
    },
    staleTime: 1000 * 60 * 15, // 15 min
  })
}

interface DoctorSelectorProps {
  value?: string
  onChange: (doctorId: string, doctorName: string) => void
  className?: string
}

export function DoctorSelector({ value, onChange, className }: DoctorSelectorProps) {
  const { t } = useTranslation("scheduling")
  const { data: doctors, isLoading } = useDoctors()

  // Auto-select first doctor when data loads and no value set
  useEffect(() => {
    if (doctors && doctors.length > 0 && !value) {
      onChange(doctors[0].id, doctors[0].fullName)
    }
  }, [doctors, value, onChange])

  return (
    <Select
      value={value}
      onValueChange={(id) => {
        const doctor = doctors?.find((d) => d.id === id)
        onChange(id, doctor?.fullName ?? "")
      }}
    >
      <SelectTrigger className={className}>
        <SelectValue placeholder={isLoading ? "..." : t("selectDoctor")} />
      </SelectTrigger>
      <SelectContent>
        {(doctors ?? []).map((doctor) => (
          <SelectItem key={doctor.id} value={doctor.id}>
            {doctor.fullName}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}

export { useDoctors }
