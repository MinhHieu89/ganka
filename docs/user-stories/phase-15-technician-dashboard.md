# User Stories: Dashboard K\u1ef9 Thu\u1eadt Vi\u00ean (Technician Pre-Exam Dashboard)

**Phase:** 15 - Implement Technician Dashboard
**Ng\u00e0y t\u1ea1o:** 2026-03-29
**Y\u00eau c\u1ea7u li\u00ean quan:** TECH-13, TECH-14, DOC-01
**S\u1ed1 l\u01b0\u1ee3ng user stories:** 9

---

## TECH-13: Dashboard K\u1ef9 Thu\u1eadt Vi\u00ean v\u00e0 Qu\u1ea3n L\u00fd H\u00e0ng \u0110\u1ee3i

### US-TCH-001: Xem dashboard k\u1ef9 thu\u1eadt vi\u00ean

**L\u00e0 m\u1ed9t** K\u1ef9 thu\u1eadt vi\u00ean,
**T\u00f4i mu\u1ed1n** xem dashboard chuy\u00ean bi\u1ec7t khi \u0111\u0103ng nh\u1eadp,
**\u0110\u1ec3** qu\u1ea3n l\u00fd h\u00e0ng \u0111\u1ee3i b\u1ec7nh nh\u00e2n ch\u1edd \u0111o Pre-Exam.

**Y\u00eau c\u1ea7u li\u00ean quan:** TECH-13
**Quy\u1ebft \u0111\u1ecbnh li\u00ean quan:** D-16, D-07

#### Ti\u00eau ch\u00ed ch\u1ea5p nh\u1eadn

**Lu\u1ed3ng ch\u00ednh (Happy Path):**
1. K\u1ef9 thu\u1eadt vi\u00ean \u0111\u0103ng nh\u1eadp v\u00e0o h\u1ec7 th\u1ed1ng -> H\u1ec7 th\u1ed1ng ph\u00e1t hi\u1ec7n role "Technician" v\u00e0 hi\u1ec3n th\u1ecb `TechnicianDashboard` thay v\u00ec DefaultDashboard
2. Dashboard hi\u1ec3n th\u1ecb 4 KPI cards (Ch\u1edd kh\u00e1m, \u0110ang \u0111o, Ho\u00e0n t\u1ea5t Pre-Exam, Red flag)
3. B\u1ea3ng b\u1ec7nh nh\u00e2n hi\u1ec3n th\u1ecb 9 c\u1ed9t: #, H\u1ecd t\u00ean, Sinh, Check-in, Ch\u1edd, L\u00fd do kh\u00e1m, Lo\u1ea1i, Tr\u1ea1ng th\u00e1i, H\u00e0nh \u0111\u1ed9ng
4. Thanh c\u00f4ng c\u1ee5 hi\u1ec3n th\u1ecb filter pills v\u00e0 \u00f4 t\u00ecm ki\u1ebfm

**Tr\u01b0\u1eddng h\u1ee3p ngo\u1ea1i l\u1ec7:**
1. User kh\u00f4ng c\u00f3 role Technician -> Hi\u1ec3n th\u1ecb DefaultDashboard ho\u1eb7c ReceptionistDashboard t\u00f9y role
2. Kh\u00f4ng c\u00f3 b\u1ec7nh nh\u00e2n n\u00e0o -> Hi\u1ec3n th\u1ecb tr\u1ea1ng th\u00e1i r\u1ed7ng: "Kh\u00f4ng c\u00f3 b\u1ec7nh nh\u00e2n ch\u1edd kh\u00e1m"

**Tr\u01b0\u1eddng h\u1ee3p l\u1ed7i:**
1. L\u1ed7i t\u1ea3i d\u1eef li\u1ec7u -> Hi\u1ec3n th\u1ecb toast l\u1ed7i: "Kh\u00f4ng th\u1ec3 t\u1ea3i d\u1eef li\u1ec7u. Vui l\u00f2ng th\u1eed l\u1ea1i"

#### Ghi ch\u00fa k\u1ef9 thu\u1eadt
- Component: `TechnicianDashboard.tsx` trong `features/technician/components/`
- Route: `/dashboard` v\u1edbi role-based rendering (D-16)
- API: GET /api/clinical/technician/dashboard

---

