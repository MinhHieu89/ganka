# SCR-002a | Dashboard Lễ tân (Receptionist Dashboard)

**Route:** `/dashboard`

**Actor:** Lễ tân (chính), Admin

**Mục tiêu:** Màn hình làm việc chính và duy nhất của lễ tân trong ngày. Quản lý toàn bộ hàng đợi bệnh nhân: tiếp nhận walk-in, check-in BN đã hẹn, theo dõi trạng thái khám, tìm kiếm hồ sơ cũ. Lễ tân KHÔNG cần truy cập dữ liệu lâm sàng (kết quả khám, chẩn đoán, đơn thuốc).

---

## 1. Mô tả chức năng

Sau khi đăng nhập, lễ tân được redirect thẳng đến `/dashboard`. Hệ thống nhận diện role = Receptionist và render giao diện riêng cho lễ tân (các role khác vào cùng route `/dashboard` nhưng thấy giao diện khác).

Dashboard lễ tân tập trung vào 3 nhiệm vụ chính:
- **Tiếp nhận BN mới** (walk-in đến phòng khám, chưa có hồ sơ)
- **Check-in BN đã hẹn** (đặt lịch qua web hoặc gọi điện, đã có thông tin sẵn)
- **Đặt lịch hẹn** (BN gọi điện đến phòng khám, lễ tân đặt giúp)

Ngoài ra lễ tân theo dõi trạng thái BN trong ngày và tìm kiếm hồ sơ BN cũ khi cần.

---

## 2. Phân quyền hiển thị

Lễ tân chỉ thấy thông tin hành chính, KHÔNG thấy dữ liệu lâm sàng:

| Được thấy | Không được thấy |
|-----------|-----------------|
| Họ tên, năm sinh, SĐT, giới tính | Kết quả khám (VA, refraction, slit-lamp...) |
| Lý do khám (sơ bộ) | Chẩn đoán |
| Giờ hẹn, giờ đến | Đơn thuốc, đơn kính |
| Trạng thái khám (chờ, đang khám, xong) | Kết quả Dry Eye, OSDI, TBUT... |
| Nguồn (hẹn / walk-in) | Ghi chú bác sĩ |

Khi lễ tân click vào 1 dòng BN → mở lại form Intake (xem/sửa thông tin cá nhân), KHÔNG mở bệnh án.

---

## 3. Cấu trúc màn hình (từ trên xuống)

### 3.1 Top bar (Header)

| Thành phần | Vị trí | Nội dung |
|------------|--------|----------|
| Logo + tên hệ thống | Trái | "G28 Ganka28 / Dashboard" |
| Ngày hiện tại | Phải | VD: "Th 4, 26/03/2026" |
| Thông tin người dùng | Phải | Avatar initials + tên + role "Lễ tân" |

### 3.2 Action bar (Thanh hành động)

| Thành phần | Vị trí | Kiểu | Mô tả |
|------------|--------|------|-------|
| Nút "Tiếp nhận BN mới" | Trái | Filled (primary, xanh đậm) | Mở form SCR-003 (new-patient-form). Cho BN walk-in đang đứng trước mặt. |
| Nút "Đặt lịch hẹn" | Trái, cạnh nút trên | Outline (tím, có icon lịch) | Mở form đặt lịch hẹn. Cho BN gọi điện, khám ngày khác. |
| Ô tìm kiếm | Trái, cạnh 2 nút | Text input + icon search | Tìm BN theo SĐT hoặc tên. Autocomplete real-time. |
| Bộ filter trạng thái | Phải | Pill buttons | Tất cả, Chưa đến, Chờ khám, Đang khám, Xong |

**Phân biệt 2 nút chính:**
- "Tiếp nhận BN mới" = BN đang ở phòng khám, khám ngay → filled, nổi bật nhất
- "Đặt lịch hẹn" = BN gọi điện, hẹn ngày sau → outline, nhẹ nhàng hơn

### 3.3 KPI Cards (Ô số liệu)

Grid 4 cột, mỗi ô hiển thị 1 metric:

