import { useRef, useCallback, useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { ToggleGroup, ToggleGroupItem } from "@/shared/components/ToggleGroup"
import { Input } from "@/shared/components/Input"
import { IconSearch } from "@tabler/icons-react"
import type { TechnicianStatus } from "@/features/technician/types/technician.types"

interface TechnicianToolbarProps {
  activeFilter: TechnicianStatus | undefined
  onFilterChange: (status?: TechnicianStatus) => void
  search: string
  onSearchChange: (search: string) => void
  counts: Record<string, number>
}

type FilterOption = {
  value: string
  labelKey: string
  countKey?: string
}

const filterOptions: FilterOption[] = [
  { value: "all", labelKey: "filter.all" },
  { value: "waiting", labelKey: "filter.waiting", countKey: "waiting" },
  { value: "in_progress", labelKey: "filter.inProgress", countKey: "inProgress" },
  { value: "completed", labelKey: "filter.completed", countKey: "completed" },
  { value: "red_flag", labelKey: "filter.redFlag", countKey: "redFlag" },
]

export function TechnicianToolbar({
  activeFilter,
  onFilterChange,
  search,
  onSearchChange,
  counts,
}: TechnicianToolbarProps) {
  const { t } = useTranslation("technician")
  const [localSearch, setLocalSearch] = useState(search)
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const handleSearchChange = useCallback(
    (value: string) => {
      setLocalSearch(value)
      if (timerRef.current) clearTimeout(timerRef.current)
      timerRef.current = setTimeout(() => {
        onSearchChange(value)
      }, 300)
    },
    [onSearchChange],
  )

  useEffect(() => {
    return () => {
      if (timerRef.current) clearTimeout(timerRef.current)
    }
  }, [])

  // Sync external search changes
  useEffect(() => {
    setLocalSearch(search)
  }, [search])

  const handleFilterChange = useCallback(
    (value: string) => {
      if (value === "all" || value === "") {
        onFilterChange(undefined)
      } else {
        onFilterChange(value as TechnicianStatus)
      }
    },
    [onFilterChange],
  )

  return (
    <div className="flex items-center justify-between gap-4">
      <ToggleGroup
        type="single"
        value={activeFilter ?? "all"}
        onValueChange={handleFilterChange}
        className="gap-2"
      >
        {filterOptions.map((option) => {
          const count = option.countKey ? counts[option.countKey] ?? 0 : undefined
          const isActive = activeFilter
            ? option.value === activeFilter
            : option.value === "all"

          return (
            <ToggleGroupItem
              key={option.value}
              value={option.value}
              className="px-3 py-1.5 text-xs h-auto"
              style={
                isActive
                  ? { backgroundColor: "#000", color: "#fff" }
                  : {
                      backgroundColor: "transparent",
                      border: "1px solid var(--border)",
                    }
              }
            >
              {t(option.labelKey)}
              {count !== undefined && ` (${count})`}
            </ToggleGroupItem>
          )
        })}
      </ToggleGroup>

      <div className="relative">
        <IconSearch className="absolute left-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          type="search"
          value={localSearch}
          onChange={(e) => handleSearchChange(e.target.value)}
          placeholder={t("search.placeholder")}
          className="pl-8 w-64"
        />
      </div>
    </div>
  )
}
