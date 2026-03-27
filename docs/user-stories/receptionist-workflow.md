# User Stories: Quy Trinh Le Tan (Receptionist Workflow)

**Phase:** 14 - Implement Receptionist Role Flow
**Ngay tao:** 2026-03-28
**Yeu cau lien quan:** RCP-01, RCP-02, RCP-03, RCP-04, RCP-05, RCP-06, RCP-07
**So luong user stories:** 16

---

## Dashboard Le Tan (SCR-002a)

### US-RCP-001: Le tan xem dashboard voi KPI va danh sach benh nhan hom nay

**La mot** le tan,
**Toi muon** xem dashboard voi 4 o KPI (Lich hen hom nay, Cho kham, Dang kham, Hoan thanh) va bang danh sach benh nhan theo ngay,
**De** nam bat toan bo tinh hinh hang doi benh nhan va thuc hien cac thao tac tiep nhan, check-in, dat lich ngay tren mot man hinh duy nhat.

**Yeu cau lien quan:** RCP-01
**Quyet dinh lien quan:** D-01, D-02, D-03, D-04

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan dang nhap he thong voi role Receptionist -> He thong redirect den `/dashboard` va render giao dien rieng cho le tan (khac voi giao dien bac si/ky thuat vien)
2. Phan KPI hien thi 4 o so lieu: Lich hen hom nay (tim), Cho kham (amber), Dang kham (xanh duong), Hoan thanh (xanh la)
3. O "Lich hen hom nay" hien thi tong so lich hen va sub-text "X chua den"
4. Bang danh sach hien thi cac cot: STT, Ho ten, Nam sinh, Gio hen, Nguon (Hen/Walk-in), Ly do kham, Trang thai, Thao tac
5. Bang sap xep mac dinh theo gio hen (som nhat len truoc)
6. KPI cap nhat tu dong moi 30 giay bang polling
7. Bang danh sach cap nhat tu dong moi 15 giay

**Truong hop ngoai le:**
1. Khong co benh nhan nao trong ngay -> Bang hien thi trang thai rong, KPI hien thi so 0
2. Le tan click vao 1 o KPI -> Bang tu dong loc theo trang thai tuong ung
3. Benh nhan walk-in khong co gio hen -> Cot "Gio hen" hien thi "—"

**Truong hop loi:**
1. Loi ket noi mang -> He thong hien thi canh bao va giu lai du lieu cu tren man hinh
2. Polling that bai -> He thong retry lan ke tiep, khong mat du lieu hien thi

#### Ghi chu ky thuat
- Route: `/dashboard` voi role-based rendering (Receptionist vs Clinical)
- Polling: 30s cho KPI, 15s cho bang danh sach
- API: GET /api/scheduling/receptionist-dashboard

---

### US-RCP-002: Le tan loc benh nhan theo trang thai

**La mot** le tan,
**Toi muon** loc danh sach benh nhan theo 4 trang thai (Chua den, Cho kham, Dang kham, Hoan thanh) hoac xem tat ca,
**De** tap trung vao nhom benh nhan can xu ly tiep theo (vi du: chi xem nhung benh nhan "Chua den" de goi nhac).

**Yeu cau lien quan:** RCP-01
**Quyet dinh lien quan:** D-03

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan nhin thay thanh filter dang pill buttons: Tat ca, Chua den, Cho kham, Dang kham, Xong
2. Mac dinh chon "Tat ca" -> Bang hien thi toan bo benh nhan trong ngay
3. Le tan click "Chua den" -> Bang chi hien thi benh nhan co trang thai "Chua den" (tim)
4. Le tan click "Cho kham" -> Bang chi hien thi benh nhan co trang thai "Cho kham" (amber)
5. So luong benh nhan hien thi cap nhat ngay khi chuyen filter
6. Click vao o KPI cung loc bang tuong tu nhu click pill button

