import { useEffect, useRef, useState } from "react"
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
  const [error, setError] = useState<string | null>(null)

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
        setError(null)
        onScan(value)
        if (inputRef.current) {
          inputRef.current.value = ""
        }
      } else if (value) {
        setError("Invalid barcode. Must be exactly 13 digits.")
        setTimeout(() => setError(null), 3000)
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
        className={cn("pl-9", error && "border-red-500")}
        inputMode="numeric"
        autoComplete="off"
      />
      {error && (
        <p className="absolute -bottom-5 left-0 text-xs text-red-500">{error}</p>
      )}
    </div>
  )
}
