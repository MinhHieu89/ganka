# SCR-005 | Check-in & Tạo lượt khám (BN đã hẹn + BN cũ walk-in)

**Trigger:**
- **Check-in:** Bấm nút "Check-in" trên cột Thao tác của Dashboard (chỉ hiện cho BN có trạng thái "Chưa đến")
- **Tạo lượt khám:** Search BN cũ trên Dashboard → click kết quả → popup

**Actor:** Lễ tân (chính), Admin

**Mục tiêu:** Đưa bệnh nhân đã đến phòng khám vào hàng đợi "Chờ khám" nhanh nhất có thể. Bao gồm 2 flow: check-in BN có hẹn, và tạo lượt khám cho BN cũ walk-in tái khám. Cả 2 đều dùng popup xác nhận — không cần mở full form Intake nếu thông tin đã đủ.

---

## 1. Mô tả chức năng

Màn hình này cover 2 flow dùng popup xác nhận:

**Flow A — Check-in BN có hẹn:** BN đã đặt lịch (qua web hoặc điện thoại), đến phòng khám. Lễ tân bấm "Check-in" trên Dashboard → popup hiện thông tin → xác nhận → BN vào "Chờ khám".

**Flow B — Tạo lượt khám BN cũ walk-in:** BN đã có hồ sơ trong hệ thống, không có hẹn, tự đến phòng khám. Lễ tân search SĐT/tên trên Dashboard → click kết quả → popup hiện thông tin + field lý do khám → xác nhận → BN vào "Chờ khám".

Cả 2 flow đều đảm bảo BN vào hàng đợi "Chờ khám" luôn có đủ thông tin tối thiểu cho Technician làm Pre-Exam.

---

# PHẦN A: CHECK-IN BN CÓ HẸN

---

## 2. Phân loại hồ sơ

Khi bấm "Check-in", hệ thống kiểm tra hồ sơ BN và phân loại:

| Điều kiện | Phân loại | Popup hiển thị |
|-----------|-----------|----------------|
| Có đủ: họ tên, ngày sinh, giới tính, SĐT, lý do khám | Đầy đủ | Popup BN cũ (cho phép check-in ngay) |
| Thiếu bất kỳ field bắt buộc nào (ngày sinh, giới tính...) | Thiếu | Popup BN mới (bắt buộc bổ sung) |

**Fields bắt buộc để được coi là "đầy đủ":** Họ tên, ngày sinh, giới tính, SĐT, lý do khám — đúng 5 fields bắt buộc của form Intake (SCR-003).

---

## 3. Popup check-in — BN cũ (hồ sơ đầy đủ)

### 3.1 Cấu trúc popup

| Thành phần | Nội dung |
|------------|----------|
| Header | Icon check tím + "Check-in bệnh nhân" + nút đóng (×) |
| Avatar + tên | Initials (nền tím) + Họ tên + mã BN (mono font) + giờ hẹn (bên phải) |
| Card thông tin | Năm sinh, giới tính, SĐT, nghề nghiệp, lý do khám, lần khám gần nhất |
| Note xanh | "Xác nhận thông tin với BN trước khi check-in. Nếu cần sửa, bấm Sửa thông tin." |
| Footer | 3 nút: "Sửa thông tin" (trái), "Hủy" + "Xác nhận check-in" (phải) |

### 3.2 Thông tin hiển thị

| Field | Nguồn | Ghi chú |
|-------|-------|---------|
| Họ tên | patient.name | Font-weight: 500, size lớn hơn |
| Mã BN | patient.patient_id | Font mono, màu tertiary |
| Giờ hẹn | appointment.time | Hiện bên phải avatar, màu tím, font-weight: 500 |
| Năm sinh | patient.birth_year | |
| Giới tính | patient.gender | |
| SĐT | patient.phone | |
| Nghề nghiệp | patient.occupation | Nếu không có hiện "—" |
| Lý do khám | appointment.reason | Từ lúc đặt hẹn |
| Lần khám gần nhất | Từ visit gần nhất | Format: "DD/MM/YYYY — Chẩn đoán — BS. Tên". Nếu lần đầu khám → không hiện dòng này. |

