# User Stories: Cải Tiến Quy Trình Lâm Sàng (Clinical Workflow Overhaul)

**Phase:** 13 - Clinical Workflow Overhaul
**Ngay tao:** 2026-03-25
**Yeu cau lien quan:** CLN-03, CLN-04
**So luong user stories:** 15

---

## CLN-03: Kanban va Table View

### US-CLN-13-001: Kanban 8 cot theo giai doan quy trinh kham

**La mot** nhan vien phong kham,
**Toi muon** nhin thay bang kanban co 8 cot tuong ung voi 8 giai doan quy trinh kham (Tiep nhan, Do khuc xa/Thi luc, Kham bac si, Chan doan hinh anh, Bac si doc ket qua, Ke don, Thu ngan, Nha thuoc/Quang),
**De** theo doi chinh xac vi tri cua tung benh nhan trong quy trinh kham ma khong bi nhom gop cac giai doan.

**Yeu cau lien quan:** CLN-03
**Quyet dinh lien quan:** D-01, D-02

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Nhan vien mo trang Dashboard lam sang (`/clinical`) -> He thong hien thi bang kanban voi 8 cot rieng biet
2. Thu tu cot tu trai sang phai: Tiep nhan (Reception), Do khuc xa/Thi luc (Refraction/VA), Kham bac si (Doctor Exam), Chan doan hinh anh (Diagnostics), Bac si doc ket qua (Doctor Reads), Ke don (Rx), Thu ngan (Cashier), Nha thuoc/Quang (Pharmacy/Optical)
3. Moi cot hien thi so luong benh nhan hien tai trong giai doan do (badge dem)
4. The benh nhan hien thi thong tin: ten benh nhan, bac si phu trach, thoi gian cho, canh bao di ung (neu co)
5. Nhan vien keo tha (drag-and-drop) the benh nhan giua cac cot de chuyen giai doan

**Truong hop ngoai le:**
1. Khong co benh nhan nao trong giai doan -> Cot hien thi trang thai rong: "Khong co benh nhan"
2. Co nhieu benh nhan trong mot cot -> Cot co thanh cuon doc de xem tat ca benh nhan
3. Benh nhan co di ung -> The benh nhan hien thi icon canh bao di ung mau do

**Truong hop loi:**
1. Loi tai du lieu benh nhan -> He thong hien thi toast loi: "Khong the tai danh sach benh nhan. Vui long thu lai"
2. Loi keo tha benh nhan -> He thong tra the benh nhan ve cot cu va hien thi toast loi

#### Ghi chu ky thuat
- Component: `WorkflowDashboard.tsx` - cap nhat `WORKFLOW_COLUMNS` tu 5 cot thanh 8 cot (1:1 mapping voi `WorkflowStage` enum)
- Enum: `WorkflowStage.cs` - Reception=0, RefractionVA=1, DoctorExam=2, Diagnostics=3, DoctorReads=4, Rx=5, Cashier=6, PharmacyOptical=7
- API: GET /api/clinical/active-visits

---

### US-CLN-13-002: Cot Done hien thi benh nhan da hoan thanh trong ngay

**La mot** nhan vien phong kham,
**Toi muon** nhin thay cot "Hoan thanh" (Done) hien thi cac benh nhan da hoan thanh quy trinh kham trong ngay hom nay,
**De** theo doi tong so benh nhan da kham xong va xac nhan khong bo sot benh nhan nao.

**Yeu cau lien quan:** CLN-03
**Quyet dinh lien quan:** D-04

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Nhan vien mo trang Dashboard lam sang -> Ngoai 8 cot chinh, co them cot thu 9 "Hoan thanh" (Done) o cuoi cung ben phai
2. Cot Done hien thi cac benh nhan da hoan thanh giai doan cuoi (PharmacyOptical) trong ngay hom nay
3. The benh nhan trong cot Done hien thi: ten benh nhan, bac si phu trach, thoi gian hoan thanh
4. Cuoi ngay (hoac khi tai lai trang vao ngay hom sau), cot Done tu dong trong

**Truong hop ngoai le:**
1. Khong co benh nhan hoan thanh trong ngay -> Cot Done hien thi trang thai rong
2. Benh nhan hoan thanh tu ngay hom truoc -> Khong hien thi trong cot Done (chi hien thi ngay hien tai)

