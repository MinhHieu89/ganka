import { useTranslation } from "react-i18next"
import { useRouterState } from "@tanstack/react-router"
import { SidebarTrigger } from "@/shared/components/Sidebar"
import { Separator } from "@/shared/components/Separator"
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/shared/components/Breadcrumb"
import { LanguageToggle } from "@/shared/components/LanguageToggle"
import { Avatar, AvatarFallback } from "@/shared/components/Avatar"
import { IconLogout } from "@tabler/icons-react"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator as DropdownSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/DropdownMenu"
import { Button } from "@/shared/components/Button"
import { useAuthStore } from "@/shared/stores/authStore"
import { useAuth } from "@/features/auth/hooks/useAuth"

/**
 * Map of route path segments to i18n keys under "sidebar.*" or "topbar.*" namespace.
 */
const segmentToI18nKey: Record<string, string> = {
  dashboard: "sidebar.dashboard",
  admin: "sidebar.admin",
  users: "sidebar.users",
  roles: "sidebar.roles",
  "audit-logs": "sidebar.auditLogs",
  settings: "sidebar.settings",
}

function getInitials(name?: string | null, email?: string | null): string {
  if (name) {
    const parts = name.split(" ").filter(Boolean)
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
    }
    return name.substring(0, 2).toUpperCase()
  }
  if (email) {
    return email.substring(0, 2).toUpperCase()
  }
  return "U"
}

export function SiteHeader() {
  const { t } = useTranslation("common")
  const routerState = useRouterState()
  const currentPath = routerState.location.pathname
  const user = useAuthStore((s) => s.user)
  const { logout } = useAuth()

  // Build breadcrumb segments from current path
  const segments = currentPath
    .split("/")
    .filter(Boolean)
    .filter((s) => s !== "_authenticated") // filter layout route segments

  const breadcrumbs = segments.map((segment, index) => {
    const i18nKey = segmentToI18nKey[segment]
    const label = i18nKey ? t(i18nKey) : segment.charAt(0).toUpperCase() + segment.slice(1)
    const path = "/" + segments.slice(0, index + 1).join("/")
    const isLast = index === segments.length - 1
    return { label, path, isLast }
  })

  const initials = getInitials(user?.fullName, user?.email)
  const displayName = user?.fullName ?? user?.email ?? t("topbar.profile")

  return (
    <header className="flex h-12 shrink-0 items-center border-b bg-background px-4 gap-2">
      <SidebarTrigger className="-ml-1" />
      <Separator orientation="vertical" className="mr-2 h-4" />
      <Breadcrumb>
        <BreadcrumbList>
          {breadcrumbs.map((crumb, index) => (
            <span key={crumb.path} className="contents">
              {index > 0 && <BreadcrumbSeparator />}
              <BreadcrumbItem>
                {crumb.isLast ? (
                  <BreadcrumbPage>{crumb.label}</BreadcrumbPage>
                ) : (
                  <BreadcrumbLink href={crumb.path}>{crumb.label}</BreadcrumbLink>
                )}
              </BreadcrumbItem>
            </span>
          ))}
        </BreadcrumbList>
      </Breadcrumb>

      <div className="flex-1" />

      <LanguageToggle />

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" size="sm" className="gap-2">
            <Avatar className="h-6 w-6">
              <AvatarFallback className="text-[10px]">{initials}</AvatarFallback>
            </Avatar>
            <span className="text-sm hidden sm:inline-block">
              {displayName}
            </span>
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="w-48">
          <DropdownMenuLabel>
            {user?.email ?? t("topbar.profile")}
          </DropdownMenuLabel>
          <DropdownSeparator />
          <DropdownMenuItem onClick={() => logout()}>
            <IconLogout className="h-4 w-4 mr-2" />
            {t("topbar.logout")}
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </header>
  )
}
