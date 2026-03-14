import * as React from "react"
import { useCallback, useEffect, useRef } from "react"
import { Textarea } from "@/shared/components/Textarea"

const AutoResizeTextarea = React.forwardRef<
  HTMLTextAreaElement,
  React.ComponentProps<typeof Textarea>
>(({ onChange, value, style, ...props }, forwardedRef) => {
  const innerRef = useRef<HTMLTextAreaElement | null>(null)

  const resize = useCallback(() => {
    const el = innerRef.current
    if (!el) return
    el.style.height = "auto"
    el.style.height = `${el.scrollHeight}px`
  }, [])

  // Merge refs
  const setRefs = useCallback(
    (node: HTMLTextAreaElement | null) => {
      innerRef.current = node
      if (typeof forwardedRef === "function") {
        forwardedRef(node)
      } else if (forwardedRef) {
        forwardedRef.current = node
      }
    },
    [forwardedRef],
  )

  // Resize on mount and when value changes
  useEffect(() => {
    resize()
  }, [value, resize])

  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLTextAreaElement>) => {
      onChange?.(e)
      resize()
    },
    [onChange, resize],
  )

  return (
    <Textarea
      ref={setRefs}
      value={value}
      onChange={handleChange}
      style={{ overflow: "hidden", ...style }}
      {...props}
    />
  )
})
AutoResizeTextarea.displayName = "AutoResizeTextarea"

export { AutoResizeTextarea }
