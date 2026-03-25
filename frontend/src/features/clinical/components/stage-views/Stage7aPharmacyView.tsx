import { useState, useMemo, useCallback } from "react"
import { toast } from "sonner"
import {
  IconCheck,
  IconSquare,
  IconSquareCheck,
  IconPrinter,
} from "@tabler/icons-react"
import { cn } from "@/shared/lib/utils"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Label } from "@/shared/components/Label"
import { StageDetailShell } from "../StageDetailShell"
import { StageBottomBar } from "../StageBottomBar"
import type { VisitDetailDto, PrescriptionItemDto } from "../../api/clinical-api"
import { useVisitById } from "../../api/clinical-api"

// -- Types --

interface DispensePharmacyInput {
  visitId: string
  dispensedItemIds: string[]
  note: string | null
}

// -- Stub mutation hook (backend not yet built) --

function useDispensePharmacy() {
  const [isPending, setIsPending] = useState(false)

  const mutate = useCallback(
    (
      input: DispensePharmacyInput,
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

// -- Constants --

const STAGE_PHARMACY = 7

interface Stage7aPharmacyViewProps {
  visit: VisitDetailDto
}

export function Stage7aPharmacyView({ visit }: Stage7aPharmacyViewProps) {
  // Collect all prescription items across all drug prescriptions
  const allItems = useMemo(() => {
    return visit.drugPrescriptions.flatMap((rx) => rx.items)
  }, [visit.drugPrescriptions])

  // Track checked state per item id
  const [checkedIds, setCheckedIds] = useState<Set<string>>(new Set())
  const [note, setNote] = useState("")
  const [completed, setCompleted] = useState(false)

  const dispensePharmacy = useDispensePharmacy()

  const allChecked = allItems.length > 0 && checkedIds.size === allItems.length

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

  const handleDispense = useCallback(() => {
    dispensePharmacy.mutate(
      {
        visitId: visit.id,
        dispensedItemIds: Array.from(checkedIds),
        note: note.trim() || null,
      },
      {
        onSuccess: () => {
          toast.success("\u0110\u00e3 ph\u00e1t \u0111\u1ee7 thu\u1ed1c")
          setCompleted(true)
        },
        onError: () => {
          toast.error("L\u1ed7i khi ph\u00e1t thu\u1ed1c")
        },
      },
    )
  }, [visit.id, checkedIds, note, dispensePharmacy])

  const handlePrintLabel = useCallback(() => {
    toast.info("In nh\u00e3n thu\u1ed1c...")
    // Will integrate with document-api when backend ready
  }, [])

  // Determine pill status
  const stagePill = completed
    ? { text: "Ho\u00e0n t\u1ea5t", variant: "green" as const }
    : { text: "\u0110ang ph\u00e1t thu\u1ed1c", variant: "amber" as const }

  // -- Bottom bar content --
  const bottomBar = completed ? (
    <StageBottomBar
      primaryButton={{
        label: "In nh\u00e3n thu\u1ed1c",
        onClick: handlePrintLabel,
        disabled: false,
      }}
    />
  ) : (
    <StageBottomBar
      primaryButton={{
        label: "\u0110\u00e3 ph\u00e1t \u0111\u1ee7 thu\u1ed1c \u2713",
        onClick: handleDispense,
        disabled: !allChecked || dispensePharmacy.isPending,
      }}
    />
  )

  return (
    <StageDetailShell
      patientName={visit.patientName}
      patientId={visit.patientId}
      doctorName={visit.doctorName}
      visitDate={visit.visitDate}
      stageName="Ph\u00e1t thu\u1ed1c"
      stagePill={stagePill}
      bottomBar={bottomBar}
    >
      <div className="space-y-4">
        {/* Completion banner */}
        {completed && (
          <div className="bg-green-50 border border-green-200 rounded-md p-4 flex items-center gap-3">
            <IconCheck className="h-5 w-5 text-green-600" />
            <div>
              <p className="text-sm font-medium text-green-800">
                Ho\u00e0n t\u1ea5t
              </p>
              <p className="text-xs text-green-600">
                D\u01b0\u1ee3c s\u0129: {visit.doctorName} &mdash;{" "}
                {new Date().toLocaleString("vi-VN")}
              </p>
            </div>
          </div>
        )}

        {/* Medication checklist */}
        {allItems.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground text-sm">
            Kh\u00f4ng c\u00f3 \u0111\u01a1n thu\u1ed1c n\u00e0o
          </div>
        ) : (
          <div className="space-y-2">
            <h3 className="text-sm font-medium">
              Danh s\u00e1ch thu\u1ed1c ({checkedIds.size}/{allItems.length})
            </h3>
            {allItems.map((item) => {
              const isChecked = checkedIds.has(item.id)
              return (
                <button
                  key={item.id}
                  type="button"
                  disabled={completed}
                  onClick={() => toggleItem(item.id)}
                  className={cn(
                    "w-full flex items-center gap-3 p-3 rounded-md border text-left transition-colors",
                    isChecked && "bg-green-50 border-green-200",
                    !isChecked && "hover:bg-muted/50",
                    completed && "opacity-75 cursor-default",
                  )}
                >
                  {/* Checkbox icon */}
                  <div className="shrink-0">
                    {isChecked ? (
                      <IconSquareCheck className="h-5 w-5 text-green-600" />
                    ) : (
                      <IconSquare className="h-5 w-5 text-muted-foreground" />
                    )}
                  </div>

                  {/* Drug info */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className="font-medium text-sm">{item.drugName}</span>
                      {item.genericName && (
                        <span className="text-xs text-muted-foreground">
                          ({item.genericName})
                        </span>
                      )}
                      {item.strength && (
                        <span className="text-xs text-muted-foreground">
                          {item.strength}
                        </span>
                      )}
                    </div>
                    <div className="text-xs text-muted-foreground mt-0.5">
                      {item.dosageOverride || item.dosage || ""}
                      {item.frequency && ` - ${item.frequency}`}
                    </div>
                  </div>

                  {/* Quantity */}
                  <div className="shrink-0 text-right">
                    <Badge variant="outline" className="text-xs">
                      {item.quantity} {item.unit}
                    </Badge>
                  </div>
                </button>
              )
            })}
          </div>
        )}

        {/* Optional note field */}
        {!completed && allItems.length > 0 && (
          <div className="space-y-1.5">
            <Label className="text-xs">Ghi ch\u00fa ph\u00e1t thu\u1ed1c</Label>
            <AutoResizeTextarea
              value={note}
              onChange={(e) => setNote(e.target.value)}
              rows={2}
              className="text-sm"
            />
          </div>
        )}
      </div>
    </StageDetailShell>
  )
}
