import { useState, useMemo, useCallback } from "react"
import { createFileRoute } from "@tanstack/react-router"
import { useQuery, useMutation } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"
import createClient from "openapi-fetch"
import { IconLanguage, IconLoader2, IconAlertTriangle, IconCircleCheck } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"

// -- Public API client (no auth) --
const API_URL = (import.meta as never as { env: Record<string, string> }).env?.VITE_API_URL ?? "http://localhost:5255"
const publicApi = createClient({ baseUrl: API_URL })

// -- Types matching backend contracts --
interface OsdiQuestionDto {
  index: number
  textEn: string
  textVi: string
}

interface OsdiQuestionnaireDto {
  questions: OsdiQuestionDto[]
  currentAnswers: number[] | null
  visitDate: string
}

// -- OSDI constants --
// Questions 1-5 are mandatory (Subscale A: Ocular Symptoms)
// Questions 6-9 are optional/N/A allowed (Subscale B: Vision-Related)
// Questions 10-12 are optional/N/A allowed (Subscale C: Environmental Triggers)
const NA_ELIGIBLE_START = 5 // Index 5+ (questions 6-12) can be marked N/A

const SEVERITY_CONFIG = [
  { max: 12, key: "normal", color: "bg-green-100 text-green-800 border-green-300" },
  { max: 22, key: "mild", color: "bg-yellow-100 text-yellow-800 border-yellow-300" },
  { max: 32, key: "moderate", color: "bg-orange-100 text-orange-800 border-orange-300" },
  { max: 100, key: "severe", color: "bg-red-100 text-red-800 border-red-300" },
] as const

function getSeverity(score: number) {
  return SEVERITY_CONFIG.find((s) => score <= s.max) ?? SEVERITY_CONFIG[3]
}

function calculateOsdiScore(answers: (number | null)[]): number | null {
  const answered = answers.filter((a): a is number => a !== null)
  if (answered.length === 0) return null
  const sum = answered.reduce((acc, val) => acc + val, 0)
  return (sum * 100) / (answered.length * 4)
}

// -- Route --
export const Route = createFileRoute("/osdi/$token")({
  component: OsdiPublicPage,
})

