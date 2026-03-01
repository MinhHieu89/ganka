import { useTranslation } from "react-i18next"
import { IconUserPlus } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Skeleton } from "@/shared/components/Skeleton"
import { UserTable } from "./UserTable"
import { UserFormDialog } from "./UserFormDialog"
import { useUsers } from "@/features/admin/hooks/useUsers"

export function UserManagementPage() {
  const { t } = useTranslation("auth")
  const {
    users,
    isLoading,
    editingUser,
    isDialogOpen,
    openCreateDialog,
    openEditDialog,
    closeDialog,
    createUser,
    updateUser,
    isCreating,
    isUpdating,
  } = useUsers()

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("admin.userManagement")}</h1>
        <Button onClick={openCreateDialog}>
          <IconUserPlus className="h-4 w-4 mr-2" />
          {t("admin.addUser")}
        </Button>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <UserTable users={users} onEdit={openEditDialog} />
      )}

      <UserFormDialog
        open={isDialogOpen}
        onClose={closeDialog}
        editingUser={editingUser}
        onCreateUser={createUser}
        onUpdateUser={updateUser}
        isSubmitting={isCreating || isUpdating}
      />
    </div>
  )
}
