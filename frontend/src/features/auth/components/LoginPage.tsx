import { useTranslation } from "react-i18next"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/Card"
import { LanguageToggle } from "@/shared/components/LanguageToggle"
import { IconEye, IconDroplet, IconMicroscope } from "@tabler/icons-react"
import { LoginForm } from "./LoginForm"

interface LoginPageProps {
  redirectTo?: string
}

export function LoginPage({ redirectTo }: LoginPageProps) {
  const { t } = useTranslation("auth")

  return (
    <div className="grid min-h-svh lg:grid-cols-2">
      {/* Left column: form */}
      <div className="flex flex-col gap-4 p-6 md:p-10">
        <div className="flex justify-between items-center">
          <a href="/" className="flex items-center gap-2.5 group">
            <div className="relative flex aspect-square size-9 items-center justify-center bg-primary text-primary-foreground text-sm font-bold tracking-wider shadow-sm transition-shadow group-hover:shadow-md">
              <span className="relative z-10">G</span>
              <div className="absolute inset-0 bg-gradient-to-br from-white/10 to-transparent" />
            </div>
            <span className="font-semibold text-[15px] tracking-tight">Ganka28</span>
          </a>
          <LanguageToggle />
        </div>
        <div className="flex flex-1 items-center justify-center">
          <div className="w-full max-w-sm">
            {/* Mobile branding */}
            <div className="flex flex-col items-center gap-3 mb-8 lg:hidden">
              <div className="relative flex aspect-square size-12 items-center justify-center bg-primary text-primary-foreground text-xl font-bold shadow-sm">
                <span className="relative z-10">G</span>
                <div className="absolute inset-0 bg-gradient-to-br from-white/10 to-transparent" />
              </div>
              <div className="text-center">
                <h2 className="text-xl font-semibold tracking-tight">Ganka28</h2>
                <p className="text-xs text-muted-foreground/70 font-medium uppercase tracking-widest mt-0.5">Ophthalmology</p>
              </div>
            </div>

            <Card className="shadow-sm">
              <CardHeader className="text-center pb-4">
                <CardTitle className="text-xl tracking-tight">{t("login.title")}</CardTitle>
                <CardDescription className="text-muted-foreground/80">{t("login.welcome")}</CardDescription>
              </CardHeader>
              <CardContent>
                <LoginForm redirectTo={redirectTo} />
              </CardContent>
            </Card>
          </div>
        </div>
      </div>

      {/* Right column: distinctive branding panel */}
      <div className="relative hidden bg-primary lg:flex lg:flex-col lg:justify-between overflow-hidden">
        {/* Dot grid pattern overlay */}
        <div
          className="absolute inset-0 opacity-[0.07]"
          style={{
            backgroundImage: "radial-gradient(circle, rgba(255,255,255,0.8) 1px, transparent 1px)",
            backgroundSize: "24px 24px",
          }}
        />

        {/* Subtle gradient overlay */}
        <div className="absolute inset-0 bg-gradient-to-b from-black/5 via-transparent to-black/10" />

        {/* Content */}
        <div className="relative z-10 flex flex-1 flex-col justify-center px-12 xl:px-16">
          <div className="space-y-8">
            {/* Logo mark */}
            <div className="flex items-center gap-4">
              <div className="flex aspect-square size-14 items-center justify-center bg-white/15 text-white text-2xl font-bold backdrop-blur-sm border border-white/10">
                G
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-white">Ganka28</h1>
                <p className="text-sm text-white/50 font-medium uppercase tracking-[0.2em] mt-0.5">
                  Ophthalmology
                </p>
              </div>
            </div>

            {/* Divider */}
            <div className="h-px w-16 bg-white/20" />

            {/* Feature list */}
            <div className="space-y-5">
              <div className="flex items-start gap-4">
                <div className="flex size-10 shrink-0 items-center justify-center bg-white/10 text-white/80">
                  <IconDroplet className="h-5 w-5" />
                </div>
                <div>
                  <p className="text-sm font-medium text-white">Dry Eye Assessment</p>
                  <p className="text-sm text-white/50 mt-0.5">OSDI scoring & treatment tracking</p>
                </div>
              </div>
              <div className="flex items-start gap-4">
                <div className="flex size-10 shrink-0 items-center justify-center bg-white/10 text-white/80">
                  <IconEye className="h-5 w-5" />
                </div>
                <div>
                  <p className="text-sm font-medium text-white">Myopia Control</p>
                  <p className="text-sm text-white/50 mt-0.5">Axial length monitoring & protocols</p>
                </div>
              </div>
              <div className="flex items-start gap-4">
                <div className="flex size-10 shrink-0 items-center justify-center bg-white/10 text-white/80">
                  <IconMicroscope className="h-5 w-5" />
                </div>
                <div>
                  <p className="text-sm font-medium text-white">Clinical Imaging</p>
                  <p className="text-sm text-white/50 mt-0.5">Meibography, OCT & fluorescein</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Bottom attribution */}
        <div className="relative z-10 px-12 xl:px-16 pb-8">
          <p className="text-xs text-white/30 font-medium">
            &copy; 2026 Ganka28 &middot; Ho Chi Minh City
          </p>
        </div>
      </div>
    </div>
  )
}
