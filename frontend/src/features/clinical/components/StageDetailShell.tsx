import type { ReactNode } from "react"
import { Link } from "@tanstack/react-router"
import { IconArrowLeft } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { cn } from "@/shared/lib/utils"

interface StagePill {
  text: string
  variant: "default" | "amber" | "green"
}

interface StageDetailShellProps {
  patientName: string
  patientId: string
  doctorName: string
  visitDate: string
  stageName: string
  stagePill?: StagePill
  children: ReactNode
  bottomBar: ReactNode
  backTo?: string
}

const PILL_STYLES: Record<string, string> = {
  default: "",
  amber: "bg-amber-100 text-amber-800 border-amber-300",
  green: "bg-green-100 text-green-800 border-green-300",
}

export function StageDetailShell({
  patientName,
  patientId,
  doctorName,
  visitDate,
  stageName,
  stagePill,
  children,
  bottomBar,
  backTo = "/clinical",
}: StageDetailShellProps) {
  const formattedDate = new Date(visitDate).toLocaleDateString(undefined, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  })

  return (
    <div className="flex flex-col h-full">
      {/* Top: Patient info card */}
      <div className="border-b bg-background px-4 py-3">
        <div className="flex items-center justify-between max-w-5xl mx-auto">
          <div className="flex items-center gap-3">
            <Link to={backTo}>
              <Button variant="ghost" size="sm">
                <IconArrowLeft className="h-4 w-4 mr-1" />
                Quay lại
              </Button>
            </Link>
            <div>
              <h1 className="text-lg font-semibold">{stageName}</h1>
              <div className="flex items-center gap-4 text-sm text-muted-foreground">
                <Link
                  to="/patients/$patientId"
                  params={{ patientId }}
                  className="text-primary hover:underline font-medium"
                >
                  {patientName}
                </Link>
                <span>BS: {doctorName}</span>
                <span>{formattedDate}</span>
              </div>
            </div>
          </div>
          {stagePill && (
            <Badge
              variant="outline"
              className={cn(PILL_STYLES[stagePill.variant])}
            >
              {stagePill.text}
            </Badge>
          )}
        </div>
      </div>

      {/* Middle: scrollable content */}
      <div className="flex-1 overflow-y-auto p-4">
        <div className="max-w-5xl mx-auto">{children}</div>
      </div>

      {/* Bottom: sticky bottom bar */}
      <div className="sticky bottom-0 border-t bg-background">
        <div className="max-w-5xl mx-auto">{bottomBar}</div>
      </div>
    </div>
  )
}
