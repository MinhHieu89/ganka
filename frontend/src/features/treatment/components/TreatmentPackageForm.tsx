interface TreatmentPackageFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  visitId?: string
}

export function TreatmentPackageForm({
  open,
  onOpenChange,
}: TreatmentPackageFormProps) {
  if (!open) return null
  // Placeholder - will be implemented in Task 2
  return null
}
