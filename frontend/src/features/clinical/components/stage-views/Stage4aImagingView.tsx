import { useState, useCallback } from "react"
import { toast } from "sonner"
import {
  IconCheck,
  IconAlertTriangle,
  IconPlus,
  IconCircleCheck,
} from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Checkbox } from "@/shared/components/Checkbox"
import { StageDetailShell } from "../StageDetailShell"
import { StageBottomBar } from "../StageBottomBar"
import { ValidationMessage } from "../ValidationMessage"
import { ImageUploader } from "../ImageUploader"
import { ImageGallery } from "../ImageGallery"
import {
  useCompleteImaging,
  useVisitImages,
  type VisitDetailDto,
} from "../../api/clinical-api"

const LABELS = {
  stageName: "Chẩn đoán hình ảnh",
  saveDraft: "Lưu nháp",
  forwardButton: "Trả kết quả cho bác sĩ \u203A",
  pillActive: "Đang chụp",
  pillComplete: "Hoàn tất",
  referralTitle: "Yêu cầu từ BS.",
  noUploads: "Chưa có kết quả — tải lên ít nhất một hình ảnh hoặc kết quả để trả cho bác sĩ.",
  partialChecklist: "dịch vụ chưa hoàn tất. Bạn vẫn có thể trả kết quả ngay nếu bác sĩ đồng ý.",
  allComplete: "Tất cả dịch vụ hoàn tất — sẵn sàng trả kết quả cho bác sĩ.",
  notDone: "Chưa thực hiện",
  done: "Hoàn tất",
  addService: "+ Thêm dịch vụ",
}

// Default imaging services that may be requested
const DEFAULT_SERVICES = [
  { id: "OCT", label: "OCT", eyeScope: "Hai mắt" },
  { id: "IOP", label: "IOP (Nhãn áp)", eyeScope: "Hai mắt" },
  { id: "Fundus", label: "Fundus (Soi đáy mắt)", eyeScope: "Hai mắt" },
  { id: "Topography", label: "Topography", eyeScope: "Hai mắt" },
  { id: "Anterior", label: "Anterior Segment", eyeScope: "Hai mắt" },
]

interface Stage4aImagingViewProps {
  visit: VisitDetailDto
}

export function Stage4aImagingView({ visit }: Stage4aImagingViewProps) {
  // Track completed services locally (advisory only)
  const [completedServices, setCompletedServices] = useState<Set<string>>(
    new Set(),
  )

  const completeImagingMutation = useCompleteImaging()
  const { data: images } = useVisitImages(visit.id)

  const imageCount = images?.length ?? 0
  const hasUploads = imageCount > 0

  // For now, we use the default services list.
  // In the future, these would come from the imaging request data.
  const services = DEFAULT_SERVICES

  const incompletCount = services.filter(
    (s) => !completedServices.has(s.id),
  ).length

  // -- Handlers --

  const toggleServiceComplete = useCallback((serviceId: string) => {
    setCompletedServices((prev) => {
      const next = new Set(prev)
      if (next.has(serviceId)) {
        next.delete(serviceId)
      } else {
        next.add(serviceId)
      }
      return next
    })
  }, [])

  const handleCompleteImaging = useCallback(() => {
    if (!hasUploads) return
    completeImagingMutation.mutate(visit.id, {
      onSuccess: () => toast.success("Đã trả kết quả cho bác sĩ"),
      onError: () => toast.error("Không thể trả kết quả"),
    })
  }, [visit.id, hasUploads, completeImagingMutation])

  // -- Validation --

  function getValidationState(): {
    state: "error" | "warning" | "success"
    message: string
  } {
    if (!hasUploads)
      return { state: "error", message: LABELS.noUploads }
    if (incompletCount > 0)
      return {
        state: "warning",
        message: `${incompletCount} ${LABELS.partialChecklist}`,
      }
    return { state: "success", message: LABELS.allComplete }
  }

  // -- Stage pill --
  const stagePill = hasUploads
    ? { text: LABELS.pillComplete, variant: "green" as const }
    : { text: LABELS.pillActive, variant: "default" as const }

  // -- Validation message --
  const validationData = getValidationState()
  const validationMessage = (
    <ValidationMessage
      state={validationData.state}
      message={validationData.message}
    />
  )

  // -- Bottom bar --
  const bottomBar = (
    <StageBottomBar
      saveDraftButton={{
        label: LABELS.saveDraft,
        onClick: () => toast.success("Đã lưu nháp"),
      }}
      primaryButton={{
        label: LABELS.forwardButton,
        onClick: handleCompleteImaging,
        disabled: !hasUploads,
      }}
      validationMessage={validationMessage}
    />
  )

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
        {/* Referral banner */}
        <div className="bg-blue-50 border border-blue-200 rounded-md px-4 py-3">
          <p className="text-sm font-medium text-blue-900">
            {LABELS.referralTitle} {visit.doctorName}
          </p>
          <p className="text-sm text-blue-700 mt-1">
            Vui lòng thực hiện các dịch vụ theo yêu cầu bên dưới.
          </p>
        </div>

        {/* Service checklist */}
        <div className="space-y-2">
          <h3 className="text-sm font-semibold">Dịch vụ cần thực hiện</h3>
          <div className="space-y-1">
            {services.map((svc) => {
              const isComplete = completedServices.has(svc.id)
              return (
                <button
                  key={svc.id}
                  type="button"
                  className="flex items-center gap-3 w-full p-3 rounded-md border text-left hover:bg-muted/50 transition-colors"
                  onClick={() => toggleServiceComplete(svc.id)}
                >
                  <Checkbox
                    checked={isComplete}
                    onCheckedChange={() => toggleServiceComplete(svc.id)}
                  />
                  <div className="flex-1">
                    <span className="text-sm font-medium">{svc.label}</span>
                    <span className="text-xs text-muted-foreground ml-2">
                      {svc.eyeScope}
                    </span>
                  </div>
                  <Badge
                    variant="outline"
                    className={
                      isComplete
                        ? "bg-green-100 text-green-800 border-green-300"
                        : "text-muted-foreground"
                    }
                  >
                    {isComplete ? (
                      <>
                        <IconCircleCheck className="h-3 w-3 mr-1" />
                        {LABELS.done}
                      </>
                    ) : (
                      LABELS.notDone
                    )}
                  </Badge>
                </button>
              )
            })}
          </div>

          {/* Add service button */}
          <button
            type="button"
            className="flex items-center gap-2 w-full p-3 rounded-md border border-dashed text-muted-foreground hover:text-foreground hover:border-foreground/30 transition-colors"
          >
            <IconPlus className="h-4 w-4" />
            <span className="text-sm">{LABELS.addService}</span>
          </button>
        </div>

        {/* Upload section */}
        <div className="space-y-3">
          <h3 className="text-sm font-semibold">Kết quả hình ảnh</h3>
          <ImageUploader visitId={visit.id} />

          {/* Gallery */}
          {images && images.length > 0 && (
            <ImageGallery images={images} visitId={visit.id} />
          )}
        </div>
      </div>
    </StageDetailShell>
  )
}
