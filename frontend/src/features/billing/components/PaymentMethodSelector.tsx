import { cn } from "@/shared/lib/utils"
import {
  IconCash,
  IconBuildingBank,
  IconQrcode,
  IconCreditCard,
} from "@tabler/icons-react"

/** PaymentMethod enum values matching backend Billing.Domain.Enums.PaymentMethod */
export const PaymentMethod = {
  Cash: 0,
  BankTransfer: 1,
  QrVnPay: 2,
  QrMomo: 3,
  QrZaloPay: 4,
  CardVisa: 5,
  CardMastercard: 6,
} as const

export type PaymentMethodValue =
  (typeof PaymentMethod)[keyof typeof PaymentMethod]

interface PaymentMethodOption {
  value: PaymentMethodValue
  label: string
  icon: React.ReactNode
}

const PAYMENT_METHOD_OPTIONS: PaymentMethodOption[] = [
  {
    value: PaymentMethod.Cash,
    label: "Cash",
    icon: <IconCash className="h-5 w-5" />,
  },
  {
    value: PaymentMethod.BankTransfer,
    label: "Bank Transfer",
    icon: <IconBuildingBank className="h-5 w-5" />,
  },
  {
    value: PaymentMethod.QrVnPay,
    label: "VNPay",
    icon: <IconQrcode className="h-5 w-5" />,
  },
  {
    value: PaymentMethod.QrMomo,
    label: "MoMo",
    icon: <IconQrcode className="h-5 w-5" />,
  },
  {
    value: PaymentMethod.QrZaloPay,
    label: "ZaloPay",
    icon: <IconQrcode className="h-5 w-5" />,
  },
  {
    value: PaymentMethod.CardVisa,
    label: "Visa",
    icon: <IconCreditCard className="h-5 w-5" />,
  },
  {
    value: PaymentMethod.CardMastercard,
    label: "MC",
    icon: <IconCreditCard className="h-5 w-5" />,
  },
]

interface PaymentMethodSelectorProps {
  value: number | undefined
  onChange: (method: number) => void
  disabled?: boolean
}

export function PaymentMethodSelector({
  value,
  onChange,
  disabled,
}: PaymentMethodSelectorProps) {
  return (
    <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
      {PAYMENT_METHOD_OPTIONS.map((option) => (
        <button
          key={option.value}
          type="button"
          disabled={disabled}
          onClick={() => onChange(option.value)}
          className={cn(
            "flex flex-col items-center gap-1.5 rounded-lg border p-3 text-sm transition-colors",
            "hover:bg-accent hover:text-accent-foreground",
            "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
            "disabled:cursor-not-allowed disabled:opacity-50",
            value === option.value
              ? "border-primary bg-primary/10 text-primary font-medium"
              : "border-border bg-background text-muted-foreground",
          )}
        >
          {option.icon}
          <span className="text-xs leading-tight">{option.label}</span>
        </button>
      ))}
    </div>
  )
}

const REFERENCE_METHODS: readonly number[] = [
  PaymentMethod.BankTransfer,
  PaymentMethod.QrVnPay,
  PaymentMethod.QrMomo,
  PaymentMethod.QrZaloPay,
]

const CARD_METHODS: readonly number[] = [
  PaymentMethod.CardVisa,
  PaymentMethod.CardMastercard,
]

/**
 * Check if the selected payment method requires a reference number.
 * Bank transfer and QR methods require references.
 */
export function methodRequiresReference(method: number): boolean {
  return REFERENCE_METHODS.includes(method)
}

/**
 * Check if the selected payment method is a card payment.
 */
export function methodIsCard(method: number): boolean {
  return CARD_METHODS.includes(method)
}

/**
 * Get the card type string for the selected method.
 */
export function getCardType(method: number): string | null {
  if (method === PaymentMethod.CardVisa) return "Visa"
  if (method === PaymentMethod.CardMastercard) return "Mastercard"
  return null
}
