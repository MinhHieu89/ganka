// -- Query key factory for treatment module --

export const treatmentKeys = {
  all: ["treatments"] as const,
  templates: () => [...treatmentKeys.all, "templates"] as const,
  template: (id: string) => [...treatmentKeys.templates(), id] as const,
  packages: () => [...treatmentKeys.all, "packages"] as const,
  package: (id: string) => [...treatmentKeys.packages(), id] as const,
  patientPackages: (patientId: string) =>
    [...treatmentKeys.all, "patient", patientId] as const,
  dueSoon: () => [...treatmentKeys.all, "due-soon"] as const,
  sessions: (packageId: string) =>
    [...treatmentKeys.all, "sessions", packageId] as const,
  pendingCancellations: () =>
    [...treatmentKeys.all, "pending-cancellations"] as const,
}
