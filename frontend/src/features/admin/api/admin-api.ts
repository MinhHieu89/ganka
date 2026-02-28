import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { api } from "@/shared/lib/api-client"

// ---- Types matching backend Auth.Contracts.Dtos ----

export interface UserDto {
  id: string
  email: string
  fullName: string
  preferredLanguage: string
  isActive: boolean
  roles: string[]
  permissions: string[]
}

export interface RoleDto {
  id: string
  name: string
  description: string
  isSystem: boolean
  permissions: PermissionDto[]
}

export interface PermissionDto {
  id: string
  module: string
  action: string
  description: string
}

export interface CreateUserCommand {
  email: string
  fullName: string
  password: string
  roleIds: string[]
}

export interface UpdateUserCommand {
  userId: string
  fullName: string
  isActive: boolean
  roleIds: string[]
}

export interface AssignRolesCommand {
  userId: string
  roleIds: string[]
}

export interface CreateRoleCommand {
  name: string
  description: string
  permissionIds: string[]
}

export interface UpdateRolePermissionsCommand {
  roleId: string
  permissionIds: string[]
}

// ---- API functions ----

async function getUsers(): Promise<UserDto[]> {
  const res = await api.GET("/api/admin/users" as never, {})
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to fetch users")
  }
  return res.data as UserDto[]
}

async function createUser(data: CreateUserCommand): Promise<UserDto> {
  const res = await api.POST("/api/admin/users" as never, {
    body: data as never,
  })
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to create user")
  }
  return res.data as UserDto
}

async function updateUser(data: UpdateUserCommand): Promise<UserDto> {
  const res = await api.PUT(`/api/admin/users/${data.userId}` as never, {
    body: data as never,
  })
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to update user")
  }
  return res.data as UserDto
}

async function assignRoles(data: AssignRolesCommand): Promise<void> {
  const res = await api.PUT(
    `/api/admin/users/${data.userId}/roles` as never,
    {
      body: data as never,
    },
  )
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to assign roles")
  }
}

async function getRoles(): Promise<RoleDto[]> {
  const res = await api.GET("/api/admin/roles" as never, {})
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to fetch roles")
  }
  return res.data as RoleDto[]
}

async function createRole(data: CreateRoleCommand): Promise<RoleDto> {
  const res = await api.POST("/api/admin/roles" as never, {
    body: data as never,
  })
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to create role")
  }
  return res.data as RoleDto
}

async function updateRolePermissions(
  data: UpdateRolePermissionsCommand,
): Promise<void> {
  const res = await api.PUT(
    `/api/admin/roles/${data.roleId}/permissions` as never,
    {
      body: data as never,
    },
  )
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to update permissions")
  }
}

async function getPermissions(): Promise<PermissionDto[]> {
  const res = await api.GET("/api/admin/permissions" as never, {})
  if (res.error) {
    const err = res.error as { detail?: string; title?: string }
    throw new Error(err.detail || err.title || "Failed to fetch permissions")
  }
  return res.data as PermissionDto[]
}

// ---- TanStack Query hooks ----

export function useUsersQuery() {
  return useQuery({
    queryKey: ["admin", "users"],
    queryFn: getUsers,
  })
}

export function useCreateUserMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: createUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "users"] })
    },
  })
}

export function useUpdateUserMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: updateUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "users"] })
    },
  })
}

export function useAssignRolesMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: assignRoles,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "users"] })
    },
  })
}

export function useRolesQuery() {
  return useQuery({
    queryKey: ["admin", "roles"],
    queryFn: getRoles,
  })
}

export function useCreateRoleMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: createRole,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "roles"] })
    },
  })
}

export function useUpdateRolePermissionsMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: updateRolePermissions,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "roles"] })
    },
  })
}

export function usePermissionsQuery() {
  return useQuery({
    queryKey: ["admin", "permissions"],
    queryFn: getPermissions,
  })
}
