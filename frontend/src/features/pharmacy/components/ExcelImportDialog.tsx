import { useState, useRef, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconUpload, IconDownload, IconLoader2, IconCircleCheck, IconCircleX } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Field, FieldLabel } from "@/shared/components/Field"
import { Badge } from "@/shared/components/Badge"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import type { ExcelImportPreviewDto, StockImportLineDto } from "@/features/pharmacy/api/pharmacy-api"
import {
  useSuppliers,
  useCreateStockImport,
} from "@/features/pharmacy/api/pharmacy-queries"
import { importStockFromExcel } from "@/features/pharmacy/api/pharmacy-api"

// Excel template headers matching the expected format
const TEMPLATE_HEADERS = [
  "DrugName",
  "BatchNumber",
  "ExpiryDate",
  "Quantity",
  "PurchasePrice",
]

function generateExcelTemplate(): Blob {
  const header = TEMPLATE_HEADERS.join(",")
  const example = "Thuốc Nhỏ Mắt Rohto,LO001,2026-12-31,100,15000"
  const content = `${header}\n${example}\n`
  return new Blob([content], { type: "text/csv;charset=utf-8;" })
}

interface ExcelImportDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess?: () => void
}

export function ExcelImportDialog({
  open,
  onOpenChange,
  onSuccess,
}: ExcelImportDialogProps) {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")
  const { data: suppliers } = useSuppliers()
  const createImport = useCreateStockImport()

  const [supplierId, setSupplierId] = useState("")
  const [file, setFile] = useState<File | null>(null)
  const [preview, setPreview] = useState<ExcelImportPreviewDto | null>(null)
  const [isUploading, setIsUploading] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const handleClose = useCallback(() => {
    setSupplierId("")
    setFile(null)
    setPreview(null)
    setIsUploading(false)
    onOpenChange(false)
  }, [onOpenChange])

  const handleDownloadTemplate = useCallback(() => {
    const blob = generateExcelTemplate()
    const url = URL.createObjectURL(blob)
    const a = document.createElement("a")
    a.href = url
    a.download = "stock-import-template.csv"
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    setTimeout(() => URL.revokeObjectURL(url), 30_000)
  }, [])

  const handleFileChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const selectedFile = e.target.files?.[0]
      if (!selectedFile) return

      setFile(selectedFile)
      setPreview(null)
    },
    [],
  )

  const handleUpload = useCallback(async () => {
    if (!file) {
      toast.error(t("stockImport.excelNoFile"))
      return
    }
    if (!supplierId) {
      toast.error(t("stockImport.excelNoSupplier"))
      return
    }

    setIsUploading(true)
    try {
      const result = await importStockFromExcel(file, supplierId)
      setPreview(result)
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : "Failed to process Excel file",
      )
    } finally {
      setIsUploading(false)
    }
  }, [file, supplierId, t])

  const handleConfirmImport = useCallback(async () => {
    if (!preview || preview.validLines.length === 0) return
    if (!supplierId) {
      toast.error(t("stockImport.excelNoSupplier"))
      return
    }

    try {
      await createImport.mutateAsync({
        supplierId,
        invoiceNumber: null,
        importDate: new Date().toISOString().slice(0, 10),
        lines: preview.validLines,
      })
      toast.success(t("stockImport.excelSuccess"))
      handleClose()
      onSuccess?.()
    } catch {
      // onError in mutation handles toast
    }
  }, [preview, supplierId, createImport, t, handleClose, onSuccess])

  const hasValidLines = (preview?.validLines.length ?? 0) > 0
  const hasErrors = (preview?.errors.length ?? 0) > 0

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-3xl">
        <DialogHeader>
          <DialogTitle>{t("stockImport.excelTitle")}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Supplier selector */}
          <Field>
            <FieldLabel>{t("stockImport.excelSupplier")}</FieldLabel>
            <Select value={supplierId} onValueChange={setSupplierId}>
              <SelectTrigger className="w-full sm:w-72">
                <SelectValue placeholder={t("stockImport.selectSupplier")} />
              </SelectTrigger>
              <SelectContent>
                {(suppliers ?? [])
                  .filter((s) => s.isActive)
                  .map((supplier) => (
                    <SelectItem key={supplier.id} value={supplier.id}>
                      {supplier.name}
                    </SelectItem>
                  ))}
              </SelectContent>
            </Select>
          </Field>

          {/* Template download + file upload */}
          <div className="flex flex-wrap items-center gap-3">
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={handleDownloadTemplate}
            >
              <IconDownload className="h-4 w-4 mr-2" />
              {t("stockImport.excelDownload")}
            </Button>

            <div className="flex items-center gap-2">
              <input
                ref={fileInputRef}
                type="file"
                accept=".xlsx,.xls,.csv"
                className="hidden"
                onChange={handleFileChange}
              />
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => fileInputRef.current?.click()}
              >
                <IconUpload className="h-4 w-4 mr-2" />
                {t("stockImport.excelFile")}
              </Button>
              {file && (
                <span className="text-sm text-muted-foreground truncate max-w-48">
                  {file.name}
                </span>
              )}
            </div>

            {file && (
              <Button
                type="button"
                size="sm"
                onClick={handleUpload}
                disabled={isUploading || !supplierId}
              >
                {isUploading ? (
                  <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
                ) : null}
                {t("stockImport.excelPreview")}
              </Button>
            )}
          </div>

          {/* Preview table */}
          {isUploading && (
            <div className="flex items-center justify-center py-8">
              <IconLoader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          )}

          {preview && !isUploading && (
            <div className="space-y-3">
              {/* Summary badges */}
              <div className="flex items-center gap-2">
                <Badge variant="outline" className="border-green-500 text-green-700 dark:text-green-400">
                  <IconCircleCheck className="h-3.5 w-3.5 mr-1" />
                  {t("stockImport.excelValidRows")}: {preview.validLines.length}
                </Badge>
                {hasErrors && (
                  <Badge variant="destructive">
                    <IconCircleX className="h-3.5 w-3.5 mr-1" />
                    {t("stockImport.excelErrors")}: {preview.errors.length}
                  </Badge>
                )}
              </div>

              {/* Error list */}
              {hasErrors && (
                <div className="rounded-md border border-destructive/40 bg-destructive/5 p-3 space-y-1">
                  {preview.errors.map((err, i) => (
                    <p key={i} className="text-xs text-destructive">
                      Row {err.rowNumber}, {err.columnName}: {err.message}
                    </p>
                  ))}
                </div>
              )}

              {/* Valid rows table */}
              {hasValidLines && (
                <div className="rounded-md border overflow-auto max-h-64">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead className="text-xs">{t("stockImport.drug")}</TableHead>
                        <TableHead className="text-xs">{t("stockImport.batchNumber")}</TableHead>
                        <TableHead className="text-xs">{t("stockImport.expiryDate")}</TableHead>
                        <TableHead className="text-xs text-right">{t("stockImport.quantity")}</TableHead>
                        <TableHead className="text-xs text-right">{t("stockImport.purchasePrice")}</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {preview.validLines.map((line: StockImportLineDto, i: number) => (
                        <TableRow key={i} className="bg-green-50/50 dark:bg-green-950/20">
                          <TableCell className="text-xs">{line.drugName}</TableCell>
                          <TableCell className="text-xs">{line.batchNumber}</TableCell>
                          <TableCell className="text-xs">{line.expiryDate}</TableCell>
                          <TableCell className="text-xs text-right">{line.quantity}</TableCell>
                          <TableCell className="text-xs text-right">
                            {line.purchasePrice.toLocaleString("vi-VN")} ₫
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}
            </div>
          )}
        </div>

        <DialogFooter>
          <Button type="button" variant="outline" onClick={handleClose}>
            {tCommon("buttons.cancel")}
          </Button>
          {preview && hasValidLines && (
            <Button
              type="button"
              onClick={handleConfirmImport}
              disabled={createImport.isPending}
            >
              {createImport.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {createImport.isPending
                ? t("stockImport.excelConfirming")
                : `${t("stockImport.excelConfirm")} (${preview.validLines.length})`}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
