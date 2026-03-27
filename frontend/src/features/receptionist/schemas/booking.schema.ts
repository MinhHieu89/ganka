import { z } from "zod"

export const bookingSchema = z
  .object({
    patientId: z.string().uuid().optional(),
    guestName: z.string().max(200).optional(),
    guestPhone: z
      .string()
      .regex(/^0\d{9,10}$/, "So dien thoai khong hop le")
      .optional()
      .or(z.literal("")),
    reason: z.string().max(500).optional(),
    doctorId: z.string().uuid().optional(),
    date: z.string().min(1, "Ngay hen la bat buoc"),
    startTime: z.string().min(1, "Gio hen la bat buoc"),
  })
  .refine(
    (data) => data.patientId || (data.guestName && data.guestPhone),
    {
      message: "Can cung cap thong tin benh nhan hoac khach",
      path: ["guestName"],
    },
  )

export type BookingFormValues = z.infer<typeof bookingSchema>
