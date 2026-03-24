import { useEffect, useRef, useState } from "react"
import { useTranslation } from "react-i18next"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import {
  IconBuilding,
  IconUpload,
  IconPhoto,
  IconLoader2,
} from "@tabler/icons-react"

import { Button } from "@/shared/components/Button"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/Card"
import { Input } from "@/shared/components/Input"
import { Label } from "@/shared/components/Label"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Skeleton } from "@/shared/components/Skeleton"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  useClinicSettings,
  useUpdateClinicSettings,
  useUploadClinicLogo,
} from "@/features/admin/api/clinic-settings-api"

function useClinicSettingsSchema() {
  const { t } = useTranslation("common")
  return z.object({
    clinicName: z.string().min(1, t("validation.required")),
    clinicNameVi: z.string().optional().or(z.literal("")),
    address: z.string().min(1, t("validation.required")),
    phone: z.string().optional().or(z.literal("")),
    fax: z.string().optional().or(z.literal("")),
    email: z.string().email(t("validation.invalidEmail")).optional().or(z.literal("")),
    website: z.string().optional().or(z.literal("")),
    licenseNumber: z.string().optional().or(z.literal("")),
    tagline: z.string().optional().or(z.literal("")),
  })
}

type ClinicSettingsFormValues = z.infer<ReturnType<typeof useClinicSettingsSchema>>

