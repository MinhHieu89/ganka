import { useState, useDeferredValue } from "react"
import { useTranslation } from "react-i18next"
import { IconPlus, IconTrash, IconSearch } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
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
import { useConsumableItems } from "@/features/consumables/api/consumables-queries"
import type { ConsumableInput } from "@/features/treatment/api/treatment-types"

// -- Props --

interface ConsumableSelectorProps {
  value: ConsumableInput[]
  onChange: (items: ConsumableInput[]) => void
}

// -- Component --

export function ConsumableSelector({
  value,
  onChange,
}: ConsumableSelectorProps) {
  const { t, i18n } = useTranslation("treatment")

  const getDisplayName = (item: { name: string; nameVi: string }) =>
    i18n.language === "vi" && item.nameVi ? item.nameVi : item.name
  const { data: consumableItems = [] } = useConsumableItems()
  const [popoverOpen, setPopoverOpen] = useState(false)
  const [searchTerm, setSearchTerm] = useState("")
  const deferredSearch = useDeferredValue(searchTerm)

  // Filter available items (exclude already selected, match search, only active)
  const selectedIds = new Set(value.map((v) => v.consumableItemId))
  const filteredItems = consumableItems
    .filter((item) => item.isActive)
    .filter((item) => !selectedIds.has(item.id))
    .filter((item) => {
      if (!deferredSearch) return true
      const term = deferredSearch.toLowerCase()
      return (
        item.name.toLowerCase().includes(term) ||
        item.nameVi.toLowerCase().includes(term)
      )
    })

  const handleSelect = (itemId: string) => {
    const item = consumableItems.find((c) => c.id === itemId)
    if (!item) return

    const newEntry: ConsumableInput = {
      consumableItemId: item.id,
      consumableName: getDisplayName(item),
      quantity: 1,
    }
    onChange([...value, newEntry])
    setPopoverOpen(false)
    setSearchTerm("")
  }

  const handleQuantityChange = (itemId: string, quantity: number) => {
    if (quantity < 1) return
    onChange(
      value.map((v) =>
        v.consumableItemId === itemId ? { ...v, quantity } : v,
      ),
    )
  }

  const handleRemove = (itemId: string) => {
    onChange(value.filter((v) => v.consumableItemId !== itemId))
  }

  return (
    <div className="space-y-3">
      {/* Selected consumables list */}
      {value.length > 0 && (
        <div className="space-y-2">
          {value.map((item) => (
            <div
              key={item.consumableItemId}
              className="flex items-center gap-3 p-2 rounded-md border bg-muted/30"
            >
              <span className="flex-1 text-sm font-medium">
                {item.consumableName}
              </span>
              <div className="flex items-center gap-2">
                <Label className="text-xs text-muted-foreground sr-only">
                  {t("consumable.qty")}
                </Label>
                <Input
                  type="number"
                  min={1}
                  value={item.quantity}
                  onChange={(e) =>
                    handleQuantityChange(
                      item.consumableItemId,
                      Number(e.target.value) || 1,
                    )
                  }
                  className="w-20 h-8 text-sm"
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => handleRemove(item.consumableItemId)}
                  className="h-8 w-8 p-0 text-muted-foreground hover:text-red-600"
                >
                  <IconTrash className="h-4 w-4" />
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add consumable button with combobox popover */}
      <Popover open={popoverOpen} onOpenChange={setPopoverOpen}>
        <PopoverTrigger asChild>
          <Button type="button" variant="outline" size="sm">
            <IconPlus className="h-4 w-4 mr-1" />
            {t("consumable.addItem")}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-80 p-0" align="start">
          <Command shouldFilter={false}>
            <CommandInput
              value={searchTerm}
              onValueChange={setSearchTerm}
            />
            <CommandList>
              <CommandEmpty>{t("consumable.noResults")}</CommandEmpty>
              <CommandGroup>
                {filteredItems.map((item) => (
                  <CommandItem
                    key={item.id}
                    value={item.id}
                    onSelect={handleSelect}
                    className="flex items-center justify-between"
                  >
                    <div>
                      <span className="text-sm">{getDisplayName(item)}</span>
                    </div>
                    <span className="text-xs text-muted-foreground">
                      {item.unit} - {t("consumable.stock")}: {item.currentStock}
                    </span>
                  </CommandItem>
                ))}
              </CommandGroup>
            </CommandList>
          </Command>
        </PopoverContent>
      </Popover>
    </div>
  )
}
