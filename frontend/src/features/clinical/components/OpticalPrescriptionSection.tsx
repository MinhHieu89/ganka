import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconPrinter, IconPencil, IconGlass } from "@tabler/icons-react"
import { toast } from "sonner"
import type { OpticalPrescriptionDto, RefractionDto } from "../api/clinical-api"
import {
  useAddOpticalPrescription,
  useUpdateOpticalPrescription,
} from "../api/prescription-api"
import { generateOpticalPrescriptionPdf } from "../api/document-api"
import { VisitSection } from "./VisitSection"
import { PrintButton } from "./PrintButton"
import { Button } from "@/shared/components/Button"
import {
  OpticalPrescriptionForm,
  type OpticalPrescriptionFormData,
} from "./OpticalPrescriptionForm"

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
  refractions: RefractionDto[]
  disabled: boolean
}

export function OpticalPrescriptionSection({
  visitId,
  prescriptions,
  refractions,
  disabled,
}: OpticalPrescriptionSectionProps) {
  const { t } = useTranslation("clinical")
  const [showForm, setShowForm] = useState(false)
  const [editMode, setEditMode] = useState(false)
  const [sectionOpen, setSectionOpen] = useState(true)

  const addMutation = useAddOpticalPrescription()
  const updateMutation = useUpdateOpticalPrescription()

  const hasPrescriptions = prescriptions.length > 0
  const existingRx = hasPrescriptions ? prescriptions[0] : null

  const handleAdd = (data: OpticalPrescriptionFormData) => {
    addMutation.mutate(
      {
        visitId,
        ...data,
      },
      {
        onSuccess: () => {
          toast.success(t("prescription.opticalRxSaved"))
          setShowForm(false)
        },
        onError: () => {
          toast.error(t("prescription.opticalRxSaveFailed"))
        },
      },
    )
  }

  const handleUpdate = (data: OpticalPrescriptionFormData) => {
    if (!existingRx) return
    updateMutation.mutate(
      {
        visitId,
        prescriptionId: existingRx.id,
        ...data,
      },
      {
        onSuccess: () => {
          toast.success(t("prescription.opticalRxSaved"))
          setEditMode(false)
        },
        onError: () => {
          toast.error(t("prescription.opticalRxSaveFailed"))
        },
      },
    )
  }

  return (
    <VisitSection
      title={t("prescription.opticalRx")}
      defaultOpen={true}
      {...(sectionOpen ? { open: true, onOpenChange: setSectionOpen } : {})}
      headerExtra={
        <div className="flex items-center gap-2">
          {!disabled && !hasPrescriptions && !showForm && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => { setShowForm(true); setSectionOpen(true) }}
            >
              <IconGlass className="h-4 w-4 mr-1" />
              {t("prescription.writeOpticalRx")}
            </Button>
          )}
          {hasPrescriptions && (
            <PrintButton
              onClick={() => generateOpticalPrescriptionPdf(visitId)}
              label={t("prescription.printOpticalRx")}
              icon={<IconPrinter className="h-4 w-4" />}
            />
          )}
        </div>
      }
    >
      <div className="space-y-4">
        {/* Create form (no existing Rx) */}
        {showForm && !hasPrescriptions && (
          <OpticalPrescriptionForm
            onSubmit={handleAdd}
            onCancel={() => setShowForm(false)}
            refractions={refractions}
            isSubmitting={addMutation.isPending}
          />
        )}

        {/* Edit form (existing Rx) */}
        {editMode && existingRx && (
          <OpticalPrescriptionForm
            onSubmit={handleUpdate}
            onCancel={() => setEditMode(false)}
            defaultValues={{
              odSph: existingRx.odSph,
              odCyl: existingRx.odCyl,
              odAxis: existingRx.odAxis,
              odAdd: existingRx.odAdd,
              osSph: existingRx.osSph,
              osCyl: existingRx.osCyl,
              osAxis: existingRx.osAxis,
              osAdd: existingRx.osAdd,
              nearOdSph: existingRx.nearOdSph,
              nearOdCyl: existingRx.nearOdCyl,
              nearOdAxis: existingRx.nearOdAxis,
              nearOsSph: existingRx.nearOsSph,
              nearOsCyl: existingRx.nearOsCyl,
              nearOsAxis: existingRx.nearOsAxis,
              farPd: existingRx.farPd,
              nearPd: existingRx.nearPd,
              lensType: existingRx.lensType,
              notes: existingRx.notes,
            }}
            refractions={refractions}
            isSubmitting={updateMutation.isPending}
          />
        )}

        {/* Read-only display */}
        {hasPrescriptions && !editMode && (
          <>
            {prescriptions.map((rx) => (
              <div key={rx.id} className="space-y-3">
                {/* Edit button */}
                {!disabled && (
                  <div className="flex justify-end">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setEditMode(true)}
                    >
                      <IconPencil className="h-4 w-4 mr-1" />
                      {t("prescription.editOpticalRx")}
                    </Button>
                  </div>
                )}

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
            ))}
          </>
        )}

        {/* Empty state (no form shown, no prescriptions) */}
        {!hasPrescriptions && !showForm && (
          <p className="text-sm text-muted-foreground">
            {t("prescription.noOpticalRx")}
          </p>
        )}
      </div>
    </VisitSection>
  )
}
