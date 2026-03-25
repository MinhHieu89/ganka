import { createFileRoute } from "@tanstack/react-router"
import { IconLoader2 } from "@tabler/icons-react"
import { useVisitById } from "@/features/clinical/api/clinical-api"
import { Stage2RefractionView } from "@/features/clinical/components/stage-views/Stage2RefractionView"
import { requirePermission } from "@/shared/utils/permission-guard"

export const Route = createFileRoute(
  "/_authenticated/clinical/visit/$visitId/stage",
)({
  beforeLoad: () => requirePermission("Clinical.View"),
  component: StageRoute,
})

// WorkflowStage enum values
const STAGE_LABELS: Record<number, string> = {
  0: "Ti\u1EBFp nh\u1EADn",
  1: "\u0110o kh\u00FAc x\u1EA1",
  2: "Kh\u00E1m b\u00E1c s\u0129",
  3: "Ch\u1EA9n \u0111o\u00E1n h\u00ECnh \u1EA3nh",
  4: "BS \u0111\u1ECDc k\u1EBFt qu\u1EA3",
  5: "K\u00EA \u0111\u01A1n",
  6: "Thu ng\u00E2n",
  7: "Ph\u00E1t thu\u1ED1c",
  8: "Quang h\u1ECDc",
  9: "X\u01B0\u1EDFng k\u00EDnh",
  10: "Tr\u1EA3 k\u00EDnh",
  99: "Ho\u00E0n t\u1EA5t",
}

function StageRoute() {
  const { visitId } = Route.useParams()
  const { data: visit, isLoading, error } = useVisitById(visitId)

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <IconLoader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    )
  }

  if (error || !visit) {
    return (
      <div className="flex items-center justify-center h-64 text-muted-foreground">
        {`Kh\u00F4ng t\u00ECm th\u1EA5y l\u01B0\u1EE3t kh\u00E1m`}
      </div>
    )
  }

  // Route to appropriate stage view based on currentStage
  switch (visit.currentStage) {
    case 1: // RefractionVA
      return <Stage2RefractionView visit={visit} />

    default: {
      // Placeholder for stages not yet implemented
      const stageLabel = STAGE_LABELS[visit.currentStage] ?? `Stage ${visit.currentStage}`
      return (
        <div className="flex items-center justify-center h-64">
          <div className="text-center text-muted-foreground">
            <p className="text-lg font-medium">{stageLabel}</p>
            <p className="text-sm mt-1">
              {`Ch\u1EE9c n\u0103ng n\u00E0y \u0111ang \u0111\u01B0\u1EE3c ph\u00E1t tri\u1EC3n`}
            </p>
          </div>
        </div>
      )
    }
  }
}
