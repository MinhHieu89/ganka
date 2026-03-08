// -- Treatment type enums matching backend Treatment.Domain.Enums --

export type TreatmentType = "IPL" | "LLLT" | "LidCare"

export type PackageStatus =
  | "Active"
  | "Paused"
  | "PendingCancellation"
  | "Cancelled"
  | "Switched"
  | "Completed"

export type SessionStatus = "Scheduled" | "InProgress" | "Completed" | "Cancelled"

export type PricingMode = "PerSession" | "PerPackage"

// -- DTOs matching backend Treatment.Contracts.Dtos --

export interface TreatmentProtocolDto {
  id: string
  name: string
  treatmentType: string
  defaultSessionCount: number
  pricingMode: string
  defaultPackagePrice: number
  defaultSessionPrice: number
  minIntervalDays: number
  maxIntervalDays: number
  defaultParametersJson: string
  cancellationDeductionPercent: number
  isActive: boolean
  description: string | null
  createdAt: string
}

export interface TreatmentPackageDto {
  id: string
  protocolTemplateId: string
  protocolTemplateName: string
  patientId: string
  patientName: string
  treatmentType: string
  status: string
  totalSessions: number
  sessionsCompleted: number
  sessionsRemaining: number
  pricingMode: string
  packagePrice: number
  sessionPrice: number
  minIntervalDays: number
  parametersJson: string
  visitId: string | null
  createdAt: string
  lastSessionDate: string | null
  nextDueDate: string | null
  sessions: TreatmentSessionDto[]
  cancellationRequest: CancellationRequestDto | null
}

export interface TreatmentSessionDto {
  id: string
  sessionNumber: number
  status: string
  parametersJson: string
  osdiScore: number | null
  osdiSeverity: string | null
  clinicalNotes: string | null
  performedById: string
  performedByName: string | null
  visitId: string | null
  scheduledAt: string | null
  completedAt: string | null
  createdAt: string
  intervalOverrideReason: string | null
  consumables: SessionConsumableDto[]
}

export interface SessionConsumableDto {
  id: string
  consumableItemId: string
  consumableName: string
  quantity: number
}

export interface CancellationRequestDto {
  id: string
  requestedById: string
  requestedByName: string
  requestedAt: string
  reason: string
  deductionPercent: number
  refundAmount: number
  status: string
  approvedById: string | null
  approvedByName: string | null
  approvedAt: string | null
  rejectionReason: string | null
}

// -- Response types --

export interface IntervalWarning {
  daysSinceLast: number
  minIntervalDays: number
}

export interface RecordSessionResponse {
  session: TreatmentSessionDto
  warning: IntervalWarning | null
}

// -- Command types for mutations --

export interface CreateProtocolTemplateCommand {
  name: string
  treatmentType: number
  defaultSessionCount: number
  pricingMode: number
  defaultPackagePrice: number
  defaultSessionPrice: number
  minIntervalDays: number
  maxIntervalDays: number
  defaultParametersJson?: string | null
  cancellationDeductionPercent: number
  description?: string | null
}

export interface UpdateProtocolTemplateCommand {
  id: string
  name: string
  treatmentType: number
  defaultSessionCount: number
  pricingMode: number
  defaultPackagePrice: number
  defaultSessionPrice: number
  minIntervalDays: number
  maxIntervalDays: number
  defaultParametersJson?: string | null
  cancellationDeductionPercent: number
  description?: string | null
}

export interface CreateTreatmentPackageCommand {
  protocolTemplateId: string
  patientId: string
  patientName: string
  totalSessions?: number | null
  pricingMode?: number | null
  packagePrice?: number | null
  sessionPrice?: number | null
  minIntervalDays?: number | null
  parametersJson?: string | null
  visitId?: string | null
}

export interface ConsumableInput {
  consumableItemId: string
  consumableName: string
  quantity: number
}

export interface RecordTreatmentSessionCommand {
  packageId: string
  parametersJson: string
  osdiScore?: number | null
  osdiSeverity?: string | null
  clinicalNotes?: string | null
  performedById: string
  visitId?: string | null
  scheduledAt?: string | null
  intervalOverrideReason?: string | null
  consumables: ConsumableInput[]
}

export interface ModifyTreatmentPackageCommand {
  packageId: string
  totalSessions?: number | null
  parametersJson?: string | null
  minIntervalDays?: number | null
  reason: string
}

export interface SwitchTreatmentTypeCommand {
  packageId: string
  newProtocolTemplateId: string
  reason: string
}

export interface RequestCancellationCommand {
  packageId: string
  reason: string
}

export interface ApproveCancellationCommand {
  packageId: string
  managerId: string
  managerPin: string
  deductionPercent: number
}

export interface RejectCancellationCommand {
  packageId: string
  managerId: string
  rejectionReason: string
}

export interface PauseTreatmentPackageCommand {
  packageId: string
  action: number // 0 = Pause, 1 = Resume
}
