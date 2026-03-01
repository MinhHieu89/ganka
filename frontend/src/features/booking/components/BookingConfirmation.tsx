import { useTranslation } from "react-i18next"
import { Link } from "@tanstack/react-router"
import { Button } from "@/shared/components/Button"
import { IconCircleCheck, IconSearch } from "@tabler/icons-react"

interface BookingConfirmationProps {
  referenceNumber: string
  onBookAnother: () => void
}

export function BookingConfirmation({
  referenceNumber,
  onBookAnother,
}: BookingConfirmationProps) {
  const { t } = useTranslation("scheduling")

  return (
    <div className="flex flex-col items-center text-center space-y-6 py-8">
      <div className="flex size-16 items-center justify-center bg-green-50 text-green-600">
        <IconCircleCheck className="h-10 w-10" />
      </div>

      <div className="space-y-2">
        <h2 className="text-xl font-semibold">{t("selfBooking.confirmation")}</h2>
        <p className="text-sm text-muted-foreground max-w-md">
          {t("selfBooking.confirmationMessage")}
        </p>
      </div>

      {/* Reference number prominently displayed */}
      <div className="bg-muted/50 border p-6 w-full max-w-sm">
        <p className="text-xs text-muted-foreground uppercase tracking-wider mb-1">
          {t("selfBooking.referenceNumber")}
        </p>
        <p className="text-2xl font-bold font-mono tracking-wider">
          {referenceNumber}
        </p>
      </div>

      <p className="text-xs text-muted-foreground max-w-sm">
        {t("selfBooking.saveReference")}
      </p>

      <div className="flex flex-col sm:flex-row gap-3 w-full max-w-sm">
        <Link
          to={"/book/status" as string}
          search={{ ref: referenceNumber } as never}
          className="flex-1"
        >
          <Button variant="outline" className="w-full">
            <IconSearch className="mr-2 h-4 w-4" />
            {t("selfBooking.checkStatus")}
          </Button>
        </Link>
        <Button onClick={onBookAnother} className="flex-1">
          {t("selfBooking.bookAnother")}
        </Button>
      </div>
    </div>
  )
}
