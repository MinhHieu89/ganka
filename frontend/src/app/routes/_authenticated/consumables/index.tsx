import { useState } from "react"
import { createFileRoute } from "@tanstack/react-router"
import { requirePermission } from "@/shared/utils/permission-guard"
import { IconPlus } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { ConsumableAlertBanner } from "@/features/consumables/components/ConsumableAlertBanner"
import { ConsumableItemTable } from "@/features/consumables/components/ConsumableItemTable"
import { ConsumableItemForm } from "@/features/consumables/components/ConsumableItemForm"

export const Route = createFileRoute("/_authenticated/consumables/")({
  beforeLoad: () => requirePermission("Pharmacy.View"),
  component: ConsumablesPage,
})

function ConsumablesPage() {
  const [createDialogOpen, setCreateDialogOpen] = useState(false)

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Kho vật tư tiêu hao</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            Quản lý kho vật tư và theo dõi tồn kho
          </p>
        </div>
        <Button onClick={() => setCreateDialogOpen(true)}>
          <IconPlus className="h-4 w-4 mr-2" />
          Thêm vật tư
        </Button>
      </div>

      {/* Alert banner */}
      <ConsumableAlertBanner />

      {/* Inventory table */}
      <ConsumableItemTable />

      {/* Create dialog */}
      <ConsumableItemForm
        mode="create"
        open={createDialogOpen}
        onOpenChange={setCreateDialogOpen}
      />
    </div>
  )
}
