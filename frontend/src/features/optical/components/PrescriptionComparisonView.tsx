import { useTranslation } from "react-i18next"
import { format } from "date-fns"
import { IconArrowUp, IconArrowDown, IconEqual, IconGitCompare } from "@tabler/icons-react"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import { Badge } from "@/shared/components/Badge"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Skeleton } from "@/shared/components/Skeleton"
import { usePrescriptionComparison } from "@/features/optical/api/optical-queries"
import type { PrescriptionComparisonDto } from "@/features/optical/api/optical-api"

interface PrescriptionComparisonViewProps {
  patientId: string
  prescriptionId1: string
  prescriptionId2: string
}

type ChangeDirection = "improved" | "worsened" | "unchanged"

/**
 * Determines if a SPH change is an improvement.
 * SPH improvement = absolute value closer to 0 (less myopic or hyperopic).
 */
function sphChangeDirection(
  older: number | null,
  newer: number | null,
): ChangeDirection {
  if (older == null || newer == null || older === newer) return "unchanged"
  const olderAbs = Math.abs(older)
  const newerAbs = Math.abs(newer)
  if (newerAbs < olderAbs) return "improved"
  if (newerAbs > olderAbs) return "worsened"
  return "unchanged"
}

/**
 * Determines if a CYL change is an improvement.
 * CYL improvement = absolute value closer to 0 (less astigmatism).
 */
function cylChangeDirection(
  older: number | null,
  newer: number | null,
): ChangeDirection {
  if (older == null || newer == null || older === newer) return "unchanged"
  const olderAbs = Math.abs(older)
  const newerAbs = Math.abs(newer)
  if (newerAbs < olderAbs) return "improved"
  if (newerAbs > olderAbs) return "worsened"
  return "unchanged"
}

/**
 * For AXIS and ADD, simply mark whether value changed (not "improved" vs "worsened").
 */
function neutralChangeDirection(
  older: number | null,
  newer: number | null,
): ChangeDirection {
  if (older == null || newer == null) return "unchanged"
  return older === newer ? "unchanged" : "worsened" // "changed" shown as worsened for neutral fields
}

interface ChangeIndicatorProps {
  direction: ChangeDirection
}

function ChangeIndicator({ direction }: ChangeIndicatorProps) {
  if (direction === "improved") {
    return (
      <span className="inline-flex items-center gap-1 text-green-600">
        <IconArrowUp className="h-3.5 w-3.5" />
        <span className="sr-only">Improved</span>
      </span>
    )
  }
  if (direction === "worsened") {
    return (
      <span className="inline-flex items-center gap-1 text-red-500">
        <IconArrowDown className="h-3.5 w-3.5" />
        <span className="sr-only">Worsened</span>
      </span>
    )
  }
  return (
    <span className="inline-flex items-center gap-1 text-muted-foreground">
      <IconEqual className="h-3.5 w-3.5" />
      <span className="sr-only">Unchanged</span>
    </span>
  )
}

function formatValue(value: number | null, decimals = 2): string {
  if (value == null) return "—"
  if (decimals === 0) return `${value}°`
  return `${value >= 0 ? "+" : ""}${value.toFixed(decimals)}`
}

interface ComparisonTableProps {
  comparison: PrescriptionComparisonDto
}

