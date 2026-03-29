import { useTranslation } from "react-i18next"
import { Button } from "@/shared/components/Button"
import type { TechnicianDashboardRow } from "@/features/technician/types/technician.types"

interface TechnicianBannerProps {
  currentPatient: TechnicianDashboardRow | null
  onContinue: () => void
}

export function TechnicianBanner({
  currentPatient,
  onContinue,
}: TechnicianBannerProps) {
  const { t } = useTranslation("technician")

  if (!currentPatient) return null

  const checkinTime = (() => {
    try {
      return new Date(currentPatient.checkinTime).toLocaleTimeString("vi-VN", {
        hour: "2-digit",
        minute: "2-digit",
      })
    } catch {
      return "--:--"
    }
  })()

  return (
    <div
      className="flex items-center justify-between rounded-lg px-4 py-3"
      style={{
        backgroundColor: "var(--tech-banner-bg)",
        border: "0.5px solid var(--tech-banner-border)",
      }}
    >
      <div className="space-y-0.5">
        <div
          className="text-xs font-bold"
          style={{ color: "var(--tech-banner-text)" }}
        >
          {t("banner.label")}
        </div>
        <div className="flex items-center gap-2 text-sm">
          <span className="font-medium">{currentPatient.patientName}</span>
          {currentPatient.birthYear && (
            <span className="text-muted-foreground">
              ({currentPatient.birthYear})
            </span>
          )}
          {currentPatient.reason && (
            <>
              <span className="text-muted-foreground">-</span>
              <span className="text-muted-foreground">
                {currentPatient.reason}
              </span>
            </>
          )}
          <span className="text-muted-foreground">
            Check-in: {checkinTime}
          </span>
        </div>
      </div>
      <Button
        size="sm"
        onClick={onContinue}
        style={{
          backgroundColor: "var(--tech-banner-text)",
          color: "white",
        }}
      >
        {t("banner.continueBtn")}
      </Button>
    </div>
  )
}
