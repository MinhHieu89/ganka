import { ToggleGroup, ToggleGroupItem } from "@/shared/components/ToggleGroup"
import type { ReceptionistKpi, ReceptionistStatus } from "@/features/receptionist/types/receptionist.types"

interface StatusFilterPillsProps {
  value: ReceptionistStatus | undefined
  onChange: (status?: ReceptionistStatus) => void
  counts: ReceptionistKpi | undefined
}

const filters: Array<{
  value: string
  label: string
  countKey: keyof ReceptionistKpi | null
}> = [
  { value: "all", label: "Tat ca", countKey: null },
  { value: "not_arrived", label: "Chua den", countKey: "notArrived" },
  { value: "waiting", label: "Cho kham", countKey: "waiting" },
  { value: "examining", label: "Dang kham", countKey: "examining" },
  { value: "completed", label: "Xong", countKey: "completed" },
]

export function StatusFilterPills({ value, onChange, counts }: StatusFilterPillsProps) {
  return (
    <ToggleGroup
      type="single"
      variant="outline"
      size="sm"
      value={value ?? "all"}
      onValueChange={(val: string) => {
        onChange(val === "all" || !val ? undefined : (val as ReceptionistStatus))
      }}
      className="justify-start"
    >
      {filters.map((filter) => {
        const count = filter.countKey ? counts?.[filter.countKey] : null
        return (
          <ToggleGroupItem
            key={filter.value}
            value={filter.value}
            className="data-[state=on]:bg-primary data-[state=on]:text-primary-foreground"
          >
            {filter.label}
            {count != null && ` (${count})`}
          </ToggleGroupItem>
        )
      })}
    </ToggleGroup>
  )
}
