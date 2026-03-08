import { useMemo } from "react"
import { format } from "date-fns"
import {
  IconCalendar,
  IconUser,
  IconAlertTriangle,
  IconNotes,
} from "@tabler/icons-react"
import { Card, CardContent, CardHeader } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { cn } from "@/shared/lib/utils"
import type { TreatmentSessionDto } from "../api/treatment-types"

// -- OSDI severity helpers --

const OSDI_SEVERITY_CONFIG: Record<
  string,
  { label: string; color: string; borderColor: string }
> = {
  Normal: {
    label: "Normal",
    color: "bg-green-100 text-green-800 border-green-300",
    borderColor: "border-l-green-500",
  },
  Mild: {
    label: "Mild",
    color: "bg-yellow-100 text-yellow-800 border-yellow-300",
    borderColor: "border-l-yellow-500",
  },
  Moderate: {
    label: "Moderate",
    color: "bg-orange-100 text-orange-800 border-orange-300",
    borderColor: "border-l-orange-500",
  },
  Severe: {
    label: "Severe",
    color: "bg-red-100 text-red-800 border-red-300",
    borderColor: "border-l-red-500",
  },
}

const SESSION_STATUS_VARIANT: Record<
  string,
  "default" | "secondary" | "outline" | "destructive"
> = {
  Scheduled: "outline",
  InProgress: "secondary",
  Completed: "default",
  Cancelled: "destructive",
}

// -- Parameter display helpers per treatment type --

interface ParsedParams {
  [key: string]: unknown
}

function parseParametersJson(json: string | null | undefined): ParsedParams {
  if (!json) return {}
  try {
    return JSON.parse(json) as ParsedParams
  } catch {
    return {}
  }
}

function IplParams({ params }: { params: ParsedParams }) {
  return (
    <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
      {params.energy != null && (
        <div>
          <span className="text-muted-foreground">Energy:</span>{" "}
          <span className="font-medium">{String(params.energy)} J/cm²</span>
        </div>
      )}
      {params.pulses != null && (
        <div>
          <span className="text-muted-foreground">Pulses:</span>{" "}
          <span className="font-medium">{String(params.pulses)}</span>
        </div>
      )}
      {params.spotSize != null && (
        <div>
          <span className="text-muted-foreground">Spot Size:</span>{" "}
          <span className="font-medium">{String(params.spotSize)} mm</span>
        </div>
      )}
      {params.zones != null && (
        <div>
          <span className="text-muted-foreground">Zones:</span>{" "}
          <span className="font-medium">{String(params.zones)}</span>
        </div>
      )}
    </div>
  )
}

function LlltParams({ params }: { params: ParsedParams }) {
  return (
    <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
      {params.wavelength != null && (
        <div>
          <span className="text-muted-foreground">Wavelength:</span>{" "}
          <span className="font-medium">{String(params.wavelength)} nm</span>
        </div>
      )}
      {params.power != null && (
        <div>
          <span className="text-muted-foreground">Power:</span>{" "}
          <span className="font-medium">{String(params.power)} mW</span>
        </div>
      )}
      {params.duration != null && (
        <div>
          <span className="text-muted-foreground">Duration:</span>{" "}
          <span className="font-medium">{String(params.duration)} min</span>
        </div>
      )}
      {params.area != null && (
        <div>
          <span className="text-muted-foreground">Area:</span>{" "}
          <span className="font-medium">{String(params.area)}</span>
        </div>
      )}
    </div>
  )
}

function LidCareParams({ params }: { params: ParsedParams }) {
  return (
    <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
      {params.steps != null && (
        <div>
          <span className="text-muted-foreground">Steps:</span>{" "}
          <span className="font-medium">{String(params.steps)}</span>
        </div>
      )}
      {params.products != null && (
        <div>
          <span className="text-muted-foreground">Products:</span>{" "}
          <span className="font-medium">{String(params.products)}</span>
        </div>
      )}
      {params.duration != null && (
        <div>
          <span className="text-muted-foreground">Duration:</span>{" "}
          <span className="font-medium">{String(params.duration)} min</span>
        </div>
      )}
    </div>
  )
}