**Truong hop ngoai le:**
1. Khong co benh nhan nao o trang thai da chon -> Bang hien thi trang thai rong: "Khong co benh nhan"
2. Le tan click nhanh nhieu filter lien tuc -> He thong cancel request truoc va chi thuc hien request moi nhat

**Truong hop loi:**
1. Loi loc du lieu -> He thong hien thi toast loi va giu lai filter truoc do

---

### US-RCP-003: Le tan tim kiem benh nhan bang so dien thoai hoac ten

**La mot** le tan,
**Toi muon** tim kiem benh nhan theo so dien thoai hoac ten ngay tren dashboard,
**De** nhanh chong tra cuu ho so benh nhan cu khi ho den kham hoac goi dien.

**Yeu cau lien quan:** RCP-01
**Quyet dinh lien quan:** D-01

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan nhap so dien thoai hoac ten benh nhan vao o tim kiem tren action bar
2. Sau 2 ky tu, he thong bat dau autocomplete real-time, hien thi danh sach ket qua phu hop
3. Ket qua hien thi: ho ten, nam sinh, ma benh nhan, so dien thoai
4. Le tan click vao 1 ket qua -> Mo popup tao luot kham walk-in (SCR-005 Flow B) cho benh nhan cu

**Truong hop ngoai le:**
1. Khong tim thay benh nhan -> Hien thi "Khong tim thay benh nhan. Bam 'Tiep nhan BN moi' de tao ho so."
2. Nhieu benh nhan trung ten -> Hien thi danh sach tat ca, phan biet bang nam sinh va so dien thoai
3. Le tan xoa o tim kiem -> Danh sach tro ve trang thai binh thuong (hien thi benh nhan trong ngay)

**Truong hop loi:**
1. Loi search API -> He thong hien thi toast loi va giu o search co the nhap lai

#### Ghi chu ky thuat
- Reuse `usePatientSearch` hook (debounced, autocomplete)
- Search toan bo database benh nhan, khong chi benh nhan hom nay

---

## Tiep Nhan Benh Nhan Moi (SCR-003)

### US-RCP-004: Le tan tiep nhan benh nhan moi voi form nhap lieu 4 phan

**La mot** le tan,
**Toi muon** tao ho so benh nhan moi qua form 4 phan (Thong tin ca nhan, Thong tin kham, Tien su benh, Lifestyle),
**De** thu thap du thong tin can thiet truoc khi benh nhan vao hang doi kham.

**Yeu cau lien quan:** RCP-02
**Quyet dinh lien quan:** D-07, D-08

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan bam nut "Tiep nhan BN moi" tren dashboard -> He thong mo form voi 4 section, tat ca expand mac dinh
2. Section 1 (Thong tin ca nhan): ho ten (bat buoc), gioi tinh (bat buoc), ngay sinh (bat buoc), so dien thoai (bat buoc), email, dia chi, nghe nghiep
3. Section 2 (Thong tin kham): ly do den kham (bat buoc) voi dem ky tu (toi da 500)
4. Section 3 (Tien su benh): tien su benh mat, benh toan than, thuoc dang dung, di ung - tat ca tuy chon
5. Section 4 (Lifestyle): screen time (gio/ngay), moi truong lam viec (dropdown), contact lens (dropdown), ghi chu - tat ca tuy chon
6. Le tan dien day du thong tin bat buoc va bam "Luu" -> He thong tao ho so moi voi ma benh nhan tu sinh (GK-YYYY-NNNN)
7. Benh nhan xuat hien tren dashboard voi trang thai "Cho kham"

**Truong hop ngoai le:**
1. Le tan bam "Luu & Chuyen tien kham" -> He thong luu ho so va tu dong chuyen benh nhan sang giai doan Pre-Exam (khong qua Reception)
2. Le tan bam "Huy" -> He thong dong form, khong luu du lieu, quay ve dashboard
3. Cac section co the thu gon/mo rong bang cach click vao header section