**Truong hop loi:**
1. Loi loc du lieu theo ngay -> He thong hien thi toast canh bao va hien thi tat ca benh nhan hoan thanh gan nhat

#### Ghi chu ky thuat
- Loc benh nhan theo `VisitDate` = ngay hien tai va `WorkflowStage` = PharmacyOptical (7) va `VisitStatus` != Cancelled
- API: GET /api/clinical/active-visits (them tham so `includeCompleted=true`)

---

### US-CLN-13-003: Cuon ngang xem tat ca 9 cot kanban

**La mot** nhan vien phong kham,
**Toi muon** cuon ngang de xem tat ca 9 cot kanban khi man hinh khong du rong hien thi het,
**De** truy cap nhanh bat ky giai doan nao ma khong can thu nho giao dien.

**Yeu cau lien quan:** CLN-03
**Quyet dinh lien quan:** D-03

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Nhan vien mo Dashboard lam sang tren man hinh co do rong khong du cho 9 cot
2. He thong hien thi thanh cuon ngang (horizontal scroll) phia duoi bang kanban
3. Nhan vien cuon ngang de xem cac cot bi che khuat -> Cac cot truot muot (smooth scrolling) theo kieu Trello/Jira
4. Moi cot giu nguyen do rong toi thieu de hien thi day du thong tin the benh nhan

**Truong hop ngoai le:**
1. Man hinh du rong hien thi tat ca 9 cot -> Khong hien thi thanh cuon ngang (an tu dong)
2. Nhan vien dung trackpad hoac chuot cuon -> Ca hai deu hoat dong binh thuong

**Truong hop loi:**
1. Loi render layout -> He thong hien thi cac cot xep doc (fallback) thay vi xep ngang

#### Ghi chu ky thuat
- CSS: `overflow-x: auto` tren container kanban, moi cot co `min-width` co dinh
- Tuong thich voi @dnd-kit/core khi cuon ngang va keo tha

---

### US-CLN-13-004: Chuyen doi giua Kanban va Table view

**La mot** nhan vien phong kham,
**Toi muon** chuyen doi giua giao dien kanban va giao dien bang (table view) bang nut bam,
**De** chon cach xem phu hop voi nhu cau cong viec cua toi.

**Yeu cau lien quan:** CLN-03
**Quyet dinh lien quan:** D-05

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Nhan vien mo Dashboard lam sang -> Thay nut chuyen doi view (icon grid/list) tren thanh cong cu
2. Nhan vien nhan nut chuyen doi -> Giao dien chuyen tu kanban sang table view (hoac nguoc lai) ngay lap tuc
3. He thong luu lua chon cuoi cung vao localStorage -> Lan sau mo Dashboard, he thong hien thi view da chon truoc do
4. Nut chuyen doi hien thi icon tuong ung voi view hien tai (icon grid khi dang xem kanban, icon list khi dang xem table)

**Truong hop ngoai le:**
1. localStorage khong kha dung -> Mac dinh hien thi kanban view
2. Du lieu localStorage bi hong -> Mac dinh hien thi kanban view

**Truong hop loi:**
1. Loi render table view -> He thong hien thi toast loi va giu kanban view

#### Ghi chu ky thuat
- Component moi: `ViewToggle.tsx` voi icon tu Tabler Icons (IconLayoutKanban, IconTable)
- localStorage key: `clinical-dashboard-view` voi gia tri `kanban` hoac `table`
- Component moi: `VisitTableView.tsx` hien thi danh sach benh nhan dang bang

---

### US-CLN-13-005: Sap xep va loc benh nhan trong Table view

**La mot** nhan vien phong kham,
**Toi muon** sap xep va loc danh sach benh nhan theo cac cot trong table view,
**De** tim nhanh benh nhan can quan tam hoac xem benh nhan theo thu tu uu tien.

