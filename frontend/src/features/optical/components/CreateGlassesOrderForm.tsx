import { useState, useEffect, useMemo } from "react"
import { useForm, Controller, useFieldArray } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "sonner"
import {
  IconLoader2,
  IconPlus,
  IconTrash,
} from "@tabler/icons-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import { Input } from "@/shared/components/Input"
import { AutoResizeTextarea } from "@/shared/components/AutoResizeTextarea"
import { Button } from "@/shared/components/Button"
import { Field, FieldLabel, FieldError } from "@/shared/components/Field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/Select"
import { RadioGroup, RadioGroupItem } from "@/shared/components/RadioGroup"
import { Label } from "@/shared/components/Label"
import { DatePicker } from "@/shared/components/DatePicker"
import { Separator } from "@/shared/components/Separator"
import { formatVND } from "@/shared/lib/format-vnd"
import { usePatientList } from "@/features/patient/api/patient-api"
import { useCreateGlassesOrder, useComboPackages, useFrames, useLensCatalog } from "@/features/optical/api/optical-queries"
import { useActiveVisits } from "@/features/clinical/api/clinical-api"

const orderItemSchema = z.object({
  frameId: z.string().nullable().optional(),
  lensCatalogItemId: z.string().nullable().optional(),
  description: z.string().min(1, "Description is required"),
  unitPrice: z.coerce.number().min(0, "Price must be non-negative"),
  quantity: z.coerce.number().int().min(1, "Quantity must be at least 1"),
})

const createOrderSchema = z.object({
  patientId: z.string().min(1, "Patient is required"),
  visitId: z.string().min(1, "Visit is required"),
  opticalPrescriptionId: z.string().min(1, "Optical prescription is required"),
  processingType: z.number({ required_error: "Processing type is required" }),
  estimatedDeliveryDate: z.date().nullable().optional(),
  comboPackageId: z.string().nullable().optional(),
  notes: z.string().nullable().optional(),
  items: z.array(orderItemSchema).min(1, "At least one item is required"),
})

type CreateOrderFormValues = z.infer<typeof createOrderSchema>

interface CreateGlassesOrderFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function CreateGlassesOrderForm({ open, onOpenChange }: CreateGlassesOrderFormProps) {
  const [patientSearch, setPatientSearch] = useState("")
  const [frameSearch, setFrameSearch] = useState("")
  const [selectedPatientId, setSelectedPatientId] = useState<string | undefined>()
  const [selectedVisitId, setSelectedVisitId] = useState<string | undefined>()

  const createMutation = useCreateGlassesOrder()
  const { data: patientsResult } = usePatientList({
    search: patientSearch.length >= 2 ? patientSearch : undefined,
    pageSize: 20,
  })
  const patients = patientsResult?.items ?? []

  const { data: activeVisits } = useActiveVisits()
  const { data: framesResult } = useFrames({ pageSize: 100 })
  const frames = framesResult?.items ?? []
  const { data: lenses } = useLensCatalog()
  const { data: combos } = useComboPackages()

  const form = useForm<CreateOrderFormValues>({
    resolver: zodResolver(createOrderSchema),
    defaultValues: {
      patientId: "",
      visitId: "",
      opticalPrescriptionId: "",
      processingType: 0,
      estimatedDeliveryDate: null,
      comboPackageId: null,
      notes: "",
      items: [
        {
          frameId: null,
          lensCatalogItemId: null,
          description: "",
          unitPrice: 0,
          quantity: 1,
        },
      ],
    },
  })

  const { fields: itemFields, append: appendItem, remove: removeItem } = useFieldArray({
    control: form.control,
    name: "items",
  })

  // Get visits for the selected patient
  const patientVisits = useMemo(() => {
    if (!selectedPatientId) return []
    return (activeVisits ?? []).filter((v) => v.patientId === selectedPatientId)
  }, [activeVisits, selectedPatientId])

  // Get optical prescriptions for the selected visit
  // Note: We need visit detail to get prescriptions. For now, use a simplified approach
  // where the user enters the prescription ID after selecting a visit.
  // The prescription ID is auto-populated when a visit with prescription is selected.
  const selectedVisit = useMemo(() => {
    return patientVisits.find((v) => v.id === selectedVisitId)
  }, [patientVisits, selectedVisitId])

  // Calculate total price from items
  const watchedItems = form.watch("items")
  const totalPrice = useMemo(() => {
    return watchedItems.reduce((sum, item) => {
      const price = Number(item.unitPrice) || 0
      const qty = Number(item.quantity) || 1
      return sum + price * qty
    }, 0)
  }, [watchedItems])