export function ClinicSettingsPage() {
  const { t } = useTranslation("common")
  const schema = useClinicSettingsSchema()
  const { data: settings, isLoading } = useClinicSettings()
  const updateMutation = useUpdateClinicSettings()
  const uploadLogoMutation = useUploadClinicLogo()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [logoPreview, setLogoPreview] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<ClinicSettingsFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      clinicName: "",
      clinicNameVi: "",
      address: "",
      phone: "",
      fax: "",
      email: "",
      website: "",
      licenseNumber: "",
      tagline: "",
    },
  })

  useEffect(() => {
    if (settings) {
      reset({
        clinicName: settings.clinicName ?? "",
        clinicNameVi: settings.clinicNameVi ?? "",
        address: settings.address ?? "",
        phone: settings.phone ?? "",
        fax: settings.fax ?? "",
        email: settings.email ?? "",
        website: settings.website ?? "",
        licenseNumber: settings.licenseNumber ?? "",
        tagline: settings.tagline ?? "",
      })
      if (settings.logoBlobUrl) {
        setLogoPreview(settings.logoBlobUrl)
      }
    }
  }, [settings, reset])

  const onSubmit = async (data: ClinicSettingsFormValues) => {
    try {
      await updateMutation.mutateAsync({
        clinicName: data.clinicName,
        address: data.address,
        clinicNameVi: data.clinicNameVi || null,
        phone: data.phone || null,
        fax: data.fax || null,
        email: data.email || null,
        website: data.website || null,
        licenseNumber: data.licenseNumber || null,
        tagline: data.tagline || null,
      })
      toast.success(t("clinicSettings.saved", "Clinic settings saved successfully"))
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : t("status.error"),
      )
    }
  }

  const handleLogoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    // Client-side MIME type validation
    const ALLOWED_TYPES = ["image/jpeg", "image/png", "image/webp", "image/gif"]
    if (!ALLOWED_TYPES.includes(file.type)) {
      toast.error(t("clinicSettings.invalidImageType", "Only JPEG, PNG, WebP, or GIF images are allowed"))
      if (fileInputRef.current) fileInputRef.current.value = ""
      return
    }

    // Show preview immediately
    const reader = new FileReader()
    reader.onload = (ev) => {
      setLogoPreview(ev.target?.result as string)
    }
    reader.readAsDataURL(file)

    try {
      await uploadLogoMutation.mutateAsync(file)
      toast.success(t("clinicSettings.logoUploaded", "Logo uploaded successfully"))
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : t("status.error"),
      )
      // Revert preview on error
      setLogoPreview(settings?.logoBlobUrl ?? null)
    }

    // Reset file input
    if (fileInputRef.current) {
      fileInputRef.current.value = ""
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-[500px] w-full" />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <IconBuilding className="h-6 w-6 text-muted-foreground" />
        <h1 className="text-2xl font-bold">
          {t("clinicSettings.title", "Clinic Settings")}
        </h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>
            {t("clinicSettings.headerConfig", "Clinic Header Configuration")}
          </CardTitle>
          <CardDescription>
            {t(
              "clinicSettings.headerDescription",
              "Configure the clinic information that appears on all printed documents",
            )}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* Logo Upload Section */}
            <div className="space-y-2">
              <Label>
                {t("clinicSettings.logo", "Clinic Logo")}
              </Label>
              <div className="flex items-center gap-4">
                <div className="flex h-24 w-24 items-center justify-center border bg-muted/30 overflow-hidden">
                  {logoPreview ? (
                    <img
                      src={logoPreview}
                      alt="Clinic logo"
                      className="h-full w-full object-contain"
                    />
                  ) : (
                    <IconPhoto className="h-8 w-8 text-muted-foreground" />
                  )}
                </div>
                <div>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => fileInputRef.current?.click()}
                    disabled={uploadLogoMutation.isPending}
                  >
                    {uploadLogoMutation.isPending ? (
                      <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
                    ) : (
                      <IconUpload className="h-4 w-4 mr-2" />
                    )}
                    {t("clinicSettings.uploadLogo", "Upload Logo")}
                  </Button>
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/*"
                    className="hidden"
                    onChange={handleLogoUpload}
                  />
                </div>
              </div>
            </div>

            {/* Clinic Name Fields */}
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <Field data-invalid={!!errors.clinicName || undefined}>
                <FieldLabel required htmlFor="clinicName">
                  {t("clinicSettings.clinicName", "Clinic Name (EN)")}
                </FieldLabel>
                <Input
                  id="clinicName"
                  {...register("clinicName")}
                  aria-invalid={!!errors.clinicName || undefined}
                />
                {errors.clinicName && (
                  <FieldError>{errors.clinicName.message}</FieldError>
                )}
              </Field>
              <Field>
                <FieldLabel htmlFor="clinicNameVi">
                  {t("clinicSettings.clinicNameVi", "Clinic Name (VI)")}
                </FieldLabel>
                <Input
                  id="clinicNameVi"
                  {...register("clinicNameVi")}
                />
              </Field>
            </div>

            {/* Address */}
            <Field data-invalid={!!errors.address || undefined}>
              <FieldLabel required htmlFor="address">
                {t("clinicSettings.address", "Address")}
              </FieldLabel>
              <Input
                id="address"
                {...register("address")}
                aria-invalid={!!errors.address || undefined}
              />
              {errors.address && (
                <FieldError>{errors.address.message}</FieldError>
              )}
            </Field>

            {/* Contact Info */}
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="phone">
                  {t("clinicSettings.phone", "Phone")}
                </FieldLabel>
                <Input
                  id="phone"
                  type="tel"
                  {...register("phone")}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="fax">
                  {t("clinicSettings.fax", "Fax")}
                </FieldLabel>
                <Input
                  id="fax"
                  type="tel"
                  {...register("fax")}
                />
              </Field>
            </div>

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <Field data-invalid={!!errors.email || undefined}>
                <FieldLabel htmlFor="email">
                  {t("clinicSettings.email", "Email")}
                </FieldLabel>
                <Input
                  id="email"
                  type="email"
                  {...register("email")}
                  aria-invalid={!!errors.email || undefined}
                />
                {errors.email && (
                  <FieldError>{errors.email.message}</FieldError>
                )}
              </Field>
              <Field>
                <FieldLabel htmlFor="website">
                  {t("clinicSettings.website", "Website")}
                </FieldLabel>
                <Input
                  id="website"
                  {...register("website")}
                />
              </Field>
            </div>

            {/* License Number */}
            <Field>
              <FieldLabel htmlFor="licenseNumber">
                {t("clinicSettings.licenseNumber", "License Number (So GPHN)")}
              </FieldLabel>
              <Input
                id="licenseNumber"
                {...register("licenseNumber")}
              />
            </Field>

            {/* Tagline */}
            <Field>
              <FieldLabel htmlFor="tagline">
                {t("clinicSettings.tagline", "Tagline")}
              </FieldLabel>
              <AutoResizeTextarea
                id="tagline"
                rows={2}
                {...register("tagline")}
              />
            </Field>

            {/* Save Button */}
            <div className="flex justify-end">
              <Button
                type="submit"
                disabled={updateMutation.isPending || (!isDirty && !updateMutation.isPending)}
              >
                {updateMutation.isPending && (
                  <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
                )}
                {t("buttons.save")}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
