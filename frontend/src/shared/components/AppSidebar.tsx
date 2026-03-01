import { useTranslation } from "react-i18next"
import { Link, useRouterState } from "@tanstack/react-router"
import {
  IconLayoutDashboard,
  IconUsers,
  IconShieldLock,
  IconFileText,
  IconCalendar,
  IconStethoscope,
  IconCamera,
  IconPill,
  IconMedicineSyrup,
  IconReceipt,
  IconEyeglass,
  IconHeartbeat,
} from "@tabler/icons-react"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from "@/shared/components/Sidebar"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/shared/components/Tooltip"
import { NavUser } from "@/shared/components/NavUser"
import { useAuthStore } from "@/shared/stores/authStore"

interface NavItem {
  titleKey: string
  to: string
  icon: React.ComponentType<{ className?: string }>
  disabled?: boolean
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

  const clinicItems: NavItem[] = [
    {
      titleKey: "sidebar.patients",
      to: "/patients",
      icon: IconUsers,
      disabled: true,
    },
    {
      titleKey: "sidebar.appointments",
      to: "/appointments",
      icon: IconCalendar,
      disabled: true,
    },
    {
      titleKey: "sidebar.clinical",
      to: "/clinical",
      icon: IconStethoscope,
      disabled: true,
    },
    {
      titleKey: "sidebar.imaging",
      to: "/imaging",
      icon: IconCamera,
      disabled: true,
    },
    {
      titleKey: "sidebar.prescriptions",
      to: "/prescriptions",
      icon: IconPill,
      disabled: true,
    },
  ]

  const operationsItems: NavItem[] = [
    {
      titleKey: "sidebar.pharmacy",
      to: "/pharmacy",
      icon: IconMedicineSyrup,
      disabled: true,
    },
    {
      titleKey: "sidebar.billing",
      to: "/billing",
      icon: IconReceipt,
      disabled: true,
    },
    {
      titleKey: "sidebar.optical",
      to: "/optical",
      icon: IconEyeglass,
      disabled: true,
    },
    {
      titleKey: "sidebar.treatments",
      to: "/treatments",
      icon: IconHeartbeat,
      disabled: true,
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

  const renderNavItems = (items: NavItem[]) =>
    items.map((item) => (
      <SidebarMenuItem key={item.to}>
        {item.disabled ? (
          <Tooltip>
            <TooltipTrigger asChild>
              <SidebarMenuButton
                disabled
                className="opacity-50 cursor-not-allowed"
                tooltip={t(item.titleKey)}
              >
                <item.icon className="h-4 w-4" />
                <span>{t(item.titleKey)}</span>
              </SidebarMenuButton>
            </TooltipTrigger>
            <TooltipContent side="right">
              {t("sidebar.comingSoon")}
            </TooltipContent>
          </Tooltip>
        ) : (
          <SidebarMenuButton
            asChild
            isActive={
              currentPath === item.to ||
              currentPath.startsWith(item.to + "/")
            }
            tooltip={t(item.titleKey)}
          >
            <Link to={item.to}>
              <item.icon className="h-4 w-4" />
              <span>{t(item.titleKey)}</span>
            </Link>
          </SidebarMenuButton>
        )}
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
        {/* Main navigation */}
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>{renderNavItems(mainItems)}</SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        {/* Clinic group -- placeholder items for future phases */}
        <SidebarGroup>
          <SidebarGroupLabel>{t("sidebar.clinic")}</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>{renderNavItems(clinicItems)}</SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        {/* Operations group -- placeholder items for future phases */}
        <SidebarGroup>
          <SidebarGroupLabel>{t("sidebar.operations")}</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>{renderNavItems(operationsItems)}</SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        {/* Admin group -- conditional on permissions */}
        {hasAdminAccess && (
          <SidebarGroup>
            <SidebarGroupLabel>{t("sidebar.admin")}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>{renderNavItems(adminItems)}</SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        )}
      </SidebarContent>

      <SidebarFooter>
        <SidebarMenu>
          <SidebarMenuItem>
            <NavUser />
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>

      <SidebarRail />
    </Sidebar>
  )
}
