import { useRef, useState } from "react"
import {
  IconUpload,
  IconFile,
  IconPhoto,
  IconExternalLink,
  IconLoader2,
  IconX,
} from "@tabler/icons-react"
import { toast } from "sonner"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { useUploadWarrantyDocument } from "@/features/optical/api/optical-queries"

const MAX_FILE_SIZE = 10 * 1024 * 1024 // 10 MB
const ACCEPTED_TYPES = ["image/jpeg", "image/png", "application/pdf"]
const ACCEPTED_EXTENSIONS = ".jpg,.jpeg,.png,.pdf"

interface WarrantyDocumentUploadProps {
  claimId: string
  existingDocuments: string[]
  readonly?: boolean
}

function isImageUrl(url: string): boolean {
  const lower = url.toLowerCase()
  return lower.includes(".jpg") || lower.includes(".jpeg") || lower.includes(".png")
}

function getFileName(url: string): string {
  try {
    const parts = new URL(url).pathname.split("/")
    return decodeURIComponent(parts[parts.length - 1] ?? url)
  } catch {
    const parts = url.split("/")
    return parts[parts.length - 1] ?? url
  }
}

function DocumentThumbnail({ url }: { url: string }) {
  const isImage = isImageUrl(url)
  const fileName = getFileName(url)

  return (
    <a
      href={url}
      target="_blank"
      rel="noopener noreferrer"
      className="group flex items-center gap-2 rounded-md border bg-background px-3 py-2 text-sm hover:bg-muted transition-colors"
    >
      {isImage ? (
        <IconPhoto className="h-4 w-4 text-blue-500 shrink-0" />
      ) : (
        <IconFile className="h-4 w-4 text-red-500 shrink-0" />
      )}
      <span className="flex-1 truncate text-muted-foreground group-hover:text-foreground">
        {fileName}
      </span>
      <IconExternalLink className="h-3 w-3 text-muted-foreground shrink-0" />
    </a>
  )
}

function ImagePreview({
  file,
  onRemove,
}: {
  file: File
  onRemove: () => void
}) {
  const previewUrl = URL.createObjectURL(file)
  return (
    <div className="relative group inline-block">
      <img
        src={previewUrl}
        alt={file.name}
        className="h-20 w-20 rounded-md border object-cover"
        onLoad={() => URL.revokeObjectURL(previewUrl)}
      />
      <button
        type="button"
        onClick={onRemove}
        className="absolute -top-1.5 -right-1.5 hidden group-hover:flex items-center justify-center h-5 w-5 rounded-full bg-red-500 text-white"
      >
        <IconX className="h-3 w-3" />
      </button>
      <div className="mt-1 text-xs text-muted-foreground max-w-[80px] truncate">{file.name}</div>
    </div>
  )
}

