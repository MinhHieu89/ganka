import { useTranslation } from "react-i18next"
import { IconAlertTriangle } from "@tabler/icons-react"
import { Alert, AlertDescription, AlertTitle } from "@/shared/components/Alert"
import { cn } from "@/shared/lib/utils"

interface OverdueOrderAlertProps {
  estimatedDate?: string | null
  orderCount?: number
  className?: string
}

export function OverdueOrderAlert({
  estimatedDate,
  orderCount,
  className,
}: OverdueOrderAlertProps) {
  const { t } = useTranslation("optical")

  if (orderCount !== undefined) {
    return (
      <Alert variant="destructive" className={cn("", className)}>
        <IconAlertTriangle className="h-4 w-4" />
        <AlertTitle>{t("orders.overdueOrders")}</AlertTitle>
        <AlertDescription>
          {t("orders.overdueAlert", { count: orderCount })}
        </AlertDescription>
      </Alert>
    )
  }

  return (
    <Alert variant="destructive" className={cn("", className)}>
      <IconAlertTriangle className="h-4 w-4" />
      <AlertTitle>{t("orders.overdue")}</AlertTitle>
      <AlertDescription>
        {t("orders.overdueAlert", { count: 1 })}
        {estimatedDate && (
          <>
            {" — "}
            <span className="font-semibold">
              {new Date(estimatedDate).toLocaleDateString("vi-VN", {
                year: "numeric",
                month: "long",
                day: "numeric",
              })}
            </span>
          </>
        )}
      </AlertDescription>
    </Alert>
  )
}
