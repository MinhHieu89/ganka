import { useCallback, useState } from "react"
import { useQueryClient } from "@tanstack/react-query"
import { receptionistKeys } from "@/features/receptionist/api/receptionist-api"

export function useManualRefresh() {
  const queryClient = useQueryClient()
  const [isRefreshing, setIsRefreshing] = useState(false)

  const refresh = useCallback(async () => {
    setIsRefreshing(true)
    try {
      await queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
    } finally {
      setIsRefreshing(false)
    }
  }, [queryClient])

  return { refresh, isRefreshing }
}
