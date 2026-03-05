import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { IconTrash, IconPlayerPlay } from "@tabler/icons-react"
import { toast } from "sonner"
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/shared/components/Tabs"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/components/AlertDialog"
import {
  type MedicalImageDto,
  ImageType,
  EYE_TAG_LABELS,
  ALLOWED_VIDEO_TYPES,
  useDeleteImage,
} from "../api/clinical-api"
import { ImageLightbox } from "./ImageLightbox"

interface ImageGalleryProps {
  images: MedicalImageDto[]
  visitId: string
}

const IMAGE_TYPES = [
  { value: ImageType.Fluorescein, key: "fluorescein" },
  { value: ImageType.Meibography, key: "meibography" },
  { value: ImageType.OCT, key: "oct" },
  { value: ImageType.SpecularMicroscopy, key: "specularMicroscopy" },
  { value: ImageType.Topography, key: "topography" },
  { value: ImageType.Video, key: "video" },
]

function isVideoFile(contentType: string): boolean {
  return ALLOWED_VIDEO_TYPES.includes(contentType)
}

export function ImageGallery({ images, visitId }: ImageGalleryProps) {
  const { t, i18n } = useTranslation("clinical")
  const deleteMutation = useDeleteImage()

  const [lightboxOpen, setLightboxOpen] = useState(false)
  const [lightboxIndex, setLightboxIndex] = useState(0)
  const [lightboxImages, setLightboxImages] = useState<MedicalImageDto[]>([])

  // Group images by type
  const grouped = useMemo(() => {
    const map = new Map<number, MedicalImageDto[]>()
    for (const type of IMAGE_TYPES) {
      map.set(type.value, [])
    }
    for (const img of images) {
      const arr = map.get(img.type)
      if (arr) {
        arr.push(img)
      }
    }
    return map
  }, [images])

  // Find the first non-empty tab as default, or the first tab
  const defaultTab = useMemo(() => {
    for (const type of IMAGE_TYPES) {
      const imgs = grouped.get(type.value)
      if (imgs && imgs.length > 0) return String(type.value)
    }
    return String(IMAGE_TYPES[0].value)
  }, [grouped])

  const handleOpenLightbox = (typeImages: MedicalImageDto[], index: number) => {
    setLightboxImages(typeImages)
    setLightboxIndex(index)
    setLightboxOpen(true)
  }

  const handleDelete = async (imageId: string) => {
    try {
      await deleteMutation.mutateAsync({ imageId, visitId })
      toast.success(t("images.deleteSuccess"))
    } catch {
      toast.error(t("images.deleteFailed"))
    }
  }

  const lang = i18n.language === "vi" ? "vi" : "en"

  if (images.length === 0) {
    return (
      <p className="text-sm text-muted-foreground py-2">
        {t("images.noImages")}
      </p>
    )
  }

  return (
    <>
      <Tabs defaultValue={defaultTab}>
        <TabsList className="flex-wrap h-auto gap-1">
          {IMAGE_TYPES.map((type) => {
            const count = grouped.get(type.value)?.length ?? 0
            return (
              <TabsTrigger
                key={type.value}
                value={String(type.value)}
                className="gap-1.5"
              >
                {t(`images.types.${type.key}`)}
                {count > 0 && (
                  <Badge variant="secondary" className="text-[10px] px-1.5 py-0 min-w-[18px]">
                    {count}
                  </Badge>
                )}
              </TabsTrigger>
            )
          })}
        </TabsList>

        {IMAGE_TYPES.map((type) => {
          const typeImages = grouped.get(type.value) ?? []
          return (
            <TabsContent key={type.value} value={String(type.value)}>
              {typeImages.length === 0 ? (
                <p className="text-sm text-muted-foreground py-4 text-center">
                  {t("images.noImagesOfType", {
                    type: t(`images.types.${type.key}`),
                  })}
                </p>
              ) : (
                <div className="grid grid-cols-[repeat(auto-fill,minmax(120px,1fr))] gap-2 mt-2">
                  {typeImages.map((img, idx) => {
                    const isVideo = isVideoFile(img.contentType)
                    const eyeLabel = img.eyeTag != null
                      ? EYE_TAG_LABELS[img.eyeTag]?.[lang] ?? ""
                      : null
                    return (
                      <div
                        key={img.id}
                        className="group relative rounded-md overflow-hidden border bg-muted"
                      >
                        {/* Thumbnail */}
                        <button
                          type="button"
                          className="block w-full aspect-square cursor-pointer"
                          onClick={() => handleOpenLightbox(typeImages, idx)}
                        >
                          {isVideo ? (
                            <div className="w-full h-full flex items-center justify-center bg-muted">
                              <IconPlayerPlay className="h-8 w-8 text-muted-foreground" />
                            </div>
                          ) : (
                            <img
                              src={img.url}
                              alt={img.fileName}
                              className="w-full h-full object-cover"
                              loading="lazy"
                            />
                          )}
                        </button>

                        {/* Badges overlay */}
                        {eyeLabel && (
                          <Badge
                            variant="secondary"
                            className="absolute top-1 left-1 text-[10px] px-1 py-0"
                          >
                            {eyeLabel}
                          </Badge>
                        )}

                        {/* Delete button */}
                        <AlertDialog>
                          <AlertDialogTrigger asChild>
                            <Button
                              variant="destructive"
                              size="sm"
                              className="absolute top-1 right-1 h-6 w-6 p-0 opacity-0 group-hover:opacity-100 transition-opacity"
                            >
                              <IconTrash className="h-3 w-3" />
                            </Button>
                          </AlertDialogTrigger>
                          <AlertDialogContent>
                            <AlertDialogHeader>
                              <AlertDialogTitle>
                                {t("images.deleteConfirmTitle")}
                              </AlertDialogTitle>
                              <AlertDialogDescription>
                                {t("images.deleteConfirmDesc")}
                              </AlertDialogDescription>
                            </AlertDialogHeader>
                            <AlertDialogFooter>
                              <AlertDialogCancel>
                                {t("images.cancel")}
                              </AlertDialogCancel>
                              <AlertDialogAction
                                onClick={() => handleDelete(img.id)}
                              >
                                {t("images.confirmDelete")}
                              </AlertDialogAction>
                            </AlertDialogFooter>
                          </AlertDialogContent>
                        </AlertDialog>

                        {/* File name */}
                        <p className="text-[10px] text-muted-foreground truncate px-1 py-0.5">
                          {img.fileName}
                        </p>
                      </div>
                    )
                  })}
                </div>
              )}
            </TabsContent>
          )
        })}
      </Tabs>

      <ImageLightbox
        open={lightboxOpen}
        onClose={() => setLightboxOpen(false)}
        images={lightboxImages}
        index={lightboxIndex}
      />
    </>
  )
}
