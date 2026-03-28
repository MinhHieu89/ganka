import { useTranslation } from "react-i18next"
import { Link, useRouterState } from "@tanstack/react-router"
import { SidebarTrigger } from "@/shared/components/Sidebar"
import { Separator } from "@/shared/components/Separator"
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/shared/components/Breadcrumb"
import { LanguageToggle } from "@/shared/components/LanguageToggle"
import { useRecentPatientsStore } from "@/shared/stores/recentPatientsStore"
import { GlobalSearch } from "@/shared/components/GlobalSearch"

/**
 * Map of route path segments to i18n keys under "sidebar.*" or "topbar.*" namespace.
 */
const segmentToI18nKey: Record<string, string> = {
  dashboard: "sidebar.dashboard",
  admin: "sidebar.admin",
  users: "sidebar.users",
  roles: "sidebar.roles",
  "audit-logs": "sidebar.auditLogs",
  settings: "sidebar.settings",
  patients: "sidebar.patients",
  appointments: "sidebar.appointments",
  clinical: "sidebar.clinical",
  visits: "sidebar.clinical",
  pharmacy: "sidebar.pharmacy",
  "drug-catalog": "sidebar.pharmacyDrugCatalog",
  queue: "sidebar.pharmacyQueue",
  suppliers: "sidebar.pharmacySuppliers",
  "stock-import": "sidebar.pharmacyStockImport",
  "otc-sales": "sidebar.pharmacyOtcSales",
  consumables: "sidebar.consumables",
  optical: "sidebar.optical",
  frames: "sidebar.opticalFrames",
  lenses: "sidebar.opticalLenses",
  orders: "sidebar.opticalOrders",
  combos: "sidebar.opticalCombos",
  warranty: "sidebar.opticalWarranty",
  stocktaking: "sidebar.opticalStocktaking",
  billing: "sidebar.billing",
  shifts: "sidebar.billingShifts",
  invoices: "sidebar.billingInvoices",
  treatments: "sidebar.treatment",
  approvals: "sidebar.treatment",
  templates: "sidebar.treatment",
  new: "sidebar.new",
  intake: "sidebar.intake",
}

export function SiteHeader() {
  const { t } = useTranslation("common")
  const routerState = useRouterState()
  const currentPath = routerState.location.pathname
  const recentPatients = useRecentPatientsStore((s) => s.recent)

  // Build breadcrumb segments from current path
  const segments = currentPath
    .split("/")
    .filter(Boolean)
    .filter((s) => s !== "_authenticated") // filter layout route segments

  // UUID regex for detecting dynamic route params
  const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

  const breadcrumbs = segments.map((segment, index) => {
    const i18nKey = segmentToI18nKey[segment]
    let label: string
    if (i18nKey) {
      label = t(i18nKey)
    } else if (uuidRegex.test(segment)) {
      // For UUID segments under /patients, look up the patient name
      const prevSegment = index > 0 ? segments[index - 1] : null
      if (prevSegment === "patients") {
        const patient = recentPatients.find((p) => p.id === segment)
        label = patient ? patient.fullName : (t("sidebar.detail") ?? "Detail")
      } else {
        label = t("sidebar.detail") ?? "Detail"
      }
    } else {
      label = segment.charAt(0).toUpperCase() + segment.slice(1)
    }
    const isLast = index === segments.length - 1
    let path = "/" + segments.slice(0, index + 1).join("/")
    // Override certain segments to redirect to a different route
    const segmentPathOverride: Record<string, string> = {
      visits: "/clinical",
    }
    if (segmentPathOverride[segment]) {
      path = segmentPathOverride[segment]
    }
    // Redirect intermediate segments that have no standalone route to their parent
    const noStandaloneRoute = ["approvals", "templates"]
    if (noStandaloneRoute.includes(segment) && !isLast) {
      path = "/" + segments.slice(0, index).join("/")
    }
    return { label, path, isLast }
  })

  return (
    <header className="flex h-(--header-height) shrink-0 items-center gap-2 border-b border-border/50 bg-background/80 backdrop-blur-sm px-4 lg:px-6 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-(--header-height)">
      <SidebarTrigger className="-ml-1" />
      <Separator orientation="vertical" className="mx-2 data-[orientation=vertical]:h-4" />
      <Breadcrumb>
        <BreadcrumbList>
          {breadcrumbs.map((crumb, index) => (
            <span key={crumb.path} className="contents">
              {index > 0 && <BreadcrumbSeparator />}
              <BreadcrumbItem>
                {crumb.isLast ? (
                  <BreadcrumbPage className="font-medium">{crumb.label}</BreadcrumbPage>
                ) : (
                  <BreadcrumbLink asChild><Link to={crumb.path}>{crumb.label}</Link></BreadcrumbLink>
                )}
              </BreadcrumbItem>
            </span>
          ))}
        </BreadcrumbList>
      </Breadcrumb>

      <div className="flex-1" />

      <div className="flex items-center gap-2">
        <GlobalSearch />
        <LanguageToggle />
      </div>
    </header>
  )
}
