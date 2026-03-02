import { Alert, AlertDescription } from "@/shared/components/Alert"
import { IconX } from "@tabler/icons-react"

interface ServerValidationAlertProps {
  error: string | null
  onDismiss: () => void
}

/**
 * Displays non-field server validation errors as a destructive alert banner.
 * Used in dialog forms (inline) and non-dialog forms (at top of form area).
 */
export function ServerValidationAlert({
  error,
  onDismiss,
}: ServerValidationAlertProps) {
  if (!error) return null
  return (
    <Alert variant="destructive" className="mb-4">
      <AlertDescription className="flex items-center justify-between">
        <span>{error}</span>
        <button
          type="button"
          onClick={onDismiss}
          className="shrink-0 ml-2 p-0.5 hover:opacity-70 transition-opacity"
          aria-label="Dismiss"
        >
          <IconX className="h-3.5 w-3.5" />
        </button>
      </AlertDescription>
    </Alert>
  )
}
