# SCR-006 | Hành động trên Dashboard (Menu ⋯, Đổi lịch, Hủy hẹn, No-show, Hủy lượt khám)

**Vị trí:** Cột "Thao tác" trên bảng danh sách BN của Dashboard lễ tân

**Actor:** Lễ tân (chính), Admin

**Mục tiêu:** Cung cấp các hành động quản lý cho lễ tân trực tiếp trên Dashboard, không cần chuyển trang. Mỗi trạng thái BN có bộ hành động riêng, phù hợp với ngữ cảnh.

---

## 1. Menu ⋯ theo trạng thái

Khi lễ tân bấm icon ⋯ hoặc nút hành động trên 1 dòng BN, hệ thống hiện dropdown menu với các action khác nhau tùy trạng thái.

### 1.1 Trạng thái "Chưa đến" (BN có hẹn)

| # | Action | Icon | Màu | Mô tả |
|---|--------|------|-----|-------|
| 1 | Xem hồ sơ | Document | Default | Mở form Intake ở chế độ read-only |
| 2 | Sửa thông tin | Pencil | Default | Mở form Intake ở chế độ edit |
| 3 | Đổi lịch hẹn | Calendar | Tím (#534AB7) | Mở popup đổi ngày/giờ hẹn (section 2) |
| 4 | Đánh dấu không đến | Warning circle | Amber (#BA7517) | Mở popup no-show (section 4) |
| 5 | Hủy hẹn | X circle | Đỏ (#A32D2D) | Mở popup hủy hẹn (section 3) |

**Lưu ý:** Nút "Check-in" nằm riêng ở cột Thao tác (không trong menu ⋯) vì đây là action chính. Menu ⋯ chỉ chứa các action phụ.

### 1.2 Trạng thái "Chờ khám" (BN đã check-in hoặc walk-in)

| # | Action | Icon | Màu | Mô tả |
|---|--------|------|-----|-------|
| 1 | Xem hồ sơ | Document | Default | Mở form Intake ở chế độ read-only |
| 2 | Sửa thông tin | Pencil | Default | Mở form Intake ở chế độ edit |
| 3 | Hủy lượt khám | X circle | Đỏ (#A32D2D) | Mở popup hủy lượt khám (section 5) |

**Không có** "Đổi lịch" và "No-show" vì BN đã đến rồi.

### 1.3 Trạng thái "Đang khám" (BN đang Pre-Exam hoặc EMR)

| # | Action | Icon | Màu | Mô tả |
|---|--------|------|-----|-------|
| 1 | Xem hồ sơ | Document | Default | Mở form Intake ở chế độ read-only |

**Chỉ 1 action.** Lễ tân không được sửa/hủy khi BN đang trong quy trình khám. Lý do: Technician hoặc Doctor đang thao tác trên dữ liệu BN — sửa/hủy ở bước này gây conflict.

### 1.4 Trạng thái "Hoàn thành" (BN đã khám xong)

| # | Action | Icon | Màu | Mô tả |
|---|--------|------|-----|-------|
| 1 | Xem hồ sơ | Document | Default | Mở form Intake ở chế độ read-only |

**Chỉ 1 action.** BN đã hoàn tất, lễ tân chỉ cần xem lại thông tin hành chính nếu cần.

### 1.5 Tổng hợp matrix action × trạng thái

| Action | Chưa đến | Chờ khám | Đang khám | Hoàn thành |
|--------|----------|----------|-----------|------------|
| Xem hồ sơ | Có | Có | Có | Có |
| Sửa thông tin | Có | Có | Không | Không |
| Đổi lịch hẹn | Có | Không | Không | Không |
| Đánh dấu không đến | Có | Không | Không | Không |
| Hủy hẹn | Có | Không | Không | Không |
| Hủy lượt khám | Không | Có | Không | Không |

---

## 2. Đổi lịch hẹn

### 2.1 Trigger

Menu ⋯ → "Đổi lịch hẹn" (chỉ cho BN trạng thái "Chưa đến").

### 2.2 Mô tả

Mở popup cho lễ tân chọn ngày giờ mới cho appointment đã có. Lịch hẹn cũ sẽ bị thay thế. Thường xảy ra khi BN gọi điện báo muốn đổi sang ngày khác.

### 2.3 Cấu trúc popup

| Thành phần | Nội dung |
|------------|----------|
| Header | Icon lịch tím + "Đổi lịch hẹn" + nút đóng (×) |
| Thông tin BN | Avatar initials (tím) + Họ tên + SĐT |
| Card lịch hẹn hiện tại | Nền xám: "Th 7, 28/03/2026 lúc 09:30" + lý do khám. Read-only. |
| Calendar mini | Giống form đặt lịch hẹn (SCR-004). Ngày hẹn cũ hiện gạch ngang. Ngày đã chọn mới highlight tím. |
| Grid slot giờ | Sáng/Chiều, 3 trạng thái (trống, đang chọn, đã đầy). Header hiện "X slot trống / Y". |
| Thanh xác nhận | Hiện "cũ (gạch ngang) → mới (bold)" để lễ tân đọc lại cho BN. |
| Footer | "Hủy" + "Xác nhận đổi lịch" (tím) |

### 2.4 Fields

Không có field nhập mới. Lễ tân chỉ chọn ngày + slot giờ trên calendar/grid.

### 2.5 Hành vi nút "Xác nhận đổi lịch"

**Validation:**
- Đã chọn ngày mới (khác ngày cũ hoặc cùng ngày nhưng khác giờ)
- Đã chọn slot giờ
- Slot chưa đầy

**Sau khi xác nhận:**
- Cập nhật appointment: ngày giờ mới thay thế ngày giờ cũ
- Nếu ngày mới = hôm nay: dòng BN trên Dashboard cập nhật cột "Giờ hẹn" mới
- Nếu ngày mới ≠ hôm nay: dòng BN biến mất khỏi Dashboard hôm nay, xuất hiện vào đúng ngày mới
- Slot cũ được giải phóng (tăng 1 slot trống cho ngày cũ)
- Đóng popup, quay về Dashboard

### 2.6 Edge cases

| Tình huống | Xử lý |
|------------|-------|
| Đổi sang cùng ngày, cùng giờ | Nút "Xác nhận" disabled — không thay đổi gì |
| Đổi sang cùng ngày, khác giờ | Cho phép — slot cũ giải phóng, slot mới bị chiếm |
| Slot mới vừa bị đặt (race condition) | Toast: "Slot vừa được đặt. Chọn slot khác." + cập nhật grid |
| BN muốn đổi cả BS chỉ định | Không hỗ trợ trong popup này. Lễ tân cần hủy hẹn cũ → đặt hẹn mới. |

---

## 3. Hủy hẹn

### 3.1 Trigger

Menu ⋯ → "Hủy hẹn" (chỉ cho BN trạng thái "Chưa đến").

### 3.2 Mô tả

Xóa hoàn toàn appointment. BN biến mất khỏi Dashboard. Slot giờ được giải phóng. Thường xảy ra khi BN gọi báo không đến, hoặc lễ tân đặt nhầm.

### 3.3 Khác biệt so với No-show

| | Hủy hẹn | No-show |
|---|---------|---------|
| Ai quyết định | BN chủ động báo, hoặc lễ tân hủy | Lễ tân đánh dấu cuối ngày |
| Khi nào | Trước giờ hẹn (BN gọi báo) | Sau giờ hẹn (BN không đến) |
| Dữ liệu | Appointment chuyển status = "cancelled" | Appointment chuyển status = "no_show" |
| Mục đích | BN không muốn/không thể đến | Ghi nhận BN vắng mặt để thống kê |
| Hiển thị Dashboard | BN biến mất | BN biến mất (hoặc hiện badge "Không đến") |

### 3.4 Cấu trúc popup

| Thành phần | Nội dung |
|------------|----------|
| Header | Icon X circle đỏ + "Hủy hẹn" + nút đóng (×) |
| Thông tin BN | Avatar (nền đỏ nhạt) + Họ tên + ngày giờ hẹn |
| Cảnh báo đỏ | "Hẹn sẽ bị xóa hoàn toàn. BN sẽ không còn xuất hiện trên Dashboard ngày này. Hành động không thể hoàn tác." |
| Lý do hủy | Dropdown, bắt buộc |
| Footer | "Quay lại" + "Xác nhận hủy hẹn" (đỏ #A32D2D) |

### 3.5 Dropdown lý do hủy

| Option | Khi nào dùng |
|--------|-------------|
| BN yêu cầu hủy | BN gọi điện báo không đến |
| BN đổi phòng khám | BN chuyển sang phòng khám khác |
| Lễ tân đặt nhầm | Đặt sai BN, sai ngày, sai giờ |
| Khác | Lý do khác (có thể thêm ghi chú) |

### 3.6 Hành vi nút "Xác nhận hủy hẹn"

**Validation:**
- Đã chọn lý do hủy (dropdown không rỗng)

**Sau khi xác nhận:**
- Appointment status → "cancelled"
- Ghi nhận: lý do hủy, ai hủy (lễ tân ID), thời điểm hủy
- Dòng BN biến mất khỏi Dashboard
- Slot giờ được giải phóng
- KPI "Lịch hẹn hôm nay" giảm 1, sub-text "X chưa đến" giảm 1
- Đóng popup, quay về Dashboard
- Toast xanh: "Đã hủy hẹn cho [Tên BN]"

### 3.7 Edge cases

| Tình huống | Xử lý |
|------------|-------|
| Hủy nhầm, muốn hoàn tác | Không hỗ trợ hoàn tác. Lễ tân phải đặt hẹn mới. |
| BN đã check-in rồi mới muốn hủy | Không dùng "Hủy hẹn" — dùng "Hủy lượt khám" (section 5) |
| Hủy hẹn ngày khác (không phải hôm nay) | Thao tác từ view Lịch hẹn sắp tới (SCR-007), logic giống hệt |

---

## 4. Đánh dấu không đến (No-show)

### 4.1 Trigger

Menu ⋯ → "Đánh dấu không đến" (chỉ cho BN trạng thái "Chưa đến").

### 4.2 Mô tả

Ghi nhận BN có hẹn nhưng không đến phòng khám. Khác với "Hủy hẹn": no-show giữ appointment trong hệ thống để thống kê (tỷ lệ vắng, BN hay vắng...). Thường thực hiện cuối buổi hoặc cuối ngày.

### 4.3 Cấu trúc popup

| Thành phần | Nội dung |
|------------|----------|
| Header | Icon warning circle amber + "Đánh dấu không đến" + nút đóng (×) |
| Thông tin BN | Avatar (nền amber) + Họ tên + ngày giờ hẹn |
| Cảnh báo amber | "BN sẽ được ghi nhận là no-show. Hẹn vẫn giữ trong hệ thống để thống kê. Lễ tân có thể đặt hẹn lại cho BN sau." |
| Ghi chú | Text input, tùy chọn. Placeholder: "VD: Đã gọi nhưng không liên lạc được..." |
| Checkbox "Đặt hẹn lại" | "Đặt hẹn lại cho BN này" — nếu check, sau xác nhận sẽ mở form đặt hẹn (SCR-004) với BN pre-fill |
| Footer | "Quay lại" + "Xác nhận no-show" (amber #BA7517) |

### 4.4 Hành vi nút "Xác nhận no-show"

**Sau khi xác nhận:**
- Appointment status → "no_show"
- Ghi nhận: ghi chú (nếu có), ai đánh dấu (lễ tân ID), thời điểm
- Dòng BN biến mất khỏi Dashboard (hoặc chuyển badge "Không đến" tùy cấu hình)
- KPI "Lịch hẹn hôm nay" sub-text "X chưa đến" giảm 1
- Slot giờ KHÔNG giải phóng (vì slot đã qua, không ai đặt được nữa)
- Đóng popup
- Nếu checkbox "Đặt hẹn lại" được check → mở form đặt hẹn (SCR-004) với patient pre-fill
- Nếu không check → quay về Dashboard

### 4.5 Dữ liệu no-show

| Field | Giá trị |
|-------|---------|
| appointment.status | "no_show" |
| appointment.no_show_at | Timestamp đánh dấu |
| appointment.no_show_by | ID lễ tân |
| appointment.no_show_notes | Ghi chú (nullable) |

**Mục đích thống kê:**
- Tỷ lệ no-show theo tuần/tháng
- Danh sách BN hay vắng (> 2 lần no-show) → ưu tiên gọi nhắc
- So sánh no-show giữa kênh đặt hẹn (web vs phone)

---

## 5. Hủy lượt khám

### 5.1 Trigger

Menu ⋯ → "Hủy lượt khám" (chỉ cho BN trạng thái "Chờ khám").

### 5.2 Mô tả

Xóa BN ra khỏi hàng đợi khám hôm nay. Khác với "Hủy hẹn": hủy lượt khám áp dụng cho BN đã check-in hoặc walk-in (đã đến phòng khám, đang chờ) nhưng muốn bỏ về. Có thể do BN không muốn chờ, lễ tân check-in nhầm, hoặc BN đổi ý.

### 5.3 Khác biệt so với Hủy hẹn

| | Hủy hẹn | Hủy lượt khám |
|---|---------|----------------|
| Áp dụng cho | BN "Chưa đến" (có hẹn, chưa tới) | BN "Chờ khám" (đã check-in/walk-in, đang chờ) |
| BN đã đến chưa | Chưa | Đã đến |
| Xóa gì | Appointment | Visit record (lượt khám hôm nay) |
| Nếu BN có hẹn | Hẹn → "cancelled" | Hẹn → "cancelled", visit → "cancelled" |
| Nếu BN walk-in | Không áp dụng | Visit → "cancelled" |
| Lý do thường gặp | BN báo không đến, đặt nhầm | BN bỏ về, check-in nhầm, đổi ý |

### 5.4 Cấu trúc popup

| Thành phần | Nội dung |
|------------|----------|
| Header | Icon X circle đỏ + "Hủy lượt khám" + nút đóng (×) |
| Thông tin BN | Avatar (nền đỏ nhạt) + Họ tên + SĐT + badge "Chờ khám" |
| Card context | Nền xám: Nguồn (Hẹn/Walk-in) + giờ hẹn (nếu có) + giờ check-in + lý do khám |
| Cảnh báo đỏ | "BN sẽ bị xóa khỏi hàng đợi hôm nay. Nếu BN có hẹn, lịch hẹn sẽ chuyển thành Đã hủy. Hành động không thể hoàn tác." |
| Lý do hủy | Dropdown, bắt buộc |
| Ghi chú | Text input, tùy chọn |
| Checkbox "Đặt hẹn lại" | "Đặt hẹn lại cho BN này" |
| Footer | "Quay lại" + "Xác nhận hủy lượt khám" (đỏ #A32D2D) |

### 5.5 Dropdown lý do hủy

| Option | Khi nào dùng |
|--------|-------------|
| BN không muốn chờ, bỏ về | BN đã đến nhưng thấy đông quá |
| BN muốn đổi sang ngày khác | BN đổi ý, muốn hẹn lại |
| Lễ tân check-in nhầm người | Bấm nhầm nút Check-in cho BN khác |
| BN chuyển sang phòng khám khác | BN đổi ý không khám ở đây |
| Khác | Lý do khác |

### 5.6 Hành vi nút "Xác nhận hủy lượt khám"

**Validation:**
- Đã chọn lý do hủy (dropdown không rỗng)

**Sau khi xác nhận:**
- Visit record status → "cancelled"
- Nếu BN có appointment: appointment status → "cancelled"
- Ghi nhận: lý do hủy, ghi chú, ai hủy (lễ tân ID), thời điểm
- Dòng BN biến mất khỏi Dashboard
- KPI "Chờ khám" giảm 1
- Đóng popup
- Nếu checkbox "Đặt hẹn lại" được check → mở form đặt hẹn (SCR-004) với patient pre-fill
- Nếu không check → quay về Dashboard
- Toast xanh: "Đã hủy lượt khám cho [Tên BN]"

### 5.7 Edge cases

| Tình huống | Xử lý |
|------------|-------|
| BN walk-in, hủy lượt khám | Chỉ xóa visit record. Không ảnh hưởng appointment (vì không có). |
| BN có hẹn, đã check-in, hủy lượt khám | Cả visit lẫn appointment đều → "cancelled". |
| Hủy nhầm, muốn hoàn tác | Không hỗ trợ. Lễ tân phải check-in lại (BN hẹn) hoặc tạo lượt khám mới (BN walk-in). |
| BN đang "Chờ khám" nhưng Technician vừa gọi vào | Race condition: nếu Technician bắt đầu Pre-Exam trước khi lễ tân bấm hủy → BN chuyển "Đang khám" → nút "Hủy lượt khám" biến mất. Lễ tân nhận toast: "BN đã chuyển sang Đang khám, không thể hủy." |
| BN bỏ về nhưng quay lại sau 30 phút | Lễ tân tạo lượt khám mới hoặc check-in lại nếu còn hẹn. |

---

## 6. Action "Xem hồ sơ" và "Sửa thông tin"

Hai action này có mặt ở hầu hết trạng thái, cần define rõ:

### 6.1 Xem hồ sơ (read-only)

- Mở form Intake (SCR-003) ở chế độ read-only.
- Tất cả fields disabled, nền xám.
- Lễ tân chỉ xem thông tin hành chính (tên, SĐT, ngày sinh, lý do khám, tiền sử...).
- KHÔNG hiện dữ liệu lâm sàng (kết quả khám, chẩn đoán, đơn thuốc).
- Nút footer: chỉ có "Đóng" (quay về Dashboard).

### 6.2 Sửa thông tin (edit mode)

- Mở form Intake (SCR-003) ở chế độ edit, dữ liệu pre-fill.
- Lễ tân sửa thông tin cá nhân (đổi SĐT, cập nhật địa chỉ, bổ sung tiền sử...).
- Nút footer: "Hủy" + "Lưu".
- Lưu → cập nhật patient record, quay về Dashboard.
- KHÔNG thay đổi trạng thái BN (vẫn giữ nguyên Chưa đến / Chờ khám / ...).

**Ngoại lệ:** Khi "Sửa thông tin" được gọi từ popup Check-in (SCR-005), lưu = check-in tự động. Xem spec SCR-005 cho chi tiết.

---

## 7. Xử lý lỗi chung

| Tình huống | Áp dụng cho | Xử lý |
|------------|-------------|-------|
| Lỗi mạng khi xác nhận action | Tất cả popup | Toast: "Thao tác thất bại, vui lòng thử lại". Popup giữ nguyên. |
| BN đã thay đổi trạng thái (race condition) | Tất cả | Popup đóng + Toast: "Trạng thái BN đã thay đổi." + Dashboard refresh. |
| 2 lễ tân thao tác cùng BN cùng lúc | Tất cả | Lễ tân thứ 2 nhận toast: "BN đã được [action] bởi người khác." |
| Dropdown lý do chưa chọn | Hủy hẹn, Hủy lượt khám | Nút xác nhận disabled. Highlight dropdown khi click nút. |

---

## 8. Dữ liệu ghi nhận cho mỗi action

Mỗi action đều ghi audit log:

| Field | Mô tả |
|-------|-------|
| action_type | "reschedule" / "cancel_appointment" / "no_show" / "cancel_visit" |
| target_id | appointment_id hoặc visit_id |
| patient_id | ID bệnh nhân |
| performed_by | ID lễ tân thực hiện |
| performed_at | Timestamp |
| reason | Lý do (từ dropdown hoặc ghi chú) |
| old_value | Giá trị cũ (VD: ngày giờ cũ khi đổi lịch) |
| new_value | Giá trị mới (VD: ngày giờ mới khi đổi lịch) |

**Mục đích:** Truy vết ai làm gì, khi nào, lý do gì. Hữu ích khi có dispute (BN khiếu nại bị hủy hẹn).

---

## 9. Mapping với các màn hình khác

| Action | Popup/Form | Dẫn đến sau xác nhận |
|--------|------------|----------------------|
| Xem hồ sơ | SCR-003 (Intake read-only) | Đóng → Dashboard |
| Sửa thông tin | SCR-003 (Intake edit) | Lưu → Dashboard (trạng thái không đổi) |
| Đổi lịch hẹn | Popup đổi lịch (section 2) | Dashboard cập nhật giờ hẹn mới hoặc BN biến mất (nếu đổi sang ngày khác) |
| Hủy hẹn | Popup hủy hẹn (section 3) | BN biến mất khỏi Dashboard |
| Đánh dấu không đến | Popup no-show (section 4) | BN biến mất + (tùy chọn) mở form đặt hẹn lại |
| Hủy lượt khám | Popup hủy lượt (section 5) | BN biến mất + (tùy chọn) mở form đặt hẹn lại |

---

## 10. Tổng hợp visual design

| Popup | Màu chủ đạo | Nút xác nhận | Mức độ nghiêm trọng |
|-------|-------------|--------------|---------------------|
| Đổi lịch hẹn | Tím (#534AB7) | "Xác nhận đổi lịch" (tím) | Thấp — thay đổi, không xóa |
| Đánh dấu không đến | Amber (#BA7517) | "Xác nhận no-show" (amber) | Trung bình — ghi nhận vắng |
| Hủy hẹn | Đỏ (#A32D2D) | "Xác nhận hủy hẹn" (đỏ) | Cao — xóa appointment |
| Hủy lượt khám | Đỏ (#A32D2D) | "Xác nhận hủy lượt khám" (đỏ) | Cao — xóa khỏi hàng đợi |

Mức độ nghiêm trọng thể hiện qua màu: tím = nhẹ (thay đổi), amber = trung bình (ghi nhận), đỏ = nặng (xóa). Cảnh báo cũng tương ứng: note info → warning → danger.
