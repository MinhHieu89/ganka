import { useEffect, useRef, useState } from "react"
import {
  HubConnectionBuilder,
  HubConnection,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr"
import { useQueryClient } from "@tanstack/react-query"
import { useAuthStore } from "@/shared/stores/authStore"
import { clinicalKeys } from "@/features/clinical/api/clinical-api"

export type ConnectionStatus = "connected" | "reconnecting" | "disconnected"

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5255"
const HUB_URL = `${API_URL}/api/hubs/osdi`

/**
 * Custom hook for connecting to the OsdiHub via SignalR.
 * Joins a visit group and invalidates TanStack Query caches
 * when an OsdiSubmitted event occurs (patient submits via QR page).
 *
 * Uses useRef for queryClient to keep the useEffect dependency array stable,
 * preventing reconnection storms when queryClient reference changes.
 *
 * Handles automatic reconnection and re-joins group after reconnect.
 */
export function useOsdiHub(visitId: string | undefined): ConnectionStatus {
  const [status, setStatus] = useState<ConnectionStatus>("disconnected")
  const connectionRef = useRef<HubConnection | null>(null)
  const queryClient = useQueryClient()
  const queryClientRef = useRef(queryClient)
  queryClientRef.current = queryClient

  const visitIdRef = useRef(visitId)
  visitIdRef.current = visitId

  useEffect(() => {
    if (!visitId) return

    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => {
          return useAuthStore.getState().accessToken ?? ""
        },
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connectionRef.current = connection

    // Register event handler -- invalidate visit query cache on OSDI submission
    connection.on("OsdiSubmitted", () => {
      const currentVisitId = visitIdRef.current
      if (currentVisitId) {
        queryClientRef.current.invalidateQueries({
          queryKey: clinicalKeys.visit(currentVisitId),
        })
        queryClientRef.current.invalidateQueries({
          queryKey: clinicalKeys.osdiAnswers(currentVisitId),
        })
      }
    })

    // Connection lifecycle events
    connection.onreconnecting(() => {
      setStatus("reconnecting")
    })

    connection.onreconnected(async () => {
      setStatus("connected")
      // Re-join the visit group (group membership lost on reconnect)
      try {
        const currentVisitId = visitIdRef.current
        if (currentVisitId) {
          await connection.invoke("JoinVisit", currentVisitId)
        }
      } catch {
        // Ignore errors; will retry on next reconnect
      }
    })

    connection.onclose(() => {
      setStatus("disconnected")
    })

    // Start connection
    const start = async () => {
      try {
        await connection.start()
        setStatus("connected")
        await connection.invoke("JoinVisit", visitId)
      } catch {
        setStatus("disconnected")
      }
    }

    start()

    // Cleanup on unmount
    return () => {
      const cleanup = async () => {
        if (connection.state === HubConnectionState.Connected) {
          try {
            const currentVisitId = visitIdRef.current
            if (currentVisitId) {
              await connection.invoke("LeaveVisit", currentVisitId)
            }
          } catch {
            // Ignore errors during cleanup
          }
        }
        await connection.stop()
      }
      cleanup()
    }
  }, [visitId]) // Reconnect when visitId changes

  return status
}
