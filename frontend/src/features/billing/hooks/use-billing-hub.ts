import { useEffect, useRef, useState } from "react"
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
 * Uses useRef for queryClient to keep the useEffect dependency array empty,
 * preventing reconnection storms when queryClient reference changes.
 *
 * Handles automatic reconnection and re-joins group after reconnect.
 */
export function useBillingHub(): ConnectionStatus {
  const [status, setStatus] = useState<ConnectionStatus>("disconnected")
  const connectionRef = useRef<HubConnection | null>(null)
  const queryClient = useQueryClient()
  const queryClientRef = useRef(queryClient)
  queryClientRef.current = queryClient // Update ref each render, no effect trigger

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

    // Register event handlers -- use queryClientRef.current for stable closure
    connection.on("InvoiceCreated", () => {
      queryClientRef.current.invalidateQueries({ queryKey: billingKeys.pendingInvoices() })
    })

    connection.on("LineItemAdded", (notification: { invoiceId: string }) => {
      queryClientRef.current.invalidateQueries({ queryKey: billingKeys.invoice(notification.invoiceId) })
      queryClientRef.current.invalidateQueries({ queryKey: billingKeys.pendingInvoices() })
    })

    connection.on("LineItemRemoved", (notification: { invoiceId: string }) => {
      queryClientRef.current.invalidateQueries({ queryKey: billingKeys.invoice(notification.invoiceId) })
      queryClientRef.current.invalidateQueries({ queryKey: billingKeys.pendingInvoices() })
    })

    connection.on("InvoiceVoided", () => {
      queryClientRef.current.invalidateQueries({ queryKey: billingKeys.pendingInvoices() })
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
  }, []) // Empty -- stable connection, queryClient accessed via ref

  return status
}
