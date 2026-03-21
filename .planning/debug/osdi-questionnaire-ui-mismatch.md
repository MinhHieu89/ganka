---
status: diagnosed
trigger: "OSDI inline questionnaire UI in Record Session dialog doesn't match the public patient OSDI page (/osdi/:id). Also needs same change in /visits/:id."
created: 2026-03-19T00:00:00Z
updated: 2026-03-19T00:00:00Z
---

## Current Focus

hypothesis: Three separate OSDI questionnaire UIs exist with different visual designs; they should all use the public page's card-based UI
test: Side-by-side code comparison of all three implementations
expecting: Identify concrete UI divergences and a unification path
next_action: Report findings

## Symptoms

expected: The inline OSDI questionnaire in the Record Session dialog (SessionOsdiCapture) and the visit detail page (/visits/:id via OsdiSection) should look identical to the public patient OSDI page (/osdi/:id).
actual: Three different visual treatments exist for the same 12-question OSDI form.
errors: N/A (visual/UX mismatch, not a runtime error)
reproduction: Compare (1) /osdi/:token public page, (2) Record Session dialog inline tab, (3) /visits/:id dry eye section inline questionnaire.
started: Since each was implemented independently.

## Eliminated

(none)

## Evidence

- timestamp: 2026-03-19
  checked: Public OSDI page (frontend/src/app/routes/osdi/$token.tsx)
  found: |
    QUESTIONNAIRE UI (the "gold standard" the user wants):
    - Each question rendered inside a `<Card>` with `<CardHeader>` and `<CardContent>`
    - Question number shown via `t("osdi.question", { number })` in `<CardTitle>`
    - Vietnamese text shown as primary (`<p className="text-sm mt-1">`)
    - English text shown as secondary italic (`<p className="text-xs text-muted-foreground italic">`)
    - Answer options are BUTTONS (not radio buttons): `<button>` elements in a `flex flex-wrap gap-2` layout
    - Each button shows the numeric value (0-4) prominently + label text below
    - Selected state: `bg-primary text-primary-foreground border-primary`
    - N/A button for questions 6-12 with distinct muted styling
    - Live score preview: centered `bg-muted rounded-lg` block with score, severity badge, and progress counter
    - Submit button: full-width on mobile, centered
    - Score calculation: uses local `calculateOsdiScore()` function
    - Questions fetched from backend API (dynamic), not hardcoded
  implication: This is the desired UI. Card-based, button-based answers, bilingual, visually polished for patient use.

- timestamp: 2026-03-19
  checked: OsdiQuestionnaire component (frontend/src/features/clinical/components/OsdiQuestionnaire.tsx) - used by BOTH SessionOsdiCapture AND OsdiSection
  found: |
    QUESTIONNAIRE UI (the shared component used inline):
    - Questions rendered as plain `<div className="py-2 border-b">` rows (QuestionRow component) -- NO Card wrapper
    - Question text: single line with number prefix, Vietnamese primary, English in parentheses
    - Answer options are RADIO BUTTONS (`<RadioGroup>` + `<RadioGroupItem>`) with small text labels
    - N/A shown as a radio option for hasNA questions
    - Grouped by subscale with `<h4>` headers (Subscale A, B, C)
    - Live score preview: horizontal flex bar with score, badge, and answered count
    - Submit button: right-aligned (not centered/full-width)
    - Questions HARDCODED in the component (OSDI_QUESTIONS array), not fetched from API
    - Score calculation: exported `calculateOsdi()` function
  implication: This is visually very different from the public page. Compact radio-button form vs polished card-based button form.

- timestamp: 2026-03-19
  checked: SessionOsdiCapture (frontend/src/features/treatment/components/SessionOsdiCapture.tsx)
  found: |
    Uses `<OsdiQuestionnaire onSubmit={handleOsdiSubmit} />` directly.
    So the Record Session dialog's inline tab renders the OsdiQuestionnaire component (radio-button style).
    After submission, shows score with retake option.
  implication: Inherits all the OsdiQuestionnaire UI differences from the public page.

- timestamp: 2026-03-19
  checked: OsdiSection (frontend/src/features/clinical/components/OsdiSection.tsx) - visit detail page
  found: |
    Uses `<OsdiQuestionnaire onSubmit={handleQuestionnaireSubmit} isSubmitting={...} disabled={...} />` inside a Collapsible.
    Same OsdiQuestionnaire component, same radio-button style UI.
  implication: Also inherits all OsdiQuestionnaire UI differences from the public page.

