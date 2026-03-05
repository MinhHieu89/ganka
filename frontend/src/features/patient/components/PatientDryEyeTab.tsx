import { Separator } from "@/shared/components/Separator"
import { OsdiTrendChart } from "./OsdiTrendChart"
import { DryEyeComparisonPanel } from "./DryEyeComparisonPanel"

interface PatientDryEyeTabProps {
  patientId: string
}

export function PatientDryEyeTab({ patientId }: PatientDryEyeTabProps) {
  return (
    <div className="space-y-6">
      <OsdiTrendChart patientId={patientId} />
      <Separator />
      <DryEyeComparisonPanel patientId={patientId} />
    </div>
  )
}
