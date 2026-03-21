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

    const isMounted = { current: true }

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
      if (!isMounted.current) return
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
      if (isMounted.current) setStatus("reconnecting")
    })

    connection.onreconnected(async () => {
      if (isMounted.current) setStatus("connected")
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
      if (isMounted.current) setStatus("disconnected")
    })

    // Start connection
    const start = async () => {
      try {
        await connection.start()
        if (isMounted.current) setStatus("connected")
        await connection.invoke("JoinVisit", visitId)
      } catch {
        if (isMounted.current) setStatus("disconnected")
      }
    }

    start()

    // Cleanup on unmount
    return () => {
      isMounted.current = false
      // Nullify ref to prevent double-invocation
      connectionRef.current = null

      const cleanup = async () => {
        // Check that connection is in a state where we can invoke methods
        if (
          connection.state === HubConnectionState.Connected ||
          connection.state === HubConnectionState.Connecting ||
          connection.state === HubConnectionState.Reconnecting
        ) {
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
        }
        await connection.stop()
      }
      cleanup()
    }
  }, [visitId]) // Reconnect when visitId changes

  return status
}

/**
 * Custom hook for connecting to the OsdiHub via SignalR for token-scoped groups.
 * Used by the treatment session flow where OSDI tokens have no VisitId.
 *
 * Joins a token group and calls the provided callback when the patient submits
 * their OSDI questionnaire via the public QR page.
 *
 * @param token - The OSDI token to listen for (undefined = no subscription)
 * @param onScoreReceived - Callback invoked with the OSDI score when received
 */
export function useOsdiTokenHub(
  token: string | undefined,
  onScoreReceived: (score: number) => void,
): ConnectionStatus {
  const [status, setStatus] = useState<ConnectionStatus>("disconnected")
  const connectionRef = useRef<HubConnection | null>(null)

  const tokenRef = useRef(token)
  tokenRef.current = token

  const onScoreReceivedRef = useRef(onScoreReceived)
  onScoreReceivedRef.current = onScoreReceived

  useEffect(() => {
    if (!token) return

    const isMounted = { current: true }

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

    // Register event handler -- call onScoreReceived when patient submits OSDI
    connection.on(
      "OsdiTokenSubmitted",
      (data: { token: string; score: number; severity: string }) => {
        if (!isMounted.current) return
        onScoreReceivedRef.current(data.score)
      },
    )

    // Connection lifecycle events
    connection.onreconnecting(() => {
      if (isMounted.current) setStatus("reconnecting")
    })

    connection.onreconnected(async () => {
      if (isMounted.current) setStatus("connected")
      // Re-join the token group (group membership lost on reconnect)
      try {
        const currentToken = tokenRef.current
        if (currentToken) {
          await connection.invoke("JoinToken", currentToken)
        }
      } catch {
        // Ignore errors; will retry on next reconnect
      }
    })

    connection.onclose(() => {
      if (isMounted.current) setStatus("disconnected")
    })

    // Start connection
    const start = async () => {
      try {
        await connection.start()
        if (isMounted.current) setStatus("connected")
        await connection.invoke("JoinToken", token)
      } catch {
        if (isMounted.current) setStatus("disconnected")
      }
    }

    start()

    // Cleanup on unmount
    return () => {
      isMounted.current = false
      connectionRef.current = null

      const cleanup = async () => {
        if (
          connection.state === HubConnectionState.Connected ||
          connection.state === HubConnectionState.Connecting ||
          connection.state === HubConnectionState.Reconnecting
        ) {
          if (connection.state === HubConnectionState.Connected) {
            try {
              const currentToken = tokenRef.current
              if (currentToken) {
                await connection.invoke("LeaveToken", currentToken)
              }
            } catch {
              // Ignore errors during cleanup
            }
          }
        }
        await connection.stop()
      }
      cleanup()
    }
  }, [token]) // Reconnect when token changes

  return status
}