| # | Metric | Màu số | Mô tả | Logic tính |
|---|--------|--------|-------|------------|
| 1 | Lịch hẹn hôm nay | Tím (#534AB7) | Tổng số BN có hẹn hôm nay + sub-text "X chưa đến" | Đếm appointments có ngày = hôm nay |
| 2 | Chờ khám | Amber (#BA7517) | BN đã có hồ sơ, đang chờ Technician gọi | Đếm BN có trạng thái = "Chờ khám" |
| 3 | Đang khám | Blue (#185FA5) | BN đang ở Pre-Exam hoặc EMR | Đếm BN có trạng thái = "Đang khám" |
| 4 | Hoàn thành | Teal (#0F6E56) | BN đã khám xong trong ngày | Đếm BN có trạng thái = "Hoàn thành" |

**Quy tắc:**
- Số liệu cập nhật real-time (polling mỗi 30 giây hoặc WebSocket).
- Ô "Lịch hẹn hôm nay" đặc biệt: hiện tổng + sub-text nhỏ bên cạnh cho biết bao nhiêu chưa đến (giúp lễ tân biết có cần gọi nhắc không).
- Click vào 1 ô → auto filter bảng bên dưới theo trạng thái tương ứng.

### 3.4 Bảng danh sách bệnh nhân (Main table)

Đây là thành phần chính, chiếm phần lớn màn hình.

#### Cột bảng

| # | Cột | Kiểu | Sortable | Mô tả |
|---|-----|------|----------|-------|
| 1 | STT | Number | Không | Số thứ tự trong danh sách hiển thị |
| 2 | Họ tên | Text, font-weight: 500 | Có | Tên BN. Click → mở hồ sơ Intake (xem/sửa thông tin cá nhân). |
| 3 | Năm sinh | Number (4 digits) | Có | Chỉ hiện năm, không cần ngày tháng đầy đủ |
| 4 | Giờ hẹn | Time (HH:mm) | Có (mặc định) | BN có hẹn → hiện giờ hẹn. Walk-in → hiện "—". Bảng sắp xếp mặc định theo cột này. |
| 5 | Nguồn | Badge text | Có | "Hẹn" (tím) hoặc "Walk-in" (coral). Phân biệt rõ 2 luồng. |
| 6 | Lý do khám | Text | Không | Lý do sơ bộ. BN "Chưa đến" hiện chữ nghiêng (italic). BN chưa nhập hiện "Chưa rõ" (italic, màu nhạt). |
| 7 | Trạng thái | Badge pill | Có | 4 trạng thái với màu riêng (chi tiết bên dưới) |
| 8 | Thao tác | Button / Icon | Không | Hành động tùy theo trạng thái (chi tiết bên dưới) |

#### Hệ thống trạng thái

| Trạng thái | Badge màu | Ý nghĩa | Cột "Thao tác" hiển thị |
|------------|-----------|---------|--------------------------|
| Chưa đến | Tím nhạt (bg: #EEEDFE, text: #3C3489) | BN có hẹn nhưng chưa đến phòng khám | Nút **"Check-in"** (outline tím) |
| Chờ khám | Amber nhạt (bg: #FAEEDA, text: #633806) | Đã có hồ sơ, đang chờ Technician | Icon ⋯ (menu thao tác) |
| Đang khám | Blue nhạt (bg: #E6F1FB, text: #0C447C) | Đang ở Pre-Exam hoặc EMR | Icon ⋯ |
| Hoàn thành | Teal nhạt (bg: #E1F5EE, text: #085041) | Đã khám xong | Icon ⋯ |

#### Visual phân biệt dòng

- Dòng BN **"Chưa đến"** (có hẹn nhưng chưa tới): nền nhẹ hơn (background-secondary) để lễ tân phân biệt bằng mắt ngay — ai đã đến, ai chưa.
- Các dòng còn lại: nền trắng bình thường.

#### Sắp xếp mặc định

- Sắp xếp theo **giờ hẹn tăng dần**.
- BN walk-in (không có giờ hẹn) xếp cuối cùng, theo giờ đến.
- Lễ tân có thể click header cột để sort lại.

#### Menu ⋯ (cho trạng thái Chờ khám, Đang khám, Hoàn thành)

| Action | Mô tả |
|--------|-------|
| Xem hồ sơ | Mở form Intake ở chế độ xem/sửa thông tin cá nhân |
| Sửa thông tin | Mở form Intake ở chế độ edit |
| Hủy lượt khám | Xóa BN khỏi hàng đợi hôm nay (yêu cầu confirm) |

### 3.5 Pagination (Footer bảng)

| Thành phần | Mô tả |
|------------|-------|
| Text trái | "Hiển thị X / Y bệnh nhân hôm nay" |
| Pagination phải | Nút ← Trước, số trang, Sau → |
| Page size | Mặc định 10 dòng/trang. Có thể đổi 10/20/50. |

---

## 4. Chức năng tìm kiếm

**Vị trí:** Ô tìm kiếm trên Action bar.

**Hành vi:**
- Lễ tân gõ SĐT hoặc tên BN → kết quả gợi ý hiện real-time (autocomplete dropdown, sau 2 ký tự).
- Tìm kiếm trong TOÀN BỘ database BN (không chỉ hôm nay).
- Mỗi kết quả gợi ý hiện: Họ tên, năm sinh, SĐT.
- Click vào kết quả → mở hồ sơ Intake của BN đó.
- Nếu không tìm thấy → hiện "Không tìm thấy. Tạo hồ sơ mới?" + link mở form Tiếp nhận.

**Mục đích:** Kiểm tra BN đã có hồ sơ chưa trước khi tạo mới (yêu cầu 1.4: không tạo trùng patient_id).

---

## 5. Chức năng filter

**Vị trí:** Pill buttons trên Action bar, bên phải.

| Filter | Điều kiện | Mặc định |
|--------|-----------|----------|
| Tất cả | Không filter | Active (mặc định) |
| Chưa đến | Trạng thái = "Chưa đến" | |
| Chờ khám | Trạng thái = "Chờ khám" | |
| Đang khám | Trạng thái = "Đang khám" | |
| Xong | Trạng thái = "Hoàn thành" | |

**Quy tắc:**
- Chỉ 1 filter active tại 1 thời điểm (single select).
- Filter active có visual khác biệt (border + background info).
- Đếm số lượng mỗi filter hiện bên cạnh label (tùy chọn).
- Click vào ô KPI cards cũng trigger filter tương ứng.

---

## 6. Luồng xử lý chính

### 6.1 BN walk-in đến phòng khám

1. BN đến quầy lễ tân.
2. Lễ tân **search SĐT/tên** trên Dashboard.
3. **Nếu tìm thấy** → BN cũ, mở hồ sơ, xác nhận thông tin → bấm "Tạo lượt khám mới" → BN vào hàng đợi "Chờ khám".
4. **Nếu không tìm thấy** → bấm **"Tiếp nhận BN mới"** → mở form SCR-003 → nhập thông tin → lưu → BN vào hàng đợi "Chờ khám".

### 6.2 BN đã hẹn đến phòng khám

1. Lễ tân thấy BN trong bảng với trạng thái **"Chưa đến"** (tím) và nút **"Check-in"**.
2. BN đến quầy → lễ tân bấm **"Check-in"**.
3. Hệ thống mở popup xác nhận nhanh: hiện tên, SĐT, lý do khám → lễ tân xác nhận "Đúng" hoặc sửa nếu cần.
4. Bấm xác nhận → trạng thái chuyển sang **"Chờ khám"**.
5. **Không cần qua form Intake** đầy đủ vì thông tin đã có từ lúc đặt hẹn.

### 6.3 BN gọi điện đặt lịch hẹn

1. Lễ tân bấm **"Đặt lịch hẹn"**.
2. Mở form đặt hẹn (SCR riêng): nhập tên, SĐT, lý do khám sơ bộ, chọn ngày + giờ hẹn.
3. Lưu → appointment xuất hiện trong bảng Dashboard vào đúng ngày hẹn, trạng thái "Chưa đến".

### 6.4 BN có hẹn nhưng không đến

- Cuối ngày, BN có trạng thái "Chưa đến" vẫn nằm trong bảng.
- Lễ tân có thể bấm ⋯ → "Đánh dấu không đến" (no-show).
- Hệ thống ghi nhận để thống kê sau.
- Tùy chọn: gửi nhắc hẹn lại (phase sau).

---

## 7. Cập nhật real-time

| Dữ liệu | Tần suất cập nhật | Phương thức |
|----------|-------------------|-------------|
| Số liệu KPI cards | Mỗi 30 giây | Polling hoặc WebSocket |
| Trạng thái BN trong bảng | Real-time | WebSocket (ưu tiên) hoặc Polling 15 giây |
| Appointment mới (từ web booking) | Real-time | WebSocket — khi BN đặt qua web, dòng mới tự xuất hiện |

**Quy tắc:** Khi trạng thái BN thay đổi (VD: Technician bắt đầu Pre-Exam → BN chuyển từ "Chờ khám" sang "Đang khám"), badge trạng thái trên Dashboard của lễ tân phải cập nhật mà không cần refresh trang.

---

## 8. Edge cases

| Tình huống | Xử lý |
|------------|-------|
| BN đến sớm hơn giờ hẹn | Lễ tân bấm Check-in bình thường, hệ thống ghi nhận giờ đến thực tế |
| BN đến trễ hơn giờ hẹn | Lễ tân bấm Check-in bình thường, BN vào cuối hàng đợi "Chờ khám" |
| BN có hẹn nhưng muốn đổi giờ | Lễ tân bấm ⋯ → "Đổi lịch hẹn" → mở form sửa appointment |
| BN walk-in khi phòng khám đầy | Lễ tân vẫn tiếp nhận bình thường, BN xếp hàng theo thứ tự |
| 2 người dùng chung 1 SĐT | Cảnh báo trùng khi tiếp nhận, cho phép tạo mới nếu lễ tân xác nhận |
| Mất kết nối mạng | Hiện banner cảnh báo "Mất kết nối" trên đầu Dashboard, dữ liệu giữ nguyên state cuối cùng |
| Appointment mới từ web xuất hiện | Dòng mới tự thêm vào bảng, KPI "Lịch hẹn hôm nay" tự tăng |
| Lễ tân mở Dashboard trước giờ làm | Hiển thị lịch hẹn cả ngày, chưa có walk-in nào, ô "Chờ khám" = 0 |

---

## 9. Responsive / UX

- **Desktop (>1024px):** Full layout như mockup, bảng 8 cột.
- **Tablet (768–1024px):** Ẩn cột "Năm sinh" và "Nguồn", KPI cards 2+2 hàng.
- **Mobile (<768px):** KPI cards stack dọc, bảng chuyển thành card list (mỗi BN = 1 card).
- **Keyboard shortcuts:**
  - `N` = mở form Tiếp nhận BN mới
  - `H` = mở form Đặt lịch hẹn
  - `/` = focus ô tìm kiếm
  - `1-4` = chuyển filter (1=Tất cả, 2=Chưa đến, 3=Chờ khám, 4=Đang khám)
- **Auto-refresh:** Trang tự cập nhật, lễ tân không cần F5.
- **Notification sound (tùy chọn):** Phát tiếng khi có appointment mới từ web hoặc BN chuyển trạng thái.

---

## 10. Mapping với yêu cầu gốc

| Yêu cầu gốc (từ đoạn chat) | Đáp ứng trong Dashboard |
|------------------------------|-------------------------|
| 1.4 Không tạo trùng patient_id | Ô tìm kiếm + cảnh báo trùng SĐT trong form intake |
| 1.4 Auto generate patient_id | Sinh tự động khi mở form tiếp nhận |
| 1.4 Search theo SĐT / tên | Ô tìm kiếm autocomplete trên action bar |
| Hệ thống đặt lịch hẹn (qua web / gọi điện) | Nút "Đặt lịch hẹn" + cột "Giờ hẹn" + cột "Nguồn" + trạng thái "Chưa đến" |
| User role: Reception = Tiếp nhận BN | Dashboard render theo role, chỉ hiện thông tin hành chính |
