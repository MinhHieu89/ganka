import { useState, useCallback, useMemo, useRef } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import {
  IconPlus,
  IconPencil,
  IconX,
  IconPrinter,
  IconTag,
} from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Label } from "@/shared/components/Label"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/AlertDialog"
import { usePatientById } from "@/features/patient/api/patient-api"
import type { DrugPrescriptionDto, PrescriptionItemDto } from "../api/clinical-api"
import {
  useAddDrugPrescription,
  useRemoveDrugPrescription,
  useUpdateDrugPrescription,
  type PrescriptionItemInput,
} from "../api/prescription-api"
import {
  generateDrugPrescriptionPdf,
  generatePharmacyLabelPdf,
  generateBatchLabelsPdf,
} from "../api/document-api"
import { VisitSection } from "./VisitSection"
import { PrintButton } from "./PrintButton"
import { DrugPrescriptionForm } from "./DrugPrescriptionForm"

// DrugForm enum labels (matches DrugPrescriptionForm options)
const DRUG_FORM_LABELS: Record<number, string> = {
  0: "drugForm.eyeDrops",
  1: "drugForm.tablet",
  2: "drugForm.capsule",
  3: "drugForm.ointment",
  4: "drugForm.injection",
  5: "drugForm.gel",
  6: "drugForm.solution",
  7: "drugForm.suspension",
  8: "drugForm.cream",
  9: "drugForm.spray",
}

// DrugRoute enum labels
const DRUG_ROUTE_LABELS: Record<number, string> = {
  0: "drugRoute.topical",
  1: "drugRoute.oral",
  2: "drugRoute.intramuscular",
  3: "drugRoute.intravenous",
  4: "drugRoute.subconjunctival",
  5: "drugRoute.intravitreal",
  6: "drugRoute.periocular",
}

interface DrugPrescriptionSectionProps {
  visitId: string
  patientId: string
  prescriptions: DrugPrescriptionDto[]
  disabled: boolean
}

