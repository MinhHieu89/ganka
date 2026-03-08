import type { ComponentProps } from "react"
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
  IconBuilding,
  IconBoxSeam,
  IconChevronDown,
  IconClock,
  IconTemplate,
  IconClipboardCheck,
} from "@tabler/icons-react"
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/shared/components/Collapsible"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuBadge,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
  SidebarRail,
} from "@/shared/components/Sidebar"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/shared/components/Tooltip"
import { NavUser } from "@/shared/components/NavUser"
import { useAuthStore } from "@/shared/stores/authStore"
import { usePendingCount } from "@/features/pharmacy/api/pharmacy-queries"

interface NavSubItem {
  titleKey: string
  to: string
}

interface NavItem {
  titleKey: string
  to: string
  icon: React.ComponentType<{ className?: string }>
  disabled?: boolean
  children?: NavSubItem[]
}

export function AppSidebar({ ...sidebarProps }: ComponentProps<typeof Sidebar>) {
  const { t } = useTranslation("common")
  const routerState = useRouterState()
  const currentPath = routerState.location.pathname
  const user = useAuthStore((s) => s.user)
  const { data: pendingCount } = usePendingCount()

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
    },
    {
      titleKey: "sidebar.appointments",
      to: "/appointments",
      icon: IconCalendar,
    },
    {
      titleKey: "sidebar.clinical",
      to: "/clinical",
      icon: IconStethoscope,
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
      children: [
        { titleKey: "sidebar.pharmacyInventory", to: "/pharmacy" },
        { titleKey: "sidebar.pharmacyQueue", to: "/pharmacy/queue" },
        { titleKey: "sidebar.pharmacySuppliers", to: "/pharmacy/suppliers" },
        { titleKey: "sidebar.pharmacyStockImport", to: "/pharmacy/stock-import" },
        { titleKey: "sidebar.pharmacyOtcSales", to: "/pharmacy/otc-sales" },
      ],
    },
    {
      titleKey: "sidebar.consumables",
      to: "/consumables",
      icon: IconBoxSeam,
    },
    {
      titleKey: "sidebar.billing",
      to: "/billing",
      icon: IconReceipt,
      children: [
        { titleKey: "sidebar.billingDashboard", to: "/billing" },
        { titleKey: "sidebar.billingShifts", to: "/billing/shifts" },
      ],
    },
    {
      titleKey: "sidebar.optical",
      to: "/optical",
      icon: IconEyeglass,
      children: [
        { titleKey: "sidebar.opticalFrames", to: "/optical/frames" },
        { titleKey: "sidebar.opticalLenses", to: "/optical/lenses" },
        { titleKey: "sidebar.opticalOrders", to: "/optical/orders" },
        { titleKey: "sidebar.opticalCombos", to: "/optical/combos" },
        { titleKey: "sidebar.opticalWarranty", to: "/optical/warranty" },
        { titleKey: "sidebar.opticalStocktaking", to: "/optical/stocktaking" },
      ],
    },
    {
      titleKey: "sidebar.treatments",
      to: "/treatments",
      icon: IconHeartbeat,
      children: [
        { titleKey: "sidebar.treatmentsList", to: "/treatments" },
        { titleKey: "sidebar.treatmentTemplates", to: "/treatments/templates" },
        { titleKey: "sidebar.treatmentApprovals", to: "/treatments/approvals" },
      ],
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
    {
      titleKey: "sidebar.clinicSettings",
      to: "/admin/clinic-settings",
      icon: IconBuilding,
    },
  ]

  const renderNavItems = (items: NavItem[]) =>
    items.map((item) => {
      const isActive = currentPath === item.to || currentPath.startsWith(item.to + "/")

      if (item.disabled) {
        return (
          <SidebarMenuItem key={item.to}>
            <Tooltip>
              <TooltipTrigger asChild>
                <SidebarMenuButton
                  disabled
                  className="opacity-40 cursor-not-allowed"
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
          </SidebarMenuItem>
        )
      }

      if (item.children && item.children.length > 0) {
        const isParentActive = currentPath === item.to || currentPath.startsWith(item.to + "/")
        const hasBadge = item.titleKey === "sidebar.pharmacy" && pendingCount != null && pendingCount > 0

        return (
          <Collapsible key={item.to} defaultOpen={isParentActive} className="group/collapsible">
            <SidebarMenuItem>
              <CollapsibleTrigger asChild>
                <SidebarMenuButton
                  isActive={isParentActive}
                  tooltip={t(item.titleKey)}
                  className={isParentActive ? "font-medium" : ""}
                >
                  <item.icon className="h-4 w-4" />
                  <span>{t(item.titleKey)}</span>
                  <IconChevronDown className="ml-auto h-4 w-4 transition-transform duration-200 group-data-[state=open]/collapsible:rotate-180" />
                </SidebarMenuButton>
              </CollapsibleTrigger>
              {hasBadge && (
                <SidebarMenuBadge className="bg-primary text-primary-foreground text-xs font-medium px-1.5 py-0.5 rounded-full">
                  {pendingCount}
                </SidebarMenuBadge>
              )}
              <CollapsibleContent>
                <SidebarMenuSub>
                  {item.children.map((child) => {
                    const isChildActive =
                      child.to === item.to
                        ? currentPath === child.to
                        : currentPath === child.to || currentPath.startsWith(child.to + "/")
                    return (
                      <SidebarMenuSubItem key={child.to}>
                        <SidebarMenuSubButton asChild isActive={isChildActive}>
                          <Link to={child.to}>
                            <span>{t(child.titleKey)}</span>
                          </Link>
                        </SidebarMenuSubButton>
                      </SidebarMenuSubItem>
                    )
                  })}
                </SidebarMenuSub>
              </CollapsibleContent>
            </SidebarMenuItem>
          </Collapsible>
        )
      }

      return (
        <SidebarMenuItem key={item.to}>
          <SidebarMenuButton
            asChild
            isActive={isActive}
            tooltip={t(item.titleKey)}
            className={isActive ? "font-medium" : ""}
          >
            <Link to={item.to}>
              <item.icon className="h-4 w-4" />
              <span>{t(item.titleKey)}</span>
            </Link>
          </SidebarMenuButton>
        </SidebarMenuItem>
      )
    })

  return (
    <Sidebar collapsible="offcanvas" {...sidebarProps}>
      <SidebarHeader className="border-b border-sidebar-border/50">
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size="lg" asChild className="hover:bg-transparent">
              <Link to="/dashboard">
                <div className="relative flex aspect-square size-9 items-center justify-center bg-primary text-primary-foreground text-sm font-bold tracking-wider shadow-sm">
                  <span className="relative z-10">G</span>
                  <div className="absolute inset-0 bg-gradient-to-br from-white/10 to-transparent" />
                </div>
                <div className="flex flex-col gap-0 leading-none">
                  <span className="font-semibold text-[15px] tracking-tight">Ganka28</span>
                  <span className="text-[11px] text-muted-foreground/70 font-medium uppercase tracking-widest">
                    Ophthalmology
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