function GenericParams({ params }: { params: ParsedParams }) {
  const entries = Object.entries(params)
  if (entries.length === 0) return null
  return (
    <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
      {entries.map(([key, value]) => (
        <div key={key}>
          <span className="text-muted-foreground">{key}:</span>{" "}
          <span className="font-medium">{String(value)}</span>
        </div>
      ))}
    </div>
  )
}

function DeviceParameters({
  treatmentType,
  parametersJson,
}: {
  treatmentType: string
  parametersJson: string | null | undefined
}) {
  const params = parseParametersJson(parametersJson)
  if (Object.keys(params).length === 0) return null

  switch (treatmentType) {
    case "IPL":
      return <IplParams params={params} />
    case "LLLT":
      return <LlltParams params={params} />
    case "LidCare":
      return <LidCareParams params={params} />
    default:
      return <GenericParams params={params} />
  }
}

// -- Main component --

interface TreatmentSessionCardProps {
  session: TreatmentSessionDto
  treatmentType: string
}

export function TreatmentSessionCard({
  session,
  treatmentType,
}: TreatmentSessionCardProps) {
  const severityConfig = useMemo(() => {
    if (!session.osdiSeverity) return null
    return OSDI_SEVERITY_CONFIG[session.osdiSeverity] ?? null
  }, [session.osdiSeverity])

  const sessionDate = session.completedAt ?? session.scheduledAt ?? session.createdAt

  return (
    <Card
      className={cn(
        "border-l-4",
        severityConfig?.borderColor ?? "border-l-muted-foreground/30",
      )}
    >
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold">
              Session #{session.sessionNumber}
            </span>
            <Badge variant={SESSION_STATUS_VARIANT[session.status] ?? "outline"}>
              {session.status}
            </Badge>
          </div>
          <div className="flex items-center gap-1 text-xs text-muted-foreground">
            <IconCalendar className="h-3.5 w-3.5" />
            {format(new Date(sessionDate), "dd/MM/yyyy")}
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-3 pt-0">
        {/* Device parameters */}
        <DeviceParameters
          treatmentType={treatmentType}
          parametersJson={session.parametersJson}
        />

        {/* OSDI Score */}
        {session.osdiScore != null && (
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">OSDI:</span>
            <span className="text-sm font-bold tabular-nums">
              {Number(session.osdiScore).toFixed(1)}
            </span>
            {severityConfig && (
              <Badge
                variant="outline"
                className={cn("text-xs border", severityConfig.color)}
              >
                {severityConfig.label}
              </Badge>
            )}
          </div>
        )}

        {/* Clinical notes */}
        {session.clinicalNotes && (
          <div className="flex items-start gap-1.5 text-sm">
            <IconNotes className="h-4 w-4 mt-0.5 text-muted-foreground flex-shrink-0" />
            <p className="text-muted-foreground">{session.clinicalNotes}</p>
          </div>
        )}

        {/* Performed by */}
        {session.performedByName && (
          <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <IconUser className="h-3.5 w-3.5" />
            <span>{session.performedByName}</span>
          </div>
        )}

        {/* Consumables */}
        {session.consumables.length > 0 && (
          <div className="text-sm">
            <span className="text-muted-foreground">Consumables:</span>
            <ul className="mt-1 space-y-0.5">
              {session.consumables.map((c) => (
                <li
                  key={c.id}
                  className="text-xs text-muted-foreground ml-4 list-disc"
                >
                  {c.consumableName} x{c.quantity}
                </li>
              ))}
            </ul>
          </div>
        )}

        {/* Interval override warning */}
        {session.intervalOverrideReason && (
          <div className="flex items-start gap-1.5 text-xs rounded-md bg-yellow-50 dark:bg-yellow-950/30 p-2 border border-yellow-200 dark:border-yellow-800">
            <IconAlertTriangle className="h-3.5 w-3.5 text-yellow-600 mt-0.5 flex-shrink-0" />
            <span className="text-yellow-800 dark:text-yellow-200">
              Interval override: {session.intervalOverrideReason}
            </span>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