  // Reset form when dialog closes
  useEffect(() => {
    if (!open) {
      form.reset()
      setPatientSearch("")
      setSelectedPatientId(undefined)
      setSelectedVisitId(undefined)
    }
  }, [open, form])

  const handlePatientChange = (patientId: string) => {
    setSelectedPatientId(patientId)
    form.setValue("patientId", patientId)
    form.setValue("visitId", "")
    form.setValue("opticalPrescriptionId", "")
    setSelectedVisitId(undefined)
  }

  const handleVisitChange = (visitId: string) => {
    setSelectedVisitId(visitId)
    form.setValue("visitId", visitId)
    form.setValue("opticalPrescriptionId", "")
  }

  const handleComboSelect = (comboId: string) => {
    if (comboId === "none") {
      form.setValue("comboPackageId", null)
      return
    }
    form.setValue("comboPackageId", comboId)
    const combo = combos?.find((c) => c.id === comboId)
    if (combo) {
      // Populate items from combo
      const comboItems: CreateOrderFormValues["items"] = []
      if (combo.frameId) {
        const frame = frames.find((f) => f.id === combo.frameId)
        comboItems.push({
          frameId: combo.frameId,
          lensCatalogItemId: null,
          description: frame ? `${frame.brand} ${frame.model} (Frame)` : "Frame",
          unitPrice: frame?.sellingPrice ?? 0,
          quantity: 1,
        })
      }
      if (combo.lensCatalogItemId) {
        const lens = lenses?.find((l) => l.id === combo.lensCatalogItemId)
        comboItems.push({
          frameId: null,
          lensCatalogItemId: combo.lensCatalogItemId,
          description: lens ? `${lens.name} (Lens)` : "Lens",
          unitPrice: lens?.sellingPrice ?? 0,
          quantity: 1,
        })
      }
      if (comboItems.length > 0) {
        form.setValue("items", comboItems)
      }
    }
  }

