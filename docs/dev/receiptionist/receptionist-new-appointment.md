# SCR-004 | Đặt lịch hẹn (New appointment form)

**Route:** `/dashboard` → Modal / Page mở khi bấm "Đặt lịch hẹn"

**Actor:** Lễ tân (chính), Admin

**Mục tiêu:** Cho phép lễ tân đặt lịch hẹn nhanh khi BN gọi điện đến phòng khám. Form phải tối ưu cho tốc độ — lễ tân vừa nói chuyện điện thoại vừa thao tác: tay trái ghi thông tin, mắt phải nhìn slot trống.

---

## 1. Mô tả chức năng

Form đặt lịch hẹn phục vụ 2 scenario:

- **BN cũ gọi hẹn tái khám:** Đã có hồ sơ trong hệ thống. Lễ tân search SĐT/tên → hệ thống load thông tin sẵn → chỉ cần chọn ngày giờ + xác nhận lý do.
- **BN mới gọi hẹn lần đầu:** Chưa có hồ sơ. Lễ tân nhập thông tin tối thiểu (tên, SĐT, lý do) + chọn ngày giờ. Thông tin đầy đủ (ngày sinh, địa chỉ, tiền sử...) sẽ bổ sung khi BN đến check-in.

Form này KHÔNG dùng cho BN walk-in (dùng form Tiếp nhận BN mới SCR-003). Form này cũng KHÔNG phải form đặt hẹn trên trang web public (đó là giao diện riêng cho BN tự đặt).

Sau khi lưu, appointment xuất hiện trên Dashboard vào đúng ngày hẹn với trạng thái "Chưa đến".

---

## 2. Khi nào mở form này

- Lễ tân bấm nút **"Đặt lịch hẹn"** (outline tím) trên Dashboard.
- BN gọi điện đến phòng khám muốn đặt lịch.
- Bác sĩ yêu cầu lễ tân đặt lịch tái khám cho BN (sau khi khám xong).

---

## 3. Layout tổng quan

Form chia 2 cột:

| Cột trái | Cột phải |
|----------|----------|
| Thông tin bệnh nhân | Chọn ngày & giờ |
| Ô tìm kiếm BN | Lịch mini (calendar) |
| Thông tin BN (read-only hoặc input) | Grid khung giờ |
| Lý do khám | Legend |
| Bác sĩ chỉ định | |
| Ghi chú | |

**Header:** Breadcrumb "← Dashboard / Đặt lịch hẹn" + ngày hiện tại.

**Thanh xác nhận:** Nằm trước footer, tóm tắt thông tin hẹn (tên BN, ngày giờ, lý do, BS) để lễ tân đọc lại cho BN qua điện thoại.

**Footer:** 2 nút — Hủy, Xác nhận đặt hẹn.

---

## 4. Chi tiết cột trái — Thông tin bệnh nhân

### 4.1 Ô tìm kiếm BN

| Thuộc tính | Chi tiết |
|------------|----------|
| Vị trí | Đầu cột trái, ngay dưới tiêu đề section |
| Kiểu | Text input + icon search |
| Placeholder | "Gõ SĐT hoặc tên để tìm..." |
| Hành vi | Autocomplete real-time (sau 2 ký tự), search toàn bộ database BN |

**Khi tìm thấy BN cũ:**
- Hiện thanh xanh lá: "Đã tìm thấy: **Lê Minh Châu** (1978) — BN-20260110-0045"
- Chuyển sang trạng thái BN cũ (section 4.2)

**Khi không tìm thấy:**
- Hiện thanh vàng: "Không tìm thấy BN với SĐT này. Nhập thông tin bên dưới để tạo hẹn cho BN mới."
- Chuyển sang trạng thái BN mới (section 4.3)

### 4.2 Trạng thái: BN cũ (đã tìm thấy hồ sơ)

Thông tin BN hiển thị dạng **card read-only** (nền xám, không cho sửa):

