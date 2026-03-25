import { useState, useMemo, useCallback } from "react"
import { toast } from "sonner"
import { IconGlass, IconCheck } from "@tabler/icons-react"
import { cn } from "@/shared/lib/utils"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { StageDetailShell } from "../StageDetailShell"
import { StageBottomBar } from "../StageBottomBar"
import type { VisitDetailDto, OpticalPrescriptionDto } from "../../api/clinical-api"

// -- Types --

interface ConfirmOpticalOrderInput {
  visitId: string
  lensType: string
  frameCode: string
  lensCostPerUnit: number
  frameCost: number
  totalCost: number
}

// -- Stub mutation hook (backend not yet built) --

function useConfirmOpticalOrder() {
  const [isPending, setIsPending] = useState(false)

  const mutate = useCallback(
    (
      input: ConfirmOpticalOrderInput,
      options: { onSuccess?: () => void; onError?: () => void },
    ) => {
      setIsPending(true)
      // Simulate API call \u2014 replace with real mutation when backend ready
      setTimeout(() => {
        setIsPending(false)
        options.onSuccess?.()
      }, 500)
    },
    [],
  )

  return { mutate, isPending }
}

// -- Lens catalog --

const LENS_OPTIONS = [
  { value: "essilor-crizal-160", label: "Essilor Crizal 1.60", price: 1500000 },
  { value: "essilor-crizal-167", label: "Essilor Crizal 1.67", price: 2200000 },
  { value: "hoya-hd-160", label: "Hoya HD 1.60", price: 1800000 },
  { value: "hoya-hd-167", label: "Hoya HD 1.67", price: 2500000 },
  { value: "zeiss-blueguard-160", label: "Zeiss BlueGuard 1.60", price: 2800000 },
  { value: "zeiss-blueguard-167", label: "Zeiss BlueGuard 1.67", price: 3500000 },
] as const

// -- Helpers --

function formatDiopter(val: number | null): string {
  if (val == null) return "-"
  const sign = val > 0 ? "+" : ""
  return `${sign}${val.toFixed(2)}`
}

function formatAxis(val: number | null): string {
  if (val == null) return "-"
  return `${val}\u00B0`
}

function formatPd(val: number | null): string {
  if (val == null) return "-"
  return `${val.toFixed(1)}`
}

const currencyFormatter = new Intl.NumberFormat("vi-VN", {
  style: "currency",
  currency: "VND",
})

// -- Constants --

const STAGE_OPTICAL_CENTER = 8
const STAGE_CASHIER = 6

interface Stage7bOpticalCenterViewProps {
  visit: VisitDetailDto
}

