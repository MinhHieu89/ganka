import { useMemo, useState } from "react"
import {
  createColumnHelper,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
  type SortingState,
} from "@tanstack/react-table"
import { IconEdit, IconPlus, IconChevronLeft, IconChevronRight } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Badge } from "@/shared/components/Badge"
import { DataTable } from "@/shared/components/DataTable"
import { Skeleton } from "@/shared/components/Skeleton"
import { Alert, AlertDescription } from "@/shared/components/Alert"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { useAuthStore } from "@/shared/stores/authStore"
import { useProtocolTemplates } from "@/features/treatment/api/treatment-api"
import type { TreatmentProtocolDto } from "@/features/treatment/api/treatment-types"
import { ProtocolTemplateForm } from "./ProtocolTemplateForm"

const columnHelper = createColumnHelper<TreatmentProtocolDto>()

const ALL_VALUE = "__all__"

const TREATMENT_TYPE_MAP: Record<string, { label: string; variant: "default" | "secondary" | "outline" | "destructive"; className: string }> = {
  IPL: { label: "IPL", variant: "default", className: "bg-blue-500 hover:bg-blue-600" },
  LLLT: { label: "LLLT", variant: "default", className: "bg-purple-500 hover:bg-purple-600" },
  LidCare: { label: "Lid Care", variant: "default", className: "bg-green-500 hover:bg-green-600" },
}

const PRICING_MODE_MAP: Record<string, string> = {
  PerSession: "Per Session",
  PerPackage: "Per Package",
}

function formatVnd(amount: number): string {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(amount)
}

