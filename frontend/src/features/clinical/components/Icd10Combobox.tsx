import { useState, useCallback, useRef, useEffect } from "react"
import { useTranslation } from "react-i18next"
import {
  IconCheck,
  IconStar,
  IconStarFilled,
  IconLoader2,
} from "@tabler/icons-react"
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
import { cn } from "@/shared/lib/utils"
import {
  useIcd10Search,
  useDoctorFavorites,
  useToggleIcd10Favorite,
  type Icd10SearchResultDto,
} from "../api/clinical-api"

// Laterality enum: 0=None, 1=OD, 2=OS, 3=OU
const LATERALITY_OPTIONS = [
  { value: 1, label: "od" },
  { value: 2, label: "os" },
  { value: 3, label: "ou" },
] as const

interface Icd10ComboboxProps {
  doctorId: string
  onSelect: (
    code: Icd10SearchResultDto,
    laterality: number,
  ) => void
  disabled?: boolean
}

export function Icd10Combobox({
  doctorId,
  onSelect,
  disabled,
}: Icd10ComboboxProps) {
  const { t, i18n } = useTranslation("clinical")
  const [open, setOpen] = useState(false)
  const [searchTerm, setSearchTerm] = useState("")
  const [debouncedTerm, setDebouncedTerm] = useState("")
  const [pendingCode, setPendingCode] = useState<Icd10SearchResultDto | null>(null)
  const [selectedLaterality, setSelectedLaterality] = useState<number | null>(null)
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const { data: searchResults, isLoading: isSearching } = useIcd10Search(debouncedTerm)
  const { data: favorites } = useDoctorFavorites(doctorId)
  const toggleFavoriteMutation = useToggleIcd10Favorite()

  // Debounce search input
  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      setDebouncedTerm(searchTerm)
    }, 300)
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current)
    }
  }, [searchTerm])

  const handleSelect = useCallback(
    (code: Icd10SearchResultDto) => {
      if (code.requiresLaterality) {
        setPendingCode(code)
        setSelectedLaterality(null)
      } else {
        onSelect(code, 0) // 0 = None/not applicable
        setOpen(false)
        setSearchTerm("")
        setDebouncedTerm("")
      }
    },
    [onSelect],
  )

  const handleLateralityConfirm = useCallback(() => {
    if (pendingCode && selectedLaterality !== null) {
      onSelect(pendingCode, selectedLaterality)
      setPendingCode(null)
      setSelectedLaterality(null)
      setOpen(false)
      setSearchTerm("")
      setDebouncedTerm("")
    }
  }, [pendingCode, selectedLaterality, onSelect])

  const handleToggleFavorite = useCallback(
    (e: React.MouseEvent, icd10Code: string) => {
      e.stopPropagation()
      e.preventDefault()
      toggleFavoriteMutation.mutate({ doctorId, icd10Code })
    },
    [doctorId, toggleFavoriteMutation],
  )

  const getDescription = (code: Icd10SearchResultDto) =>
    i18n.language === "vi" ? code.descriptionVi : code.descriptionEn

  const getSecondaryDescription = (code: Icd10SearchResultDto) =>
    i18n.language === "vi" ? code.descriptionEn : code.descriptionVi

  // Group results by category
  const groupedResults = (searchResults ?? []).reduce<
    Record<string, Icd10SearchResultDto[]>
  >((acc, item) => {
    const cat = item.category || "Other"
    ;(acc[cat] ??= []).push(item)
    return acc
  }, {})

  // Filter favorites that match search if actively searching, else show all
  const filteredFavorites = favorites?.filter((fav) => {
    if (!debouncedTerm || debouncedTerm.length < 2) return true
    const term = debouncedTerm.toLowerCase()
    return (
      fav.code.toLowerCase().includes(term) ||
      fav.descriptionEn.toLowerCase().includes(term) ||
      fav.descriptionVi.toLowerCase().includes(term)
    )
  })

  return (
    <Popover
      open={open}
      onOpenChange={(v) => {
        setOpen(v)
        if (!v) {
          setPendingCode(null)
          setSelectedLaterality(null)
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
            {t("diagnosis.search")}
          </Button>
        </div>
      </PopoverTrigger>
      <PopoverContent
        className="w-[400px] p-0"
        align="start"
        onOpenAutoFocus={(e) => e.preventDefault()}
      >
        {pendingCode ? (
          // Laterality selector
          <div className="p-4 space-y-3">
            <div className="text-sm font-medium">
              {pendingCode.code} - {getDescription(pendingCode)}
            </div>
            <div className="text-xs text-muted-foreground">
              {t("diagnosis.selectLaterality")}
            </div>
            <div className="flex gap-2">
              {LATERALITY_OPTIONS.map((opt) => (
                <Button
                  key={opt.value}
                  type="button"
                  variant={selectedLaterality === opt.value ? "default" : "outline"}
                  size="sm"
                  onClick={() => setSelectedLaterality(opt.value)}
                >
                  {t(`diagnosis.${opt.label}`)}
                </Button>
              ))}
            </div>
            <div className="flex justify-end gap-2">
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => {
                  setPendingCode(null)
                  setSelectedLaterality(null)
                }}
              >
                {t("diagnosis.search")}
              </Button>
              <Button
                type="button"
                size="sm"
                disabled={selectedLaterality === null}
                onClick={handleLateralityConfirm}
              >
                <IconCheck className="h-4 w-4 mr-1" />
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
                (!searchResults || searchResults.length === 0) &&
                (!filteredFavorites || filteredFavorites.length === 0) && (
                  <CommandEmpty>No results</CommandEmpty>
                )}

              {/* Favorites group */}
              {filteredFavorites && filteredFavorites.length > 0 && (
                <CommandGroup heading={t("diagnosis.favorites")}>
                  {filteredFavorites.map((fav) => (
                    <CommandItem
                      key={`fav-${fav.code}`}
                      value={`fav-${fav.code}`}
                      onSelect={() => handleSelect(fav)}
                      className="flex items-center gap-2"
                    >
                      <button
                        type="button"
                        className="shrink-0"
                        onClick={(e) => handleToggleFavorite(e, fav.code)}
                      >
                        <IconStarFilled className="h-4 w-4 text-yellow-500" />
                      </button>
                      <span className="font-mono text-xs shrink-0">
                        {fav.code}
                      </span>
                      <span className="text-sm truncate">
                        {getDescription(fav)}
                      </span>
                      <span className="text-xs text-muted-foreground truncate ml-auto">
                        {getSecondaryDescription(fav)}
                      </span>
                    </CommandItem>
                  ))}
                </CommandGroup>
              )}

              {/* Search results grouped by category */}
              {Object.entries(groupedResults).map(([category, items]) => (
                <CommandGroup key={category} heading={category}>
                  {items.map((item) => {
                    const isFav = favorites?.some(
                      (f) => f.code === item.code,
                    )
                    return (
                      <CommandItem
                        key={item.code}
                        value={item.code}
                        onSelect={() => handleSelect(item)}
                        className="flex items-center gap-2"
                      >
                        <button
                          type="button"
                          className="shrink-0"
                          onClick={(e) =>
                            handleToggleFavorite(e, item.code)
                          }
                        >
                          {isFav ? (
                            <IconStarFilled className="h-4 w-4 text-yellow-500" />
                          ) : (
                            <IconStar className="h-4 w-4 text-muted-foreground" />
                          )}
                        </button>
                        <span className="font-mono text-xs shrink-0">
                          {item.code}
                        </span>
                        <span className="text-sm truncate">
                          {getDescription(item)}
                        </span>
                        <span className="text-xs text-muted-foreground truncate ml-auto">
                          {getSecondaryDescription(item)}
                        </span>
                      </CommandItem>
                    )
                  })}
                </CommandGroup>
              ))}
            </CommandList>
          </Command>
        )}
      </PopoverContent>
    </Popover>
  )
}