**Yeu cau lien quan:** CLN-03
**Quyet dinh lien quan:** D-06

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Nhan vien chuyen sang table view -> He thong hien thi bang voi cac cot: Benh nhan, Bac si, Giai doan, Thoi gian cho, Thoi gian kham, Trang thai
2. Nhan vien nhan vao tieu de cot -> Bang sap xep theo cot do (toggle giua tang dan va giam dan)
3. Nhan vien su dung bo loc (filter) -> Bang chi hien thi benh nhan khop dieu kien loc
4. Bo loc ho tro: loc theo giai doan (chon nhieu giai doan), loc theo bac si, loc theo trang thai

**Truong hop ngoai le:**
1. Ket qua loc rong -> He thong hien thi: "Khong tim thay benh nhan nao khop dieu kien loc"
2. Nhieu dieu kien loc dong thoi -> He thong ap dung tat ca dieu kien (AND logic)

**Truong hop loi:**
1. Loi sap xep -> He thong giu thu tu hien tai va hien thi toast canh bao
2. Loi loc -> He thong xoa bo loc va hien thi tat ca benh nhan

#### Ghi chu ky thuat
- Su dung shadcn/ui Table component voi header sortable
- Bo loc su dung shadcn/ui Select va Popover components
- Du lieu tu cung API `GET /api/clinical/active-visits`, xu ly sort/filter phia client

---

## CLN-04: Tien trinh giai doan va lich su kham

### US-CLN-13-006: Bac si dua benh nhan ve giai doan truoc (Stage Reversal)

**La mot** bac si,
**Toi muon** chuyen benh nhan ve giai doan kham truoc do (voi ly do bat buoc),
**De** xu ly cac truong hop can kham lai, bo sung xet nghiem, hoac sua doi chan doan.

**Yeu cau lien quan:** CLN-04
**Quyet dinh lien quan:** D-07, D-08, D-09

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Bac si mo chi tiet luot kham cua benh nhan -> Nhan nut "Chuyen ve giai doan truoc" (Reverse Stage)
2. He thong hien thi dialog chon giai doan muon chuyen ve (chi hien thi cac giai doan duoc phep quay lai)
3. Bac si chon giai doan va nhap ly do bat buoc (o nhap van ban) -> Nhan "Xac nhan"
4. He thong chuyen benh nhan ve giai doan da chon -> The benh nhan xuat hien trong cot kanban tuong ung
5. He thong ghi nhan audit trail: ai chuyen, tu giai doan nao, den giai doan nao, ly do, thoi gian

**Truong hop ngoai le:**
1. Bac si khong nhap ly do -> He thong hien thi loi: "Vui long nhap ly do chuyen giai doan"
2. Ly do qua ngan (duoi 10 ky tu) -> He thong hien thi loi: "Ly do phai co it nhat 10 ky tu"
3. Benh nhan dang o giai doan dau tien (Reception) -> Nut "Chuyen ve giai doan truoc" bi vo hieu hoa

**Truong hop loi:**
1. Loi cap nhat giai doan -> He thong giu nguyen giai doan hien tai va hien thi toast loi: "Chuyen giai doan that bai. Vui long thu lai"
2. Loi ghi audit trail -> He thong van chuyen giai doan nhung ghi log canh bao de xu ly sau

#### Ghi chu ky thuat
- API moi: POST /api/clinical/visits/{visitId}/reverse-stage voi body: { targetStage, reason }
- Domain: Them phuong thuc `ReverseStage(WorkflowStage targetStage, string reason, Guid reversedBy)` vao Visit entity
- Audit: Ghi vao bang WorkflowStageTransition voi direction = "Backward"

---

### US-CLN-13-007: He thong chan viec quay lai tu giai doan Thu ngan/Nha thuoc

**La mot** he thong,
**Toi muon** chan viec quay lai giai doan truoc khi benh nhan da den giai doan Thu ngan hoac Nha thuoc/Quang,
**De** dam bao tinh toan ven cua quy trinh tai chinh va cap phat thuoc.

**Yeu cau lien quan:** CLN-04
**Quyet dinh lien quan:** D-07

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Benh nhan dang o giai doan Thu ngan (Cashier) hoac Nha thuoc/Quang (PharmacyOptical)
2. Bac si hoac nhan vien co gang chuyen benh nhan ve giai doan truoc
3. He thong tu choi va hien thi thong bao: "Khong the chuyen ve giai doan truoc khi benh nhan da den giai doan Thu ngan/Nha thuoc. Vui long lien he quan ly."
4. He thong ghi nhan lan thu chuyen giai doan bi tu choi vao audit trail

