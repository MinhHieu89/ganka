import { useState } from "react"
import {
  IconFileTypePdf,
  IconFileTypeXml,
  IconBraces,
  IconFileInvoice,
  IconLoader2,
} from "@tabler/icons-react"
import { toast } from "sonner"
import { Button } from "@/shared/components/Button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/shared/components/DropdownMenu"
import {
  getEInvoicePdf,
  exportEInvoiceJson,
  exportEInvoiceXml,
} from "@/features/billing/api/shift-api"

// -- Props --

interface EInvoiceExportButtonProps {
  invoiceId: string
  invoiceNumber?: string
}

export function EInvoiceExportButton({
  invoiceId,
  invoiceNumber,
}: EInvoiceExportButtonProps) {
  const [isExporting, setIsExporting] = useState(false)

  const handleExportPdf = async () => {
    setIsExporting(true)
    try {
      const blob = await getEInvoicePdf(invoiceId)
      const url = URL.createObjectURL(blob)
      window.open(url, "_blank")
      // Clean up blob URL after a short delay to allow the tab to load
      setTimeout(() => URL.revokeObjectURL(url), 5000)
    } catch {
      toast.error("Khong the xuat hoa don dien tu PDF")
    } finally {
      setIsExporting(false)
    }
  }

  const handleExportJson = async () => {
    setIsExporting(true)
    try {
      const blob = await exportEInvoiceJson(invoiceId)
      triggerDownload(
        blob,
        `e-invoice-${invoiceNumber || invoiceId}.json`,
      )
    } catch {
      toast.error("Khong the xuat hoa don dien tu JSON")
    } finally {
      setIsExporting(false)
    }
  }

  const handleExportXml = async () => {
    setIsExporting(true)
    try {
      const blob = await exportEInvoiceXml(invoiceId)
      triggerDownload(
        blob,
        `e-invoice-${invoiceNumber || invoiceId}.xml`,
      )
    } catch {
      toast.error("Khong the xuat hoa don dien tu XML")
    } finally {
      setIsExporting(false)
    }
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="outline" size="sm" disabled={isExporting}>
          {isExporting ? (
            <IconLoader2 className="mr-2 h-4 w-4 animate-spin" />
          ) : (
            <IconFileInvoice className="mr-2 h-4 w-4" />
          )}
          Xuat hoa don dien tu
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem onClick={handleExportPdf}>
          <IconFileTypePdf className="mr-2 h-4 w-4" />
          PDF
        </DropdownMenuItem>
        <DropdownMenuItem onClick={handleExportJson}>
          <IconBraces className="mr-2 h-4 w-4" />
          JSON (MISA)
        </DropdownMenuItem>
        <DropdownMenuItem onClick={handleExportXml}>
          <IconFileTypeXml className="mr-2 h-4 w-4" />
          XML (MISA)
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}

// -- Helper: trigger file download --

function triggerDownload(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob)
  const a = document.createElement("a")
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  // Clean up
  setTimeout(() => {
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
  }, 100)
}
