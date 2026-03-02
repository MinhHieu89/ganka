import { useState, useEffect } from "react"
import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { vi, enUS } from "date-fns/locale"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Badge } from "@/shared/components/Badge"
import {
  Field,
  FieldLabel,
} from "@/shared/components/Field"
import {
  useCheckBookingStatus,
  BookingStatus,
} from "@/features/booking/api/booking-api"
import {
  IconSearch,
  IconLoader2,
  IconClock,
  IconCircleCheck,
  IconCircleX,
  IconPhone,
} from "@tabler/icons-react"

interface BookingStatusCheckProps {
  initialReference?: string
}

export function BookingStatusCheck({ initialReference }: BookingStatusCheckProps) {
  const { t, i18n } = useTranslation("scheduling")
  const locale = i18n.language === "vi" ? vi : enUS

  const [referenceInput, setReferenceInput] = useState(initialReference ?? "")
  const [searchRef, setSearchRef] = useState(initialReference ?? "")

  useEffect(() => {
    if (initialReference) {
      setReferenceInput(initialReference)
      setSearchRef(initialReference)
    }
  }, [initialReference])

  const { data: status, isLoading, error } = useCheckBookingStatus(searchRef)

  const handleCheck = () => {
    const trimmed = referenceInput.trim()
    if (trimmed) {
      setSearchRef(trimmed)
    }
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleCheck()
    }
  }

  return (
    <div className="space-y-6">
      {/* Search form */}
      <div className="space-y-3">
        <Field>
          <FieldLabel>{t("selfBooking.enterReference")}</FieldLabel>
          <div className="flex gap-2">
            <Input
              value={referenceInput}
              onChange={(e) => setReferenceInput(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder="BK-XXXXXX-XXXX"
              className="font-mono"
            />
            <Button onClick={handleCheck} disabled={isLoading || !referenceInput.trim()}>
              {isLoading ? (
                <IconLoader2 className="h-4 w-4 animate-spin" />
              ) : (
                <IconSearch className="h-4 w-4" />
              )}
              <span className="ml-2">{t("selfBooking.check")}</span>
            </Button>
          </div>
        </Field>
      </div>

      {/* Status result */}
      {status && (
        <div className="border rounded-lg p-6 space-y-4">
          {/* Reference */}
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">
              {t("selfBooking.referenceNumber")}
            </span>
            <span className="font-mono font-medium">{status.referenceNumber}</span>
          </div>

          {/* Status display */}
          {status.status === BookingStatus.Pending && (
            <div className="flex items-start gap-3 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
              <IconClock className="h-5 w-5 text-yellow-600 shrink-0 mt-0.5" />
              <div>
                <Badge className="bg-yellow-100 text-yellow-800 border-yellow-300 mb-2">
                  {t("selfBooking.statusPending")}
                </Badge>
                <p className="text-sm text-yellow-800">
                  {t("selfBooking.statusMessage.pending")}
                </p>
              </div>
            </div>
          )}

          {status.status === BookingStatus.Approved && (
            <div className="flex items-start gap-3 p-4 bg-green-50 border border-green-200 rounded-lg">
              <IconCircleCheck className="h-5 w-5 text-green-600 shrink-0 mt-0.5" />
              <div>
                <Badge className="bg-green-100 text-green-800 border-green-300 mb-2">
                  {t("selfBooking.statusApproved")}
                </Badge>
                <p className="text-sm text-green-800">
                  {t("selfBooking.statusMessage.approved", {
                    date: status.appointmentDate
                      ? format(new Date(status.appointmentDate), "EEEE, dd/MM/yyyy HH:mm", { locale })
                      : "---",
                  })}
                </p>
              </div>
            </div>
          )}

          {status.status === BookingStatus.Rejected && (
            <div className="space-y-3">
              <div className="flex items-start gap-3 p-4 bg-red-50 border border-red-200 rounded-lg">
                <IconCircleX className="h-5 w-5 text-red-600 shrink-0 mt-0.5" />
                <div>
                  <Badge variant="destructive" className="mb-2">
                    {t("selfBooking.statusRejected")}
                  </Badge>
                  <p className="text-sm text-red-800">
                    {t("selfBooking.statusMessage.rejected", {
                      reason: status.rejectionReason ?? "---",
                    })}
                  </p>
                </div>
              </div>
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <IconPhone className="h-4 w-4" />
                {t("selfBooking.callForAssistance", { phone: t("clinicPhone") })}
              </div>
            </div>
          )}
        </div>
      )}

      {/* Error */}
      {error && searchRef && (
        <div className="text-sm text-destructive p-3 bg-destructive/10 border border-destructive/20 rounded-lg">
          {i18n.language === "vi"
            ? "Kh\u00F4ng t\u00ECm th\u1EA5y m\u00E3 tham chi\u1EBFu. Vui l\u00F2ng ki\u1EC3m tra l\u1EA1i."
            : "Reference number not found. Please check and try again."}
        </div>
      )}
    </div>
  )
}