**Truong hop loi:**
1. Thieu truong bat buoc -> He thong highlight truong loi va hien thi thong bao "Vui long dien day du thong tin bat buoc"
2. So dien thoai khong dung dinh dang -> He thong hien thi loi "So dien thoai phai co 10-11 so va bat dau bang 0"
3. Ngay sinh vuot qua ngay hien tai -> He thong hien thi loi "Ngay sinh khong hop le"

#### Ghi chu ky thuat
- Form pattern: React Hook Form + Zod schema
- Route: Tu dashboard mo form (page hoac modal)
- API: POST /api/patients

---

### US-RCP-005: He thong phat hien trung so dien thoai khi dang ky benh nhan

**La mot** le tan,
**Toi muon** duoc canh bao khi nhap so dien thoai da ton tai trong he thong,
**De** tranh tao trung ho so cho cung mot benh nhan.

**Yeu cau lien quan:** RCP-02
**Quyet dinh lien quan:** D-07

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan nhap so dien thoai vao form tiep nhan benh nhan moi
2. He thong kiem tra real-time so dien thoai trong database
3. Neu so dien thoai da ton tai -> Hien thi thanh canh bao vang ngay duoi field so dien thoai: "SDT 0912 345 678 da ton tai -- BN: **Nguyen Van An** (1985)"
4. Thanh canh bao co nut "Mo ho so cu" -> Click se redirect den ho so benh nhan da co

**Truong hop ngoai le:**
1. So dien thoai chua ton tai -> Khong hien thi canh bao, le tan tiep tuc nhap thong tin binh thuong
2. Le tan van muon tao ho so moi du trung so dien thoai -> He thong cho phep (truong hop nguoi than cung so)

**Truong hop loi:**
1. Loi kiem tra trung so dien thoai -> He thong cho phep tiep tuc nhap (khong chan form), hien thi canh bao "Khong the kiem tra trung SĐT"

#### Ghi chu ky thuat
- Debounce kiem tra 500ms sau khi ngung go
- API: GET /api/patients/check-duplicate?phone={phone}

---

### US-RCP-006: Le tan luu va chuyen tien kham (auto-advance to Pre-Exam)

**La mot** le tan,
**Toi muon** luu ho so benh nhan va tu dong chuyen benh nhan sang giai doan Pre-Exam (do khuc xa/thi luc),
**De** giam mot buoc thao tac thu cong va benh nhan duoc vao kham nhanh hon.

**Yeu cau lien quan:** RCP-02
**Quyet dinh lien quan:** D-08

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan dien day du thong tin bat buoc tren form tiep nhan
2. Le tan bam nut "Luu & Chuyen tien kham" (primary button)
3. He thong luu ho so benh nhan moi
4. He thong tu dong tao luot kham (Visit) voi WorkflowStage = PreExam (bo qua Reception)
5. Benh nhan xuat hien tren dashboard bac si/ky thuat vien o giai doan Pre-Exam
6. Tren dashboard le tan, benh nhan hien thi voi trang thai "Dang kham" (vi da o Pre-Exam)

**Truong hop ngoai le:**
1. Le tan bam "Luu" (khong chuyen tien kham) -> He thong chi luu ho so, benh nhan o trang thai "Cho kham" (Reception)

**Truong hop loi:**
1. Loi tao Visit -> He thong van luu ho so benh nhan thanh cong, hien thi toast: "Da luu ho so nhung chua the chuyen tien kham. Vui long chuyen thu cong."
2. Validation loi -> He thong khong luu va highlight cac truong can sua

#### Ghi chu ky thuat
- Button "Luu & Chuyen tien kham" goi 2 API lien tiep: POST /api/patients roi POST /api/clinical/visits (voi stage = PreExam)
- Data flow: Ly do kham -> Chief Complaint, Tien su benh -> EMR History, Di ung -> EMR allergy warnings

---

## Dat Lich Hen (SCR-004)

### US-RCP-007: Le tan dat lich hen cho benh nhan co ho so

