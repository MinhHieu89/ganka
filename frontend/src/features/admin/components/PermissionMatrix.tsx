import { useState, useMemo, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { IconLoader2 } from "@tabler/icons-react"
import { toast } from "sonner"
import { Checkbox } from "@/shared/components/ui/checkbox"
import { Button } from "@/shared/components/ui/button"
import { Separator } from "@/shared/components/ui/separator"
import type {
  RoleDto,
  PermissionDto,
  UpdateRolePermissionsCommand,
} from "@/features/admin/api/admin-api"

interface PermissionMatrixProps {
  role: RoleDto
  permissionsByModule: Record<string, PermissionDto[]>
  allActions: string[]
  onSave: (data: UpdateRolePermissionsCommand) => Promise<void>
  isSaving: boolean
}

export function PermissionMatrix({
  role,
  permissionsByModule,
  allActions,
  onSave,
  isSaving,
}: PermissionMatrixProps) {
  const { t } = useTranslation("auth")
  const { t: tCommon } = useTranslation("common")

  // Track selected permission IDs locally for editing
  const [selectedIds, setSelectedIds] = useState<Set<string>>(() => {
    return new Set(role.permissions.map((p) => p.id))
  })

  // Reset when role changes
  const roleId = role.id
  const [prevRoleId, setPrevRoleId] = useState(roleId)
  if (roleId !== prevRoleId) {
    setPrevRoleId(roleId)
    setSelectedIds(new Set(role.permissions.map((p) => p.id)))
  }

  // Build a lookup: module+action -> permissionId
  const permissionLookup = useMemo(() => {
    const lookup: Record<string, string> = {}
    for (const [module, perms] of Object.entries(permissionsByModule)) {
      for (const perm of perms) {
        lookup[`${module}.${perm.action}`] = perm.id
      }
    }
    return lookup
  }, [permissionsByModule])

  const modules = useMemo(
    () => Object.keys(permissionsByModule).sort(),
    [permissionsByModule],
  )

  const isChecked = useCallback(
    (module: string, action: string) => {
      const id = permissionLookup[`${module}.${action}`]
      return id ? selectedIds.has(id) : false
    },
    [permissionLookup, selectedIds],
  )

  const hasPermission = useCallback(
    (module: string, action: string) => {
      return `${module}.${action}` in permissionLookup
    },
    [permissionLookup],
  )

  const togglePermission = useCallback(
    (module: string, action: string) => {
      const id = permissionLookup[`${module}.${action}`]
      if (!id) return
      setSelectedIds((prev) => {
        const next = new Set(prev)
        if (next.has(id)) {
          next.delete(id)
        } else {
          next.add(id)
        }
        return next
      })
    },
    [permissionLookup],
  )

  // Select All per row (module)
  const isModuleAllSelected = useCallback(
    (module: string) => {
      const perms = permissionsByModule[module] ?? []
      return perms.length > 0 && perms.every((p) => selectedIds.has(p.id))
    },
    [permissionsByModule, selectedIds],
  )

  const toggleModuleAll = useCallback(
    (module: string) => {
      const perms = permissionsByModule[module] ?? []
      const allSelected = isModuleAllSelected(module)
      setSelectedIds((prev) => {
        const next = new Set(prev)
        for (const perm of perms) {
          if (allSelected) {
            next.delete(perm.id)
          } else {
            next.add(perm.id)
          }
        }
        return next
      })
    },
    [permissionsByModule, isModuleAllSelected],
  )

  // Select All per column (action)
  const isActionAllSelected = useCallback(
    (action: string) => {
      let count = 0
      let selected = 0
      for (const [module, perms] of Object.entries(permissionsByModule)) {
        const perm = perms.find((p) => p.action === action)
        if (perm) {
          count++
          if (selectedIds.has(perm.id)) selected++
        }
      }
      return count > 0 && count === selected
    },
    [permissionsByModule, selectedIds],
  )

  const toggleActionAll = useCallback(
    (action: string) => {
      const allSelected = isActionAllSelected(action)
      setSelectedIds((prev) => {
        const next = new Set(prev)
        for (const perms of Object.values(permissionsByModule)) {
          const perm = perms.find((p) => p.action === action)
          if (perm) {
            if (allSelected) {
              next.delete(perm.id)
            } else {
              next.add(perm.id)
            }
          }
        }
        return next
      })
    },
    [permissionsByModule, isActionAllSelected],
  )

  const handleSave = async () => {
    try {
      await onSave({
        roleId: role.id,
        permissionIds: [...selectedIds],
      })
      toast.success(t("admin.permissionsUpdated"))
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : tCommon("status.error"),
      )
    }
  }

  // Check if there are unsaved changes
  const hasChanges = useMemo(() => {
    const currentIds = new Set(role.permissions.map((p) => p.id))
    if (currentIds.size !== selectedIds.size) return true
    for (const id of selectedIds) {
      if (!currentIds.has(id)) return true
    }
    return false
  }, [role.permissions, selectedIds])

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold">
          {t("admin.permissions")}: {role.name}
        </h3>
        <Button onClick={handleSave} disabled={isSaving || !hasChanges}>
          {isSaving && <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />}
          {t("admin.saveChanges")}
        </Button>
      </div>

      <Separator />

      <div className="overflow-x-auto">
        <table className="w-full border-collapse">
          <thead>
            <tr className="border-b">
              <th className="text-left p-3 font-medium text-sm min-w-[160px]">
                Module
              </th>
              {allActions.map((action) => (
                <th key={action} className="p-3 text-center min-w-[80px]">
                  <div className="flex flex-col items-center gap-1">
                    <span className="text-xs font-medium">
                      {t(`permissionActions.${action}`, action)}
                    </span>
                    <Checkbox
                      checked={isActionAllSelected(action)}
                      onCheckedChange={() => toggleActionAll(action)}
                      className="h-3.5 w-3.5"
                    />
                  </div>
                </th>
              ))}
              <th className="p-3 text-center min-w-[80px]">
                <span className="text-xs font-medium">
                  {t("admin.selectAll")}
                </span>
              </th>
            </tr>
          </thead>
          <tbody>
            {modules.map((module, index) => (
              <tr
                key={module}
                className={index % 2 === 0 ? "bg-muted/30" : ""}
              >
                <td className="p-3 font-medium text-sm">
                  {t(`permissionModules.${module}`, module)}
                </td>
                {allActions.map((action) => (
                  <td key={action} className="p-3 text-center">
                    {hasPermission(module, action) ? (
                      <div className="flex justify-center">
                        <Checkbox
                          checked={isChecked(module, action)}
                          onCheckedChange={() =>
                            togglePermission(module, action)
                          }
                        />
                      </div>
                    ) : (
                      <span className="text-muted-foreground/30">&mdash;</span>
                    )}
                  </td>
                ))}
                <td className="p-3 text-center">
                  <div className="flex justify-center">
                    <Checkbox
                      checked={isModuleAllSelected(module)}
                      onCheckedChange={() => toggleModuleAll(module)}
                      className="h-3.5 w-3.5"
                    />
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
