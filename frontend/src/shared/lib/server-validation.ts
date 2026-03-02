import type { UseFormSetError, FieldValues, Path } from "react-hook-form"

interface RFC7807ValidationError {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

/**
 * Parses a server error response and maps field-level validation errors
 * to react-hook-form's setError. Returns any non-field errors for display.
 *
 * Usage in form onError:
 *   const nonFieldErrors = handleServerValidationError(error, form.setError)
 *   if (nonFieldErrors.length > 0) { show in toast/alert }
 */
export function handleServerValidationError<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  fieldMap?: Record<string, Path<T>>,
): string[] {
  const nonFieldErrors: string[] = []

  // Parse the error - could be an Error object with the response body in message,
  // or could be a raw response object
  let parsed: RFC7807ValidationError | null = null

  if (error instanceof Error) {
    try {
      // Try to parse the error message as JSON (if the API client put it there)
      parsed = JSON.parse(error.message) as RFC7807ValidationError
    } catch {
      // Not JSON - treat as a generic error message
      nonFieldErrors.push(error.message)
      return nonFieldErrors
    }
  }

  if (!parsed?.errors) {
    // No structured errors - return the detail/title as non-field error
    const msg =
      parsed?.detail ||
      parsed?.title ||
      (error instanceof Error ? error.message : "Unknown error")
    nonFieldErrors.push(msg)
    return nonFieldErrors
  }

  // Map each field error to setError
  for (const [fieldName, messages] of Object.entries(parsed.errors)) {
    // Convert PascalCase backend field name to camelCase frontend field name
    const camelField = fieldName.charAt(0).toLowerCase() + fieldName.slice(1)
    const formField =
      fieldMap?.[fieldName] ??
      fieldMap?.[camelField] ??
      (camelField as Path<T>)

    try {
      setError(formField, {
        type: "server",
        message: messages[0], // Show first error message
      })
    } catch {
      // Field not in form - add to non-field errors
      nonFieldErrors.push(`${fieldName}: ${messages.join(", ")}`)
    }
  }

  return nonFieldErrors
}

/**
 * Helper: Check if an error is a server validation error (400 with errors dict)
 */
export function isServerValidationError(error: unknown): boolean {
  if (!(error instanceof Error)) return false
  try {
    const parsed = JSON.parse(error.message)
    return parsed?.status === 400 && parsed?.errors != null
  } catch {
    return false
  }
}
