import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { createValidationMessages } from "@/shared/lib/validation"
import { toast } from "sonner"
import { IconPlus, IconLoader2 } from "@tabler/icons-react"
import { Button } from "@/shared/components/Button"
import { Input } from "@/shared/components/Input"
import { Skeleton } from "@/shared/components/Skeleton"
import { Separator } from "@/shared/components/Separator"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { RoleTable } from "./RoleTable"
import { PermissionMatrix } from "./PermissionMatrix"
import { useRoles } from "@/features/admin/hooks/useRoles"

function createRoleSchemaFactory(t: (key: string, opts?: Record<string, unknown>) => string) {
  const v = createValidationMessages(t)
  return z.object({
    name: z.string().min(1, v.required),
    description: z.string().min(1, v.required),
  })
}

type CreateRoleFormValues = z.infer<ReturnType<typeof createRoleSchemaFactory>>

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

  const createRoleSchema = useMemo(() => createRoleSchemaFactory(tCommon), [tCommon])
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
            <Controller
              name="name"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel required htmlFor={field.name}>{t("admin.name")}</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{tCommon("validation.required")}</FieldError>
                  )}
                </Field>
              )}
            />

            <Controller
              name="description"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid || undefined}>
                  <FieldLabel required htmlFor={field.name}>{t("admin.description")}</FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    aria-invalid={fieldState.invalid || undefined}
                  />
                  {fieldState.error && (
                    <FieldError>{tCommon("validation.required")}</FieldError>
                  )}
                </Field>
              )}
            />

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
