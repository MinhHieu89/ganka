import { useState } from "react"
import { useTranslation } from "react-i18next"
import { IconPlus, IconEdit, IconEye, IconEyeOff, IconPackage } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Skeleton } from "@/shared/components/Skeleton"
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/shared/components/Card"
import { useComboPackages } from "@/features/optical/api/optical-queries"
import type { ComboPackageDto } from "@/features/optical/api/optical-api"
import { ComboPackageForm } from "./ComboPackageForm"

function formatVnd(amount: number): string {
  return new Intl.NumberFormat("vi-VN").format(amount) + " ₫"
}

function SavingsBadge({ combo }: { combo: ComboPackageDto }) {
  const { t } = useTranslation("optical")
  if (!combo.originalTotalPrice || !combo.savings || combo.savings <= 0) return null
  const pct = Math.round((combo.savings / combo.originalTotalPrice) * 100)
  return (
    <div className="flex items-center gap-1.5">
      <span className="text-sm text-muted-foreground line-through">
        {formatVnd(combo.originalTotalPrice)}
      </span>
      <Badge className="bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400 border-0 text-xs">
        {t("combos.savings")} {pct}%
      </Badge>
    </div>
  )
}

interface ComboCardProps {
  combo: ComboPackageDto
  onEdit: (combo: ComboPackageDto) => void
}

function ComboCard({ combo, onEdit }: ComboCardProps) {
  const { t } = useTranslation("optical")
  return (
    <Card className={combo.isActive ? undefined : "opacity-60 border-dashed"}>
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between gap-2">
          <div className="min-w-0 flex-1">
            <CardTitle className="text-base leading-tight">{combo.name}</CardTitle>
            {combo.description && (
              <CardDescription className="mt-1 text-sm line-clamp-2">
                {combo.description}
              </CardDescription>
            )}
          </div>
          <div className="flex items-center gap-1 shrink-0">
            {combo.isActive ? (
              <Badge
                variant="outline"
                className="border-green-500 text-green-700 dark:text-green-400 text-xs"
              >
                {t("combos.active")}
              </Badge>
            ) : (
              <Badge
                variant="outline"
                className="border-muted-foreground text-muted-foreground text-xs"
              >
                {t("combos.inactive")}
              </Badge>
            )}
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {/* Frame and Lens info */}
        <div className="space-y-1 text-sm">
          <div className="flex items-center gap-2">
            <span className="text-muted-foreground w-12 shrink-0">{t("combos.frame")}:</span>
            <span className="font-medium truncate">
              {combo.frameName ?? <span className="text-muted-foreground italic">Any frame</span>}
            </span>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-muted-foreground w-12 shrink-0">{t("combos.lens")}:</span>
            <span className="font-medium truncate">
              {combo.lensName ?? <span className="text-muted-foreground italic">Any lens</span>}
            </span>
          </div>
        </div>

        {/* Pricing */}
        <div className="pt-2 border-t space-y-1">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">{t("combos.comboPrice")}</span>
            <span className="text-base font-bold text-primary">
              {formatVnd(combo.comboPrice)}
            </span>
          </div>
          {combo.originalTotalPrice && combo.savings && combo.savings > 0 && (
            <div className="flex items-center justify-between">
              <SavingsBadge combo={combo} />
              <span className="text-sm font-medium text-green-700 dark:text-green-400">
                {t("combos.savings")} {formatVnd(combo.savings)}
              </span>
            </div>
          )}
        </div>

        {/* Edit button */}
        <Button
          variant="outline"
          size="sm"
          className="w-full"
          onClick={() => onEdit(combo)}
        >
          <IconEdit className="h-3.5 w-3.5 mr-1.5" />
          {t("common.edit")}
        </Button>
      </CardContent>
    </Card>
  )
}

export function ComboPackagePage() {
  const { t } = useTranslation("optical")
  const [showInactive, setShowInactive] = useState(false)
  const [formOpen, setFormOpen] = useState(false)
  const [editingCombo, setEditingCombo] = useState<ComboPackageDto | undefined>(undefined)

  const { data: combos, isLoading } = useComboPackages(showInactive ? true : undefined)

  const visibleCombos = combos ?? []

  const openCreate = () => {
    setEditingCombo(undefined)
    setFormOpen(true)
  }

  const openEdit = (combo: ComboPackageDto) => {
    setEditingCombo(combo)
    setFormOpen(true)
  }

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t("combos.title")}</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {t("combos.subtitle")}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowInactive((v) => !v)}
          >
            {showInactive ? (
              <IconEyeOff className="h-4 w-4 mr-1.5" />
            ) : (
              <IconEye className="h-4 w-4 mr-1.5" />
            )}
            {t("combos.inactive")}
          </Button>
          <Button onClick={openCreate}>
            <IconPlus className="h-4 w-4 mr-2" />
            {t("combos.addCombo")}
          </Button>
        </div>
      </div>

      {/* Content */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-52 w-full rounded-lg" />
          ))}
        </div>
      ) : visibleCombos.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-16 text-center">
          <IconPackage className="h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-medium">{t("combos.empty")}</h3>
          <p className="text-sm text-muted-foreground mt-1 mb-4">
            {t("combos.subtitle")}
          </p>
          <Button onClick={openCreate}>
            <IconPlus className="h-4 w-4 mr-2" />
            {t("combos.addCombo")}
          </Button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {visibleCombos.map((combo) => (
            <ComboCard key={combo.id} combo={combo} onEdit={openEdit} />
          ))}
        </div>
      )}

      {/* Create/Edit dialog */}
      <ComboPackageForm
        combo={editingCombo}
        open={formOpen}
        onOpenChange={setFormOpen}
      />
    </div>
  )
}