function OsdiPublicPage() {
  const { token } = Route.useParams()
  const { t, i18n } = useTranslation("clinical")

  const [answers, setAnswers] = useState<(number | null)[]>([])
  const [submitted, setSubmitted] = useState(false)
  const [finalScore, setFinalScore] = useState<number | null>(null)

  const toggleLanguage = () => {
    const next = i18n.language === "vi" ? "en" : "vi"
    i18n.changeLanguage(next)
  }

  // Fetch questionnaire
  const {
    data: questionnaire,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["public-osdi", token],
    queryFn: async (): Promise<OsdiQuestionnaireDto> => {
      const { data, error, response } = await publicApi.GET(
        `/api/public/osdi/${token}` as never,
      )
      if (error || !response.ok) {
        const status = response.status
        if (status === 404) throw new Error("INVALID")
        if (status === 410 || status === 400) throw new Error("EXPIRED")
        throw new Error("NETWORK")
      }
      // Initialize answers array
      const q = data as OsdiQuestionnaireDto
      return q
    },
    retry: false,
  })

  // Initialize answers when questionnaire loads
  useMemo(() => {
    if (questionnaire && answers.length === 0) {
      const initial = questionnaire.currentAnswers
        ?? new Array(questionnaire.questions.length).fill(null)
      setAnswers([...initial])
    }
  }, [questionnaire, answers.length])

  // Submit mutation
  const submitMutation = useMutation({
    mutationFn: async (answersData: (number | null)[]) => {
      const { data, error, response } = await publicApi.POST(
        `/api/public/osdi/${token}` as never,
        {
          body: { answers: answersData },
        } as never,
      )
      if (error || !response.ok) {
        throw new Error("SUBMIT_FAILED")
      }
      return data as number
    },
    onSuccess: (score) => {
      setFinalScore(score)
      setSubmitted(true)
    },
  })

  const setAnswer = useCallback(
    (index: number, value: number | null) => {
      setAnswers((prev) => {
        const next = [...prev]
        next[index] = value
        return next
      })
    },
    [],
  )

  const liveScore = useMemo(() => calculateOsdiScore(answers), [answers])

  const answeredCount = answers.filter((a) => a !== null).length
  const totalQuestions = questionnaire?.questions.length ?? 12

  const canSubmit = useMemo(() => {
    // At minimum, questions 1-5 (index 0-4) must be answered
    if (answers.length < 5) return false
    for (let i = 0; i < Math.min(5, answers.length); i++) {
      if (answers[i] === null) return false
    }
    return true
  }, [answers])

  const handleSubmit = () => {
    if (!canSubmit) return
    submitMutation.mutate(answers)
  }

  // Error states
  if (error) {
    const errMsg = (error as Error).message
    return (
      <PublicPageShell toggleLanguage={toggleLanguage} langCode={i18n.language}>
        <div className="text-center py-12">
          <IconAlertTriangle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <h2 className="text-xl font-semibold mb-2">
            {errMsg === "EXPIRED"
              ? t("osdi.expired")
              : errMsg === "INVALID"
                ? t("osdi.invalid")
                : t("osdi.networkError")}
          </h2>
        </div>
      </PublicPageShell>
    )
  }

  // Loading
  if (isLoading || !questionnaire) {
    return (
      <PublicPageShell toggleLanguage={toggleLanguage} langCode={i18n.language}>
        <div className="flex items-center justify-center py-12">
          <IconLoader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      </PublicPageShell>
    )
  }

  // Success state
  if (submitted && finalScore != null) {
    const severity = getSeverity(finalScore)
    return (
      <PublicPageShell toggleLanguage={toggleLanguage} langCode={i18n.language}>
        <div className="text-center py-12 space-y-4">
          <IconCircleCheck className="h-16 w-16 text-green-500 mx-auto" />
          <h2 className="text-2xl font-semibold">{t("osdi.thankYou")}</h2>
          <p className="text-muted-foreground">{t("osdi.thankYouDesc")}</p>
          <div className="mt-6">
            <p className="text-sm text-muted-foreground mb-1">
              {t("osdi.yourScore")}
            </p>
            <div className="text-4xl font-bold">{finalScore.toFixed(1)}</div>
            <Badge
              variant="outline"
              className={`mt-2 ${severity.color}`}
            >
              {t(`osdi.severity.${severity.key}`)}
            </Badge>
          </div>
        </div>
      </PublicPageShell>
    )
  }

  // Questionnaire form
  const isVi = i18n.language === "vi"
  const visitDate = new Date(questionnaire.visitDate).toLocaleDateString(
    isVi ? "vi-VN" : "en-US",
    { year: "numeric", month: "long", day: "numeric" },
  )

  return (
    <PublicPageShell toggleLanguage={toggleLanguage} langCode={i18n.language}>
      {/* Title */}
      <div className="text-center mb-6">
        <h2 className="text-2xl font-semibold tracking-tight">
          {t("osdi.title")}
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          {t("osdi.subtitle")}
        </p>
        <p className="text-xs text-muted-foreground mt-2">
          {t("osdi.visitDate")}: {visitDate}
        </p>
      </div>

      {/* Live score preview */}
      {liveScore != null && (
        <div className="mb-6 p-3 bg-muted rounded-lg text-center">
          <p className="text-xs text-muted-foreground mb-1">
            {t("osdi.score")}
          </p>
          <div className="text-2xl font-bold">{liveScore.toFixed(1)}</div>
          <Badge
            variant="outline"
            className={`mt-1 ${getSeverity(liveScore).color}`}
          >
            {t(`osdi.severity.${getSeverity(liveScore).key}`)}
          </Badge>
          <p className="text-xs text-muted-foreground mt-1">
            {answeredCount}/{totalQuestions}
          </p>
        </div>
      )}

      {/* Questions */}
      <div className="space-y-4">
        {questionnaire.questions.map((q, idx) => {
          const isNaEligible = idx >= NA_ELIGIBLE_START
          const currentAnswer = answers[idx] ?? null
          const isNa = isNaEligible && currentAnswer === null && answers.length > idx

          return (
            <Card key={q.index} className="overflow-hidden">
              <CardHeader className="pb-2 pt-3 px-4">
                <CardTitle className="text-sm font-medium">
                  {t("osdi.question", { number: q.index + 1 })}
                </CardTitle>
                {/* Vietnamese text primary */}
                <p className="text-sm mt-1">{q.textVi}</p>
                {/* English text secondary */}
                <p className="text-xs text-muted-foreground italic">{q.textEn}</p>
              </CardHeader>
              <CardContent className="px-4 pb-3">
                <div className="flex flex-wrap gap-2">
                  {[0, 1, 2, 3, 4].map((val) => (
                    <button
                      key={val}
                      type="button"
                      className={`flex-1 min-w-[60px] py-2 px-1 text-xs rounded-md border transition-colors ${
                        currentAnswer === val
                          ? "bg-primary text-primary-foreground border-primary"
                          : "bg-card hover:bg-muted border-border"
                      }`}
                      onClick={() => setAnswer(idx, val)}
                    >
                      <div className="font-medium">{val}</div>
                      <div className="mt-0.5 leading-tight">
                        {t(`osdi.answers.${val}`)}
                      </div>
                    </button>
                  ))}
                  {isNaEligible && (
                    <button
                      type="button"
                      className={`min-w-[60px] py-2 px-3 text-xs rounded-md border transition-colors ${
                        currentAnswer === null && answers.length > idx
                          ? "bg-muted text-muted-foreground border-muted-foreground/30"
                          : "bg-card hover:bg-muted border-border"
                      }`}
                      onClick={() => setAnswer(idx, null)}
                    >
                      {t("osdi.na")}
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
        {!canSubmit && (
          <p className="text-xs text-muted-foreground mb-2">
            {t("osdi.fillAllRequired")}
          </p>
        )}
        <Button
          onClick={handleSubmit}
          disabled={!canSubmit || submitMutation.isPending}
          className="w-full sm:w-auto min-w-[200px]"
        >
          {submitMutation.isPending ? (
            <>
              <IconLoader2 className="h-4 w-4 animate-spin mr-2" />
              {t("osdi.submitting")}
            </>
          ) : (
            t("osdi.submit")
          )}
        </Button>
      </div>
    </PublicPageShell>
  )
}

// Reusable shell for the public page layout
function PublicPageShell({
  children,
  toggleLanguage,
  langCode,
}: {
  children: React.ReactNode
  toggleLanguage: () => void
  langCode: string
}) {
  return (
    <div className="min-h-screen bg-muted">
      {/* Header */}
      <header className="bg-card border-b">
        <div className="max-w-2xl mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex size-10 items-center justify-center rounded-md bg-primary text-primary-foreground text-sm font-bold tracking-wider shadow-sm">
              <span>G</span>
            </div>
            <div>
              <h1 className="font-semibold text-lg tracking-tight">Ganka28</h1>
              <p className="text-xs text-muted-foreground font-medium uppercase tracking-widest">
                Ophthalmology
              </p>
            </div>
          </div>
          <Button
            variant="ghost"
            size="sm"
            onClick={toggleLanguage}
            className="gap-1.5"
          >
            <IconLanguage className="h-4 w-4" />
            <span className="text-xs font-medium">
              {langCode === "vi" ? "EN" : "VI"}
            </span>
          </Button>
        </div>
      </header>

      {/* Main content */}
      <main className="max-w-2xl mx-auto px-4 py-6 sm:py-8">
        <div className="bg-card border p-4 sm:p-6 shadow-sm rounded-lg">
          {children}
        </div>
      </main>

      {/* Footer */}
      <footer className="border-t bg-card mt-8">
        <div className="max-w-2xl mx-auto px-4 py-4 text-center text-xs text-muted-foreground/70">
          &copy; {new Date().getFullYear()} Ganka28. All rights reserved.
        </div>
      </footer>
    </div>
  )
}
