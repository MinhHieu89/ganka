import { useState } from "react"
import { useTranslation } from "react-i18next"
import {
  IconArrowLeft,
  IconShieldCheck,
  IconAlertTriangle,
  IconCheck,
  IconClock,
  IconLoader2,
} from "@tabler/icons-react"
import { useRouter } from "@tanstack/react-router"
import { Alert, AlertDescription, AlertTitle } from "@/shared/components/Alert"
import { Badge } from "@/shared/components/Badge"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/Card"
import { Button } from "@/shared/components/Button"
import { Separator } from "@/shared/components/Separator"
import { useGlassesOrderById, useUpdateOrderStatus } from "@/features/optical/api/optical-queries"
import { GLASSES_ORDER_STATUS_MAP, PROCESSING_TYPE_MAP } from "@/features/optical/api/optical-api"
import { OrderStatusBadge } from "./OrderStatusBadge"
import { OverdueOrderAlert } from "./OverdueOrderAlert"
import { formatVND } from "@/shared/lib/format-vnd"
import { cn } from "@/shared/lib/utils"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"

// Status timeline step key mappings
const STATUS_STEP_KEYS = [
  { status: 0, key: "enums.orderStatus.ordered" },
  { status: 1, key: "enums.orderStatus.processing" },
  { status: 2, key: "enums.orderStatus.received" },
  { status: 3, key: "enums.orderStatus.ready" },
  { status: 4, key: "enums.orderStatus.delivered" },
]

// Next status transitions per current status
const NEXT_STATUS_KEYS: Record<number, { status: number; key: string }> = {
  0: { status: 1, key: "orders.startProcessing" },
  1: { status: 2, key: "orders.markReceived" },
  2: { status: 3, key: "orders.markReady" },
  3: { status: 4, key: "orders.markDelivered" },
}

interface GlassesOrderDetailPageProps {
  orderId: string
}

