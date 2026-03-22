import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconSearch } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
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
import { cn } from "@/shared/lib/utils"
import { useDrugCatalogList, useDrugCatalogSearch } from "@/features/pharmacy/api/pharmacy-queries"
import type { DrugCatalogItemDto } from "@/features/pharmacy/api/pharmacy-api"

interface DrugComboboxProps {
  value: string
  onSelect: (drug: DrugCatalogItemDto) => void
  disabled?: boolean
  /** Extra info shown below the drug name (e.g. price). */
  renderExtra?: (drug: DrugCatalogItemDto) => React.ReactNode
}

export function DrugCombobox({
  value,
  onSelect,
  disabled,
  renderExtra,
}: DrugComboboxProps) {
  const { t } = useTranslation("pharmacy")
  const [open, setOpen] = useState(false)
  const [search, setSearch] = useState("")
  const [pickedDrug, setPickedDrug] = useState<DrugCatalogItemDto | null>(null)
  const { data: initialDrugs } = useDrugCatalogList()
  const { data: searchResults } = useDrugCatalogSearch(search)
  const drugs = search.length >= 2 ? searchResults : initialDrugs

  const selectedDrug = pickedDrug?.id === value
    ? pickedDrug
    : (initialDrugs ?? []).find((d) => d.id === value)

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          type="button"
          variant="outline"
          role="combobox"
          aria-expanded={open}
          disabled={disabled}
          className={cn(
            "w-full justify-start text-left font-normal",
            !selectedDrug && "text-muted-foreground",
          )}
        >
          <IconSearch className="h-4 w-4 mr-2 shrink-0" />
          {selectedDrug ? (
            <span className="truncate">
              {selectedDrug.nameVi || selectedDrug.name}
            </span>
          ) : (
            t("stockImport.selectDrug")
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-80 p-0" align="start">
        <Command shouldFilter={false}>
          <CommandInput
            placeholder={t("catalog.search")}
            value={search}
            onValueChange={setSearch}
          />
          <CommandList>
            <CommandEmpty>{t("catalog.empty")}</CommandEmpty>
            <CommandGroup className="max-h-52 overflow-y-auto">
              {(drugs ?? []).map((drug) => (
                <CommandItem
                  key={drug.id}
                  value={drug.id}
                  onSelect={() => {
                    setPickedDrug(drug)
                    onSelect(drug)
                    setOpen(false)
                    setSearch("")
                  }}
                >
                  <div>
                    <div className="text-sm font-medium">
                      {drug.nameVi || drug.name}
                    </div>
                    <div className="text-xs text-muted-foreground">
                      {drug.genericName}
                      {renderExtra?.(drug)}
                    </div>
                  </div>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
