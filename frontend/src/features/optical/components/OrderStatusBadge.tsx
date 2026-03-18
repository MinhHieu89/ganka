import { useTranslation } from "react-i18next"
import { cn } from "@/shared/lib/utils"

// Status enum mapping:
//   0 = Ordered, 1 = Processing, 2 = Received, 3 = Ready (for pickup), 4 = Delivered

const STATUS_CLASS: Record<number, string> = {
  0: "bg-blue-100 text-blue-800 border-blue-200 dark:bg-blue-950/40 dark:text-blue-300 dark:border-blue-800",
  1: "bg-yellow-100 text-yellow-800 border-yellow-200 dark:bg-yellow-950/40 dark:text-yellow-300 dark:border-yellow-800",
  2: "bg-purple-100 text-purple-800 border-purple-200 dark:bg-purple-950/40 dark:text-purple-300 dark:border-purple-800",
  3: "bg-green-100 text-green-800 border-green-200 dark:bg-green-950/40 dark:text-green-300 dark:border-green-800",
  4: "bg-gray-100 text-gray-600 border-gray-200 dark:bg-gray-800/60 dark:text-gray-400 dark:border-gray-700",
}

const STATUS_KEYS: Record<number, string> = {
  0: "enums.orderStatus.ordered",
  1: "enums.orderStatus.processing",
  2: "enums.orderStatus.received",
  3: "enums.orderStatus.ready",
  4: "enums.orderStatus.delivered",
}

interface OrderStatusBadgeProps {
  status: number
  isOverdue?: boolean
  isPaymentConfirmed?: boolean
  className?: string
}

export function OrderStatusBadge({
  status,
  isOverdue,
  isPaymentConfirmed,
  className,
}: OrderStatusBadgeProps) {
  const { t } = useTranslation("optical")

  const statusClassName = STATUS_CLASS[status] ?? "bg-gray-100 text-gray-600 border-gray-200"
  const label = STATUS_KEYS[status] ? t(STATUS_KEYS[status]) : `Status ${status}`

  return (
    <span className={cn("inline-flex items-center gap-1 flex-wrap", className)}>
      <span
        className={cn(
          "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold",
          statusClassName,
        )}
      >
        {label}
      </span>

      {isOverdue && status !== 4 && (
        <span className="inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-semibold bg-red-100 text-red-700 border-red-200 dark:bg-red-950/40 dark:text-red-400 dark:border-red-800">
          {t("orders.overdue")}
        </span>
      )}

      {isPaymentConfirmed !== undefined && !isPaymentConfirmed && (
        <span className="inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium bg-orange-50 text-orange-700 border-orange-200 dark:bg-orange-950/40 dark:text-orange-400 dark:border-orange-800">
          {t("enums.warrantyApproval.pending")}
        </span>
      )}
    </span>
  )
}
