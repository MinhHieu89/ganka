import { format, type Locale } from "date-fns"
import { vi, enUS } from "date-fns/locale"
import { useTranslation } from "react-i18next"
import { IconCalendar } from "@tabler/icons-react"
import { cn } from "@/shared/lib/utils"
import { Button } from "@/shared/components/Button"
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

export function DatePicker({
  value,
  onChange,
  placeholder,
  disabled,
  className,
  fromDate,
  toDate,
}: DatePickerProps) {
  const { i18n, t } = useTranslation("common")
  const locale = localeMap[i18n.language] ?? vi

  const formatPattern = i18n.language === "vi" ? "dd/MM/yyyy" : "MM/dd/yyyy"
  const displayPlaceholder = placeholder ?? t("buttons.search")

  const defaultFrom = fromDate ?? new Date(1920, 0, 1)
  const defaultTo = toDate ?? new Date()

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          disabled={disabled}
          className={cn(
            "justify-start text-left font-normal",
            !value && "text-muted-foreground",
            className,
          )}
        >
          <IconCalendar className="mr-2 h-4 w-4" />
          {value ? format(value, formatPattern, { locale }) : displayPlaceholder}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar
          mode="single"
          selected={value}
          onSelect={onChange}
          locale={locale}
          captionLayout="dropdown"
          startMonth={defaultFrom}
          endMonth={defaultTo}
          defaultMonth={value ?? undefined}
        />
      </PopoverContent>
    </Popover>
  )
}
