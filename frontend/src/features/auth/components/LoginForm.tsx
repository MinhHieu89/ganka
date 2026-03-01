import { useForm } from "react-hook-form"
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
import { useAuth } from "@/features/auth/hooks/useAuth"

const loginSchema = z.object({
  email: z.string().min(1, "required").email("invalidEmail"),
  password: z.string().min(8, "minLength"),
  rememberMe: z.boolean().default(false),
})

type LoginFormValues = z.infer<typeof loginSchema>

interface LoginFormProps {
  redirectTo?: string
}

export function LoginForm({ redirectTo }: LoginFormProps) {
  const { t } = useTranslation("auth")
  const { t: tCommon } = useTranslation("common")
  const navigate = useNavigate()
  const { login, isLoggingIn, loginError } = useAuth()
  const [showPassword, setShowPassword] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
      rememberMe: false,
    },
  })

  const rememberMe = watch("rememberMe")

  const onSubmit = async (data: LoginFormValues) => {
    try {
      await login(data.email, data.password, data.rememberMe)
      navigate({ to: redirectTo || "/dashboard" })
    } catch (error) {
      if (error instanceof Error && error.message.includes("fetch")) {
        toast.error(tCommon("status.error"))
      }
      // Login errors are shown inline via loginError
    }
  }

  const getErrorMessage = (
    error: { message?: string } | undefined,
  ): string | undefined => {
    if (!error?.message) return undefined
    // Map Zod error codes to i18n keys
    if (error.message === "required") return tCommon("validation.required")
    if (error.message === "invalidEmail")
      return tCommon("validation.invalidEmail")
    if (error.message === "minLength")
      return tCommon("validation.minLength", { min: 8 })
    return error.message
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-2">
        <Label htmlFor="email">{t("login.email")}</Label>
        <Input
          id="email"
          type="email"
          placeholder="doctor@ganka28.com"
          autoComplete="email"
          {...register("email")}
        />
        {errors.email && (
          <p className="text-sm text-destructive">
            {getErrorMessage(errors.email)}
          </p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="password">{t("login.password")}</Label>
        <div className="relative">
          <Input
            id="password"
            type={showPassword ? "text" : "password"}
            autoComplete="current-password"
            className="pr-10"
            {...register("password")}
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
        {errors.password && (
          <p className="text-sm text-destructive">
            {getErrorMessage(errors.password)}
          </p>
        )}
      </div>

      <div className="flex items-center space-x-2">
        <Checkbox
          id="rememberMe"
          checked={rememberMe}
          onCheckedChange={(checked) =>
            setValue("rememberMe", checked === true)
          }
        />
        <Label htmlFor="rememberMe" className="text-sm font-normal cursor-pointer">
          {t("login.rememberMe")}
        </Label>
      </div>

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
