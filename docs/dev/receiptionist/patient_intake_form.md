# SCR-003 | Tiếp nhận bệnh nhân mới (New patient intake form)

**Route:** `/dashboard` → Modal / Page mở khi bấm "Tiếp nhận BN mới"

**Actor:** Lễ tân (chính), Admin

**Mục tiêu:** Tạo hồ sơ bệnh nhân mới nhanh nhất có thể, đảm bảo đủ dữ liệu tối thiểu để BN vào hàng đợi khám. Dữ liệu từ form này sẽ được sử dụng xuyên suốt quy trình: Pre-Exam, EMR, Dry Eye, Reporting.

---

## 1. Mô tả chức năng

Lễ tân tạo hồ sơ mới cho bệnh nhân đến khám trực tiếp (walk-in). Form thu thập 4 nhóm thông tin: thông tin cá nhân, thông tin khám, tiền sử bệnh, và lifestyle. Sau khi lưu, hệ thống auto generate mã bệnh nhân (patient_id) và BN xuất hiện trong danh sách Dashboard với trạng thái "Chờ khám".

Form này KHÔNG dùng cho BN đặt lịch hẹn (có form riêng) và KHÔNG dùng cho BN tái khám đã có hồ sơ (dùng chức năng Check-in trên Dashboard).

---

## 2. Khi nào mở form này

- Lễ tân bấm nút **"+ Tiếp nhận BN mới"** trên Dashboard.
- BN walk-in đến phòng khám, chưa có hồ sơ trong hệ thống.

---

## 3. Layout tổng quan

Form chia 4 section theo thứ tự từ trên xuống:

| # | Section | Bắt buộc | Mô tả |
|---|---------|----------|-------|
| 1 | Thông tin cá nhân | Có (5 fields bắt buộc) | Họ tên, ngày sinh, giới tính, SĐT, email, địa chỉ, nghề nghiệp |
| 2 | Thông tin khám | Có (1 field bắt buộc) | Lý do đến khám |
| 3 | Tiền sử bệnh | Tùy chọn | Bệnh mắt, bệnh toàn thân, thuốc đang dùng, dị ứng |
| 4 | Lifestyle | Tùy chọn | Screen time, môi trường làm việc, contact lens, ghi chú |

**Header form:** Hiển thị breadcrumb "← Dashboard / Tiếp nhận bệnh nhân mới" và mã BN tự sinh (patient_id).

**Footer form:** 3 nút hành động — Hủy, Lưu, Lưu & chuyển Pre-Exam.

---

## 5. Chi tiết fields

### 5.1 Thông tin cá nhân

| Field | Kiểu | Bắt buộc | Validation | Ghi chú |
|-------|------|----------|------------|---------|
| Họ và tên | Text input | Có | Không rỗng, tối đa 100 ký tự | Chiếm 2/3 hàng (2 cột trong grid 3 cột) |
| Giới tính | Dropdown | Có | Phải chọn 1 giá trị | Options: Nam, Nữ, Khác |
| Ngày sinh | Date input | Có | Format dd/mm/yyyy, không vượt quá ngày hiện tại | Hỗ trợ date picker |
| Số điện thoại | Phone input | Có | 10–11 số, định dạng VN (bắt đầu 0) | Validate real-time, trigger kiểm tra trùng |
| Email | Email input | Không | Format email hợp lệ nếu có nhập | |
| Địa chỉ | Text input | Không | Tối đa 200 ký tự | Chiếm full hàng |
| Nghề nghiệp | Text input | Không | Tối đa 100 ký tự | Placeholder gợi ý: "Nhân viên văn phòng, Sinh viên..." |

### 5.2 Thông tin khám

| Field | Kiểu | Bắt buộc | Validation | Ghi chú |
|-------|------|----------|------------|---------|
| Lý do đến khám | Textarea | Có | Không rỗng, tối đa 500 ký tự | Placeholder gợi ý cách viết cụ thể. Hiển thị đếm ký tự. Dữ liệu này sẽ flow sang Pre-Exam Step 1 làm Chief Complaint ban đầu. |

### 5.3 Tiền sử bệnh

| Field | Kiểu | Bắt buộc | Validation | Ghi chú |
|-------|------|----------|------------|---------|
| Tiền sử bệnh mắt | Textarea | Không | Tối đa 500 ký tự | VD: Cận thị từ nhỏ, đã Lasik 2020 |
| Tiền sử bệnh toàn thân | Textarea | Không | Tối đa 500 ký tự | VD: Tiểu đường type 2, cao huyết áp |
| Thuốc đang dùng | Text input | Không | Tối đa 300 ký tự | VD: Metformin 500mg, Amlodipine 5mg |
| Dị ứng | Text input | Không | Tối đa 200 ký tự | Bao gồm cả dị ứng thuốc và dị ứng khác |

### 5.4 Lifestyle

