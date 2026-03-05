import { useState } from "react"
import type { ReactNode } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconLoader2 } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"

interface PrintButtonProps {
  onClick: () => Promise<Blob>
  label: string
  disabled?: boolean
  icon?: ReactNode
  variant?: "outline" | "ghost" | "default"
  size?: "sm" | "default"
}

export function PrintButton({
  onClick,
  label,
  disabled,
  icon,
  variant = "outline",
  size = "sm",
}: PrintButtonProps) {
  const { t } = useTranslation("clinical")
  const [isPrinting, setIsPrinting] = useState(false)

  const handlePrint = async () => {
    setIsPrinting(true)
    try {
      const blob = await onClick()
      const url = URL.createObjectURL(blob)
      window.open(url, "_blank")
      // Clean up blob URL after a delay to allow the new tab to load
      setTimeout(() => URL.revokeObjectURL(url), 30000)
    } catch {
      toast.error(t("prescription.printFailed"))
    } finally {
      setIsPrinting(false)
    }
  }

  return (
    <Button
      variant={variant}
      size={size}
      disabled={disabled || isPrinting}
      onClick={handlePrint}
    >
      {isPrinting ? (
        <IconLoader2 className="h-4 w-4 animate-spin" />
      ) : (
        icon
      )}
      {label}
    </Button>
  )
}
