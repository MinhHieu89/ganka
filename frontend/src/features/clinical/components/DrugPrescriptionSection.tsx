import { useTranslation } from "react-i18next"
import { IconPrinter, IconTag } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import type { DrugPrescriptionDto } from "../api/clinical-api"
import {
  generateDrugPrescriptionPdf,
  generatePharmacyLabelPdf,
} from "../api/document-api"
import { VisitSection } from "./VisitSection"
import { PrintButton } from "./PrintButton"

interface DrugPrescriptionSectionProps {
  visitId: string
  prescriptions: DrugPrescriptionDto[]
  disabled: boolean
}

export function DrugPrescriptionSection({
  visitId,
  prescriptions,
  disabled,
}: DrugPrescriptionSectionProps) {
  const { t } = useTranslation("clinical")

  const hasPrescriptions = prescriptions.length > 0
  const hasItems = hasPrescriptions && prescriptions.some((p) => p.items.length > 0)

  return (
    <VisitSection
      title={t("prescription.drugRx")}
      headerExtra={
        <PrintButton
          onClick={() => generateDrugPrescriptionPdf(visitId)}
          label={t("prescription.printDrugRx")}
          icon={<IconPrinter className="h-4 w-4" />}
          disabled={!hasItems}
        />
      }
    >
      <div className="space-y-4">
        {!hasPrescriptions ? (
          <p className="text-sm text-muted-foreground">
            {t("prescription.noPrescriptions")}
          </p>
        ) : (
          prescriptions.map((rx) => (
            <div key={rx.id} className="space-y-2">
              {rx.prescriptionCode && (
                <p className="text-xs text-muted-foreground">
                  {rx.prescriptionCode}
                </p>
              )}
              {rx.items.length === 0 ? (
                <p className="text-sm text-muted-foreground">
                  {t("prescription.noPrescriptions")}
                </p>
              ) : (
                <div className="space-y-2">
                  {rx.items.map((item) => (
                    <div
                      key={item.id}
                      className="flex items-center gap-2 p-2 rounded-md border text-sm"
                    >
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          <span className="font-medium truncate">
                            {item.drugName}
                          </span>
                          {item.strength && (
                            <span className="text-muted-foreground text-xs shrink-0">
                              {item.strength}
                            </span>
                          )}
                          {item.isOffCatalog && (
                            <Badge variant="outline" className="shrink-0 text-xs">
                              {t("prescription.offCatalog")}
                            </Badge>
                          )}
                          {item.hasAllergyWarning && (
                            <Badge variant="destructive" className="shrink-0 text-xs">
                              {t("prescription.allergyWarning")}
                            </Badge>
                          )}
                        </div>
                        <div className="text-xs text-muted-foreground mt-0.5">
                          {item.dosageOverride || item.dosage}
                          {item.frequency && ` - ${item.frequency}`}
                          {item.durationDays != null &&
                            ` - ${item.durationDays} ${t("prescription.duration").toLowerCase()}`}
                        </div>
                      </div>
                      <span className="text-xs text-muted-foreground shrink-0">
                        {item.quantity} {item.unit}
                      </span>
                      <PrintButton
                        onClick={() => generatePharmacyLabelPdf(item.id)}
                        label={t("prescription.printLabel")}
                        icon={<IconTag className="h-3 w-3" />}
                        size="sm"
                        variant="ghost"
                      />
                    </div>
                  ))}
                </div>
              )}
              {rx.notes && (
                <p className="text-xs text-muted-foreground italic mt-2">
                  {rx.notes}
                </p>
              )}
            </div>
          ))
        )}
      </div>
    </VisitSection>
  )
}
