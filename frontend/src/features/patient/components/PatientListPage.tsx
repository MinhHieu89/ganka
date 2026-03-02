import { useState } from "react"
import { useTranslation } from "react-i18next"
import type { PaginationState } from "@tanstack/react-table"
import { IconPlus, IconSearch } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Field, FieldLabel } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { DatePicker } from "@/shared/components/DatePicker"
import { usePatientList, type Gender } from "@/features/patient/api/patient-api"
import { PatientTable } from "@/features/patient/components/PatientTable"
import { PatientRegistrationForm } from "@/features/patient/components/PatientRegistrationForm"

const ALL = "__all__"

export function PatientListPage() {
  const { t } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const [registerOpen, setRegisterOpen] = useState(false)

  // Filter state
  const [genderFilter, setGenderFilter] = useState<string>(ALL)
  const [allergyFilter, setAllergyFilter] = useState<string>(ALL)
  const [dateFrom, setDateFrom] = useState<Date | undefined>(undefined)
  const [dateTo, setDateTo] = useState<Date | undefined>(undefined)
  const [searchTerm, setSearchTerm] = useState("")

  // Pagination state
  const [pagination, setPagination] = useState<PaginationState>({
    pageIndex: 0,
    pageSize: 20,
  })

  const patientList = usePatientList({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    gender: genderFilter !== ALL ? (genderFilter as Gender) : null,
    hasAllergies:
      allergyFilter === ALL
        ? null
        : allergyFilter === "yes",
    dateFrom: dateFrom ? dateFrom.toISOString() : null,
    dateTo: dateTo ? dateTo.toISOString() : null,
  })

  const patients = patientList.data?.items ?? []
  const totalCount = patientList.data?.totalCount ?? 0

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            {t("title")}
          </h1>
          <p className="text-sm text-muted-foreground mt-1">{t("list")}</p>
        </div>
        <Button onClick={() => setRegisterOpen(true)}>
          <IconPlus className="h-4 w-4 mr-2" />
          {t("register")}
        </Button>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-end gap-4 p-4 border rounded-lg bg-muted/30">
        {/* Search */}
        <Field className="flex-1 min-w-[200px]">
          <FieldLabel>{tCommon("buttons.search")}</FieldLabel>
          <div className="relative">
            <IconSearch className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder={t("search")}
              className="pl-9"
            />
          </div>
        </Field>

        {/* Gender filter */}
        <Field className="w-[160px]">
          <FieldLabel>{t("gender")}</FieldLabel>
          <Select
            value={genderFilter}
            onValueChange={(v) => {
              setGenderFilter(v)
              setPagination((p) => ({ ...p, pageIndex: 0 }))
            }}
          >
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={ALL}>{t("all")}</SelectItem>
              <SelectItem value="Male">{t("male")}</SelectItem>
              <SelectItem value="Female">{t("female")}</SelectItem>
              <SelectItem value="Other">{t("other")}</SelectItem>
            </SelectContent>
          </Select>
        </Field>

        {/* Has Allergies filter */}
        <Field className="w-[160px]">
          <FieldLabel>{t("hasAllergies")}</FieldLabel>
          <Select
            value={allergyFilter}
            onValueChange={(v) => {
              setAllergyFilter(v)
              setPagination((p) => ({ ...p, pageIndex: 0 }))
            }}
          >
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={ALL}>{t("all")}</SelectItem>
              <SelectItem value="yes">{t("yes")}</SelectItem>
              <SelectItem value="no">{t("no")}</SelectItem>
            </SelectContent>
          </Select>
        </Field>

        {/* Date range */}
        <Field className="w-auto">
          <FieldLabel>{t("dateRange")}</FieldLabel>
          <div className="flex items-center gap-2">
            <DatePicker
              value={dateFrom}
              onChange={(d) => {
                setDateFrom(d)
                setPagination((p) => ({ ...p, pageIndex: 0 }))
              }}
              placeholder={tCommon("buttons.search")}
            />
            <span className="text-muted-foreground">-</span>
            <DatePicker
              value={dateTo}
              onChange={(d) => {
                setDateTo(d)
                setPagination((p) => ({ ...p, pageIndex: 0 }))
              }}
              placeholder={tCommon("buttons.search")}
            />
          </div>
        </Field>
      </div>

      {/* Table */}
      <PatientTable
        data={patients}
        totalCount={totalCount}
        pagination={pagination}
        onPaginationChange={setPagination}
      />

      {/* Registration Dialog */}
      <PatientRegistrationForm
        open={registerOpen}
        onClose={() => setRegisterOpen(false)}
      />
    </div>
  )
}
