import { format, formatISO } from "date-fns"
import { TZDate } from "@date-fns/tz"

/**
 * Vietnam timezone identifier (UTC+7).
 * All timestamps from the backend are stored in UTC.
 * This app is used in Vietnam, so we always display in this timezone.
 */
const VIETNAM_TZ = "Asia/Ho_Chi_Minh"

/**
 * Parse a backend UTC timestamp and return a TZDate in Vietnam timezone.
 * Handles both ISO strings with "Z" suffix and without (treats missing as UTC).
 */
export function toVietnamDate(utcDateStr: string): TZDate {
  // Ensure the string is treated as UTC if no timezone indicator present
  const normalized = utcDateStr.endsWith("Z") || utcDateStr.includes("+")
    ? utcDateStr
    : `${utcDateStr}Z`
  return new TZDate(new Date(normalized), VIETNAM_TZ)
}

/**
 * Format a backend UTC timestamp in Vietnam timezone.
 * @param utcDateStr - ISO date string from the backend (UTC)
 * @param formatStr - date-fns format string (default: "dd/MM/yyyy HH:mm")
 */
export function formatVietnamDate(
  utcDateStr: string,
  formatStr: string = "dd/MM/yyyy HH:mm",
): string {
  return format(toVietnamDate(utcDateStr), formatStr)
}

/**
 * Format a Date to "yyyy-MM-dd" using the local date components.
 * Unlike `date.toISOString().split("T")[0]`, this does NOT shift to UTC,
 * so it always returns the date the user actually selected.
 */
export function toLocalDateString(date: Date): string {
  return formatISO(date, { representation: "date" })
}
