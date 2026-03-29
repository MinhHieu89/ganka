// -- Types matching backend Clinical.Contracts.Dtos for Technician --

export type TechnicianStatus = "waiting" | "in_progress" | "red_flag" | "completed"

export type TechnicianVisitType = "new" | "follow_up" | "additional"

export interface TechnicianDashboardRow {
  orderId: string
  visitId: string
  patientId: string
  patientName: string
  patientCode: string | null
  birthYear: number | null
  checkinTime: string // ISO datetime
  waitMinutes: number
  reason: string | null
  visitType: TechnicianVisitType
  status: TechnicianStatus
  technicianName: string | null
  redFlagReason: string | null
  isRedFlag: boolean
}

export interface TechnicianKpi {
  waiting: number
  inProgress: number
  completed: number
  redFlag: number
}

export interface TechnicianDashboardFilters {
  status?: TechnicianStatus
  search?: string
  page: number
  pageSize: number
}

export interface TechnicianDashboardResponse {
  items: TechnicianDashboardRow[]
  totalCount: number
}
