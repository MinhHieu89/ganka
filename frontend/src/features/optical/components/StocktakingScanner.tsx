import { useRef, useState } from "react"
import { useTranslation } from "react-i18next"
import { IconBarcode, IconCheck, IconX } from "@tabler/icons-react"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/Tabs"
import { NumberInput } from "@/shared/components/NumberInput"
import { Button } from "@/shared/components/Button"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { BarcodeScannerInput } from "./BarcodeScannerInput"
import { CameraScanner } from "./CameraScanner"
import { useRecordStocktakingItem } from "../api/optical-queries"
import type { StocktakingItemDto } from "../api/optical-api"
import { cn } from "@/shared/lib/utils"

interface StocktakingScannerProps {
  sessionId: string
  onItemRecorded?: (item: StocktakingItemDto) => void
}

interface ScannedItem {
  id: string
  barcode: string
  frameName: string | null
  physicalCount: number
  systemCount: number
  discrepancy: number
  recordedAt: string
}

export function StocktakingScanner({ sessionId, onItemRecorded }: StocktakingScannerProps) {
  const { t } = useTranslation("optical")
  const [scannedBarcode, setScannedBarcode] = useState("")
  const [physicalCount, setPhysicalCount] = useState<number>(1)
  const [recentItems, setRecentItems] = useState<ScannedItem[]>([])
  const physicalCountRef = useRef<HTMLInputElement>(null)

  const recordItem = useRecordStocktakingItem()

  const handleBarcodeScanned = (barcode: string) => {
    setScannedBarcode(barcode)
    // Auto-focus physical count after barcode scan
    setTimeout(() => {
      physicalCountRef.current?.focus()
      physicalCountRef.current?.select()
    }, 50)
  }

  const handleSubmit = async () => {
    if (!scannedBarcode || physicalCount < 0) return

    try {
      await recordItem.mutateAsync(
        { sessionId, barcode: scannedBarcode, physicalCount },
        {
          onSuccess: () => {
            // Optimistically add to recent items list (system count unknown until refresh)
            const newItem: ScannedItem = {
              id: crypto.randomUUID(),
              barcode: scannedBarcode,
              frameName: null,
              physicalCount,
              systemCount: 0,
              discrepancy: physicalCount,
              recordedAt: new Date().toISOString(),
            }
            setRecentItems((prev) => [newItem, ...prev].slice(0, 10))
            if (onItemRecorded) {
              onItemRecorded(newItem as unknown as StocktakingItemDto)
            }
            setScannedBarcode("")
            setPhysicalCount(1)
          },
        },
      )
    } catch {
      // Error handled by mutation's onError toast
    }
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault()
      handleSubmit()
    }
  }

  const getDiscrepancyColor = (discrepancy: number) => {
    if (discrepancy === 0) return "text-green-600"
    if (discrepancy > 0) return "text-yellow-600"
    return "text-red-600"
  }

  const getDiscrepancyBadge = (discrepancy: number) => {
    if (discrepancy === 0) return "default"
    if (discrepancy > 0) return "secondary"
    return "destructive"
  }

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="flex items-center gap-2 text-base">
            <IconBarcode className="h-5 w-5" />
            {t("stocktaking.scanBarcode")}
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Tabs defaultValue="usb">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="usb">USB</TabsTrigger>
              <TabsTrigger value="camera">Camera</TabsTrigger>
            </TabsList>

            <TabsContent value="usb" className="space-y-3 pt-3">
              <div>
                <label className="mb-1.5 block text-sm font-medium">{t("stocktaking.barcode")}</label>
                <BarcodeScannerInput
                  onScan={handleBarcodeScanned}
                  autoFocus
                />
                {scannedBarcode && (
                  <p className="text-muted-foreground mt-1 text-xs">
                    Scanned: <span className="font-mono font-medium text-foreground">{scannedBarcode}</span>
                  </p>
                )}
              </div>
            </TabsContent>

            <TabsContent value="camera" className="space-y-3 pt-3">
              <CameraScanner onScan={handleBarcodeScanned} />
              {scannedBarcode && (
                <p className="text-muted-foreground mt-1 text-xs">
                  Scanned: <span className="font-mono font-medium text-foreground">{scannedBarcode}</span>
                </p>
              )}
            </TabsContent>
          </Tabs>

          <div className="flex gap-3">
            <div className="flex-1">
              <label className="mb-1.5 block text-sm font-medium">{t("stocktaking.physicalCount")}</label>
              <NumberInput
                ref={physicalCountRef}
                min={0}
                value={physicalCount}
                onChange={(e) => setPhysicalCount(parseInt(e.target.value, 10) || 0)}
                onKeyDown={handleKeyDown}
                className="w-full"
              />
            </div>
            <div className="flex items-end">
              <Button
                onClick={handleSubmit}
                disabled={!scannedBarcode || physicalCount < 0 || recordItem.isPending}
                className="shrink-0"
              >
                <IconCheck className="mr-1.5 h-4 w-4" />
                {t("stocktaking.counted")}
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {recentItems.length > 0 && (
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Recently Scanned (last {recentItems.length})
            </CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <div className="divide-y">
              {recentItems.map((item) => (
                <div
                  key={item.id}
                  className="flex items-center justify-between px-4 py-2.5"
                >
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-mono text-sm font-medium">{item.barcode}</p>
                    <p className="text-muted-foreground truncate text-xs">
                      {item.frameName ?? "Unknown"}
                    </p>
                  </div>
                  <div className="ml-4 flex items-center gap-3 text-right">
                    <div className="text-xs">
                      <span className="text-muted-foreground">Phys: </span>
                      <span className="font-medium">{item.physicalCount}</span>
                    </div>
                    <div className="text-xs">
                      <span className="text-muted-foreground">Sys: </span>
                      <span className="font-medium">{item.systemCount}</span>
                    </div>
                    <Badge variant={getDiscrepancyBadge(item.discrepancy)} className="text-xs">
                      <span className={cn(getDiscrepancyColor(item.discrepancy))}>
                        {item.discrepancy === 0 ? (
                          <IconCheck className="h-3 w-3" />
                        ) : item.discrepancy > 0 ? (
                          `+${item.discrepancy}`
                        ) : (
                          `${item.discrepancy}`
                        )}
                      </span>
                    </Badge>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {recentItems.length === 0 && (
        <div className="text-muted-foreground flex flex-col items-center justify-center py-8 text-sm">
          <IconX className="mb-2 h-8 w-8 opacity-20" />
          {t("stocktaking.empty")}
        </div>
      )}
    </div>
  )
}
