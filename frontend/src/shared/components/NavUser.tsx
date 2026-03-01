import { useTranslation } from "react-i18next"
import { IconLogout, IconSelector } from "@tabler/icons-react"
import { Avatar, AvatarFallback } from "@/shared/components/Avatar"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/DropdownMenu"
import { SidebarMenuButton } from "@/shared/components/Sidebar"
import { useAuthStore } from "@/shared/stores/authStore"
import { useAuth } from "@/features/auth/hooks/useAuth"

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

export function NavUser() {
  const { t } = useTranslation("common")
  const user = useAuthStore((s) => s.user)
  const { logout } = useAuth()

  const initials = getInitials(user?.fullName, user?.email)
  const displayName = user?.fullName ?? user?.email ?? t("topbar.profile")

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <SidebarMenuButton
          size="lg"
          className="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground"
        >
          <Avatar className="h-8 w-8">
            <AvatarFallback className="text-xs">{initials}</AvatarFallback>
          </Avatar>
          <div className="grid flex-1 text-left text-sm leading-tight">
            <span className="truncate font-medium">{displayName}</span>
            {user?.email && user.fullName && (
              <span className="truncate text-xs text-muted-foreground">
                {user.email}
              </span>
            )}
          </div>
          <IconSelector className="ml-auto h-4 w-4" />
        </SidebarMenuButton>
      </DropdownMenuTrigger>
      <DropdownMenuContent
        className="w-[--radix-dropdown-menu-trigger-width] min-w-56"
        side="bottom"
        align="end"
        sideOffset={4}
      >
        <DropdownMenuLabel className="p-0 font-normal">
          <div className="flex items-center gap-2 px-1 py-1.5 text-left text-sm">
            <Avatar className="h-8 w-8">
              <AvatarFallback className="text-xs">{initials}</AvatarFallback>
            </Avatar>
            <div className="grid flex-1 text-left text-sm leading-tight">
              <span className="truncate font-medium">{displayName}</span>
              {user?.email && (
                <span className="truncate text-xs text-muted-foreground">
                  {user.email}
                </span>
              )}
            </div>
          </div>
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={() => logout()}>
          <IconLogout className="h-4 w-4 mr-2" />
          {t("topbar.logout")}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
