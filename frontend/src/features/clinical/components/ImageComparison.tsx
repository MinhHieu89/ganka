import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { IconX } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/shared/components/Select"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import {
  type MedicalImageDto,
  ImageType,
  useImageComparison,
} from "../api/clinical-api"
import { ImageLightbox } from "./ImageLightbox"

interface VisitOption {
  id: string
  date: string
  label: string
}

interface ImageComparisonProps {
  open: boolean
  onClose: () => void
  patientId: string
  visits: VisitOption[]
  initialVisitId1?: string
  initialVisitId2?: string
  initialImageType?: number
}

const IMAGE_TYPES = [
  { value: ImageType.Fluorescein, key: "fluorescein" },
  { value: ImageType.Meibography, key: "meibography" },
  { value: ImageType.OCT, key: "oct" },
  { value: ImageType.SpecularMicroscopy, key: "specularMicroscopy" },
  { value: ImageType.Topography, key: "topography" },
  { value: ImageType.Video, key: "video" },
]

export function ImageComparison({
  open,
  onClose,
  patientId,
  visits,
  initialVisitId1,
  initialVisitId2,
  initialImageType,
}: ImageComparisonProps) {
  const { t } = useTranslation("clinical")
  const [visitId1, setVisitId1] = useState(initialVisitId1 ?? "")
  const [visitId2, setVisitId2] = useState(initialVisitId2 ?? "")
  const [imageType, setImageType] = useState<string>(
    initialImageType != null ? String(initialImageType) : "",
  )

  const [lightboxOpen, setLightboxOpen] = useState(false)
  const [lightboxImages, setLightboxImages] = useState<MedicalImageDto[]>([])
  const [lightboxIndex, setLightboxIndex] = useState(0)

  const imageTypeNum = imageType ? parseInt(imageType, 10) : undefined

  const { data: comparison, isLoading } = useImageComparison(
    patientId,
    visitId1 || undefined,
    visitId2 || undefined,
    imageTypeNum,
  )

  const visit1Label = useMemo(
    () => visits.find((v) => v.id === visitId1)?.label ?? "",
    [visits, visitId1],
  )
  const visit2Label = useMemo(
    () => visits.find((v) => v.id === visitId2)?.label ?? "",
    [visits, visitId2],
  )

  const handleImageClick = (images: MedicalImageDto[], index: number) => {
    setLightboxImages(images)
    setLightboxIndex(index)
    setLightboxOpen(true)
  }

  return (
    <>
      <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
        <DialogContent className="max-w-[95vw] w-full max-h-[95vh] h-full flex flex-col p-0">
          {/* Header with controls */}
          <DialogHeader className="flex flex-row items-center gap-3 px-4 py-3 border-b shrink-0">
            <DialogTitle className="text-base font-semibold shrink-0">
              {t("comparison.title")}
            </DialogTitle>

            <div className="flex items-center gap-3 flex-1 justify-center flex-wrap">
              {/* Visit 1 selector */}
              <Select value={visitId1} onValueChange={setVisitId1}>
                <SelectTrigger className="w-[200px]">
                  <SelectValue placeholder={t("comparison.selectVisit")} />
                </SelectTrigger>
                <SelectContent>
                  {visits.map((v) => (
                    <SelectItem key={v.id} value={v.id}>
                      {v.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              {/* Image type filter */}
              <Select value={imageType} onValueChange={setImageType}>
                <SelectTrigger className="w-[180px]">
                  <SelectValue placeholder={t("comparison.selectType")} />
                </SelectTrigger>
                <SelectContent>
                  {IMAGE_TYPES.map((type) => (
                    <SelectItem key={type.value} value={String(type.value)}>
                      {t(`images.types.${type.key}`)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              {/* Visit 2 selector */}
              <Select value={visitId2} onValueChange={setVisitId2}>
                <SelectTrigger className="w-[200px]">
                  <SelectValue placeholder={t("comparison.selectVisit")} />
                </SelectTrigger>
                <SelectContent>
                  {visits.map((v) => (
                    <SelectItem key={v.id} value={v.id}>
                      {v.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <Button
              variant="ghost"
              size="sm"
              className="h-8 w-8 p-0 shrink-0"
              onClick={onClose}
            >
              <IconX className="h-4 w-4" />
            </Button>
          </DialogHeader>

          {/* Two-panel comparison area */}
          <div className="flex-1 flex overflow-hidden min-h-0">
            {/* Left panel - Visit 1 */}
            <div className="flex-1 border-r overflow-y-auto p-4">
              <h3 className="text-sm font-medium text-muted-foreground mb-3">
                {visit1Label || t("comparison.visit1")}
              </h3>
              <ComparisonPanel
                images={comparison?.visit1Images ?? []}
                isLoading={isLoading && !!visitId1 && !!visitId2 && !!imageType}
                noSelections={!visitId1 || !visitId2 || !imageType}
                onImageClick={(idx) =>
                  handleImageClick(comparison?.visit1Images ?? [], idx)
                }
                t={t}
              />
            </div>

            {/* Right panel - Visit 2 */}
            <div className="flex-1 overflow-y-auto p-4">
              <h3 className="text-sm font-medium text-muted-foreground mb-3">
                {visit2Label || t("comparison.visit2")}
              </h3>
              <ComparisonPanel
                images={comparison?.visit2Images ?? []}
                isLoading={isLoading && !!visitId1 && !!visitId2 && !!imageType}
                noSelections={!visitId1 || !visitId2 || !imageType}
                onImageClick={(idx) =>
                  handleImageClick(comparison?.visit2Images ?? [], idx)
                }
                t={t}
              />
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <ImageLightbox
        open={lightboxOpen}
        onClose={() => setLightboxOpen(false)}
        images={lightboxImages}
        index={lightboxIndex}
      />
    </>
  )
}

// Reusable panel for each side of the comparison
function ComparisonPanel({
  images,
  isLoading,
  noSelections,
  onImageClick,
  t,
}: {
  images: MedicalImageDto[]
  isLoading: boolean
  noSelections: boolean
  onImageClick: (index: number) => void
  t: (key: string) => string
}) {
  if (noSelections) {
    return (
      <p className="text-sm text-muted-foreground text-center py-8">
        {t("comparison.selectVisit")}
      </p>
    )
  }

  if (isLoading) {
    return (
      <div className="grid grid-cols-2 gap-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <div key={i} className="aspect-square bg-muted animate-pulse rounded-md" />
        ))}
      </div>
    )
  }

  if (images.length === 0) {
    return (
      <p className="text-sm text-muted-foreground text-center py-8">
        {t("comparison.noImages")}
      </p>
    )
  }

  return (
    <div className="grid grid-cols-2 gap-2">
      {images.map((img, idx) => (
        <button
          key={img.id}
          type="button"
          className="aspect-square rounded-md overflow-hidden border bg-muted cursor-pointer hover:ring-2 hover:ring-primary transition-all"
          onClick={() => onImageClick(idx)}
        >
          <img
            src={img.url}
            alt={img.fileName}
            className="w-full h-full object-cover"
            loading="lazy"
          />
        </button>
      ))}
    </div>
  )
}
