/**
 * Format a number as Vietnamese Dong currency.
 * e.g. formatVND(1500000) => "1.500.000 VND" (or locale-specific equivalent)
 */
export function formatVND(amount: number | null | undefined): string {
  if (amount == null) return "0 VND"
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(amount)
}

/**
 * Compact format without currency symbol (for tables).
 * e.g. formatVNDCompact(1500000) => "1.500.000"
 */
export function formatVNDCompact(amount: number | null | undefined): string {
  if (amount == null) return "0"
  return new Intl.NumberFormat("vi-VN", {
    maximumFractionDigits: 0,
  }).format(amount)
}
