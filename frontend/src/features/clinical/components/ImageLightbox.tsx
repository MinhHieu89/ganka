import Lightbox from "yet-another-react-lightbox"
import Zoom from "yet-another-react-lightbox/plugins/zoom"
import Video from "yet-another-react-lightbox/plugins/video"
import Fullscreen from "yet-another-react-lightbox/plugins/fullscreen"
import "yet-another-react-lightbox/styles.css"
import type { MedicalImageDto } from "../api/clinical-api"
import { ALLOWED_VIDEO_TYPES } from "../api/clinical-api"

interface ImageLightboxProps {
  open: boolean
  onClose: () => void
  images: MedicalImageDto[]
  index: number
}

function isVideoFile(contentType: string): boolean {
  return ALLOWED_VIDEO_TYPES.includes(contentType)
}

export function ImageLightbox({
  open,
  onClose,
  images,
  index,
}: ImageLightboxProps) {
  const slides = images.map((img) => {
    if (isVideoFile(img.contentType)) {
      return {
        type: "video" as const,
        sources: [{ src: img.url, type: img.contentType }],
        poster: undefined,
      }
    }
    return {
      src: img.url,
      alt: img.fileName,
    }
  })

  return (
    <Lightbox
      open={open}
      close={onClose}
      index={index}
      slides={slides}
      plugins={[Zoom, Video, Fullscreen]}
      zoom={{ maxZoomPixelRatio: 5 }}
      carousel={{ finite: false }}
      animation={{ fade: 250 }}
    />
  )
}
