import { useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { IconLoader2 } from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog"
import { Input } from "@/shared/components/ui/input"
import { Label } from "@/shared/components/ui/label"
import { Button } from "@/shared/components/ui/button"
import { Checkbox } from "@/shared/components/ui/checkbox"
import { Badge } from "@/shared/components/ui/badge"
import {
  useRolesQuery,
  type UserDto,
  type CreateUserCommand,
  type UpdateUserCommand,
} from "@/features/admin/api/admin-api"

const createSchema = z.object({
  email: z.string().min(1, "required").email("invalidEmail"),
  fullName: z.string().min(1, "required"),
  password: z.string().min(8, "minLength"),
  roleIds: z.array(z.string()).min(1, "required"),
})

const editSchema = z.object({
  fullName: z.string().min(1, "required"),
  isActive: z.boolean(),
  roleIds: z.array(z.string()).min(1, "required"),
})

type CreateFormValues = z.infer<typeof createSchema>
type EditFormValues = z.infer<typeof editSchema>

interface UserFormDialogProps {
  open: boolean
  onClose: () => void
  editingUser: UserDto | null
  onCreateUser: (data: CreateUserCommand) => Promise<void>
  onUpdateUser: (data: UpdateUserCommand) => Promise<void>
  isSubmitting: boolean
}

export function UserFormDialog({
  open,
  onClose,
  editingUser,
  onCreateUser,
  onUpdateUser,
  isSubmitting,
}: UserFormDialogProps) {
  const { t } = useTranslation("auth")
  const { t: tCommon } = useTranslation("common")
  const rolesQuery = useRolesQuery()
  const roles = rolesQuery.data ?? []

  const isEditMode = editingUser !== null

  // Create mode form
  const createForm = useForm<CreateFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: {
      email: "",
      fullName: "",
      password: "",
      roleIds: [],
    },
  })

  // Edit mode form
  const editForm = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    defaultValues: {
      fullName: "",
      isActive: true,
      roleIds: [],
    },
  })

  // Reset forms when dialog opens/closes or user changes
  useEffect(() => {
    if (open && editingUser) {
      // Find role IDs from role names
      const roleIds = roles
        .filter((r) => editingUser.roles.includes(r.name))
        .map((r) => r.id)
      editForm.reset({
        fullName: editingUser.fullName,
        isActive: editingUser.isActive,
        roleIds,
      })
    } else if (open && !editingUser) {
      createForm.reset({
        email: "",
        fullName: "",
        password: "",
        roleIds: [],
      })
    }
  }, [open, editingUser, roles, createForm, editForm])

  const handleCreateSubmit = async (data: CreateFormValues) => {
    try {
      await onCreateUser(data)
      toast.success(t("admin.userCreated"))
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : tCommon("status.error"),
      )
    }
  }

  const handleEditSubmit = async (data: EditFormValues) => {
    if (!editingUser) return
    try {
      await onUpdateUser({
        userId: editingUser.id,
        fullName: data.fullName,
        isActive: data.isActive,
        roleIds: data.roleIds,
      })
      toast.success(t("admin.userUpdated"))
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : tCommon("status.error"),
      )
    }
  }

  const currentRoleIds = isEditMode
    ? editForm.watch("roleIds")
    : createForm.watch("roleIds")

  const toggleRole = (roleId: string) => {
    if (isEditMode) {
      const current = editForm.getValues("roleIds")
      if (current.includes(roleId)) {
        editForm.setValue(
          "roleIds",
          current.filter((id: string) => id !== roleId),
          { shouldValidate: true },
        )
      } else {
        editForm.setValue("roleIds", [...current, roleId], {
          shouldValidate: true,
        })
      }
    } else {
      const current = createForm.getValues("roleIds")
      if (current.includes(roleId)) {
        createForm.setValue(
          "roleIds",
          current.filter((id: string) => id !== roleId),
          { shouldValidate: true },
        )
      } else {
        createForm.setValue("roleIds", [...current, roleId], {
          shouldValidate: true,
        })
      }
    }
  }

  const getErrorMessage = (
    error: { message?: string } | undefined,
  ): string | undefined => {
    if (!error?.message) return undefined
    if (error.message === "required") return tCommon("validation.required")
    if (error.message === "invalidEmail")
      return tCommon("validation.invalidEmail")
    if (error.message === "minLength")
      return tCommon("validation.minLength", { min: 8 })
    return error.message
  }

  return (
    <Dialog open={open} onOpenChange={(v) => !v && onClose()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>
            {isEditMode ? t("admin.editUser") : t("admin.createUser")}
          </DialogTitle>
        </DialogHeader>

        <form
          onSubmit={
            isEditMode
              ? editForm.handleSubmit(handleEditSubmit)
              : createForm.handleSubmit(handleCreateSubmit)
          }
          className="space-y-4"
        >
          {!isEditMode && (
            <div className="space-y-2">
              <Label htmlFor="email">{t("admin.email")}</Label>
              <Input
                id="email"
                type="email"
                {...createForm.register("email")}
              />
              {createForm.formState.errors.email && (
                <p className="text-sm text-destructive">
                  {getErrorMessage(createForm.formState.errors.email)}
                </p>
              )}
            </div>
          )}

          <div className="space-y-2">
            <Label htmlFor="fullName">{t("admin.fullName")}</Label>
            <Input
              id="fullName"
              {...(isEditMode
                ? editForm.register("fullName")
                : createForm.register("fullName"))}
            />
            {(isEditMode
              ? editForm.formState.errors.fullName
              : createForm.formState.errors.fullName) && (
              <p className="text-sm text-destructive">
                {getErrorMessage(
                  isEditMode
                    ? editForm.formState.errors.fullName
                    : createForm.formState.errors.fullName,
                )}
              </p>
            )}
          </div>

          {!isEditMode && (
            <div className="space-y-2">
              <Label htmlFor="password">{t("admin.password")}</Label>
              <Input
                id="password"
                type="password"
                {...createForm.register("password")}
              />
              {createForm.formState.errors.password && (
                <p className="text-sm text-destructive">
                  {getErrorMessage(createForm.formState.errors.password)}
                </p>
              )}
            </div>
          )}

          {isEditMode && (
            <div className="flex items-center space-x-2">
              <Checkbox
                id="isActive"
                checked={editForm.watch("isActive")}
                onCheckedChange={(checked) =>
                  editForm.setValue("isActive", checked === true)
                }
              />
              <Label htmlFor="isActive" className="font-normal cursor-pointer">
                {t("admin.active")}
              </Label>
            </div>
          )}

          <div className="space-y-2">
            <Label>{t("admin.selectRoles")}</Label>
            <div className="border p-3 space-y-2 max-h-48 overflow-y-auto">
              {roles.map((role) => (
                <div key={role.id} className="flex items-center space-x-2">
                  <Checkbox
                    id={`role-${role.id}`}
                    checked={currentRoleIds.includes(role.id)}
                    onCheckedChange={() => toggleRole(role.id)}
                  />
                  <Label
                    htmlFor={`role-${role.id}`}
                    className="font-normal cursor-pointer flex items-center gap-2"
                  >
                    {role.name}
                    {role.isSystem && (
                      <Badge
                        variant="outline"
                        className="text-xs"
                      >
                        {t("admin.systemRole")}
                      </Badge>
                    )}
                  </Label>
                </div>
              ))}
            </div>
            {(isEditMode
              ? editForm.formState.errors.roleIds
              : createForm.formState.errors.roleIds) && (
              <p className="text-sm text-destructive">
                {tCommon("validation.required")}
              </p>
            )}
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              {tCommon("buttons.cancel")}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              {tCommon("buttons.save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
