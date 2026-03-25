import { useState, useCallback, useRef, useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import { cn } from "@/shared/lib/utils"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/shared/components/Tabs"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import { StageDetailShell } from "../StageDetailShell"
import { StageBottomBar } from "../StageBottomBar"
import { ValidationMessage } from "../ValidationMessage"
import { SkipStageModal } from "./SkipStageModal"
import { RefractionSkipBanner } from "./RefractionSkipBanner"
import {
  useUpdateRefraction,
  useAdvanceStage,
  useSkipRefraction,
  useUndoRefractionSkip,
  useVisitById,
} from "../../api/clinical-api"
import type { VisitDetailDto, RefractionDto } from "../../api/clinical-api"

// -- Constants --

const REFRACTION_TABS = [
  { value: "0", label: "Th\u01B0\u1EDDng quy" },
  { value: "1", label: "M\u00E1y \u0111o t\u1EF1 \u0111\u1ED9ng" },
  { value: "2", label: "Li\u1EC7t \u0111i\u1EC1u ti\u1EBFt" },
]

const SKIP_REASON_CHIPS = [
  { value: 0, label: "T\u00E1i kh\u00E1m, \u0111\u00E3 c\u00F3 k\u1EBFt qu\u1EA3 c\u0169" },
  { value: 1, label: "B\u1EC7nh nh\u00E2n t\u1EEB ch\u1ED1i \u0111o" },
  { value: 2, label: "Kh\u00E1m t\u1ED5ng qu\u00E1t, kh\u00F4ng li\u00EAn quan kh\u00FAc x\u1EA1" },
  { value: 3, label: "Kh\u00E1c" },
]

// WorkflowStage enum values
const STAGE_REFRACTION_VA = 1
const STAGE_DOCTOR_EXAM = 2

const LABELS = {
  stageName: "\u0110o kh\u00FAc x\u1EA1 / Th\u1ECB l\u1EF1c",
  saveDraft: "L\u01B0u nh\u00E1p",
  skipButton: "B\u1ECF qua b\u01B0\u1EDBc n\u00E0y",
  forwardButton: "Ho\u00E0n t\u1EA5t \u0111o, chuy\u1EC3n b\u00E1c s\u0129 \u203A",
  forwardButtonSkipped: "Chuy\u1EC3n b\u00E1c s\u0129 \u203A",
  skipModalTitle: "B\u1ECF qua \u0111o kh\u00FAc x\u1EA1 / th\u1ECB l\u1EF1c?",
  pillActive: "\u0110ang \u0111o",
  pillSkipped: "\u0110\u00E3 b\u1ECF qua",
  pillComplete: "Ho\u00E0n t\u1EA5t",
  odLabel: "MP (M\u1EAFt ph\u1EA3i)",
  osLabel: "MT (M\u1EAFt tr\u00E1i)",
  validationBothEmpty: "Ch\u01B0a th\u1EC3 chuy\u1EC3n \u2014 c\u1EA7n nh\u1EADp SPH cho m\u1EAFt ph\u1EA3i v\u00E0 m\u1EAFt tr\u00E1i.",
  validationOdOnly: "M\u1EAFt ph\u1EA3i \u0111\u00E3 nh\u1EADp. C\u1EA7n nh\u1EADp SPH cho m\u1EAFt tr\u00E1i \u0111\u1EC3 chuy\u1EC3n ti\u1EBFp.",
  validationOsOnly: "M\u1EAFt tr\u00E1i \u0111\u00E3 nh\u1EADp. C\u1EA7n nh\u1EADp SPH cho m\u1EAFt ph\u1EA3i \u0111\u1EC3 chuy\u1EC3n ti\u1EBFp.",
  validationReady: "S\u1EB5n s\u00E0ng chuy\u1EC3n \u2014 b\u00E1c s\u0129 s\u1EBD nh\u1EADn l\u01B0\u1EE3t kh\u00E1m ngay.",
}

// -- Form schema --

const optionalNumber = z
  .union([z.string(), z.number(), z.null()])
  .transform((v) => {
    if (v === null || v === "" || v === undefined) return null
    const n = Number(v)
    return isNaN(n) ? null : n
  })
  .nullable()
  .optional()

const refractionSchema = z.object({
  odSph: optionalNumber,
  odCyl: optionalNumber,
  odAxis: optionalNumber,
  odAdd: optionalNumber,
  odPd: optionalNumber,
  osSph: optionalNumber,
  osCyl: optionalNumber,
  osAxis: optionalNumber,
  osAdd: optionalNumber,
  osPd: optionalNumber,
  ucvaOd: optionalNumber,
  ucvaOs: optionalNumber,
  bcvaOd: optionalNumber,
  bcvaOs: optionalNumber,
  iopOd: optionalNumber,
  iopOs: optionalNumber,
  axialLengthOd: optionalNumber,
  axialLengthOs: optionalNumber,
})

type RefractionFormValues = z.infer<typeof refractionSchema>

// -- Field definitions --

interface FieldDef {
  key: string
  label: string
  min?: number
  max?: number
  step?: number
  unit?: string
}

const EYE_FIELDS: FieldDef[] = [
  { key: "Sph", label: "SPH", min: -30, max: 30, step: 0.25, unit: "D" },
  { key: "Cyl", label: "CYL", min: -10, max: 10, step: 0.25, unit: "D" },
  { key: "Axis", label: "AXIS", min: 1, max: 180, step: 1, unit: "\u00B0" },
  { key: "Add", label: "ADD", min: 0.25, max: 4, step: 0.25, unit: "D" },
  { key: "Pd", label: "PD", min: 20, max: 80, step: 0.5, unit: "mm" },
]

const VISUAL_FIELDS: FieldDef[] = [
  { key: "ucva", label: "UCVA", min: 0.01, max: 2.0, step: 0.01 },
  { key: "bcva", label: "BCVA", min: 0.01, max: 2.0, step: 0.01 },
  { key: "iop", label: "IOP", min: 1, max: 60, step: 1, unit: "mmHg" },
  { key: "axialLength", label: "Axial Length", min: 15.0, max: 40.0, step: 0.01, unit: "mm" },
]

// -- Component --

interface Stage2RefractionViewProps {
  visit: VisitDetailDto
}

export function Stage2RefractionView({ visit }: Stage2RefractionViewProps) {
  const [activeTab, setActiveTab] = useState("0")
  const [skipModalOpen, setSkipModalOpen] = useState(false)
  const [isSkipped, setIsSkipped] = useState(false)
  const [skipInfo, setSkipInfo] = useState<{ reason: number; actorName?: string; skippedAt?: string } | null>(null)

  const updateMutation = useUpdateRefraction()
  const advanceMutation = useAdvanceStage()
  const skipMutation = useSkipRefraction()
  const undoSkipMutation = useUndoRefractionSkip()

  const refractionType = Number(activeTab)
  const data = visit.refractions.find((r) => r.type === refractionType)

  const form = useForm<RefractionFormValues>({
    resolver: zodResolver(refractionSchema),
    defaultValues: buildDefaults(data),
  })

  // Reset form when tab changes or data reloads
  useEffect(() => {
    form.reset(buildDefaults(data))
  }, [data, activeTab]) // eslint-disable-line react-hooks/exhaustive-deps

  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  useEffect(() => {
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current)
    }
  }, [])

  // -- SPH validation state --
  const odSph = form.watch("odSph")
  const osSph = form.watch("osSph")
  const hasOdSph = odSph !== null && odSph !== undefined
  const hasOsSph = osSph !== null && osSph !== undefined
  const bothSphFilled = hasOdSph && hasOsSph

  function getValidationState(): { state: "error" | "warning" | "success"; message: string } | null {
    if (isSkipped) return null
    if (!hasOdSph && !hasOsSph) return { state: "error", message: LABELS.validationBothEmpty }
    if (hasOdSph && !hasOsSph) return { state: "warning", message: LABELS.validationOdOnly }
    if (!hasOdSph && hasOsSph) return { state: "warning", message: LABELS.validationOsOnly }
    return { state: "success", message: LABELS.validationReady }
  }

  // -- Handlers --

  const handleSaveDraft = useCallback(() => {
    const values = form.getValues()
    const parsed = refractionSchema.safeParse(values)
    if (!parsed.success) return

    updateMutation.mutate(
      { visitId: visit.id, refractionType, ...parsed.data },
      {
        onSuccess: () => toast.success("Refraction saved"),
        onError: (error) => {
          handleServerValidationError(error, form.setError, {})
          toast.error("Failed to save")
        },
      },
    )
  }, [form, visit.id, refractionType, updateMutation])

  const handleBlur = useCallback(() => {
    if (isSkipped) return
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      handleSaveDraft()
    }, 500)
  }, [isSkipped, handleSaveDraft])

  function handleAdvance() {
    if (!bothSphFilled && !isSkipped) return
    advanceMutation.mutate(
      { visitId: visit.id, newStage: STAGE_DOCTOR_EXAM },
      {
        onSuccess: () => {
          toast.success("Chuy\u1EC3n b\u00E1c s\u0129 th\u00E0nh c\u00F4ng")
        },
        onError: () => {
          toast.error("Kh\u00F4ng th\u1EC3 chuy\u1EC3n")
        },
      },
    )
  }

  function handleSkipConfirm(reason: number, freeTextNote?: string) {
    skipMutation.mutate(
      { visitId: visit.id, reason, freeTextNote },
      {
        onSuccess: () => {
          setIsSkipped(true)
          setSkipInfo({ reason, skippedAt: new Date().toISOString() })
          setSkipModalOpen(false)
          toast.success("\u0110\u00E3 b\u1ECF qua \u0111o kh\u00FAc x\u1EA1")
        },
        onError: () => {
          toast.error("Kh\u00F4ng th\u1EC3 b\u1ECF qua")
        },
      },
    )
  }

  function handleUndo() {
    undoSkipMutation.mutate(visit.id, {
      onSuccess: () => {
        setIsSkipped(false)
        setSkipInfo(null)
        toast.success("Ho\u00E0n t\u00E1c th\u00E0nh c\u00F4ng")
      },
      onError: () => {
        toast.error("Kh\u00F4ng th\u1EC3 ho\u00E0n t\u00E1c")
      },
    })
  }

  // -- Stage pill --
  const stagePill = isSkipped
    ? { text: LABELS.pillSkipped, variant: "amber" as const }
    : bothSphFilled
      ? { text: LABELS.pillComplete, variant: "green" as const }
      : { text: LABELS.pillActive, variant: "default" as const }

  // -- Validation message --
  const validationData = getValidationState()
  const validationMessage = validationData ? (
    <ValidationMessage state={validationData.state} message={validationData.message} />
  ) : null

  // -- Bottom bar --
  const bottomBar = (
    <StageBottomBar
      saveDraftButton={{
        label: LABELS.saveDraft,
        onClick: handleSaveDraft,
      }}
      secondaryButton={
        isSkipped
          ? undefined
          : {
              label: LABELS.skipButton,
              onClick: () => setSkipModalOpen(true),
              className: "border-red-500 text-red-600 hover:bg-red-50",
            }
      }
      primaryButton={{
        label: isSkipped ? LABELS.forwardButtonSkipped : LABELS.forwardButton,
        onClick: handleAdvance,
        disabled: !isSkipped && !bothSphFilled,
      }}
      validationMessage={validationMessage}
    />
  )

  return (
    <>
      <StageDetailShell
        patientName={visit.patientName}
        patientId={visit.patientId}
        doctorName={visit.doctorName}
        visitDate={visit.visitDate}
        stageName={LABELS.stageName}
        stagePill={stagePill}
        bottomBar={bottomBar}
      >
        {/* Skip banner */}
        {isSkipped && skipInfo && (
          <RefractionSkipBanner
            reason={skipInfo.reason}
            actorName={skipInfo.actorName}
            skippedAt={skipInfo.skippedAt}
            onUndo={handleUndo}
            undoDisabled={visit.currentStage > STAGE_DOCTOR_EXAM}
            isUndoing={undoSkipMutation.isPending}
          />
        )}

        {/* Measurement method tabs */}
        <div className={cn(isSkipped && "opacity-50 pointer-events-none")}>
          <Tabs value={activeTab} onValueChange={setActiveTab}>
            <TabsList className="mb-4">
              {REFRACTION_TABS.map((tab) => (
                <TabsTrigger key={tab.value} value={tab.value}>
                  {tab.label}
                </TabsTrigger>
              ))}
            </TabsList>

            {REFRACTION_TABS.map((tab) => (
              <TabsContent key={tab.value} value={tab.value}>
                <RefractionFormGrid
                  form={form}
                  disabled={isSkipped}
                  onBlur={handleBlur}
                  hasOdSph={hasOdSph}
                  hasOsSph={hasOsSph}
                />
              </TabsContent>
            ))}
          </Tabs>
        </div>
      </StageDetailShell>

      <SkipStageModal
        open={skipModalOpen}
        onClose={() => setSkipModalOpen(false)}
        onConfirm={handleSkipConfirm}
        title={LABELS.skipModalTitle}
        reasonChips={SKIP_REASON_CHIPS}
        isSubmitting={skipMutation.isPending}
      />
    </>
  )
}

