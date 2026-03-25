import { useState, useMemo } from "react"
import { toast } from "sonner"
import { IconClock, IconReceipt } from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Input } from "@/shared/components/Input"
import { Separator } from "@/shared/components/Separator"
import { StageDetailShell } from "../StageDetailShell"
import { StageBottomBar } from "../StageBottomBar"
import {
  PaymentMethodSelector,
  type PaymentMethod,
} from "./PaymentMethodSelector"
import {
  useConfirmVisitPayment,
  type VisitDetailDto,
} from "../../api/clinical-api"

// Payment method enum mapping (matches backend)
const PAYMENT_METHOD_MAP: Record<PaymentMethod, number> = {
  cash: 0,
  card: 1,
  transfer: 2,
}

function formatVND(amount: number): string {
  return new Intl.NumberFormat("vi-VN").format(amount) + " ₫"
}

// --- Invoice line item types ---

interface InvoiceLineItem {
  name: string
  subNote?: string
  quantity: number
  unitPrice: number
}

interface InvoiceSection {
  title: string
  items: InvoiceLineItem[]
}

// --- Build invoice from visit data ---

function buildInvoiceSections(visit: VisitDetailDto): InvoiceSection[] {
  const sections: InvoiceSection[] = []

  // Examination services (Stage 1 service selection)
  sections.push({
    title: "Dịch vụ khám",
    items: [
      {
        name: "Khám mắt tổng quát",
        subNote: undefined,
        quantity: 1,
        unitPrice: 300000,
      },
    ],
  })

  // Imaging services (Stage 4a)
  if (visit.imagingRequested) {
    sections.push({
      title: "Dịch vụ CĐHA",
      items: [
        {
          name: "Chẩn đoán hình ảnh",
          subNote: "Hai mắt",
          quantity: 1,
          unitPrice: 200000,
        },
      ],
    })
  }

  // Medications (Stage 5 drug prescription)
  if (visit.drugPrescriptions.length > 0) {
    const drugItems: InvoiceLineItem[] = []
    for (const rx of visit.drugPrescriptions) {
      for (const item of rx.items) {
        drugItems.push({
          name: item.drugName,
          subNote: item.strength ?? undefined,
          quantity: item.quantity,
          unitPrice: 0, // Price from drug catalog; wired when backend provides pricing
        })
      }
    }
    if (drugItems.length > 0) {
      sections.push({ title: "Thuốc", items: drugItems })
    }
  }

  // Glasses (OpticalCenter completed before Cashier — single combined invoice)
  if (visit.opticalPrescriptions.length > 0) {
    const glassesItems: InvoiceLineItem[] = []
    for (const rx of visit.opticalPrescriptions) {
      glassesItems.push({
        name: "Tròng kính",
        subNote: `OD: ${rx.odSph ?? 0}/${rx.odCyl ?? 0} OS: ${rx.osSph ?? 0}/${rx.osCyl ?? 0}`,
        quantity: 2,
        unitPrice: 0, // Price from optical order; wired when backend provides pricing
      })
      glassesItems.push({
        name: "Gọng kính",
        subNote: undefined,
        quantity: 1,
        unitPrice: 0, // Price from optical order; wired when backend provides pricing
      })
    }
    if (glassesItems.length > 0) {
      sections.push({ title: "Kính", items: glassesItems })
    }
  }

  return sections
}

function computeTotal(sections: InvoiceSection[]): number {
  return sections.reduce(
    (sum, section) =>
      sum +
      section.items.reduce(
        (s, item) => s + item.quantity * item.unitPrice,
        0,
      ),
    0,
  )
}

// --- Elapsed wait time ---

function getElapsedMinutes(signedAt: string | null): number | null {
  if (!signedAt) return null
  const diff = Date.now() - new Date(signedAt).getTime()
  return Math.floor(diff / 60000)
}

// --- Component ---

interface Stage6CashierViewProps {
  visit: VisitDetailDto
}

type Phase = "invoice" | "payment"

