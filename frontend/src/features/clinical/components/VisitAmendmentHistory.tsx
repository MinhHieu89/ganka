import { useTranslation } from "react-i18next"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import { Separator } from "@/shared/components/Separator"
import type { VisitAmendmentDto } from "../api/clinical-api"
import { VisitSection } from "./VisitSection"

interface FieldChange {
  field: string
  oldValue: string
  newValue: string
}

function parseFieldChanges(json: string): FieldChange[] {
  try {
    const parsed = JSON.parse(json)
    if (Array.isArray(parsed)) return parsed as FieldChange[]
    return []
  } catch {
    return []
  }
}

interface VisitAmendmentHistoryProps {
  amendments: VisitAmendmentDto[]
}

export function VisitAmendmentHistory({
  amendments,
}: VisitAmendmentHistoryProps) {
  const { t } = useTranslation("clinical")

  // Reverse chronological order
  const sorted = [...amendments].sort(
    (a, b) => new Date(b.amendedAt).getTime() - new Date(a.amendedAt).getTime(),
  )

  return (
    <VisitSection title={t("visit.amendments")} defaultOpen={false}>
      <div className="space-y-4">
        {sorted.map((amendment, index) => {
          const fieldChanges = parseFieldChanges(amendment.fieldChangesJson)
          const amendedAt = new Date(amendment.amendedAt).toLocaleString(
            undefined,
            {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
            },
          )

          return (
            <div key={amendment.id} className="space-y-2">
              {index > 0 && <Separator />}
              <div className="flex items-center justify-between text-sm">
                <span className="font-medium">
                  {t("visit.amendmentBy", { name: amendment.amendedByName })}
                </span>
                <span className="text-muted-foreground">{amendedAt}</span>
              </div>
              <p className="text-sm text-muted-foreground">
                {amendment.reason}
              </p>
              {fieldChanges.length > 0 && (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead className="w-[150px]">
                        {t("visit.amendmentField")}
                      </TableHead>
                      <TableHead>{t("visit.amendmentOld")}</TableHead>
                      <TableHead>{t("visit.amendmentNew")}</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {fieldChanges.map((change, i) => (
                      <TableRow key={i}>
                        <TableCell className="font-medium">
                          {change.field}
                        </TableCell>
                        <TableCell className="text-muted-foreground">
                          {change.oldValue || "-"}
                        </TableCell>
                        <TableCell>{change.newValue || "-"}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </div>
          )
        })}
      </div>
    </VisitSection>
  )
}
