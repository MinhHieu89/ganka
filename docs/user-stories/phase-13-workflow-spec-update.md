# User Stories: Cap Nhat Quy Trinh Kham Day Du (Full Workflow Spec Implementation)

**Phase:** 13 - Clinical Workflow Overhaul (Plans 09-17)
**Ngay tao:** 2026-03-25
**Yeu cau lien quan:** CLN-03, CLN-04
**So luong user stories:** 14

---

## CLN-04: Tien Trinh Giai Doan — Cac Giai Doan Moi

### US-CLN-13-201: Bo qua buoc do khuc xa voi ly do (Stage 2 Skip Path)

**La mot** ky thuat vien do khuc xa (KTV),
**Toi muon** bo qua buoc do khuc xa/thi luc khi khong can thiet, voi ly do bat buoc va kha nang hoan tac,
**De** benh nhan tai kham hoac kham tong quat khong phai mat thoi gian do lai.

**Yeu cau lien quan:** CLN-04
**Tham chieu spec:** Stage 2 — Refraction / Visual Acuity, Skip Path

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. KTV mo chi tiet luot kham o giai doan 2 -> Thay nut `Bo qua buoc nay` (vien do, luon hien thi) phia duoi cung
2. KTV nhan `Bo qua buoc nay` -> He thong hien thi modal voi tieu de "Bo qua do khuc xa / thi luc?"
3. Modal hien thi cac chip ly do bat buoc (chon 1): `Tai kham, da co ket qua cu` / `Benh nhan tu choi do` / `Kham tong quat, khong lien quan khuc xa` / `Khac`
4. Nut xac nhan bi vo hieu hoa cho den khi chon chip -> KTV chon chip va (tuy chon) nhap ghi chu tu do (toi da 200 ky tu voi bo dem)
5. KTV nhan `Xac nhan bo qua` -> Form bi lam mo (opacity giam) va vo hieu hoa
6. He thong hien thi banner vang: "Da bo qua do khuc xa — Ly do: [ly do] · [KTV] · [thoi gian]" voi nut `Hoan tac`
7. Stage pill o thanh tren doi thanh mau ho phach: "Da bo qua"
8. Thanh duoi cung doi thanh: `Luu nhap` + `Chuyen bac si >`

**Truong hop ngoai le:**
1. KTV nhan `Hoan tac` tren banner vang -> He thong khoi phuc form ve trang thai binh thuong, banner bien mat
2. Hoan tac chi co the thuc hien truoc khi chuyen benh nhan sang bac si
3. KTV da chuyen benh nhan sang bac si -> Nut `Hoan tac` khong con hien thi

**Truong hop loi:**
1. Loi luu trang thai bo qua -> He thong hien thi toast loi va giu form o trang thai binh thuong
2. Loi hoan tac -> He thong hien thi toast loi va giu trang thai bo qua

#### Ghi chu ky thuat
- Skip modal: chip ly do (single-select), truong ghi chu tuy chon (max 200 chars), nut `Huy` / `Xac nhan bo qua`
- Domain: `SkipRefraction(string reason, string? note, Guid skippedBy)` tren Visit entity
- Audit trail: ghi nhan nguoi bo qua, ly do, thoi gian

---

### US-CLN-13-202: Bac si quyet dinh luong kham — ke don truc tiep hoac chuyen CDHA (Stage 3 Branch Decision)

**La mot** bac si,
**Toi muon** chon giua ke don truc tiep hoac chuyen benh nhan di chan doan hinh anh truoc khi ke don,
**De** linh hoat xu ly tung truong hop lam sang theo nhu cau thuc te.

**Yeu cau lien quan:** CLN-04
**Tham chieu spec:** Stage 3 — Doctor Examination, Branch Decision

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Bac si mo chi tiet luot kham o giai doan 3 -> Thay tom tat khuc xa (read-only) hoac canh bao bo qua (banner ho phach)
2. Bac si nhap chan doan ICD-10 bang o tim kiem typeahead -> Chon it nhat 1 ma ICD-10
3. Cac ma da chon hien thi dang pill tags mau xanh ngoc (teal) phia duoi o tim kiem
4. Thanh duoi cung co 3 nut: `Luu nhap` | `Chuyen CDHA >` (outlined, luon bat) | `Ke don >` (den, bat khi co ICD-10)
5. **Luong A:** Bac si nhan `Ke don >` -> Benh nhan chuyen thang den giai doan 5 (Ke don)
6. **Luong B:** Bac si nhan `Chuyen CDHA >` -> Benh nhan chuyen sang giai doan 4a (Chan doan hinh anh)

