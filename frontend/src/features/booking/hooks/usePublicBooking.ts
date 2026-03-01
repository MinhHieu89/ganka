import { useState, useCallback } from "react"
import { useSubmitBooking, type SubmitBookingCommand } from "@/features/booking/api/booking-api"

type BookingState = "form" | "confirmation"

export function usePublicBooking() {
  const [state, setState] = useState<BookingState>("form")
  const [referenceNumber, setReferenceNumber] = useState("")
  const submitBooking = useSubmitBooking()

  const handleSubmit = useCallback(
    async (command: SubmitBookingCommand) => {
      const result = await submitBooking.mutateAsync(command)
      setReferenceNumber(result.referenceNumber)
      setState("confirmation")
      return result.referenceNumber
    },
    [submitBooking],
  )

  const resetForm = useCallback(() => {
    setState("form")
    setReferenceNumber("")
  }, [])

  return {
    state,
    referenceNumber,
    isSubmitting: submitBooking.isPending,
    submitError: submitBooking.error,
    handleSubmit,
    resetForm,
  }
}
