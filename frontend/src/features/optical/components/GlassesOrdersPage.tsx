import { useState } from "react"
import { useNavigate } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { IconPlus } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { GlassesOrderTable } from "./GlassesOrderTable"
import { CreateGlassesOrderForm } from "./CreateGlassesOrderForm"
import { OverdueOrderAlert } from "./OverdueOrderAlert"
import { useGlassesOrders, useOverdueOrders } from "@/features/optical/api/optical-queries"

export function GlassesOrdersPage() {
  const { t } = useTranslation("optical")
  const navigate = useNavigate()
  const [statusFilter, setStatusFilter] = useState<number | undefined>(undefined)
  const [createDialogOpen, setCreateDialogOpen] = useState(false)

  const { data: ordersResult, isLoading: ordersLoading } = useGlassesOrders(
    statusFilter !== undefined ? { statusFilter } : {},
  )
  const { data: overdueOrders } = useOverdueOrders()

  const orders = ordersResult?.items ?? []
  const overdueCount = overdueOrders?.length ?? 0

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("orders.title")}</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {t("orders.subtitle")}
          </p>
        </div>
        <Button onClick={() => setCreateDialogOpen(true)}>
          <IconPlus className="h-4 w-4 mr-2" />
          {t("orders.addOrder")}
        </Button>
      </div>

      {/* Overdue alert banner */}
      {overdueCount > 0 && (
        <OverdueOrderAlert orderCount={overdueCount} />
      )}

      {/* Orders table */}
      {ordersLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <GlassesOrderTable
          orders={orders}
          statusFilter={statusFilter}
          onStatusFilterChange={setStatusFilter}
          onRowClick={(order) => navigate({ to: "/optical/orders/$orderId", params: { orderId: order.id } })}
        />
      )}

      {/* Create order dialog */}
      <CreateGlassesOrderForm
        open={createDialogOpen}
        onOpenChange={setCreateDialogOpen}
      />
    </div>
  )
}
