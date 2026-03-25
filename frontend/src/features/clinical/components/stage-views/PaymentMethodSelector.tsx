import { Button } from "@/shared/components/Button"
import { cn } from "@/shared/lib/utils"
import { IconCash, IconCreditCard, IconTransfer } from "@tabler/icons-react"

export type PaymentMethod = "cash" | "card" | "transfer"

interface PaymentMethodSelectorProps {
  value: PaymentMethod
  onChange: (method: PaymentMethod) => void
}

const METHODS: { key: PaymentMethod; label: string; icon: typeof IconCash }[] = [
  { key: "cash", label: "Tiền mặt", icon: IconCash },
  { key: "card", label: "Thẻ", icon: IconCreditCard },
  { key: "transfer", label: "Chuyển khoản", icon: IconTransfer },
]

export function PaymentMethodSelector({ value, onChange }: PaymentMethodSelectorProps) {
  return (
    <div className="grid grid-cols-3 gap-3">
      {METHODS.map(({ key, label, icon: Icon }) => {
        const isSelected = value === key
        return (
          <Button
            key={key}
            type="button"
            variant="outline"
            className={cn(
              "h-auto flex flex-col items-center gap-2 py-4",
              isSelected
                ? "border-blue-500 bg-blue-50 text-blue-700 hover:bg-blue-50"
                : "bg-white hover:bg-muted/50",
            )}
            onClick={() => onChange(key)}
          >
            <Icon className="h-6 w-6" />
            <span className="text-sm font-medium">{label}</span>
          </Button>
        )
      })}
    </div>
  )
}
