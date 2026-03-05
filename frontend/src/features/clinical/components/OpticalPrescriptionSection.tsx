import { useTranslation } from "react-i18next"
import { IconPrinter } from "@tabler/icons-react"
import type { OpticalPrescriptionDto } from "../api/clinical-api"
import { generateOpticalPrescriptionPdf } from "../api/document-api"
import { VisitSection } from "./VisitSection"
import { PrintButton } from "./PrintButton"

// LensType enum: 0=SingleVision, 1=Bifocal, 2=Progressive, 3=Reading
const LENS_TYPE_KEYS: Record<number, string> = {
  0: "singleVision",
  1: "bifocal",
  2: "progressive",
  3: "reading",
}

function formatDiopter(val: number | null): string {
  if (val == null) return "-"
  const sign = val > 0 ? "+" : ""
  return `${sign}${val.toFixed(2)}`
}

function formatAxis(val: number | null): string {
  if (val == null) return "-"
  return `${val}\u00B0`
}

function formatPd(val: number | null): string {
  if (val == null) return "-"
  return `${val.toFixed(1)}`
}

interface OpticalPrescriptionSectionProps {
  visitId: string
  prescriptions: OpticalPrescriptionDto[]
  disabled: boolean
}

export function OpticalPrescriptionSection({
  visitId,
  prescriptions,
  disabled,
}: OpticalPrescriptionSectionProps) {
  const { t } = useTranslation("clinical")

  const hasPrescriptions = prescriptions.length > 0

  return (
    <VisitSection
      title={t("prescription.opticalRx")}
      headerExtra={
        <PrintButton
          onClick={() => generateOpticalPrescriptionPdf(visitId)}
          label={t("prescription.printOpticalRx")}
          icon={<IconPrinter className="h-4 w-4" />}
          disabled={!hasPrescriptions}
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
            <div key={rx.id} className="space-y-3">
              {/* Distance Rx */}
              <div>
                <h4 className="text-sm font-medium mb-2">
                  {t("prescription.distanceRx")}
                </h4>
                <div className="grid grid-cols-5 gap-2 text-sm">
                  <div className="text-muted-foreground" />
                  <div className="text-center text-xs text-muted-foreground font-medium">
                    {t("refraction.sph")}
                  </div>
                  <div className="text-center text-xs text-muted-foreground font-medium">
                    {t("refraction.cyl")}
                  </div>
                  <div className="text-center text-xs text-muted-foreground font-medium">
                    {t("refraction.axis")}
                  </div>
                  <div className="text-center text-xs text-muted-foreground font-medium">
                    {t("refraction.add")}
                  </div>

                  <div className="font-medium text-xs">{t("refraction.od")}</div>
                  <div className="text-center font-mono text-xs">
                    {formatDiopter(rx.odSph)}
                  </div>
                  <div className="text-center font-mono text-xs">
                    {formatDiopter(rx.odCyl)}
                  </div>
                  <div className="text-center font-mono text-xs">
                    {formatAxis(rx.odAxis)}
                  </div>
                  <div className="text-center font-mono text-xs">
                    {formatDiopter(rx.odAdd)}
                  </div>

                  <div className="font-medium text-xs">{t("refraction.os")}</div>
                  <div className="text-center font-mono text-xs">
                    {formatDiopter(rx.osSph)}
                  </div>
                  <div className="text-center font-mono text-xs">
                    {formatDiopter(rx.osCyl)}
                  </div>
                  <div className="text-center font-mono text-xs">
                    {formatAxis(rx.osAxis)}
                  </div>
                  <div className="text-center font-mono text-xs">
                    {formatDiopter(rx.osAdd)}
                  </div>
                </div>
              </div>

              {/* Near Rx (show only if values exist) */}
              {(rx.nearOdSph != null ||
                rx.nearOdCyl != null ||
                rx.nearOsSph != null ||
                rx.nearOsCyl != null) && (
                <div>
                  <h4 className="text-sm font-medium mb-2">
                    {t("prescription.nearRx")}
                  </h4>
                  <div className="grid grid-cols-4 gap-2 text-sm">
                    <div className="text-muted-foreground" />
                    <div className="text-center text-xs text-muted-foreground font-medium">
                      {t("refraction.sph")}
                    </div>
                    <div className="text-center text-xs text-muted-foreground font-medium">
                      {t("refraction.cyl")}
                    </div>
                    <div className="text-center text-xs text-muted-foreground font-medium">
                      {t("refraction.axis")}
                    </div>

                    <div className="font-medium text-xs">{t("refraction.od")}</div>
                    <div className="text-center font-mono text-xs">
                      {formatDiopter(rx.nearOdSph)}
                    </div>
                    <div className="text-center font-mono text-xs">
                      {formatDiopter(rx.nearOdCyl)}
                    </div>
                    <div className="text-center font-mono text-xs">
                      {formatAxis(rx.nearOdAxis)}
                    </div>

                    <div className="font-medium text-xs">{t("refraction.os")}</div>
                    <div className="text-center font-mono text-xs">
                      {formatDiopter(rx.nearOsSph)}
                    </div>
                    <div className="text-center font-mono text-xs">
                      {formatDiopter(rx.nearOsCyl)}
                    </div>
                    <div className="text-center font-mono text-xs">
                      {formatAxis(rx.nearOsAxis)}
                    </div>
                  </div>
                </div>
              )}

              {/* PD and Lens Type */}
              <div className="flex gap-6 text-sm">
                <div>
                  <span className="text-muted-foreground text-xs">
                    {t("prescription.farPd")}:{" "}
                  </span>
                  <span className="font-mono text-xs">{formatPd(rx.farPd)}</span>
                </div>
                <div>
                  <span className="text-muted-foreground text-xs">
                    {t("prescription.nearPd")}:{" "}
                  </span>
                  <span className="font-mono text-xs">{formatPd(rx.nearPd)}</span>
                </div>
                <div>
                  <span className="text-muted-foreground text-xs">
                    {t("prescription.lensType")}:{" "}
                  </span>
                  <span className="text-xs">
                    {t(
                      `prescription.${LENS_TYPE_KEYS[rx.lensType] ?? "singleVision"}`,
                    )}
                  </span>
                </div>
              </div>

              {rx.notes && (
                <p className="text-xs text-muted-foreground italic">
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
