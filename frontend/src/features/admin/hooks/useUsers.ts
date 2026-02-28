import { useState, useMemo } from "react"
import {
  useUsersQuery,
  useCreateUserMutation,
  useUpdateUserMutation,
  type UserDto,
  type CreateUserCommand,
  type UpdateUserCommand,
} from "@/features/admin/api/admin-api"

export function useUsers() {
  const usersQuery = useUsersQuery()
  const createMutation = useCreateUserMutation()
  const updateMutation = useUpdateUserMutation()
  const [editingUser, setEditingUser] = useState<UserDto | null>(null)
  const [isDialogOpen, setIsDialogOpen] = useState(false)

  const users = useMemo(() => usersQuery.data ?? [], [usersQuery.data])

  const openCreateDialog = () => {
    setEditingUser(null)
    setIsDialogOpen(true)
  }

  const openEditDialog = (user: UserDto) => {
    setEditingUser(user)
    setIsDialogOpen(true)
  }

  const closeDialog = () => {
    setEditingUser(null)
    setIsDialogOpen(false)
  }

  const createUser = async (data: CreateUserCommand) => {
    await createMutation.mutateAsync(data)
    closeDialog()
  }

  const updateUser = async (data: UpdateUserCommand) => {
    await updateMutation.mutateAsync(data)
    closeDialog()
  }

  return {
    users,
    isLoading: usersQuery.isLoading,
    error: usersQuery.error,
    editingUser,
    isDialogOpen,
    openCreateDialog,
    openEditDialog,
    closeDialog,
    createUser,
    updateUser,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
  }
}