export function ProtocolTemplateList() {
  const { data: templates, isLoading, isError } = useProtocolTemplates()
  const canCreate = useAuthStore(
    (s) => s.user?.permissions?.includes("Treatment.Create") || s.user?.permissions?.includes("Admin"),
  )
  const canManage = useAuthStore(
    (s) => s.user?.permissions?.includes("Treatment.Manage") || s.user?.permissions?.includes("Admin"),
  )
  const [sorting, setSorting] = useState<SortingState>([])
  const [globalFilter, setGlobalFilter] = useState("")
  const [typeFilter, setTypeFilter] = useState<string>(ALL_VALUE)

  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingTemplate, setEditingTemplate] = useState<TreatmentProtocolDto | undefined>()

  const filteredTemplates = useMemo(() => {
    if (!templates) return []
    return templates.filter((t) => {
      if (typeFilter !== ALL_VALUE && t.treatmentType !== typeFilter) return false
      return true
    })
  }, [templates, typeFilter])

  const openCreateDialog = () => {
    setEditingTemplate(undefined)
    setDialogOpen(true)
  }

  const openEditDialog = (template: TreatmentProtocolDto) => {
    setEditingTemplate(template)
    setDialogOpen(true)
  }

  const columns = useMemo(
    () => [
      columnHelper.accessor("name", {
        header: "Name",
        cell: (info) => <span className="font-medium">{info.getValue()}</span>,
        enableSorting: true,
      }),
      columnHelper.accessor("treatmentType", {
        header: "Treatment Type",
        cell: (info) => {
          const type = info.getValue()
          const config = TREATMENT_TYPE_MAP[type]
          if (!config) return <Badge variant="secondary">{type}</Badge>
          return (
            <Badge variant={config.variant} className={config.className}>
              {config.label}
            </Badge>
          )
        },
        enableSorting: true,
      }),
      columnHelper.accessor("defaultSessionCount", {
        header: "Sessions",
        cell: (info) => info.getValue(),
        enableSorting: true,
      }),
      columnHelper.display({
        id: "pricing",
        header: "Pricing",
        cell: ({ row }) => {
          const { pricingMode, defaultPackagePrice, defaultSessionPrice } = row.original
          const modeLabel = PRICING_MODE_MAP[pricingMode] ?? pricingMode
          const price = pricingMode === "PerPackage" ? defaultPackagePrice : defaultSessionPrice
          return (
            <div className="text-sm">
              <span className="text-muted-foreground">{modeLabel}:</span>{" "}
              <span className="font-medium">{formatVnd(price)}</span>
            </div>
          )
        },
      }),
      columnHelper.display({
        id: "interval",
        header: "Interval",
        cell: ({ row }) => {
          const { minIntervalDays, maxIntervalDays } = row.original
          return (
            <span className="text-sm">
              {minIntervalDays}-{maxIntervalDays} days
            </span>
          )
        },
      }),
      columnHelper.accessor("cancellationDeductionPercent", {
        header: "Deduction %",
        cell: (info) => <span>{info.getValue()}%</span>,
        enableSorting: true,
      }),
      columnHelper.accessor("isActive", {
        header: "Active",
        cell: (info) => (
          <Badge variant={info.getValue() ? "default" : "secondary"}>
            {info.getValue() ? "Active" : "Inactive"}
          </Badge>
        ),
        enableSorting: false,
      }),
      ...(canManage
        ? [
            columnHelper.display({
              id: "actions",
              header: "Actions",
              cell: ({ row }) => (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation()
                    openEditDialog(row.original)
                  }}
                  title="Edit template"
                >
                  <IconEdit className="h-4 w-4" />
                </Button>
              ),
            }),
          ]
        : []),
    ],
    [canManage],
  )

  const table = useReactTable({
    data: filteredTemplates,
    columns,
    state: { sorting, globalFilter },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: { pagination: { pageSize: 20 } },
    globalFilterFn: (row, _columnId, filterValue: string) => {
      const search = filterValue.toLowerCase()
      const template = row.original
      return (
        template.name.toLowerCase().includes(search) ||
        template.treatmentType.toLowerCase().includes(search) ||
        (template.description?.toLowerCase().includes(search) ?? false)
      )
    },
  })

  if (isError) {
    return (
      <Alert variant="destructive">
        <AlertDescription>Failed to load protocol templates. Please try again.</AlertDescription>
      </Alert>
    )
  }

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Protocol Templates</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            Manage treatment protocol templates for IPL, LLLT, and Lid Care
          </p>
        </div>
        {canCreate && (
          <Button onClick={openCreateDialog}>
            <IconPlus className="h-4 w-4 mr-2" />
            Create Template
          </Button>
        )}
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <Input
          value={globalFilter}
          onChange={(e) => setGlobalFilter(e.target.value)}
          placeholder="Search templates..."
          className="max-w-sm"
        />

        <Select value={typeFilter} onValueChange={setTypeFilter}>
          <SelectTrigger className="w-40">
            <SelectValue placeholder="Treatment Type" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>All Types</SelectItem>
            <SelectItem value="IPL">IPL</SelectItem>
            <SelectItem value="LLLT">LLLT</SelectItem>
            <SelectItem value="LidCare">Lid Care</SelectItem>
          </SelectContent>
        </Select>

        {(typeFilter !== ALL_VALUE || globalFilter) && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => {
              setTypeFilter(ALL_VALUE)
              setGlobalFilter("")
            }}
          >
            Clear filters
          </Button>
        )}
      </div>

      {/* Data table */}
      {isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <DataTable
          table={table}
          columns={columns}
          emptyMessage="No protocol templates found. Create one to get started."
          onRowClick={(template) => openEditDialog(template)}
        />
      )}

      {/* Pagination */}
      {!isLoading && table.getPageCount() > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            Showing{" "}
            {table.getState().pagination.pageIndex * table.getState().pagination.pageSize + 1}
            {" "}-{" "}
            {Math.min(
              (table.getState().pagination.pageIndex + 1) * table.getState().pagination.pageSize,
              filteredTemplates.length,
            )}{" "}
            of {filteredTemplates.length} templates
          </p>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage()}
            >
              <IconChevronLeft className="h-4 w-4" />
            </Button>
            <span className="text-sm">
              Page {table.getState().pagination.pageIndex + 1} of {table.getPageCount()}
            </span>
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.nextPage()}
              disabled={!table.getCanNextPage()}
            >
              <IconChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}

      {/* Create/Edit dialog */}
      <ProtocolTemplateForm
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        initialData={editingTemplate}
      />
    </div>
  )
}
