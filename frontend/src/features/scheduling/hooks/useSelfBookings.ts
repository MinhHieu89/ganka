import { useMemo } from "react"
import { usePendingSelfBookings } from "@/features/scheduling/api/scheduling-api"

export function useSelfBookings() {
  const query = usePendingSelfBookings()

  const pendingCount = useMemo(
    () => query.data?.length ?? 0,
    [query.data],
  )

  return {
    ...query,
    pendingCount,
  }
}
