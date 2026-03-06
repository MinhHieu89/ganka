import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import { formatVND } from "@/shared/lib/format-vnd"
import type { InvoiceLineItemDto } from "@/features/billing/api/billing-api"

interface InvoiceLineItemsTableProps {
  lineItems: InvoiceLineItemDto[]
}

interface DepartmentGroup {
  department: number
  labelKey: string
  items: InvoiceLineItemDto[]
  subtotal: number
}

const DEPARTMENT_ORDER = [0, 1, 2, 3] as const
const DEPARTMENT_LABEL_KEYS: Record<number, string> = {
  0: "departments.medical",
  1: "departments.pharmacy",
  2: "departments.optical",
  3: "departments.treatment",
}

export function InvoiceLineItemsTable({ lineItems }: InvoiceLineItemsTableProps) {
  const { t } = useTranslation("billing")

  const groups = useMemo<DepartmentGroup[]>(() => {
    const grouped = new Map<number, InvoiceLineItemDto[]>()

    for (const item of lineItems) {
      const existing = grouped.get(item.department) ?? []
      existing.push(item)
      grouped.set(item.department, existing)
    }

    return DEPARTMENT_ORDER
      .filter((dept) => grouped.has(dept))
      .map((dept) => ({
        department: dept,
        labelKey: DEPARTMENT_LABEL_KEYS[dept] ?? `Department ${dept}`,
        items: grouped.get(dept)!,
        subtotal: grouped.get(dept)!.reduce((sum, item) => sum + item.lineTotal, 0),
      }))
  }, [lineItems])

  const grandTotal = useMemo(
    () => lineItems.reduce((sum, item) => sum + item.lineTotal, 0),
    [lineItems],
  )

  if (lineItems.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        {t("noPendingInvoices")}
      </div>
    )
  }

  let runningIndex = 0

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead className="w-12">{t("lineItems.index")}</TableHead>
          <TableHead>{t("lineItems.description")}</TableHead>
          <TableHead className="w-16 text-right">{t("lineItems.quantity")}</TableHead>
          <TableHead className="w-32 text-right">{t("lineItems.unitPrice")}</TableHead>
          <TableHead className="w-36 text-right">{t("lineItems.total")}</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {groups.map((group) => (
          <DepartmentSection
            key={group.department}
            group={group}
            t={t}
            startIndex={(() => {
              const idx = runningIndex
              runningIndex += group.items.length
              return idx
            })()}
          />
        ))}
      </TableBody>
      <TableFooter>
        <TableRow className="font-bold">
          <TableCell colSpan={4} className="text-right">
            {t("lineItems.grandTotal")}
          </TableCell>
          <TableCell className="text-right">{formatVND(grandTotal)}</TableCell>
        </TableRow>
      </TableFooter>
    </Table>
  )
}

function DepartmentSection({
  group,
  t,
  startIndex,
}: {
  group: DepartmentGroup
  t: (key: string) => string
  startIndex: number
}) {
  return (
    <>
      {/* Department header row */}
      <TableRow className="bg-muted/50">
        <TableCell colSpan={5} className="font-semibold text-sm uppercase tracking-wide">
          {t(group.labelKey)}
        </TableCell>
      </TableRow>

      {/* Line items */}
      {group.items.map((item, idx) => (
        <TableRow key={item.id}>
          <TableCell className="text-muted-foreground">{startIndex + idx + 1}</TableCell>
          <TableCell>{item.descriptionVi ?? item.description}</TableCell>
          <TableCell className="text-right">{item.quantity}</TableCell>
          <TableCell className="text-right">{formatVND(item.unitPrice)}</TableCell>
          <TableCell className="text-right">{formatVND(item.lineTotal)}</TableCell>
        </TableRow>
      ))}

      {/* Section subtotal */}
      <TableRow className="border-b-2">
        <TableCell colSpan={4} className="text-right font-medium text-sm">
          {t("lineItems.sectionTotal")}
        </TableCell>
        <TableCell className="text-right font-medium">{formatVND(group.subtotal)}</TableCell>
      </TableRow>
    </>
  )
}
