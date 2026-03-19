import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { cn } from "@/shared/lib/utils"

// OSDI severity thresholds and config
const SEVERITY_CONFIG = {
  normal: { label: "osdi.normal", color: "bg-green-100 text-green-800 border-green-300", max: 12 },
  mild: { label: "osdi.mild", color: "bg-yellow-100 text-yellow-800 border-yellow-300", max: 22 },
  moderate: { label: "osdi.moderate", color: "bg-orange-100 text-orange-800 border-orange-300", max: 32 },
  severe: { label: "osdi.severe", color: "bg-red-100 text-red-800 border-red-300", max: 100 },
} as const

// 12 OSDI questions with both English and Vietnamese text
const OSDI_QUESTIONS = [
  // Subscale A: Ocular Symptoms (Q1-5)
  {
    index: 1,
    en: "Eyes that are sensitive to light?",
    vi: "Mắt nhạy cảm với ánh sáng?",
    hasNA: false,
  },
  {
    index: 2,
    en: "Eyes that feel gritty?",
    vi: "Mắt cảm giác cộm, như có cát?",
    hasNA: false,
  },
  {
    index: 3,
    en: "Painful or sore eyes?",
    vi: "Mắt đau hoặc nhức?",
    hasNA: false,
  },
  {
    index: 4,
    en: "Blurred vision?",
    vi: "Nhìn mờ?",
    hasNA: false,
  },
  {
    index: 5,
    en: "Poor vision?",
    vi: "Thị lực kém?",
    hasNA: false,
  },
  // Subscale B: Vision-Related Function (Q6-9)
  {
    index: 6,
    en: "Reading?",
    vi: "Đọc sách?",
    hasNA: true,
  },
  {
    index: 7,
    en: "Driving at night?",
    vi: "Lái xe ban đêm?",
    hasNA: true,
  },
  {
    index: 8,
    en: "Working with a computer or bank machine (ATM)?",
    vi: "Làm việc với máy tính hoặc máy ATM?",
    hasNA: true,
  },
  {
    index: 9,
    en: "Watching TV?",
    vi: "Xem TV?",
    hasNA: true,
  },
  // Subscale C: Environmental Triggers (Q10-12)
  {
    index: 10,
    en: "Windy conditions?",
    vi: "Khi trời gió?",
    hasNA: true,
  },
  {
    index: 11,
    en: "Places or areas with low humidity (very dry)?",
    vi: "Nơi có độ ẩm thấp (rất khô)?",
    hasNA: true,
  },
  {
    index: 12,
    en: "Areas that are air conditioned?",
    vi: "Nơi có điều hòa không khí?",
    hasNA: true,
  },
] as const

const ANSWER_OPTIONS = [
  { value: 0, labelKey: "osdi.noneOfTime" },
  { value: 1, labelKey: "osdi.someOfTime" },
  { value: 2, labelKey: "osdi.halfOfTime" },
  { value: 3, labelKey: "osdi.mostOfTime" },
  { value: 4, labelKey: "osdi.allOfTime" },
] as const

interface OsdiResult {
  score: number | null
  severity: keyof typeof SEVERITY_CONFIG
  answeredCount: number
}

function calculateOsdi(answers: (number | null)[]): OsdiResult {
  const answered = answers.filter((a): a is number => a !== null && a !== undefined)
  if (answered.length === 0) return { score: null, severity: "normal", answeredCount: 0 }

  const sum = answered.reduce((acc, val) => acc + val, 0)
  const score = (sum * 100) / (answered.length * 4)

  let severity: OsdiResult["severity"]
  if (score <= 12) severity = "normal"
  else if (score <= 22) severity = "mild"
  else if (score <= 32) severity = "moderate"
  else severity = "severe"

  return { score: Math.round(score * 100) / 100, severity, answeredCount: answered.length }
}

interface OsdiQuestionnaireProps {
  onSubmit: (answers: (number | null)[]) => void
  isSubmitting?: boolean
  disabled?: boolean
}