export function DrugPrescriptionSection({
  visitId,
  patientId,
  prescriptions,
  disabled,
}: DrugPrescriptionSectionProps) {
  const { t } = useTranslation("clinical")

  // Fetch patient data for allergies
  const { data: patient } = usePatientById(patientId)
  const patientAllergies = patient?.allergies ?? []

  // Local state for new prescription items (add-then-edit pattern)
  const [localItems, setLocalItems] = useState<PrescriptionItemInput[]>([])
  const [notes, setNotes] = useState("")
  const notesRef = useRef(notes)
  notesRef.current = notes
  const [formOpen, setFormOpen] = useState(false)
  const [editingIndex, setEditingIndex] = useState<number | null>(null)
  const [allergyDialogOpen, setAllergyDialogOpen] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [deletingPrescriptionId, setDeletingPrescriptionId] = useState<string | null>(null)

  // Mutations
  const addPrescriptionMutation = useAddDrugPrescription()
  const removePrescriptionMutation = useRemoveDrugPrescription()
  const updatePrescriptionMutation = useUpdateDrugPrescription()

  const hasPrescriptions = prescriptions.length > 0
  const hasItems = hasPrescriptions && prescriptions.some((p) => p.items.length > 0)

  // Handle adding a new item from the form dialog
  const handleFormSubmit = useCallback(
    (item: PrescriptionItemInput) => {
      if (editingIndex != null) {
        // Editing existing local item
        setLocalItems((prev) => {
          const updated = [...prev]
          updated[editingIndex] = item
          return updated
        })
        setEditingIndex(null)
      } else {
        // Adding new item
        setLocalItems((prev) => [...prev, item])
      }
    },
    [editingIndex],
  )

  // Open form dialog for adding
  const handleAddClick = useCallback(() => {
    setEditingIndex(null)
    setFormOpen(true)
  }, [])

  // Open form dialog for editing a local item
  const handleEditLocalItem = useCallback((index: number) => {
    setEditingIndex(index)
    setFormOpen(true)
  }, [])

  // Remove a local item
  const handleRemoveLocalItem = useCallback((index: number) => {
    setLocalItems((prev) => prev.filter((_, i) => i !== index))
  }, [])

  // Check if any local items have allergy warnings
  const hasAllergyConflicts = useMemo(
    () => localItems.some((item) => item.hasAllergyWarning),
    [localItems],
  )

  const doSavePrescription = useCallback(() => {
    addPrescriptionMutation.mutate(
      {
        visitId,
        notes: notesRef.current.trim() || null,
        items: localItems,
      },
      {
        onSuccess: () => {
          toast.success(t("prescription.addDrug"))
          setLocalItems([])
          setNotes("")
        },
        onError: () => {
          toast.error(t("prescription.printFailed"))
        },
      },
    )
  }, [visitId, localItems, addPrescriptionMutation, t])

  // Submit prescription (save all local items)
  const handleSavePrescription = useCallback(() => {
    if (localItems.length === 0) return

    if (hasAllergyConflicts) {
      // Show blocking AlertDialog
      setAllergyDialogOpen(true)
      return
    }

    doSavePrescription()
  }, [localItems, hasAllergyConflicts, doSavePrescription])

  // Confirm allergy and save
  const handleAllergyConfirm = useCallback(() => {
    setAllergyDialogOpen(false)
    doSavePrescription()
  }, [doSavePrescription])

  // Remove an existing server-side prescription
  const handleRemovePrescriptionClick = useCallback(
    (prescriptionId: string) => {
      setDeletingPrescriptionId(prescriptionId)
      setDeleteDialogOpen(true)
    },
    [],
  )

  const handleDeleteConfirm = useCallback(() => {
    if (!deletingPrescriptionId) return
    setDeleteDialogOpen(false)
    removePrescriptionMutation.mutate(
      { visitId, prescriptionId: deletingPrescriptionId },
      {
        onSuccess: () => {
          toast.success(t("prescription.removeDrug"))
          setDeletingPrescriptionId(null)
        },
        onError: () => {
          toast.error(t("prescription.printFailed"))
          setDeletingPrescriptionId(null)
        },
      },
    )
  }, [visitId, deletingPrescriptionId, removePrescriptionMutation, t])

  // Update notes on an existing prescription
  const handleUpdateNotes = useCallback(
    (prescriptionId: string, updatedNotes: string) => {
      updatePrescriptionMutation.mutate(
        {
          visitId,
          prescriptionId,
          notes: updatedNotes.trim() || null,
        },
        {
          onSuccess: () => {
            toast.success(t("prescription.doctorAdvice"))
          },
          onError: () => {
            toast.error(t("prescription.printFailed"))
          },
        },
      )
    },
    [visitId, updatePrescriptionMutation, t],
  )

  const editingItem = editingIndex != null ? localItems[editingIndex] : null

  return (
    <VisitSection
      title={t("prescription.drugRx")}
      headerExtra={
        <div className="flex items-center gap-2">
          {hasItems && (
            <PrintButton
              onClick={() => generateDrugPrescriptionPdf(visitId)}
              label={t("prescription.printDrugRx")}
              icon={<IconPrinter className="h-4 w-4" />}
            />
          )}
          {!disabled && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={handleAddClick}
            >
              <IconPlus className="h-4 w-4 mr-1" />
              {t("prescription.addDrug")}
            </Button>
          )}
        </div>
      }
    >
      <div className="space-y-4">
        {/* Existing server-side prescriptions */}
        {!hasPrescriptions && localItems.length === 0 ? (
          <div className="text-center py-6">
            <p className="text-sm text-muted-foreground">
              {t("prescription.noPrescriptions")}
            </p>
            {!disabled && (
              <Button
                type="button"
                variant="outline"
                size="sm"
                className="mt-3"
                onClick={handleAddClick}
              >
                <IconPlus className="h-4 w-4 mr-1" />
                {t("prescription.addDrug")}
              </Button>
            )}
          </div>
        ) : (
          <>
            {/* Saved prescriptions */}
            {prescriptions.map((rx) => (
              <div key={rx.id} className="space-y-2">
                {rx.prescriptionCode && (
                  <p className="text-xs text-muted-foreground font-medium">
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
                      <PrescriptionItemRow
                        key={item.id}
                        item={item}
                        disabled={disabled}
                        t={t}
                      />
                    ))}
                  </div>
                )}
                {/* Notes / Doctor's advice for existing prescription */}
                {rx.notes && (
                  <p className="text-xs text-muted-foreground italic mt-2">
                    {t("prescription.doctorAdvice")}: {rx.notes}
                  </p>
                )}
                {!disabled && (
                  <div className="flex items-center gap-2 mt-2">
                    <PrintButton
                      onClick={() => generateDrugPrescriptionPdf(visitId)}
                      label={t("prescription.printDrugRx")}
                      icon={<IconPrinter className="h-3 w-3" />}
                      size="sm"
                      variant="ghost"
                    />
                    {rx.items.length > 0 && (
                      <PrintButton
                        onClick={() => generateBatchLabelsPdf(rx.id)}
                        label={t("prescription.printAllLabels")}
                        icon={<IconTag className="h-3 w-3" />}
                        size="sm"
                        variant="ghost"
                      />
                    )}
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      className="text-destructive"
                      onClick={() => handleRemovePrescriptionClick(rx.id)}
                    >
                      <IconX className="h-3 w-3 mr-1" />
                      {t("prescription.removeDrug")}
                    </Button>
                  </div>
                )}
              </div>
            ))}

            {/* Local items (not yet saved) */}
            {localItems.length > 0 && (
              <div className="space-y-2 border-t pt-3">
                <p className="text-xs font-medium text-muted-foreground">
                  {t("prescription.addDrug")}
                </p>
                {localItems.map((item, idx) => (
                  <LocalItemRow
                    key={idx}
                    item={item}
                    index={idx}
                    onEdit={handleEditLocalItem}
                    onRemove={handleRemoveLocalItem}
                    t={t}
                  />
                ))}

                {/* Doctor's advice */}
                <div className="space-y-1.5 mt-3">
                  <Label className="text-xs">{t("prescription.doctorAdvice")}</Label>
                  <AutoResizeTextarea
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    rows={2}
                    className="text-sm"
                  />
                </div>

                {/* Save button */}
                <div className="flex justify-end gap-2 mt-3">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      setLocalItems([])
                      setNotes("")
                    }}
                  >
                    {t("images.cancel")}
                  </Button>
                  <Button
                    type="button"
                    size="sm"
                    onClick={handleSavePrescription}
                    disabled={addPrescriptionMutation.isPending}
                  >
                    {t("prescription.confirmPrescribe")}
                  </Button>
                </div>
              </div>
            )}
          </>
        )}
      </div>

      {/* Drug prescription form dialog */}
      <DrugPrescriptionForm
        open={formOpen}
        onOpenChange={setFormOpen}
        onSubmit={handleFormSubmit}
        editingItem={editingItem}
        patientAllergies={patientAllergies}
      />

      {/* Allergy blocking AlertDialog */}
      <AlertDialog open={allergyDialogOpen} onOpenChange={setAllergyDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {t("prescription.allergyConfirmTitle")}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {t("prescription.allergyConfirmMessage")}
              <ul className="list-disc list-inside mt-2 text-destructive">
                {localItems
                  .filter((item) => item.hasAllergyWarning)
                  .map((item, idx) => (
                    <li key={idx} className="text-sm">
                      {item.drugName}
                      {item.genericName && (
                        <span className="text-muted-foreground">
                          {" "}
                          ({item.genericName})
                        </span>
                      )}
                    </li>
                  ))}
              </ul>
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t("images.cancel")}</AlertDialogCancel>
            <AlertDialogAction
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
              onClick={handleAllergyConfirm}
            >
              {t("prescription.confirmPrescribe")}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Delete confirmation AlertDialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {t("prescription.deleteConfirmTitle")}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {t("prescription.deleteConfirmMessage")}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t("images.cancel")}</AlertDialogCancel>
            <AlertDialogAction
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
              onClick={handleDeleteConfirm}
            >
              {t("prescription.removeDrug")}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </VisitSection>
  )
}