**La mot** le tan,
**Toi muon** dat lich hen cho benh nhan da co ho so trong he thong bang cach search ten hoac so dien thoai,
**De** benh nhan co lich hen va den dung ngay gio da hen.

**Yeu cau lien quan:** RCP-03
**Quyet dinh lien quan:** D-09, D-10, D-12

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan bam nut "Dat lich hen" tren dashboard -> He thong mo trang `/appointments/new` voi layout 2 cot
2. Cot trai: le tan nhap so dien thoai hoac ten vao o tim kiem
3. He thong tim thay benh nhan -> Hien thi thanh xanh la: "Da tim thay: **Le Minh Chau** (1978) -- BN-20260110-0045"
4. Thong tin benh nhan hien thi dang card read-only: ho ten, SDT, nam sinh, gioi tinh, lan kham gan nhat
5. Le tan nhap ly do kham (bat buoc), chon bac si chi dinh (tuy chon, mac dinh = BS lan kham truoc), ghi chu (tuy chon)
6. Cot phai: le tan chon ngay tren calendar mini va slot gio tren grid khung gio (sang/chieu, slot 30 phut)
7. Grid hien thi slot trong (xanh), da dat (xam), dang chon (tim)
8. Thanh xac nhan o cuoi hien thi tom tat: ten BN, ngay gio, ly do, BS -> Le tan doc lai cho BN qua dien thoai
9. Le tan bam "Xac nhan dat hen" -> He thong tao appointment, appointment hien thi tren dashboard vao dung ngay hen voi trang thai "Chua den"

**Truong hop ngoai le:**
1. Tat ca slot trong ngay da day -> He thong hien thi thong bao va goi y chon ngay khac
2. Le tan chon bac si cu the -> Grid chi hien thi slot cua bac si do (1 BN/BS/slot)
3. Le tan chon "Khong chi dinh (BS nao trong)" -> Grid hien thi tat ca slot tu do

**Truong hop loi:**
1. Loi tao appointment -> He thong hien thi toast loi, khong dong form, le tan co the thu lai
2. Slot vua duoc dat boi nguoi khac -> He thong hien thi "Slot nay vua duoc dat. Vui long chon slot khac."

#### Ghi chu ky thuat
- Route: `/appointments/new` (full page, khong phai dialog)
- Slot: 30 phut co dinh, lay gio hoat dong tu ClinicSchedule entity
- API: POST /api/scheduling/appointments

---

### US-RCP-008: Le tan dat lich hen cho benh nhan moi (qua dien thoai, chi luu ten + SDT)

**La mot** le tan,
**Toi muon** dat lich hen cho benh nhan moi chua co ho so bang cach nhap ten va so dien thoai,
**De** benh nhan co lich hen ma khong can tao ho so day du luc goi dien (thong tin day du se bo sung khi den check-in).

**Yeu cau lien quan:** RCP-03
**Quyet dinh lien quan:** D-09, D-11

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan search so dien thoai tren form dat lich hen -> He thong khong tim thay
2. Hien thi thanh vang: "Khong tim thay BN voi SDT nay. Nhap thong tin ben duoi de tao hen cho BN moi."
3. Form chuyen sang che do BN moi: hien thi cac field nhap tay: Ho ten (bat buoc), SDT (bat buoc, auto-fill tu o search), Ly do kham (bat buoc), Bac si chi dinh (tuy chon), Ghi chu (tuy chon)
4. Le tan chon ngay va slot gio tren cot phai
5. Le tan bam "Xac nhan dat hen" -> He thong tao appointment voi thong tin guest (GuestName, GuestPhone, GuestReason) tren Appointment record
6. He thong KHONG tao Patient record -> Patient record chi duoc tao khi BN den check-in

**Truong hop ngoai le:**
1. So dien thoai khong dung dinh dang -> Hien thi loi validation
2. Benh nhan moi goi lai dat them lich -> He thong cho phep nhieu appointment cho cung guest phone

