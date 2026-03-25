import { cn } from "@/shared/lib/utils"

interface ValidationMessageProps {
  state: "error" | "warning" | "success"
  message: string
}

const STATE_STYLES: Record<string, string> = {
  error: "text-red-600 border-l-red-500",
  warning: "text-amber-600 border-l-amber-500",
  success: "text-green-600 border-l-green-500",
}

export function ValidationMessage({ state, message }: ValidationMessageProps) {
  return (
    <div
      className={cn(
        "border-l-4 px-3 py-2 text-sm",
        STATE_STYLES[state],
      )}
    >
      {message}
    </div>
  )
}
