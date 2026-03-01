import { useTranslation } from "react-i18next"
import { IconLogout, IconUser } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { SidebarTrigger } from "@/shared/components/Sidebar"
import { Separator } from "@/shared/components/Separator"
import { LanguageToggle } from "@/shared/components/LanguageToggle"
import { useAuthStore } from "@/shared/stores/authStore"
import { useAuth } from "@/features/auth/hooks/useAuth"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/DropdownMenu"

export function TopBar() {
  const { t } = useTranslation("common")
  const user = useAuthStore((s) => s.user)
  const { logout } = useAuth()

  return (
    <header className="flex h-14 shrink-0 items-center border-b bg-background px-4 gap-2">
      <SidebarTrigger className="-ml-1" />
      <Separator orientation="vertical" className="mr-2 h-4" />

      <div className="flex-1" />

      <LanguageToggle />

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" size="sm" className="gap-2">
            <IconUser className="h-4 w-4" />
            <span className="text-sm hidden sm:inline-block">
              {user?.fullName ?? user?.email ?? t("topbar.profile")}
            </span>
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="w-48">
          <DropdownMenuLabel>
            {user?.email ?? t("topbar.profile")}
          </DropdownMenuLabel>
          <DropdownMenuSeparator />
          <DropdownMenuItem onClick={() => logout()}>
            <IconLogout className="h-4 w-4 mr-2" />
            {t("topbar.logout")}
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </header>
  )
}
