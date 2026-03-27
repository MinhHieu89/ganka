import { IconLoader2 } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Card } from "@/shared/components/Card"

interface ConfirmationBarProps {
  patientName: string
  date: string
  time: string
  reason?: string
  doctorName?: string
  onCancel: () => void
  onConfirm: () => void
  isSubmitting: boolean
}

function formatDisplayDate(dateStr: string): string {
  const date = new Date(dateStr + "T00:00:00")
  const days = ["CN", "Th 2", "Th 3", "Th 4", "Th 5", "Th 6", "Th 7"]
  const dayName = days[date.getDay()]
  const dd = String(date.getDate()).padStart(2, "0")
  const mm = String(date.getMonth() + 1).padStart(2, "0")
  const yyyy = date.getFullYear()
  return `${dayName}, ${dd}/${mm}/${yyyy}`
}

function formatDisplayTime(isoString: string): string {
  const date = new Date(isoString)
  return date.toLocaleTimeString("vi-VN", {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone: "Asia/Ho_Chi_Minh",
  })
}

export function ConfirmationBar({
  patientName,
  date,
  time,
  reason,
  doctorName,
  onCancel,
  onConfirm,
  isSubmitting,
}: ConfirmationBarProps) {
  const formattedDate = formatDisplayDate(date)
  const formattedTime = formatDisplayTime(time)

  return (
    <Card className="sticky bottom-0 z-10 border-t bg-[#EEEDFE] p-4">
      <div className="flex items-center justify-between gap-4">
        <div className="min-w-0 flex-1">
          <p className="text-sm font-semibold text-[#3C3489]">
            Xac nhan lich hen
          </p>
          <p className="mt-1 truncate text-sm text-foreground">
            <span className="font-semibold">{patientName}</span>
            {" — "}
            {formattedDate} luc {formattedTime}
          </p>
          <p className="truncate text-sm text-muted-foreground">
            {reason || "Chua nhap ly do"}
            {doctorName ? ` — ${doctorName}` : " — Khong chi dinh BS"}
          </p>
        </div>

        <div className="flex shrink-0 items-center gap-2">
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            disabled={isSubmitting}
          >
            Huy bo
          </Button>
          <Button
            type="button"
            onClick={onConfirm}
            disabled={isSubmitting}
            className="bg-[var(--checkin-confirm)] text-white hover:bg-[var(--checkin-confirm)]/90"
          >
            {isSubmitting && (
              <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
            )}
            Xac nhan dat hen
          </Button>
        </div>
      </div>
    </Card>
  )
}
