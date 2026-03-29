import { createFileRoute, Link } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { Button } from "@/shared/components/Button"
import { IconStethoscope } from "@tabler/icons-react"

export const Route = createFileRoute("/_authenticated/technician/pre-exam")({
  component: PreExamStubPage,
})

function PreExamStubPage() {
  const { t } = useTranslation("technician")

  return (
    <div className="flex flex-col items-center justify-center gap-4 min-h-[60vh]">
      <IconStethoscope className="h-16 w-16 text-muted-foreground" />
      <h1 className="text-xl font-semibold text-muted-foreground">
        {t("stub.title")}
      </h1>
      <Button variant="outline" asChild>
        <Link to="/dashboard">{t("stub.backBtn")}</Link>
      </Button>
    </div>
  )
}
