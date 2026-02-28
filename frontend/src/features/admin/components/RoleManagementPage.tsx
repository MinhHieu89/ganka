import { useState } from "react"
import { useTranslation } from "react-i18next"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import { IconPlus, IconLoader2 } from "@tabler/icons-react"
import { Button } from "@/shared/components/ui/button"
import { Input } from "@/shared/components/ui/input"
import { Label } from "@/shared/components/ui/label"
import { Skeleton } from "@/shared/components/ui/skeleton"
import { Separator } from "@/shared/components/ui/separator"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog"
import { RoleTable } from "./RoleTable"
import { PermissionMatrix } from "./PermissionMatrix"
import { useRoles } from "@/features/admin/hooks/useRoles"

const createRoleSchema = z.object({
  name: z.string().min(1, "required"),
  description: z.string().min(1, "required"),
})

type CreateRoleFormValues = z.infer<typeof createRoleSchema>

export function RoleManagementPage() {
  const { t } = useTranslation("auth")
  const { t: tCommon } = useTranslation("common")
  const {
    roles,
    permissionsByModule,
    allActions,
    selectedRole,
    isCreateDialogOpen,
    isLoading,
    selectRole,
    createRole,
    updatePermissions,
    setIsCreateDialogOpen,
    isCreating,
    isUpdatingPermissions,
  } = useRoles()

  const form = useForm<CreateRoleFormValues>({
    resolver: zodResolver(createRoleSchema),
    defaultValues: {
      name: "",
      description: "",
    },
  })

  const handleCreateRole = async (data: CreateRoleFormValues) => {
    try {
      await createRole({
        name: data.name,
        description: data.description,
        permissionIds: [],
      })
      toast.success(t("admin.roleCreated"))
      form.reset()
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : tCommon("status.error"),
      )
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("admin.roleManagement")}</h1>
        <Button onClick={() => setIsCreateDialogOpen(true)}>
          <IconPlus className="h-4 w-4 mr-2" />
          {t("admin.createRole")}
        </Button>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      ) : (
        <>
          <RoleTable
            roles={roles}
            selectedRoleId={selectedRole?.id ?? null}
            onSelectRole={selectRole}
          />

          {selectedRole && (
            <>
              <Separator />
              <PermissionMatrix
                role={selectedRole}
                permissionsByModule={permissionsByModule}
                allActions={allActions}
                onSave={updatePermissions}
                isSaving={isUpdatingPermissions}
              />
            </>
          )}
        </>
      )}

      {/* Create Role Dialog */}
      <Dialog
        open={isCreateDialogOpen}
        onOpenChange={(v) => !v && setIsCreateDialogOpen(false)}
      >
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t("admin.createRole")}</DialogTitle>
          </DialogHeader>

          <form
            onSubmit={form.handleSubmit(handleCreateRole)}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label htmlFor="roleName">{t("admin.name")}</Label>
              <Input id="roleName" {...form.register("name")} />
              {form.formState.errors.name && (
                <p className="text-sm text-destructive">
                  {tCommon("validation.required")}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="roleDescription">{t("admin.description")}</Label>
              <Input
                id="roleDescription"
                {...form.register("description")}
              />
              {form.formState.errors.description && (
                <p className="text-sm text-destructive">
                  {tCommon("validation.required")}
                </p>
              )}
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => setIsCreateDialogOpen(false)}
              >
                {tCommon("buttons.cancel")}
              </Button>
              <Button type="submit" disabled={isCreating}>
                {isCreating && (
                  <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
                )}
                {tCommon("buttons.save")}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