**Truong hop loi:**
1. Loi tao appointment -> He thong hien thi toast loi va giu lai du lieu da nhap

#### Ghi chu ky thuat
- Appointment.PatientId = null cho guest booking
- Thong tin luu tren Appointment: GuestName, GuestPhone, GuestReason
- Patient record duoc tao khi check-in (SCR-005)

---

### US-RCP-009: He thong hien thi slot trong theo bac si va ngay

**La mot** le tan,
**Toi muon** nhin thay cac slot gio trong va da dat theo tung ngay va bac si tren grid khung gio,
**De** chon duoc slot phu hop cho benh nhan mot cach nhanh chong.

**Yeu cau lien quan:** RCP-03
**Quyet dinh lien quan:** D-10, D-12

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan chon 1 ngay tren calendar mini -> Grid khung gio cap nhat hien thi cac slot cua ngay do
2. Grid chia theo buoi: Sang va Chieu, moi slot 30 phut
3. Gio hoat dong lay tu ClinicSchedule entity (VD: T2-T6 13:00-20:00, T7-CN 08:00-12:00)
4. Moi slot hien thi 3 trang thai: Trong (xanh la), Da dat (xam), Dang chon (tim)
5. Header grid hien thi "X slot trong / Y tong" de le tan biet con bao nhieu cho

**Truong hop ngoai le:**
1. Ngay le hoac ngay nghi -> Grid khong hien thi slot (hoac hien thi "Ngay nghi")
2. Le tan chon bac si -> Grid chi hien thi slot cua bac si do, moi slot toi da 1 benh nhan
3. Le tan chon "BS nao trong" -> Grid hien thi slot tu do, khong gioi han theo bac si

**Truong hop loi:**
1. Loi tai lich hen -> He thong hien thi toast loi va grid hien thi trang thai loading

#### Ghi chu ky thuat
- API: GET /api/scheduling/available-slots?date={date}&doctorId={doctorId}
- ClinicSchedule entity cung cap gio hoat dong theo ngay trong tuan
- Slot 30 phut co dinh (D-10)

---

## Check-in & Tao Luot Kham (SCR-005)

### US-RCP-010: Le tan check-in benh nhan co ho so day du

**La mot** le tan,
**Toi muon** check-in benh nhan co ho so day du bang popup xac nhan nhanh,
**De** dua benh nhan vao hang doi "Cho kham" chi trong vai giay ma khong can nhap them thong tin.

**Yeu cau lien quan:** RCP-04
**Quyet dinh lien quan:** D-05, D-06

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan bam nut "Check-in" tren cot Thao tac cua benh nhan co trang thai "Chua den"
2. He thong kiem tra ho so: co du 5 truong bat buoc (ho ten, ngay sinh, gioi tinh, SDT, ly do kham)
3. He thong mo popup hien thi thong tin benh nhan: avatar initials (tim), ho ten, ma BN, gio hen, nam sinh, gioi tinh, SDT, nghe nghiep, ly do kham, lan kham gan nhat
4. Popup hien thi note xanh: "Xac nhan thong tin voi BN truoc khi check-in. Neu can sua, bam Sua thong tin."
5. Le tan bam "Xac nhan check-in" -> He thong chuyen trang thai "Chua den" -> "Cho kham", ghi nhan `checked_in_at` = thoi diem hien tai
6. Popup dong, quay ve dashboard. KPI "Chua den" giam 1, KPI "Cho kham" tang 1

**Truong hop ngoai le:**
1. Le tan bam "Sua thong tin" -> Dong popup, mo form Intake o che do edit voi du lieu pre-fill. Sau khi luu Intake -> BN tu dong check-in va chuyen "Cho kham"
2. Le tan bam "Huy" -> Dong popup, BN van o trang thai "Chua den"

**Truong hop loi:**
1. Loi check-in -> He thong hien thi toast loi, BN van o trang thai "Chua den", popup khong dong