**Truong hop ngoai le:**
1. Quan ly (Manager role) co the ghi de han che nay trong truong hop dac biet -> He thong yeu cau xac nhan them va ly do chi tiet
2. Benh nhan o giai doan Ke don (Rx) -> Van cho phep quay lai (chua den giai doan tai chinh)

**Truong hop loi:**
1. Loi kiem tra quyen -> He thong mac dinh tu choi chuyen giai doan (fail-safe)

#### Ghi chu ky thuat
- Domain rule trong Visit entity: `CanReverseFrom(WorkflowStage currentStage)` tra ve false khi stage >= Cashier (6)
- Ngoai le cho Manager role thong qua tham so `overrideByManager` (can them xac nhan)
- Backend validation truoc khi xu ly request

---

### US-CLN-13-008: Tu dong chuyen giai doan khi bac si ky ten (Sign-off Auto-advance)

**La mot** bac si,
**Toi muon** he thong tu dong chuyen benh nhan sang giai doan tiep theo khi toi ky ten (sign off) luot kham,
**De** khong phai thao tac them viec keo tha the benh nhan tren kanban sau khi da hoan thanh kham.

**Yeu cau lien quan:** CLN-04
**Quyet dinh lien quan:** D-11

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Bac si mo chi tiet luot kham -> Hoan thanh kham -> Nhan nut "Ky ten" (Sign Off)
2. He thong ghi nhan ky ten thanh cong -> Tu dong chuyen benh nhan sang giai doan tiep theo trong quy trinh
3. Tren bang kanban, the benh nhan tu dong di chuyen sang cot giai doan moi (realtime cap nhat)
4. He thong ghi nhan audit trail: giai doan tu dong chuyen, triggered by sign-off

**Truong hop ngoai le:**
1. Benh nhan dang o giai doan cuoi (PharmacyOptical) khi bac si ky ten -> Khong chuyen giai doan (da la giai doan cuoi), danh dau luot kham hoan thanh
2. Bac si ky ten tu giai doan DoctorExam -> He thong chuyen sang Diagnostics (hoac Rx neu bo qua giai doan)
3. Bac si ky ten tu giai doan DoctorReads -> He thong chuyen sang Rx

**Truong hop loi:**
1. Ky ten thanh cong nhung loi chuyen giai doan -> He thong giu trang thai ky ten, ghi log loi, hien thi toast canh bao: "Da ky ten nhung chua chuyen giai doan tu dong. Vui long chuyen thu cong"
2. Loi cap nhat realtime kanban -> He thong yeu cau nhan vien tai lai trang

#### Ghi chu ky thuat
- Domain event: `VisitSignedOffEvent` -> Handler tu dong goi `AdvanceStage()` tren Visit entity
- Backend: `SignOffVisitHandler` phat domain event, `AutoAdvanceOnSignOffHandler` xu ly event
- SignalR cap nhat realtime kanban khi giai doan thay doi

---

### US-CLN-13-009: Nhan vien bo qua giai doan khong can thiet (Stage Skipping)

**La mot** nhan vien phong kham,
**Toi muon** bo qua cac giai doan kham khong can thiet cho benh nhan (vi du: bo qua Chan doan hinh anh va Bac si doc ket qua),
**De** benh nhan di chuyen nhanh hon trong quy trinh khi khong can thuc hien tat ca cac buoc.

**Yeu cau lien quan:** CLN-04
**Quyet dinh lien quan:** D-12

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Nhan vien nhin thay the benh nhan tren kanban o giai doan hien tai (vi du: DoctorExam)
2. Nhan vien keo tha the benh nhan sang cot giai doan xa hon (vi du: tu DoctorExam sang Rx, bo qua Diagnostics va DoctorReads)
3. He thong hien thi dialog xac nhan: "Bo qua giai doan Chan doan hinh anh va Bac si doc ket qua. Ban co chac chan?"
4. Nhan vien xac nhan -> He thong chuyen benh nhan sang giai doan moi va ghi nhan cac giai doan bi bo qua vao audit trail

