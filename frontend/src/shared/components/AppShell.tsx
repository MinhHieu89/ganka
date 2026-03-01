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
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <SiteHeader />
        <main className="flex-1 p-4 md:p-6">
          <Outlet />
        </main>
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