#### Ghi chu ky thuat
- Popup: Dialog modal (shadcn/ui Dialog)
- API: POST /api/scheduling/appointments/{id}/check-in

---

### US-RCP-011: Le tan check-in benh nhan co ho so chua day du (bo sung thong tin)

**La mot** le tan,
**Toi muon** check-in benh nhan co ho so thieu thong tin va bo sung cac truong con thieu,
**De** dam bao benh nhan co du du lieu truoc khi vao kham (vi du: benh nhan dat hen qua dien thoai chi co ten va SDT).

**Yeu cau lien quan:** RCP-04
**Quyet dinh lien quan:** D-05, D-06

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan bam nut "Check-in" tren cot Thao tac cua benh nhan co trang thai "Chua den"
2. He thong kiem tra ho so: thieu truong bat buoc (vi du: thieu ngay sinh, gioi tinh)
3. He thong mo popup voi avatar initials (amber thay vi tim de canh bao)
4. Fields co du lieu hien thi binh thuong. Fields thieu hien thi "Chua co" (italic, mau nhat)
5. Popup hien thi canh bao vang: "Ho so chua day du -- BN dat hen qua dien thoai, chi co ten + SDT. Can bo sung ngay sinh, gioi tinh va cac thong tin khac truoc khi kham."
6. Le tan bam "Check-in & bo sung ho so ->" -> He thong dong popup va mo form Intake (SCR-003) o che do edit voi du lieu hien co pre-fill
7. Le tan dien day du thong tin con thieu va bam "Luu" -> He thong luu ho so va tu dong check-in, chuyen "Cho kham"

**Truong hop ngoai le:**
1. Le tan bam "Huy" -> Dong popup, BN van o trang thai "Chua den", ho so khong thay doi
2. Benh nhan guest (dat hen qua dien thoai, chua co Patient record) -> He thong tao Patient record moi tu thong tin GuestName, GuestPhone tren Appointment

**Truong hop loi:**
1. Loi luu ho so -> He thong hien thi loi validation tren form Intake, khong check-in

#### Ghi chu ky thuat
- Logic phan loai: co du 5 truong bat buoc = popup BN cu, thieu bat ky truong nao = popup BN moi (canh bao)
- Guest booking (PatientId = null): tao Patient record tu GuestName + GuestPhone truoc khi check-in

---

### US-RCP-012: Le tan tao luot kham walk-in cho benh nhan khong co hen

**La mot** le tan,
**Toi muon** tao luot kham truc tiep cho benh nhan cu da co ho so nhung khong co lich hen (walk-in tai kham),
**De** dua benh nhan vao hang doi "Cho kham" nhanh chong ma khong can dat lich hen truoc.

**Yeu cau lien quan:** RCP-04
**Quyet dinh lien quan:** D-05

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan search benh nhan bang SDT/ten tren dashboard -> Tim thay benh nhan cu
2. Le tan click vao ket qua search -> He thong mo popup "Tao luot kham"
3. Popup hien thi thong tin benh nhan (read-only): ho ten, ma BN, nam sinh, gioi tinh, SDT, lan kham gan nhat
4. Le tan nhap ly do kham (bat buoc) va ghi chu (tuy chon)
5. Le tan bam "Xac nhan tao luot kham" -> He thong tao Visit moi voi source = Walk-in, chuyen benh nhan sang "Cho kham"
6. Benh nhan xuat hien tren dashboard voi badge "Walk-in" (coral) va trang thai "Cho kham"

**Truong hop ngoai le:**
1. Benh nhan da co luot kham chua hoan thanh trong ngay -> He thong canh bao: "BN da co luot kham dang xu ly. Ban co muon tao them luot kham moi?"
2. Le tan bam "Huy" -> Dong popup, khong tao luot kham

**Truong hop loi:**
1. Loi tao Visit -> He thong hien thi toast loi, popup khong dong

