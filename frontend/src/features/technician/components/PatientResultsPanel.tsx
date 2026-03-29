import { useTranslation } from "react-i18next"
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
} from "@/shared/components/Sheet"
import { Separator } from "@/shared/components/Separator"
import { Badge } from "@/shared/components/Badge"
import type { TechnicianDashboardRow } from "@/features/technician/types/technician.types"
import { IconUser, IconCalendar, IconStethoscope } from "@tabler/icons-react"

interface PatientResultsPanelProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  row: TechnicianDashboardRow | null
}

export function PatientResultsPanel({
  open,
  onOpenChange,
  row,
}: PatientResultsPanelProps) {
  const { t } = useTranslation("technician")

  if (!row) return null

  const checkinDate = row.checkinTime
    ? new Date(row.checkinTime).toLocaleDateString()
    : null

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="right" className="sm:max-w-lg overflow-y-auto">
        <SheetHeader>
          <SheetTitle>{row.patientName}</SheetTitle>
          <SheetDescription>
            {row.patientCode ?? t("panel.noData")}
          </SheetDescription>
        </SheetHeader>

        <div className="mt-6 space-y-6">
          {/* Personal Info Section */}
          <section>
            <div className="flex items-center gap-2 mb-3">
              <IconUser className="h-4 w-4 text-muted-foreground" />
              <h3 className="text-sm font-semibold">
                {t("panel.personalInfo")}
              </h3>
            </div>
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div>
                <span className="text-muted-foreground">
                  {t("panel.patientCode")}
                </span>
                <p className="font-medium">
                  {row.patientCode ?? t("panel.noData")}
                </p>
              </div>
              <div>
                <span className="text-muted-foreground">
                  {t("panel.birthYear")}
                </span>
                <p className="font-medium">
                  {row.birthYear ?? t("panel.noData")}
                </p>
              </div>
            </div>
          </section>

          <Separator />

          {/* Visit History Section */}
          <section>
            <div className="flex items-center gap-2 mb-3">
              <IconCalendar className="h-4 w-4 text-muted-foreground" />
              <h3 className="text-sm font-semibold">
                {t("panel.visitHistory")}
              </h3>
            </div>
            <div className="space-y-2 text-sm">
              {checkinDate && (
                <div className="flex items-center justify-between p-2 bg-muted/50">
                  <span>{t("panel.visitOn", { date: checkinDate })}</span>
                  <Badge variant="outline">{t(`visitType.${row.visitType === "follow_up" ? "followUp" : row.visitType}`)}</Badge>
                </div>
              )}
              {row.reason && (
                <div>
                  <span className="text-muted-foreground">
                    {t("panel.reason")}
                  </span>
                  <p className="font-medium">{row.reason}</p>
                </div>
              )}
            </div>
          </section>

          <Separator />

          {/* Pre-Exam Data Section */}
          <section>
            <div className="flex items-center gap-2 mb-3">
              <IconStethoscope className="h-4 w-4 text-muted-foreground" />
              <h3 className="text-sm font-semibold">
                {t("panel.preExamData")}
              </h3>
            </div>
            <div className="flex items-center justify-center py-8 text-sm text-muted-foreground">
              {t("panel.noPreExamData")}
            </div>
          </section>
        </div>
      </SheetContent>
    </Sheet>
  )
}
