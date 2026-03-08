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
  if (orderCount !== undefined) {
    return (
      <Alert variant="destructive" className={cn("", className)}>
        <IconAlertTriangle className="h-4 w-4" />
        <AlertTitle>Overdue Orders</AlertTitle>
        <AlertDescription>
          {orderCount} order{orderCount !== 1 ? "s are" : " is"} past the estimated delivery date
          and require attention.
        </AlertDescription>
      </Alert>
    )
  }

  return (
    <Alert variant="destructive" className={cn("", className)}>
      <IconAlertTriangle className="h-4 w-4" />
      <AlertTitle>Order Overdue</AlertTitle>
      <AlertDescription>
        This order is past its estimated delivery date
        {estimatedDate ? (
          <>
            {" "}
            of{" "}
            <span className="font-semibold">
              {new Date(estimatedDate).toLocaleDateString("vi-VN", {
                year: "numeric",
                month: "long",
                day: "numeric",
              })}
            </span>
          </>
        ) : (
          ""
        )}
        . Please follow up with the processing team.
      </AlertDescription>
    </Alert>
  )
}
