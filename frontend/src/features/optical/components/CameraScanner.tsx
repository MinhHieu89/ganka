import { useEffect, useRef, useState } from "react"
import { Html5QrcodeScanner, Html5QrcodeSupportedFormats } from "html5-qrcode"
import { IconCamera, IconCameraOff } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/Card"

interface CameraScannerProps {
  onScan: (barcode: string) => void
  onError?: (error: string) => void
  isActive?: boolean
}

const SCANNER_ELEMENT_ID = "optical-barcode-reader"

export function CameraScanner({
  onScan,
  onError,
  isActive: externalIsActive,
}: CameraScannerProps) {
  const scannerRef = useRef<Html5QrcodeScanner | null>(null)
  const [internalActive, setInternalActive] = useState(false)

  // If the parent controls activation via prop, use that; otherwise use internal state
  const isActive = externalIsActive !== undefined ? externalIsActive : internalActive

  useEffect(() => {
    if (!isActive) {
      // Clean up if scanner is running
      if (scannerRef.current) {
        scannerRef.current.clear().catch(() => {})
        scannerRef.current = null
      }
      return
    }

    const scanner = new Html5QrcodeScanner(
      SCANNER_ELEMENT_ID,
      {
        fps: 10,
        qrbox: { width: 300, height: 100 },
        formatsToSupport: [Html5QrcodeSupportedFormats.EAN_13],
        rememberLastUsedCamera: true,
      },
      false
    )

    scanner.render(
      (decodedText) => {
        onScan(decodedText)
        scanner.clear().catch(() => {})
        scannerRef.current = null
        setInternalActive(false)
      },
      (errorMessage) => {
        onError?.(errorMessage)
      }
    )

    scannerRef.current = scanner

    return () => {
      scanner.clear().catch(() => {})
      scannerRef.current = null
    }
  }, [isActive, onScan, onError])

  const handleToggle = () => {
    setInternalActive((prev) => !prev)
  }

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <div>
            <CardTitle className="text-sm font-medium">Camera Scanner</CardTitle>
            <CardDescription className="text-xs">
              {isActive ? "Point camera at barcode" : "Use camera to scan barcodes"}
            </CardDescription>
          </div>
          {externalIsActive === undefined && (
            <Button
              variant={isActive ? "destructive" : "outline"}
              size="sm"
              onClick={handleToggle}
            >
              {isActive ? (
                <>
                  <IconCameraOff className="mr-1 h-4 w-4" />
                  Stop
                </>
              ) : (
                <>
                  <IconCamera className="mr-1 h-4 w-4" />
                  Start Camera
                </>
              )}
            </Button>
          )}
        </div>
      </CardHeader>
      <CardContent>
        <div
          id={SCANNER_ELEMENT_ID}
          className="min-h-[200px]"
          style={{ display: isActive ? "block" : "none" }}
        />
        {!isActive && (
          <div className="text-muted-foreground flex min-h-[100px] items-center justify-center text-sm">
            Camera is off. Click "Start Camera" to begin scanning.
          </div>
        )}
      </CardContent>
    </Card>
  )
}