### 3.3 Hành động

| Nút | Vị trí | Kiểu | Hành vi |
|-----|--------|------|---------|
| Sửa thông tin | Footer trái | Outline | Đóng popup → mở form Intake (SCR-003) ở chế độ edit với dữ liệu pre-fill. Sau khi lưu Intake → BN tự động check-in, chuyển "Chờ khám". |
| Hủy | Footer phải | Outline | Đóng popup, không làm gì. BN vẫn ở trạng thái "Chưa đến". |
| Xác nhận check-in | Footer phải | Filled tím (#534AB7) | Check-in ngay: chuyển trạng thái "Chưa đến" → "Chờ khám". Ghi nhận giờ đến = thời điểm bấm. Đóng popup, quay về Dashboard. |

### 3.4 Sau khi check-in thành công

- Trạng thái BN trên Dashboard: "Chưa đến" → "Chờ khám" (badge amber)
- Cột "Thao tác": nút "Check-in" biến mất, thay bằng icon ⋯
- KPI "Lịch hẹn hôm nay": sub-text "X chưa đến" giảm 1
- KPI "Chờ khám": tăng 1
- Ghi nhận: `appointment.checked_in_at` = timestamp hiện tại

---

## 4. Popup check-in — BN mới (hồ sơ thiếu)

### 4.1 Cấu trúc popup

| Thành phần | Nội dung |
|------------|----------|
| Header | Giống popup BN cũ |
| Avatar + tên | Initials (nền amber thay vì tím — nhấn mạnh cảnh báo) + Họ tên + mã BN + giờ hẹn |
| Card thông tin | Hiện fields có dữ liệu bình thường. Fields thiếu hiện "Chưa có" (italic, màu nhạt). |
| Cảnh báo vàng | "Hồ sơ chưa đầy đủ — BN đặt hẹn qua điện thoại, chỉ có tên + SĐT. Cần bổ sung ngày sinh, giới tính và các thông tin khác trước khi khám." |
| Footer | 2 nút: "Hủy" (trái) + "Check-in & bổ sung hồ sơ →" (phải) |

### 4.2 Thông tin hiển thị

| Field | Có dữ liệu | Thiếu dữ liệu |
|-------|-------------|----------------|
| Họ tên | Hiện bình thường | (luôn có — bắt buộc khi đặt hẹn) |
| SĐT | Hiện bình thường | (luôn có — bắt buộc khi đặt hẹn) |
| Năm sinh | Hiện bình thường | "Chưa có" (italic, color: tertiary) |
| Giới tính | Hiện bình thường | "Chưa có" (italic, color: tertiary) |
| Nghề nghiệp | Hiện bình thường | "Chưa có" (italic, color: tertiary) |
| Lý do khám | Hiện bình thường | (luôn có — bắt buộc khi đặt hẹn) |
| Lần khám gần nhất | Không hiện | BN mới, chưa có lịch sử |

### 4.3 Khác biệt visual so với popup BN cũ

| Yếu tố | BN cũ | BN mới |
|---------|-------|--------|
| Avatar nền | Tím (#EEEDFE) | Amber (#FAEEDA) |
| Avatar text | Tím (#3C3489) | Amber (#854F0B) |
| Fields thiếu | Không có | "Chưa có" italic |
| Banner cảnh báo | Note xanh (info) | Banner vàng (warning) |
| Nút chính | "Xác nhận check-in" (1 click xong) | "Check-in & bổ sung hồ sơ →" (mở Intake) |
| Nút "Sửa thông tin" | Có | Không (vì bắt buộc phải vào Intake rồi) |

### 4.4 Hành động

| Nút | Kiểu | Hành vi |
|-----|------|---------|
| Hủy | Outline | Đóng popup. BN vẫn "Chưa đến". |
| Check-in & bổ sung hồ sơ → | Filled tím | Đóng popup → mở form Intake (SCR-003) ở chế độ edit. Fields đã có (tên, SĐT, lý do) pre-fill, fields thiếu để trống. Lễ tân nhập bổ sung (ngày sinh, giới tính bắt buộc). Bấm "Lưu" trong Intake → BN check-in, chuyển "Chờ khám". |

### 4.5 Tại sao KHÔNG cho check-in ngay?

BN vào hàng đợi "Chờ khám" nghĩa là Technician có thể gọi vào Pre-Exam bất cứ lúc nào. Nếu hồ sơ thiếu ngày sinh và giới tính → Pre-Exam không có đủ dữ liệu nền. Nên bắt buộc bổ sung trước.

---

## 5. Flow form Intake khi mở từ check-in

Khi popup check-in dẫn đến form Intake (cả "Sửa thông tin" và "Check-in & bổ sung"), form Intake hoạt động hơi khác so với mode "Tiếp nhận BN mới":

| Thuộc tính | Intake mở từ "Tiếp nhận BN mới" | Intake mở từ Check-in |
|------------|----------------------------------|----------------------|
| Mã BN (patient_id) | Sinh mới | Đã có, hiện read-only |
| Fields | Tất cả trống | Pre-fill dữ liệu đã có |
| Nút "Lưu" | Tạo record mới → "Chờ khám" | Cập nhật record → check-in → "Chờ khám" |
| Nút "Lưu & chuyển Pre-Exam" | Hiện | Hiện |
| Nút "Hủy" | Quay về Dashboard, không tạo gì | Quay về Dashboard, BN vẫn "Chưa đến" |
| Nguồn BN trên Dashboard | "Walk-in" | "Hẹn" (giữ nguyên) |

**Quan trọng:** Sau khi lưu Intake từ flow check-in, BN chuyển từ "Chưa đến" → "Chờ khám" tự động. Lễ tân KHÔNG cần quay lại Dashboard bấm check-in lần nữa.

---

## 6. Quy tắc nghiệp vụ

### 6.1 Ghi nhận thời gian

| Timestamp | Ghi khi nào | Mục đích |
|-----------|-------------|----------|
| appointment.checked_in_at | Lúc check-in thành công | Biết BN đến lúc mấy giờ |
| appointment.scheduled_at | Lúc đặt hẹn (đã có) | So sánh giờ hẹn vs giờ đến |

Hiệu số (checked_in_at - scheduled_at) cho biết BN đến sớm hay trễ bao nhiêu phút → hữu ích cho thống kê sau.

### 6.2 Thứ tự hàng đợi

Sau check-in, BN xếp vào hàng đợi "Chờ khám" theo thứ tự:

1. BN đã check-in sớm hơn → ưu tiên trước (FIFO theo checked_in_at)
2. Không phân biệt hẹn hay walk-in trong hàng đợi — ai check-in / tiếp nhận trước thì khám trước

### 6.3 Check-in nhiều lần

- Mỗi appointment chỉ check-in 1 lần.
- Sau khi check-in, nút "Check-in" biến mất, không thể check-in lại.
- Nếu cần hủy check-in (nhầm người) → dùng menu ⋯ → "Hủy lượt khám".

---

# PHẦN B: TẠO LƯỢT KHÁM BN CŨ WALK-IN

## 7. Khi nào dùng flow này

BN đã có hồ sơ trong hệ thống (đã khám ít nhất 1 lần trước), không có hẹn hôm nay, tự đến phòng khám.

**Trigger:** Lễ tân search SĐT/tên trên ô tìm kiếm Dashboard → kết quả autocomplete hiện ra → click vào kết quả → mở popup "Tạo lượt khám mới".

**Khác với Check-in:** BN không nằm sẵn trong bảng Dashboard (vì không có hẹn). Lễ tân phải chủ động search.

---

## 8. Popup tạo lượt khám mới

### 8.1 Cấu trúc popup

| Thành phần | Nội dung |
|------------|----------|
| Header | Icon user xanh + "Tạo lượt khám mới" + nút đóng (×) |
| Avatar + tên | Initials (nền xanh blue) + Họ tên + mã BN (mono font) + badge "Walk-in" (coral, bên phải) |
| Card thông tin | Năm sinh, giới tính, SĐT, nghề nghiệp, lần khám gần nhất (read-only) |
| Field lý do khám | Textarea editable, bắt buộc |
| Note xanh | "BN sẽ vào hàng đợi Chờ khám ngay sau khi tạo lượt khám. Nếu cần cập nhật thông tin cá nhân, bấm Sửa thông tin." |
| Footer | 3 nút: "Sửa thông tin" (trái), "Hủy" + "Tạo lượt khám" (phải) |

### 8.2 Khác biệt so với popup check-in BN cũ

| Thuộc tính | Check-in (BN có hẹn) | Tạo lượt khám (BN cũ walk-in) |
|------------|----------------------|-------------------------------|
| Trigger | Nút "Check-in" trên Dashboard | Search → click kết quả |
| Header | "Check-in bệnh nhân" + icon check tím | "Tạo lượt khám mới" + icon user xanh |
| Avatar nền | Tím (#EEEDFE) | Xanh blue (#E6F1FB) |
| Giờ hẹn | Hiện bên phải avatar | Không có — thay bằng badge "Walk-in" (coral) |
| Lý do khám | Read-only (từ lúc đặt hẹn) | Textarea editable, bắt buộc (visit mới cần lý do mới) |
| Nút chính | "Xác nhận check-in" (tím) | "Tạo lượt khám" (xanh đậm #185FA5) |
| Nguồn trên Dashboard | "Hẹn" (badge tím) | "Walk-in" (badge coral) |
| Sau khi bấm | "Chưa đến" → "Chờ khám" | BN xuất hiện mới trên Dashboard, trạng thái "Chờ khám" |

### 8.3 Field lý do khám

| Thuộc tính | Chi tiết |
|------------|----------|
| Kiểu | Textarea |
| Bắt buộc | Có — không cho bấm "Tạo lượt khám" nếu trống |
| Placeholder | "VD: Tái khám dry eye, mắt khô trở lại, kiểm tra định kỳ..." |
| Validation | Không rỗng, tối đa 500 ký tự |
| Ghi chú dưới field | "Lý do này sẽ hiển thị trên Dashboard và flow sang Pre-Exam." |

**Tại sao lý do khám phải nhập mới?** Mỗi visit có lý do riêng. BN lần trước đến vì dry eye, lần này có thể đến vì cận thị tăng. Lý do từ visit cũ KHÔNG được tái sử dụng.

### 8.4 Hành động

| Nút | Vị trí | Kiểu | Hành vi |
|-----|--------|------|---------|
| Sửa thông tin | Footer trái | Outline | Đóng popup → mở form Intake (SCR-003) ở chế độ edit với dữ liệu pre-fill + field lý do khám từ popup (nếu đã nhập). Sau khi lưu Intake → tạo lượt khám tự động, BN vào "Chờ khám". |
| Hủy | Footer phải | Outline | Đóng popup, không làm gì. |
| Tạo lượt khám | Footer phải | Filled xanh (#185FA5) | Validate lý do khám không rỗng → tạo visit record mới → BN xuất hiện trên Dashboard "Chờ khám" nguồn "Walk-in". Đóng popup. |

### 8.5 Sau khi tạo lượt khám thành công

- BN xuất hiện mới trong bảng Dashboard với:
  - Cột "Giờ hẹn" = "—" (không có hẹn)
  - Cột "Nguồn" = "Walk-in" (badge coral)
  - Cột "Trạng thái" = "Chờ khám" (badge amber)
  - Cột "Lý do khám" = lý do vừa nhập
  - Cột "Thao tác" = icon ⋯
- KPI "Chờ khám" trên Dashboard tăng 1
- Ghi nhận: `visit.created_at` = timestamp hiện tại

### 8.6 Dữ liệu tạo mới

Khi bấm "Tạo lượt khám", hệ thống tạo 1 visit record mới (KHÔNG tạo patient record mới — BN đã có):

| Field | Giá trị |
|-------|---------|
| visit_id | Auto-generated |
| patient_id | ID BN đã có |
| visit_date | Hôm nay |
| reason | Lý do khám vừa nhập |
| status | "waiting" (chờ khám) |
| source | "walk-in" |
| created_by | ID lễ tân đang đăng nhập |
| created_at | Timestamp hiện tại |

---

# PHẦN C: QUY TẮC CHUNG (CẢ 2 FLOW)

## 9. Xử lý lỗi

| Tình huống | Flow | Xử lý |
|------------|------|-------|
| BN đã check-in rồi (click lại) | A | Không xảy ra — nút "Check-in" đã biến mất sau lần đầu |
| Popup mở nhưng BN bỏ đi | A, B | Bấm "Hủy" hoặc đóng popup (×). Không thay đổi gì. |
| Mở Intake từ popup nhưng lễ tân bấm "Hủy" trong Intake | A, B | Quay về Dashboard. BN vẫn ở trạng thái cũ. |
| Lỗi mạng khi xác nhận | A, B | Toast: "Thao tác thất bại, vui lòng thử lại". Popup giữ nguyên. |
| 2 lễ tân check-in cùng BN cùng lúc | A | Lễ tân đầu tiên thành công. Lễ tân thứ 2: "BN đã được check-in bởi người khác". |
| Lý do khám bỏ trống | B | Nút "Tạo lượt khám" disabled. Highlight field đỏ khi focus out. |
| BN cũ walk-in nhưng đã có lượt khám hôm nay | B | Cảnh báo: "BN đã có lượt khám hôm nay (Chờ khám). Bạn muốn tạo thêm lượt mới?" |

---

## 10. Edge cases

| Tình huống | Flow | Xử lý |
|------------|------|-------|
| BN đến sớm hơn giờ hẹn | A | Check-in bình thường. Ghi nhận giờ đến thực tế. |
| BN đến trễ 2 tiếng | A | Check-in bình thường. BN xếp cuối hàng đợi (theo checked_in_at). |
| BN đến nhưng muốn đổi lý do khám | A | Bấm "Sửa thông tin" → sửa lý do trong Intake → lưu → check-in. |
| BN đến nhưng thông tin sai | A, B | Bấm "Sửa thông tin" → sửa trong Intake → lưu → tự động check-in / tạo lượt khám. |
| BN hẹn nhưng ngày khác (không nằm trong Dashboard) | A → B | Không hiện nút "Check-in". Lễ tân search → dùng flow B "Tạo lượt khám mới". |
| BN cũ walk-in nhưng lễ tân lỡ bấm "Tiếp nhận BN mới" | B | Khi nhập SĐT trong form Intake → hệ thống cảnh báo trùng → nút "Mở hồ sơ cũ" → quay lại flow đúng. |
| BN có hẹn nhưng cũng walk-in thêm người nhà | A + B | Check-in cho BN hẹn (flow A). Tiếp nhận BN mới cho người nhà (flow SCR-003). Hai flow độc lập. |

---

## 11. Mapping với các màn hình khác

| Hành động | Flow | Dẫn đến |
|-----------|------|---------|
| Bấm "Xác nhận check-in" (popup check-in BN cũ) | A | Đóng popup → cập nhật Dashboard ("Chưa đến" → "Chờ khám") |
| Bấm "Sửa thông tin" (popup check-in BN cũ) | A | SCR-003 (Intake edit mode, pre-fill, lưu = check-in) |
| Bấm "Check-in & bổ sung hồ sơ" (popup check-in BN mới) | A | SCR-003 (Intake edit mode, partial pre-fill, lưu = check-in) |
| Bấm "Tạo lượt khám" (popup walk-in BN cũ) | B | Đóng popup → BN xuất hiện mới trên Dashboard "Chờ khám" |
| Bấm "Sửa thông tin" (popup walk-in BN cũ) | B | SCR-003 (Intake edit mode, pre-fill + lý do, lưu = tạo lượt khám) |
| Lưu Intake từ bất kỳ flow nào | A, B | Quay về Dashboard, BN đã ở "Chờ khám" |
| Bấm "Lưu & chuyển Pre-Exam" trong Intake | A, B | Check-in/tạo lượt khám + mở Pre-Exam Step 1 cho BN này |
