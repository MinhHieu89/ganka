import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconPlus, IconPackage, IconChevronDown, IconChevronUp } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/shared/components/Collapsible"
import { Badge } from "@/shared/components/Badge"
import { LensCatalogTable } from "./LensCatalogTable"
import { LensFormDialog } from "./LensFormDialog"
import {
  useLensCatalog,
  useLowLensStockAlerts,
} from "@/features/optical/api/optical-queries"
import type { LensCatalogItemDto } from "@/features/optical/api/optical-api"

function LowLensStockAlert() {
  const { t } = useTranslation("optical")
  const { data: alerts, isLoading } = useLowLensStockAlerts()
  const [open, setOpen] = useState(false)

  if (isLoading) return null

  const count = alerts?.length ?? 0

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <div
        className={
          count > 0
            ? "rounded-lg p-3 bg-yellow-50 border border-yellow-300 dark:bg-yellow-950/30 dark:border-yellow-700"
            : "rounded-lg p-3 bg-muted/40 border border-border"
        }
      >
        <div className="flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 flex-1 min-w-0">
            <IconPackage
              className={`h-4 w-4 shrink-0 ${count > 0 ? "text-yellow-600 dark:text-yellow-400" : "text-muted-foreground"}`}
            />
            <span
              className={`text-sm font-medium ${count > 0 ? "text-yellow-700 dark:text-yellow-400" : "text-muted-foreground"}`}
            >
              {t("lenses.lowStockAlert")}
            </span>
            {count > 0 ? (
              <Badge
                variant="outline"
                className="text-xs shrink-0 border-yellow-500 text-yellow-700 dark:text-yellow-400"
              >
                {t("lenses.lowStockCount", { count })}
              </Badge>
            ) : (
              <span className="text-xs text-muted-foreground">{t("lenses.stockOk")}</span>
            )}
          </div>

          {count > 0 && (
            <CollapsibleTrigger asChild>
              <Button variant="ghost" size="sm" className="h-7 w-7 p-0 shrink-0">
                {open ? (
                  <IconChevronUp className="h-4 w-4" />
                ) : (
                  <IconChevronDown className="h-4 w-4" />
                )}
              </Button>
            </CollapsibleTrigger>
          )}
        </div>

        {count > 0 && (
          <CollapsibleContent>
            <div className="mt-3 space-y-1 max-h-48 overflow-y-auto">
              <div className="grid grid-cols-5 gap-2 text-xs font-medium text-muted-foreground pb-1 border-b">
                <span>{t("lenses.brand")} / {t("lenses.name")}</span>
                <span>{t("lenses.sph")}</span>
                <span>{t("lenses.cyl")}</span>
                <span>{t("lenses.quantity")}</span>
                <span>{t("lenses.minStock")}</span>
              </div>
              {alerts?.map((alert) => (
                <div
                  key={`${alert.lensCatalogItemId}-${alert.sph}-${alert.cyl}-${alert.add}`}
                  className="grid grid-cols-5 gap-2 text-xs py-0.5"
                >
                  <span className="font-medium truncate">
                    {alert.brand} {alert.lensName}
                  </span>
                  <span className="font-mono">
                    {alert.sph >= 0 ? "+" : ""}{alert.sph.toFixed(2)}
                  </span>
                  <span className="font-mono">
                    {alert.cyl >= 0 ? "+" : ""}{alert.cyl.toFixed(2)}
                  </span>
                  <span
                    className={`font-semibold ${alert.currentStock === 0 ? "text-destructive" : "text-yellow-600 dark:text-yellow-400"}`}
                  >
                    {alert.currentStock}
                  </span>
                  <span className="text-muted-foreground">{alert.minStockLevel}</span>
                </div>
              ))}
            </div>
          </CollapsibleContent>
        )}
      </div>
    </Collapsible>
  )
}

export function LensCatalogPage() {
  const { t } = useTranslation("optical")
  const { data: lenses, isLoading } = useLensCatalog()

  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<"create" | "edit" | "stock">("create")
  const [selectedLens, setSelectedLens] = useState<LensCatalogItemDto | undefined>()

  const openCreateDialog = () => {
    setDialogMode("create")
    setSelectedLens(undefined)
    setDialogOpen(true)
  }

  const openEditDialog = (lens: LensCatalogItemDto) => {
    setDialogMode("edit")
    setSelectedLens(lens)
    setDialogOpen(true)
  }

  const openStockDialog = (lens: LensCatalogItemDto) => {
    setDialogMode("stock")
    setSelectedLens(lens)
    setDialogOpen(true)
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("lenses.title")}</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {t("lenses.subtitle")}
          </p>
        </div>
        <Button onClick={openCreateDialog}>
          <IconPlus className="h-4 w-4 mr-2" />
          {t("lenses.addLens")}
        </Button>
      </div>

      <LowLensStockAlert />

      {isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <LensCatalogTable
          lenses={lenses ?? []}
          onEdit={openEditDialog}
          onAdjustStock={openStockDialog}
        />
      )}

      <LensFormDialog
        mode={dialogMode}
        lens={selectedLens}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
      />
    </div>
  )
}
