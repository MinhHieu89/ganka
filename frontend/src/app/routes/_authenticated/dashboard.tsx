import { createFileRoute, Link } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Skeleton } from "@/shared/components/Skeleton"
import { useAuthStore } from "@/shared/stores/authStore"
import { useRecentPatientsStore } from "@/shared/stores/recentPatientsStore"
import { useDashboardStats } from "@/features/dashboard/api/dashboard-api"
import { ReceptionistDashboard } from "@/features/receptionist/components/ReceptionistDashboard"
import {
  IconUsers,
  IconCalendar,
  IconStethoscope,
  IconActivity,
  IconUser,
  IconChevronRight,
} from "@tabler/icons-react"

export const Route = createFileRoute("/_authenticated/dashboard")({
  component: DashboardPage,
})

function StatValue({ value, isLoading }: { value: number | undefined; isLoading: boolean }) {
  if (isLoading) {
    return <Skeleton className="h-8 w-16" />
  }
  return (
    <div className="text-2xl font-semibold tabular-nums">
      {value?.toLocaleString() ?? 0}
    </div>
  )
}

function DashboardPage() {
  const user = useAuthStore((s) => s.user)
  const isReceptionist = user?.roles?.includes("Receptionist") ?? false

  if (isReceptionist) {
    return <ReceptionistDashboard />
  }

  return <DefaultDashboard />
}

function DefaultDashboard() {
  const { t } = useTranslation("common")
  const { t: tAuth } = useTranslation("auth")
  const { t: tPatient } = useTranslation("patient")
  const user = useAuthStore((s) => s.user)
  const recentPatients = useRecentPatientsStore((s) => s.recent)
  const firstName = user?.fullName?.split(" ").pop() ?? user?.fullName ?? ""
  const { data: stats, isLoading: statsLoading } = useDashboardStats()

  return (
    <div className="space-y-8">
      {/* Welcome section */}
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">
          {tAuth("login.welcome")}{firstName ? `, ${firstName}` : ""}
        </h1>
        <p className="text-muted-foreground mt-1">
          Ganka28 &middot; Clinic Management
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
            <StatValue value={stats?.totalPatients} isLoading={statsLoading} />
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
            <StatValue value={stats?.todayAppointments} isLoading={statsLoading} />
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
            <StatValue value={stats?.activeVisits} isLoading={statsLoading} />
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
            <StatValue value={stats?.activeTreatments} isLoading={statsLoading} />
          </CardContent>
        </Card>
      </div>

      {/* Quick info section */}
      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-base">{tPatient("recent")}</CardTitle>
            <Link
              to={"/patients" as string}
              className="text-xs text-muted-foreground hover:text-foreground transition-colors"
            >
              {tPatient("list")}
              <IconChevronRight className="inline h-3 w-3 ml-0.5" />
            </Link>
          </CardHeader>
          <CardContent>
            {recentPatients.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-12 text-center">
                <div className="flex size-12 items-center justify-center bg-primary/5 text-primary mb-4">
                  <IconUsers className="h-6 w-6" />
                </div>
                <p className="text-sm text-muted-foreground max-w-sm">
                  {tPatient("noRecent")}
                </p>
              </div>
            ) : (
              <div className="space-y-1">
                {recentPatients.slice(0, 5).map((patient) => (
                  <Link
                    key={patient.id}
                    to={"/patients/$patientId" as string}
                    params={{ patientId: patient.id } as never}
                    className="flex items-center gap-3 p-2 -mx-2 hover:bg-muted/50 transition-colors group"
                  >
                    <div className="flex size-8 items-center justify-center bg-primary/5 text-primary shrink-0">
                      <IconUser className="h-4 w-4" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium truncate group-hover:text-primary transition-colors">
                        {patient.fullName}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {patient.patientCode}
                        {patient.phone && <> &middot; {patient.phone}</>}
                      </p>
                    </div>
                    <IconChevronRight className="h-4 w-4 text-muted-foreground/50 group-hover:text-muted-foreground transition-colors shrink-0" />
                  </Link>
                ))}
              </div>
            )}
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
              <span className="font-medium">{user?.permissions?.[0]?.split(".")?.[0] ?? "---"}</span>
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