**Truong hop ngoai le:**
1. Giai doan 2 da bi bo qua -> Thay the tom tat khuc xa bang canh bao ho phach: "Da bo qua do — Ly do: [ly do] · [KTV] · [thoi gian]"
2. Bac si nhan `Chuyen CDHA >` khi chua co ICD-10 -> Van cho phep (co the can hinh anh truoc khi chan doan)
3. Sau khi da chuyen CDHA, bac si khong the nhay truc tiep tu giai doan 3 sang giai doan 5 -> Phai qua 4a -> 4b truoc

**Truong hop loi:**
1. Loi tim kiem ICD-10 -> He thong hien thi toast loi va giu trang thai hien tai
2. Loi chuyen giai doan -> He thong hien thi toast loi va benh nhan o lai giai doan 3

#### Ghi chu ky thuat
- ICD-10 typeahead: hien thi toi da 5 ket qua, ma ben phai, ten ben trai
- Tom tat khuc xa: 2 cot OD/OS voi SPH, CYL, AXIS, VA khong kinh, VA co kinh (read-only)
- Validation: "Chan doan bat buoc — chon it nhat 1 ma ICD-10 de tiep tuc"

---

### US-CLN-13-203: KTV CDHA thuc hien dich vu va tra ket qua (Stage 4a Imaging)

**La mot** ky thuat vien chan doan hinh anh (KTV CDHA),
**Toi muon** thuc hien cac dich vu chan doan hinh anh theo yeu cau cua bac si, tai len ket qua va tra ve cho bac si,
**De** bac si co du lieu hinh anh can thiet de chan doan chinh xac.

**Yeu cau lien quan:** CLN-04
**Tham chieu spec:** Stage 4a — Imaging & Diagnostics (CDHA)

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. KTV CDHA mo chi tiet luot kham o giai doan 4a -> Thay banner xanh duong hien thi ghi chu cua bac si, thoi gian, danh sach dich vu yeu cau
2. Danh sach dich vu hien thi dang checklist -> KTV tich tung dong khi hoan thanh dich vu
3. KTV tai len hinh anh/video: chon loai hinh anh (dropdown) + mat (OD/OS/Ca hai) + keo tha file vao drop zone
4. File duoc chap nhan: JPEG, PNG, TIFF, DICOM, MP4, MOV, AVI (toi da 50 MB/file)
5. Hinh anh da tai len hien thi dang thumbnail 3 cot voi nhan loai, mat, va checkmark xanh
6. KTV nhan `Tra ket qua cho bac si >` -> Benh nhan chuyen sang giai doan 4b

**Truong hop ngoai le:**
1. Khong phai tat ca dich vu da hoan thanh -> Hien thi canh bao ho phach: "X dich vu chua thuc hien — ban van co the tra ket qua ngay"
2. KTV them dich vu ngoai yeu cau -> Nhan `+ Them dich vu` o cuoi danh sach
3. KTV them ghi chu tu do cho bac si -> Truong ghi chu tuy chon o cuoi trang

**Truong hop loi:**
1. Loi tai len file -> He thong hien thi toast loi va cho phep thu lai
2. File vuot qua gioi han 50 MB -> He thong tu choi va hien thi thong bao kich thuoc toi da
3. Loi chuyen giai doan -> He thong hien thi toast loi va giu trang thai hien tai

#### Ghi chu ky thuat
- Banner yeu cau: xanh duong, ghim o tren cung, hien thi ten bac si, thoi gian, danh sach dich vu
- Nut `Tra ket qua cho bac si >` vo hieu hoa khi chua tai len bat ky file nao
- Trang thai badge dich vu: `Chua thuc hien` (xam) -> `Hoan tat` (xanh)