| Thông tin | Nguồn |
|-----------|-------|
| Họ tên | Từ database |
| SĐT | Từ database |
| Năm sinh | Từ database |
| Giới tính | Từ database |
| Lần khám gần nhất | Ngày + chẩn đoán + BS khám. Giúp lễ tân và BN nhắc lại context. |

Fields nhập thêm:

| Field | Kiểu | Bắt buộc | Ghi chú |
|-------|------|----------|---------|
| Lý do khám | Textarea | Có | Placeholder gợi ý: "Tái khám dry eye, kiểm tra mắt định kỳ..." |
| Bác sĩ chỉ định | Dropdown | Không | Mặc định = BS lần khám trước. Options: danh sách BS + "Không chỉ định (BS nào trống)" |
| Ghi chú | Text input | Không | VD: "BN cần đến sớm 15p để đo mắt trước" |

### 4.3 Trạng thái: BN mới (không tìm thấy hồ sơ)

Fields nhập tay (nền trắng, editable):

| Field | Kiểu | Bắt buộc | Validation | Ghi chú |
|-------|------|----------|------------|---------|
| Họ tên | Text input | Có | Không rỗng, tối đa 100 ký tự | |
| SĐT | Phone input | Có | 10–11 số, định dạng VN | Auto-fill từ ô search |
| Lý do khám | Textarea | Có | Không rỗng, tối đa 500 ký tự | |
| Bác sĩ chỉ định | Dropdown | Không | | Mặc định = "Không chỉ định (BS nào trống)" |
| Ghi chú | Text input | Không | Tối đa 300 ký tự | |

**Lưu ý quan trọng:** BN mới chỉ cần 3 thông tin tối thiểu: tên, SĐT, lý do khám. KHÔNG yêu cầu ngày sinh, địa chỉ, tiền sử... Những thông tin này sẽ bổ sung khi BN đến check-in (lễ tân mở form Intake đầy đủ lúc đó).

Hiển thị note xanh dương ở cuối cột trái: "BN mới chỉ cần tên + SĐT + lý do khám để đặt hẹn. Thông tin đầy đủ sẽ bổ sung khi BN đến check-in."

---

## 5. Chi tiết cột phải — Chọn ngày & giờ

### 5.1 Lịch mini (Calendar)

