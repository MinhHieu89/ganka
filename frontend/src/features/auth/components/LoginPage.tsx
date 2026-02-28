import { useTranslation } from "react-i18next"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card"
import { LanguageToggle } from "@/shared/components/LanguageToggle"
import { LoginForm } from "./LoginForm"

interface LoginPageProps {
  redirectTo?: string
}

export function LoginPage({ redirectTo }: LoginPageProps) {
  const { t } = useTranslation("auth")

  return (
    <div className="flex min-h-screen">
      {/* Left panel - branding */}
      <div className="hidden lg:flex lg:flex-1 items-center justify-center bg-gradient-to-br from-primary/10 via-primary/5 to-background">
        <div className="text-center space-y-6 px-8">
          <div className="flex items-center justify-center">
            <div className="flex aspect-square size-20 items-center justify-center bg-primary text-primary-foreground text-3xl font-bold shadow-lg">
              G
            </div>
          </div>
          <div className="space-y-2">
            <h1 className="text-4xl font-bold tracking-tight">Ganka28</h1>
            <p className="text-lg text-muted-foreground max-w-md">
              Ophthalmology Clinic Management System
            </p>
          </div>
          <div className="mt-8 text-sm text-muted-foreground/60">
            Dry Eye &middot; Myopia Control &middot; Clinical Imaging
          </div>
        </div>
      </div>

      {/* Right panel - login form */}
      <div className="flex flex-1 items-center justify-center p-6 md:p-8">
        <div className="w-full max-w-sm space-y-6">
          <div className="flex justify-end">
            <LanguageToggle />
          </div>

          {/* Mobile branding (shown only on small screens) */}
          <div className="flex items-center justify-center gap-3 lg:hidden">
            <div className="flex aspect-square size-10 items-center justify-center bg-primary text-primary-foreground text-lg font-bold">
              G
            </div>
            <span className="text-xl font-bold">Ganka28</span>
          </div>

          <Card>
            <CardHeader className="text-center">
              <CardTitle className="text-xl">{t("login.title")}</CardTitle>
              <CardDescription>{t("login.welcome")}</CardDescription>
            </CardHeader>
            <CardContent>
              <LoginForm redirectTo={redirectTo} />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