- timestamp: 2026-03-19
  checked: All three implementations compared side-by-side
  found: |
    KEY UI DIFFERENCES (OsdiQuestionnaire vs Public Page):

    1. QUESTION LAYOUT
       - Public: Each question in a Card (CardHeader + CardContent) with visual separation
       - Inline: Plain div rows with border-bottom, no card styling

    2. ANSWER INPUT TYPE
       - Public: Styled `<button>` elements showing number + label, flex-wrapped, primary color when selected
       - Inline: `<RadioGroup>` with `<RadioGroupItem>` circles + small text labels

    3. BILINGUAL TEXT DISPLAY
       - Public: Vietnamese as a full paragraph line, English as separate italic paragraph below
       - Inline: Vietnamese inline with English in parentheses on same line

    4. QUESTION NUMBERING
       - Public: Uses i18n key `t("osdi.question", { number })` for localized "Question X" label in CardTitle
       - Inline: Just "X. question text" as plain numbered text

    5. SUBSCALE GROUPING
       - Public: No visible subscale headers (questions flow sequentially)
       - Inline: Explicit "Subscale A/B/C" headers splitting the questions

    6. LIVE SCORE DISPLAY
       - Public: Centered block with larger text, progress "X/12"
       - Inline: Horizontal bar, smaller, left-aligned label

    7. SUBMIT BUTTON
       - Public: Full-width on mobile, centered, with loading spinner
       - Inline: Right-aligned, smaller

    8. QUESTION DATA SOURCE
       - Public: Fetched from backend API (OsdiQuestionDto[]) -- allows future question edits without frontend deploy
       - Inline: Hardcoded OSDI_QUESTIONS array in the component

    9. N/A HANDLING
       - Public: Separate styled button with muted appearance
       - Inline: Additional radio button option
  implication: The divergence is significant. Every visual aspect differs.

## Resolution

root_cause: |
  The public OSDI page (`/osdi/$token.tsx`) and the shared `OsdiQuestionnaire` component were implemented independently with completely different UI approaches:

  - **Public page**: Card-based layout, button-based answer selection, bilingual text as separate lines, questions from API
  - **OsdiQuestionnaire component** (used by both SessionOsdiCapture and OsdiSection): Radio-button layout, compact rows, inline bilingual text, hardcoded questions

  The OsdiQuestionnaire component is used in two places:
  1. `SessionOsdiCapture.tsx` (Record Session dialog inline tab)
  2. `OsdiSection.tsx` (Visit detail page, inside DryEyeSection)

  Both inherit the radio-button style that differs from the public page.

fix: |
  **Recommended approach: Rewrite OsdiQuestionnaire to match the public page UI.**

  Since OsdiQuestionnaire is the single shared component used by both the Record Session dialog and the Visit detail page, updating it once will fix both locations. The changes needed:

  1. **Replace RadioGroup with styled buttons** -- match the public page's `<button>` elements with number + label, primary color when selected, flex-wrapped layout
  2. **Wrap each question in a Card** -- use `<Card>`, `<CardHeader>`, `<CardContent>` like the public page
  3. **Bilingual text as separate lines** -- Vietnamese as primary paragraph, English as secondary italic paragraph (not inline parenthetical)
  4. **Question numbering** -- use localized "Question X" label in CardTitle
  5. **Remove subscale headers** -- let questions flow sequentially like the public page (or make headers optional via prop)
  6. **Live score** -- match centered block layout with progress counter
  7. **Submit button** -- full-width on mobile, centered
  8. **N/A as a styled button** -- not a radio option, match the public page's muted button style

  Optionally, consider having OsdiQuestionnaire accept questions as a prop (to support API-fetched questions in the future), but for now the hardcoded array is acceptable since the OSDI standard has fixed questions.

  **Files to change:**
  - `frontend/src/features/clinical/components/OsdiQuestionnaire.tsx` -- rewrite UI to match public page style

  **Files that benefit automatically (no changes needed):**
  - `frontend/src/features/treatment/components/SessionOsdiCapture.tsx` -- uses OsdiQuestionnaire
  - `frontend/src/features/clinical/components/OsdiSection.tsx` -- uses OsdiQuestionnaire

  **Optional cleanup:**
  - After OsdiQuestionnaire matches the public page, the public page (`/osdi/$token.tsx`) could potentially import OsdiQuestionnaire instead of having its own inline implementation, fully deduplicating the code.

verification:
files_changed: []
