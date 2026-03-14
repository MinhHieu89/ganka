import { useRef, useState } from "react"
import { useTranslation } from "react-i18next"
import { IconUpload, IconCheck, IconX, IconLoader2 } from "@tabler/icons-react"
import { toast } from "sonner"
import { Button } from "@/shared/components/Button"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/shared/components/Dialog"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import { Badge } from "@/shared/components/Badge"
import {
  useImportDrugCatalogPreview,
  useConfirmDrugCatalogImport,
} from "@/features/pharmacy/api/pharmacy-queries"
import type {
  ValidDrugCatalogRow,
  DrugCatalogImportErrorDto,
} from "@/features/pharmacy/api/pharmacy-api"

interface DrugCatalogImportDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

interface PreviewRow {
  rowNumber: number
  name: string
  nameVi: string
  genericName: string
  form: string
  route: string
  strength: string | null
  unit: string
  sellingPrice: number
  minStockLevel: number
  isValid: boolean
  errors: DrugCatalogImportErrorDto[]
}

const COLUMNS = [
  "Name",
  "NameVi",
  "GenericName",
  "Form",
  "Route",
  "Strength",
  "Unit",
  "SellingPrice",
  "MinStockLevel",
] as const

export function DrugCatalogImportDialog({ open, onOpenChange }: DrugCatalogImportDialogProps) {
  const { t } = useTranslation("pharmacy")
  const { t: tCommon } = useTranslation("common")
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [previewRows, setPreviewRows] = useState<PreviewRow[]>([])
  const [validRows, setValidRows] = useState<ValidDrugCatalogRow[]>([])

  const previewMutation = useImportDrugCatalogPreview()
  const confirmMutation = useConfirmDrugCatalogImport()

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    try {
      const result = await previewMutation.mutateAsync(file)

      const rows: PreviewRow[] = []

      // Group errors by row number
      const errorsByRow = new Map<number, DrugCatalogImportErrorDto[]>()
      for (const err of result.errors) {
        const existing = errorsByRow.get(err.rowNumber) ?? []
        existing.push(err)
        errorsByRow.set(err.rowNumber, existing)
      }

      // Add error rows first (these have accurate row numbers from backend)
      for (const [rowNumber, errs] of errorsByRow) {
        rows.push({
          rowNumber,
          name: "",
          nameVi: "",
          genericName: "",
          form: "",
          route: "",
          strength: null,
          unit: "",
          sellingPrice: 0,
          minStockLevel: 0,
          isValid: false,
          errors: errs,
        })
      }

      // Valid rows: backend does not return original row numbers, so we
      // display a sequential index (1-based) rather than guessing Excel rows.
      // The # column for valid rows represents display order, not the
      // original Excel row number.
      for (let i = 0; i < result.validRows.length; i++) {
        const vr = result.validRows[i]
        rows.push({
          rowNumber: i + 1,
          name: vr.name,
          nameVi: vr.nameVi,
          genericName: vr.genericName,
          form: vr.form,
          route: vr.route,
          strength: vr.strength,
          unit: vr.unit,
          sellingPrice: vr.sellingPrice,
          minStockLevel: vr.minStockLevel,
          isValid: true,
          errors: [],
        })
      }

      // Sort: error rows first (by Excel row number), then valid rows
      rows.sort((a, b) => {
        if (a.isValid !== b.isValid) return a.isValid ? 1 : -1
        return a.rowNumber - b.rowNumber
      })
      setPreviewRows(rows)
      setValidRows(result.validRows)
    } catch {
      // Error handled by mutation onError
    }

    // Reset file input
    if (fileInputRef.current) {
      fileInputRef.current.value = ""
    }
  }

  const handleConfirmImport = async () => {
    if (validRows.length === 0) return

    try {
      await confirmMutation.mutateAsync(validRows)
      toast.success(t("catalog.importSuccess"))
      handleClose()
    } catch {
      // Error handled by mutation onError
    }
  }

  const handleClose = () => {
    setPreviewRows([])
    setValidRows([])
    previewMutation.reset()
    confirmMutation.reset()
    onOpenChange(false)
  }

  const validCount = previewRows.filter((r) => r.isValid).length
  const invalidCount = previewRows.filter((r) => !r.isValid).length

  const getCellError = (row: PreviewRow, columnName: string): string | undefined => {
    return row.errors.find((e) => e.columnName === columnName)?.message
  }

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-5xl max-h-[80vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>{t("catalog.importExcelTitle")}</DialogTitle>
          <DialogDescription>{t("catalog.importExcelDesc")}</DialogDescription>
        </DialogHeader>

        <div className="space-y-4 flex-1 overflow-hidden flex flex-col">
          {/* File upload */}
          <div className="flex items-center gap-3">
            <Button
              variant="outline"
              onClick={() => fileInputRef.current?.click()}
              disabled={previewMutation.isPending}
            >
              {previewMutation.isPending ? (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              ) : (
                <IconUpload className="h-4 w-4 mr-2" />
              )}
              {t("catalog.selectFile")}
            </Button>
            <input
              ref={fileInputRef}
              type="file"
              accept=".xlsx,.xls"
              className="hidden"
              onChange={handleFileSelect}
            />
            <span className="text-sm text-muted-foreground">
              .xlsx, .xls
            </span>
          </div>

          {/* Preview table */}
          {previewRows.length > 0 && (
            <>
              <div className="flex gap-3">
                <Badge variant="default" className="gap-1">
                  <IconCheck className="h-3 w-3" />
                  {validCount} {t("catalog.validRows")}
                </Badge>
                {invalidCount > 0 && (
                  <Badge variant="destructive" className="gap-1">
                    <IconX className="h-3 w-3" />
                    {invalidCount} {t("catalog.invalidRows")}
                  </Badge>
                )}
              </div>

              <div className="border rounded-lg overflow-auto flex-1">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead className="w-10">#</TableHead>
                      <TableHead className="w-10"></TableHead>
                      {COLUMNS.map((col) => (
                        <TableHead key={col}>{col}</TableHead>
                      ))}
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {previewRows.map((row, idx) => (
                      <TableRow
                        key={idx}
                        className={
                          row.isValid
                            ? "bg-green-50 dark:bg-green-950/20"
                            : "bg-red-50 dark:bg-red-950/20"
                        }
                      >
                        <TableCell className="text-xs text-muted-foreground" title={row.isValid ? `#${row.rowNumber}` : `Excel row ${row.rowNumber}`}>
                          {row.isValid ? `#${row.rowNumber}` : `R${row.rowNumber}`}
                        </TableCell>
                        <TableCell>
                          {row.isValid ? (
                            <IconCheck className="h-4 w-4 text-green-600" />
                          ) : (
                            <IconX className="h-4 w-4 text-red-600" />
                          )}
                        </TableCell>
                        <TableCell>
                          <CellWithError value={row.name} error={getCellError(row, "Name")} />
                        </TableCell>
                        <TableCell>
                          <CellWithError value={row.nameVi} error={getCellError(row, "NameVi")} />
                        </TableCell>
                        <TableCell>
                          <CellWithError value={row.genericName} error={getCellError(row, "GenericName")} />
                        </TableCell>
                        <TableCell>
                          <CellWithError value={row.form} error={getCellError(row, "Form")} />
                        </TableCell>
                        <TableCell>
                          <CellWithError value={row.route} error={getCellError(row, "Route")} />
                        </TableCell>
                        <TableCell>
                          <CellWithError value={row.strength ?? ""} error={getCellError(row, "Strength")} />
                        </TableCell>
                        <TableCell>
                          <CellWithError value={row.unit} error={getCellError(row, "Unit")} />
                        </TableCell>
                        <TableCell>
                          <CellWithError
                            value={row.isValid ? String(row.sellingPrice) : ""}
                            error={getCellError(row, "SellingPrice")}
                          />
                        </TableCell>
                        <TableCell>
                          <CellWithError
                            value={row.isValid ? String(row.minStockLevel) : ""}
                            error={getCellError(row, "MinStockLevel")}
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            </>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose}>
            {tCommon("buttons.cancel")}
          </Button>
          {validRows.length > 0 && (
            <Button
              onClick={handleConfirmImport}
              disabled={confirmMutation.isPending}
            >
              {confirmMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {t("catalog.importValidRows", { count: validCount })}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

function CellWithError({ value, error }: { value: string; error?: string }) {
  if (!error) {
    return <span className="text-sm">{value}</span>
  }

  return (
    <div>
      <span className="text-sm">{value || "-"}</span>
      <p className="text-xs text-red-600 mt-0.5">{error}</p>
    </div>
  )
}
