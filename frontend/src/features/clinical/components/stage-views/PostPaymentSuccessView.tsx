import { useNavigate } from "@tanstack/react-router"
import {
  IconCheck,
  IconPill,
  IconGlasses,
  IconPrinter,
} from "@tabler/icons-react"
import { Card, CardContent } from "@/shared/components/Card"
import { Badge } from "@/shared/components/Badge"
import { Button } from "@/shared/components/Button"
import { StageDetailShell } from "../StageDetailShell"
import type { VisitDetailDto } from "../../api/clinical-api"

// Track status enum values from backend
const TrackStatus = {
  NotApplicable: 0,
  Pending: 1,
  InProgress: 2,
  Completed: 3,
} as const

interface PostPaymentSuccessViewProps {
  visit: VisitDetailDto
  /** Track statuses from ActiveVisitDto (fetched via useActiveVisits) */
  drugTrackStatus: number
  glassesTrackStatus: number
  paymentAmount?: number
  paymentMethod?: string
  cashierName?: string
}

function formatVND(amount: number): string {
  return new Intl.NumberFormat("vi-VN").format(amount) + " ₫"
}

export function PostPaymentSuccessView({
  visit,
  drugTrackStatus,
  glassesTrackStatus,
  paymentAmount,
  paymentMethod,
  cashierName,
}: PostPaymentSuccessViewProps) {
  const navigate = useNavigate()

  const hasDrugTrack = drugTrackStatus !== TrackStatus.NotApplicable
  const hasGlassesTrack = glassesTrackStatus !== TrackStatus.NotApplicable
  const hasAnyTrack = hasDrugTrack || hasGlassesTrack

  function handlePrintInvoice() {
    // Print invoice — will be wired to print API
    window.print()
  }

  function handlePrintReceipt() {
    // Print receipt — will be wired to print API
    window.print()
  }

  function handleNewPatient() {
    navigate({ to: "/clinical" })
  }

  const bottomBar = (
    <div className="p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={handlePrintInvoice}>
            <IconPrinter className="h-4 w-4 mr-1" />
            In hóa đơn
          </Button>
          <Button variant="outline" onClick={handlePrintReceipt}>
            <IconPrinter className="h-4 w-4 mr-1" />
            Xuất biên lai
          </Button>
        </div>
        <Button onClick={handleNewPatient}>
          Tiếp nhận bệnh nhân mới
        </Button>
      </div>
    </div>
  )

  return (
    <StageDetailShell
      patientName={visit.patientName}
      patientId={visit.patientId}
      doctorName={visit.doctorName}
      visitDate={visit.visitDate}
      stageName="Thu ngân"
      stagePill={{ text: "Đã thu", variant: "green" }}
      bottomBar={bottomBar}
    >
      <div className="space-y-6">
        {/* Success banner */}
        <div className="bg-green-50 border border-green-300 p-4">
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center h-10 w-10 bg-green-100 rounded-full">
              <IconCheck className="h-6 w-6 text-green-600" />
            </div>
            <div>
              <h3 className="font-semibold text-green-800">
                Thanh toán thành công
              </h3>
              <div className="text-sm text-green-700 space-y-0.5 mt-1">
                {paymentAmount !== undefined && (
                  <p>Số tiền: {formatVND(paymentAmount)}</p>
                )}
                {paymentMethod && <p>Phương thức: {paymentMethod}</p>}
                {cashierName && <p>Thu ngân: {cashierName}</p>}
                <p>
                  Thời gian:{" "}
                  {new Date().toLocaleTimeString("vi-VN", {
                    hour: "2-digit",
                    minute: "2-digit",
                  })}
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Next steps */}
        {hasAnyTrack ? (
          <div className="space-y-3">
            <h4 className="text-sm font-semibold text-muted-foreground">
              Bước tiếp theo
            </h4>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              {hasDrugTrack && (
                <Card>
                  <CardContent className="flex items-center gap-3 p-4">
                    <div className="flex items-center justify-center h-10 w-10 bg-blue-50 rounded-full">
                      <IconPill className="h-5 w-5 text-blue-600" />
                    </div>
                    <div className="flex-1">
                      <p className="font-medium">Nhà thuốc</p>
                      <p className="text-xs text-muted-foreground">
                        Phát thuốc theo đơn
                      </p>
                    </div>
                    <Badge className="bg-green-100 text-green-700 border-green-200">
                      Đang chờ
                    </Badge>
                  </CardContent>
                </Card>
              )}
              {hasGlassesTrack && (
                <Card>
                  <CardContent className="flex items-center gap-3 p-4">
                    <div className="flex items-center justify-center h-10 w-10 bg-purple-50 rounded-full">
                      <IconGlasses className="h-5 w-5 text-purple-600" />
                    </div>
                    <div className="flex-1">
                      <p className="font-medium">Kỹ thuật mài lắp kính</p>
                      <p className="text-xs text-muted-foreground">
                        Gia công tròng kính
                      </p>
                    </div>
                    <Badge className="bg-green-100 text-green-700 border-green-200">
                      Đang chờ
                    </Badge>
                  </CardContent>
                </Card>
              )}
            </div>
            {hasGlassesTrack && (
              <p className="text-sm text-muted-foreground italic">
                Kính đang được chuyển đến phòng kỹ thuật.
              </p>
            )}
          </div>
        ) : (
          <Card>
            <CardContent className="p-6 text-center">
              <IconCheck className="h-8 w-8 text-green-500 mx-auto mb-2" />
              <p className="font-medium">
                Lượt khám hoàn tất — không có thuốc hoặc kính cần xử lý.
              </p>
            </CardContent>
          </Card>
        )}
      </div>
    </StageDetailShell>
  )
}