// -- Sub-components for item display --

function PrescriptionItemRow({
  item,
  disabled,
  t,
}: {
  item: PrescriptionItemDto
  disabled: boolean
  t: (key: string) => string
}) {
  return (
    <div className="flex items-center gap-2 p-2 rounded-md border text-sm">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 flex-wrap">
          <span className="font-medium truncate">{item.drugName}</span>
          {item.genericName && (
            <span className="text-muted-foreground text-xs">
              ({item.genericName})
            </span>
          )}
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
        <div className="text-xs text-muted-foreground mt-0.5 flex items-center gap-2 flex-wrap">
          <span>{t(DRUG_FORM_LABELS[item.form] ?? "drugForm.eyeDrops")}</span>
          <span>-</span>
          <span>{t(DRUG_ROUTE_LABELS[item.route] ?? "drugRoute.topical")}</span>
          {(item.dosageOverride || item.dosage) && (
            <>
              <span>-</span>
              <span>{item.dosageOverride || item.dosage}</span>
            </>
          )}
        </div>
      </div>
      <span className="text-xs text-muted-foreground shrink-0">
        {item.quantity} {item.unit}
      </span>
      {!disabled && (
        <PrintButton
          onClick={() => generatePharmacyLabelPdf(item.id)}
          label={t("prescription.printLabel")}
          icon={<IconTag className="h-3 w-3" />}
          size="sm"
          variant="ghost"
        />
      )}
    </div>
  )
}

