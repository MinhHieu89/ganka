import type { ReactNode } from "react"
import { IconChevronDown } from "@tabler/icons-react"
import {
  Collapsible,
  CollapsibleTrigger,
  CollapsibleContent,
} from "@/shared/components/Collapsible"
import {
  Card,
  CardHeader,
  CardTitle,
  CardContent,
} from "@/shared/components/Card"
import { cn } from "@/shared/lib/utils"

interface VisitSectionProps {
  title: string
  defaultOpen?: boolean
  open?: boolean
  onOpenChange?: (open: boolean) => void
  children: ReactNode
  className?: string
  headerExtra?: ReactNode
}

export function VisitSection({
  title,
  defaultOpen = true,
  open,
  onOpenChange,
  children,
  className,
  headerExtra,
}: VisitSectionProps) {
  return (
    <Collapsible defaultOpen={defaultOpen} open={open} onOpenChange={onOpenChange}>
      <Card className={cn(className)}>
        <CardHeader className="flex flex-row items-center justify-between py-3">
          <CollapsibleTrigger asChild>
            <div className="flex items-center gap-2 cursor-pointer flex-1">
              <CardTitle className="text-base">{title}</CardTitle>
              <IconChevronDown className="h-4 w-4 shrink-0 transition-transform [[data-state=open]>&]:rotate-180" />
            </div>
          </CollapsibleTrigger>
          {headerExtra}
        </CardHeader>
        <CollapsibleContent>
          <CardContent className="pt-0">{children}</CardContent>
        </CollapsibleContent>
      </Card>
    </Collapsible>
  )
}
