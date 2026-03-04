import { useState, useCallback, useRef, useEffect } from "react"
import { useTranslation } from "react-i18next"
import { toast } from "sonner"
import { Textarea } from "@/shared/components/Textarea"
import { useUpdateNotes } from "../api/clinical-api"
import { VisitSection } from "./VisitSection"

interface ExaminationNotesSectionProps {
  visitId: string
  initialNotes: string | null
  disabled: boolean
}

export function ExaminationNotesSection({
  visitId,
  initialNotes,
  disabled,
}: ExaminationNotesSectionProps) {
  const { t } = useTranslation("clinical")
  const updateNotesMutation = useUpdateNotes()
  const [notes, setNotes] = useState(initialNotes ?? "")
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const lastSavedRef = useRef(initialNotes ?? "")

  // Sync when parent data changes (e.g. after refetch)
  useEffect(() => {
    const newValue = initialNotes ?? ""
    if (newValue !== lastSavedRef.current) {
      setNotes(newValue)
      lastSavedRef.current = newValue
    }
  }, [initialNotes])

  const saveNotes = useCallback(
    (value: string) => {
      if (value === lastSavedRef.current) return
      lastSavedRef.current = value
      updateNotesMutation.mutate(
        {
          visitId,
          notes: value || null,
        },
        {
          onSuccess: () => {
            toast.success(t("visit.notesAutoSaved"))
          },
        },
      )
    },
    [visitId, updateNotesMutation, t],
  )

  const handleBlur = useCallback(() => {
    if (disabled) return
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      saveNotes(notes)
    }, 1000)
  }, [disabled, notes, saveNotes])

  useEffect(() => {
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current)
    }
  }, [])

  return (
    <VisitSection title={t("visit.examinationNotes")}>
      <Textarea
        className="min-h-[120px] resize-y"
        rows={4}
        value={notes}
        disabled={disabled}
        onChange={(e) => setNotes(e.target.value)}
        onBlur={handleBlur}
      />
    </VisitSection>
  )
}
