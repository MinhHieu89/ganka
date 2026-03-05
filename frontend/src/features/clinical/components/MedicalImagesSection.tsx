import { useTranslation } from "react-i18next"
import { Badge } from "@/shared/components/Badge"
import { Skeleton } from "@/shared/components/Skeleton"
import { VisitSection } from "./VisitSection"
import { ImageUploader } from "./ImageUploader"
import { ImageGallery } from "./ImageGallery"
import { useVisitImages } from "../api/clinical-api"

interface MedicalImagesSectionProps {
  visitId: string
}

export function MedicalImagesSection({ visitId }: MedicalImagesSectionProps) {
  const { t } = useTranslation("clinical")
  const { data: images, isLoading } = useVisitImages(visitId)

  const imageCount = images?.length ?? 0

  return (
    <VisitSection
      title={t("images.title")}
      defaultOpen={true}
      headerExtra={
        imageCount > 0 ? (
          <Badge variant="secondary" className="text-xs">
            {imageCount}
          </Badge>
        ) : null
      }
    >
      <div className="space-y-4">
        <ImageUploader visitId={visitId} />

        {isLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-8 w-full" />
            <div className="grid grid-cols-4 gap-2">
              <Skeleton className="aspect-square" />
              <Skeleton className="aspect-square" />
              <Skeleton className="aspect-square" />
              <Skeleton className="aspect-square" />
            </div>
          </div>
        ) : (
          <ImageGallery images={images ?? []} visitId={visitId} />
        )}
      </div>
    </VisitSection>
  )
}