  async function onSubmit(values: CreateOrderFormValues) {
    try {
      await createMutation.mutateAsync({
        patientId: values.patientId,
        visitId: values.visitId,
        opticalPrescriptionId: values.opticalPrescriptionId,
        processingType: values.processingType,
        estimatedDeliveryDate: values.estimatedDeliveryDate
          ? values.estimatedDeliveryDate.toISOString().split("T")[0]
          : null,
        frameId: values.items.find((i) => i.frameId)?.frameId ?? null,
        lensCatalogItemId: values.items.find((i) => i.lensCatalogItemId)?.lensCatalogItemId ?? null,
        comboPackageId: values.comboPackageId ?? null,
        totalPrice,
        notes: values.notes || null,
      })
      toast.success("Glasses order created successfully")
      onOpenChange(false)
    } catch (error) {
      // Error handled by mutation onError
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Create Glasses Order</DialogTitle>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          {/* Patient selection */}
          <div className="grid grid-cols-2 gap-4">
            <Field>
              <FieldLabel>Patient *</FieldLabel>
              <div className="space-y-2">
                <Input
                  value={patientSearch}
                  onChange={(e) => setPatientSearch(e.target.value)}
                  placeholder="Search by name or code..."
                />
                {patients.length > 0 && patientSearch.length >= 2 && (
                  <div className="border rounded-md max-h-40 overflow-y-auto">
                    {patients.map((p) => (
                      <button
                        key={p.id}
                        type="button"
                        className="w-full text-left px-3 py-2 text-sm hover:bg-muted transition-colors"
                        onClick={() => {
                          handlePatientChange(p.id)
                          setPatientSearch(p.fullName)
                        }}
                      >
                        <span className="font-medium">{p.fullName}</span>
                        <span className="text-muted-foreground ml-2 text-xs">
                          {p.patientCode ?? p.phone}
                        </span>
                      </button>
                    ))}
                  </div>
                )}
              </div>
              {form.formState.errors.patientId && (
                <FieldError>{form.formState.errors.patientId.message}</FieldError>
              )}
            </Field>

            <Field>
              <FieldLabel>Visit *</FieldLabel>
              <Controller
                control={form.control}
                name="visitId"
                render={({ field }) => (
                  <Select
                    value={field.value}
                    onValueChange={(val) => {
                      field.onChange(val)
                      handleVisitChange(val)
                    }}
                    disabled={!selectedPatientId}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select visit..." />
                    </SelectTrigger>
                    <SelectContent>
                      {patientVisits.length === 0 ? (
                        <SelectItem value="_none" disabled>
                          No active visits
                        </SelectItem>
                      ) : (
                        patientVisits.map((v) => (
                          <SelectItem key={v.id} value={v.id}>
                            {new Date(v.visitDate).toLocaleDateString("vi-VN")} -{" "}
                            {v.doctorName}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                )}
              />
              {form.formState.errors.visitId && (
                <FieldError>{form.formState.errors.visitId.message}</FieldError>
              )}
            </Field>
          </div>

          {/* Optical Prescription */}
          <Field>
            <FieldLabel>Optical Prescription ID *</FieldLabel>
            <Controller
              control={form.control}
              name="opticalPrescriptionId"
              render={({ field }) => (
                <Input
                  {...field}
                  placeholder="Enter optical prescription ID from visit"
                  disabled={!selectedVisitId}
                />
              )}
            />
            {selectedVisit && (
              <p className="text-xs text-muted-foreground mt-1">
                Visit: {new Date(selectedVisit.visitDate).toLocaleDateString("vi-VN")} with Dr.{" "}
                {selectedVisit.doctorName}
              </p>
            )}
            {form.formState.errors.opticalPrescriptionId && (
              <FieldError>{form.formState.errors.opticalPrescriptionId.message}</FieldError>
            )}
          </Field>

          <Separator />

          {/* Processing type */}
          <Field>
            <FieldLabel>Processing Type *</FieldLabel>
            <Controller
              control={form.control}
              name="processingType"
              render={({ field }) => (
                <RadioGroup
                  value={String(field.value)}
                  onValueChange={(val) => field.onChange(Number(val))}
                  className="flex gap-6"
                >
                  <div className="flex items-center gap-2">
                    <RadioGroupItem value="0" id="inhouse" />
                    <Label htmlFor="inhouse">In-House</Label>
                  </div>
                  <div className="flex items-center gap-2">
                    <RadioGroupItem value="1" id="outsourced" />
                    <Label htmlFor="outsourced">Outsourced</Label>
                  </div>
                </RadioGroup>
              )}
            />
          </Field>

          {/* Estimated delivery date */}
          <div className="grid grid-cols-2 gap-4">
            <Field>
              <FieldLabel>Estimated Delivery Date</FieldLabel>
              <Controller
                control={form.control}
                name="estimatedDeliveryDate"
                render={({ field }) => (
                  <DatePicker
                    value={field.value ?? undefined}
                    onChange={field.onChange}
                    placeholder="Select date..."
                    fromDate={new Date()}
                    toDate={new Date(Date.now() + 365 * 24 * 60 * 60 * 1000)}
                  />
                )}
              />
            </Field>

            <Field>
              <FieldLabel>Combo Package (optional)</FieldLabel>
              <Controller
                control={form.control}
                name="comboPackageId"
                render={({ field }) => (
                  <Select
                    value={field.value ?? "none"}
                    onValueChange={(val) => {
                      field.onChange(val === "none" ? null : val)
                      handleComboSelect(val)
                    }}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select combo..." />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">No combo</SelectItem>
                      {(combos ?? [])
                        .filter((c) => c.isActive)
                        .map((c) => (
                          <SelectItem key={c.id} value={c.id}>
                            {c.name} — {formatVND(c.comboPrice)}
                          </SelectItem>
                        ))}
                    </SelectContent>
                  </Select>
                )}
              />
            </Field>
          </div>

          <Separator />

          {/* Order items */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <h3 className="font-semibold text-sm">Order Items</h3>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() =>
                  appendItem({
                    frameId: null,
                    lensCatalogItemId: null,
                    description: "",
                    unitPrice: 0,
                    quantity: 1,
                  })
                }
              >
                <IconPlus className="h-3.5 w-3.5 mr-1" />
                Add Item
              </Button>
            </div>

            {itemFields.map((field, index) => (
              <div
                key={field.id}
                className="border rounded-lg p-4 space-y-3 bg-muted/20"
              >
                <div className="grid grid-cols-2 gap-3">
                  <Field>
                    <FieldLabel>Frame (optional)</FieldLabel>
                    <Controller
                      control={form.control}
                      name={`items.${index}.frameId`}
                      render={({ field: f }) => (
                        <Select
                          value={f.value ?? "none"}
                          onValueChange={(val) => {
                            const frameId = val === "none" ? null : val
                            f.onChange(frameId)
                            if (frameId) {
                              const frame = frames.find((fr) => fr.id === frameId)
                              if (frame) {
                                form.setValue(
                                  `items.${index}.description`,
                                  `${frame.brand} ${frame.model} ${frame.color}`,
                                )
                                form.setValue(`items.${index}.unitPrice`, frame.sellingPrice)
                              }
                            }
                          }}
                        >
                          <SelectTrigger>
                            <SelectValue placeholder="Select frame..." />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="none">No frame</SelectItem>
                            {frames
                              .filter((fr) => fr.isActive && fr.stockQuantity > 0)
                              .filter(
                                (fr) =>
                                  !frameSearch ||
                                  `${fr.brand} ${fr.model}`.toLowerCase().includes(frameSearch.toLowerCase()),
                              )
                              .map((fr) => (
                                <SelectItem key={fr.id} value={fr.id}>
                                  {fr.brand} {fr.model} — {fr.color} ({formatVND(fr.sellingPrice)})
                                </SelectItem>
                              ))}
                          </SelectContent>
                        </Select>
                      )}
                    />
                  </Field>

                  <Field>
                    <FieldLabel>Lens (optional)</FieldLabel>
                    <Controller
                      control={form.control}
                      name={`items.${index}.lensCatalogItemId`}
                      render={({ field: f }) => (
                        <Select
                          value={f.value ?? "none"}
                          onValueChange={(val) => {
                            const lensId = val === "none" ? null : val
                            f.onChange(lensId)
                            if (lensId) {
                              const lens = lenses?.find((l) => l.id === lensId)
                              if (lens) {
                                if (!form.getValues(`items.${index}.description`)) {
                                  form.setValue(`items.${index}.description`, lens.name)
                                }
                                if (!form.getValues(`items.${index}.unitPrice`)) {
                                  form.setValue(`items.${index}.unitPrice`, lens.sellingPrice)
                                }
                              }
                            }
                          }}
                        >
                          <SelectTrigger>
                            <SelectValue placeholder="Select lens..." />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="none">No lens</SelectItem>
                            {(lenses ?? [])
                              .filter((l) => l.isActive)
                              .map((l) => (
                                <SelectItem key={l.id} value={l.id}>
                                  {l.name} — {formatVND(l.sellingPrice)}
                                </SelectItem>
                              ))}
                          </SelectContent>
                        </Select>
                      )}
                    />
                  </Field>
                </div>

                <Field>
                  <FieldLabel>Description *</FieldLabel>
                  <Controller
                    control={form.control}
                    name={`items.${index}.description`}
                    render={({ field: f }) => <Input {...f} />}
                  />
                  {form.formState.errors.items?.[index]?.description && (
                    <FieldError>
                      {form.formState.errors.items[index]?.description?.message}
                    </FieldError>
                  )}
                </Field>

                <div className="grid grid-cols-3 gap-3 items-end">
                  <Field>
                    <FieldLabel>Unit Price (VND) *</FieldLabel>
                    <Controller
                      control={form.control}
                      name={`items.${index}.unitPrice`}
                      render={({ field: f }) => (
                        <Input
                          {...f}
                          type="number"
                          min={0}
                          step={1000}
                        />
                      )}
                    />
                  </Field>

                  <Field>
                    <FieldLabel>Quantity *</FieldLabel>
                    <Controller
                      control={form.control}
                      name={`items.${index}.quantity`}
                      render={({ field: f }) => (
                        <Input {...f} type="number" min={1} />
                      )}
                    />
                  </Field>

                  <div className="flex items-end gap-2">
                    <div className="text-sm text-muted-foreground pb-2">
                      Subtotal:{" "}
                      <span className="font-semibold text-foreground">
                        {formatVND(
                          (Number(form.watch(`items.${index}.unitPrice`)) || 0) *
                            (Number(form.watch(`items.${index}.quantity`)) || 1),
                        )}
                      </span>
                    </div>
                    {itemFields.length > 1 && (
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:text-destructive"
                        onClick={() => removeItem(index)}
                      >
                        <IconTrash className="h-4 w-4" />
                      </Button>
                    )}
                  </div>
                </div>
              </div>
            ))}

            {form.formState.errors.items?.root && (
              <FieldError>{form.formState.errors.items.root.message}</FieldError>
            )}

            {/* Total price */}
            <div className="flex justify-end pt-2">
              <div className="text-right">
                <span className="text-sm text-muted-foreground">Total Price:</span>
                <div className="text-xl font-bold">{formatVND(totalPrice)}</div>
              </div>
            </div>
          </div>

          <Separator />

          {/* Notes */}
          <Field>
            <FieldLabel>Notes (optional)</FieldLabel>
            <Controller
              control={form.control}
              name="notes"
              render={({ field }) => (
                <AutoResizeTextarea
                  {...field}
                  value={field.value ?? ""}
                  rows={3}
                />
              )}
            />
          </Field>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={createMutation.isPending}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={createMutation.isPending}>
              {createMutation.isPending && (
                <IconLoader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              Create Order
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