**Truong hop ngoai le:**
1. Nhan vien bo qua giai doan DoctorExam -> He thong canh bao: "Giai doan Kham bac si thuong khong nen bo qua. Ban co chac chan?" (canh bao nhung van cho phep)
2. Nhan vien bo qua den giai doan Thu ngan (Cashier) -> He thong kiem tra benh nhan co don thuoc/don kinh chua truoc khi cho phep
3. Nhan vien keo tha nguoc lai -> Xu ly nhu Stage Reversal (US-CLN-13-006)

**Truong hop loi:**
1. Loi chuyen giai doan -> He thong tra the benh nhan ve cot cu va hien thi toast loi
2. Loi ghi audit trail -> He thong van chuyen giai doan nhung ghi log canh bao

#### Ghi chu ky thuat
- Su dung cung API `AdvanceStage` nhung cho phep nhay nhieu buoc (targetStage thay vi chi nextStage)
- API: POST /api/clinical/visits/{visitId}/advance-stage voi body: { targetStage, skippedStages[] }
- Domain: Them validation logic cho viec bo qua giai doan trong Visit entity

---

### US-CLN-13-010: Bac si xem lich su kham cua benh nhan (Visit History Timeline)

**La mot** bac si,
**Toi muon** xem lich su tat ca cac luot kham cua benh nhan theo thoi gian (timeline),
**De** nam bat toan bo qua trinh dieu tri va dua ra quyet dinh lam sang phu hop.

**Yeu cau lien quan:** CLN-04
**Quyet dinh lien quan:** D-13, D-14, D-15

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Bac si mo ho so benh nhan -> Chon tab "Lich su kham" (Visit History)
2. He thong hien thi layout 2 cot: timeline ben trai (~300px) va chi tiet luot kham ben phai (phan con lai)
3. Timeline hien thi danh sach luot kham theo thu tu thoi gian (moi nhat o tren)
4. Moi the luot kham tren timeline hien thi: ngay kham, ten bac si, chan doan chinh, badge trang thai (Nhap/Da ky/Da sua doi)
5. Bac si nhan vao mot luot kham tren timeline -> Panel ben phai hien thi chi tiet luot kham do

**Truong hop ngoai le:**
1. Benh nhan chua co luot kham nao -> He thong hien thi: "Benh nhan chua co lich su kham"
2. Benh nhan co nhieu luot kham (>50) -> He thong phan trang (infinite scroll hoac pagination) tren timeline
3. Luot kham da huy (Cancelled) -> Van hien thi tren timeline nhung voi badge "Da huy" va mau xam

**Truong hop loi:**
1. Loi tai lich su kham -> He thong hien thi toast loi: "Khong the tai lich su kham. Vui long thu lai"
2. Loi tai chi tiet mot luot kham -> He thong hien thi toast loi va giu panel hien tai

#### Ghi chu ky thuat
- Tab moi tren PatientProfilePage: "Lich su kham" (Visit History)
- API moi: GET /api/clinical/patients/{patientId}/visit-history (phan trang, sap xep theo ngay giam dan)
- Component moi: `VisitHistoryTab.tsx` voi layout 2 cot
- Component moi: `VisitTimeline.tsx` hien thi danh sach the luot kham
- Component moi: `VisitHistoryDetail.tsx` tai su dung cac section tu VisitDetailPage

---

### US-CLN-13-011: Bac si xem chi tiet luot kham tu lich su (Read-only)

**La mot** bac si,
**Toi muon** xem day du chi tiet cua mot luot kham cu (bao gom khuc xa, kho mat, chan doan, don thuoc, hinh anh, ghi chu, thong tin ky ten) trong che do chi doc (read-only),
**De** tham khao ket qua kham truoc khi kham lan moi cho benh nhan.

**Yeu cau lien quan:** CLN-04
**Quyet dinh lien quan:** D-16

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Bac si chon mot luot kham tren timeline -> Panel ben phai hien thi day du chi tiet
2. Chi tiet bao gom tat ca section giong trang VisitDetailPage: Khuc xa (Refraction), Kho mat (Dry Eye), Chan doan (Diagnosis), Don thuoc (Drug Prescription), Don kinh (Optical Prescription), Hinh anh y khoa, Ghi chu, Thong tin ky ten
3. Tat ca du lieu hien thi o che do chi doc (khong co nut chinh sua, khong co form input)
4. Hinh anh y khoa co the nhan vao de xem phong to (lightbox)

