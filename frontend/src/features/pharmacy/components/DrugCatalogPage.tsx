import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconPlus } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { DrugCatalogTable } from "./DrugCatalogTable"
import { DrugFormDialog } from "./DrugFormDialog"
import { type DrugCatalogItemDto } from "@/features/pharmacy/api/pharmacy-api"
import { useDrugCatalogList } from "@/features/pharmacy/api/pharmacy-queries"

export function DrugCatalogPage() {
  const { t } = useTranslation("pharmacy")
  const { data: drugs, isLoading } = useDrugCatalogList()

  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<"create" | "edit">("create")
  const [editingDrug, setEditingDrug] = useState<DrugCatalogItemDto | undefined>()

  const openCreateDialog = () => {
    setDialogMode("create")
    setEditingDrug(undefined)
    setDialogOpen(true)
  }

  const openEditDialog = (drug: DrugCatalogItemDto) => {
    setDialogMode("edit")
    setEditingDrug(drug)
    setDialogOpen(true)
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("catalog.title")}</h1>
        <Button onClick={openCreateDialog}>
          <IconPlus className="h-4 w-4 mr-2" />
          {t("catalog.addDrug")}
        </Button>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <DrugCatalogTable
          drugs={drugs ?? []}
          onEdit={openEditDialog}
        />
      )}

      <DrugFormDialog
        mode={dialogMode}
        drug={editingDrug}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
      />
    </div>
  )
}
