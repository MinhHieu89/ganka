import { createFileRoute, Link } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { IconBuildingWarehouse, IconMedicineSyrup, IconTruck } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { ExpiryAlertBanner } from "@/features/pharmacy/components/ExpiryAlertBanner"
import { LowStockAlertBanner } from "@/features/pharmacy/components/LowStockAlertBanner"
import { DrugInventoryTable } from "@/features/pharmacy/components/DrugInventoryTable"
import { useDrugInventory } from "@/features/pharmacy/api/pharmacy-queries"

export const Route = createFileRoute("/_authenticated/pharmacy/")({
  component: PharmacyInventoryPage,
})

function PharmacyInventoryPage() {
  const { t } = useTranslation("pharmacy")
  const { data: drugs, isLoading } = useDrugInventory()

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("inventory.title")}</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {t("inventory.subtitle")}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" asChild>
            <Link to={"/pharmacy/drug-catalog" as string}>
              <IconMedicineSyrup className="h-4 w-4 mr-2" />
              {t("catalog.title")}
            </Link>
          </Button>
          <Button variant="outline" asChild>
            <Link to={"/pharmacy/suppliers" as string}>
              <IconTruck className="h-4 w-4 mr-2" />
              {t("supplier.manageLink")}
            </Link>
          </Button>
          <Button asChild>
            <Link to={"/pharmacy/stock-import" as string}>
              <IconBuildingWarehouse className="h-4 w-4 mr-2" />
              {t("stockImport.importLink")}
            </Link>
          </Button>
        </div>
      </div>

      {/* Alert banners */}
      <div className="space-y-2">
        <ExpiryAlertBanner />
        <LowStockAlertBanner />
      </div>

      {/* Inventory table */}
      {isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <DrugInventoryTable drugs={drugs ?? []} />
      )}
    </div>
  )
}