**Truong hop ngoai le:**
1. Luot kham chua co du lieu khuc xa -> Section khuc xa hien thi: "Chua co du lieu"
2. Luot kham chua duoc ky ten (Draft) -> Hien thi canh bao: "Luot kham nay chua duoc ky ten - du lieu co the chua day du"
3. Luot kham da sua doi (Amended) -> Hien thi lich su sua doi va ly do sua

**Truong hop loi:**
1. Loi tai chi tiet luot kham -> He thong hien thi toast loi va giu nguyen panel truoc do
2. Loi tai hinh anh -> He thong hien thi placeholder "Khong the tai hinh anh"

#### Ghi chu ky thuat
- Tai su dung cac component tu VisitDetailPage: RefractionSection, DryEyeSection, DiagnosisSection, DrugPrescriptionSection, OpticalPrescriptionSection nhung truyen prop `readOnly={true}`
- API: GET /api/clinical/visits/{visitId} (dung API hien co)

---

### US-CLN-13-012: Nhan vao ten benh nhan de chuyen den trang ho so

**La mot** nhan vien phong kham,
**Toi muon** nhan vao ten benh nhan tren the kanban hoac trang chi tiet luot kham de chuyen den trang ho so benh nhan,
**De** truy cap nhanh thong tin ca nhan va lich su y khoa cua benh nhan.

**Yeu cau lien quan:** CLN-04
**Quyet dinh lien quan:** D-17

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Nhan vien nhin thay ten benh nhan tren the kanban -> Ten benh nhan la duong dan (link) mau xanh co gach chan khi hover
2. Nhan vien nhan vao ten benh nhan -> He thong chuyen huong den trang ho so benh nhan (`/patients/{patientId}`)
3. Tuong tu, tren trang chi tiet luot kham, ten benh nhan o phan header cung la link den trang ho so
4. Link mo trang ho so benh nhan trong cung tab (khong mo tab moi)

**Truong hop ngoai le:**
1. Nhan Ctrl+Click (hoac Cmd+Click tren Mac) -> Mo trang ho so trong tab moi
2. Benh nhan da bi xoa hoac vo hieu hoa -> Link van hoat dong, trang ho so hien thi trang thai tuong ung

**Truong hop loi:**
1. PatientId khong hop le -> He thong chuyen huong den trang 404
2. Loi tai trang ho so -> He thong hien thi toast loi

#### Ghi chu ky thuat
- Cap nhat PatientCard.tsx: them Link component tu TanStack Router bao quanh ten benh nhan
- Cap nhat VisitDetailPage.tsx: them Link cho ten benh nhan o header
- Route: `/patients/$patientId`

---

### US-CLN-13-013: Bac si xem cau tra loi OSDI tren trang luot kham

**La mot** bac si,
**Toi muon** xem chi tiet cac cau tra loi bo cau hoi OSDI (khong chi diem so) tren trang chi tiet luot kham,
**De** hieu ro hon tinh trang kho mat cua benh nhan va dua ra phac do dieu tri chinh xac.

**Yeu cau lien quan:** CLN-04
**Quyet dinh lien quan:** D-18

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Bac si mo trang chi tiet luot kham -> Section OSDI hien thi: diem tong (score), muc do (Binh thuong/Nhe/Trung binh/Nang)
2. Bac si nhan "Xem chi tiet" -> He thong mo rong hien thi tat ca 12 cau hoi OSDI va cau tra loi cua benh nhan
3. Moi cau hoi hien thi: noi dung cau hoi (tieng Viet), cau tra loi da chon (thang diem 0-4), va y nghia cau tra loi
4. Cau tra loi duoc nhom theo 3 nhom: Trieu chung mat (cau 1-5), Anh huong thi giac (cau 6-9), Yeu to moi truong (cau 10-12)

