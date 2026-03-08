import { createFileRoute } from "@tanstack/react-router"
import { ProtocolTemplateList } from "@/features/treatment/components/ProtocolTemplateList"

export const Route = createFileRoute("/_authenticated/treatments/templates")({
  component: ProtocolTemplateList,
})
