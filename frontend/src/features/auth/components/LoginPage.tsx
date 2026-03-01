import { useTranslation } from "react-i18next"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/Card"
import { LanguageToggle } from "@/shared/components/LanguageToggle"
import { LoginForm } from "./LoginForm"

interface LoginPageProps {
  redirectTo?: string
}

export function LoginPage({ redirectTo }: LoginPageProps) {
  const { t } = useTranslation("auth")

  return (
    <div className="grid min-h-svh lg:grid-cols-2">
      {/* Left column: logo + form */}
      <div className="flex flex-col gap-4 p-6 md:p-10">
        <div className="flex justify-between items-center">
          {/* Ganka28 logo */}
          <a href="/" className="flex items-center gap-2 font-medium">
            <div className="flex aspect-square size-8 items-center justify-center bg-primary text-primary-foreground text-sm font-bold">
              G
            </div>
            <span className="font-semibold">Ganka28</span>
          </a>
          {/* Language toggle in top-right */}
          <LanguageToggle />
        </div>
        <div className="flex flex-1 items-center justify-center">
          <div className="w-full max-w-sm">
            {/* Mobile branding (shown only on small screens when right panel hidden) */}
            <div className="flex items-center justify-center gap-3 mb-6 lg:hidden">
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

      {/* Right column: branding image */}
      <div className="relative hidden bg-muted lg:block">
        <div className="absolute inset-0 flex items-center justify-center bg-gradient-to-br from-primary/10 via-primary/5 to-background">
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
      </div>
    </div>
  )
}
