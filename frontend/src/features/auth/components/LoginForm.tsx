import { Controller, useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useTranslation } from "react-i18next"
import { useNavigate } from "@tanstack/react-router"
import { toast } from "sonner"
import { IconLoader2, IconEye, IconEyeOff } from "@tabler/icons-react"
import { useState } from "react"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import { Button } from "@/shared/components/Button"
import { Checkbox } from "@/shared/components/Checkbox"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import { useAuth } from "@/features/auth/hooks/useAuth"

interface LoginFormProps {
  redirectTo?: string
}

export function LoginForm({ redirectTo }: LoginFormProps) {
  const { t } = useTranslation("auth")
  const { t: tCommon } = useTranslation("common")
  const navigate = useNavigate()
  const { login, isLoggingIn, loginError } = useAuth()
  const [showPassword, setShowPassword] = useState(false)

  const loginSchema = z.object({
    username: z.string().min(1, tCommon("validation.required")),
    password: z.string().min(8, tCommon("validation.minLength", { min: 8 })),
    rememberMe: z.boolean().default(false),
  })

  type LoginFormValues = z.infer<typeof loginSchema>

  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      username: "",
      password: "",
      rememberMe: false,
    },
  })

  const onSubmit = async (data: LoginFormValues) => {
    try {
      // Send username value as email parameter to maintain backward compatibility with API contract
      await login(data.username, data.password, data.rememberMe)
      navigate({ to: redirectTo || "/dashboard" })
    } catch (error) {
      if (error instanceof Error && error.message.includes("fetch")) {
        toast.error(tCommon("status.error"))
      }
      // Login errors are shown inline via loginError
    }
  }

  return (
    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
      <Controller
        name="username"
        control={form.control}
        render={({ field, fieldState }) => (
          <Field data-invalid={fieldState.invalid || undefined}>
            <FieldLabel htmlFor={field.name}>
              {t("login.username")}
            </FieldLabel>
            <Input
              {...field}
              id={field.name}
              type="text"
              placeholder="doctor@ganka28.com"
              autoComplete="username"
              aria-invalid={fieldState.invalid || undefined}
            />
            {fieldState.error && (
              <FieldError>{fieldState.error.message}</FieldError>
            )}
          </Field>
        )}
      />

      <Controller
        name="password"
        control={form.control}
        render={({ field, fieldState }) => (
          <Field data-invalid={fieldState.invalid || undefined}>
            <FieldLabel htmlFor={field.name}>
              {t("login.password")}
            </FieldLabel>
            <div className="relative">
              <Input
                {...field}
                id={field.name}
                type={showPassword ? "text" : "password"}
                autoComplete="current-password"
                className="pr-10"
                aria-invalid={fieldState.invalid || undefined}
              />
              <Button
                type="button"
                variant="ghost"
                size="sm"
                className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                onClick={() => setShowPassword(!showPassword)}
              >
                {showPassword ? (
                  <IconEyeOff className="h-4 w-4 text-muted-foreground" />
                ) : (
                  <IconEye className="h-4 w-4 text-muted-foreground" />
                )}
              </Button>
            </div>
            {fieldState.error && (
              <FieldError>{fieldState.error.message}</FieldError>
            )}
          </Field>
        )}
      />

      <Controller
        name="rememberMe"
        control={form.control}
        render={({ field }) => (
          <div className="flex items-center space-x-2">
            <Checkbox
              id="rememberMe"
              checked={field.value}
              onCheckedChange={field.onChange}
            />
            <Label
              htmlFor="rememberMe"
              className="text-sm font-normal cursor-pointer"
            >
              {t("login.rememberMe")}
            </Label>
          </div>
        )}
      />

      {loginError && (
        <div className="p-3 text-sm text-destructive bg-destructive/10 border border-destructive/20">
          {t("login.invalidCredentials")}
        </div>
      )}

      <Button type="submit" className="w-full" disabled={isLoggingIn}>
        {isLoggingIn ? (
          <>
            <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
            {t("login.submit")}
          </>
        ) : (
          t("login.submit")
        )}
      </Button>
    </form>
  )
}
