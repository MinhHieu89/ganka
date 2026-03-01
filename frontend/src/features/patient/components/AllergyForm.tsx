import { useState } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconLoader2 } from "@tabler/icons-react"
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
  CommandInput,
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
  type AllergySeverity,
} from "@/features/patient/api/patient-api"

interface AllergyFormProps {
  open: boolean
  onClose: () => void
  patientId: string
}

// Common ophthalmology allergy catalog for autocomplete
const ALLERGY_CATALOG = [
  "Atropine",
  "Tropicamide",
  "Cyclopentolate",
  "Phenylephrine",
  "Timolol",
  "Latanoprost",
  "Brimonidine",
  "Dorzolamide",
  "Fluorescein",
  "Proparacaine",
  "Tetracaine",
  "Moxifloxacin",
  "Ofloxacin",
  "Tobramycin",
  "Dexamethasone",
  "Prednisolone",
  "Neomycin",
  "Polymyxin B",
  "Sulfacetamide",
  "Erythromycin",
  "Latex",
  "Benzalkonium chloride",
  "Thimerosal",
  "Chlorhexidine",
  "Iodine",
  "Adhesive tape",
]

export function AllergyForm({ open, onClose, patientId }: AllergyFormProps) {
  const { t } = useTranslation("patient")
  const { t: tCommon } = useTranslation("common")
  const addAllergyMutation = useAddAllergy()
  const [popoverOpen, setPopoverOpen] = useState(false)

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
    }
  }

  const nameValue = form.watch("name")
  const filteredCatalog = ALLERGY_CATALOG.filter((item) =>
    item.toLowerCase().includes((nameValue ?? "").toLowerCase()),
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
                    <Input
                      {...field}
                      placeholder={t("allergyName")}
                      aria-invalid={fieldState.invalid || undefined}
                      onClick={() => setPopoverOpen(true)}
                      onChange={(e) => {
                        field.onChange(e)
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
                        <CommandEmpty>
                          {tCommon("search.noResults")}
                        </CommandEmpty>
                        <CommandGroup>
                          {filteredCatalog.slice(0, 10).map((item) => (
                            <CommandItem
                              key={item}
                              value={item}
                              onSelect={(val) => {
                                form.setValue("name", val)
                                setPopoverOpen(false)
                              }}
                            >
                              {item}
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
