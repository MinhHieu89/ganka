import { createFileRoute, Link } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { IconArrowLeft } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import {
  Breadcrumb,
  BreadcrumbList,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/shared/components/Breadcrumb"
import { InvoiceHistoryPage } from "@/features/billing/components/InvoiceHistoryPage"

export const Route = createFileRoute("/_authenticated/billing/invoices/")({
  component: InvoiceHistoryRoute,
})

function InvoiceHistoryRoute() {
  const { t } = useTranslation("billing")

  return (
    <div className="space-y-4">
      {/* Breadcrumb */}
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem>
            <BreadcrumbLink asChild>
              <Link to="/billing">{t("title")}</Link>
            </BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbPage>{t("allInvoices")}</BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

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
