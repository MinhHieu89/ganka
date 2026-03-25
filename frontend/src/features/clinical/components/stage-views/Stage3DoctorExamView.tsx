import { useState, useCallback } from "react"
import { toast } from "sonner"
import { IconX } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Checkbox } from "@/shared/components/Checkbox"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/shared/components/Dialog"
import { StageDetailShell } from "../StageDetailShell"
import { StageBottomBar } from "../StageBottomBar"
import { ValidationMessage } from "../ValidationMessage"
import { Icd10Combobox } from "../Icd10Combobox"
import {
  useAdvanceStage,
  useRequestImaging,
  useUpdateNotes,
  type VisitDetailDto,
  type VisitDiagnosisDto,
  type Icd10SearchResultDto,
  useAddDiagnosis,
  useRemoveDiagnosis,
} from "../../api/clinical-api"

// WorkflowStage enum values
const STAGE_DOCTOR_EXAM = 2
const STAGE_IMAGING = 3
const STAGE_PRESCRIPTION = 5

// Imaging service options
const IMAGING_SERVICES = [
  { value: "OCT", label: "OCT" },
  { value: "IOP", label: "IOP (Nhãn áp)" },
  { value: "Fundus", label: "Fundus (Soi đáy mắt)" },
  { value: "Topography", label: "Topography (Địa hình giác mạc)" },
  { value: "Anterior", label: "Anterior Segment (Bán phần trước)" },
]

// Skip reason labels (for skip banner)
const SKIP_REASON_LABELS: Record<number, string> = {
  0: "Tái khám, đã có kết quả cũ",
  1: "Bệnh nhân từ chối đo",
  2: "Khám tổng quát, không liên quan khúc xạ",
  3: "Khác",
}

const LABELS = {
  stageName: "Khám bác sĩ",
  saveDraft: "Lưu nháp",
  imagingButton: "Chuyển CĐHA \u203A",
  prescriptionButton: "Kê đơn \u203A",
  pillActive: "Đang khám",
  pillComplete: "Sẵn sàng",
  refractionSkipped: "Đo khúc xạ đã bỏ qua",
  validationEmpty: "Chẩn đoán bắt buộc — chọn ít nhất một mã ICD-10 để tiếp tục.",
  validationSearching: "Đang tìm kiếm — chọn mã từ danh sách để tiếp tục.",
  validationReady: "Sẵn sàng — chuyển kê đơn, hoặc yêu cầu CĐHA nếu cần.",
  imagingModalTitle: "Yêu cầu chẩn đoán hình ảnh",
  imagingNote: "Ghi chú cho KTV (tùy chọn)",
  imagingServices: "Dịch vụ cần thực hiện",
  send: "Gửi yêu cầu",
  cancel: "Hủy",
}

interface Stage3DoctorExamViewProps {
  visit: VisitDetailDto
}

