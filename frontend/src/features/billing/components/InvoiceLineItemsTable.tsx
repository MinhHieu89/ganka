import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { IconTrash, IconLoader2, IconLock } from "@tabler/icons-react"
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/components/AlertDialog"
import { Button } from "@/shared/components/Button"
import { formatVND } from "@/shared/lib/format-vnd"
import type { InvoiceLineItemDto } from "@/features/billing/api/billing-api"
import { useRemoveLineItem } from "@/features/billing/api/billing-api"

interface InvoiceLineItemsTableProps {
  lineItems: InvoiceLineItemDto[]
  invoiceId: string
  isDraft: boolean
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

export function InvoiceLineItemsTable({ lineItems, invoiceId, isDraft }: InvoiceLineItemsTableProps) {
  const { t } = useTranslation("billing")
  const removeLineItem = useRemoveLineItem()

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
          {isDraft && <TableHead className="w-12"></TableHead>}
        </TableRow>
      </TableHeader>
      <TableBody>
        {groups.map((group) => (
          <DepartmentSection
            key={group.department}
            group={group}
            t={t}
            isDraft={isDraft}
            invoiceId={invoiceId}
            removeLineItem={removeLineItem}
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
          <TableCell colSpan={isDraft ? 5 : 4} className="text-right">
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
  isDraft,
  invoiceId,
  removeLineItem,
  startIndex,
}: {
  group: DepartmentGroup
  t: (key: string, options?: Record<string, unknown>) => string
  isDraft: boolean
  invoiceId: string
  removeLineItem: ReturnType<typeof useRemoveLineItem>
  startIndex: number
}) {
  return (
    <>
      {/* Department header row */}
      <TableRow className="bg-muted/50">
        <TableCell colSpan={isDraft ? 6 : 5} className="font-semibold text-sm uppercase tracking-wide">
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
          {isDraft && (
            <TableCell>
              {item.sourceType === "Prescription" ? (
                <IconLock className="h-4 w-4 text-muted-foreground" title={t("lineItems.prescriptionLocked")} />
              ) : (
                <AlertDialog>
                  <AlertDialogTrigger asChild>
                    <Button variant="ghost" size="icon" className="h-8 w-8 text-muted-foreground hover:text-destructive">
                      <IconTrash className="h-4 w-4" />
                    </Button>
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>{t("lineItems.removeTitle")}</AlertDialogTitle>
                      <AlertDialogDescription>
                        {t("lineItems.removeConfirm", { name: item.descriptionVi ?? item.description })}
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>{t("buttons.cancel", { ns: "common" })}</AlertDialogCancel>
                      <AlertDialogAction
                        onClick={() => removeLineItem.mutate({ invoiceId, lineItemId: item.id })}
                        className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                      >
                        {removeLineItem.isPending ? (
                          <IconLoader2 className="h-4 w-4 animate-spin mr-2" />
                        ) : (
                          <IconTrash className="h-4 w-4 mr-2" />
                        )}
                        {t("lineItems.remove")}
                      </AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              )}
            </TableCell>
          )}
        </TableRow>
      ))}

      {/* Section subtotal */}
      <TableRow className="border-b-2">
        <TableCell colSpan={isDraft ? 5 : 4} className="text-right font-medium text-sm">
          {t("lineItems.sectionTotal")}
        </TableCell>
        <TableCell className="text-right font-medium">{formatVND(group.subtotal)}</TableCell>
      </TableRow>
    </>
  )
}
