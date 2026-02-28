import { useTranslation } from "react-i18next"
import { Button } from "@/shared/components/ui/button"
import { IconLanguage } from "@tabler/icons-react"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip"
import { useUpdateLanguageMutation } from "@/features/auth/api/auth-api"
import { useAuthStore } from "@/shared/stores/authStore"

export function LanguageToggle() {
  const { i18n, t } = useTranslation("common")
  const updateLanguage = useUpdateLanguageMutation()
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  const currentLang = i18n.language
  const nextLang = currentLang === "vi" ? "en" : "vi"
  const displayLabel = currentLang === "vi" ? "EN" : "VI"

  const handleToggle = () => {
    // Instant UI update
    i18n.changeLanguage(nextLang)

    // Persist to backend if authenticated (fire-and-forget)
    if (isAuthenticated) {
      updateLanguage.mutate({ language: nextLang })
    }
  }

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <Button
          variant="ghost"
          size="sm"
          onClick={handleToggle}
          className="gap-1.5"
        >
          <IconLanguage className="h-4 w-4" />
          <span className="text-xs font-medium">{displayLabel}</span>
        </Button>
      </TooltipTrigger>
      <TooltipContent>{t("topbar.language")}</TooltipContent>
    </Tooltip>
  )
}
