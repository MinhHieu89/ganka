import { useTranslation } from "react-i18next"
import { ToggleGroup, ToggleGroupItem } from "@/shared/components/ToggleGroup"
import type { ReceptionistKpi, ReceptionistStatus } from "@/features/receptionist/types/receptionist.types"

interface StatusFilterPillsProps {
  value: ReceptionistStatus | undefined
  onChange: (status?: ReceptionistStatus) => void
  counts: ReceptionistKpi | undefined
}

const filters: Array<{
  value: string
  labelKey: string
  countKey: keyof ReceptionistKpi | null
}> = [
  { value: "all", labelKey: "filter.all", countKey: null },
  { value: "not_arrived", labelKey: "filter.notArrived", countKey: "notArrived" },
  { value: "waiting", labelKey: "filter.waiting", countKey: "waiting" },
  { value: "examining", labelKey: "filter.examining", countKey: "examining" },
  { value: "completed", labelKey: "filter.done", countKey: "completed" },
  { value: "cancelled", labelKey: "filter.cancelled", countKey: "cancelled" },
]

export function StatusFilterPills({ value, onChange, counts }: StatusFilterPillsProps) {
  const { t } = useTranslation("receptionist")
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
            {t(filter.labelKey)}
            {count != null && ` (${count})`}
          </ToggleGroupItem>
        )
      })}
    </ToggleGroup>
  )
}
