import { Button } from "@/shared/components/Button"

const SKIP_REASON_LABELS: Record<number, string> = {
  0: "T\u00E1i kh\u00E1m, \u0111\u00E3 c\u00F3 k\u1EBFt qu\u1EA3 c\u0169",
  1: "B\u1EC7nh nh\u00E2n t\u1EEB ch\u1ED1i \u0111o",
  2: "Kh\u00E1m t\u1ED5ng qu\u00E1t, kh\u00F4ng li\u00EAn quan kh\u00FAc x\u1EA1",
  3: "Kh\u00E1c",
}

interface RefractionSkipBannerProps {
  reason: number
  actorName?: string
  skippedAt?: string
  onUndo: () => void
  undoDisabled?: boolean
  isUndoing?: boolean
}

export function RefractionSkipBanner({
  reason,
  actorName,
  skippedAt,
  onUndo,
  undoDisabled,
  isUndoing,
}: RefractionSkipBannerProps) {
  const reasonLabel = SKIP_REASON_LABELS[reason] ?? SKIP_REASON_LABELS[3]

  const timeLabel = skippedAt
    ? new Date(skippedAt).toLocaleTimeString(undefined, {
        hour: "2-digit",
        minute: "2-digit",
      })
    : ""

  const details = [reasonLabel, actorName, timeLabel].filter(Boolean).join(" \u00B7 ")

  return (
    <div className="bg-amber-50 border border-amber-300 px-4 py-3 flex items-center justify-between">
      <span className="text-sm text-amber-800">
        {"\u0110o kh\u00FAc x\u1EA1 \u0111\u00E3 b\u1ECF qua \u2014 L\u00FD do: "}
        {details}
      </span>
      <Button
        variant="outline"
        size="sm"
        onClick={onUndo}
        disabled={undoDisabled || isUndoing}
        className="border-amber-500 text-amber-700 hover:bg-amber-100"
      >
        {isUndoing
          ? "\u0110ang ho\u00E0n t\u00E1c..."
          : "Ho\u00E0n t\u00E1c"}
      </Button>
    </div>
  )
}
