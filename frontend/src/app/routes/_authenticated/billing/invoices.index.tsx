import { createFileRoute, Link } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { IconArrowLeft } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { InvoiceHistoryPage } from "@/features/billing/components/InvoiceHistoryPage"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute("/_authenticated/billing/invoices/")({
  beforeLoad: () => requirePermission("Billing.View"),
  component: InvoiceHistoryRoute,
})

function InvoiceHistoryRoute() {
  const { t } = useTranslation("billing")

  return (
    <div className="space-y-4">
      {/* Back button */}
      <Button variant="ghost" size="sm" asChild>
        <Link to="/billing">
          <IconArrowLeft className="h-4 w-4 mr-1" />
          {t("dashboard")}
        </Link>
      </Button>

      {/* Page title */}
      <div>
        <h1 className="text-2xl font-bold">{t("invoiceHistory")}</h1>
      </div>

      {/* Invoice history content */}
      <InvoiceHistoryPage />
    </div>
  )
}
