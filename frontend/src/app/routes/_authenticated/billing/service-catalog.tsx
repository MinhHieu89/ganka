import { useState, useCallback } from "react"
import { createFileRoute } from "@tanstack/react-router"
import { requirePermission } from "@/shared/utils/permission-guard"
import { useTranslation } from "react-i18next"
import { IconPlus } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import {
  type ServiceCatalogItemDto,
  useServiceCatalogItems,
} from "@/features/billing/api/service-catalog-api"
import { ServiceCatalogTable } from "@/features/billing/components/ServiceCatalogTable"
import { ServiceCatalogFormDialog } from "@/features/billing/components/ServiceCatalogFormDialog"

export const Route = createFileRoute("/_authenticated/billing/service-catalog")({
  beforeLoad: () => requirePermission("Billing.Manage"),
  component: ServiceCatalogPage,
})

function ServiceCatalogPage() {
  const { t } = useTranslation("billing")
  const [includeInactive, setIncludeInactive] = useState(false)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editItem, setEditItem] = useState<ServiceCatalogItemDto | null>(null)

  const { data: items, isLoading } = useServiceCatalogItems(includeInactive)

  const handleEdit = useCallback((item: ServiceCatalogItemDto) => {
    setEditItem(item)
    setDialogOpen(true)
  }, [])

  const handleAdd = useCallback(() => {
    setEditItem(null)
    setDialogOpen(true)
  }, [])

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("serviceCatalog.title")}</h1>
        </div>
        <Button onClick={handleAdd}>
          <IconPlus className="mr-2 h-4 w-4" />
          {t("serviceCatalog.addService")}
        </Button>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      ) : (
        <ServiceCatalogTable
          items={items ?? []}
          includeInactive={includeInactive}
          onIncludeInactiveChange={setIncludeInactive}
          onEdit={handleEdit}
        />
      )}

      <ServiceCatalogFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        editItem={editItem}
      />
    </div>
  )
}
