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

/**
 * Maps camelCase refraction field keys to medical abbreviation labels.
 * These are universal medical abbreviations (same in English and Vietnamese),
 * so they do NOT need i18n translation.
 */
const REFRACTION_FIELD_LABELS: Record<string, string> = {
  odSph: "OD SPH",
  osSph: "OS SPH",
  odCyl: "OD CYL",
  osCyl: "OS CYL",
  odAxis: "OD AXIS",
  osAxis: "OS AXIS",
  odAdd: "OD ADD",
  osAdd: "OS ADD",
  odPd: "OD PD",
  osPd: "OS PD",
  ucvaOd: "OD UCVA",
  ucvaOs: "OS UCVA",
  bcvaOd: "OD BCVA",
  bcvaOs: "OS BCVA",
  iopOd: "OD IOP",
  iopOs: "OS IOP",
  axialLengthOd: "OD Axial Length",
  axialLengthOs: "OS Axial Length",
}

/**
 * Converts raw field keys from amendment diffs into localized display labels.
 *
 * Field key patterns:
 * - "refraction.manifest.odSph" -> "Manifest OD SPH" (en) / "Thường quy OD SPH" (vi)
 * - "refraction.manifest" -> "Manifest" / "Thường quy"
 * - "examinationNotes" -> "Examination Notes" / "Ghi chú khám"
 * - "diagnosis.added.H40.1" -> "Diagnosis (+) H40.1" / "Chẩn đoán (+) H40.1"
 * - "diagnosis.removed.H40.1" -> "Diagnosis (-) H40.1" / "Chẩn đoán (-) H40.1"
 */
function formatFieldLabel(
  fieldKey: string,
  t: (key: string) => string,
): string {
  // Refraction per-field: "refraction.manifest.odSph" -> "Manifest OD SPH"
  const refractionMatch = fieldKey.match(/^refraction\.(\w+)\.(\w+)$/)
  if (refractionMatch) {
    const [, type, field] = refractionMatch
    const typeLabel = t(`refraction.${type}`)
    const fieldLabel = REFRACTION_FIELD_LABELS[field] || field
    return `${typeLabel} ${fieldLabel}`
  }

  // Refraction type-level: "refraction.manifest" -> "Manifest"
  const typeMatch = fieldKey.match(/^refraction\.(\w+)$/)
  if (typeMatch) {
    return t(`refraction.${typeMatch[1]}`)
  }

  // Examination notes
  if (fieldKey === "examinationNotes") return t("visit.examinationNotes")

  // Diagnosis added: "diagnosis.added.H40.1" -> "Diagnosis (+) H40.1"
  if (fieldKey.startsWith("diagnosis.added.")) {
    const code = fieldKey.replace("diagnosis.added.", "")
    return `${t("visit.diagnosis")} (+) ${code}`
  }

  // Diagnosis removed: "diagnosis.removed.H40.1" -> "Diagnosis (-) H40.1"
  if (fieldKey.startsWith("diagnosis.removed.")) {
    const code = fieldKey.replace("diagnosis.removed.", "")
    return `${t("visit.diagnosis")} (-) ${code}`
  }

  // Fallback: show raw key
  return fieldKey
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
                          {formatFieldLabel(change.field, t)}
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