| Field | Kiểu | Bắt buộc | Validation | Ghi chú |
|-------|------|----------|------------|---------|
| Screen time | Number input | Không | 0–24, đơn vị giờ/ngày | Dữ liệu quan trọng cho Dry Eye và Myopia Control |
| Môi trường làm việc | Dropdown | Không | Phải chọn 1 nếu có | Options: Văn phòng (máy lạnh), Ngoài trời, Nhà xưởng, Khác |
| Contact lens | Dropdown | Không | Phải chọn 1 nếu có | Options: Không đeo, Đeo hàng ngày, Đeo thỉnh thoảng |
| Ghi chú lifestyle | Text input | Không | Tối đa 300 ký tự | VD: Hay bơi lội, lái xe đêm, dùng thuốc nhỏ mắt hàng ngày |

---

## 6. Quy tắc nghiệp vụ

### 6.1 Kiểm tra trùng lặp (yêu cầu 1.4)

- Khi lễ tân nhập SĐT, hệ thống **kiểm tra real-time** trong database.
- Nếu SĐT đã tồn tại → hiển thị **thanh cảnh báo vàng** ngay dưới field SĐT:
  - Nội dung: "SĐT 0912 345 678 đã tồn tại — BN: **Nguyễn Văn An** (1985)"
  - Kèm nút **"Mở hồ sơ cũ"** → redirect đến hồ sơ BN đã có.
- Mục tiêu: tránh 1 bệnh nhân có 2 hồ sơ khác nhau.

### 6.2 Search theo SĐT / tên (yêu cầu 1.4)

- Chức năng search nằm trên **Dashboard** (ô tìm kiếm), không nằm trong form này.
- Lễ tân search trước khi bấm "Tiếp nhận BN mới" để kiểm tra BN đã có hồ sơ chưa.

### 6.3 Sau khi lưu

- Hệ thống tạo record mới với patient_id đã sinh.
- BN xuất hiện trong bảng Dashboard với trạng thái **"Chờ khám"** và nguồn **"Walk-in"**.
- Giờ đến = thời điểm lưu form.

### 6.4 Dữ liệu flow sang các module khác

| Field intake | Sử dụng tại | Cách sử dụng |
|-------------|-------------|---------------|
| Lý do khám | Pre-Exam Step 1 | Pre-fill vào Chief Complaint |
| Tiền sử bệnh | EMR Tab History | Hiển thị read-only để BS tham khảo |
| Thuốc đang dùng | EMR Tab History | Hiển thị read-only |
| Dị ứng | EMR Tab History + Plan | Hiển thị cảnh báo khi kê đơn thuốc |
| Screen time | Pre-Exam Block A | Pre-fill vào field screen time |
| Contact lens | Pre-Exam Block A | Pre-fill vào field contact lens |
| Nghề nghiệp | EMR, Dry Eye | Tham khảo khi chẩn đoán |
| Môi trường làm việc | Dry Eye | Yếu tố risk cho dry eye |

---

## 7. Hành động (Footer buttons)

| Nút | Kiểu | Hành vi |
|-----|------|---------|
| Hủy | Outline, bên trái | Hiển thị confirm "Bạn có chắc muốn hủy? Dữ liệu chưa lưu sẽ mất." → quay về Dashboard |
| Lưu | Outline, bên phải | Validate fields bắt buộc → lưu → quay về Dashboard. BN vào hàng đợi. |
| Lưu & chuyển Pre-Exam | Filled (primary), bên phải | Validate → lưu → redirect sang màn hình Pre-Exam Step 1 với BN này. Flow chính khi phòng khám vắng, BN khám ngay. |

---

## 8. Xử lý lỗi

| Tình huống | Xử lý |
|------------|-------|
| Bỏ trống field bắt buộc | Highlight field đỏ + message "Trường này không được bỏ trống" |
| SĐT sai format | Message "SĐT phải có 10–11 số và bắt đầu bằng số 0" |
| Ngày sinh không hợp lệ | Message "Ngày sinh không hợp lệ" |
| Email sai format | Message "Email không đúng định dạng" |
| Lý do khám vượt 500 ký tự | Chặn nhập thêm + message "Đã đạt giới hạn 500 ký tự" |
| Lỗi mạng khi lưu | Toast "Lưu thất bại, vui lòng thử lại" + giữ nguyên dữ liệu form |

---

## 9. Trạng thái đặc biệt

### 9.1 Form trống (mới mở)
- Tất cả fields rỗng.
- Patient_id đã được sinh sẵn và hiển thị.
- Focus tự động vào field "Họ và tên".

### 9.2 BN tái khám nhưng lễ tân lỡ bấm "Tiếp nhận mới"
- Hệ thống phát hiện qua SĐT trùng → hiện cảnh báo + nút "Mở hồ sơ cũ".
- Nếu lễ tân vẫn muốn tạo mới (trường hợp 2 người dùng chung SĐT) → cho phép, nhưng yêu cầu xác nhận.

---

## 10. Responsive / UX

- Form layout: grid 3 cột cho desktop, stack 1 cột cho tablet/mobile.
- Tab order theo thứ tự logic: họ tên → giới tính → ngày sinh → SĐT → email → địa chỉ → nghề nghiệp → lý do khám → ...
- Keyboard shortcut: `Ctrl+S` = Lưu, `Ctrl+Enter` = Lưu & chuyển Pre-Exam.
- Auto-save draft mỗi 30 giây để tránh mất dữ liệu khi mất kết nối.