export function Stage6CashierView({ visit }: Stage6CashierViewProps) {
  const [phase, setPhase] = useState<Phase>("invoice")
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>("cash")
  const [amountReceivedStr, setAmountReceivedStr] = useState("")

  const confirmPayment = useConfirmVisitPayment()

  const sections = useMemo(() => buildInvoiceSections(visit), [visit])
  const totalAmount = useMemo(() => computeTotal(sections), [sections])
  const amountReceived = Number(amountReceivedStr) || 0
  const changeDue = amountReceived - totalAmount
  const canConfirm = amountReceived >= totalAmount && totalAmount > 0

  const elapsedMin = getElapsedMinutes(visit.signedAt)

  function handleConfirmPayment() {
    confirmPayment.mutate(
      {
        visitId: visit.id,
        amount: totalAmount,
        paymentMethod: PAYMENT_METHOD_MAP[paymentMethod],
        amountReceived,
        splitGlasses: false, // Single combined invoice per CONFIRMATION_2.md Q1
      },
      {
        onSuccess: () => {
          toast.success("Đã xác nhận thu tiền thành công")
        },
        onError: () => {
          toast.error("Lỗi khi xác nhận thu tiền")
        },
      },
    )
  }

  // --- Invoice phase ---

  function renderInvoice() {
    return (
      <div className="space-y-4">
        {/* Elapsed wait time */}
        {elapsedMin !== null && elapsedMin > 15 && (
          <div className="flex items-center gap-2 text-amber-600 bg-amber-50 border border-amber-200 px-3 py-2">
            <IconClock className="h-4 w-4" />
            <span className="text-sm">
              Đã chờ {elapsedMin} phút kể từ ký đơn
            </span>
          </div>
        )}

        {/* Invoice card */}
        <Card>
          <CardHeader className="pb-3">
            <div className="flex items-center gap-2">
              <IconReceipt className="h-5 w-5 text-muted-foreground" />
              <CardTitle className="text-base">Hóa đơn</CardTitle>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            {sections.map((section) => (
              <div key={section.title}>
                <h4 className="text-sm font-semibold text-muted-foreground mb-2">
                  {section.title}
                </h4>
                <div className="space-y-1">
                  {section.items.map((item, idx) => (
                    <div
                      key={`${section.title}-${idx}`}
                      className="flex items-center justify-between text-sm"
                    >
                      <div className="flex-1">
                        <span>{item.name}</span>
                        {item.subNote && (
                          <span className="text-muted-foreground ml-2 text-xs">
                            ({item.subNote})
                          </span>
                        )}
                        {item.quantity > 1 && (
                          <span className="text-muted-foreground ml-1">
                            x{item.quantity}
                          </span>
                        )}
                      </div>
                      <span className="font-medium">
                        {formatVND(item.quantity * item.unitPrice)}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            ))}

            <Separator />

            {/* Total */}
            <div className="flex items-center justify-between font-bold text-lg">
              <span>Tổng cộng</span>
              <span>{formatVND(totalAmount)}</span>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  // --- Payment phase ---

  function renderPayment() {
    return (
      <div className="space-y-6">
        {/* Amount due */}
        <div className="text-center">
          <p className="text-sm text-muted-foreground mb-1">Số tiền cần thu</p>
          <p className="text-2xl font-bold">{formatVND(totalAmount)}</p>
        </div>

        {/* Payment method selector */}
        <div>
          <p className="text-sm font-medium mb-2">Phương thức thanh toán</p>
          <PaymentMethodSelector
            value={paymentMethod}
            onChange={setPaymentMethod}
          />
        </div>

        {/* Amount received input */}
        <div>
          <label
            htmlFor="amount-received"
            className="text-sm font-medium block mb-1"
          >
            Số tiền nhận
          </label>
          <Input
            id="amount-received"
            type="number"
            className="text-lg h-12"
            value={amountReceivedStr}
            onChange={(e) => setAmountReceivedStr(e.target.value)}
            min={0}
          />
        </div>

        {/* Change calculation */}
        {amountReceivedStr !== "" && (
          <div className="text-center">
            {changeDue < 0 ? (
              <p className="text-red-600 font-medium">
                Còn thiếu: {formatVND(Math.abs(changeDue))}
              </p>
            ) : (
              <p className="text-green-600 font-medium">
                Tiền thừa: {formatVND(changeDue)}
              </p>
            )}
          </div>
        )}
      </div>
    )
  }

  // --- Bottom bar ---

  const bottomBar =
    phase === "invoice" ? (
      <StageBottomBar
        primaryButton={{
          label: "Thu tiền \u203A",
          onClick: () => setPhase("payment"),
        }}
      />
    ) : (
      <StageBottomBar
        secondaryButton={{
          label: "\u2039 Quay lại hóa đơn",
          onClick: () => setPhase("invoice"),
        }}
        primaryButton={{
          label: "Xác nhận đã thu tiền",
          onClick: handleConfirmPayment,
          disabled: !canConfirm || confirmPayment.isPending,
        }}
      />
    )

  return (
    <StageDetailShell
      patientName={visit.patientName}
      patientId={visit.patientId}
      doctorName={visit.doctorName}
      visitDate={visit.visitDate}
      stageName="Thu ngân"
      stagePill={{
        text: phase === "invoice" ? "Chưa thu" : "Đang thu",
        variant: phase === "invoice" ? "amber" : "green",
      }}
      bottomBar={bottomBar}
    >
      {phase === "invoice" ? renderInvoice() : renderPayment()}
    </StageDetailShell>
  )
}