export function Stage7bOpticalCenterView({ visit }: Stage7bOpticalCenterViewProps) {
  const opticalRx = useMemo(() => {
    return visit.opticalPrescriptions.length > 0
      ? visit.opticalPrescriptions[0]
      : null
  }, [visit.opticalPrescriptions])

  const [selectedLens, setSelectedLens] = useState("")
  const [frameCode, setFrameCode] = useState("")
  const [frameCost, setFrameCost] = useState("")
  const [confirmed, setConfirmed] = useState(false)

  const confirmOpticalOrder = useConfirmOpticalOrder()

  // Find lens price
  const lensOption = useMemo(
    () => LENS_OPTIONS.find((o) => o.value === selectedLens),
    [selectedLens],
  )
  const lensCostPerUnit = lensOption?.price ?? 0
  const lensCostTotal = lensCostPerUnit * 2
  const frameCostNum = Number(frameCost) || 0
  const totalCost = lensCostTotal + frameCostNum

  const canConfirm = selectedLens !== "" && frameCode.trim() !== ""

  const handleConfirm = useCallback(() => {
    if (!lensOption) return
    confirmOpticalOrder.mutate(
      {
        visitId: visit.id,
        lensType: lensOption.label,
        frameCode: frameCode.trim(),
        lensCostPerUnit,
        frameCost: frameCostNum,
        totalCost,
      },
      {
        onSuccess: () => {
          toast.success("X\u00e1c nh\u1eadn \u0111\u01a1n k\u00ednh th\u00e0nh c\u00f4ng")
          setConfirmed(true)
        },
        onError: () => {
          toast.error("L\u1ed7i khi x\u00e1c nh\u1eadn \u0111\u01a1n k\u00ednh")
        },
      },
    )
  }, [visit.id, lensOption, frameCode, lensCostPerUnit, frameCostNum, totalCost, confirmOpticalOrder])

  // Stage pill
  const stagePill = confirmed
    ? { text: "Ho\u00e0n t\u1ea5t", variant: "green" as const }
    : { text: "Ch\u1ecdn g\u1ecdng/tr\u00f2ng", variant: "amber" as const }

  // Bottom bar
  const bottomBar = confirmed ? (
    <StageBottomBar
      primaryButton={{
        label: "\u0110\u00e3 x\u00e1c nh\u1eadn \u2714",
        onClick: () => {},
        disabled: true,
      }}
    />
  ) : (
    <StageBottomBar
      primaryButton={{
        label: "X\u00e1c nh\u1eadn \u0111\u01a1n k\u00ednh \u203A",
        onClick: handleConfirm,
        disabled: !canConfirm || confirmOpticalOrder.isPending,
      }}
    />
  )

  return (
    <StageDetailShell
      patientName={visit.patientName}
      patientId={visit.patientId}
      doctorName={visit.doctorName}
      visitDate={visit.visitDate}
      stageName="Quang h\u1ecdc"
      stagePill={stagePill}
      bottomBar={bottomBar}
    >
      <div className="space-y-6">
        {/* Confirmed banner */}
        {confirmed && (
          <div className="bg-green-50 border border-green-200 rounded-md p-4 flex items-center gap-3">
            <IconCheck className="h-5 w-5 text-green-600" />
            <div>
              <p className="text-sm font-medium text-green-800">
                \u0110\u00e3 x\u00e1c nh\u1eadn \u0111\u01a1n k\u00ednh
              </p>
              <p className="text-xs text-green-600">
                Chuy\u1ec3n sang thu ng\u00e2n \u0111\u1ec3 thanh to\u00e1n chung
              </p>
            </div>
          </div>
        )}

        {/* Glasses prescription card (read-only) */}
        {opticalRx ? (
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm flex items-center gap-2">
                <IconGlass className="h-4 w-4" />
                \u0110\u01a1n k\u00ednh (ch\u1ec9 \u0111\u1ecdc)
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {/* Rx table */}
                <div className="grid grid-cols-5 gap-2 text-sm">
                  <div className="text-muted-foreground" />
                  <div className="text-center text-xs text-muted-foreground font-medium">SPH</div>
                  <div className="text-center text-xs text-muted-foreground font-medium">CYL</div>
                  <div className="text-center text-xs text-muted-foreground font-medium">AXIS</div>
                  <div className="text-center text-xs text-muted-foreground font-medium">ADD</div>

                  <div className="font-medium text-xs">MP (OD)</div>
                  <div className="text-center font-mono text-xs">{formatDiopter(opticalRx.odSph)}</div>
                  <div className="text-center font-mono text-xs">{formatDiopter(opticalRx.odCyl)}</div>
                  <div className="text-center font-mono text-xs">{formatAxis(opticalRx.odAxis)}</div>
                  <div className="text-center font-mono text-xs">{formatDiopter(opticalRx.odAdd)}</div>

                  <div className="font-medium text-xs">MT (OS)</div>
                  <div className="text-center font-mono text-xs">{formatDiopter(opticalRx.osSph)}</div>
                  <div className="text-center font-mono text-xs">{formatDiopter(opticalRx.osCyl)}</div>
                  <div className="text-center font-mono text-xs">{formatAxis(opticalRx.osAxis)}</div>
                  <div className="text-center font-mono text-xs">{formatDiopter(opticalRx.osAdd)}</div>
                </div>

                {/* PD */}
                <div className="flex gap-4 text-xs">
                  <span className="text-muted-foreground">PD xa: <span className="font-mono">{formatPd(opticalRx.farPd)}</span></span>
                  <span className="text-muted-foreground">PD g\u1ea7n: <span className="font-mono">{formatPd(opticalRx.nearPd)}</span></span>
                </div>

                {/* Notes */}
                {opticalRx.notes && (
                  <p className="text-xs text-muted-foreground italic">
                    Ghi ch\u00fa BS: {opticalRx.notes}
                  </p>
                )}
              </div>
            </CardContent>
          </Card>
        ) : (
          <Card>
            <CardContent className="py-6 text-center text-sm text-muted-foreground">
              Ch\u01b0a c\u00f3 \u0111\u01a1n k\u00ednh
            </CardContent>
          </Card>
        )}

        {/* Frame and lens selection */}
        {!confirmed && (
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm">Ch\u1ecdn g\u1ecdng v\u00e0 tr\u00f2ng</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {/* Lens type dropdown */}
                <div className="space-y-1.5">
                  <Label className="text-xs">Lo\u1ea1i tr\u00f2ng k\u00ednh</Label>
                  <Select value={selectedLens} onValueChange={setSelectedLens}>
                    <SelectTrigger>
                      <SelectValue placeholder="Ch\u1ecdn lo\u1ea1i tr\u00f2ng..." />
                    </SelectTrigger>
                    <SelectContent>
                      {LENS_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>
                          {opt.label} \u2014 {currencyFormatter.format(opt.price)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Frame code input */}
                <div className="space-y-1.5">
                  <Label className="text-xs">M\u00e3 g\u1ecdng</Label>
                  <Input
                    value={frameCode}
                    onChange={(e) => setFrameCode(e.target.value)}
                  />
                </div>

                {/* Frame cost */}
                <div className="space-y-1.5">
                  <Label className="text-xs">Gi\u00e1 g\u1ecdng (\u0111)</Label>
                  <Input
                    type="number"
                    value={frameCost}
                    onChange={(e) => setFrameCost(e.target.value)}
                    min={0}
                  />
                </div>

                {/* Price breakdown (show when both selected) */}
                {canConfirm && (
                  <div className="bg-muted/50 rounded-md p-3 space-y-1">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">
                        Tr\u00f2ng k\u00ednh (x2):
                      </span>
                      <span className="font-mono">{currencyFormatter.format(lensCostTotal)}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">
                        G\u1ecdng k\u00ednh:
                      </span>
                      <span className="font-mono">{currencyFormatter.format(frameCostNum)}</span>
                    </div>
                    <div className="border-t pt-1 flex justify-between text-sm font-medium">
                      <span>T\u1ed5ng c\u1ed9ng:</span>
                      <span className="font-mono">{currencyFormatter.format(totalCost)}</span>
                    </div>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        )}

        {/* Confirmed order summary */}
        {confirmed && lensOption && (
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm">\u0110\u01a1n k\u00ednh \u0111\u00e3 x\u00e1c nh\u1eadn</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Tr\u00f2ng:</span>
                  <span>{lensOption.label}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">G\u1ecdng:</span>
                  <span>{frameCode}</span>
                </div>
                <div className="border-t pt-1 flex justify-between font-medium">
                  <span>T\u1ed5ng:</span>
                  <span className="font-mono">{currencyFormatter.format(totalCost)}</span>
                </div>
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </StageDetailShell>
  )
}
