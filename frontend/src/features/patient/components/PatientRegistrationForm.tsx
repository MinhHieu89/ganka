import { useState } from "react"
import { useForm, Controller, useFieldArray } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import { toast } from "sonner"
import {
  IconCheck,
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
  CommandInput,
  CommandItem,
  CommandList,
} from "@/shared/components/Command"
import { cn } from "@/shared/lib/utils"
import { handleServerValidationError } from "@/shared/lib/server-validation"
import { ServerValidationAlert } from "@/shared/components/ServerValidationAlert"

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
  const [nonFieldError, setNonFieldError] = useState<string | null>(null)

  const schema = z.object({
    fullName: z.string()
      .min(3, t("validation.fullNameMin"))
      .max(50, t("validation.fullNameMax")),
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
  }).superRefine((data, ctx) => {
    if (patientType === "Medical") {
      if (!data.dateOfBirth) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: t("validation.dobRequired"),
          path: ["dateOfBirth"],
        })
      }
      if (!data.gender) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: t("validation.genderRequired"),
          path: ["gender"],
        })
      }
    }
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
      const nonFieldErrors = handleServerValidationError(error, form.setError, {
        Phone: "phone" as const,
      })
      // Override phone duplicate error with localized message
      const errorMessage = error instanceof Error ? error.message : ""
      if (
        errorMessage.toLowerCase().includes("phone") &&
        errorMessage.toLowerCase().includes("already exists")
      ) {
        form.setError("phone", {
          type: "server",
          message: t("validation.phoneDuplicate"),
        })
      }
      if (nonFieldErrors.length > 0) {
        setNonFieldError(nonFieldErrors[0])
      }
    }
  }

  const handleOpenChange = (v: boolean) => {
    if (!v) {
      onClose()
      form.reset()
      setPatientType("Medical")
      setNonFieldError(null)
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

          <ServerValidationAlert
            error={nonFieldError}
            onDismiss={() => setNonFieldError(null)}
          />

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
                watch={form.watch}
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

const categoryKeyMap: Record<string, string> = {
  "Ophthalmic Drug": "allergyCategory.ophthalmicDrug",
  "General Drug": "allergyCategory.generalDrug",
  Material: "allergyCategory.material",
  Environmental: "allergyCategory.environmental",
}

function AllergyRow({
  index,
  control,
  setValue,
  watch,
  remove,
  t,
  i18nLanguage,
}: {
  index: number
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  control: any
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setValue: any
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  watch: any
  remove: (index: number) => void
  t: (key: string) => string
  i18nLanguage: string
}) {
  const [popoverOpen, setPopoverOpen] = useState(false)
  const [searchValue, setSearchValue] = useState("")

  const catalogItems = ALLERGY_CATALOG_BILINGUAL.map((item) => ({
    label: i18nLanguage === "vi" ? item.vi : item.en,
    value: item.en, // always store English name for backend
    category: item.category,
    categoryLabel: t(categoryKeyMap[item.category] ?? item.category),
  }))

  const filtered = searchValue.trim()
    ? catalogItems.filter((item) =>
        item.label.toLowerCase().includes(searchValue.toLowerCase()),
      )
    : catalogItems

  const hasExactMatch = catalogItems.some(
    (item) =>
      item.label.toLowerCase() === searchValue.toLowerCase() ||
      item.value.toLowerCase() === searchValue.toLowerCase(),
  )

  const nameValue = watch(`allergies.${index}.name`)

  // Display label: find the catalog item matching the stored English name
  const displayLabel = nameValue
    ? (catalogItems.find((item) => item.value === nameValue)?.label ?? nameValue)
    : ""

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
                  <div>
                    <Button
                      type="button"
                      variant="outline"
                      role="combobox"
                      className={cn(
                        "w-full justify-start text-left font-normal",
                        !field.value && "text-muted-foreground",
                      )}
                      aria-invalid={fieldState.invalid || undefined}
                    >
                      {displayLabel || t("allergyName")}
                    </Button>
                  </div>
                </PopoverTrigger>
                <PopoverContent
                  className="w-[--radix-popover-trigger-width] p-0"
                  align="start"
                  onOpenAutoFocus={(e) => e.preventDefault()}
                  onWheel={(e) => e.stopPropagation()}
                >
                  <Command shouldFilter={false}>
                    <CommandInput
                      placeholder={t("allergyName")}
                      value={searchValue}
                      onValueChange={setSearchValue}
                    />
                    <CommandList className="max-h-52 overflow-y-auto">
                      {filtered.length === 0 && !searchValue.trim() && (
                        <CommandEmpty>
                          {t("noResults") || "No results"}
                        </CommandEmpty>
                      )}
                      {/* Show "Add custom" option when typed text doesn't match any item exactly */}
                      {searchValue.trim() && !hasExactMatch && (
                        <CommandItem
                          value={`custom:${searchValue}`}
                          onSelect={() => {
                            field.onChange(searchValue.trim())
                            setSearchValue("")
                            setPopoverOpen(false)
                          }}
                        >
                          <IconPlus className="mr-2 h-4 w-4" />
                          {t("allergyCategory.custom") || "Add custom"}:{" "}
                          &quot;{searchValue.trim()}&quot;
                        </CommandItem>
                      )}
                      {/* Group filtered catalog items by category */}
                      {Object.entries(
                        filtered.reduce<Record<string, typeof filtered>>(
                          (acc, item) => {
                            const cat = item.categoryLabel
                            ;(acc[cat] ??= []).push(item)
                            return acc
                          },
                          {},
                        ),
                      ).map(([categoryLabel, items]) => (
                        <CommandGroup key={categoryLabel} heading={categoryLabel}>
                          {items.map((item) => (
                            <CommandItem
                              key={item.value}
                              value={item.value}
                              onSelect={() => {
                                field.onChange(item.value)
                                setSearchValue("")
                                setPopoverOpen(false)
                              }}
                            >
                              <IconCheck
                                className={cn(
                                  "mr-2 h-4 w-4",
                                  nameValue === item.value
                                    ? "opacity-100"
                                    : "opacity-0",
                                )}
                              />
                              {item.label}
                            </CommandItem>
                          ))}
                        </CommandGroup>
                      ))}
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
