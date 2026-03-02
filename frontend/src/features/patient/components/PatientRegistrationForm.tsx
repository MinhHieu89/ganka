import { useState } from "react"
import { useForm, Controller, useFieldArray } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import { toast } from "sonner"
import {
  IconLoader2,
  IconPlus,
  IconTrash,
  IconStethoscope,
  IconWalk,
} from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Input } from "@/shared/components/Input"
import { Button } from "@/shared/components/Button"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { Tabs, TabsList, TabsTrigger } from "@/shared/components/Tabs"
import { DatePicker } from "@/shared/components/DatePicker"
import {
  useRegisterPatient,
  ALLERGY_CATALOG_BILINGUAL,
  type PatientType,
  type Gender,
  type AllergySeverity,
} from "@/features/patient/api/patient-api"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/shared/components/Popover"
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandItem,
  CommandList,
} from "@/shared/components/Command"

interface PatientRegistrationFormProps {
  open: boolean
  onClose: () => void
}

export function PatientRegistrationForm({
  open,
  onClose,
}: PatientRegistrationFormProps) {
  const { t, i18n } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const navigate = useNavigate()
  const registerMutation = useRegisterPatient()
  const [patientType, setPatientType] = useState<PatientType>("Medical")

  const schema = z.object({
    fullName: z.string().min(1, tCommon("validation.required")),
    phone: z
      .string()
      .min(1, tCommon("validation.required"))
      .regex(/^0\d{9,10}$/, t("invalidPhone") || "Invalid phone number"),
    dateOfBirth: z.date().nullable().optional(),
    gender: z.enum(["Male", "Female", "Other"]).nullable().optional(),
    address: z.string().optional(),
    cccd: z.string().optional(),
    allergies: z
      .array(
        z.object({
          name: z.string().min(1, tCommon("validation.required")),
          severity: z.enum(["Mild", "Moderate", "Severe"]),
        }),
      )
      .optional(),
  })

  type FormValues = z.infer<typeof schema>

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      fullName: "",
      phone: "",
      dateOfBirth: null,
      gender: null,
      address: "",
      cccd: "",
      allergies: [],
    },
  })

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "allergies",
  })

  const handleSubmit = async (data: FormValues) => {
    try {
      const patientId = await registerMutation.mutateAsync({
        fullName: data.fullName,
        phone: data.phone,
        patientType,
        dateOfBirth: data.dateOfBirth
          ? data.dateOfBirth.toISOString()
          : null,
        gender: (patientType === "Medical" ? data.gender : null) as
          | Gender
          | null,
        address: patientType === "Medical" ? data.address : null,
        cccd: patientType === "Medical" ? data.cccd : null,
        allergies:
          data.allergies && data.allergies.length > 0
            ? data.allergies.map((a) => ({
                name: a.name,
                severity: a.severity as AllergySeverity,
              }))
            : null,
      })
      onClose()
      form.reset()
      navigate({
        to: "/patients/$patientId" as string,
        params: { patientId } as never,
      })
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : tCommon("status.error"),
      )
    }
  }

  const handleOpenChange = (v: boolean) => {
    if (!v) {
      onClose()
      form.reset()
      setPatientType("Medical")
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-lg max-h-[90vh] flex flex-col overflow-hidden">
        <DialogHeader className="shrink-0">
          <DialogTitle>{t("register")}</DialogTitle>
          {/* Patient Type Tabs */}
          <Tabs
            value={patientType}
            onValueChange={(v) => setPatientType(v as PatientType)}
            className="mt-2"
          >
            <TabsList className="w-full">
              <TabsTrigger value="Medical" className="flex-1">
                <IconStethoscope className="h-4 w-4 mr-2" />
                {t("medicalPatient")}
              </TabsTrigger>
              <TabsTrigger value="WalkIn" className="flex-1">
                <IconWalk className="h-4 w-4 mr-2" />
                {t("walkInCustomer")}
              </TabsTrigger>
            </TabsList>
          </Tabs>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="flex flex-col flex-1 min-h-0">
        <div className="flex-1 overflow-y-auto space-y-4 px-1">

          {/* Full Name */}
          <Controller
            name="fullName"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>{t("fullName")}</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Phone */}
          <Controller
            name="phone"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel htmlFor={field.name}>{t("phone")}</FieldLabel>
                <Input
                  {...field}
                  id={field.name}
                  placeholder="0901234567"
                  aria-invalid={fieldState.invalid || undefined}
                />
                {fieldState.error && (
                  <FieldError>{fieldState.error.message}</FieldError>
                )}
              </Field>
            )}
          />

          {/* Medical-only fields */}
          {patientType === "Medical" && (
            <>
              {/* Date of Birth */}
              <Controller
                name="dateOfBirth"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid || undefined}>
                    <FieldLabel>{t("dateOfBirth")}</FieldLabel>
                    <DatePicker
                      value={field.value ?? undefined}
                      onChange={field.onChange}
                      placeholder={t("dateOfBirth")}
                      className="w-full"
                    />
                    {fieldState.error && (
                      <FieldError>{fieldState.error.message}</FieldError>
                    )}
                  </Field>
                )}
              />

              {/* Gender */}
              <Controller
                name="gender"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid || undefined}>
                    <FieldLabel>{t("gender")}</FieldLabel>
                    <Select
                      value={field.value ?? ""}
                      onValueChange={field.onChange}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder={t("gender")} />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Male">{t("male")}</SelectItem>
                        <SelectItem value="Female">{t("female")}</SelectItem>
                        <SelectItem value="Other">{t("other")}</SelectItem>
                      </SelectContent>
                    </Select>
                    {fieldState.error && (
                      <FieldError>{fieldState.error.message}</FieldError>
                    )}
                  </Field>
                )}
              />

              {/* Address */}
              <Controller
                name="address"
                control={form.control}
                render={({ field }) => (
                  <Field>
                    <FieldLabel htmlFor={field.name}>{t("address")}</FieldLabel>
                    <Input {...field} id={field.name} />
                  </Field>
                )}
              />

              {/* CCCD */}
              <Controller
                name="cccd"
                control={form.control}
                render={({ field }) => (
                  <Field>
                    <FieldLabel htmlFor={field.name}>{t("cccd")}</FieldLabel>
                    <Input {...field} id={field.name} />
                  </Field>
                )}
              />
            </>
          )}

          {/* Allergies section */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <FieldLabel>{t("allergies")}</FieldLabel>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() =>
                  append({ name: "", severity: "Mild" })
                }
              >
                <IconPlus className="h-3 w-3 mr-1" />
                {t("addAllergy")}
              </Button>
            </div>

            {fields.map((item, index) => (
              <AllergyRow
                key={item.id}
                index={index}
                control={form.control}
                setValue={form.setValue}
                remove={remove}
                t={t}
                i18nLanguage={i18n.language}
              />
            ))}
          </div>
          </div>

          <DialogFooter className="shrink-0 border-t pt-4 mt-2">
            <Button type="button" variant="outline" onClick={onClose}>
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={registerMutation.isPending}>
              {registerMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {t("register")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function AllergyRow({
  index,
  control,
  setValue,
  remove,
  t,
  i18nLanguage,
}: {
  index: number
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  control: any
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setValue: any
  remove: (index: number) => void
  t: (key: string) => string
  i18nLanguage: string
}) {
  const [popoverOpen, setPopoverOpen] = useState(false)
  const [inputValue, setInputValue] = useState("")

  const catalogItems = ALLERGY_CATALOG_BILINGUAL.map((item) => ({
    label: i18nLanguage === "vi" ? item.vi : item.en,
    value: item.en, // always store English name for backend
  }))
  const filtered = catalogItems.filter((item) =>
    item.label.toLowerCase().includes(inputValue.toLowerCase()),
  )

  return (
    <div className="flex items-start gap-2 p-3 border rounded-md bg-muted/30">
      <div className="flex-1 space-y-2">
        <Controller
          name={`allergies.${index}.name`}
          control={control}
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid || undefined}>
              <Popover open={popoverOpen} onOpenChange={setPopoverOpen}>
                <PopoverTrigger asChild>
                  <Input
                    value={field.value as string}
                    placeholder={t("allergyName")}
                    aria-invalid={fieldState.invalid || undefined}
                    onClick={() => setPopoverOpen(true)}
                    onChange={(e) => {
                      field.onChange(e.target.value)
                      setInputValue(e.target.value)
                      setPopoverOpen(true)
                    }}
                    autoComplete="off"
                  />
                </PopoverTrigger>
                <PopoverContent
                  className="w-[--radix-popover-trigger-width] p-0"
                  align="start"
                  onOpenAutoFocus={(e) => e.preventDefault()}
                >
                  <Command>
                    <CommandList>
                      <CommandEmpty>{t("noResults") || "No results"}</CommandEmpty>
                      <CommandGroup>
                        {filtered.slice(0, 8).map((item) => (
                          <CommandItem
                            key={item.value}
                            value={item.label}
                            onSelect={() => {
                              setValue(`allergies.${index}.name` as never, item.value as never)
                              setInputValue(item.label)
                              setPopoverOpen(false)
                            }}
                          >
                            {item.label}
                          </CommandItem>
                        ))}
                      </CommandGroup>
                    </CommandList>
                  </Command>
                </PopoverContent>
              </Popover>
              {fieldState.error && (
                <FieldError>{fieldState.error.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          name={`allergies.${index}.severity`}
          control={control}
          render={({ field }) => (
            <Select
              value={field.value as string}
              onValueChange={field.onChange}
            >
              <SelectTrigger>
                <SelectValue placeholder={t("severity")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Mild">{t("mild")}</SelectItem>
                <SelectItem value="Moderate">{t("moderate")}</SelectItem>
                <SelectItem value="Severe">{t("severe")}</SelectItem>
              </SelectContent>
            </Select>
          )}
        />
      </div>
      <Button
        type="button"
        variant="ghost"
        size="sm"
        onClick={() => remove(index)}
        className="text-destructive hover:text-destructive mt-1"
      >
        <IconTrash className="h-4 w-4" />
      </Button>
    </div>
  )
}
