import { useState, useDeferredValue, useEffect } from "react"
import { useNavigate } from "@tanstack/react-router"
import { useTranslation } from "react-i18next"
import { IconSearch, IconUser } from "@tabler/icons-react"
import {
  Command,
  CommandInput,
  CommandList,
  CommandEmpty,
  CommandGroup,
  CommandItem,
} from "@/shared/components/Command"
import { Popover, PopoverContent, PopoverTrigger } from "@/shared/components/Popover"
import { Button } from "@/shared/components/Button"
import { usePatientSearch } from "@/features/patient/hooks/usePatientSearch"
import { useRecentPatientsStore } from "@/shared/stores/recentPatientsStore"

export function GlobalSearch() {
  const { t } = useTranslation("common")
  const { t: tPatient } = useTranslation("patient")
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState("")
  const deferredQuery = useDeferredValue(query)
  const { data: results, isLoading } = usePatientSearch(deferredQuery, {
    enabled: deferredQuery.length >= 2,
  })
  const recentPatients = useRecentPatientsStore((s) => s.recent)
  const addRecent = useRecentPatientsStore((s) => s.addRecent)
  const navigate = useNavigate()

  const showRecent = open && query.length < 2

  // Keyboard shortcut: Ctrl+K to open
  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      if ((e.ctrlKey || e.metaKey) && e.key === "k") {
        e.preventDefault()
        setOpen((prev) => !prev)
      }
    }
    document.addEventListener("keydown", handleKeyDown)
    return () => document.removeEventListener("keydown", handleKeyDown)
  }, [])

  function handleSelect(patient: {
    id: string
    fullName: string
    patientCode: string
    phone: string
  }) {
    addRecent(patient)
    setOpen(false)
    setQuery("")
    // Route will be created in Plan 04 (Patient Frontend)
    navigate({ to: "/patients/$patientId" as string, params: { patientId: patient.id } as never })
  }

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          className="relative h-8 w-full justify-start gap-2 text-sm text-muted-foreground sm:w-64 lg:w-80"
        >
          <IconSearch className="h-4 w-4" />
          <span className="hidden sm:inline-flex">{tPatient("search")}</span>
          <span className="inline-flex sm:hidden">{t("buttons.search")}</span>
          <kbd className="pointer-events-none absolute right-1.5 hidden h-5 select-none items-center gap-1 border bg-muted px-1.5 font-mono text-[10px] font-medium opacity-100 sm:flex">
            <span className="text-xs">Ctrl</span>K
          </kbd>
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-80 p-0" align="start" sideOffset={8}>
        <Command shouldFilter={false}>
          <CommandInput
            value={query}
            onValueChange={setQuery}
            placeholder={tPatient("search")}
          />
          <CommandList>
            {showRecent && recentPatients.length > 0 && (
              <CommandGroup heading={tPatient("recent")}>
                {recentPatients.slice(0, 5).map((p) => (
                  <CommandItem
                    key={p.id}
                    onSelect={() => handleSelect(p)}
                    className="cursor-pointer"
                  >
                    <IconUser className="h-4 w-4 shrink-0 text-muted-foreground" />
                    <span className="truncate">{p.fullName}</span>
                    <span className="ml-auto text-xs text-muted-foreground">
                      {p.patientCode}
                    </span>
                  </CommandItem>
                ))}
              </CommandGroup>
            )}
            {showRecent && recentPatients.length === 0 && (
              <CommandEmpty>{tPatient("noRecent")}</CommandEmpty>
            )}
            {!showRecent && (
              <>
                <CommandEmpty>
                  {isLoading ? t("status.loading") : t("search.noResults")}
                </CommandEmpty>
                {results && results.length > 0 && (
                  <CommandGroup heading={tPatient("title")}>
                    {results.map((p) => (
                      <CommandItem
                        key={p.id}
                        onSelect={() => handleSelect(p)}
                        className="cursor-pointer"
                      >
                        <IconUser className="h-4 w-4 shrink-0 text-muted-foreground" />
                        <div className="flex flex-col min-w-0">
                          <span className="truncate">{p.fullName}</span>
                          <span className="text-xs text-muted-foreground">
                            {p.patientCode} &middot; {p.phone}
                          </span>
                        </div>
                      </CommandItem>
                    ))}
                  </CommandGroup>
                )}
              </>
            )}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