export function GlassesOrderDetailPage({ orderId }: GlassesOrderDetailPageProps) {
  const { t } = useTranslation("optical")
  const router = useRouter()
  const { data: order, isLoading, error } = useGlassesOrderById(orderId)
  const updateStatus = useUpdateOrderStatus()
  const [confirmingAdvance, setConfirmingAdvance] = useState(false)

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-20">
        <IconLoader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    )
  }

  if (error || !order) {
    return (
      <div className="flex flex-col items-center justify-center py-20 gap-4">
        <IconAlertTriangle className="h-12 w-12 text-destructive" />
        <p className="text-lg font-medium text-destructive">{t("common.noData")}</p>
        <p className="text-sm text-muted-foreground">
          {error?.message ?? t("orders.orderNotFound")}
        </p>
      </div>
    )
  }

  const nextStepDef = NEXT_STATUS_KEYS[order.status]
  const nextStep = nextStepDef ? { status: nextStepDef.status, label: t(nextStepDef.key) } : undefined
  const isPaymentGateBlocked =
    order.status === 0 && !order.isPaymentConfirmed && nextStep?.status === 1

  const handleAdvanceStatus = () => {
    if (!nextStep) return
    setConfirmingAdvance(true)
    updateStatus.mutate(
      { id: order.id, newStatus: nextStep.status },
      {
        onSettled: () => setConfirmingAdvance(false),
      },
    )
  }

  // Build order items from the order's frame/lens fields
  type OrderItem = { description: string; unitPrice: number; quantity: number; lineTotal: number }
  const orderItems: OrderItem[] = []
  if (order.frameBrand && order.frameModel) {
    orderItems.push({
      description: `${t("orders.frame")}: ${order.frameBrand} ${order.frameModel}`,
      unitPrice: 0,
      quantity: 1,
      lineTotal: 0,
    })
  }
  if (order.lensName) {
    orderItems.push({
      description: `${t("orders.lens")}: ${order.lensName}`,
      unitPrice: 0,
      quantity: 1,
      lineTotal: 0,
    })
  }
  if (order.comboPackageName) {
    orderItems.push({
      description: `${t("orders.comboPackage")}: ${order.comboPackageName}`,
      unitPrice: order.totalPrice,
      quantity: 1,
      lineTotal: order.totalPrice,
    })
  }

  // Warranty expiry: 12 months from deliveredAt
  const warrantyExpiryDate = order.deliveredAt
    ? new Date(new Date(order.deliveredAt).getTime() + 365.25 * 24 * 60 * 60 * 1000).toISOString()
    : null

  return (
    <div className="container max-w-4xl mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          type="button"
          onClick={() => router.history.back()}
          className="text-muted-foreground hover:text-foreground transition-colors"
          aria-label="Go back"
        >
          <IconArrowLeft className="h-5 w-5" />
        </button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold tracking-tight">
              Order {order.id.substring(0, 8).toUpperCase()}
            </h1>
            <OrderStatusBadge
              status={order.status}
              isOverdue={order.isOverdue}
              isPaymentConfirmed={order.isPaymentConfirmed}
            />
          </div>
          <p className="text-muted-foreground text-sm mt-0.5">
            {t("orders.patient")}: <span className="font-medium text-foreground">{order.patientName}</span>
          </p>
        </div>
      </div>

      {/* Overdue Alert */}
      {order.isOverdue && order.status !== 4 && (
        <OverdueOrderAlert estimatedDate={order.estimatedDeliveryDate} />
      )}

      {/* Warranty Badge */}
      {order.isUnderWarranty && warrantyExpiryDate && (
        <div className="flex items-center gap-2 p-3 rounded-lg bg-green-50 dark:bg-green-950/20 border border-green-200 dark:border-green-800">
          <IconShieldCheck className="h-5 w-5 text-green-600 dark:text-green-400" />
          <span className="text-green-700 dark:text-green-300 text-sm font-medium">
            {t("warranty.withinWarranty")}
          </span>
          <span className="text-green-600 dark:text-green-400 text-sm">
            &mdash; Expires{" "}
            {new Date(warrantyExpiryDate).toLocaleDateString("vi-VN", {
              year: "numeric",
              month: "long",
              day: "numeric",
            })}
          </span>
        </div>
      )}

      {/* Status Timeline */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">{t("orders.status")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-0">
            {STATUS_STEP_KEYS.map((step, index) => {
              const isCompleted = order.status > step.status
              const isCurrent = order.status === step.status
              const isLast = index === STATUS_STEP_KEYS.length - 1

              return (
                <div key={step.status} className="flex items-center flex-1">
                  <div className="flex flex-col items-center gap-1.5">
                    <div
                      className={cn(
                        "h-8 w-8 rounded-full flex items-center justify-center border-2 transition-colors",
                        isCompleted
                          ? "bg-primary border-primary text-primary-foreground"
                          : isCurrent
                            ? "bg-primary/10 border-primary text-primary"
                            : "bg-muted border-muted-foreground/20 text-muted-foreground",
                      )}
                    >
                      {isCompleted ? (
                        <IconCheck className="h-4 w-4" />
                      ) : isCurrent ? (
                        <IconClock className="h-4 w-4" />
                      ) : (
                        <span className="text-xs font-medium">{index + 1}</span>
                      )}
                    </div>
                    <span
                      className={cn(
                        "text-xs font-medium text-center leading-tight whitespace-nowrap",
                        isCurrent
                          ? "text-primary"
                          : isCompleted
                            ? "text-foreground"
                            : "text-muted-foreground",
                      )}
                    >
                      {t(step.key)}
                    </span>
                  </div>

                  {!isLast && (
                    <div
                      className={cn(
                        "flex-1 h-0.5 mx-1 mb-5 transition-colors",
                        isCompleted ? "bg-primary" : "bg-muted-foreground/20",
                      )}
                    />
                  )}
                </div>
              )
            })}
          </div>
        </CardContent>
      </Card>

      {/* Advance Status Action */}
      {nextStep && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t("orders.status")}</CardTitle>
            <CardDescription>
              {t("orders.currentStatus")}:{" "}
              <span className="font-medium">
                {t(GLASSES_ORDER_STATUS_MAP[order.status] ?? `Status ${order.status}`)}
              </span>
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {isPaymentGateBlocked && (
              <Alert variant="destructive">
                <IconAlertTriangle className="h-4 w-4" />
                <AlertTitle>{t("orders.paymentStatus")}</AlertTitle>
                <AlertDescription>
                  {t("orders.paymentRequired")}
                </AlertDescription>
              </Alert>
            )}

            <div className="flex items-center gap-3">
              <Button
                onClick={handleAdvanceStatus}
                disabled={isPaymentGateBlocked || confirmingAdvance || updateStatus.isPending}
              >
                {(confirmingAdvance || updateStatus.isPending) && (
                  <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
                )}
                {nextStep.label}
              </Button>

              {isPaymentGateBlocked && (
                <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                  <IconAlertTriangle className="h-4 w-4 text-destructive" />
                  {t("orders.paymentRequired")}
                </span>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Order Details */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t("common.details")}</CardTitle>
          </CardHeader>
          <CardContent>
            <dl className="space-y-3">
              <div className="flex justify-between items-center">
                <dt className="text-sm text-muted-foreground">{t("orders.processingType")}</dt>
                <dd className="text-sm font-medium">
                  {t(PROCESSING_TYPE_MAP[order.processingType] ??
                    `Type ${order.processingType}`)}
                </dd>
              </div>
              <Separator />
              <div className="flex justify-between items-center">
                <dt className="text-sm text-muted-foreground">{t("orders.estimatedDelivery")}</dt>
                <dd className="text-sm font-medium">
                  {order.estimatedDeliveryDate ? (
                    <span
                      className={cn(
                        order.isOverdue && order.status !== 4
                          ? "text-destructive font-semibold"
                          : "",
                      )}
                    >
                      {new Date(order.estimatedDeliveryDate).toLocaleDateString("vi-VN")}
                    </span>
                  ) : (
                    <span className="text-muted-foreground">—</span>
                  )}
                </dd>
              </div>
              <Separator />
              <div className="flex justify-between items-center">
                <dt className="text-sm text-muted-foreground">{t("orders.createdAt")}</dt>
                <dd className="text-sm font-medium">
                  {new Date(order.createdAt).toLocaleDateString("vi-VN")}
                </dd>
              </div>
              {order.deliveredAt && (
                <>
                  <Separator />
                  <div className="flex justify-between items-center">
                    <dt className="text-sm text-muted-foreground">{t("orders.deliveredAt")}</dt>
                    <dd className="text-sm font-medium">
                      {new Date(order.deliveredAt).toLocaleDateString("vi-VN")}
                    </dd>
                  </div>
                </>
              )}
              <Separator />
              <div className="flex justify-between items-center">
                <dt className="text-sm text-muted-foreground">{t("orders.totalPrice")}</dt>
                <dd className="text-sm font-bold tabular-nums">{formatVND(order.totalPrice)}</dd>
              </div>
            </dl>
          </CardContent>
        </Card>

        {/* Payment Status */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t("orders.paymentStatus")}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-3">
              {order.isPaymentConfirmed ? (
                <>
                  <div className="h-10 w-10 rounded-full bg-green-100 dark:bg-green-950/30 flex items-center justify-center">
                    <IconCheck className="h-5 w-5 text-green-600 dark:text-green-400" />
                  </div>
                  <div>
                    <p className="font-medium text-green-700 dark:text-green-400">
                      {t("enums.warrantyApproval.approved")}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      {t("orders.processingStarted")}
                    </p>
                  </div>
                </>
              ) : (
                <>
                  <div className="h-10 w-10 rounded-full bg-orange-100 dark:bg-orange-950/30 flex items-center justify-center">
                    <IconAlertTriangle className="h-5 w-5 text-orange-600 dark:text-orange-400" />
                  </div>
                  <div>
                    <p className="font-medium text-orange-700 dark:text-orange-400">
                      {t("enums.warrantyApproval.pending")}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      {t("orders.paymentRequired")}
                    </p>
                  </div>
                </>
              )}
            </div>

            {!order.isPaymentConfirmed && (
              <Alert className="mt-4">
                <AlertDescription className="text-sm">
                  {t("orders.billingNote")}
                </AlertDescription>
              </Alert>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Items Table */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">{t("orders.title")}</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {orderItems.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t("combos.description")}</TableHead>
                  <TableHead className="text-right">{t("common.price")}</TableHead>
                  <TableHead className="text-right">{t("common.quantity")}</TableHead>
                  <TableHead className="text-right">{t("orders.totalPrice")}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {orderItems.map((item, index) => (
                  <TableRow key={index}>
                    <TableCell className="font-medium">{item.description}</TableCell>
                    <TableCell className="text-right tabular-nums">
                      {item.unitPrice > 0 ? formatVND(item.unitPrice) : "-"}
                    </TableCell>
                    <TableCell className="text-right">{item.quantity}</TableCell>
                    <TableCell className="text-right tabular-nums font-medium">
                      {item.lineTotal > 0 ? formatVND(item.lineTotal) : "-"}
                    </TableCell>
                  </TableRow>
                ))}
                <TableRow className="bg-muted/30">
                  <TableCell colSpan={3} className="font-semibold text-right">
                    {t("orders.totalPrice")}
                  </TableCell>
                  <TableCell className="text-right tabular-nums font-bold">
                    {formatVND(order.totalPrice)}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          ) : (
            <div className="px-6 py-8 text-center text-muted-foreground text-sm">
              {t("orders.noItems")}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Notes */}
      {order.notes && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t("orders.notes")}</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground whitespace-pre-wrap">{order.notes}</p>
          </CardContent>
        </Card>
      )}

      {/* Warranty Info (if delivered) */}
      {order.status === 4 && warrantyExpiryDate && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base flex items-center gap-2">
              <IconShieldCheck className="h-4 w-4" />
              {t("warranty.title")}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <dl className="space-y-2">
              <div className="flex justify-between">
                <dt className="text-sm text-muted-foreground">{t("warranty.warrantyExpiry")}</dt>
                <dd className="text-sm font-medium">12 months from delivery</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-sm text-muted-foreground">{t("warranty.warrantyExpiry")}</dt>
                <dd className="text-sm font-medium">
                  {new Date(warrantyExpiryDate).toLocaleDateString("vi-VN", {
                    year: "numeric",
                    month: "long",
                    day: "numeric",
                  })}
                </dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-sm text-muted-foreground">{t("orders.status")}</dt>
                <dd>
                  {order.isUnderWarranty ? (
                    <Badge className="bg-green-100 text-green-800 border-green-200 dark:bg-green-950/40 dark:text-green-300">
                      Active
                    </Badge>
                  ) : (
                    <Badge variant="secondary">{t("warranty.warrantyExpired")}</Badge>
                  )}
                </dd>
              </div>
            </dl>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
