import { useEffect, useRef, useState, useCallback } from "react"
import {
  HubConnectionBuilder,
  HubConnection,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr"
import { useQueryClient } from "@tanstack/react-query"
import { useAuthStore } from "@/shared/stores/authStore"
import { billingKeys } from "@/features/billing/api/billing-api"

export type ConnectionStatus = "connected" | "reconnecting" | "disconnected"

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5255"
const HUB_URL = `${API_URL}/api/hubs/billing`

/**
 * Custom hook for connecting to the BillingHub via SignalR.
 * Joins the cashier-dashboard group and invalidates TanStack Query caches
 * when billing events occur (InvoiceCreated, LineItemAdded, InvoiceVoided).
 *
 * Handles automatic reconnection and re-joins group after reconnect.
 */
export function useBillingHub(): ConnectionStatus {
  const [status, setStatus] = useState<ConnectionStatus>("disconnected")
  const connectionRef = useRef<HubConnection | null>(null)
  const queryClient = useQueryClient()

  const invalidatePendingInvoices = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: billingKeys.pendingInvoices() })
  }, [queryClient])

  const invalidateInvoice = useCallback(
    (invoiceId: string) => {
      queryClient.invalidateQueries({
        queryKey: billingKeys.invoice(invoiceId),
      })
      queryClient.invalidateQueries({
        queryKey: billingKeys.pendingInvoices(),
      })
    },
    [queryClient],
  )

  useEffect(() => {
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

    // Register event handlers
    connection.on("InvoiceCreated", () => {
      invalidatePendingInvoices()
    })

    connection.on("LineItemAdded", (_notification: { invoiceId: string }) => {
      invalidateInvoice(_notification.invoiceId)
    })

    connection.on("InvoiceVoided", () => {
      invalidatePendingInvoices()
    })

    // Connection lifecycle events
    connection.onreconnecting(() => {
      setStatus("reconnecting")
    })

    connection.onreconnected(async () => {
      setStatus("connected")
      // Re-join the cashier-dashboard group (group membership lost on reconnect)
      try {
        await connection.invoke("JoinCashierDashboard")
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
        await connection.invoke("JoinCashierDashboard")
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
            await connection.invoke("LeaveCashierDashboard")
          } catch {
            // Ignore errors during cleanup
          }
        }
        await connection.stop()
      }
      cleanup()
    }
  }, [invalidatePendingInvoices, invalidateInvoice])

  return status
}