// -- Helpers --

function buildDefaults(data: RefractionDto | undefined): RefractionFormValues {
  return {
    odSph: data?.odSph ?? null,
    odCyl: data?.odCyl ?? null,
    odAxis: data?.odAxis ?? null,
    odAdd: data?.odAdd ?? null,
    odPd: data?.odPd ?? null,
    osSph: data?.osSph ?? null,
    osCyl: data?.osCyl ?? null,
    osAxis: data?.osAxis ?? null,
    osAdd: data?.osAdd ?? null,
    osPd: data?.osPd ?? null,
    ucvaOd: data?.ucvaOd ?? null,
    ucvaOs: data?.ucvaOs ?? null,
    bcvaOd: data?.bcvaOd ?? null,
    bcvaOs: data?.bcvaOs ?? null,
    iopOd: data?.iopOd ?? null,
    iopOs: data?.iopOs ?? null,
    axialLengthOd: data?.axialLengthOd ?? null,
    axialLengthOs: data?.axialLengthOs ?? null,
  }
}

function toFormValue(v: number | null | undefined): string {
  if (v === null || v === undefined) return ""
  return String(v)
}

// -- Refraction form grid --

function RefractionFormGrid({
  form,
  disabled,
  onBlur,
  hasOdSph,
  hasOsSph,
}: {
  form: ReturnType<typeof useForm<RefractionFormValues>>
  disabled: boolean
  onBlur: () => void
  hasOdSph: boolean
  hasOsSph: boolean
}) {
  function renderInput(
    name: keyof RefractionFormValues,
    field: FieldDef,
    extraClassName?: string,
  ) {
    const value = form.watch(name) as number | null | undefined
    const [localValue, setLocalValue] = useState(toFormValue(value))

    // eslint-disable-next-line react-hooks/rules-of-hooks
    useEffect(() => {
      const formStr = toFormValue(value)
      const localAsNumber = localValue === "" ? null : Number(localValue)
      const formAsNumber = value ?? null
      if (localValue === "" && formStr === "") return
      if (localAsNumber !== null && !isNaN(localAsNumber) && localAsNumber === formAsNumber) return
      if (localValue === "" && formAsNumber === null) return
      setLocalValue(formStr)
    }, [value]) // eslint-disable-line react-hooks/exhaustive-deps

    return (
      <div className="flex items-center gap-1">
        <Input
          type="text"
          inputMode="decimal"
          className={cn("h-8 text-sm tabular-nums", extraClassName)}
          min={field.min}
          max={field.max}
          step={field.step}
          value={localValue}
          disabled={disabled}
          onChange={(e) => {
            const val = e.target.value
            if (val !== "" && !/^-?\d*\.?\d*$/.test(val)) return
            setLocalValue(val)
          }}
          onBlur={() => {
            const numVal = localValue === "" ? null : Number(localValue)
            if (localValue !== "" && isNaN(numVal as number)) {
              setLocalValue(toFormValue(value))
            } else {
              form.setValue(name, numVal as never)
            }
            onBlur()
          }}
        />
        {field.unit && (
          <span className="text-xs text-muted-foreground whitespace-nowrap min-w-[2.5rem] text-right">
            {field.unit}
          </span>
        )}
      </div>
    )
  }

  function getSphClassName(isFilled: boolean): string {
    if (isFilled) return "border-green-500 bg-green-50"
    return "border-red-500 bg-red-50"
  }

  return (
    <div className="space-y-6">
      {/* Eye-specific fields: two-column OD/OS */}
      <div className="grid grid-cols-[80px_1fr_1fr] gap-x-3 gap-y-1 items-center">
        {/* Header */}
        <div />
        <Label className="text-center font-semibold text-sm">{LABELS.odLabel}</Label>
        <Label className="text-center font-semibold text-sm">{LABELS.osLabel}</Label>

        {EYE_FIELDS.map((field) => {
          const odKey = `od${field.key}` as keyof RefractionFormValues
          const osKey = `os${field.key}` as keyof RefractionFormValues
          const isSph = field.key === "Sph"

          return (
            <div key={field.key} className="contents">
              <Label className="text-xs text-muted-foreground">{field.label}</Label>
              {renderInput(odKey, field, isSph ? getSphClassName(hasOdSph) : undefined)}
              {renderInput(osKey, field, isSph ? getSphClassName(hasOsSph) : undefined)}
            </div>
          )
        })}
      </div>

      {/* Visual acuity + measurements */}
      <div className="grid grid-cols-[80px_1fr_1fr] gap-x-3 gap-y-1 items-center border-t pt-3">
        <div />
        <Label className="text-center text-xs font-medium text-muted-foreground">OD</Label>
        <Label className="text-center text-xs font-medium text-muted-foreground">OS</Label>

        {VISUAL_FIELDS.map((field) => {
          const odKey = `${field.key}Od` as keyof RefractionFormValues
          const osKey = `${field.key}Os` as keyof RefractionFormValues
          return (
            <div key={field.key} className="contents">
              <Label className="text-xs text-muted-foreground">{field.label}</Label>
              {renderInput(odKey, field)}
              {renderInput(osKey, field)}
            </div>
          )
        })}
      </div>
    </div>
  )
}
