import { useState, useCallback, useMemo } from "react"
import { toast } from "sonner"
import { Badge } from "@/shared/components/Badge"
import { StageDetailShell } from "../StageDetailShell"
import { StageBottomBar } from "../StageBottomBar"
import { ValidationMessage } from "../ValidationMessage"
import { Icd10Combobox } from "../Icd10Combobox"
import { DiagnosisTag } from "./Stage3DoctorExamView"
import {
  useAdvanceStage,
  useVisitImages,
  useAddDiagnosis,
  useRemoveDiagnosis,
  type VisitDetailDto,
  type VisitDiagnosisDto,
  type MedicalImageDto,
  type Icd10SearchResultDto,
  IMAGE_TYPE_LABELS,
  EYE_TAG_LABELS,
} from "../../api/clinical-api"

// WorkflowStage enum values
const STAGE_DOCTOR_REVIEWS = 4
const STAGE_PRESCRIPTION = 5

// Modality color tints
const MODALITY_TINTS: Record<number, string> = {
  2: "bg-blue-50 border-blue-200",     // OCT
  0: "bg-orange-50 border-orange-200",  // Fluorescein (Fundus-like)
  1: "bg-orange-50 border-orange-200",  // Meibography
  3: "bg-green-50 border-green-200",    // Specular Microscopy (anterior segment)
  4: "bg-green-50 border-green-200",    // Topography
  5: "bg-gray-50 border-gray-200",      // Video
}

const LABELS = {
  stageName: "BS đọc kết quả",
  saveDraft: "Lưu nháp",
  forwardButton: "Xác nhận, tiếp tục kê đơn \u203A",
  pillActive: "Đang xem",
  pillComplete: "Sẵn sàng",
  ktvNote: "Ghi chú KTV",
  diagnosisTitle: "Chẩn đoán",
  existingDiagnoses: "Chẩn đoán từ Stage 3",
  newDiagnoses: "Chẩn đoán bổ sung",
  changeLog: "Thay đổi",
  readyMessage: "Sẵn sàng — xác nhận để tiếp tục kê đơn.",
  imageViewer: "Hình ảnh chẩn đoán",
}

interface Stage4bDoctorReviewViewProps {
  visit: VisitDetailDto
}

