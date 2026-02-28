import { createFileRoute } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { z } from "zod"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card"
import { Input } from "@/shared/components/ui/input"
import { Label } from "@/shared/components/ui/label"
import { Button } from "@/shared/components/ui/button"
import { LanguageToggle } from "@/shared/components/LanguageToggle"

const loginSearchSchema = z.object({
  redirect: z.string().optional(),
})

export const Route = createFileRoute("/login")({
  validateSearch: loginSearchSchema,
  component: LoginPage,
})

function LoginPage() {
  const { t } = useTranslation("auth")

  return (
    <div className="flex min-h-screen">
      {/* Left panel - branding */}
      <div className="hidden lg:flex lg:flex-1 items-center justify-center bg-primary/5">
        <div className="text-center space-y-4">
          <div className="flex items-center justify-center">
            <div className="flex aspect-square size-16 items-center justify-center bg-primary text-primary-foreground text-2xl font-bold">
              G
            </div>
          </div>
          <h1 className="text-3xl font-bold">Ganka28</h1>
          <p className="text-muted-foreground max-w-sm">
            Clinic Management System
          </p>
        </div>
      </div>

      {/* Right panel - login form */}
      <div className="flex flex-1 items-center justify-center p-8">
        <div className="w-full max-w-sm space-y-6">
          <div className="flex justify-end">
            <LanguageToggle />
          </div>

          <Card>
            <CardHeader className="text-center">
              <CardTitle className="text-xl">{t("login.title")}</CardTitle>
              <CardDescription>{t("login.welcome")}</CardDescription>
            </CardHeader>
            <CardContent>
              <form className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="email">{t("login.email")}</Label>
                  <Input
                    id="email"
                    type="email"
                    placeholder="doctor@ganka28.com"
                    disabled
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="password">{t("login.password")}</Label>
                  <Input id="password" type="password" disabled />
                </div>
                <Button type="button" className="w-full" disabled>
                  {t("login.submit")}
                </Button>
                <p className="text-xs text-center text-muted-foreground">
                  Login functionality will be implemented in Plan 05
                </p>
              </form>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
