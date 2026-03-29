---
status: complete
phase: 15-implement-technician-dashboard-according-to-the-requirement-and-mockup
source: [15-01-SUMMARY.md, 15-02-SUMMARY.md, 15-03-SUMMARY.md, 15-04-SUMMARY.md, 15-05-SUMMARY.md]
started: 2026-03-29T12:00:00Z
updated: 2026-03-29T16:00:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Role-Based Dashboard Routing
expected: Log in as a user with the Technician role. The dashboard page should render the Technician Dashboard instead of the Receptionist Dashboard.
result: pass

### 2. KPI Cards Display
expected: The technician dashboard shows 4 KPI cards at the top: Waiting (amber), In Progress (blue), Completed (teal), Red Flag (red). Each card shows a count value and icon.
result: pass

### 3. Filter Toolbar Pills
expected: Below the KPI cards, filter toggle pills appear: All, Waiting, In Progress, Completed, Red Flag. Each pill shows a count. Active pill is black/white, inactive pills are transparent with border. Clicking a pill filters the table. Banner persists regardless of filter.
result: pass

### 4. Search Input
expected: A search input is present in the toolbar. Typing a patient name filters the queue table after a short debounce (~300ms). A clear button appears when text is entered.
result: pass

### 5. Queue Table Columns
expected: The queue table displays 9 columns: #, Patient Name, Date of Birth, Check-in Time, Wait Time, Reason, Visit Type (New/Follow-up), Status, Actions. Red-flagged patient names and reasons appear in red. Completed rows appear at reduced opacity.
result: pass

### 6. Accept Patient Action
expected: For a patient with "Waiting" status, click the Actions menu and select "Bắt đầu đo". The patient status changes to "In Progress" and is assigned to you. You are navigated to the Pre-Exam page.
result: pass

### 7. In-Progress Banner
expected: When you have an active patient (accepted/in-progress), a blue banner appears at the top of the dashboard showing the patient's info and a dark blue "Tiếp tục đo" button.
result: pass

### 8. Complete Exam Action
expected: For your in-progress patient, click Actions > Hoàn tất chuyển BS. The patient status changes to "Completed" and the visit advances to Doctor Exam stage. Success toast appears.
result: pass

### 9. Return to Queue Action
expected: For your in-progress patient, click Actions > Trả lại hàng đợi. A confirmation dialog appears with the patient's name. Confirming returns the patient to "Waiting" status and clears your assignment. Success toast appears.
result: pass

### 10. Red Flag Action
expected: For your in-progress patient, click Actions > Chuyển BS ngay. A dialog appears with 4 reason options (select dropdown). Selecting "Khác" shows a custom text input. Submitting flags the patient and advances the visit to Doctor Exam. Success toast appears.
result: pass

### 11. Patient Results Panel
expected: Click "Xem kết quả" from the action menu for an examined patient. A slide-over panel opens from the right showing patient result details.
result: pass

### 12. Pre-Exam Stub Page
expected: Navigate to /technician/pre-exam (e.g., via "Tiếp tục đo" button). A stub page appears with a stethoscope icon and a "Quay lại Dashboard" link that returns you to the technician dashboard.
result: pass

### 13. Wait Time Urgency Indicator
expected: Patients waiting 25 minutes or more show their wait time in red/urgent color in the queue table. Wait times increment automatically every 60 seconds without page refresh.
result: pass

### 14. Empty State
expected: When no patients are in the queue (or all filtered out), a helpful empty state message is displayed instead of an empty table.
result: pass

### 15. Vietnamese i18n
expected: All dashboard labels, buttons, filter pills, dialog text, and status badges display in Vietnamese when the app language is set to Vietnamese.
result: pass

## Summary

total: 15
passed: 15
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

[none yet]
