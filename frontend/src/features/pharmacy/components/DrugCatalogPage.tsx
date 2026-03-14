import { useState, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { IconPlus, IconFileSpreadsheet, IconDownload } from "@tabler/icons-react"
import { toast } from "sonner"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { DrugCatalogTable } from "./DrugCatalogTable"
import { DrugFormDialog } from "./DrugFormDialog"
import { DrugCatalogImportDialog } from "./DrugCatalogImportDialog"
import { type DrugCatalogItemDto, getDrugCatalogTemplate } from "@/features/pharmacy/api/pharmacy-api"
import { useSearchDrugCatalog } from "@/features/pharmacy/api/pharmacy-queries"
import { useDebounce } from "@/shared/hooks/useDebounce"
import { Input } from "@/shared/components/Input"
import type { PaginationState } from "@tanstack/react-table"

export function DrugCatalogPage() {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")

  const [searchInput, setSearchInput] = useState("")
  const debouncedSearch = useDebounce(searchInput, 300)

  const [pagination, setPagination] = useState<PaginationState>({
    pageIndex: 0,
    pageSize: 20,
  })

  // Reset to page 1 when search changes
  const handleSearchChange = useCallback((value: string) => {
    setSearchInput(value)
    setPagination((prev) => ({ ...prev, pageIndex: 0 }))
  }, [])

  const { data, isLoading } = useSearchDrugCatalog(
    pagination.pageIndex + 1,
    pagination.pageSize,
    debouncedSearch || undefined,
  )

  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<"create" | "edit">("create")
  const [editingDrug, setEditingDrug] = useState<DrugCatalogItemDto | undefined>()
  const [importDialogOpen, setImportDialogOpen] = useState(false)

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

  const handleDownloadTemplate = async () => {
    try {
      const blob = await getDrugCatalogTemplate()
      const url = URL.createObjectURL(blob)
      const a = document.createElement("a")
      a.href = url
      a.download = "drug-catalog-template.xlsx"
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
    } catch {
      toast.error("Failed to download template")
    }
  }

  const totalCount = data?.totalCount ?? 0
  const pageCount = data?.totalPages ?? 0

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("catalog.title")}</h1>
        <div className="flex gap-2">
          <Button variant="outline" onClick={handleDownloadTemplate}>
            <IconDownload className="h-4 w-4 mr-2" />
            {t("catalog.downloadTemplate")}
          </Button>
          <Button variant="outline" onClick={() => setImportDialogOpen(true)}>
            <IconFileSpreadsheet className="h-4 w-4 mr-2" />
            {t("catalog.importExcel")}
          </Button>
          <Button onClick={openCreateDialog}>
            <IconPlus className="h-4 w-4 mr-2" />
            {t("catalog.addDrug")}
          </Button>
        </div>
      </div>

      <Input
        value={searchInput}
        onChange={(e) => handleSearchChange(e.target.value)}
        placeholder={t("catalog.search")}
        className="max-w-sm"
      />

      {isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <>
          <DrugCatalogTable
            drugs={data?.items ?? []}
            onEdit={openEditDialog}
            pagination={pagination}
            onPaginationChange={setPagination}
            pageCount={pageCount}
          />

          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>
              {tCommon("table.page")} {pagination.pageIndex + 1} {tCommon("table.of")}{" "}
              {pageCount || 1} ({totalCount} {tCommon("table.total").toLowerCase()})
            </span>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={pagination.pageIndex === 0}
                onClick={() =>
                  setPagination((prev) => ({
                    ...prev,
                    pageIndex: Math.max(0, prev.pageIndex - 1),
                  }))
                }
              >
                {tCommon("buttons.previous")}
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={pagination.pageIndex >= pageCount - 1}
                onClick={() =>
                  setPagination((prev) => ({
                    ...prev,
                    pageIndex: prev.pageIndex + 1,
                  }))
                }
              >
                {tCommon("buttons.next")}
              </Button>
            </div>
          </div>
        </>
      )}

      <DrugFormDialog
        mode={dialogMode}
        drug={editingDrug}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
      />

      <DrugCatalogImportDialog
        open={importDialogOpen}
        onOpenChange={setImportDialogOpen}
      />
    </div>
  )
}
