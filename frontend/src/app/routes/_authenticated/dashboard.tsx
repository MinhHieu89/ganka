import { createFileRoute } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { useAuthStore } from "@/shared/stores/authStore"
import {
  IconUsers,
  IconCalendar,
  IconStethoscope,
  IconActivity,
} from "@tabler/icons-react"

export const Route = createFileRoute("/_authenticated/dashboard")({
  component: DashboardPage,
})

function DashboardPage() {
  const { t } = useTranslation("common")
  const { t: tAuth } = useTranslation("auth")
  const user = useAuthStore((s) => s.user)
  const firstName = user?.fullName?.split(" ").pop() ?? user?.fullName ?? ""

  return (
    <div className="space-y-8">
      {/* Welcome section */}
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">
          {tAuth("login.welcome")}{firstName ? `, ${firstName}` : ""}
        </h1>
        <p className="text-muted-foreground mt-1">
          Ganka28 &middot; Ophthalmology Clinic Management
        </p>
      </div>

      {/* Stats overview */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card className="group hover:shadow-sm transition-shadow">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t("sidebar.patients")}
            </CardTitle>
            <IconUsers className="h-4 w-4 text-muted-foreground/50" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-semibold tabular-nums">&mdash;</div>
            <p className="text-xs text-muted-foreground/70 mt-1">{t("sidebar.comingSoon")}</p>
          </CardContent>
        </Card>

        <Card className="group hover:shadow-sm transition-shadow">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t("sidebar.appointments")}
            </CardTitle>
            <IconCalendar className="h-4 w-4 text-muted-foreground/50" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-semibold tabular-nums">&mdash;</div>
            <p className="text-xs text-muted-foreground/70 mt-1">{t("sidebar.comingSoon")}</p>
          </CardContent>
        </Card>

        <Card className="group hover:shadow-sm transition-shadow">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t("sidebar.clinical")}
            </CardTitle>
            <IconStethoscope className="h-4 w-4 text-muted-foreground/50" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-semibold tabular-nums">&mdash;</div>
            <p className="text-xs text-muted-foreground/70 mt-1">{t("sidebar.comingSoon")}</p>
          </CardContent>
        </Card>

        <Card className="group hover:shadow-sm transition-shadow">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t("sidebar.treatments")}
            </CardTitle>
            <IconActivity className="h-4 w-4 text-muted-foreground/50" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-semibold tabular-nums">&mdash;</div>
            <p className="text-xs text-muted-foreground/70 mt-1">{t("sidebar.comingSoon")}</p>
          </CardContent>
        </Card>
      </div>

      {/* Quick info section */}
      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="text-base">{t("sidebar.dashboard")}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <div className="flex size-12 items-center justify-center bg-primary/5 text-primary mb-4">
                <IconActivity className="h-6 w-6" />
              </div>
              <p className="text-sm text-muted-foreground max-w-sm">
                Patient workflows, appointment queues, and clinical activity will appear here as modules are activated.
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">System</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Version</span>
              <span className="font-mono text-xs bg-muted px-2 py-0.5">v1.0-alpha</span>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">{tAuth("admin.role")}</span>
              <span className="font-medium">{user?.permissions?.[0]?.split(".")?.[0] ?? "—"}</span>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">{t("topbar.language")}</span>
              <span className="font-medium uppercase text-xs tracking-wider">
                {document.documentElement.lang || "vi"}
              </span>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
