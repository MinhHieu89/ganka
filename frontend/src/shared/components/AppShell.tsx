import { Outlet } from "@tanstack/react-router"
import { SidebarProvider, SidebarInset } from "@/shared/components/ui/sidebar"
import { AppSidebar } from "@/shared/components/AppSidebar"
import { TopBar } from "@/shared/components/TopBar"

export function AppShell() {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <TopBar />
        <main className="flex-1 p-4 md:p-6">
          <Outlet />
        </main>
      </SidebarInset>
    </SidebarProvider>
  )
}