export function Stage3DoctorExamView({ visit }: Stage3DoctorExamViewProps) {
  const [notes, setNotes] = useState(visit.examinationNotes ?? "")
  const [imagingModalOpen, setImagingModalOpen] = useState(false)
  const [imagingNote, setImagingNote] = useState("")
  const [selectedServices, setSelectedServices] = useState<string[]>([])

  const advanceMutation = useAdvanceStage()
  const requestImagingMutation = useRequestImaging()
  const updateNotesMutation = useUpdateNotes()
  const addDiagnosisMutation = useAddDiagnosis()
  const removeDiagnosisMutation = useRemoveDiagnosis()

  const diagnoses = visit.diagnoses
  const hasDiagnoses = diagnoses.length > 0

  // -- Handlers --

  const handleSaveDraft = useCallback(() => {
    updateNotesMutation.mutate(
      { visitId: visit.id, notes: notes || null },
      {
        onSuccess: () => toast.success("Đã lưu nháp"),
        onError: () => toast.error("Không thể lưu"),
      },
    )
  }, [visit.id, notes, updateNotesMutation])

  const handleAddDiagnosis = useCallback(
    (code: Icd10SearchResultDto, laterality: number) => {
      const role = diagnoses.length === 0 ? 0 : 1
      const sortOrder = diagnoses.length
      addDiagnosisMutation.mutate(
        {
          visitId: visit.id,
          icd10Code: code.code,
          descriptionEn: code.descriptionEn,
          descriptionVi: code.descriptionVi,
          laterality,
          role,
          sortOrder,
        },
        {
          onSuccess: () => toast.success("Đã thêm chẩn đoán"),
          onError: () => toast.error("Không thể thêm chẩn đoán"),
        },
      )
    },
    [visit.id, diagnoses.length, addDiagnosisMutation],
  )

  const handleRemoveDiagnosis = useCallback(
    (diagnosisId: string) => {
      removeDiagnosisMutation.mutate(
        { visitId: visit.id, diagnosisId },
        {
          onSuccess: () => toast.success("Đã xóa chẩn đoán"),
          onError: () => toast.error("Không thể xóa chẩn đoán"),
        },
      )
    },
    [visit.id, removeDiagnosisMutation],
  )

  const handlePrescription = useCallback(() => {
    if (!hasDiagnoses) return
    advanceMutation.mutate(
      { visitId: visit.id, newStage: STAGE_PRESCRIPTION },
      {
        onSuccess: () => toast.success("Chuyển kê đơn thành công"),
        onError: () => toast.error("Không thể chuyển"),
      },
    )
  }, [visit.id, hasDiagnoses, advanceMutation])

  const handleRequestImaging = useCallback(() => {
    if (selectedServices.length === 0) {
      toast.error("Chọn ít nhất một dịch vụ")
      return
    }
    requestImagingMutation.mutate(
      {
        visitId: visit.id,
        note: imagingNote || undefined,
        services: selectedServices,
      },
      {
        onSuccess: () => {
          toast.success("Đã gửi yêu cầu CĐHA")
          setImagingModalOpen(false)
          setImagingNote("")
          setSelectedServices([])
        },
        onError: () => toast.error("Không thể gửi yêu cầu"),
      },
    )
  }, [visit.id, imagingNote, selectedServices, requestImagingMutation])

  const toggleService = useCallback((service: string) => {
    setSelectedServices((prev) =>
      prev.includes(service)
        ? prev.filter((s) => s !== service)
        : [...prev, service],
    )
  }, [])

  // -- Validation --
  function getValidationState(): {
    state: "error" | "warning" | "success"
    message: string
  } {
    if (!hasDiagnoses)
      return { state: "error", message: LABELS.validationEmpty }
    return { state: "success", message: LABELS.validationReady }
  }

  // -- Refraction summary --
  const refractionData = visit.refractions.find((r) => r.type === 0) // manifest refraction

  // -- Stage pill --
  const stagePill = hasDiagnoses
    ? { text: LABELS.pillComplete, variant: "green" as const }
    : { text: LABELS.pillActive, variant: "default" as const }

  // -- Validation message --
  const validationData = getValidationState()
  const validationMessage = (
    <ValidationMessage state={validationData.state} message={validationData.message} />
  )

  // -- Bottom bar --
  const bottomBar = (
    <StageBottomBar
      saveDraftButton={{
        label: LABELS.saveDraft,
        onClick: handleSaveDraft,
      }}
      secondaryButton={{
        label: LABELS.imagingButton,
        onClick: () => setImagingModalOpen(true),
      }}
      primaryButton={
        // Hide prescription button if imaging was already requested
        visit.imagingRequested
          ? undefined
          : {
              label: LABELS.prescriptionButton,
              onClick: handlePrescription,
              disabled: !hasDiagnoses,
            }
      }
      validationMessage={validationMessage}
    />
  )

  return (
    <>
      <StageDetailShell
        patientName={visit.patientName}
        patientId={visit.patientId}
        doctorName={visit.doctorName}
        visitDate={visit.visitDate}
        stageName={LABELS.stageName}
        stagePill={stagePill}
        bottomBar={bottomBar}
      >
        <div className="space-y-6">
          {/* Refraction summary card */}
          {visit.refractionSkipped ? (
            <RefractionSkipNotice />
          ) : (
            refractionData && <RefractionSummaryCard refraction={refractionData} />
          )}

          {/* ICD-10 Diagnosis section */}
          <div className="space-y-3">
            <h3 className="text-sm font-semibold">Chẩn đoán ICD-10</h3>

            {/* Selected diagnoses as tags */}
            {diagnoses.length > 0 && (
              <div className="flex flex-wrap gap-2">
                {diagnoses.map((d) => (
                  <DiagnosisTag
                    key={d.id}
                    diagnosis={d}
                    onRemove={() => handleRemoveDiagnosis(d.id)}
                  />
                ))}
              </div>
            )}

            {/* Search input */}
            <Icd10Combobox
              doctorId={visit.doctorId}
              onSelect={handleAddDiagnosis}
              disabled={false}
            />
          </div>

          {/* Examination notes */}
          <div className="space-y-2">
            <h3 className="text-sm font-semibold">Ghi chú khám</h3>
            <AutoResizeTextarea
              className="min-h-[100px] resize-none"
              rows={4}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </div>
        </div>
      </StageDetailShell>

      {/* Imaging request modal */}
      <Dialog open={imagingModalOpen} onOpenChange={setImagingModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{LABELS.imagingModalTitle}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">{LABELS.imagingServices}</label>
              <div className="space-y-2">
                {IMAGING_SERVICES.map((svc) => (
                  <label
                    key={svc.value}
                    className="flex items-center gap-2 cursor-pointer"
                  >
                    <Checkbox
                      checked={selectedServices.includes(svc.value)}
                      onCheckedChange={() => toggleService(svc.value)}
                    />
                    <span className="text-sm">{svc.label}</span>
                  </label>
                ))}
              </div>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">{LABELS.imagingNote}</label>
              <AutoResizeTextarea
                className="min-h-[60px] resize-none"
                rows={2}
                value={imagingNote}
                onChange={(e) => setImagingNote(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setImagingModalOpen(false)}>
              {LABELS.cancel}
            </Button>
            <Button
              onClick={handleRequestImaging}
              disabled={
                selectedServices.length === 0 || requestImagingMutation.isPending
              }
            >
              {LABELS.send}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}

// -- Sub-components --

function RefractionSkipNotice() {
  return (
    <div className="bg-amber-50 border border-amber-300 rounded-md px-4 py-3">
      <p className="text-sm text-amber-800">
        {LABELS.refractionSkipped}
      </p>
    </div>
  )
}

interface RefractionSummaryCardProps {
  refraction: {
    odSph: number | null
    odCyl: number | null
    odAxis: number | null
    osSph: number | null
    osCyl: number | null
    osAxis: number | null
    ucvaOd: number | null
    ucvaOs: number | null
    bcvaOd: number | null
    bcvaOs: number | null
  }
}

function RefractionSummaryCard({ refraction }: RefractionSummaryCardProps) {
  const fmt = (v: number | null) => (v !== null ? String(v) : "—")

  return (
    <div className="border rounded-md p-4 bg-muted/30">
      <h3 className="text-sm font-semibold mb-3">Tóm tắt khúc xạ</h3>
      <div className="grid grid-cols-[80px_1fr_1fr] gap-x-4 gap-y-1 text-sm">
        <div />
        <span className="text-center font-medium text-xs text-muted-foreground">
          OD (Mắt phải)
        </span>
        <span className="text-center font-medium text-xs text-muted-foreground">
          OS (Mắt trái)
        </span>

        <span className="text-xs text-muted-foreground">SPH</span>
        <span className="text-center tabular-nums">{fmt(refraction.odSph)}</span>
        <span className="text-center tabular-nums">{fmt(refraction.osSph)}</span>

        <span className="text-xs text-muted-foreground">CYL</span>
        <span className="text-center tabular-nums">{fmt(refraction.odCyl)}</span>
        <span className="text-center tabular-nums">{fmt(refraction.osCyl)}</span>

        <span className="text-xs text-muted-foreground">AXIS</span>
        <span className="text-center tabular-nums">{fmt(refraction.odAxis)}</span>
        <span className="text-center tabular-nums">{fmt(refraction.osAxis)}</span>

        <span className="text-xs text-muted-foreground">UCVA</span>
        <span className="text-center tabular-nums">{fmt(refraction.ucvaOd)}</span>
        <span className="text-center tabular-nums">{fmt(refraction.ucvaOs)}</span>

        <span className="text-xs text-muted-foreground">BCVA</span>
        <span className="text-center tabular-nums">{fmt(refraction.bcvaOd)}</span>
        <span className="text-center tabular-nums">{fmt(refraction.bcvaOs)}</span>
      </div>
    </div>
  )
}

interface DiagnosisTagProps {
  diagnosis: VisitDiagnosisDto
  variant?: "teal" | "amber"
  showNewBadge?: boolean
  onRemove?: () => void
}

function DiagnosisTag({
  diagnosis,
  variant = "teal",
  showNewBadge = false,
  onRemove,
}: DiagnosisTagProps) {
  const colorClasses =
    variant === "amber"
      ? "bg-amber-100 text-amber-800 border-amber-300"
      : "bg-teal-100 text-teal-800 border-teal-300"

  return (
    <Badge variant="outline" className={`${colorClasses} gap-1 pr-1`}>
      <span className="font-mono text-xs">{diagnosis.icd10Code}</span>
      <span className="text-xs truncate max-w-[150px]">
        {diagnosis.descriptionVi || diagnosis.descriptionEn}
      </span>
      {showNewBadge && (
        <span className="bg-amber-200 text-amber-900 text-[10px] px-1 rounded">
          new
        </span>
      )}
      {onRemove && (
        <button
          type="button"
          className="ml-0.5 hover:bg-black/10 rounded p-0.5"
          onClick={onRemove}
        >
          <IconX className="h-3 w-3" />
        </button>
      )}
    </Badge>
  )
}

export { DiagnosisTag }
