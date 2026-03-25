import { useState, useMemo, useCallback } from "react"
import { toast } from "sonner"
import {
  IconCheck,
  IconSquare,
  IconSquareCheck,
  IconGlass,
} from "@tabler/icons-react"
import { cn } from "@/shared/lib/utils"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { StageDetailShell } from "../StageDetailShell"
import { StageBottomBar } from "../StageBottomBar"
import type { VisitDetailDto, OpticalPrescriptionDto } from "../../api/clinical-api"

// -- Types --

interface CompleteOpticalLabInput {
  visitId: string
  checklistCompleted: boolean
}

// -- Stub mutation hook (backend not yet built) --

function useCompleteOpticalLab() {
  const [isPending, setIsPending] = useState(false)

  const mutate = useCallback(
    (
      input: CompleteOpticalLabInput,
      options: { onSuccess?: () => void; onError?: () => void },
    ) => {
      setIsPending(true)
      // Simulate API call — replace with real mutation when backend ready
      setTimeout(() => {
        setIsPending(false)
        options.onSuccess?.()
      }, 500)
    },
    [],
  )

  return { mutate, isPending }
}

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

// -- Quality checklist items --

const QUALITY_CHECKLIST = [
  { id: "lens-params", label: "Ki\u1EC3m tra th\u00F4ng s\u1ED1 tr\u00F2ng k\u00EDnh" },
  { id: "optical-center", label: "Ki\u1EC3m tra \u0111\u1ED9 ch\u00EDnh x\u00E1c t\u00E2m quang h\u1ECDc" },
  { id: "frame-assembly", label: "Ki\u1EC3m tra g\u1ECDng k\u00EDnh v\u00E0 l\u1EAFp r\u00E1p" },
  { id: "coating", label: "Ki\u1EC3m tra ch\u1ED1ng x\u01B0\u1EDBc v\u00E0 l\u1EDBp ph\u1EE7" },
  { id: "clean", label: "V\u1EC7 sinh k\u00EDnh tr\u01B0\u1EDBc khi giao" },
] as const

// -- Component --

interface Stage9OpticalLabViewProps {
  visit: VisitDetailDto
}

export function Stage9OpticalLabView({ visit }: Stage9OpticalLabViewProps) {
  const opticalRx = useMemo(() => {
    return visit.opticalPrescriptions.length > 0
      ? visit.opticalPrescriptions[0]
      : null
  }, [visit.opticalPrescriptions])

  const [checkedIds, setCheckedIds] = useState<Set<string>>(new Set())
  const [completed, setCompleted] = useState(false)

  const completeOpticalLab = useCompleteOpticalLab()

  const allChecked = checkedIds.size === QUALITY_CHECKLIST.length

  const toggleItem = useCallback((itemId: string) => {
    setCheckedIds((prev) => {
      const next = new Set(prev)
      if (next.has(itemId)) {
        next.delete(itemId)
      } else {
        next.add(itemId)
      }
      return next
    })
  }, [])

  const handleComplete = useCallback(() => {
    completeOpticalLab.mutate(
      {
        visitId: visit.id,
        checklistCompleted: true,
      },
      {
        onSuccess: () => {
          toast.success("K\u00EDnh s\u1EB5n s\u00E0ng, chuy\u1EC3n tr\u1EA3 cho b\u1EC7nh nh\u00E2n")
          setCompleted(true)
        },
        onError: () => {
          toast.error("L\u1ED7i khi ho\u00E0n t\u1EA5t ki\u1EC3m tra")
        },
      },
    )
  }, [visit.id, completeOpticalLab])

  // Stage pill
  const stagePill = completed
    ? { text: "Ho\u00E0n t\u1EA5t", variant: "green" as const }
    : { text: "\u0110ang ki\u1EC3m tra", variant: "amber" as const }

  // Bottom bar
  const bottomBar = completed ? (
    <StageBottomBar
      primaryButton={{
        label: "\u0110\u00E3 ho\u00E0n t\u1EA5t \u2714",
        onClick: () => {},
        disabled: true,
      }}
    />
  ) : (
    <StageBottomBar
      primaryButton={{
        label: "K\u00EDnh s\u1EB5n s\u00E0ng, tr\u1EA3 cho b\u1EC7nh nh\u00E2n \u203A",
        onClick: handleComplete,
        disabled: !allChecked || completeOpticalLab.isPending,
      }}
    />
  )

  return (
    <StageDetailShell
      patientName={visit.patientName}
      patientId={visit.patientId}
      doctorName={visit.doctorName}
      visitDate={visit.visitDate}
      stageName="X\u01B0\u1EDFng k\u00EDnh"
      stagePill={stagePill}
      bottomBar={bottomBar}
    >
      <div className="space-y-6">
        {/* Completion banner */}
        {completed && (
          <div className="bg-green-50 border border-green-200 rounded-md p-4 flex items-center gap-3">
            <IconCheck className="h-5 w-5 text-green-600" />
            <div>
              <p className="text-sm font-medium text-green-800">
                Ki\u1EC3m tra ch\u1EA5t l\u01B0\u1EE3ng ho\u00E0n t\u1EA5t
              </p>
              <p className="text-xs text-green-600">
                K\u00EDnh s\u1EB5n s\u00E0ng \u0111\u1EC3 tr\u1EA3 cho b\u1EC7nh nh\u00E2n
              </p>
            </div>
          </div>
        )}

        {/* Reference card: glasses prescription (read-only) */}
        {opticalRx ? (
          <Card className="bg-muted/30 border">
            <CardHeader className="pb-3">
              <CardTitle className="text-sm flex items-center gap-2">
                <IconGlass className="h-4 w-4" />
                \u0110\u01A1n k\u00EDnh (tham chi\u1EBFu)
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
                  <span className="text-muted-foreground">PD g\u1EA7n: <span className="font-mono">{formatPd(opticalRx.nearPd)}</span></span>
                </div>

                {/* Notes */}
                {opticalRx.notes && (
                  <p className="text-xs text-muted-foreground italic">
                    Ghi ch\u00FA BS: {opticalRx.notes}
                  </p>
                )}
              </div>
            </CardContent>
          </Card>
        ) : (
          <Card className="bg-muted/30 border">
            <CardContent className="py-6 text-center text-sm text-muted-foreground">
              Ch\u01B0a c\u00F3 \u0111\u01A1n k\u00EDnh
            </CardContent>
          </Card>
        )}

        {/* Quality checklist */}
        <div className="space-y-2">
          <h3 className="text-sm font-medium">
            Ki\u1EC3m tra ch\u1EA5t l\u01B0\u1EE3ng ({checkedIds.size}/{QUALITY_CHECKLIST.length})
          </h3>
          {QUALITY_CHECKLIST.map((item) => {
            const isChecked = checkedIds.has(item.id)
            return (
              <button
                key={item.id}
                type="button"
                disabled={completed}
                onClick={() => toggleItem(item.id)}
                className={cn(
                  "w-full flex items-center justify-between gap-3 p-3 rounded-md border text-left transition-colors",
                  isChecked && "bg-green-50 border-green-200",
                  !isChecked && "hover:bg-muted/50",
                  completed && "opacity-75 cursor-default",
                )}
              >
                {/* Label */}
                <span className="text-sm">{item.label}</span>

                {/* Checkbox icon on right */}
                <div className="shrink-0">
                  {isChecked ? (
                    <IconSquareCheck className="h-5 w-5 text-green-600" />
                  ) : (
                    <IconSquare className="h-5 w-5 text-muted-foreground" />
                  )}
                </div>
              </button>
            )
          })}
        </div>
      </div>
    </StageDetailShell>
  )
}
