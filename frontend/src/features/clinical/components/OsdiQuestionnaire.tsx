import { useState, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { Button } from "@/shared/components/Button"
import { Label } from "@/shared/components/Label"
import { Badge } from "@/shared/components/Badge"
import { RadioGroup, RadioGroupItem } from "@/shared/components/RadioGroup"
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

  // -1 = N/A, null = unanswered, 0-4 = score
  const [answers, setAnswers] = useState<(number | null)[]>(
    new Array(12).fill(null),
  )

  const osdiResult = useMemo(() => calculateOsdi(answers), [answers])

  const handleAnswerChange = (questionIndex: number, value: number | null) => {
    setAnswers((prev) => {
      const next = [...prev]
      next[questionIndex] = value
      return next
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
      <div className="flex items-center gap-3 p-3 rounded-lg border bg-muted/30">
        <span className="text-sm font-medium">{t("osdi.liveScore")}:</span>
        {osdiResult.score !== null ? (
          <>
            <span className="text-2xl font-bold tabular-nums">
              {osdiResult.score.toFixed(1)}
            </span>
            <Badge className={cn("border", severityConfig.color)} variant="outline">
              {t(severityConfig.label)}
            </Badge>
          </>
        ) : (
          <span className="text-sm text-muted-foreground">--</span>
        )}
        <span className="text-xs text-muted-foreground ml-auto">
          {t("osdi.answered", { count: osdiResult.answeredCount, total: 12 })}
        </span>
      </div>

      {/* Questions */}
      <div className="space-y-4">
        {/* Subscale headers */}
        <div>
          <h4 className="text-sm font-semibold mb-3">{t("osdi.subscaleA")}</h4>
          {OSDI_QUESTIONS.slice(0, 5).map((q, idx) => (
            <QuestionRow
              key={q.index}
              question={q}
              answer={answers[idx]}
              onChange={(val) => handleAnswerChange(idx, val)}
              isVietnamese={isVietnamese}
              disabled={disabled}
              t={t}
            />
          ))}
        </div>

        <div>
          <h4 className="text-sm font-semibold mb-3">{t("osdi.subscaleB")}</h4>
          {OSDI_QUESTIONS.slice(5, 9).map((q, idx) => (
            <QuestionRow
              key={q.index}
              question={q}
              answer={answers[idx + 5]}
              onChange={(val) => handleAnswerChange(idx + 5, val)}
              isVietnamese={isVietnamese}
              disabled={disabled}
              t={t}
            />
          ))}
        </div>

        <div>
          <h4 className="text-sm font-semibold mb-3">{t("osdi.subscaleC")}</h4>
          {OSDI_QUESTIONS.slice(9, 12).map((q, idx) => (
            <QuestionRow
              key={q.index}
              question={q}
              answer={answers[idx + 9]}
              onChange={(val) => handleAnswerChange(idx + 9, val)}
              isVietnamese={isVietnamese}
              disabled={disabled}
              t={t}
            />
          ))}
        </div>
      </div>

      {/* Submit */}
      <div className="flex justify-end pt-2">
        <Button
          onClick={handleSubmit}
          disabled={disabled || isSubmitting || osdiResult.answeredCount === 0}
        >
          {isSubmitting ? t("osdi.submitting") : t("osdi.submit")}
        </Button>
      </div>
    </div>
  )
}

interface QuestionRowProps {
  question: (typeof OSDI_QUESTIONS)[number]
  answer: number | null
  onChange: (value: number | null) => void
  isVietnamese: boolean
  disabled?: boolean
  t: (key: string) => string
}

function QuestionRow({ question, answer, onChange, isVietnamese, disabled, t }: QuestionRowProps) {
  return (
    <div className="py-2 border-b last:border-b-0">
      <div className="mb-2">
        <span className="text-sm font-medium">
          {question.index}. {isVietnamese ? question.vi : question.en}
        </span>
        {isVietnamese && (
          <span className="text-xs text-muted-foreground ml-2 italic">
            ({question.en})
          </span>
        )}
      </div>
      <RadioGroup
        value={answer !== null ? String(answer) : undefined}
        onValueChange={(val) => onChange(val === "na" ? null : Number(val))}
        className="flex flex-wrap gap-2"
        disabled={disabled}
      >
        {ANSWER_OPTIONS.map((opt) => (
          <div key={opt.value} className="flex items-center gap-1">
            <RadioGroupItem
              value={String(opt.value)}
              id={`q${question.index}-${opt.value}`}
            />
            <Label
              htmlFor={`q${question.index}-${opt.value}`}
              className="text-xs cursor-pointer"
            >
              {t(opt.labelKey)}
            </Label>
          </div>
        ))}
        {question.hasNA && (
          <div className="flex items-center gap-1">
            <RadioGroupItem
              value="na"
              id={`q${question.index}-na`}
            />
            <Label
              htmlFor={`q${question.index}-na`}
              className="text-xs cursor-pointer"
            >
              N/A
            </Label>
          </div>
        )}
      </RadioGroup>
    </div>
  )
}

export { SEVERITY_CONFIG, calculateOsdi }
export type { OsdiResult }