function LocalItemRow({
  item,
  index,
  onEdit,
  onRemove,
  t,
}: {
  item: PrescriptionItemInput
  index: number
  onEdit: (index: number) => void
  onRemove: (index: number) => void
  t: (key: string) => string
}) {
  return (
    <div className="flex items-center gap-2 p-2 rounded-md border border-dashed text-sm">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 flex-wrap">
          <span className="font-medium truncate">{item.drugName}</span>
          {item.genericName && (
            <span className="text-muted-foreground text-xs">
              ({item.genericName})
            </span>
          )}
          {item.strength && (
            <span className="text-muted-foreground text-xs shrink-0">
              {item.strength}
            </span>
          )}
          {!item.drugCatalogItemId && (
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
        <div className="text-xs text-muted-foreground mt-0.5 flex items-center gap-2 flex-wrap">
          <span>{t(DRUG_FORM_LABELS[item.form] ?? "drugForm.eyeDrops")}</span>
          <span>-</span>
          <span>{t(DRUG_ROUTE_LABELS[item.route] ?? "drugRoute.topical")}</span>
          {(item.dosageOverride || item.dosage) && (
            <>
              <span>-</span>
              <span>{item.dosageOverride || item.dosage}</span>
            </>
          )}
        </div>
      </div>
      <span className="text-xs text-muted-foreground shrink-0">
        {item.quantity} {item.unit}
      </span>
      <Button
        type="button"
        variant="ghost"
        size="sm"
        className="h-7 w-7 p-0"
        onClick={() => onEdit(index)}
      >
        <IconPencil className="h-3 w-3" />
      </Button>
      <Button
        type="button"
        variant="ghost"
        size="sm"
        className="h-7 w-7 p-0 text-destructive"
        onClick={() => onRemove(index)}
      >
        <IconX className="h-3 w-3" />
      </Button>
    </div>
  )
}
