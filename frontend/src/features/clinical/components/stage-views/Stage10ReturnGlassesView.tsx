import { useState, useMemo, useCallback } from "react"
import { useNavigate } from "@tanstack/react-router"
import { toast } from "sonner"
import {
  IconSquare,
  IconSquareCheck,
  IconPrinter,
} from "@tabler/icons-react"
import { cn } from "@/shared/lib/utils"
import { Button } from "@/shared/components/Button"
import { StageDetailShell } from "../StageDetailShell"
import { StageBottomBar } from "../StageBottomBar"
import { VisitCompleteBanner } from "./VisitCompleteBanner"
import type { VisitDetailDto } from "../../api/clinical-api"

// -- Types --

interface CompleteHandoffInput {
  visitId: string
  prescriptionVerified: boolean
  frameCorrect: boolean
  patientConfirmedFit: boolean
}

// -- Stub mutation hook (backend not yet built) --

function useCompleteHandoff() {
  const [isPending, setIsPending] = useState(false)

  const mutate = useCallback(
    (
      input: CompleteHandoffInput,
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

function formatVisitDuration(visitDate: string): string {
  const start = new Date(visitDate)
  const now = new Date()
  const diffMs = now.getTime() - start.getTime()
  const diffMin = Math.floor(diffMs / 60000)
  if (diffMin < 60) return `${diffMin} ph\u00FAt`
  const hours = Math.floor(diffMin / 60)
  const mins = diffMin % 60
  return `${hours}h ${mins}ph`
}

// -- Component --

interface Stage10ReturnGlassesViewProps {
  visit: VisitDetailDto
}

export function Stage10ReturnGlassesView({ visit }: Stage10ReturnGlassesViewProps) {
  const navigate = useNavigate()

  const opticalRx = useMemo(() => {
    return visit.opticalPrescriptions.length > 0
      ? visit.opticalPrescriptions[0]
      : null
  }, [visit.opticalPrescriptions])

  const [checkedIds, setCheckedIds] = useState<Set<string>>(new Set())
  const [completed, setCompleted] = useState(false)

  const completeHandoff = useCompleteHandoff()

  // Build checklist items with inline prescription values
  const checklistItems = useMemo(() => {
    const odSph = opticalRx ? formatDiopter(opticalRx.odSph) : "-"
    const osSph = opticalRx ? formatDiopter(opticalRx.osSph) : "-"

    return [
      {
        id: "prescription-verified",
        label: `\u0110\u00FAng c\u00F4ng su\u1EA5t: OD ${odSph} / OS ${osSph}`,
      },
      {
        id: "frame-correct",
        label: "G\u1ECDng k\u00EDnh \u0111\u00FAng m\u00E3, kh\u00F4ng tr\u1EA7y x\u01B0\u1EDBc",
      },
      {
        id: "patient-confirmed",
        label: "B\u1EC7nh nh\u00E2n x\u00E1c nh\u1EADn v\u1EEBa v\u1EB7n",
      },
    ]
  }, [opticalRx])

  const allChecked = checkedIds.size === checklistItems.length

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
    completeHandoff.mutate(
      {
        visitId: visit.id,
        prescriptionVerified: true,
        frameCorrect: true,
        patientConfirmedFit: true,
      },
      {
        onSuccess: () => {
          toast.success("Ho\u00E0n t\u1EA5t \u2014 \u0111\u00E3 tr\u1EA3 k\u00EDnh")
          setCompleted(true)
        },
        onError: () => {
          toast.error("L\u1ED7i khi ho\u00E0n t\u1EA5t")
        },
      },
    )
  }, [visit.id, completeHandoff])

  const handlePrintWarranty = useCallback(() => {
    toast.info("In phi\u1EBFu b\u1EA3o h\u00E0nh...")
    // Will integrate with document-api when backend ready
  }, [])

  const handleCloseRecord = useCallback(() => {
    navigate({ to: "/clinical" })
  }, [navigate])

  // Stage pill
  const stagePill = completed
    ? { text: "Ho\u00E0n t\u1EA5t", variant: "green" as const }
    : { text: "Tr\u1EA3 k\u00EDnh", variant: "amber" as const }

  // Bottom bar
  const bottomBar = completed ? (
    <StageBottomBar
      secondaryButton={{
        label: "In phi\u1EBFu b\u1EA3o h\u00E0nh",
        onClick: handlePrintWarranty,
      }}
      primaryButton={{
        label: "\u0110\u00F3ng h\u1ED3 s\u01A1",
        onClick: handleCloseRecord,
      }}
    />
  ) : (
    <StageBottomBar
      primaryButton={{
        label: "Ho\u00E0n t\u1EA5t \u2014 \u0111\u00E3 tr\u1EA3 k\u00EDnh \u2713",
        onClick: handleComplete,
        disabled: !allChecked || completeHandoff.isPending,
      }}
    />
  )

  return (
    <StageDetailShell
      patientName={visit.patientName}
      patientId={visit.patientId}
      doctorName={visit.doctorName}
      visitDate={visit.visitDate}
      stageName="Tr\u1EA3 k\u00EDnh"
      stagePill={stagePill}
      bottomBar={bottomBar}
    >
      <div className="space-y-6">
        {/* Visit complete banner */}
        {completed && (
          <VisitCompleteBanner
            totalCollected={0}
            visitDuration={formatVisitDuration(visit.visitDate)}
            completedDate={new Date().toLocaleDateString("vi-VN")}
          />
        )}

        {/* Handoff checklist */}
        <div className="space-y-2">
          <h3 className="text-sm font-medium">
            Ki\u1EC3m tra tr\u01B0\u1EDBc khi tr\u1EA3 ({checkedIds.size}/{checklistItems.length})
          </h3>
          {checklistItems.map((item) => {
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