**Truong hop ngoai le:**
1. Benh nhan chua lam OSDI -> Section hien thi: "Benh nhan chua hoan thanh bo cau hoi OSDI"
2. OSDI chi co diem tong (khong co chi tiet cau tra loi) -> Hien thi diem tong va thong bao: "Chi tiet cau tra loi khong kha dung"

**Truong hop loi:**
1. Loi tai du lieu OSDI -> He thong hien thi toast loi va giu section OSDI voi diem tong

#### Ghi chu ky thuat
- Component: `OsdiAnswersSection.tsx` (da co, can cap nhat de hien thi cau tra loi chi tiet)
- API: GET /api/clinical/visits/{visitId}/osdi-answers (hoac bao gom trong visit detail response)

---

### US-CLN-13-014: Tu dong mo rong section Don kinh khi co du lieu

**La mot** bac si,
**Toi muon** section Don kinh (Optical Prescription) tu dong mo rong khi co du lieu don kinh,
**De** khong phai nhan them vao de xem thong tin don kinh da ke.

**Yeu cau lien quan:** CLN-04
**Quyet dinh lien quan:** D-19

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Bac si mo trang chi tiet luot kham -> He thong kiem tra luot kham co du lieu don kinh hay khong
2. Neu co du lieu don kinh -> Section Don kinh tu dong mo rong (expanded) hien thi day du thong tin
3. Neu khong co du lieu don kinh -> Section Don kinh giu trang thai thu gon (collapsed) nhu hien tai

**Truong hop ngoai le:**
1. Du lieu don kinh trong hoac chi co gia tri mac dinh -> Section giu trang thai thu gon
2. Bac si thu cong thu gon section da tu dong mo rong -> He thong ton trong lua chon cua bac si (cho phep thu gon)

**Truong hop loi:**
1. Loi kiem tra du lieu don kinh -> Section giu trang thai thu gon mac dinh (fail-safe)

#### Ghi chu ky thuat
- Component: `OpticalPrescriptionSection.tsx` - them logic kiem tra du lieu va tu dong set `defaultExpanded={hasData}`
- Kiem tra: opticalPrescription != null && co it nhat 1 truong khong rong (SPH, CYL, AXIS, ADD, PD)

---

### US-CLN-13-015: Cap nhat diem OSDI realtime khi benh nhan hoan thanh

**La mot** bac si,
**Toi muon** nhin thay diem OSDI tu dong cap nhat tren trang chi tiet luot kham khi benh nhan hoan thanh bo cau hoi (khong can tai lai trang),
**De** theo doi ket qua ngay lap tuc ma khong bi gian doan trong quy trinh kham.

**Yeu cau lien quan:** CLN-04
**Quyet dinh lien quan:** D-20

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Bac si dang mo trang chi tiet luot kham cua benh nhan
2. Benh nhan hoan thanh bo cau hoi OSDI (tu thiet bi khac hoac qua link)
3. Diem OSDI tren trang bac si dang xem tu dong cap nhat trong vong 2-3 giay (khong can tai lai trang)
4. Section OSDI hien thi animation nhe (highlight mau) de thu hut su chu y cua bac si

**Truong hop ngoai le:**
1. Bac si dang o trang khac khi OSDI duoc cap nhat -> Du lieu OSDI moi se hien thi khi bac si quay lai trang chi tiet
2. Ket noi mang bi gian doan -> He thong tu dong ket noi lai va dong bo du lieu OSDI moi nhat

**Truong hop loi:**
1. Loi nhan tin hieu realtime -> He thong ghi log loi va bac si co the tai lai trang de cap nhat thu cong
2. Du lieu OSDI khong dong bo -> He thong hien thi toast canh bao: "Du lieu OSDI co the chua cap nhat. Vui long tai lai trang"

#### Ghi chu ky thuat
- SignalR hub: gui event `OsdiCompleted` khi benh nhan hoan thanh bo cau hoi
- Frontend: lang nghe SignalR event va cap nhat React Query cache cho visit detail
- Invalidate query key: `['visit', visitId, 'osdi']` khi nhan event

---

*Tai lieu nay duoc tao theo chuan DOC-01 cho Phase 13: Clinical Workflow Overhaul.*
*Cap nhat: 2026-03-25*
