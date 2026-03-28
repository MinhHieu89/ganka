import { useState, useRef, useCallback } from "react"
import { format, parse, isValid, type Locale } from "date-fns"
import { vi, enUS } from "date-fns/locale"
import { useTranslation } from "react-i18next"
import { IconCalendar } from "@tabler/icons-react"
import { cn } from "@/shared/lib/utils"
import { Input } from "@/shared/components/Input"
import { Popover, PopoverContent, PopoverTrigger } from "@/shared/components/Popover"
import { Calendar } from "@/shared/components/ui/calendar"

interface DatePickerProps {
  value?: Date
  onChange?: (date: Date | undefined) => void
  placeholder?: string
  disabled?: boolean
  className?: string
  /** Earliest selectable date (defaults to 1920-01-01) */
  fromDate?: Date
  /** Latest selectable date (defaults to today) */
  toDate?: Date
}

const localeMap: Record<string, Locale> = {
  vi,
  en: enUS,
}

/**
 * Apply dd/mm/yyyy mask to raw digits.
 * Accepts only digits, auto-inserts slashes.
 */
function applyDateMask(raw: string): string {
  const digits = raw.replace(/\D/g, "").slice(0, 8)
  if (digits.length <= 2) return digits
  if (digits.length <= 4) return `${digits.slice(0, 2)}/${digits.slice(2)}`
  return `${digits.slice(0, 2)}/${digits.slice(2, 4)}/${digits.slice(4)}`
}

function dateToString(date: Date | undefined, pattern: string, loc: Locale): string {
  return date ? format(date, pattern, { locale: loc }) : ""
}

export function DatePicker({
  value,
  onChange,
  placeholder,
  disabled,
  className,
  fromDate,
  toDate,
}: DatePickerProps) {
  const { i18n } = useTranslation("common")
  const locale = localeMap[i18n.language] ?? vi

  const formatPattern = i18n.language === "vi" ? "dd/MM/yyyy" : "MM/dd/yyyy"
  const displayPlaceholder = placeholder ?? formatPattern.toLowerCase()

  const defaultFrom = fromDate ?? new Date(1920, 0, 1)
  const defaultTo = toDate ?? new Date()

  const [open, setOpen] = useState(false)
  const [inputValue, setInputValue] = useState(() => dateToString(value, formatPattern, locale))
  const isFocused = useRef(false)
  const lastExternalValue = useRef(value?.getTime())

  // Sync from external value only when not focused and value actually changed
  const externalTime = value?.getTime()
  if (externalTime !== lastExternalValue.current) {
    lastExternalValue.current = externalTime
    if (!isFocused.current) {
      setInputValue(dateToString(value, formatPattern, locale))
    }
  }

  const tryParse = useCallback(
    (text: string): Date | null => {
      if (text.length !== 10) return null
      const parsed = parse(text, formatPattern, new Date())
      if (!isValid(parsed)) return null
      if (parsed < defaultFrom || parsed > defaultTo) return null
      return parsed
    },
    [formatPattern, defaultFrom, defaultTo],
  )

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const masked = applyDateMask(e.target.value)
    setInputValue(masked)

    if (masked.length === 10) {
      const parsed = tryParse(masked)
      if (parsed) {
        onChange?.(parsed)
        lastExternalValue.current = parsed.getTime()
      }
    } else if (masked === "") {
      onChange?.(undefined)
      lastExternalValue.current = undefined
    }
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Backspace" && inputRef.current) {
      const pos = inputRef.current.selectionStart ?? 0
      if (pos > 0 && inputValue[pos - 1] === "/") {
        e.preventDefault()
        const digits = inputValue.replace(/\D/g, "")
        const digitsBefore = inputValue.slice(0, pos - 1).replace(/\D/g, "").length
        const newDigits = digits.slice(0, digitsBefore - 1) + digits.slice(digitsBefore)
        const newMasked = applyDateMask(newDigits)
        setInputValue(newMasked)

        requestAnimationFrame(() => {
          const newPos = Math.max(0, pos - 2)
          inputRef.current?.setSelectionRange(newPos, newPos)
        })
      }
    }
  }

  const handleFocus = () => {
    isFocused.current = true
  }

  const handleBlur = () => {
    isFocused.current = false

    if (!inputValue) {
      onChange?.(undefined)
      lastExternalValue.current = undefined
      return
    }

    const parsed = tryParse(inputValue)
    if (parsed) {
      onChange?.(parsed)
      lastExternalValue.current = parsed.getTime()
      setInputValue(format(parsed, formatPattern, { locale }))
    } else {
      // Incomplete or invalid — reset to current value
      setInputValue(dateToString(value, formatPattern, locale))
    }
  }

  const handleCalendarSelect = (date: Date | undefined) => {
    onChange?.(date)
    lastExternalValue.current = date?.getTime()
    setInputValue(dateToString(date, formatPattern, locale))
    setOpen(false)
    setTimeout(() => inputRef.current?.focus(), 0)
  }

  const inputRef = useRef<HTMLInputElement>(null)

  return (
    <div className={cn("relative", className)}>
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <button
            type="button"
            disabled={disabled}
            className="absolute left-3 top-1/2 z-10 -translate-y-1/2 text-muted-foreground hover:text-foreground"
            tabIndex={-1}
          >
            <IconCalendar className="h-4 w-4" />
          </button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="single"
            selected={value}
            onSelect={handleCalendarSelect}
            locale={locale}
            captionLayout="dropdown"
            startMonth={defaultFrom}
            endMonth={defaultTo}
            defaultMonth={value ?? undefined}
          />
        </PopoverContent>
      </Popover>
      <Input
        ref={inputRef}
        value={inputValue}
        onChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onFocus={handleFocus}
        onBlur={handleBlur}
        placeholder={displayPlaceholder}
        disabled={disabled}
        className="pl-9"
      />
    </div>
  )
}