export function WarrantyDocumentUpload({
  claimId,
  existingDocuments,
  readonly = false,
}: WarrantyDocumentUploadProps) {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [selectedFiles, setSelectedFiles] = useState<File[]>([])
  const [uploadingIndex, setUploadingIndex] = useState<number | null>(null)
  const [uploadedDocuments, setUploadedDocuments] = useState<string[]>([])

  const uploadMutation = useUploadWarrantyDocument()

  const allDocuments = [...existingDocuments, ...uploadedDocuments]

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? [])
    const validFiles: File[] = []

    for (const file of files) {
      if (!ACCEPTED_TYPES.includes(file.type)) {
        toast.error(`${file.name}: Only JPG, PNG and PDF files are accepted.`)
        continue
      }
      if (file.size > MAX_FILE_SIZE) {
        toast.error(`${file.name}: File size must not exceed 10MB.`)
        continue
      }
      validFiles.push(file)
    }

    setSelectedFiles((prev) => [...prev, ...validFiles])
    // Reset input so the same file can be selected again
    if (fileInputRef.current) {
      fileInputRef.current.value = ""
    }
  }

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    const files = Array.from(e.dataTransfer.files)
    const validFiles: File[] = []

    for (const file of files) {
      if (!ACCEPTED_TYPES.includes(file.type)) {
        toast.error(`${file.name}: Only JPG, PNG and PDF files are accepted.`)
        continue
      }
      if (file.size > MAX_FILE_SIZE) {
        toast.error(`${file.name}: File size must not exceed 10MB.`)
        continue
      }
      validFiles.push(file)
    }

    setSelectedFiles((prev) => [...prev, ...validFiles])
  }

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault()
  }

  const handleRemoveSelected = (index: number) => {
    setSelectedFiles((prev) => prev.filter((_, i) => i !== index))
  }

  const handleUploadAll = async () => {
    for (let i = 0; i < selectedFiles.length; i++) {
      const file = selectedFiles[i]
      if (!file) continue
      setUploadingIndex(i)
      try {
        const result = await uploadMutation.mutateAsync({ id: claimId, file })
        setUploadedDocuments((prev) => [...prev, result.documentUrl])
        toast.success(`${file.name} uploaded successfully.`)
      } catch {
        // error handled by mutation
      }
    }
    setSelectedFiles([])
    setUploadingIndex(null)
  }

  const isUploading = uploadMutation.isPending

  return (
    <div className="space-y-4">
      {/* Existing documents list */}
      {allDocuments.length > 0 && (
        <div className="space-y-2">
          <p className="text-sm font-medium text-muted-foreground">
            {allDocuments.length} document{allDocuments.length !== 1 ? "s" : ""} attached
          </p>
          <div className="space-y-1.5">
            {allDocuments.map((url, index) => (
              <DocumentThumbnail key={`${url}-${index}`} url={url} />
            ))}
          </div>
        </div>
      )}

      {/* Upload area (hidden in readonly mode) */}
      {!readonly && (
        <>
          {/* Drag & drop zone */}
          <div
            onDrop={handleDrop}
            onDragOver={handleDragOver}
            onClick={() => fileInputRef.current?.click()}
            className="flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed border-muted-foreground/25 bg-muted/10 px-6 py-8 text-center hover:border-muted-foreground/40 hover:bg-muted/20 transition-colors"
          >
            <IconUpload className="h-8 w-8 text-muted-foreground mb-2" />
            <p className="text-sm font-medium">
              Drag & drop files here, or click to browse
            </p>
            <p className="text-xs text-muted-foreground mt-1">
              Accepted: JPG, PNG, PDF — Max 10 MB per file
            </p>
            <Input
              ref={fileInputRef}
              type="file"
              accept={ACCEPTED_EXTENSIONS}
              multiple
              className="hidden"
              onChange={handleFileChange}
            />
          </div>

          {/* Selected file previews */}
          {selectedFiles.length > 0 && (
            <div className="space-y-3">
              <p className="text-sm font-medium">
                {selectedFiles.length} file{selectedFiles.length !== 1 ? "s" : ""} selected
              </p>
              <div className="flex flex-wrap gap-3">
                {selectedFiles.map((file, index) =>
                  file.type.startsWith("image/") ? (
                    <ImagePreview
                      key={`${file.name}-${index}`}
                      file={file}
                      onRemove={() => handleRemoveSelected(index)}
                    />
                  ) : (
                    <div
                      key={`${file.name}-${index}`}
                      className="group relative flex items-center gap-2 rounded-md border bg-muted/20 px-3 py-2"
                    >
                      <IconFile className="h-4 w-4 text-red-500 shrink-0" />
                      <span className="text-sm max-w-[120px] truncate">{file.name}</span>
                      <button
                        type="button"
                        onClick={() => handleRemoveSelected(index)}
                        className="ml-1 text-muted-foreground hover:text-red-500"
                      >
                        <IconX className="h-3 w-3" />
                      </button>
                    </div>
                  ),
                )}
              </div>

              {/* Upload progress / button */}
              <Button
                type="button"
                onClick={handleUploadAll}
                disabled={isUploading}
                size="sm"
              >
                {isUploading ? (
                  <>
                    <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
                    Uploading {uploadingIndex !== null ? uploadingIndex + 1 : ""}
                    /{selectedFiles.length}...
                  </>
                ) : (
                  <>
                    <IconUpload className="h-4 w-4 mr-2" />
                    Upload {selectedFiles.length} file
                    {selectedFiles.length !== 1 ? "s" : ""}
                  </>
                )}
              </Button>
            </div>
          )}
        </>
      )}

      {/* Empty state in readonly */}
      {readonly && allDocuments.length === 0 && (
        <p className="text-sm text-muted-foreground italic">No documents attached.</p>
      )}
    </div>
  )
}
