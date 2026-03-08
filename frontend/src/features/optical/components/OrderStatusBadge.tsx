import { cn } from "@/shared/lib/utils"

// Status enum mapping:
//   0 = Ordered, 1 = Processing, 2 = Received, 3 = Ready (for pickup), 4 = Delivered

interface StatusConfig {
  label: string
  className: string
}

const STATUS_CONFIG: Record<number, StatusConfig> = {
  0: {
    label: "Ordered",
    className:
      "bg-blue-100 text-blue-800 border-blue-200 dark:bg-blue-950/40 dark:text-blue-300 dark:border-blue-800",
  },
  1: {
    label: "Processing",
    className:
      "bg-yellow-100 text-yellow-800 border-yellow-200 dark:bg-yellow-950/40 dark:text-yellow-300 dark:border-yellow-800",
  },
  2: {
    label: "Received",
    className:
      "bg-purple-100 text-purple-800 border-purple-200 dark:bg-purple-950/40 dark:text-purple-300 dark:border-purple-800",
  },
  3: {
    label: "Ready for Pickup",
    className:
      "bg-green-100 text-green-800 border-green-200 dark:bg-green-950/40 dark:text-green-300 dark:border-green-800",
  },
  4: {
    label: "Delivered",
    className:
      "bg-gray-100 text-gray-600 border-gray-200 dark:bg-gray-800/60 dark:text-gray-400 dark:border-gray-700",
  },
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
  const config = STATUS_CONFIG[status] ?? {
    label: `Status ${status}`,
    className: "bg-gray-100 text-gray-600 border-gray-200",
  }

  return (
    <span className={cn("inline-flex items-center gap-1 flex-wrap", className)}>
      <span
        className={cn(
          "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold",
          config.className,
        )}
      >
        {config.label}
      </span>

      {isOverdue && status !== 4 && (
        <span className="inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-semibold bg-red-100 text-red-700 border-red-200 dark:bg-red-950/40 dark:text-red-400 dark:border-red-800">
          Overdue
        </span>
      )}

      {isPaymentConfirmed !== undefined && !isPaymentConfirmed && (
        <span className="inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium bg-orange-50 text-orange-700 border-orange-200 dark:bg-orange-950/40 dark:text-orange-400 dark:border-orange-800">
          Unpaid
        </span>
      )}
    </span>
  )
}
