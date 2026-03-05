import { useState, useCallback, useRef } from "react"
import { useTranslation } from "react-i18next"
import { IconUpload, IconLoader2, IconX } from "@tabler/icons-react"
import { toast } from "sonner"
import { Button } from "@/shared/components/Button"
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/shared/components/Select"
import {
  useUploadImage,
  ImageType,
  EyeTag,
  ALLOWED_VIDEO_TYPES,
  ALL_ALLOWED_TYPES,
  MAX_IMAGE_SIZE,
  MAX_VIDEO_SIZE,
  MAX_FILES_PER_BATCH,
} from "../api/clinical-api"

const ACCEPT_STRING = [
  "image/jpeg", "image/png", "image/tiff", "image/bmp", "image/webp",
  "video/mp4", "video/quicktime", "video/x-msvideo",
  ".jpg", ".jpeg", ".png", ".tiff", ".tif", ".bmp", ".webp",
  ".mp4", ".mov", ".avi",
].join(",")

interface ImageUploaderProps {
  visitId: string
}

export function ImageUploader({ visitId }: ImageUploaderProps) {
  const { t } = useTranslation("clinical")
  const uploadMutation = useUploadImage(visitId)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const [selectedType, setSelectedType] = useState<string>("")
  const [selectedEyeTag, setSelectedEyeTag] = useState<string>("")
  const [isDragOver, setIsDragOver] = useState(false)
  const [uploadingCount, setUploadingCount] = useState(0)
  const [uploadedCount, setUploadedCount] = useState(0)
  const [totalCount, setTotalCount] = useState(0)

  const isUploading = uploadingCount > 0

  const validateFile = useCallback(
    (file: File): string | null => {
      if (!ALL_ALLOWED_TYPES.includes(file.type)) {
        return t("images.invalidType", { name: file.name })
      }
      const isVideo = ALLOWED_VIDEO_TYPES.includes(file.type)
      const maxSize = isVideo ? MAX_VIDEO_SIZE : MAX_IMAGE_SIZE
      if (file.size > maxSize) {
        const limitMb = maxSize / (1024 * 1024)
        return t("images.fileTooLarge", { name: file.name, limit: limitMb })
      }
      return null
    },
    [t],
  )

  const processFiles = useCallback(
    async (files: File[]) => {
      if (!selectedType) {
        toast.error(t("images.selectTypeFirst"))
        return
      }

      if (files.length > MAX_FILES_PER_BATCH) {
        toast.error(t("images.tooManyFiles", { max: MAX_FILES_PER_BATCH }))
        return
      }

      // Validate all files first
      const errors: string[] = []
      const validFiles: File[] = []
      for (const file of files) {
        const err = validateFile(file)
        if (err) {
          errors.push(err)
        } else {
          validFiles.push(file)
        }
      }

      if (errors.length > 0) {
        errors.forEach((e) => toast.error(e))
      }

      if (validFiles.length === 0) return

      const imageType = parseInt(selectedType, 10)
      const eyeTag = selectedEyeTag ? parseInt(selectedEyeTag, 10) : null

      setUploadingCount(validFiles.length)
      setUploadedCount(0)
      setTotalCount(validFiles.length)

      let successCount = 0
      let failCount = 0

      for (const file of validFiles) {
        try {
          await uploadMutation.mutateAsync({
            file,
            imageType,
            eyeTag,
          })
          successCount++
          setUploadedCount((prev) => prev + 1)
        } catch {
          failCount++
          toast.error(t("images.uploadFailed", { name: file.name }))
        }
      }

      setUploadingCount(0)
      setUploadedCount(0)
      setTotalCount(0)

      if (successCount > 0) {
        toast.success(
          t("images.uploadSuccess", { count: successCount }),
        )
      }
      if (failCount > 0 && successCount > 0) {
        toast.warning(t("images.partialUpload", { failed: failCount }))
      }

      // Reset file input
      if (fileInputRef.current) {
        fileInputRef.current.value = ""
      }
    },
    [selectedType, selectedEyeTag, uploadMutation, validateFile, t],
  )

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault()
      setIsDragOver(false)
      const files = Array.from(e.dataTransfer.files)
      processFiles(files)
    },
    [processFiles],
  )

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    setIsDragOver(true)
  }, [])

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    setIsDragOver(false)
  }, [])

  const handleFileSelect = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = Array.from(e.target.files ?? [])
      if (files.length > 0) {
        processFiles(files)
      }
    },
    [processFiles],
  )

  const imageTypeOptions = [
    { value: String(ImageType.Fluorescein), label: t("images.types.fluorescein") },
    { value: String(ImageType.Meibography), label: t("images.types.meibography") },
    { value: String(ImageType.OCT), label: t("images.types.oct") },
    { value: String(ImageType.SpecularMicroscopy), label: t("images.types.specularMicroscopy") },
    { value: String(ImageType.Topography), label: t("images.types.topography") },
    { value: String(ImageType.Video), label: t("images.types.video") },
  ]

  const eyeTagOptions = [
    { value: String(EyeTag.OD), label: t("images.eyeTags.od") },
    { value: String(EyeTag.OS), label: t("images.eyeTags.os") },
    { value: String(EyeTag.OU), label: t("images.eyeTags.ou") },
  ]

  return (
    <div className="space-y-3">
      {/* Selectors */}
      <div className="flex flex-wrap items-end gap-3">
        <div className="space-y-1">
          <label className="text-xs font-medium text-muted-foreground">
            {t("images.imageType")} *
          </label>
          <Select value={selectedType} onValueChange={setSelectedType}>
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder={t("images.selectType")} />
            </SelectTrigger>
            <SelectContent>
              {imageTypeOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>
                  {opt.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-1">
          <label className="text-xs font-medium text-muted-foreground">
            {t("images.eyeTag")}
          </label>
          <div className="flex items-center gap-1">
            <Select value={selectedEyeTag} onValueChange={setSelectedEyeTag}>
              <SelectTrigger className="w-[120px]">
                <SelectValue placeholder={t("images.noEyeTag")} />
              </SelectTrigger>
              <SelectContent>
                {eyeTagOptions.map((opt) => (
                  <SelectItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {selectedEyeTag && (
              <Button
                variant="ghost"
                size="sm"
                className="h-8 w-8 p-0"
                onClick={() => setSelectedEyeTag("")}
              >
                <IconX className="h-3 w-3" />
              </Button>
            )}
          </div>
        </div>
      </div>

      {/* Drop zone */}
      <div
        className={`relative border-2 border-dashed rounded-lg p-6 text-center transition-colors ${
          isDragOver
            ? "border-primary bg-primary/5"
            : "border-muted-foreground/25 hover:border-muted-foreground/50"
        } ${isUploading ? "pointer-events-none opacity-60" : "cursor-pointer"}`}
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onClick={() => !isUploading && fileInputRef.current?.click()}
      >
        <input
          ref={fileInputRef}
          type="file"
          multiple
          accept={ACCEPT_STRING}
          onChange={handleFileSelect}
          className="hidden"
        />

        {isUploading ? (
          <div className="flex flex-col items-center gap-2">
            <IconLoader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-sm font-medium">
              {t("images.uploading", {
                current: uploadedCount + 1,
                total: totalCount,
              })}
            </p>
            <div className="w-48 h-2 bg-muted rounded-full overflow-hidden">
              <div
                className="h-full bg-primary transition-all duration-300"
                style={{
                  width: `${totalCount > 0 ? (uploadedCount / totalCount) * 100 : 0}%`,
                }}
              />
            </div>
          </div>
        ) : (
          <div className="flex flex-col items-center gap-2">
            <IconUpload className="h-8 w-8 text-muted-foreground" />
            <p className="text-sm font-medium">
              {t("images.dropOrClick")}
            </p>
            <p className="text-xs text-muted-foreground">
              {t("images.acceptedFormats")}
            </p>
          </div>
        )}
      </div>
    </div>
  )
}
