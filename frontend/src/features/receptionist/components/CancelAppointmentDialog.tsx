import { useState } from "react"
import { toast } from "sonner"
import { IconAlertTriangle } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/shared/components/Dialog"
import { Button } from "@/shared/components/Button"
import { Label } from "@/shared/components/Label"
import { Textarea } from "@/shared/components/Textarea"
import { Avatar, AvatarFallback } from "@/shared/components/Avatar"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { useCancelAppointment } from "@/features/scheduling/api/scheduling-api"
import { receptionistKeys } from "@/features/receptionist/api/receptionist-api"
import { useQueryClient } from "@tanstack/react-query"
import type { ReceptionistDashboardRow } from "@/features/receptionist/types/receptionist.types"

interface CancelAppointmentDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  row: ReceptionistDashboardRow
}

const CANCEL_REASONS = [
  { value: "0", label: "BN yeu cau huy" },
  { value: "1", label: "BN doi phong kham" },
  { value: "2", label: "Le tan dat nham" },
  { value: "3", label: "Khac" },
] as const

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/)
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

export function CancelAppointmentDialog({
  open,
  onOpenChange,
  row,
}: CancelAppointmentDialogProps) {
  const [reason, setReason] = useState("")
  const [note, setNote] = useState("")
  const queryClient = useQueryClient()
  const cancelAppointment = useCancelAppointment()

  const handleConfirm = () => {
    if (!row.appointmentId || !reason) return

    cancelAppointment.mutate(
      {
        appointmentId: row.appointmentId,
        cancellationReason: Number(reason),
        cancellationNote: note || undefined,
      },
      {
        onSuccess: () => {
          toast.success(`Da huy hen cho ${row.patientName}`)
          queryClient.invalidateQueries({ queryKey: receptionistKeys.all })
          handleClose()
        },
        onError: () => {
          toast.error("Khong the huy hen. Vui long thu lai.")
        },
      },
    )
  }

  const handleClose = () => {
    setReason("")
    setNote("")
    onOpenChange(false)
  }

  const selectedLabel = CANCEL_REASONS.find((r) => r.value === reason)?.label

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold">Huy hen</DialogTitle>
          <DialogDescription className="sr-only">
            Xac nhan huy lich hen cho benh nhan
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Patient avatar with cancel styling */}
          <div className="flex items-center gap-3">
            <Avatar className="h-12 w-12">
              <AvatarFallback
                className="text-sm font-semibold"
                style={{
                  backgroundColor: "var(--avatar-cancel-bg)",
                  color: "var(--avatar-cancel-text)",
                }}
              >
                {getInitials(row.patientName)}
              </AvatarFallback>
            </Avatar>
            <div>
              <div className="text-sm font-semibold">{row.patientName}</div>
              {row.patientCode && (
                <div className="font-mono text-xs text-muted-foreground">
                  {row.patientCode}
                </div>
              )}
            </div>
          </div>

          {/* Red warning */}
          <div
            className="flex items-start gap-2 rounded-md px-3 py-2.5 text-sm"
            style={{
              backgroundColor: "#FDE8E4",
              color: "#A32D2D",
            }}
          >
            <IconAlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
            <span>
              Hen se bi xoa hoan toan. BN se khong con xuat hien tren Dashboard
              ngay nay. Hanh dong khong the hoan tac.
            </span>
          </div>

          {/* Reason dropdown */}
          <div className="space-y-2">
            <Label>
              Ly do huy <span className="text-destructive">*</span>
            </Label>
            <Select value={reason} onValueChange={setReason}>
              <SelectTrigger>
                <SelectValue placeholder="Chon ly do" />
              </SelectTrigger>
              <SelectContent>
                {CANCEL_REASONS.map((r) => (
                  <SelectItem key={r.value} value={r.value}>
                    {r.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Note field when "Khac" selected */}
          {selectedLabel === "Khac" && (
            <div className="space-y-2">
              <Label>Ghi chu</Label>
              <Textarea
                value={note}
                onChange={(e) => setNote(e.target.value)}
                rows={2}
                className="resize-none"
              />
            </div>
          )}
        </div>

        <DialogFooter className="gap-2 sm:justify-end">
          <Button variant="ghost" onClick={handleClose}>
            Huy
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={!reason || cancelAppointment.isPending}
            style={{ backgroundColor: "#A32D2D", color: "white" }}
            className="hover:opacity-90"
          >
            {cancelAppointment.isPending
              ? "Dang xu ly..."
              : "Xac nhan huy hen"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
