import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconChevronDown, IconChevronUp } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Card, CardContent } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { useOsdiAnswers } from "../api/clinical-api"
import type { OsdiAnswerGroup } from "../api/clinical-api"

interface OsdiAnswersSectionProps {
  visitId: string
}

export function OsdiAnswersSection({ visitId }: OsdiAnswersSectionProps) {
  const { t, i18n } = useTranslation("clinical")
  const [open, setOpen] = useState(false)
  const { data: answers, isLoading } = useOsdiAnswers(open ? visitId : undefined)
  const isVietnamese = i18n.language === "vi"

  return (
    <div className="mt-2">
      <Button
        variant="ghost"
        size="sm"
        className="text-xs gap-1"
        onClick={() => setOpen(!open)}
      >
        {open ? (
          <IconChevronUp className="h-3.5 w-3.5" />
        ) : (
          <IconChevronDown className="h-3.5 w-3.5" />
        )}
        {t("osdi.viewDetails")}
      </Button>
      {open && (
        <>
          {isLoading ? (
            <div className="py-4 text-center text-sm text-muted-foreground">...</div>
          ) : !answers ? (
            <div className="py-4 text-center text-sm text-muted-foreground">
              {t("osdi.noAnswers")}
            </div>
          ) : (
            <div className="space-y-3 mt-2">
              {answers.groups.map((group) => (
                <AnswerGroupCard
                  key={group.category}
                  group={group}
                  isVietnamese={isVietnamese}
                  t={t}
                />
              ))}
            </div>
          )}
        </>
      )}
    </div>
  )
}

function AnswerGroupCard({
  group,
  isVietnamese,
  t,
}: {
  group: OsdiAnswerGroup
  isVietnamese: boolean
  t: (key: string) => string
}) {
  const avgScore = group.questions.reduce((sum, q) => {
    return sum + (q.score ?? 0)
  }, 0) / Math.max(group.questions.filter((q) => q.score !== null).length, 1)

  const categoryLabelKey = `osdi.category.${group.category}`

  return (
    <Card>
      <CardContent className="p-3">
        <div className="flex items-center justify-between mb-2">
          <h5 className="text-sm font-medium">{t(categoryLabelKey)}</h5>
          <Badge variant="outline" className="text-xs">
            {t("osdi.avgScore")}: {avgScore.toFixed(1)}
          </Badge>
        </div>
        <div className="space-y-1.5">
          {group.questions.map((q) => (
            <div
              key={q.questionNumber}
              className="flex items-start justify-between gap-2 text-xs"
            >
              <span className="text-muted-foreground flex-1">
                Q{q.questionNumber}. {isVietnamese ? q.textVi : q.textEn}
              </span>
              <span className="font-medium shrink-0 tabular-nums w-8 text-right">
                {q.score !== null ? q.score : "N/A"}
              </span>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  )
}