export function OsdiQuestionnaire({ onSubmit, isSubmitting, disabled }: OsdiQuestionnaireProps) {
  const { t, i18n } = useTranslation("clinical")
  const isVietnamese = i18n.language === "vi"

  const [answers, setAnswers] = useState<(number | null)[]>(
    new Array(12).fill(null),
  )
  const [hasInteracted, setHasInteracted] = useState<boolean[]>(new Array(12).fill(false))

  const osdiResult = useMemo(() => calculateOsdi(answers), [answers])

  const handleAnswerChange = (questionIndex: number, value: number | null) => {
    setAnswers((prev) => {
      const next = [...prev]
      next[questionIndex] = value
      return next
    })
    setHasInteracted((prev) => {
      const n = [...prev]
      n[questionIndex] = true
      return n
    })
  }

  const handleSubmit = () => {
    if (osdiResult.answeredCount === 0) return
    onSubmit(answers)
  }

  const severityConfig = SEVERITY_CONFIG[osdiResult.severity]

  return (
    <div className="space-y-4">
      {/* Live score preview */}
      <div className="mb-6 p-3 bg-muted rounded-lg text-center">
        <p className="text-xs text-muted-foreground mb-1">{t("osdi.liveScore")}</p>
        {osdiResult.score !== null ? (
          <>
            <div className="text-2xl font-bold">{osdiResult.score.toFixed(1)}</div>
            <Badge variant="outline" className={cn("mt-1 border", severityConfig.color)}>
              {t(severityConfig.label)}
            </Badge>
          </>
        ) : (
          <div className="text-2xl font-bold text-muted-foreground">--</div>
        )}
        <p className="text-xs text-muted-foreground mt-1">
          {t("osdi.answered", { count: osdiResult.answeredCount, total: 12 })}
        </p>
      </div>

      {/* Questions */}
      <div className="space-y-4">
        {OSDI_QUESTIONS.map((q, idx) => {
          const currentAnswer = answers[idx]
          const isNaEligible = q.hasNA
          const isNaSelected = isNaEligible && currentAnswer === null && hasInteracted[idx]

          return (
            <Card key={q.index} className="overflow-hidden">
              <CardHeader className="pb-2 pt-3 px-4">
                <CardTitle className="text-sm font-medium">
                  {t("osdi.question", { number: q.index })}
                </CardTitle>
                <p className="text-sm mt-1">{isVietnamese ? q.vi : q.en}</p>
                {isVietnamese && (
                  <p className="text-xs text-muted-foreground italic">{q.en}</p>
                )}
              </CardHeader>
              <CardContent className="px-4 pb-3">
                <div className="flex flex-wrap gap-2">
                  {ANSWER_OPTIONS.map((opt) => (
                    <button
                      key={opt.value}
                      type="button"
                      disabled={disabled}
                      className={cn(
                        "flex-1 min-w-[60px] py-2 px-1 text-xs rounded-md border transition-colors",
                        currentAnswer === opt.value
                          ? "bg-primary text-primary-foreground border-primary"
                          : "bg-card hover:bg-muted border-border"
                      )}
                      onClick={() => handleAnswerChange(idx, opt.value)}
                    >
                      <div className="font-medium">{opt.value}</div>
                      <div className="mt-0.5 leading-tight">{t(opt.labelKey)}</div>
                    </button>
                  ))}
                  {isNaEligible && (
                    <button
                      type="button"
                      disabled={disabled}
                      className={cn(
                        "min-w-[60px] py-2 px-3 text-xs rounded-md border transition-colors",
                        isNaSelected
                          ? "bg-muted text-muted-foreground border-muted-foreground/30"
                          : "bg-card hover:bg-muted border-border"
                      )}
                      onClick={() => {
                        handleAnswerChange(idx, null)
                        setHasInteracted((prev) => {
                          const n = [...prev]
                          n[idx] = true
                          return n
                        })
                      }}
                    >
                      N/A
                    </button>
                  )}
                </div>
              </CardContent>
            </Card>
          )
        })}
      </div>

      {/* Submit */}
      <div className="mt-6 text-center">
        <Button
          onClick={handleSubmit}
          disabled={disabled || isSubmitting || osdiResult.answeredCount === 0}
          className="w-full sm:w-auto min-w-[200px]"
        >
          {isSubmitting ? t("osdi.submitting") : t("osdi.submit")}
        </Button>
      </div>
    </div>
  )
}

export { SEVERITY_CONFIG, calculateOsdi }
export type { OsdiResult }
