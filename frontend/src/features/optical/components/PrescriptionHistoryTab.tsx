import { useState } from "react"
import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { IconEye, IconGitCompare } from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Checkbox } from "@/shared/components/Checkbox"
import { Skeleton } from "@/shared/components/Skeleton"
import { usePatientPrescriptionHistory } from "@/features/optical/api/optical-queries"
import { PrescriptionComparisonView } from "./PrescriptionComparisonView"
import type { OpticalPrescriptionHistoryDto } from "@/features/optical/api/optical-api"

interface PrescriptionHistoryTabProps {
  patientId: string
}

function formatOpticalValue(
  sph: number | null,
  cyl: number | null,
  axis: number | null,
): string {
  if (sph == null && cyl == null) return "—"
  const parts: string[] = []
  if (sph != null) {
    parts.push(`${sph >= 0 ? "+" : ""}${sph.toFixed(2)}`)
  }
  if (cyl != null) {
    parts.push(`${cyl >= 0 ? "+" : ""}${cyl.toFixed(2)}`)
    if (axis != null) parts.push(`x ${axis}°`)
  }
  return parts.join(" / ")
}

function formatAdd(add: number | null): string {
  if (add == null) return "—"
  return `${add >= 0 ? "+" : ""}${add.toFixed(2)}`
}

function formatPd(pd: number | null): string {
  if (pd == null) return "—"
  return `${pd.toFixed(1)} mm`
}

interface PrescriptionCardProps {
  prescription: OpticalPrescriptionHistoryDto
  index: number
  isSelected: boolean
  selectionDisabled: boolean
  onSelectionChange: (id: string, checked: boolean) => void
}

