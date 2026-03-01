import { create } from "zustand"
import { persist } from "zustand/middleware"

export interface RecentPatient {
  id: string
  fullName: string
  patientCode: string
  phone: string
}

interface RecentPatientsState {
  recent: RecentPatient[]
  addRecent: (patient: RecentPatient) => void
  clearRecent: () => void
}

const MAX_RECENT = 10

export const useRecentPatientsStore = create<RecentPatientsState>()(
  persist(
    (set) => ({
      recent: [],
      addRecent: (patient) =>
        set((state) => {
          // Deduplicate by id, move existing to front, cap at MAX_RECENT
          const filtered = state.recent.filter((p) => p.id !== patient.id)
          return { recent: [patient, ...filtered].slice(0, MAX_RECENT) }
        }),
      clearRecent: () => set({ recent: [] }),
    }),
    {
      name: "ganka28-recent-patients",
    },
  ),
)
