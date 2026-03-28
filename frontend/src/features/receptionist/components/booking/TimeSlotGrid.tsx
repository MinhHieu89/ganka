import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { Skeleton } from "@/shared/components/Skeleton"
import { cn } from "@/shared/lib/utils"
import type { AvailableSlot } from "@/features/receptionist/types/receptionist.types"

interface SlotGroup {
  label: string
  slots: AvailableSlot[]
}

interface TimeSlotGridProps {
  slots: AvailableSlot[]
  selectedSlot: string | null
  onSelectSlot: (startTime: string) => void
  isLoading: boolean
}

function formatTime(isoString: string): string {
  const date = new Date(isoString)
  return date.toLocaleTimeString("vi-VN", {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone: "Asia/Ho_Chi_Minh",
  })
}

function isSlotFull(slot: AvailableSlot): boolean {
  // If the slot has no doctor assigned or the doctorId is empty, it is free
  // The backend returns available slots; a slot is "full" when capacity is reached
  // For now, we consider all returned slots as available (backend filters)
  // A full slot would not be returned by the API, but we handle it via a capacity field if present
  return false
}

export function TimeSlotGrid({
  slots,
  selectedSlot,
  onSelectSlot,
  isLoading,
}: TimeSlotGridProps) {
  const { t } = useTranslation("scheduling")
  const groups = useMemo<SlotGroup[]>(() => {
    // Deduplicate slots by startTime (multiple doctors may share the same time)
    const uniqueTimes = new Map<string, AvailableSlot>()
    for (const slot of slots) {
      const time = formatTime(slot.startTime)
      if (!uniqueTimes.has(time)) {
        uniqueTimes.set(time, slot)
      }
    }

    const morning: AvailableSlot[] = []
    const afternoon: AvailableSlot[] = []

    for (const slot of uniqueTimes.values()) {
      const hour = new Date(slot.startTime).getHours()
      if (hour < 12) {
        morning.push(slot)
      } else {
        afternoon.push(slot)
      }
    }

    // Sort by time
    const sortByTime = (a: AvailableSlot, b: AvailableSlot) =>
      new Date(a.startTime).getTime() - new Date(b.startTime).getTime()

    morning.sort(sortByTime)
    afternoon.sort(sortByTime)

    return [
      { label: t("slots.morning"), slots: morning },
      { label: t("slots.afternoon"), slots: afternoon },
    ]
  }, [slots, t])

  if (isLoading) {
    return (
      <div className="space-y-4">
        {[0, 1].map((i) => (
          <div key={i} className="space-y-2">
            <Skeleton className="h-5 w-32" />
            <div className="flex flex-wrap gap-2">
              {Array.from({ length: 6 }).map((_, j) => (
                <Skeleton key={j} className="h-9 w-[80px]" />
              ))}
            </div>
          </div>
        ))}
      </div>
    )
  }

  if (slots.length === 0) {
    return (
      <div className="py-8 text-center text-sm text-muted-foreground">
        {t("slots.noSlots")}
      </div>
    )
  }

  const totalSlots = groups.reduce((sum, g) => sum + g.slots.length, 0)
  const availableSlots = groups.reduce(
    (sum, g) => sum + g.slots.filter((s) => !isSlotFull(s)).length,
    0,
  )

  return (
    <div className="space-y-4">
      <div className="text-sm font-semibold text-foreground">
        {availableSlots} {t("slots.available")} / {totalSlots}
      </div>

      {groups.map((group) => {
        if (group.slots.length === 0) return null

        const groupAvailable = group.slots.filter((s) => !isSlotFull(s)).length

        return (
          <div key={group.label} className="space-y-2">
            <div className="flex items-center gap-2">
              <span className="text-sm font-semibold">{group.label}</span>
              <span className="text-xs text-muted-foreground">
                {groupAvailable} {t("slots.available")} / {group.slots.length}
              </span>
            </div>

            <div className="flex flex-wrap gap-2">
              {group.slots.map((slot) => {
                const time = formatTime(slot.startTime)
                const full = isSlotFull(slot)
                const selected = selectedSlot === slot.startTime

                return (
                  <button
                    key={slot.startTime}
                    type="button"
                    disabled={full}
                    onClick={() => onSelectSlot(slot.startTime)}
                    className={cn(
                      "inline-flex min-w-[80px] items-center justify-center rounded-md border px-3 h-9 text-sm transition-colors",
                      full && [
                        "bg-[var(--secondary)] text-muted-foreground line-through cursor-not-allowed",
                      ],
                      selected &&
                        !full && [
                          "border-[#534AB7] bg-[#EEEDFE] text-[#3C3489] font-semibold",
                        ],
                      !selected &&
                        !full && [
                          "border-[var(--border)] bg-white text-foreground hover:bg-accent cursor-pointer",
                        ],
                    )}
                  >
                    {time}
                  </button>
                )
              })}
            </div>
          </div>
        )
      })}

      {/* Legend */}
      <div className="flex items-center gap-4 pt-2 text-xs text-muted-foreground">
        <div className="flex items-center gap-1.5">
          <div className="h-4 w-6 rounded border border-[var(--border)] bg-white" />
          <span>{t("slots.legendEmpty")}</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="h-4 w-6 rounded border border-[#534AB7] bg-[#EEEDFE]" />
          <span>{t("slots.legendSelected")}</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="h-4 w-6 rounded bg-[var(--secondary)]" />
          <span>{t("slots.legendFull")}</span>
        </div>
      </div>
    </div>
  )
}