export function Stage4bDoctorReviewView({
  visit,
}: Stage4bDoctorReviewViewProps) {
  const [selectedImage, setSelectedImage] = useState<MedicalImageDto | null>(
    null,
  )
  // Track which diagnosis IDs existed at stage entry (from Stage 3)
  const [stage3DiagnosisIds] = useState<Set<string>>(
    () => new Set(visit.diagnoses.map((d) => d.id)),
  )

  const advanceMutation = useAdvanceStage()
  const { data: images } = useVisitImages(visit.id)
  const addDiagnosisMutation = useAddDiagnosis()
  const removeDiagnosisMutation = useRemoveDiagnosis()

  const imageList = images ?? []

  // Separate diagnoses into existing (Stage 3) and new (added in 4b)
  const existingDiagnoses = visit.diagnoses.filter((d) =>
    stage3DiagnosisIds.has(d.id),
  )
  const newDiagnoses = visit.diagnoses.filter(
    (d) => !stage3DiagnosisIds.has(d.id),
  )

  // -- Handlers --

  const handleAdvance = useCallback(() => {
    advanceMutation.mutate(
      { visitId: visit.id, newStage: STAGE_PRESCRIPTION },
      {
        onSuccess: () => toast.success("Chuyển kê đơn thành công"),
        onError: () => toast.error("Không thể chuyển"),
      },
    )
  }, [visit.id, advanceMutation])

  const handleAddDiagnosis = useCallback(
    (code: Icd10SearchResultDto, laterality: number) => {
      const role = visit.diagnoses.length === 0 ? 0 : 1
      const sortOrder = visit.diagnoses.length
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
    [visit.id, visit.diagnoses.length, addDiagnosisMutation],
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

  // -- Stage pill --
  const stagePill = { text: LABELS.pillComplete, variant: "green" as const }

  // -- Validation message --
  const validationMessage = (
    <ValidationMessage state="success" message={LABELS.readyMessage} />
  )

  // -- Bottom bar (always enabled) --
  const bottomBar = (
    <StageBottomBar
      saveDraftButton={{
        label: LABELS.saveDraft,
        onClick: () => toast.success("Đã lưu nháp"),
      }}
      primaryButton={{
        label: LABELS.forwardButton,
        onClick: handleAdvance,
        disabled: false,
      }}
      validationMessage={validationMessage}
    />
  )

  // Get IOP data from refractions for display
  const refractionData = visit.refractions.find((r) => r.type === 0)
  const iopOd = refractionData?.iopOd
  const iopOs = refractionData?.iopOs

  return (
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
        {/* Image viewer */}
        <div className="space-y-3">
          <h3 className="text-sm font-semibold">{LABELS.imageViewer}</h3>

          {imageList.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              Không có hình ảnh chẩn đoán.
            </p>
          ) : (
            <>
              {/* Thumbnail grid */}
              <div className="grid grid-cols-3 gap-2">
                {imageList.map((img) => {
                  const tint =
                    MODALITY_TINTS[img.type] ?? "bg-gray-50 border-gray-200"
                  const isSelected = selectedImage?.id === img.id
                  const typeLabel =
                    IMAGE_TYPE_LABELS[img.type]?.vi ??
                    IMAGE_TYPE_LABELS[img.type]?.en ??
                    ""
                  const eyeLabel =
                    img.eyeTag != null
                      ? EYE_TAG_LABELS[img.eyeTag]?.vi ?? ""
                      : ""

                  return (
                    <button
                      key={img.id}
                      type="button"
                      className={`relative rounded-md overflow-hidden border-2 transition-all ${tint} ${
                        isSelected
                          ? "ring-2 ring-primary ring-offset-1"
                          : "hover:opacity-80"
                      }`}
                      onClick={() => setSelectedImage(img)}
                    >
                      <img
                        src={img.url}
                        alt={img.fileName}
                        className="w-full aspect-square object-cover"
                        loading="lazy"
                      />
                      <div className="absolute bottom-0 left-0 right-0 bg-black/50 px-1 py-0.5">
                        <p className="text-[10px] text-white truncate">
                          {typeLabel}
                          {eyeLabel ? ` - ${eyeLabel}` : ""}
                        </p>
                      </div>
                    </button>
                  )
                })}
              </div>

              {/* Inline preview panel */}
              {selectedImage && (
                <div className="border rounded-md overflow-hidden">
                  <div className="relative aspect-video bg-black">
                    <img
                      src={selectedImage.url}
                      alt={selectedImage.fileName}
                      className="w-full h-full object-contain"
                    />
                    <div className="absolute bottom-2 left-2 right-2 flex items-center justify-between">
                      <Badge
                        variant="outline"
                        className="bg-black/60 text-white border-transparent text-xs"
                      >
                        {IMAGE_TYPE_LABELS[selectedImage.type]?.vi ??
                          IMAGE_TYPE_LABELS[selectedImage.type]?.en ??
                          ""}
                        {selectedImage.eyeTag != null &&
                          ` - ${EYE_TAG_LABELS[selectedImage.eyeTag]?.vi ?? ""}`}
                      </Badge>
                    </div>
                  </div>
                  {/* IOP metadata */}
                  {(iopOd !== null || iopOs !== null) && (
                    <div className="px-3 py-2 text-sm text-muted-foreground bg-muted/30">
                      IOP:{" "}
                      {iopOd !== null && `OD ${iopOd} mmHg`}
                      {iopOd !== null && iopOs !== null && " \u00B7 "}
                      {iopOs !== null && `OS ${iopOs} mmHg`}
                    </div>
                  )}
                </div>
              )}
            </>
          )}
        </div>

        {/* KTV note card */}
        <div className="bg-[#E6F1FB] border border-blue-200 rounded-md px-4 py-3">
          <div className="flex items-center gap-2 mb-1">
            <span className="text-sm font-bold text-blue-900">
              {LABELS.ktvNote}
            </span>
            <span className="text-xs text-blue-600">
              {new Date(visit.visitDate).toLocaleString("vi-VN")}
            </span>
          </div>
          <p className="text-sm text-blue-800">
            Các dịch vụ đã được thực hiện theo yêu cầu. Kết quả hình ảnh đã
            được tải lên.
          </p>
        </div>

        {/* Diagnosis section with change tracking */}
        <div className="space-y-3">
          <h3 className="text-sm font-semibold">{LABELS.diagnosisTitle}</h3>

          {/* Existing diagnoses from Stage 3 (teal) */}
          {existingDiagnoses.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {existingDiagnoses.map((d) => (
                <DiagnosisTag
                  key={d.id}
                  diagnosis={d}
                  variant="teal"
                  onRemove={() => handleRemoveDiagnosis(d.id)}
                />
              ))}
            </div>
          )}

          {/* New diagnoses added in 4b (amber with new badge) */}
          {newDiagnoses.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {newDiagnoses.map((d) => (
                <DiagnosisTag
                  key={d.id}
                  diagnosis={d}
                  variant="amber"
                  showNewBadge
                  onRemove={() => handleRemoveDiagnosis(d.id)}
                />
              ))}
            </div>
          )}

          {/* Change log */}
          {newDiagnoses.length > 0 && (
            <div className="border rounded-md p-3 bg-muted/30 space-y-1">
              <p className="text-xs font-medium text-muted-foreground">
                {LABELS.changeLog}
              </p>
              {newDiagnoses.map((d) => (
                <div key={d.id} className="flex items-center gap-2 text-xs">
                  <span className="w-2 h-2 rounded-full bg-green-500 shrink-0" />
                  <span>
                    Thêm: {d.icd10Code} -{" "}
                    {d.descriptionVi || d.descriptionEn}
                  </span>
                </div>
              ))}
            </div>
          )}

          {/* Add more diagnoses */}
          <Icd10Combobox
            doctorId={visit.doctorId}
            onSelect={handleAddDiagnosis}
            disabled={false}
          />
        </div>
      </div>
    </StageDetailShell>
  )
}
