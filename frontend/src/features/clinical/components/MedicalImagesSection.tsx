import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconColumns } from "@tabler/icons-react"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { VisitSection } from "./VisitSection"
import { ImageUploader } from "./ImageUploader"
import { ImageGallery } from "./ImageGallery"
import { ImageComparison } from "./ImageComparison"
import { useVisitImages, useOsdiHistory } from "../api/clinical-api"
import { format } from "date-fns"

interface MedicalImagesSectionProps {
  visitId: string
  patientId: string
}

export function MedicalImagesSection({ visitId, patientId }: MedicalImagesSectionProps) {
  const { t } = useTranslation("clinical")
  const { data: images, isLoading } = useVisitImages(visitId)
  const { data: osdiHistory } = useOsdiHistory(patientId)
  const [compareOpen, setCompareOpen] = useState(false)

  const imageCount = images?.length ?? 0

  const visitOptions = (osdiHistory?.items ?? []).map((item) => ({
    id: item.visitId,
    date: item.visitDate,
    label: format(new Date(item.visitDate), "dd/MM/yyyy"),
  }))

  return (
    <VisitSection
      title={t("images.title")}
      defaultOpen={true}
      headerExtra={
        imageCount > 0 ? (
          <div className="flex items-center gap-2">
            <Badge variant="secondary" className="text-xs">
              {imageCount}
            </Badge>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setCompareOpen(true)}
            >
              <IconColumns className="h-4 w-4 mr-1" />
              {t("images.compare")}
            </Button>
          </div>
        ) : undefined
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

      <ImageComparison
        open={compareOpen}
        onClose={() => setCompareOpen(false)}
        patientId={patientId}
        visits={visitOptions}
        initialVisitId1={visitId}
      />
    </VisitSection>
  )
}
