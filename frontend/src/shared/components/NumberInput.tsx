import * as React from "react"
import { cn } from "@/shared/lib/utils"

/**
 * NumberInput — a drop-in replacement for `<Input type="number">` that works
 * correctly with react-hook-form's `Controller` `{...field}` pattern.
 *
 * Fixes:
 * - No leading zeros (cleans on blur)
 * - Allows typing negative values (no premature conversion)
 * - Works with `{...field}` spread — no custom onChange needed
 *
 * Usage with Controller:
 * ```tsx
 * <Controller
 *   name="quantity"
 *   control={form.control}
 *   render={({ field }) => (
 *     <NumberInput {...field} step={1} min={0} />
 *   )}
 * />
 * ```
 */

interface NumberInputProps
  extends Omit<React.ComponentProps<"input">, "type" | "onChange" | "value"> {
  value?: number | string
  onChange?: (value: number) => void
}

const NumberInput = React.forwardRef<HTMLInputElement, NumberInputProps>(
  ({ className, value, onChange, onBlur, min, max, step, ...props }, ref) => {
    const [raw, setRaw] = React.useState(() =>
      value == null || value === "" ? "" : String(value),
    )

    // Sync from external value changes (e.g., form.reset)
    React.useEffect(() => {
      const externalStr = value == null || value === "" ? "" : String(value)
      // Only sync if the numeric value actually differs to avoid cursor jumps
      const rawNum = raw === "" || raw === "-" ? NaN : parseFloat(raw)
      const extNum = externalStr === "" ? NaN : parseFloat(externalStr)
      if (
        (Number.isNaN(rawNum) && Number.isNaN(extNum)) ||
        rawNum === extNum
      ) {
        return
      }
      setRaw(externalStr)
    }, [value]) // eslint-disable-line react-hooks/exhaustive-deps

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const v = e.target.value
      setRaw(v)

      // Only propagate valid numbers
      if (v === "" || v === "-" || v === "." || v === "-.") return
      const num = parseFloat(v)
      if (!Number.isNaN(num)) {
        onChange?.(num)
      }
    }

    const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
      // Clean up the display value on blur
      if (raw === "" || raw === "-") {
        setRaw("0")
        onChange?.(0)
      } else {
        const num = parseFloat(raw)
        if (!Number.isNaN(num)) {
          // Apply min/max clamping
          let clamped = num
          if (min != null) clamped = Math.max(Number(min), clamped)
          if (max != null) clamped = Math.min(Number(max), clamped)

          // Apply step snapping if step is defined
          if (step != null) {
            const s = Number(step)
            if (s > 0) {
              clamped = Math.round(clamped / s) * s
              // Fix floating point precision
              const decimals = (String(s).split(".")[1] || "").length
              clamped = parseFloat(clamped.toFixed(decimals))
            }
          }

          setRaw(String(clamped))
          if (clamped !== num) onChange?.(clamped)
        } else {
          setRaw("0")
          onChange?.(0)
        }
      }

      onBlur?.(e)
    }

    return (
      <input
        type="number"
        inputMode="decimal"
        className={cn(
          "flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-base ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 md:text-sm",
          className,
        )}
        ref={ref}
        value={raw}
        onChange={handleChange}
        onBlur={handleBlur}
        min={min}
        max={max}
        step={step}
        {...props}
      />
    )
  },
)
NumberInput.displayName = "NumberInput"

export { NumberInput }
export type { NumberInputProps }