function PrescriptionCard({
  prescription,
  index,
  isSelected,
  selectionDisabled,
  onSelectionChange,
}: PrescriptionCardProps) {
  const { t } = useTranslation("optical")

  return (
    <Card className={`transition-all ${isSelected ? "ring-2 ring-primary" : ""}`}>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Checkbox
              checked={isSelected}
              disabled={!isSelected && selectionDisabled}
              onCheckedChange={(checked) =>
                onSelectionChange(prescription.id, !!checked)
              }
              id={`prescription-${prescription.id}`}
            />
            <div>
              <CardTitle className="text-sm font-semibold">
                {format(new Date(prescription.visitDate), "PPP")}
              </CardTitle>
              {index === 0 && (
                <Badge variant="secondary" className="mt-1 text-xs">
                  {t("prescriptions.currentPrescription")}
                </Badge>
              )}
            </div>
          </div>
          <IconEye className="h-4 w-4 text-muted-foreground" />
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-4 text-sm">
          {/* Right Eye (OD) */}
          <div>
            <p className="mb-1 font-medium text-muted-foreground">
              {t("prescriptions.rightEye")}
            </p>
            <dl className="space-y-0.5">
              <div className="flex justify-between gap-4">
                <dt className="text-muted-foreground">{t("prescriptions.sph")}</dt>
                <dd className="font-mono text-right">
                  {prescription.sphOd != null
                    ? `${prescription.sphOd >= 0 ? "+" : ""}${prescription.sphOd.toFixed(2)}`
                    : "—"}
                </dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-muted-foreground">{t("prescriptions.cyl")}</dt>
                <dd className="font-mono text-right">
                  {prescription.cylOd != null
                    ? `${prescription.cylOd >= 0 ? "+" : ""}${prescription.cylOd.toFixed(2)}`
                    : "—"}
                </dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-muted-foreground">{t("prescriptions.axis")}</dt>
                <dd className="font-mono text-right">
                  {prescription.axisOd != null ? `${prescription.axisOd}°` : "—"}
                </dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-muted-foreground">{t("prescriptions.add")}</dt>
                <dd className="font-mono text-right">{formatAdd(prescription.addOd)}</dd>
              </div>
            </dl>
            <p className="mt-1.5 font-mono text-xs text-muted-foreground">
              {formatOpticalValue(prescription.sphOd, prescription.cylOd, prescription.axisOd)}
            </p>
          </div>

          {/* Left Eye (OS) */}
          <div>
            <p className="mb-1 font-medium text-muted-foreground">
              {t("prescriptions.leftEye")}
            </p>
            <dl className="space-y-0.5">
              <div className="flex justify-between gap-4">
                <dt className="text-muted-foreground">{t("prescriptions.sph")}</dt>
                <dd className="font-mono text-right">
                  {prescription.sphOs != null
                    ? `${prescription.sphOs >= 0 ? "+" : ""}${prescription.sphOs.toFixed(2)}`
                    : "—"}
                </dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-muted-foreground">{t("prescriptions.cyl")}</dt>
                <dd className="font-mono text-right">
                  {prescription.cylOs != null
                    ? `${prescription.cylOs >= 0 ? "+" : ""}${prescription.cylOs.toFixed(2)}`
                    : "—"}
                </dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-muted-foreground">{t("prescriptions.axis")}</dt>
                <dd className="font-mono text-right">
                  {prescription.axisOs != null ? `${prescription.axisOs}°` : "—"}
                </dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-muted-foreground">{t("prescriptions.add")}</dt>
                <dd className="font-mono text-right">{formatAdd(prescription.addOs)}</dd>
              </div>
            </dl>
            <p className="mt-1.5 font-mono text-xs text-muted-foreground">
              {formatOpticalValue(prescription.sphOs, prescription.cylOs, prescription.axisOs)}
            </p>
          </div>
        </div>

        {/* PD and Notes row */}
        <div className="mt-3 flex flex-wrap gap-x-6 gap-y-1 border-t pt-3 text-sm">
          <div className="flex items-center gap-2">
            <span className="text-muted-foreground">{t("prescriptions.pd")}</span>
            <span className="font-mono">{formatPd(prescription.pd)}</span>
          </div>
          {prescription.notes && (
            <div className="flex items-start gap-2">
              <span className="shrink-0 text-muted-foreground">{t("common.notes")}</span>
              <span className="text-foreground">{prescription.notes}</span>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  )
}

export function PrescriptionHistoryTab({ patientId }: PrescriptionHistoryTabProps) {
  const { t } = useTranslation("optical")
  const { data: prescriptions, isLoading } = usePatientPrescriptionHistory(patientId)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const handleSelectionChange = (id: string, checked: boolean) => {
    if (checked) {
      if (selectedIds.length < 2) {
        setSelectedIds((prev) => [...prev, id])
      }
    } else {
      setSelectedIds((prev) => prev.filter((s) => s !== id))
    }
  }

  const handleClearComparison = () => {
    setSelectedIds([])
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-48 w-full" />
        <Skeleton className="h-48 w-full" />
      </div>
    )
  }

  if (!prescriptions || prescriptions.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <IconEye className="mb-3 h-10 w-10 text-muted-foreground/40" />
        <p className="text-muted-foreground">{t("prescriptions.empty")}</p>
      </div>
    )
  }

  const canCompare = prescriptions.length >= 2
  const isComparing = selectedIds.length === 2

  return (
    <div className="space-y-4">
      {/* Compare hint */}
      {canCompare && (
        <div className="flex items-center justify-between rounded-md border bg-muted/40 px-4 py-2 text-sm">
          <div className="flex items-center gap-2 text-muted-foreground">
            <IconGitCompare className="h-4 w-4" />
            <span>
              {isComparing
                ? t("prescriptions.yearOverYear")
                : t("prescriptions.comparison")}
            </span>
          </div>
          {selectedIds.length > 0 && (
            <Button variant="ghost" size="sm" onClick={handleClearComparison}>
              {t("common.close")}
            </Button>
          )}
        </div>
      )}

      {/* Timeline */}
      <div className="space-y-3">
        {prescriptions.map((prescription, index) => (
          <PrescriptionCard
            key={prescription.id}
            prescription={prescription}
            index={index}
            isSelected={selectedIds.includes(prescription.id)}
            selectionDisabled={selectedIds.length >= 2}
            onSelectionChange={handleSelectionChange}
          />
        ))}
      </div>

      {/* Comparison view */}
      {isComparing && (
        <div className="mt-6">
          <PrescriptionComparisonView
            patientId={patientId}
            prescriptionId1={selectedIds[0]}
            prescriptionId2={selectedIds[1]}
          />
        </div>
      )}
    </div>
  )
}
