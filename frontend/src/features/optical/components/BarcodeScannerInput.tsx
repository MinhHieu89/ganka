import { useEffect, useRef } from "react"
import { IconBarcode } from "@tabler/icons-react"
import { Input } from "@/shared/components/Input"
import { cn } from "@/shared/lib/utils"

interface BarcodeScannerInputProps {
  onScan: (barcode: string) => void
  autoFocus?: boolean
  className?: string
}

export function BarcodeScannerInput({
  onScan,
  autoFocus = false,
  className,
}: BarcodeScannerInputProps) {
  const inputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (autoFocus && inputRef.current) {
      inputRef.current.focus()
    }
  }, [autoFocus])

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault()
      const value = inputRef.current?.value?.trim()
      if (value && /^\d{13}$/.test(value)) {
        onScan(value)
        if (inputRef.current) {
          inputRef.current.value = ""
        }
      }
    }
  }

  return (
    <div className={cn("relative", className)}>
      <div className="pointer-events-none absolute inset-y-0 left-3 flex items-center">
        <IconBarcode className="text-muted-foreground h-4 w-4" />
      </div>
      <Input
        ref={inputRef}
        onKeyDown={handleKeyDown}
        placeholder="Scan barcode..."
        className="pl-9"
        inputMode="numeric"
        autoComplete="off"
      />
    </div>
  )
}