---

### US-CLN-13-204: Bac si doc ket qua chan doan hinh anh (Stage 4b Doctor Review)

**La mot** bac si,
**Toi muon** xem ket qua hinh anh, cap nhat chan doan neu can, va tiep tuc ke don,
**De** ra quyet dinh dieu tri dua tren day du thong tin lam sang va hinh anh.

**Yeu cau lien quan:** CLN-04
**Tham chieu spec:** Stage 4b — Doctor Reviews Imaging Results

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Bac si mo chi tiet luot kham o giai doan 4b -> Thay luoi thumbnail hinh anh (3 cot) voi mau nen theo loai (xanh = OCT, cam = Fundus, xanh la = tiet doan truoc)
2. Bac si nhan vao thumbnail -> Xem truoc inline (16:9) hien thi ngay duoi luoi, khong mo modal/tab moi
3. Gia tri IOP (neu co) hien thi phia duoi phan xem truoc: "IOP: OD 14 mmHg · OS 15 mmHg"
4. Ghi chu cua KTV hien thi trong the xanh duong rieng biet (nen #E6F1FB)
5. Chan doan ICD-10 tu giai doan 3 hien thi dang pill tags xanh ngoc (teal)
6. Bac si co the them ma ICD-10 moi -> Pill tags moi hien thi mau ho phach voi badge `new`
7. Nhat ky thay doi (them/xoa) hien thi tu dong khi co sua doi
8. Bac si nhan `Xac nhan, tiep tuc ke don >` -> Chuyen sang giai doan 5

**Truong hop ngoai le:**
1. Bac si khong thay doi chan doan -> Tieu de section hien thi "khong thay doi", van chuyen tiep binh thuong
2. Bac si xoa mot ma ICD-10 cu -> Nhat ky thay doi hien thi dot do (da xoa)

**Truong hop loi:**
1. Loi tai hinh anh -> He thong hien thi placeholder "Khong the tai hinh anh"
2. Loi chuyen giai doan -> He thong hien thi toast loi va giu trang thai hien tai

#### Ghi chu ky thuat
- Nut `Xac nhan, tiep tuc ke don >` luon bat (bac si da xem ket qua khi mo trang)
- Nhat ky thay doi: chi doc, hien thi dot xanh (them) va dot do (xoa)
- Thanh duoi: `Luu nhap` + `Xac nhan, tiep tuc ke don >`

---

### US-CLN-13-205: Bac si ke don thuoc va kinh, ky duyet (Stage 5 Prescription)

**La mot** bac si,
**Toi muon** ke don thuoc va/hoac don kinh, xem lai tom tat va ky duyet de khoa ho so,
**De** hoan thanh phan lam sang va chuyen benh nhan sang thu ngan.

**Yeu cau lien quan:** CLN-04
**Tham chieu spec:** Stage 5 — Prescription

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Bac si mo chi tiet luot kham o giai doan 5 -> Thay phan ke don thuoc va phan ke don kinh
2. **Don thuoc:** Bac si them dong thuoc (ten thuoc typeahead, so luong, huong dan, ghi chu tuy chon), co the them nhieu dong voi nut `+ Them thuoc`
3. **Don kinh:** Gia tri SPH, CYL, AXIS, ADD, PD tu dong dien tu ket qua do khuc xa giai doan 2, vien teal, bac si co the sua bat ky truong nao. 3 tab loai kinh: `Kinh don trong` / `Kinh luy tien` / `Kinh ap trong`
4. Bac si nhan `Ky duyet luot kham` -> He thong hien thi modal xac nhan voi: so ma ICD-10, so thuoc, loai don kinh, so hinh anh, trang thai ghi chu, ten bac si va thoi gian
5. Bac si nhan `Xac nhan ky duyet` -> Ho so bi khoa
6. **Trang thai khoa:** Form lam mo (opacity ~75%), moi tieu de section co hau to "— locked", banner xanh tren cung voi icon khoa, thanh duoi chi con nut in: `In giay chuyen vien` / `In phieu dong y`
7. He thong tu dong chuyen benh nhan sang giai doan 6 (Thu ngan) — khong can nhan nut chuyen

**Truong hop ngoai le:**
1. Bac si chua nhap don thuoc va don kinh -> Nut `Ky duyet luot kham` vo hieu hoa voi thong bao: "Can it nhat mot don thuoc hoac don kinh"
2. Bac si nhan `Kiem tra lai` trong modal -> Dong modal, quay lai form, khong mat du lieu
3. Truong kinh khong ap dung cho loai da chon -> Truong vo hieu hoa va hien thi `—`

**Truong hop loi:**
1. Loi ky duyet -> He thong hien thi toast loi va giu form o trang thai chinh sua
2. Loi tu dong chuyen giai doan -> He thong giu trang thai ky ten, hien thi canh bao: "Da ky nhung chua chuyen tu dong — vui long chuyen thu cong"

#### Ghi chu ky thuat
- Auto-transition: sau ky duyet, phat domain event `VisitSignedOffEvent` -> tu dong chuyen sang giai doan 6
- Don kinh pre-fill: tu RefractionData cua visit, truong co gia tri hien thi vien teal
- Modal tom tat: bang 6 dong (ICD-10, thuoc, kinh, hinh anh, ghi chu, bac si+thoi gian)

---

### US-CLN-13-206: Thu ngan thanh toan luot kham va dinh tuyen tu dong (Stage 6 Visit Payment)

**La mot** thu ngan,
**Toi muon** xem hoa don tu dong tao, thu tien va he thong tu dong dinh tuyen benh nhan den nha thuoc/quang,
**De** benh nhan duoc chuyen tiep nhanh chong ma khong can thao tac thu cong.

**Yeu cau lien quan:** CLN-03, CLN-04
**Tham chieu spec:** Stage 6 — Cashier (Visit Payment)

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Thu ngan mo chi tiet luot kham o giai doan 6 -> Hoa don tu dong hien thi voi cac muc: dich vu kham, dich vu CDHA, thuoc
2. Co nut toggle `Tach rieng tien kinh` (mac dinh BAT khi co don kinh) -> Khi bat: chi thanh toan kham+thuoc+CDHA, kinh thanh toan rieng sau
3. Thu ngan nhan `Thu tien >` -> Chon phuong thuc (tien mat / the / chuyen khoan) trong luoi 3 cot
4. Nhap so tien nhan -> He thong tinh va hien thi tien thua thoi gian thuc (xanh) hoac con thieu (do)
5. Thu ngan nhan `Xac nhan da thu tien` -> He thong xac nhan thanh toan
6. **Dinh tuyen tu dong:** He thong kiem tra don va chuyen benh nhan tuong ung:
   - Co don thuoc -> Gui tu dong sang giai doan 7a (Nha thuoc)
   - Co don kinh -> Gui tu dong sang giai doan 7b (Trung tam kinh)
   - Ca hai -> Gui dong thoi ca hai, chay song song
   - Khong co -> Luot kham hoan thanh ngay

**Truong hop ngoai le:**
1. Toggle `Tach rieng tien kinh` TAT -> Tien kinh (neu da biet gia) tinh vao tong hoa don
2. Benh nhan co the thanh thien VIP -> Giam gia tu dong ap dung tren hoa don
3. Khong co don thuoc va don kinh -> Luot kham hoan thanh ngay sau thu tien, khong co giai doan tiep

**Truong hop loi:**
1. Loi xac nhan thanh toan -> He thong hien thi toast loi va giu trang thai hien tai
2. Loi dinh tuyen tu dong -> He thong ghi log loi va hien thi canh bao de nhan vien chuyen thu cong

#### Ghi chu ky thuat
- Hoa don nhom theo: Dich vu kham / Dich vu CDHA / Thuoc, moi dong co: ten, ghi chu, so luong, gia
- Post-payment: hien thi man hinh thanh cong voi banner xanh, the "Buoc tiep theo" voi badge `Dang cho` xanh
- Thanh duoi sau thanh toan: `In hoa don` + `Xuat bien lai` + `Tiep nhan benh nhan moi`

---

### US-CLN-13-207: Duoc si phat thuoc theo don (Stage 7a Pharmacy Dispensing)

**La mot** duoc si,
**Toi muon** xem danh sach thuoc can phat va tich tung dong khi phat xong,
**De** dam bao phat day du thuoc theo don va ghi nhan chinh xac.

**Yeu cau lien quan:** CLN-04
**Tham chieu spec:** Stage 7a — Pharmacy (Parallel)

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Duoc si mo chi tiet o giai doan 7a -> Thay danh sach thuoc can phat dang checklist
2. Moi dong thuoc hien thi: ten thuoc, so luong, huong dan su dung, va checkbox ben phai
3. Duoc si tich dong khi da phat -> Dong chuyen nen xanh voi checkmark
4. Nut `Da phat du thuoc` vo hieu hoa cho den khi tat ca dong deu duoc tich
5. Duoc si nhan `Da phat du thuoc` -> Track A hoan thanh, hien thi banner xanh "Done" voi ten duoc si va thoi gian
6. Thanh duoi sau hoan thanh chi con: `In nhan thuoc`

**Truong hop ngoai le:**
1. Duoc si them ghi chu phat thuoc o truong ghi chu tu do o cuoi trang (vd: "Bao quan lanh", "Uong sau an")
2. Khong the phat mot phan -> Tat ca dong phai duoc tich truoc khi hoan thanh

**Truong hop loi:**
1. Loi luu trang thai phat thuoc -> He thong hien thi toast loi va giu trang thai hien tai
2. Loi hoan thanh track -> He thong hien thi toast loi va giu trang thai chua hoan thanh

#### Ghi chu ky thuat
- Dieu kien vao: Thu ngan da xac nhan thanh toan giai doan 6
- Chay song song voi giai doan 7b (Trung tam kinh) neu co don kinh
- Checkbox toggle cho phep tich va bo tich (ca hai huong)

---

### US-CLN-13-208: Tu van vien chon gong kinh va thau kinh (Stage 7b Optical Center)

**La mot** tu van vien quang hoc,
**Toi muon** xem don kinh cua bac si, chon loai thau kinh va ma gong, tinh gia va chuyen sang thu tien kinh,
**De** benh nhan co don kinh day du truoc khi thanh toan.

**Yeu cau lien quan:** CLN-04
**Tham chieu spec:** Stage 7b — Optical Center Frame Consultation (Parallel)

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Tu van vien mo chi tiet o giai doan 7b -> Thay don kinh tu giai doan 5 dang the read-only: SPH, CYL, AXIS theo mat, PD, ghi chu cua bac si
2. Tu van vien chon loai thau kinh tu dropdown (vd: Essilor Crizal 1.60, Hoya HD 1.60)
3. Tu van vien nhap ma gong (truong van ban tu do)
4. Khi ca hai truong da dien -> Hien thi bang gia: thau kinh (x2) + gong = tong
5. Tu van vien nhan `Xac nhan don, thu tien kinh >` -> Chuyen sang giai doan 8

**Truong hop ngoai le:**
1. Chua chon thau kinh hoac chua nhap ma gong -> Nut `Xac nhan don, thu tien kinh >` vo hieu hoa
2. Don kinh chi co mot mat -> He thong van hien thi ca hai mat, mat khong co de trong

**Truong hop loi:**
1. Loi luu don kinh -> He thong hien thi toast loi va giu trang thai hien tai
2. Loi chuyen giai doan -> He thong hien thi toast loi

#### Ghi chu ky thuat
- Don kinh read-only tu PrescriptionData cua visit
- Chay song song voi giai doan 7a (Nha thuoc) neu co don thuoc
- Sau xac nhan: hien thi tom tat don hang, tong gia, trang thai "Done"

---

### US-CLN-13-209: Thu ngan thu tien kinh (Stage 8 Glasses Payment)

**La mot** thu ngan,
**Toi muon** thu tien kinh sau khi trung tam kinh xac nhan don hang,
**De** benh nhan thanh toan day du truoc khi kinh duoc gia cong.

**Yeu cau lien quan:** CLN-04
**Tham chieu spec:** Stage 8 — Cashier (Glasses Payment)

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Thu ngan mo chi tiet o giai doan 8 -> Thay tom tat don kinh va tong gia tu giai doan 7b
2. Quy trinh thu tien giong giai doan 6: chon phuong thuc (tien mat / the / chuyen khoan), nhap so tien nhan, xem tien thua thoi gian thuc
3. Thu ngan nhan `Xac nhan da thu — chuyen mai kinh >` -> He thong tu dong thong bao giai doan 9 (Ky thuat mai lap)

**Truong hop ngoai le:**
1. Giai doan 7b chua hoan thanh -> Cot hien thi placeholder: "Dang cho trung tam kinh xac nhan gong va thau kinh" voi nut vo hieu hoa `Cho trung tam kinh...`
2. Giai doan 7b vua hoan thanh -> Cot tu dong cap nhat hien thi don hang va cho phep thu tien

**Truong hop loi:**
1. Loi thu tien -> He thong hien thi toast loi va giu trang thai hien tai
2. Loi thong bao giai doan 9 -> He thong ghi log loi va hien thi canh bao

#### Ghi chu ky thuat
- Bi chan boi giai doan 7b (khong co gia cho den khi chon gong)
- Quy trinh thanh toan giong giai doan 6 (phuong thuc, so tien, tien thua)
- Sau thanh toan: tu dong thong bao Optical Lab (giai doan 9)

---

### US-CLN-13-210: Ky thuat vien mai lap kinh va kiem tra chat luong (Stage 9 Optical Lab)

**La mot** ky thuat vien mai lap kinh,
**Toi muon** nhan don kinh, gia cong va hoan thanh checklist chat luong truoc khi tra kinh cho benh nhan,
**De** dam bao kinh dat tieu chuan truoc khi giao.

**Yeu cau lien quan:** CLN-04
**Tham chieu spec:** Stage 9 — Optical Lab: Lens Grinding & Fitting

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. KTV mai lap mo chi tiet o giai doan 9 -> Thay the tham chieu read-only: don kinh day du (SPH, CYL, AXIS, PD theo mat) va lua chon gong/thau tu giai doan 7b
2. Checklist chat luong hien thi cac hang kiem tra -> KTV tich tung dong khi hoan thanh
3. Nut `Kinh san sang, tra cho benh nhan >` vo hieu hoa cho den khi tat ca dong duoc tich
4. KTV nhan `Kinh san sang, tra cho benh nhan >` -> Giai doan 10 (Tra kinh) duoc kich hoat

**Truong hop ngoai le:**
1. Khong the hoan thanh mot phan checklist -> Tat ca dong phai duoc tich truoc khi chuyen tiep
2. KTV can xem lai don kinh -> The tham chieu luon hien thi o tren cung

**Truong hop loi:**
1. Loi luu trang thai checklist -> He thong hien thi toast loi va giu trang thai hien tai
2. Loi kich hoat giai doan 10 -> He thong hien thi toast loi

#### Ghi chu ky thuat
- Dieu kien vao: Thu tien kinh giai doan 8 da xac nhan
- Checklist tuong tu giai doan 7a (checkbox, tat ca bat buoc)
- Sau hoan thanh: giai doan 10 tu dong nhan thong bao

---

### US-CLN-13-211: Tra kinh cho benh nhan va hoan tat luot kham (Stage 10 Return Glasses)

**La mot** thu ngan / le tan,
**Toi muon** kiem tra kinh theo checklist ban giao, xac nhan benh nhan da nhan kinh va dong ho so,
**De** dam bao kinh dung don, chat luong tot va benh nhan hai long truoc khi ket thuc.

**Yeu cau lien quan:** CLN-04
**Tham chieu spec:** Stage 10 — Return Glasses to Patient

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Nhan vien mo chi tiet o giai doan 10 -> Thay checklist ban giao 3 muc:
   - Kiem tra do kinh dung (hien thi gia tri SPH OD/OS inline de doi chieu nhanh)
   - Gong kinh dung (khop ma don, khong tray xuoc)
   - Benh nhan da thu kinh va xac nhan vua
2. Moi muc la dong checkbox — tat ca 3 bat buoc
3. Nhan vien tich ca 3 muc -> Nut `Hoan tat — da tra kinh` chuyen sang trang thai xanh bat
4. Nhan vien nhan `Hoan tat — da tra kinh` -> Ho so luot kham dong
5. Giao dien 3 cot hien thi banner tong ket: tong so tien da thu (tat ca giai doan), tong thoi gian luot kham, ngay
6. Thanh duoi: `In phieu bao hanh` + `Dong ho so` (nut den chinh)

**Truong hop ngoai le:**
1. Track A (thuoc) chua hoan thanh -> Luot kham chua hoan tat du track B da xong — doi ca hai track
2. Track A da hoan thanh truoc -> Luot kham hoan tat ngay khi giai doan 10 xong

**Truong hop loi:**
1. Loi dong ho so -> He thong hien thi toast loi va giu trang thai hien tai
2. Loi in phieu bao hanh -> He thong hien thi toast loi nhung van cho phep dong ho so

#### Ghi chu ky thuat
- Checklist bang giao: 3 muc co dinh, tat ca bat buoc, khong cho phep ban giao mot phan
- Hien thi gia tri don kinh inline trong dong 1 de nhan vien doi chieu nhanh
- Visit complete = ca hai track (A va B) da hoan tat

---

### US-CLN-13-212: Quan ly luong song song — thuoc va kinh phai hoan thanh (Parallel Track Management)

**La mot** he thong,
**Toi muon** quan ly hai luong cong viec song song (phat thuoc va gia cong kinh) mot cach doc lap, va chi danh dau luot kham hoan tat khi ca hai luong da xong,
**De** benh nhan co the nhan thuoc ngay ma khong phai doi kinh, va nguoc lai.

**Yeu cau lien quan:** CLN-03, CLN-04
**Tham chieu spec:** Parallel Execution Notes

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. **Case A — Chi co thuoc:** Sau giai doan 6, chi Track A (Nha thuoc) duoc kich hoat -> Luot kham hoan tat khi phat du thuoc
2. **Case B — Chi co kinh:** Sau giai doan 6, chi Track B (7b -> 8 -> 9 -> 10) duoc kich hoat -> Luot kham hoan tat khi tra kinh
3. **Case C — Ca thuoc va kinh:** Sau giai doan 6, ca Track A va Track B chay song song -> Luot kham hoan tat khi CA HAI track da xong
4. **Case D — Khong co thuoc va kinh:** Sau giai doan 6, luot kham hoan tat ngay lap tuc
5. Track A va Track B khong chan nhau — benh nhan co the nhan thuoc trong khi kinh dang gia cong

**Truong hop ngoai le:**
1. Track A xong truoc Track B -> He thong ghi nhan Track A hoan tat, doi Track B
2. Track B xong truoc Track A -> He thong ghi nhan Track B hoan tat, doi Track A
3. Mot track bi loi -> Track con lai van tiep tuc binh thuong

**Truong hop loi:**
1. Loi kiem tra trang thai track -> He thong ghi log loi va hien thi canh bao tren kanban

#### Ghi chu ky thuat
- Domain: Visit entity co TrackA_Completed va TrackB_Completed flags
- Visit hoan tat khi: (khong co track nao) HOAC (tat ca track active da completed)
- Sau giai doan 6, he thong tao cac track dua tren noi dung don tu giai doan 5

---

## CLN-03: Kanban Board — Cot Dieu Kien va The Bai

### US-CLN-13-213: Hien thi cot kanban co dieu kien theo nhu cau (Conditional Column Visibility)

**La mot** nhan vien phong kham,
**Toi muon** chi thay cac cot kanban can thiet — cac cot cho giai doan khong co benh nhan nao se tu dong an di,
**De** bang kanban gon gang va de theo doi hon.

**Yeu cau lien quan:** CLN-03
**Tham chieu spec:** Kanban Board Design, Column Visibility Rules

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Nhan vien mo Dashboard lam sang -> He thong hien thi cac cot kanban theo quy tac:
   - `Do khuc xa / Thi luc`, `Chan doan hinh anh`, `Bac si doc KQ`: **Luon hien thi** (co the trong)
   - `Nha thuoc`: Chi hien thi khi co it nhat 1 benh nhan trong phien co don thuoc
   - `Trung tam kinh`: Chi hien thi khi co it nhat 1 benh nhan co don kinh
   - `Thu ngan (kinh)`: Chi hien thi khi co it nhat 1 benh nhan co don kinh
   - `Ky thuat mai lap`: Chi hien thi khi co it nhat 1 benh nhan co don kinh
   - `Tra kinh`: Chi hien thi khi co it nhat 1 benh nhan co don kinh
2. Khi benh nhan dau tien co don thuoc xuat hien -> Cot `Nha thuoc` tu dong hien thi
3. Khi benh nhan dau tien co don kinh xuat hien -> Cac cot kinh tu dong hien thi

**Truong hop ngoai le:**
1. Tat ca benh nhan co don thuoc da hoan tat -> Cot `Nha thuoc` van hien thi (vi co benh nhan trong phien)
2. Khong co benh nhan nao co don kinh trong phien -> Cac cot kinh an di, bang kanban gon hon

**Truong hop loi:**
1. Loi kiem tra dieu kien hien thi -> Mac dinh hien thi tat ca cot (fail-safe)

#### Ghi chu ky thuat
- Client-side logic: dem so benh nhan co don thuoc/kinh trong phien hien tai
- Cot an hoan toan (khong chiem khong gian) khi khong co benh nhan tuong ung
- Cap nhat thoi gian thuc qua SignalR khi trang thai benh nhan thay doi

---

### US-CLN-13-214: Thiet ke lai the benh nhan tren kanban (Card Anatomy Redesign)

**La mot** nhan vien phong kham,
**Toi muon** the benh nhan tren kanban hien thi ro rang: ten benh nhan, bac si, thoi gian cho (voi dong ho dem song va dot trang thai), trang thai dac biet,
**De** nhanh chong nam bat thong tin benh nhan ma khong can mo chi tiet.

**Yeu cau lien quan:** CLN-03
**Tham chieu spec:** Kanban Board Design, Card Anatomy, Card State Variants

#### Tieu chi chap nhan

**Luong chinh (Happy Path):**
1. Nhan vien nhin thay the benh nhan tren kanban voi bo cuc:
   - **Ten benh nhan:** In dam, 14px
   - **Bac si phu trach:** Van ban phu, mau nhat
   - **Thoi gian hen:** Canh phai, van ban phu
   - **Dong ho cho:** Bo dem song voi dot trang thai mau ho phach (dang cho)
   - **Stage pill:** Goc tren phai cua detail view topbar, hien thi trang thai ho so
2. Chi giai doan 1 (Tiep nhan) co nut `Chuyen tiep >` tren the. Tat ca giai doan khac hien thi "Nhan de xem chi tiet ->" (mau nhat)
3. Toan bo the co the nhan vao de mo chi tiet

**Truong hop ngoai le:**
1. **Giai doan 2 da bo qua:** Stage pill chuyen ho phach: `Da bo qua`
2. **Giai doan 5 da ky:** Stage pill chuyen xanh: `Da ky duyet`
3. **Giai doan 6 da thanh toan:** Stage pill chuyen xanh: `Da thanh toan`
4. **Giai doan 10 hoan tat:** Stage pill chuyen xanh: `Hoan tat`
5. **Trang thai binh thuong:** Stage pill xam voi ten giai doan (vd: `Bac si kham`)

**Truong hop loi:**
1. Loi tai thong tin the -> He thong hien thi the voi ten benh nhan va thong bao loi
2. Loi dong ho cho -> He thong hien thi "—" thay vi thoi gian

#### Ghi chu ky thuat
- Dot trang thai: ho phach = dang cho (mac dinh), khong thay doi tren the kanban — trang thai dac biet chi hien thi qua stage pill
- Stage pill chi hien thi tren topbar cua detail view, khong phai tren the kanban
- Nut `Chuyen tiep >` chi o giai doan 1 (Reception) — ngoai le duy nhat cho phep nut tren the

---

*Tai lieu nay duoc tao theo chuan DOC-01 cho Phase 13: Clinical Workflow Overhaul (Plans 09-17).*
*Yeu cau lien quan: CLN-03, CLN-04*
*Cap nhat: 2026-03-25*