### US-TCH-002: Xem th\u1ed1ng k\u00ea KPI

**L\u00e0 m\u1ed9t** K\u1ef9 thu\u1eadt vi\u00ean,
**T\u00f4i mu\u1ed1n** xem s\u1ed1 li\u1ec7u th\u1ed1ng k\u00ea (ch\u1edd kh\u00e1m, \u0111ang \u0111o, ho\u00e0n t\u1ea5t, red flag),
**\u0110\u1ec3** n\u1eafm b\u1eaft t\u00ecnh h\u00ecnh h\u00e0ng \u0111\u1ee3i nhanh ch\u00f3ng.

**Y\u00eau c\u1ea7u li\u00ean quan:** TECH-13
**Quy\u1ebft \u0111\u1ecbnh li\u00ean quan:** D-09, D-17

#### Ti\u00eau ch\u00ed ch\u1ea5p nh\u1eadn

**Lu\u1ed3ng ch\u00ednh (Happy Path):**
1. 4 KPI cards hi\u1ec3n th\u1ecb tr\u00ean \u0111\u1ea7u dashboard v\u1edbi gi\u00e1 tr\u1ecb m\u00e0u s\u1eafc: Ch\u1edd kh\u00e1m (amber #BA7517), \u0110ang \u0111o (blue #185FA5), Ho\u00e0n t\u1ea5t (teal #0F6E56), Red flag (red #A32D2D)
2. KPI t\u1ef1 \u0111\u1ed9ng c\u1eadp nh\u1eadt m\u1ed7i 30 gi\u00e2y (polling interval)
3. Ch\u1ec9 hi\u1ec3n th\u1ecb d\u1eef li\u1ec7u h\u00f4m nay

**Tr\u01b0\u1eddng h\u1ee3p ngo\u1ea1i l\u1ec7:**
1. Kh\u00f4ng c\u00f3 d\u1eef li\u1ec7u -> T\u1ea5t c\u1ea3 KPI hi\u1ec3n th\u1ecb 0

**Tr\u01b0\u1eddng h\u1ee3p l\u1ed7i:**
1. L\u1ed7i t\u1ea3i KPI -> Hi\u1ec3n th\u1ecb skeleton loading

#### Ghi ch\u00fa k\u1ef9 thu\u1eadt
- Component: `TechnicianKpiCards.tsx`
- API: GET /api/clinical/technician/kpi
- Polling: `refetchInterval: 30_000`

---

### US-TCH-003: Nh\u1eadn b\u1ec7nh nh\u00e2n

**L\u00e0 m\u1ed9t** K\u1ef9 thu\u1eadt vi\u00ean,
**T\u00f4i mu\u1ed1n** nh\u1eadn b\u1ec7nh nh\u00e2n t\u1eeb h\u00e0ng \u0111\u1ee3i,
**\u0110\u1ec3** b\u1eaft \u0111\u1ea7u \u0111o Pre-Exam cho b\u1ec7nh nh\u00e2n \u0111\u00f3.

**Y\u00eau c\u1ea7u li\u00ean quan:** TECH-13
**Quy\u1ebft \u0111\u1ecbnh li\u00ean quan:** D-15, D-11

#### Ti\u00eau ch\u00ed ch\u1ea5p nh\u1eadn

**Lu\u1ed3ng ch\u00ednh (Happy Path):**
1. K\u1ef9 thu\u1eadt vi\u00ean click menu 3 ch\u1ea5m tr\u00ean d\u00f2ng b\u1ec7nh nh\u00e2n "Ch\u1edd kh\u00e1m" -> Ch\u1ecdn "Nh\u1eadn BN"
2. H\u1ec7 th\u1ed1ng g\u1ecdi API accept -> Th\u00e0nh c\u00f4ng -> Chuy\u1ec3n \u0111\u1ebfn trang Pre-Exam
3. Tr\u1ea1ng th\u00e1i b\u1ec7nh nh\u00e2n chuy\u1ec3n t\u1eeb "Ch\u1edd kh\u00e1m" sang "\u0110ang \u0111o"

**Tr\u01b0\u1eddng h\u1ee3p ngo\u1ea1i l\u1ec7:**
1. \u0110ang \u0111o b\u1ec7nh nh\u00e2n kh\u00e1c -> Hi\u1ec3n dialog x\u00e1c nh\u1eadn t\u1ea1m d\u1eebng: "B\u1ea1n \u0111ang \u0111o BN {t\u00ean}. Mu\u1ed1n t\u1ea1m d\u1eebng v\u00e0 nh\u1eadn BN m\u1edbi?"
2. X\u00e1c nh\u1eadn t\u1ea1m d\u1eebng -> H\u1ec7 th\u1ed1ng accept BN m\u1edbi, BN c\u0169 tr\u1edf l\u1ea1i h\u00e0ng \u0111\u1ee3i

**Tr\u01b0\u1eddng h\u1ee3p l\u1ed7i:**
1. BN \u0111\u00e3 \u0111\u01b0\u1ee3c nh\u1eadn b\u1edfi k\u1ef9 thu\u1eadt vi\u00ean kh\u00e1c (race condition) -> Hi\u1ec3n toast l\u1ed7i: "BN \u0111\u00e3 \u0111\u01b0\u1ee3c nh\u1eadn b\u1edfi {t\u00ean}"
2. L\u1ed7i m\u1ea1ng -> Hi\u1ec3n toast l\u1ed7i chung

#### Ghi ch\u00fa k\u1ef9 thu\u1eadt
- Component: `TechnicianActionMenu.tsx`, `PausePatientDialog.tsx`
- API: POST /api/clinical/technician/orders/{orderId}/accept
- Optimistic validation: backend ki\u1ec3m tra TechnicianId != null (D-15)

---

### US-TCH-004: Ho\u00e0n t\u1ea5t chuy\u1ec3n b\u00e1c s\u0129

**L\u00e0 m\u1ed9t** K\u1ef9 thu\u1eadt vi\u00ean,
**T\u00f4i mu\u1ed1n** \u0111\u00e1nh d\u1ea5u ho\u00e0n t\u1ea5t Pre-Exam v\u00e0 chuy\u1ec3n b\u1ec7nh nh\u00e2n sang b\u00e1c s\u0129,
**\u0110\u1ec3** b\u00e1c s\u0129 c\u00f3 th\u1ec3 kh\u00e1m b\u1ec7nh nh\u00e2n \u0111\u00f3.

**Y\u00eau c\u1ea7u li\u00ean quan:** TECH-13
**Quy\u1ebft \u0111\u1ecbnh li\u00ean quan:** D-08, D-01

#### Ti\u00eau ch\u00ed ch\u1ea5p nh\u1eadn

**Lu\u1ed3ng ch\u00ednh (Happy Path):**
1. K\u1ef9 thu\u1eadt vi\u00ean click menu 3 ch\u1ea5m tr\u00ean d\u00f2ng BN "\u0110ang \u0111o" -> Ch\u1ecdn "Ho\u00e0n t\u1ea5t chuy\u1ec3n BS"
2. H\u1ec7 th\u1ed1ng g\u1ecdi API complete -> Th\u00e0nh c\u00f4ng
3. Hi\u1ec3n toast: "\u0110\u00e3 chuy\u1ec3n BN {t\u00ean} sang b\u00e1c s\u0129"
4. BN chuy\u1ec3n sang tr\u1ea1ng th\u00e1i "Ho\u00e0n t\u1ea5t", visit advance sang DoctorExam

**Tr\u01b0\u1eddng h\u1ee3p l\u1ed7i:**
1. L\u1ed7i m\u1ea1ng -> Hi\u1ec3n toast l\u1ed7i chung

#### Ghi ch\u00fa k\u1ef9 thu\u1eadt
- API: POST /api/clinical/technician/orders/{orderId}/complete
- Backend: CompleteTechnicianOrder handler set CompletedAt, advance Visit stage

---

### US-TCH-005: Tr\u1ea3 l\u1ea1i h\u00e0ng \u0111\u1ee3i

**L\u00e0 m\u1ed9t** K\u1ef9 thu\u1eadt vi\u00ean,
**T\u00f4i mu\u1ed1n** tr\u1ea3 b\u1ec7nh nh\u00e2n v\u1ec1 h\u00e0ng \u0111\u1ee3i,
**\u0110\u1ec3** k\u1ef9 thu\u1eadt vi\u00ean kh\u00e1c c\u00f3 th\u1ec3 nh\u1eadn b\u1ec7nh nh\u00e2n \u0111\u00f3.

**Y\u00eau c\u1ea7u li\u00ean quan:** TECH-13
**Quy\u1ebft \u0111\u1ecbnh li\u00ean quan:** D-08

#### Ti\u00eau ch\u00ed ch\u1ea5p nh\u1eadn

**Lu\u1ed3ng ch\u00ednh (Happy Path):**
1. K\u1ef9 thu\u1eadt vi\u00ean click menu 3 ch\u1ea5m -> Ch\u1ecdn "Tr\u1ea3 l\u1ea1i h\u00e0ng \u0111\u1ee3i"
2. Hi\u1ec3n dialog x\u00e1c nh\u1eadn: "Tr\u1ea3 BN {t\u00ean} v\u1ec1 h\u00e0ng \u0111\u1ee3i? D\u1eef li\u1ec7u \u0111\u00e3 nh\u1eadp s\u1ebd \u0111\u01b0\u1ee3c gi\u1eef nguy\u00ean."
3. X\u00e1c nh\u1eadn -> API success -> Toast: "\u0110\u00e3 tr\u1ea3 BN {t\u00ean} v\u1ec1 h\u00e0ng \u0111\u1ee3i"
4. BN tr\u1edf l\u1ea1i tr\u1ea1ng th\u00e1i "Ch\u1edd kh\u00e1m"

**Tr\u01b0\u1eddng h\u1ee3p ngo\u1ea1i l\u1ec7:**
1. H\u1ee7y dialog -> Kh\u00f4ng c\u00f3 thay \u0111\u1ed5i

**Tr\u01b0\u1eddng h\u1ee3p l\u1ed7i:**
1. L\u1ed7i m\u1ea1ng -> Hi\u1ec3n toast l\u1ed7i chung

#### Ghi ch\u00fa k\u1ef9 thu\u1eadt
- Component: `ReturnToQueueDialog.tsx`
- API: POST /api/clinical/technician/orders/{orderId}/return-to-queue
- Backend: ReturnToQueue handler clear TechnicianId, StartedAt

---

### US-TCH-006: Chuy\u1ec3n BS ngay (Red Flag)

**L\u00e0 m\u1ed9t** K\u1ef9 thu\u1eadt vi\u00ean,
**T\u00f4i mu\u1ed1n** chuy\u1ec3n b\u1ec7nh nh\u00e2n sang b\u00e1c s\u0129 ngay l\u1eadp t\u1ee9c v\u1edbi l\u00fd do kh\u1ea9n c\u1ea5p,
**\u0110\u1ec3** b\u00e1c s\u0129 kh\u00e1m ngay cho b\u1ec7nh nh\u00e2n c\u00f3 d\u1ea5u hi\u1ec7u nguy hi\u1ec3m.

**Y\u00eau c\u1ea7u li\u00ean quan:** TECH-13
**Quy\u1ebft \u0111\u1ecbnh li\u00ean quan:** D-08, D-01

#### Ti\u00eau ch\u00ed ch\u1ea5p nh\u1eadn

**Lu\u1ed3ng ch\u00ednh (Happy Path):**
1. K\u1ef9 thu\u1eadt vi\u00ean click menu 3 ch\u1ea5m -> Ch\u1ecdn "Chuy\u1ec3n BS ngay"
2. Hi\u1ec3n dialog v\u1edbi danh s\u00e1ch l\u00fd do: \u0110au m\u1eaft nhi\u1ec1u, Gi\u1ea3m th\u1ecb l\u1ef1c \u0111\u1ed9t ng\u1ed9t, Tri\u1ec7u ch\u1ee9ng l\u1ec7ch 1 b\u00ean r\u00f5, Kh\u00e1c
3. Ch\u1ecdn l\u00fd do (b\u1eaft bu\u1ed9c) -> Click "Chuy\u1ec3n BS ngay"
4. Toast: "\u0110\u00e3 chuy\u1ec3n BN {t\u00ean} sang BS (Red flag)"
5. BN \u0111\u01b0\u1ee3c \u0111\u00e1nh d\u1ea5u Red Flag v\u00e0 chuy\u1ec3n sang b\u00e1c s\u0129

**Tr\u01b0\u1eddng h\u1ee3p ngo\u1ea1i l\u1ec7:**
1. Ch\u1ecdn "Kh\u00e1c" -> Ph\u1ea3i nh\u1eadp l\u00fd do c\u1ee5 th\u1ec3 (b\u1eaft bu\u1ed9c)
2. Kh\u00f4ng ch\u1ecdn l\u00fd do -> Hi\u1ec3n validation: "Vui l\u00f2ng ch\u1ecdn l\u00fd do"

**Tr\u01b0\u1eddng h\u1ee3p l\u1ed7i:**
1. L\u1ed7i m\u1ea1ng -> Hi\u1ec3n toast l\u1ed7i chung

#### Ghi ch\u00fa k\u1ef9 thu\u1eadt
- Component: `RedFlagDialog.tsx`
- API: POST /api/clinical/technician/orders/{orderId}/red-flag
- Body: `{ reason: string }`
- Backend: RedFlagTechnicianOrder handler set IsRedFlag, RedFlagReason, RedFlaggedAt, advance Visit stage

---

### US-TCH-007: Xem k\u1ebft qu\u1ea3 b\u1ec7nh nh\u00e2n

**L\u00e0 m\u1ed9t** K\u1ef9 thu\u1eadt vi\u00ean,
**T\u00f4i mu\u1ed1n** xem th\u00f4ng tin v\u00e0 k\u1ebft qu\u1ea3 c\u1ee7a b\u1ec7nh nh\u00e2n,
**\u0110\u1ec3** ki\u1ec3m tra d\u1eef li\u1ec7u tr\u01b0\u1edbc khi chuy\u1ec3n sang b\u00e1c s\u0129.

**Y\u00eau c\u1ea7u li\u00ean quan:** TECH-14
**Quy\u1ebft \u0111\u1ecbnh li\u00ean quan:** D-13

#### Ti\u00eau ch\u00ed ch\u1ea5p nh\u1eadn

**Lu\u1ed3ng ch\u00ednh (Happy Path):**
1. K\u1ef9 thu\u1eadt vi\u00ean click menu 3 ch\u1ea5m -> Ch\u1ecdn "Xem k\u1ebft qu\u1ea3"
2. Panel tr\u01b0\u1ee3t t\u1eeb b\u00ean ph\u1ea3i (slide-over), b\u1ea3ng h\u00e0ng \u0111\u1ee3i v\u1eabn hi\u1ec3n th\u1ecb ph\u00eda sau overlay
3. Panel hi\u1ec3n th\u1ecb:
   - Th\u00f4ng tin c\u00e1 nh\u00e2n: T\u00ean b\u1ec7nh nh\u00e2n, m\u00e3 BN, n\u0103m sinh
   - L\u1ecbch s\u1eed kh\u00e1m: Ng\u00e0y kh\u00e1m, l\u00fd do, lo\u1ea1i kh\u00e1m
   - D\u1eef li\u1ec7u Pre-Exam: Hi\u1ec3n th\u1ecb n\u1ebfu c\u00f3, ho\u1eb7c "Ch\u01b0a c\u00f3 d\u1eef li\u1ec7u Pre-Exam"
4. Nh\u1ea5n X ho\u1eb7c Escape \u0111\u1ec3 \u0111\u00f3ng panel

**Tr\u01b0\u1eddng h\u1ee3p ngo\u1ea1i l\u1ec7:**
1. BN ch\u01b0a c\u00f3 d\u1eef li\u1ec7u Pre-Exam -> Hi\u1ec3n placeholder "Ch\u01b0a c\u00f3 d\u1eef li\u1ec7u Pre-Exam"

#### Ghi ch\u00fa k\u1ef9 thu\u1eadt
- Component: `PatientResultsPanel.tsx` s\u1eed d\u1ee5ng shadcn `Sheet` v\u1edbi `side="right"`
- D\u1eef li\u1ec7u l\u1ea5y t\u1eeb `TechnicianDashboardRow` (kh\u00f4ng c\u1ea7n API ri\u00eang)
- Panel width: `sm:max-w-lg`

---

### US-TCH-008: L\u1ecdc v\u00e0 t\u00ecm ki\u1ebfm b\u1ec7nh nh\u00e2n

**L\u00e0 m\u1ed9t** K\u1ef9 thu\u1eadt vi\u00ean,
**T\u00f4i mu\u1ed1n** l\u1ecdc theo tr\u1ea1ng th\u00e1i v\u00e0 t\u00ecm ki\u1ebfm theo t\u00ean/S\u0110T,
**\u0110\u1ec3** t\u00ecm nhanh b\u1ec7nh nh\u00e2n c\u1ea7n \u0111o.

**Y\u00eau c\u1ea7u li\u00ean quan:** TECH-13
**Quy\u1ebft \u0111\u1ecbnh li\u00ean quan:** D-07, D-08

#### Ti\u00eau ch\u00ed ch\u1ea5p nh\u1eadn

**Lu\u1ed3ng ch\u00ednh (Happy Path):**
1. Filter pills hi\u1ec3n th\u1ecb: T\u1ea5t c\u1ea3, Ch\u1edd kh\u00e1m, \u0110ang \u0111o, Ho\u00e0n t\u1ea5t, Red flag - m\u1ed7i pill k\u00e8m s\u1ed1 \u0111\u1ebfm
2. Click pill -> B\u1ea3ng l\u1ecdc theo tr\u1ea1ng th\u00e1i t\u01b0\u01a1ng \u1ee9ng
3. Nh\u1eadp t\u00ecm ki\u1ebfm -> B\u1ea3ng l\u1ecdc theo t\u00ean ho\u1eb7c s\u1ed1 \u0111i\u1ec7n tho\u1ea1i
4. T\u00ecm ki\u1ebfm c\u00f3 debounce 300ms

**Tr\u01b0\u1eddng h\u1ee3p ngo\u1ea1i l\u1ec7:**
1. Kh\u00f4ng t\u00ecm th\u1ea5y k\u1ebft qu\u1ea3 -> Hi\u1ec3n tr\u1ea1ng th\u00e1i r\u1ed7ng
2. X\u00f3a \u00f4 t\u00ecm ki\u1ebfm -> Tr\u1edf v\u1ec1 danh s\u00e1ch \u0111\u1ea7y \u0111\u1ee7

#### Ghi ch\u00fa k\u1ef9 thu\u1eadt
- Component: `TechnicianToolbar.tsx`
- Filter truy\u1ec1n v\u00e0o API nh\u01b0 query params: `?status=waiting&search=Nguy\u1ec5n`
- Debounce: 300ms tr\u00ean search input

---

## TECH-14: T\u1ef1 \u0110\u1ed9ng T\u1ea1o TechnicianOrder

### US-TCH-009: T\u1ef1 \u0111\u1ed9ng t\u1ea1o TechnicianOrder khi BN v\u00e0o PreExam

**L\u00e0 m\u1ed9t** H\u1ec7 th\u1ed1ng,
**Khi** b\u1ec7nh nh\u00e2n \u0111\u01b0\u1ee3c chuy\u1ec3n sang giai \u0111o\u1ea1n Pre-Exam,
**H\u1ec7 th\u1ed1ng** t\u1ef1 \u0111\u1ed9ng t\u1ea1o TechnicianOrder(\u0110o kh\u00fac x\u1ea1/Th\u1ecb l\u1ef1c).

**Y\u00eau c\u1ea7u li\u00ean quan:** TECH-14
**Quy\u1ebft \u0111\u1ecbnh li\u00ean quan:** D-04, D-01, D-02

#### Ti\u00eau ch\u00ed ch\u1ea5p nh\u1eadn

**Lu\u1ed3ng ch\u00ednh (Happy Path):**
1. Ti\u1ebfp t\u00e2n advance Visit sang giai \u0111o\u1ea1n PreExam (AdvanceWorkflowStage)
2. H\u1ec7 th\u1ed1ng t\u1ef1 \u0111\u1ed9ng t\u1ea1o `TechnicianOrder` v\u1edbi `OrderType=PreExam`
3. B\u1ec7nh nh\u00e2n xu\u1ea5t hi\u1ec7n tr\u00ean dashboard k\u1ef9 thu\u1eadt vi\u00ean v\u1edbi tr\u1ea1ng th\u00e1i "Ch\u1edd kh\u00e1m"

**Tr\u01b0\u1eddng h\u1ee3p ngo\u1ea1i l\u1ec7:**
1. Visit \u0111\u00e3 c\u00f3 TechnicianOrder(PreExam) -> Kh\u00f4ng t\u1ea1o tr\u00f9ng l\u1eb7p

**Tr\u01b0\u1eddng h\u1ee3p l\u1ed7i:**
1. L\u1ed7i database -> Transaction rollback, kh\u00f4ng advance Visit

#### Ghi ch\u00fa k\u1ef9 thu\u1eadt
- Entity: `TechnicianOrder.cs` trong Clinical.Domain
- Hook: `AdvanceWorkflowStage` handler t\u1ea1o TechnicianOrder khi target stage = PreExam
- Idempotent: ki\u1ec3m tra t\u1ed3n t\u1ea1i tr\u01b0\u1edbc khi t\u1ea1o

---

## B\u1ea3ng Truy Xu\u1ea5t (Traceability)

| User Story | Y\u00eau c\u1ea7u | Quy\u1ebft \u0111\u1ecbnh | Component ch\u00ednh |
|-----------|-----------|----------------|-----------------|
| US-TCH-001 | TECH-13 | D-16, D-07 | TechnicianDashboard.tsx |
| US-TCH-002 | TECH-13 | D-09, D-17 | TechnicianKpiCards.tsx |
| US-TCH-003 | TECH-13 | D-15, D-11 | TechnicianActionMenu.tsx, PausePatientDialog.tsx |
| US-TCH-004 | TECH-13 | D-08, D-01 | TechnicianActionMenu.tsx |
| US-TCH-005 | TECH-13 | D-08 | ReturnToQueueDialog.tsx |
| US-TCH-006 | TECH-13 | D-08, D-01 | RedFlagDialog.tsx |
| US-TCH-007 | TECH-14 | D-13 | PatientResultsPanel.tsx |
| US-TCH-008 | TECH-13 | D-07, D-08 | TechnicianToolbar.tsx |
| US-TCH-009 | TECH-14 | D-04, D-01, D-02 | TechnicianOrder.cs, AdvanceWorkflowStage.cs |

### Quy\u1ebft \u0111\u1ecbnh tham chi\u1ebfu

| Quy\u1ebft \u0111\u1ecbnh | M\u00f4 t\u1ea3 |
|----------|---------|
| D-01 | TechnicianOrder entity l\u01b0u tr\u1eef t\u1ea5t c\u1ea3 th\u00f4ng tin pre-exam tracking |
| D-02 | TechnicianOrderType enum: PreExam, AdditionalExam |
| D-03 | Enum l\u01b0u d\u1ea1ng string trong DB |
| D-04 | T\u1ef1 \u0111\u1ed9ng t\u1ea1o TechnicianOrder khi Visit advance sang PreExam |
| D-05 | C\u00e1c tr\u01b0\u1eddng c\u1ee7a TechnicianOrder entity |
| D-06 | \u0110\u1ed5i t\u00ean WorkflowStage.RefractionVA th\u00e0nh PreExam |
| D-07 | Dashboard query t\u1eeb b\u1ea3ng TechnicianOrder |
| D-08 | Tr\u1ea1ng th\u00e1i \u0111\u01b0\u1ee3c derive t\u1eeb c\u00e1c field c\u1ee7a TechnicianOrder |
| D-09 | KPI counts query c\u00f9ng b\u1ea3ng TechnicianOrder |
| D-10 | C\u1ed9t Lo\u1ea1i: M\u1edbi/T\u00e1i kh\u00e1m/\u0110o b\u1ed5 sung |
| D-11 | Stub navigation cho Pre-Exam page |
| D-12 | \u0110o b\u1ed5 sung deferred sang phase sau |
| D-13 | Xem k\u1ebft qu\u1ea3 m\u1edf slide-over panel t\u1eeb b\u00ean ph\u1ea3i |
| D-14 | Endpoints n\u1eb1m trong Clinical module |
| D-15 | 1 BN/k\u1ef9 thu\u1eadt vi\u00ean, optimistic validation |
| D-16 | C\u00f9ng route /dashboard, role-based rendering |
| D-17 | Polling-based updates (kh\u00f4ng WebSocket) |

---

*Phase: 15 - Implement Technician Dashboard*
*T\u00e0i li\u1ec7u t\u1ea1o: 2026-03-29*
