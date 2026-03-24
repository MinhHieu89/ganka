import { useTranslation } from "react-i18next"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Badge } from "@/shared/components/Badge"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import type { StockImportDto } from "@/features/pharmacy/api/pharmacy-api"

interface StockImportDetailDialogProps {
  importRecord: StockImportDto | null
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function StockImportDetailDialog({
  importRecord,
  open,
  onOpenChange,
}: StockImportDetailDialogProps) {
  const { t } = useTranslation("pharmacy")

  if (!importRecord) return null

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{t("stockImport.detailTitle")}</DialogTitle>
        </DialogHeader>

        {/* Summary info */}
        <div className="grid grid-cols-2 gap-x-6 gap-y-2 text-sm">
          <div>
            <span className="text-muted-foreground">{t("stockImport.historySupplier")}:</span>{" "}
            <span className="font-medium">{importRecord.supplierName}</span>
          </div>
          <div>
            <span className="text-muted-foreground">{t("stockImport.historyInvoice")}:</span>{" "}
            <span className="font-medium">{importRecord.invoiceNumber ?? "—"}</span>
          </div>
          <div>
            <span className="text-muted-foreground">{t("stockImport.historyDate")}:</span>{" "}
            <span className="font-medium">
              {new Date(importRecord.importedAt).toLocaleDateString("vi-VN")}
            </span>
          </div>
          <div>
            <span className="text-muted-foreground">{t("stockImport.historySource")}:</span>{" "}
            {importRecord.importSource === 0 ? (
              <Badge variant="outline">{t("stockImport.sourceInvoice")}</Badge>
            ) : (
              <Badge variant="secondary">{t("stockImport.sourceExcel")}</Badge>
            )}
          </div>
          {importRecord.notes && (
            <div className="col-span-2">
              <span className="text-muted-foreground">{t("stockImport.notes")}:</span>{" "}
              <span>{importRecord.notes}</span>
            </div>
          )}
        </div>

        {/* Line items table */}
        <div className="rounded-md border mt-2">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("stockImport.detailDrug")}</TableHead>
                <TableHead>{t("stockImport.detailBatch")}</TableHead>
                <TableHead>{t("stockImport.detailExpiry")}</TableHead>
                <TableHead className="text-right">{t("stockImport.detailQty")}</TableHead>
                <TableHead className="text-right">{t("stockImport.detailPrice")}</TableHead>
                <TableHead className="text-right">{t("stockImport.detailSubtotal")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {importRecord.lines.map((line) => (
                <TableRow key={line.drugCatalogItemId + line.batchNumber}>
                  <TableCell className="font-medium">{line.drugName}</TableCell>
                  <TableCell>{line.batchNumber}</TableCell>
                  <TableCell>
                    {new Date(line.expiryDate).toLocaleDateString("vi-VN")}
                  </TableCell>
                  <TableCell className="text-right">{line.quantity}</TableCell>
                  <TableCell className="text-right">
                    {line.purchasePrice.toLocaleString("vi-VN")} ₫
                  </TableCell>
                  <TableCell className="text-right font-medium">
                    {(line.quantity * line.purchasePrice).toLocaleString("vi-VN")} ₫
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        {/* Total */}
        <div className="flex justify-end text-sm font-semibold pt-1">
          {t("stockImport.detailTotal")}:{" "}
          {importRecord.totalAmount.toLocaleString("vi-VN")} ₫
        </div>
      </DialogContent>
    </Dialog>
  )
}
