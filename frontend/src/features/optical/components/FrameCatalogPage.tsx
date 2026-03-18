import { useState, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { IconPlus } from "@tabler/icons-react"
import { toast } from "sonner"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { BarcodeScannerInput } from "./BarcodeScannerInput"
import { FrameCatalogTable } from "./FrameCatalogTable"
import { FrameFormDialog } from "./FrameFormDialog"
import { type FrameDto } from "@/features/optical/api/optical-api"
import {
  useFrames,
  useSearchFrames,
  useGenerateBarcode,
} from "@/features/optical/api/optical-queries"

export function FrameCatalogPage() {
  const { t } = useTranslation("optical")
  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<"create" | "edit">("create")
  const [editingFrame, setEditingFrame] = useState<FrameDto | undefined>()
  const [searchParams, setSearchParams] = useState<{
    searchTerm?: string
    material?: number
    frameType?: number
    gender?: number
  }>({})

  const hasFilters = !!(searchParams.searchTerm && searchParams.searchTerm.length >= 2) ||
    searchParams.material != null ||
    searchParams.frameType != null ||
    searchParams.gender != null

  const { data: allFramesResult, isLoading: isLoadingAll } = useFrames({ pageSize: 100 })
  const { data: searchResult, isLoading: isLoadingSearch } = useSearchFrames({
    ...searchParams,
    pageSize: 100,
  })

  const isLoading = hasFilters ? isLoadingSearch : isLoadingAll
  const frames = hasFilters
    ? (searchResult?.items ?? [])
    : (allFramesResult?.items ?? [])

  const generateBarcodeMutation = useGenerateBarcode()

  const openCreateDialog = () => {
    setDialogMode("create")
    setEditingFrame(undefined)
    setDialogOpen(true)
  }

  const openEditDialog = (frame: FrameDto) => {
    setDialogMode("edit")
    setEditingFrame(frame)
    setDialogOpen(true)
  }

  const handleGenerateBarcode = useCallback(
    async (frameId: string) => {
      try {
        const result = await generateBarcodeMutation.mutateAsync(frameId)
        toast.success(`${t("frames.barcodeScanned")}: ${result.barcode}`)
      } catch {
        // Error handled by mutation's onError toast
      }
    },
    [generateBarcodeMutation],
  )

  const handleBarcodeScan = useCallback(
    (barcode: string) => {
      setSearchParams({ searchTerm: barcode })
    },
    [],
  )

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("frames.title")}</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {t("frames.subtitle")}
          </p>
        </div>
        <Button onClick={openCreateDialog}>
          <IconPlus className="h-4 w-4 mr-2" />
          {t("frames.addFrame")}
        </Button>
      </div>

      {/* Barcode scanner input for quick lookup */}
      <div className="max-w-sm">
        <BarcodeScannerInput
          onScan={handleBarcodeScan}
          autoFocus={false}
        />
        <p className="text-xs text-muted-foreground mt-1">
          {t("frames.scanBarcode")}
        </p>
      </div>

      {/* Frame table */}
      {isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <FrameCatalogTable
          frames={frames}
          onEdit={openEditDialog}
          onGenerateBarcode={handleGenerateBarcode}
          isGeneratingBarcode={generateBarcodeMutation.isPending}
        />
      )}

      {/* Create/Edit dialog */}
      <FrameFormDialog
        mode={dialogMode}
        frame={editingFrame}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
      />
    </div>
  )
}
