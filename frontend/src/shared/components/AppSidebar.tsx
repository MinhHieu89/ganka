import { useTranslation } from "react-i18next"
import { Link, useRouterState } from "@tanstack/react-router"
import {
  IconLayoutDashboard,
  IconUsers,
  IconShieldLock,
  IconFileText,
  IconSettings,
} from "@tabler/icons-react"
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from "@/shared/components/Sidebar"
import { useAuthStore } from "@/shared/stores/authStore"

interface NavItem {
  titleKey: string
  to: string
  icon: React.ComponentType<{ className?: string }>
}

export function AppSidebar() {
  const { t } = useTranslation("common")
  const routerState = useRouterState()
  const currentPath = routerState.location.pathname
  const user = useAuthStore((s) => s.user)

  // Check if user has admin permissions
  const hasAdminAccess =
    user?.permissions?.some(
      (p) =>
        p.startsWith("Auth.Manage") ||
        p.startsWith("Auth.View") ||
        p === "Auth.Manage" ||
        p === "Auth.View",
    ) ?? false

  const mainItems: NavItem[] = [
    {
      titleKey: "sidebar.dashboard",
      to: "/dashboard",
      icon: IconLayoutDashboard,
    },
  ]

  const adminItems: NavItem[] = [
    {
      titleKey: "sidebar.users",
      to: "/admin/users",
      icon: IconUsers,
    },
    {
      titleKey: "sidebar.roles",
      to: "/admin/roles",
      icon: IconShieldLock,
    },
    {
      titleKey: "sidebar.auditLogs",
      to: "/admin/audit-logs",
      icon: IconFileText,
    },
  ]

  const settingsItems: NavItem[] = [
    {
      titleKey: "sidebar.settings",
      to: "/settings",
      icon: IconSettings,
    },
  ]

  const renderNavItems = (items: NavItem[]) =>
    items.map((item) => (
      <SidebarMenuItem key={item.to}>
        <SidebarMenuButton
          asChild
          isActive={currentPath === item.to || currentPath.startsWith(item.to + "/")}
          tooltip={t(item.titleKey)}
        >
          <Link to={item.to}>
            <item.icon className="h-4 w-4" />
            <span>{t(item.titleKey)}</span>
          </Link>
        </SidebarMenuButton>
      </SidebarMenuItem>
    ))

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader className="border-b">
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size="lg" asChild>
              <Link to="/dashboard">
                <div className="flex aspect-square size-8 items-center justify-center bg-primary text-primary-foreground text-sm font-bold">
                  G
                </div>
                <div className="flex flex-col gap-0.5 leading-none">
                  <span className="font-semibold">{t("appName")}</span>
                  <span className="text-xs text-muted-foreground">
                    Clinic Management
                  </span>
                </div>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>{renderNavItems(mainItems)}</SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        {hasAdminAccess && (
          <SidebarGroup>
            <SidebarGroupLabel>{t("sidebar.admin")}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>{renderNavItems(adminItems)}</SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        )}

        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>{renderNavItems(settingsItems)}</SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarRail />
    </Sidebar>
  )
}
