import { IconCircleCheck } from "@tabler/icons-react"

interface VisitCompleteBannerProps {
  totalCollected: number
  visitDuration: string
  completedDate: string
}

const currencyFormatter = new Intl.NumberFormat("vi-VN", {
  style: "currency",
  currency: "VND",
})

export function VisitCompleteBanner({
  totalCollected,
  visitDuration,
  completedDate,
}: VisitCompleteBannerProps) {
  return (
    <div className="bg-green-50 border border-green-200 rounded-md p-5">
      <div className="flex items-start gap-4">
        <IconCircleCheck className="h-8 w-8 text-green-600 shrink-0 mt-0.5" />
        <div className="space-y-2 flex-1">
          <h2 className="text-lg font-semibold text-green-800">
            L\u01B0\u1EE3t kh\u00E1m ho\u00E0n t\u1EA5t
          </h2>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 text-sm">
            <div>
              <span className="text-green-600">T\u1ED5ng thu:</span>{" "}
              <span className="font-semibold text-green-800">
                {currencyFormatter.format(totalCollected)}
              </span>
            </div>
            <div>
              <span className="text-green-600">Th\u1EDDi gian kh\u00E1m:</span>{" "}
              <span className="font-semibold text-green-800">{visitDuration}</span>
            </div>
            <div>
              <span className="text-green-600">Ng\u00E0y:</span>{" "}
              <span className="font-semibold text-green-800">{completedDate}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
