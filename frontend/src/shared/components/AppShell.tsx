import type React from "react"
import { Outlet } from "@tanstack/react-router"
import { SidebarProvider, SidebarInset } from "@/shared/components/Sidebar"
import { AppSidebar } from "@/shared/components/AppSidebar"
import { SiteHeader } from "@/shared/components/SiteHeader"
import { SessionWarningModal } from "@/features/auth/components/SessionWarningModal"
import { useSession } from "@/features/auth/hooks/useSession"
import { useAuth } from "@/features/auth/hooks/useAuth"

export function AppShell() {
  const { showWarning, remainingSeconds, extendSession } = useSession()
  const { logout } = useAuth()

  return (
    <SidebarProvider
      style={
        {
          "--sidebar-width": "calc(var(--spacing) * 72)",
          "--header-height": "calc(var(--spacing) * 12)",
        } as React.CSSProperties
      }
    >
      <AppSidebar variant="inset" />
      <SidebarInset>
        <SiteHeader />
        <div className="flex flex-1 flex-col">
          <div className="@container/main flex flex-1 flex-col gap-2">
            <div className="flex-1 p-4 md:p-6">
              <Outlet />
            </div>
          </div>
        </div>
      </SidebarInset>

      <SessionWarningModal
        open={showWarning}
        remainingSeconds={remainingSeconds}
        onExtend={extendSession}
        onLogout={logout}
      />
    </SidebarProvider>
  )
}
