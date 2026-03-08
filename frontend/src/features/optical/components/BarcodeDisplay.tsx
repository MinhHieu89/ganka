import { useEffect, useRef } from "react"
import JsBarcode from "jsbarcode"

interface BarcodeDisplayProps {
  value: string
  width?: number
  height?: number
  showText?: boolean
}

export function BarcodeDisplay({
  value,
  width = 2,
  height = 60,
  showText = true,
}: BarcodeDisplayProps) {
  const svgRef = useRef<SVGSVGElement>(null)
  const isValid = /^\d{13}$/.test(value)

  useEffect(() => {
    if (!svgRef.current || !value) return

    try {
      JsBarcode(svgRef.current, value, {
        format: "EAN13",
        width,
        height,
        displayValue: showText,
        fontSize: 14,
        margin: 10,
      })
    } catch {
      // Invalid barcode value -- fallback rendered via isValid check below
    }
  }, [value, width, height, showText])

  if (!isValid) {
    return (
      <div className="text-muted-foreground flex h-16 items-center justify-center text-sm">
        Invalid barcode: {value || "(empty)"}
      </div>
    )
  }

  return <svg ref={svgRef} />
}
