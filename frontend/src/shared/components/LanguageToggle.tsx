import { useTranslation } from "react-i18next"
import { Button } from "@/shared/components/ui/button"
import { IconLanguage } from "@tabler/icons-react"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip"

export function LanguageToggle() {
  const { i18n, t } = useTranslation("common")

  const currentLang = i18n.language
  const nextLang = currentLang === "vi" ? "en" : "vi"
  const displayLabel = currentLang === "vi" ? "EN" : "VI"

  const handleToggle = () => {
    i18n.changeLanguage(nextLang)
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
