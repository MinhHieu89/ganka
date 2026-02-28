import { useState, useMemo } from "react"
import {
  useRolesQuery,
  useCreateRoleMutation,
  useUpdateRolePermissionsMutation,
  usePermissionsQuery,
  type RoleDto,
  type CreateRoleCommand,
  type UpdateRolePermissionsCommand,
} from "@/features/admin/api/admin-api"

export function useRoles() {
  const rolesQuery = useRolesQuery()
  const permissionsQuery = usePermissionsQuery()
  const createMutation = useCreateRoleMutation()
  const updatePermissionsMutation = useUpdateRolePermissionsMutation()
  const [selectedRole, setSelectedRole] = useState<RoleDto | null>(null)
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)

  const roles = useMemo(() => rolesQuery.data ?? [], [rolesQuery.data])
  const permissions = useMemo(
    () => permissionsQuery.data ?? [],
    [permissionsQuery.data],
  )

  // Group permissions by module
  const permissionsByModule = useMemo(() => {
    const grouped: Record<
      string,
      { id: string; module: string; action: string; description: string }[]
    > = {}
    for (const perm of permissions) {
      if (!grouped[perm.module]) {
        grouped[perm.module] = []
      }
      grouped[perm.module].push(perm)
    }
    return grouped
  }, [permissions])

  // All unique actions across all permissions
  const allActions = useMemo(() => {
    const actions = new Set<string>()
    for (const perm of permissions) {
      actions.add(perm.action)
    }
    // Sort in a logical order
    const order = ["View", "Create", "Update", "Delete", "Export", "Manage"]
    return [...actions].sort(
      (a, b) => order.indexOf(a) - order.indexOf(b),
    )
  }, [permissions])

  const selectRole = (role: RoleDto) => {
    setSelectedRole(role)
  }

  const createRole = async (data: CreateRoleCommand) => {
    await createMutation.mutateAsync(data)
    setIsCreateDialogOpen(false)
  }

  const updatePermissions = async (data: UpdateRolePermissionsCommand) => {
    await updatePermissionsMutation.mutateAsync(data)
    // Refresh the selected role data
    const updatedRoles = await rolesQuery.refetch()
    const updated = updatedRoles.data?.find((r) => r.id === data.roleId)
    if (updated) {
      setSelectedRole(updated)
    }
  }

  return {
    roles,
    permissions,
    permissionsByModule,
    allActions,
    selectedRole,
    isCreateDialogOpen,
    isLoading: rolesQuery.isLoading || permissionsQuery.isLoading,
    error: rolesQuery.error || permissionsQuery.error,
    selectRole,
    createRole,
    updatePermissions,
    setIsCreateDialogOpen,
    isCreating: createMutation.isPending,
    isUpdatingPermissions: updatePermissionsMutation.isPending,
  }
}
