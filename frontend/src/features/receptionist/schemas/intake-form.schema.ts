import { z } from "zod"

export const intakeFormSchema = z.object({
  fullName: z.string().min(1, "Họ tên là bắt buộc").max(200),
  phone: z
    .string()
    .min(1, "Số điện thoại là bắt buộc")
    .regex(/^0\d{9,10}$/, "Số điện thoại không hợp lệ"),
  dateOfBirth: z.string().min(1, "Ngày sinh là bắt buộc"),
  gender: z.string().min(1, "Giới tính là bắt buộc"),
  address: z.string().max(500).optional(),
  cccd: z.string().optional(),
  email: z.string().email("Email không hợp lệ").optional().or(z.literal("")),
  occupation: z.string().max(200).optional(),
  reason: z.string().max(500).optional(),
  ocularHistory: z.string().max(2000).optional(),
  systemicHistory: z.string().max(2000).optional(),
  currentMedications: z.string().max(2000).optional(),
  screenTimeHours: z.coerce.number().min(0).optional(),
  workEnvironment: z
    .enum(["office", "outdoor", "mixed", "other"])
    .optional(),
  contactLensUsage: z
    .enum(["none", "soft", "rgp", "ortho_k", "other"])
    .optional(),
  lifestyleNotes: z.string().max(2000).optional(),
})

export type IntakeFormValues = z.infer<typeof intakeFormSchema>
