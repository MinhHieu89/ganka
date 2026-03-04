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
  children: ReactNode
  className?: string
  headerExtra?: ReactNode
}

export function VisitSection({
  title,
  defaultOpen = true,
  children,
  className,
  headerExtra,
}: VisitSectionProps) {
  return (
    <Collapsible defaultOpen={defaultOpen}>
      <Card className={cn(className)}>
        <CollapsibleTrigger asChild>
          <CardHeader className="cursor-pointer flex flex-row items-center justify-between py-3">
            <div className="flex items-center gap-2">
              <CardTitle className="text-base">{title}</CardTitle>
              {headerExtra}
            </div>
            <IconChevronDown className="h-4 w-4 shrink-0 transition-transform [[data-state=open]>&]:rotate-180" />
          </CardHeader>
        </CollapsibleTrigger>
        <CollapsibleContent>
          <CardContent className="pt-0">{children}</CardContent>
        </CollapsibleContent>
      </Card>
    </Collapsible>
  )
}
