import { createFileRoute, Link } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { IconArrowLeft } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { InvoiceView } from "@/features/billing/components/InvoiceView"

export const Route = createFileRoute(
  "/_authenticated/billing/invoices/$invoiceId",
)({
  component: InvoiceDetailRoute,
})

function InvoiceDetailRoute() {
  const { invoiceId } = Route.useParams()
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

      {/* Invoice detail view */}
      <InvoiceView invoiceId={invoiceId} />
    </div>
  )
}
