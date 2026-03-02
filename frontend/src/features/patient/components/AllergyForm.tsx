import { useState } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconCheck, IconLoader2, IconPlus } from "@tabler/icons-react"
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
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandItem,
  CommandList,
} from "@/shared/components/Command"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/shared/components/Popover"
import {
  useAddAllergy,
  ALLERGY_CATALOG_BILINGUAL,
  type AllergySeverity,
} from "@/features/patient/api/patient-api"
import { cn } from "@/shared/lib/utils"

const categoryKeyMap: Record<string, string> = {
  "Ophthalmic Drug": "allergyCategory.ophthalmicDrug",
  "General Drug": "allergyCategory.generalDrug",
  Material: "allergyCategory.material",
  Environmental: "allergyCategory.environmental",
}

interface AllergyFormProps {
  open: boolean
  onClose: () => void
  patientId: string
}

export function AllergyForm({ open, onClose, patientId }: AllergyFormProps) {
  const { t, i18n } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const addAllergyMutation = useAddAllergy()
  const [popoverOpen, setPopoverOpen] = useState(false)
  const [inputValue, setInputValue] = useState("")

  const schema = z.object({
    name: z.string().min(1, tCommon("validation.required")),
    severity: z.enum(["Mild", "Moderate", "Severe"]),
  })

  type FormValues = z.infer<typeof schema>

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: "",
      severity: "Mild",
    },
  })

  const handleSubmit = async (data: FormValues) => {
    try {
      await addAllergyMutation.mutateAsync({
        patientId,
        name: data.name,
        severity: data.severity as AllergySeverity,
      })
      toast.success(t("addAllergy"))
      form.reset()
      setInputValue("")
      onClose()
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
      setInputValue("")
    }
  }

  const catalogItems = ALLERGY_CATALOG_BILINGUAL.map((item) => ({
    label: i18n.language === "vi" ? item.vi : item.en,
    value: item.en,
    category: item.category,
    categoryLabel: t(categoryKeyMap[item.category] ?? item.category),
  }))

  const nameValue = form.watch("name")
  const filtered = inputValue.trim()
    ? catalogItems.filter((item) =>
        item.label.toLowerCase().includes(inputValue.toLowerCase()),
      )
    : catalogItems

  const hasExactMatch = catalogItems.some(
    (item) =>
      item.label.toLowerCase() === inputValue.toLowerCase() ||
      item.value.toLowerCase() === inputValue.toLowerCase(),
  )

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("addAllergy")}</DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Allergy Name with autocomplete */}
          <Controller
            name="name"
            control={form.control}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid || undefined}>
                <FieldLabel>{t("allergyName")}</FieldLabel>
                <Popover open={popoverOpen} onOpenChange={setPopoverOpen}>
                  <PopoverTrigger asChild>
                    <div>
                      <Input
                        value={inputValue}
                        placeholder={t("allergyName")}
                        aria-invalid={fieldState.invalid || undefined}
                        onFocus={() => setPopoverOpen(true)}
                        onChange={(e) => {
                          const val = e.target.value
                          setInputValue(val)
                          field.onChange(val)
                          if (!popoverOpen) setPopoverOpen(true)
                        }}
                        autoComplete="off"
                      />
                    </div>
                  </PopoverTrigger>
                  <PopoverContent
                    className="w-[--radix-popover-trigger-width] p-0"
                    align="start"
                    onOpenAutoFocus={(e) => e.preventDefault()}
                  >
                    <Command shouldFilter={false}>
                      <CommandList>
                        {filtered.length === 0 && !inputValue.trim() && (
                          <CommandEmpty>
                            {tCommon("search.noResults")}
                          </CommandEmpty>
                        )}
                        {/* Show "Add custom" option when typed text doesn't match any item exactly */}
                        {inputValue.trim() && !hasExactMatch && (
                          <CommandItem
                            value={`custom:${inputValue}`}
                            onSelect={() => {
                              field.onChange(inputValue.trim())
                              setPopoverOpen(false)
                            }}
                          >
                            <IconPlus className="mr-2 h-4 w-4" />
                            {t("allergyCategory.custom")}:{" "}
                            &quot;{inputValue.trim()}&quot;
                          </CommandItem>
                        )}
                        {/* Group filtered catalog items by category */}
                        {Object.entries(
                          filtered.reduce<
                            Record<string, typeof filtered>
                          >((acc, item) => {
                            const cat = item.categoryLabel
                            ;(acc[cat] ??= []).push(item)
                            return acc
                          }, {}),
                        ).map(([categoryLabel, items]) => (
                          <CommandGroup
                            key={categoryLabel}
                            heading={categoryLabel}
                          >
                            {items.map((item) => (
                              <CommandItem
                                key={item.value}
                                value={item.value}
                                onSelect={() => {
                                  field.onChange(item.value)
                                  setInputValue(item.label)
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

          {/* Severity */}
          <Controller
            name="severity"
            control={form.control}
            render={({ field }) => (
              <Field>
                <FieldLabel>{t("severity")}</FieldLabel>
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger>
                    <SelectValue placeholder={t("severity")} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Mild">{t("mild")}</SelectItem>
                    <SelectItem value="Moderate">{t("moderate")}</SelectItem>
                    <SelectItem value="Severe">{t("severe")}</SelectItem>
                  </SelectContent>
                </Select>
              </Field>
            )}
          />

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={addAllergyMutation.isPending}>
              {addAllergyMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {tCommon("buttons.save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