#### Ghi chu ky thuat
- Popup: Dialog modal voi field ly do kham + ghi chu
- API: POST /api/clinical/visits (voi source = WalkIn)

---

## Dashboard Actions (SCR-006)

### US-RCP-013: Le tan doi lich hen cho benh nhan

**La mot** le tan,
**Toi muon** doi ngay gio hen cho benh nhan khi ho goi dien bao thay doi,
**De** cap nhat lich hen ma khong can huy va dat lai tu dau.

**Yeu cau lien quan:** RCP-05
**Quyet dinh lien quan:** D-13

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan bam icon ⋯ tren dong benh nhan co trang thai "Chua den" -> Menu dropdown hien thi
2. Le tan chon "Doi lich hen" (icon lich, mau tim) -> He thong mo popup doi lich
3. Popup hien thi: thong tin BN (ten, SDT), card lich hen hien tai (ngay gio cu, ly do), calendar mini, grid slot gio
4. Le tan chon ngay moi va slot gio moi tren calendar/grid
5. Thanh xac nhan hien thi so sanh: lich cu (gach ngang) -> lich moi (bold)
6. Le tan bam "Xac nhan doi lich" -> He thong cap nhat appointment voi ngay gio moi
7. Dashboard cap nhat: neu ngay moi la hom nay -> BN van hien thi; neu ngay moi la ngay khac -> BN bien mat khoi danh sach hom nay

**Truong hop ngoai le:**
1. Le tan chon cung ngay cung gio cu -> He thong hien thi loi: "Vui long chon thoi gian khac voi lich hen hien tai"
2. Slot da day -> He thong hien thi slot do la "Da dat" (xam), khong cho chon

**Truong hop loi:**
1. Slot vua duoc dat boi nguoi khac -> He thong hien thi "Slot nay vua duoc dat. Vui long chon slot khac."
2. Loi cap nhat appointment -> He thong hien thi toast loi, lich hen cu van giu nguyen

#### Ghi chu ky thuat
- Chi ap dung cho trang thai "Chua den" (BN chua den)
- API: PUT /api/scheduling/appointments/{id}/reschedule

---

### US-RCP-014: Le tan danh dau benh nhan khong den (no-show) voi ghi chu

**La mot** le tan,
**Toi muon** danh dau benh nhan khong den kham (no-show) voi ghi chu tuy chon va lua chon dat hen lai,
**De** cap nhat trang thai va giu lieu trinh sach cho cac benh nhan con lai.

**Yeu cau lien quan:** RCP-06
**Quyet dinh lien quan:** D-13, D-14, D-15

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan bam icon ⋯ tren dong benh nhan co trang thai "Chua den" -> Chon "Danh dau khong den" (icon warning, mau amber)
2. He thong mo popup no-show voi: thong tin BN (ten, SDT), field ghi chu (tuy chon, text input), checkbox "Dat hen lai"
3. Le tan nhap ghi chu (VD: "BN goi bao ban, khong den duoc") va bam "Xac nhan"
4. He thong cap nhat appointment: trang thai = NoShow, ghi nhan `no_show_at`, `no_show_by`, `no_show_notes`
5. Benh nhan bien mat khoi danh sach dashboard (hoac hien thi voi trang thai dac biet)
6. KPI "Lich hen hom nay" sub-text "X chua den" giam 1

**Truong hop ngoai le:**
1. Le tan tick checkbox "Dat hen lai" -> Sau khi xac nhan no-show, he thong tu dong chuyen den trang `/appointments/new` voi thong tin benh nhan da dien san
2. Le tan khong nhap ghi chu -> He thong van cho phep xac nhan (ghi chu la tuy chon)

**Truong hop loi:**
1. Loi cap nhat trang thai -> He thong hien thi toast loi, BN van o trang thai "Chua den"

#### Ghi chu ky thuat
- Chi ap dung cho trang thai "Chua den"
- Checkbox "Dat hen lai" navigate den `/appointments/new` voi patient pre-fill (D-15)
- API: POST /api/scheduling/appointments/{id}/no-show

