import { useTranslation } from "react-i18next"
import {
  Tabs,
  TabsList,
  TabsTrigger,
  TabsContent,
} from "@/shared/components/Tabs"
import { VisitSection } from "./VisitSection"
import { RefractionForm } from "./RefractionForm"
import type { RefractionDto } from "../api/clinical-api"

const REFRACTION_TYPES = [
  { type: 0, label: "manifest" },
  { type: 1, label: "autorefraction" },
  { type: 2, label: "cycloplegic" },
] as const

function hasRefractionData(refraction: RefractionDto | undefined): boolean {
  if (!refraction) return false
  const fields: (keyof RefractionDto)[] = [
    "odSph", "odCyl", "odAxis", "odAdd", "odPd",
    "osSph", "osCyl", "osAxis", "osAdd", "osPd",
    "ucvaOd", "ucvaOs", "bcvaOd", "bcvaOs",
    "iopOd", "iopOs", "axialLengthOd", "axialLengthOs",
  ]
  return fields.some((f) => refraction[f] !== null && refraction[f] !== undefined)
}

interface RefractionSectionProps {
  visitId: string
  refractions: RefractionDto[]
  disabled: boolean
}

export function RefractionSection({
  visitId,
  refractions,
  disabled,
}: RefractionSectionProps) {
  const { t } = useTranslation("clinical")

  const getRefractionByType = (type: number): RefractionDto | undefined =>
    refractions.find((r) => r.refractionType === type)

  return (
    <VisitSection title={t("visit.refraction")}>
      <Tabs defaultValue="0">
        <TabsList>
          {REFRACTION_TYPES.map(({ type, label }) => {
            const data = getRefractionByType(type)
            const hasData = hasRefractionData(data)
            return (
              <TabsTrigger key={type} value={String(type)}>
                {t(`refraction.${label}`)}
                {hasData && (
                  <span className="ml-1 text-primary">*</span>
                )}
              </TabsTrigger>
            )
          })}
        </TabsList>
        {REFRACTION_TYPES.map(({ type }) => (
          <TabsContent key={type} value={String(type)} className="mt-4">
            <RefractionForm
              visitId={visitId}
              refractionType={type}
              data={getRefractionByType(type)}
              disabled={disabled}
            />
          </TabsContent>
        ))}
      </Tabs>
    </VisitSection>
  )
}