| Thuộc tính | Chi tiết |
|------------|----------|
| Kiểu | Month calendar, hiện 1 tháng |
| Navigation | Mũi tên ← → chuyển tháng |
| Ngày hôm nay | Gạch dưới xanh (underline), không phải highlight nền |
| Ngày đã chọn | Nền tím nhạt (#EEEDFE), text tím đậm (#3C3489), border-radius |
| Ngày quá khứ | Text mờ (tertiary), không click được |
| Ngày Chủ nhật | Mờ nếu phòng khám không mở CN (tùy cấu hình) |

**Hành vi khi click ngày:**
- Highlight ngày đã chọn (nền tím).
- Grid khung giờ bên dưới cập nhật theo ngày được chọn.
- Hiện tiêu đề "Th X, DD/MM/YYYY" trên grid giờ.

### 5.2 Grid khung giờ (Time slots)

| Thuộc tính | Chi tiết |
|------------|----------|
| Chia theo | Sáng (08:00–11:30) và Chiều (13:00–16:30) |
| Interval | Mỗi slot 30 phút |
| Số slot/ngày | 12 slot mặc định (6 sáng + 6 chiều). Admin cấu hình được. |

**3 trạng thái slot:**

| Trạng thái | Visual | Click được | Điều kiện |
|------------|--------|-----------|-----------|
| Trống | Viền xám nhạt, text đen, nền trắng | Có | Chưa đạt capacity (admin cấu hình) |
| Đang chọn | Nền tím nhạt (#EEEDFE), viền tím (#534AB7), text tím đậm, font-weight: 500 | Có (click lại để bỏ chọn) | Slot đang được lễ tân chọn |
| Đã đầy | Nền xám (background-secondary), text mờ, gạch ngang (line-through) | Không | Đã đạt capacity |

**Header grid:** Hiện "Th X, DD/MM/YYYY" + "X slot trống / Y" (VD: "6 slot trống / 12") để lễ tân biết ngay ngày đó còn chỗ không.

### 5.3 Logic slot capacity

Mỗi slot 30 phút có giới hạn số BN hẹn (capacity). Khi số appointment đạt capacity → slot chuyển thành "Đã đầy".

| Cấu hình | Mặc định | Ai quản lý |
|-----------|----------|------------|
| Capacity mỗi slot | Tùy admin cấu hình (1–5 BN/slot) | Admin |
| Giờ bắt đầu sáng | 08:00 | Admin |
| Giờ kết thúc sáng | 11:30 | Admin |
| Giờ bắt đầu chiều | 13:00 | Admin |
| Giờ kết thúc chiều | 16:30 | Admin |
| Ngày nghỉ | Chủ nhật (mặc định), tùy cấu hình | Admin |
| Ngày nghỉ đặc biệt | Lễ, Tết... | Admin |

**Lưu ý:** Walk-in KHÔNG chiếm slot hẹn. Capacity chỉ tính BN có appointment.

**Khi chọn BS cụ thể:** Grid slot có thể filter chỉ hiện slot BS đó còn trống (phase sau, nếu phòng khám quản lý lịch theo từng BS).

### 5.4 Legend

Hiện ở cuối grid giờ, 3 items:
- Ô viền = Trống
- Ô tím = Đang chọn
- Ô xám = Đã đầy

---

## 6. Thanh xác nhận (Confirmation bar)

**Vị trí:** Nằm giữa form content và footer, nền tím nhạt (#EEEDFE).

**Nội dung:**

| Scenario | Dòng 1 | Dòng 2 |
|----------|--------|--------|
| BN cũ | "**Lê Minh Châu** — Th 7, 28/03/2026 lúc 09:30" | "Tái khám dry eye — BS. Trần Minh Đức" |
| BN mới | "**BN mới** (0901 222 333) — Th 5, 27/03/2026 lúc 09:30" | "Lý do khám — BS chỉ định" |
| Chưa đủ info | Hiện text mờ nhắc lễ tân điền thêm | VD: "Chưa nhập lý do — Không chỉ định BS" |

**Mục đích:** Lễ tân đọc lại thông tin này cho BN xác nhận qua điện thoại trước khi bấm nút.

---

## 7. Hành động (Footer buttons)

| Nút | Kiểu | Hành vi |
|-----|------|---------|
| Hủy | Outline, bên trái | Confirm "Bạn có chắc muốn hủy?" → quay về Dashboard |
| Xác nhận đặt hẹn | Filled tím (#534AB7), bên phải | Validate → tạo appointment → quay về Dashboard |

**Validation trước khi xác nhận:**
- Họ tên không rỗng
- SĐT hợp lệ
- Lý do khám không rỗng
- Đã chọn ngày
- Đã chọn slot giờ
- Nếu thiếu → highlight field lỗi + không cho bấm

---

## 8. Quy tắc nghiệp vụ

### 8.1 Sau khi xác nhận đặt hẹn

**BN cũ:**
- Tạo appointment record liên kết với patient_id đã có.
- Appointment xuất hiện trên Dashboard vào đúng ngày hẹn, trạng thái "Chưa đến", nguồn "Hẹn".
- Khi BN đến check-in → lễ tân chỉ cần xác nhận, không cần qua form Intake.

**BN mới:**
- Tạo patient record tạm (chỉ có tên + SĐT) với patient_id auto-generated.
- Tạo appointment record liên kết.
- Appointment xuất hiện trên Dashboard vào đúng ngày hẹn, trạng thái "Chưa đến", nguồn "Hẹn".
- Khi BN đến check-in → lễ tân mở form Intake để bổ sung thông tin đầy đủ (ngày sinh, giới tính, địa chỉ, tiền sử...).

### 8.2 Kiểm tra trùng lặp appointment

- Cùng 1 BN (patient_id) không thể có 2 appointment trong cùng 1 ngày.
- Nếu BN đã có hẹn ngày đó → hiện cảnh báo: "BN này đã có hẹn ngày 28/03 lúc 09:30. Bạn muốn đổi lịch hay tạo thêm?"

### 8.3 Giới hạn đặt hẹn

- Không đặt hẹn quá khứ (ngày hôm nay chỉ đặt được slot chưa qua).
- Không đặt hẹn quá xa (tối đa 3 tháng trước, admin cấu hình).
- Không đặt hẹn ngày nghỉ (Chủ nhật, lễ — theo cấu hình admin).

### 8.4 BN mới trùng SĐT với BN cũ

- Nếu lễ tân nhập SĐT mới nhưng SĐT đó đã có trong hệ thống → hệ thống tự chuyển sang trạng thái BN cũ (hiện thanh xanh lá + card read-only).
- Tránh tạo 2 hồ sơ cho cùng 1 người.

---

## 9. Luồng xử lý chi tiết

### 9.1 BN cũ gọi hẹn tái khám

1. BN gọi điện: "Tôi muốn hẹn tái khám dry eye"
2. Lễ tân bấm **"Đặt lịch hẹn"** trên Dashboard
3. Gõ SĐT BN vào ô tìm kiếm → hệ thống hiện "Đã tìm thấy: Lê Minh Châu"
4. Lễ tân xác nhận: "Anh Châu, năm 78, khám lần trước ngày 10/01 đúng không ạ?"
5. Nhập lý do: "Tái khám dry eye"
6. Chọn BS: "BS. Trần" (mặc định từ lần trước)
7. Chọn ngày 28/03 trên lịch → grid slot hiện ra
8. Chọn slot 09:30
9. Đọc lại thanh xác nhận cho BN: "Anh Châu, hẹn thứ 7 ngày 28/03 lúc 09:30 với BS Trần nhé"
10. Bấm **"Xác nhận đặt hẹn"**
11. Quay về Dashboard

### 9.2 BN mới gọi hẹn lần đầu

1. BN gọi điện: "Tôi muốn đặt lịch khám mắt"
2. Lễ tân bấm **"Đặt lịch hẹn"**
3. Hỏi SĐT → gõ vào ô tìm kiếm → "Không tìm thấy"
4. Nhập tên BN: "Trần Văn Dũng"
5. SĐT auto-fill từ ô search: "0901 222 333"
6. Lý do: "Mắt mờ 1 tuần, muốn kiểm tra"
7. BS: "Không chỉ định"
8. Chọn ngày 27/03 → chọn slot 09:30
9. Đọc lại: "Anh Dũng, hẹn thứ 5 ngày 27/03 lúc 09:30 nhé. Khi đến anh mang theo CMND để lễ tân bổ sung thông tin."
10. Bấm **"Xác nhận đặt hẹn"**
11. Quay về Dashboard

---

## 10. Xử lý lỗi

| Tình huống | Xử lý |
|------------|-------|
| Bỏ trống họ tên | Highlight đỏ + "Vui lòng nhập họ tên" |
| SĐT sai format | "SĐT phải có 10–11 số và bắt đầu bằng 0" |
| Bỏ trống lý do khám | Highlight đỏ + "Vui lòng nhập lý do khám" |
| Chưa chọn ngày | Nút "Xác nhận" disabled + tooltip "Chọn ngày hẹn" |
| Chưa chọn slot giờ | Nút "Xác nhận" disabled + tooltip "Chọn giờ hẹn" |
| BN đã có hẹn cùng ngày | Cảnh báo amber: "BN đã có hẹn ngày này lúc 09:30. Đổi lịch hay tạo thêm?" |
| Slot vừa bị người khác đặt (race condition) | Toast: "Slot 09:30 vừa được đặt. Vui lòng chọn slot khác." + cập nhật grid |
| Lỗi mạng khi lưu | Toast: "Đặt hẹn thất bại, vui lòng thử lại" + giữ nguyên form |

---

## 11. Edge cases

| Tình huống | Xử lý |
|------------|-------|
| BN gọi đặt hẹn ngay hôm nay | Cho phép, nhưng chỉ hiện slot chưa qua giờ hiện tại |
| BN muốn hẹn 4 tháng sau | Chặn, hiện: "Chỉ đặt hẹn trong vòng 3 tháng tới" |
| BN gọi lại muốn đổi lịch | Lễ tân search BN → thấy appointment cũ → bấm sửa/hủy trên Dashboard (không qua form này) |
| 2 lễ tân đặt cùng slot cùng lúc | Slot capacity giảm real-time. Nếu slot hết chỗ giữa chừng → thông báo + gợi ý slot khác |
| Phòng khám đổi giờ làm việc | Admin cập nhật cấu hình → grid slot tự thay đổi cho ngày tương lai, không ảnh hưởng appointment đã đặt |
| BN mới nhưng SĐT trùng BN cũ | Hệ thống tự chuyển sang mode BN cũ, không tạo trùng hồ sơ |

---

## 12. Responsive / UX

- **Desktop (>1024px):** Layout 2 cột như mockup.
- **Tablet (768–1024px):** 2 cột nhưng lịch mini nhỏ hơn.
- **Mobile (<768px):** Stack dọc — thông tin BN trước, lịch + slot sau.
- **Focus tự động:** Khi mở form → focus vào ô tìm kiếm.
- **Keyboard:**
  - `Enter` trong ô search → trigger tìm kiếm
  - `Tab` → chuyển giữa các fields
  - `Ctrl+Enter` → xác nhận đặt hẹn
  - `Esc` → hủy (có confirm)

---

## 13. Dữ liệu sau khi đặt hẹn

### Appointment record

| Field | Giá trị |
|-------|---------|
| appointment_id | Auto-generated |
| patient_id | ID BN cũ hoặc ID BN mới vừa tạo |
| appointment_date | Ngày đã chọn |
| appointment_time | Slot giờ đã chọn |
| reason | Lý do khám |
| doctor_id | ID bác sĩ chỉ định (nullable) |
| notes | Ghi chú |
| status | "scheduled" (chưa đến) |
| source | "phone" (lễ tân đặt qua điện thoại) |
| created_by | ID lễ tân đang đăng nhập |
| created_at | Timestamp tạo |

### Hiển thị trên Dashboard

Appointment mới xuất hiện trên Dashboard vào **đúng ngày hẹn** (không phải ngày tạo):
- Cột "Giờ hẹn" = appointment_time
- Cột "Nguồn" = "Hẹn" (badge tím)
- Cột "Trạng thái" = "Chưa đến" (badge tím nhạt)
- Cột "Thao tác" = Nút "Check-in"

---

## 14. So sánh 2 scenario

| Thuộc tính | BN cũ | BN mới |
|------------|-------|--------|
| Ô tìm kiếm | Tìm thấy → thanh xanh lá | Không tìm thấy → thanh vàng |
| Thông tin BN | Card read-only (nền xám) | Input fields (nền trắng, nhập tay) |
| Lần khám gần nhất | Hiện ngày + chẩn đoán + BS | Không có |
| Fields bắt buộc | Lý do khám | Họ tên + SĐT + Lý do khám |
| BS chỉ định mặc định | BS lần trước | "Không chỉ định" |
| Sau khi xác nhận | Tạo appointment cho patient_id có sẵn | Tạo patient record tạm + appointment |
| Khi BN đến check-in | Xác nhận nhanh, không cần Intake | Mở form Intake để bổ sung đầy đủ |
| Thanh xác nhận | Hiện tên + lý do + BS | Hiện "BN mới (SĐT)" + lý do + BS |

---

## 15. Mapping với yêu cầu gốc

| Yêu cầu | Đáp ứng |
|----------|---------|
| Đặt lịch hẹn qua gọi điện | Form này — lễ tân đặt giúp |
| Đặt lịch hẹn qua web public | Giao diện riêng cho BN (không thuộc scope form này) |
| 1.4 Không tạo trùng patient_id | Search BN trước khi tạo, auto-link nếu BN cũ |
| 1.4 Search theo SĐT / tên | Ô tìm kiếm autocomplete đầu form |
| Hiển thị trên Dashboard | Appointment xuất hiện đúng ngày hẹn, trạng thái "Chưa đến" |