---

### US-RCP-015: Le tan huy lich hen voi ly do bat buoc

**La mot** le tan,
**Toi muon** huy lich hen cua benh nhan voi ly do bat buoc,
**De** ghi nhan ro rang ly do huy va giai phong slot gio cho benh nhan khac.

**Yeu cau lien quan:** RCP-06
**Quyet dinh lien quan:** D-13, D-14

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan bam icon ⋯ tren dong benh nhan co trang thai "Chua den" -> Chon "Huy hen" (icon X circle, mau do)
2. He thong mo popup huy hen voi: thong tin BN (ten, SDT, lich hen hien tai), dropdown ly do huy (bat buoc)
3. Cac ly do huy: "BN yeu cau huy", "Phong kham huy", "Trung lich", "Ly do khac"
4. Neu chon "Ly do khac" -> Hien thi textarea de le tan nhap ly do cu the
5. Le tan chon ly do va bam "Xac nhan huy hen" -> He thong cap nhat appointment: trang thai = Cancelled, ghi nhan `cancelled_by`, `cancelled_reason`
6. Benh nhan bien mat khoi danh sach dashboard, slot gio duoc giai phong

**Truong hop ngoai le:**
1. Le tan chua chon ly do -> Nut "Xac nhan huy hen" bi disable, khong cho bam
2. Le tan bam "Huy" tren popup -> Dong popup, lich hen van giu nguyen

**Truong hop loi:**
1. Loi huy appointment -> He thong hien thi toast loi, lich hen van giu nguyen

#### Ghi chu ky thuat
- Chi ap dung cho trang thai "Chua den"
- Ly do huy bat buoc (dropdown) - D-14
- API: POST /api/scheduling/appointments/{id}/cancel

---

### US-RCP-016: Le tan huy luot kham voi ly do va tuy chon dat hen lai

**La mot** le tan,
**Toi muon** huy luot kham cua benh nhan dang cho kham voi ly do bat buoc va tuy chon dat hen lai,
**De** xu ly truong hop benh nhan doi y khong muon kham nua sau khi da check-in.

**Yeu cau lien quan:** RCP-06
**Quyet dinh lien quan:** D-13, D-14, D-15

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Le tan bam icon ⋯ tren dong benh nhan co trang thai "Cho kham" -> Chon "Huy luot kham" (icon X circle, mau do)
2. He thong mo popup huy luot kham voi: thong tin BN (ten, SDT), dropdown ly do huy (bat buoc), checkbox "Dat hen lai"
3. Cac ly do huy: "BN yeu cau huy", "Cho qua lau", "Ly do ca nhan", "Ly do khac"
4. Neu chon "Ly do khac" -> Hien thi textarea de nhap ly do cu the
5. Le tan chon ly do va bam "Xac nhan huy luot kham" -> He thong cap nhat Visit: status = Cancelled, ghi nhan `cancelled_reason`, `cancelled_by`
6. Benh nhan chuyen sang trang thai huy tren dashboard (hoac bien mat), KPI "Cho kham" giam 1

**Truong hop ngoai le:**
1. Le tan tick checkbox "Dat hen lai" -> Sau khi xac nhan huy, he thong tu dong chuyen den trang `/appointments/new` voi thong tin benh nhan da dien san
2. Le tan chua chon ly do -> Nut "Xac nhan huy luot kham" bi disable

**Truong hop loi:**
1. Loi huy Visit -> He thong hien thi toast loi, luot kham van giu nguyen

#### Ghi chu ky thuat
- Chi ap dung cho trang thai "Cho kham" (BN da check-in nhung chua vao kham)
- Ly do huy bat buoc (dropdown) - D-14
- Checkbox "Dat hen lai" navigate den `/appointments/new` voi patient pre-fill (D-15)
- API: POST /api/clinical/visits/{id}/cancel
