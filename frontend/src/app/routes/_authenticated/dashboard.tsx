import { createFileRoute } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card"

export const Route = createFileRoute("/_authenticated/dashboard")({
  component: DashboardPage,
})

function DashboardPage() {
  const { t } = useTranslation("auth")

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">{t("login.welcome")}</h1>
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t("login.welcome")}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">
              Ganka28 Clinic Management System
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
