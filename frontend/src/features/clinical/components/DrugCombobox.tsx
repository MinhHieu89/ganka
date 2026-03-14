import { useState, useCallback, useRef, useEffect } from "react"
import { useTranslation } from "react-i18next"
import { IconLoader2, IconAlertTriangle } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
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
import { Input } from "@/shared/components/Input"
import { Badge } from "@/shared/components/Badge"
import type { AllergyDto } from "@/features/patient/api/patient-api"
import {
  useDrugCatalogSearch,
  type DrugCatalogItemDto,
} from "../api/prescription-api"

interface DrugComboboxProps {
  onSelect: (drug: DrugCatalogItemDto, hasAllergyWarning: boolean) => void
  patientAllergies?: AllergyDto[]
  disabled?: boolean
  onOffCatalog?: (drugName: string) => void
}

export function DrugCombobox({
  onSelect,
  patientAllergies,
  disabled,
  onOffCatalog,
}: DrugComboboxProps) {
  const { t, i18n } = useTranslation("clinical")
  const [open, setOpen] = useState(false)
  const [searchTerm, setSearchTerm] = useState("")
  const [debouncedTerm, setDebouncedTerm] = useState("")
  const [offCatalogMode, setOffCatalogMode] = useState(false)
  const [offCatalogName, setOffCatalogName] = useState("")
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const { data: searchResults, isLoading: isSearching } =
    useDrugCatalogSearch(debouncedTerm)

  // Debounce search input (300ms)
  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      setDebouncedTerm(searchTerm)
    }, 300)
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current)
    }
  }, [searchTerm])

  // Check if drug matches any patient allergy (case-insensitive, bidirectional)
  const checkAllergyMatch = useCallback(
    (drugName: string, genericName: string | null): boolean => {
      if (!patientAllergies || patientAllergies.length === 0) return false
      const dn = drugName.toLowerCase()
      const gn = genericName?.toLowerCase() ?? ""
      return patientAllergies.some((a) => {
        const allergyName = a.name.toLowerCase()
        return (
          dn.includes(allergyName) ||
          allergyName.includes(dn) ||
          (gn && (gn.includes(allergyName) || allergyName.includes(gn)))
        )
      })
    },
    [patientAllergies],
  )

  const handleSelect = useCallback(
    (drug: DrugCatalogItemDto) => {
      const hasWarning = checkAllergyMatch(drug.name, drug.genericName)
      onSelect(drug, hasWarning)
      setOpen(false)
      setSearchTerm("")
      setDebouncedTerm("")
    },
    [onSelect, checkAllergyMatch],
  )

  const handleOffCatalogConfirm = useCallback(() => {
    if (offCatalogName.trim()) {
      onOffCatalog?.(offCatalogName.trim())
      setOffCatalogMode(false)
      setOffCatalogName("")
      setOpen(false)
      setSearchTerm("")
      setDebouncedTerm("")
    }
  }, [offCatalogName, onOffCatalog])

  const getDrugDisplayName = (drug: DrugCatalogItemDto) =>
    i18n.language === "vi" ? drug.nameVi || drug.name : drug.name

  return (
    <Popover
      open={open}
      onOpenChange={(v) => {
        setOpen(v)
        if (!v) {
          setOffCatalogMode(false)
          setOffCatalogName("")
        }
      }}
    >
      <PopoverTrigger asChild>
        <div>
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={disabled}
            className="w-full justify-start text-muted-foreground"
          >
            {t("prescription.addDrug")}
          </Button>
        </div>
      </PopoverTrigger>
      <PopoverContent
        className="w-[420px] p-0"
        align="start"

      >
        {offCatalogMode ? (
          <div className="p-4 space-y-3">
            <div className="text-sm font-medium">
              {t("prescription.offCatalog")}
            </div>
            <Input
              value={offCatalogName}
              onChange={(e) => setOffCatalogName(e.target.value)}
              autoFocus
            />
            <div className="flex justify-end gap-2">
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => setOffCatalogMode(false)}
              >
                {t("images.cancel")}
              </Button>
              <Button
                type="button"
                size="sm"
                disabled={!offCatalogName.trim()}
                onClick={handleOffCatalogConfirm}
              >
                OK
              </Button>
            </div>
          </div>
        ) : (
          <Command shouldFilter={false}>
            <div className="p-2">
              <Input
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="h-8"
                autoFocus
              />
            </div>
            <CommandList>
              {isSearching && (
                <div className="flex items-center justify-center py-4">
                  <IconLoader2 className="h-4 w-4 animate-spin" />
                </div>
              )}

              {!isSearching &&
                debouncedTerm.length >= 2 &&
                (!searchResults || searchResults.length === 0) && (
                  <CommandEmpty>No results</CommandEmpty>
                )}

              {/* Search results */}
              {searchResults && searchResults.length > 0 && (
                <CommandGroup>
                  {searchResults.map((drug) => {
                    const hasWarning = checkAllergyMatch(
                      drug.name,
                      drug.genericName,
                    )
                    return (
                      <CommandItem
                        key={drug.id}
                        value={drug.id}
                        onSelect={() => handleSelect(drug)}
                        className="flex items-center gap-2"
                      >
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-1.5">
                            <span className="text-sm font-medium truncate">
                              {getDrugDisplayName(drug)}
                            </span>
                            {drug.strength && (
                              <span className="text-xs text-muted-foreground shrink-0">
                                {drug.strength}
                              </span>
                            )}
                            {hasWarning && (
                              <Badge
                                variant="destructive"
                                className="shrink-0 text-xs gap-0.5"
                              >
                                <IconAlertTriangle className="h-3 w-3" />
                                {t("prescription.allergyWarning")}
                              </Badge>
                            )}
                          </div>
                          {drug.genericName && (
                            <span className="text-xs text-muted-foreground">
                              {drug.genericName}
                            </span>
                          )}
                        </div>
                      </CommandItem>
                    )
                  })}
                </CommandGroup>
              )}

              {/* Off-catalog option at bottom */}
              <CommandGroup>
                <CommandItem
                  value="__off_catalog__"
                  onSelect={() => setOffCatalogMode(true)}
                  className="text-muted-foreground"
                >
                  <span className="text-sm">
                    + {t("prescription.offCatalog")}
                  </span>
                </CommandItem>
              </CommandGroup>
            </CommandList>
          </Command>
        )}
      </PopoverContent>
    </Popover>
  )
}
