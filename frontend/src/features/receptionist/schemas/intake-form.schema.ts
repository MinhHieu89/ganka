import { z } from "zod"

export const intakeFormSchema = z.object({
  fullName: z.string().min(1, "Ho ten la bat buoc").max(200),
  phone: z
    .string()
    .min(1, "So dien thoai la bat buoc")
    .regex(/^0\d{9,10}$/, "So dien thoai khong hop le"),
  dateOfBirth: z.string().min(1, "Ngay sinh la bat buoc"),
  gender: z.string().min(1, "Gioi tinh la bat buoc"),
  address: z.string().max(500).optional(),
  cccd: z.string().optional(),
  email: z.string().email("Email khong hop le").optional().or(z.literal("")),
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