function ComparisonTable({ comparison }: ComparisonTableProps) {
  const { t } = useTranslation("optical")
  const { prescription1: older, prescription2: newer } = comparison

  type ComparisonRow = {
    label: string
    olderValue: number | null
    newerValue: number | null
    decimals?: number
    directionFn: (a: number | null, b: number | null) => ChangeDirection
  }

  const rows: ComparisonRow[] = [
    {
      label: `${t("prescriptions.sph")} OD`,
      olderValue: older.rightSph,
      newerValue: newer.rightSph,
      directionFn: sphChangeDirection,
    },
    {
      label: `${t("prescriptions.cyl")} OD`,
      olderValue: older.rightCyl,
      newerValue: newer.rightCyl,
      directionFn: cylChangeDirection,
    },
    {
      label: `${t("prescriptions.axis")} OD`,
      olderValue: older.rightAxis,
      newerValue: newer.rightAxis,
      decimals: 0,
      directionFn: neutralChangeDirection,
    },
    {
      label: `${t("prescriptions.add")} OD`,
      olderValue: older.rightAdd,
      newerValue: newer.rightAdd,
      directionFn: neutralChangeDirection,
    },
    {
      label: `${t("prescriptions.sph")} OS`,
      olderValue: older.leftSph,
      newerValue: newer.leftSph,
      directionFn: sphChangeDirection,
    },
    {
      label: `${t("prescriptions.cyl")} OS`,
      olderValue: older.leftCyl,
      newerValue: newer.leftCyl,
      directionFn: cylChangeDirection,
    },
    {
      label: `${t("prescriptions.axis")} OS`,
      olderValue: older.leftAxis,
      newerValue: newer.leftAxis,
      decimals: 0,
      directionFn: neutralChangeDirection,
    },
    {
      label: `${t("prescriptions.add")} OS`,
      olderValue: older.leftAdd,
      newerValue: newer.leftAdd,
      directionFn: neutralChangeDirection,
    },
    {
      label: t("prescriptions.pd"),
      olderValue: older.pupillaryDistance,
      newerValue: newer.pupillaryDistance,
      directionFn: neutralChangeDirection,
    },
  ]

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead className="w-28">Field</TableHead>
          <TableHead>
            <div className="text-sm">
              <div className="font-semibold">{t("prescriptions.previousPrescription")}</div>
              <div className="text-xs font-normal text-muted-foreground">
                {format(new Date(older.prescribedAt), "PP")}
              </div>
            </div>
          </TableHead>
          <TableHead>
            <div className="text-sm">
              <div className="font-semibold">{t("prescriptions.currentPrescription")}</div>
              <div className="text-xs font-normal text-muted-foreground">
                {format(new Date(newer.prescribedAt), "PP")}
              </div>
            </div>
          </TableHead>
          <TableHead className="w-20 text-center">{t("prescriptions.changeDirection")}</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {rows.map((row) => {
          const direction = row.directionFn(row.olderValue, row.newerValue)
          return (
            <TableRow key={row.label}>
              <TableCell className="font-medium text-muted-foreground">{row.label}</TableCell>
              <TableCell className="font-mono">{formatValue(row.olderValue, row.decimals)}</TableCell>
              <TableCell className="font-mono">{formatValue(row.newerValue, row.decimals)}</TableCell>
              <TableCell className="text-center">
                <ChangeIndicator direction={direction} />
              </TableCell>
            </TableRow>
          )
        })}
      </TableBody>
    </Table>
  )
}

function ComparisonSummary({ comparison }: ComparisonTableProps) {
  const { t } = useTranslation("optical")

  const summaryItems: Array<{ label: string; direction: ChangeDirection }> = [
    {
      label: `SPH OD`,
      direction: sphChangeDirection(comparison.prescription1.rightSph, comparison.prescription2.rightSph),
    },
    {
      label: `CYL OD`,
      direction: cylChangeDirection(comparison.prescription1.rightCyl, comparison.prescription2.rightCyl),
    },
    {
      label: `SPH OS`,
      direction: sphChangeDirection(comparison.prescription1.leftSph, comparison.prescription2.leftSph),
    },
    {
      label: `CYL OS`,
      direction: cylChangeDirection(comparison.prescription1.leftCyl, comparison.prescription2.leftCyl),
    },
  ].filter((item) => item.direction !== "unchanged")

  if (summaryItems.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">{t("prescriptions.noChange")}</p>
    )
  }

  return (
    <div className="flex flex-wrap gap-2">
      {summaryItems.map((item) => (
        <Badge
          key={item.label}
          variant={item.direction === "improved" ? "default" : "destructive"}
          className="gap-1"
        >
          {item.direction === "improved" ? (
            <IconArrowUp className="h-3 w-3" />
          ) : (
            <IconArrowDown className="h-3 w-3" />
          )}
          {item.label} {item.direction}
        </Badge>
      ))}
    </div>
  )
}

export function PrescriptionComparisonView({
  patientId,
  prescriptionId1,
  prescriptionId2,
}: PrescriptionComparisonViewProps) {
  const { t } = useTranslation("optical")
  const { data: comparison, isLoading } = usePrescriptionComparison({
    patientId,
    id1: prescriptionId1,
    id2: prescriptionId2,
  })

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-6 w-48" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-64 w-full" />
        </CardContent>
      </Card>
    )
  }

  if (!comparison) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          {t("prescriptions.noComparison")}
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <IconGitCompare className="h-5 w-5 text-primary" />
          <CardTitle className="text-base">{t("prescriptions.yearOverYear")}</CardTitle>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Summary badges */}
        <div>
          <p className="mb-2 text-xs font-medium uppercase tracking-wide text-muted-foreground">
            Overall Change
          </p>
          <ComparisonSummary comparison={comparison} />
        </div>

        {/* Detailed table */}
        <ComparisonTable comparison={comparison} />
      </CardContent>
    </Card>
  )
}
