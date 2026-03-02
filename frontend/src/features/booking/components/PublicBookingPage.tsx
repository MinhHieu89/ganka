import { useTranslation } from "react-i18next"
import { Button } from "@/shared/components/Button"
import { usePublicBooking } from "@/features/booking/hooks/usePublicBooking"
import { BookingForm } from "@/features/booking/components/BookingForm"
import { BookingConfirmation } from "@/features/booking/components/BookingConfirmation"
import { IconLanguage, IconPhone, IconClock } from "@tabler/icons-react"

export function PublicBookingPage() {
  const { t, i18n } = useTranslation("scheduling")

  const {
    state,
    referenceNumber,
    isSubmitting,
    submitError,
    handleSubmit,
    resetForm,
  } = usePublicBooking()

  const toggleLanguage = () => {
    const next = i18n.language === "vi" ? "en" : "vi"
    i18n.changeLanguage(next)
  }

  return (
    <div className="min-h-screen bg-muted">
      {/* Header */}
      <header className="bg-card border-b">
        <div className="max-w-2xl mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex size-10 items-center justify-center bg-primary text-primary-foreground text-sm font-bold tracking-wider shadow-sm">
              <span>G</span>
            </div>
            <div>
              <h1 className="font-semibold text-lg tracking-tight">Ganka28</h1>
              <p className="text-xs text-muted-foreground font-medium uppercase tracking-widest">
                Ophthalmology
              </p>
            </div>
          </div>
          <Button
            variant="ghost"
            size="sm"
            onClick={toggleLanguage}
            className="gap-1.5"
          >
            <IconLanguage className="h-4 w-4" />
            <span className="text-xs font-medium">
              {i18n.language === "vi" ? "EN" : "VI"}
            </span>
          </Button>
        </div>
      </header>

      {/* Main content */}
      <main className="max-w-2xl mx-auto px-4 py-8">
        <div className="bg-card border p-6 sm:p-8 shadow-sm rounded-lg">
          {/* Page title */}
          <div className="text-center mb-8">
            <h2 className="text-2xl font-semibold tracking-tight">
              {t("selfBooking.title")}
            </h2>
            <p className="text-sm text-muted-foreground mt-1">
              {t("selfBooking.subtitle")}
            </p>
          </div>

          {/* Form or Confirmation */}
          {state === "form" ? (
            <BookingForm
              onSubmit={handleSubmit}
              isSubmitting={isSubmitting}
              error={submitError}
            />
          ) : (
            <BookingConfirmation
              referenceNumber={referenceNumber}
              onBookAnother={resetForm}
            />
          )}
        </div>
      </main>

      {/* Footer */}
      <footer className="border-t bg-card mt-8">
        <div className="max-w-2xl mx-auto px-4 py-6">
          <div className="grid gap-4 sm:grid-cols-2 text-sm text-muted-foreground">
            <div className="space-y-2">
              <div className="flex items-center gap-2 font-medium text-foreground">
                <IconClock className="h-4 w-4" />
                {t("clinicHours")}
              </div>
              <div className="space-y-1 text-xs">
                <p>{t("clinicHoursSchedule.tueFri")}</p>
                <p>{t("clinicHoursSchedule.satSun")}</p>
                <p>{t("clinicHoursSchedule.monClosed")}</p>
              </div>
            </div>
            <div className="space-y-2">
              <div className="flex items-center gap-2 font-medium text-foreground">
                <IconPhone className="h-4 w-4" />
                {t("clinicPhone")}
              </div>
              <p className="text-xs">{t("clinicAddress")}</p>
            </div>
          </div>
          <div className="border-t mt-4 pt-4 text-center text-xs text-muted-foreground/70">
            &copy; {new Date().getFullYear()} Ganka28. All rights reserved.
          </div>
        </div>
      </footer>
    </div>
  )
}
