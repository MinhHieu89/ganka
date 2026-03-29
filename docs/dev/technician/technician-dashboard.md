# Technician Dashboard

> **Module:** Pre-Exam (Screening)
> **Route:** `/dashboard` (render cho role Technician)
> **Actor:** Technician / Refractionist
> **Trạng thái:** Draft v1.0

---

## 1. Mục tiêu màn hình

Cung cấp cho Technician cái nhìn tổng quan về hàng đợi bệnh nhân (BN) đang chờ Pre-Exam trong ngày, theo dõi BN đang đo, và thực hiện các thao tác chính: nhận BN, tiếp tục đo, hoàn tất chuyển bác sĩ, xử lý red flag.

**Khác biệt so với Receptionist Dashboard:** Receptionist focus tiếp nhận & quản lý lịch hẹn. Technician focus hàng đợi đo đạc & phân luồng BN.

---

## 2. Layout tổng quan

Từ trên xuống dưới:

1. **Header** — Tiêu đề + thông tin user đăng nhập
2. **KPI cards** — 4 ô thống kê nhanh
3. **Banner "Đang thực hiện"** — Hiển thị khi Technician đang đo dở 1 BN (ẩn nếu không có)
4. **Toolbar** — Filter pills + Search
5. **Bảng BN** — Danh sách BN 9 cột, menu ⋯ theo trạng thái
6. **Footer note** — Ghi chú sắp xếp

---

## 3. Header

| Thành phần | Mô tả |
|---|---|
| Tiêu đề | "Pre-Exam dashboard" (h1, 22px, font-weight 500) |
| Role badge | Pill "Technician" (nền xanh nhạt `#E6F1FB`, text `#0C447C`) |
| Tên user | Hiển thị tên Technician đang đăng nhập (13px, text-secondary) |

---

## 4. KPI Cards

Grid 4 cột, gap 12px. Mỗi card gồm: label (12px, muted), giá trị (24px, bold, màu theo ngữ nghĩa), sub-text (11px, hint).

| # | Label | Giá trị | Màu | Sub-text | Logic |
|---|---|---|---|---|---|
| 1 | Chờ khám | Số BN | Amber (`#BA7517`) | "trong hàng đợi" | `COUNT(status = "Chờ khám")` trong ngày |
| 2 | Đang đo | Số BN | Blue (`#185FA5`) | "BN của tôi" | `COUNT(status = "Đang đo" AND assigned_to = current_technician)` |
| 3 | Hoàn tất Pre-Exam | Số BN | Teal (`#0F6E56`) | "hôm nay" | `COUNT(status = "Hoàn tất" AND completed_date = today)` |
| 4 | Red flag | Số BN | Red (`#A32D2D`) | "fast-track hôm nay" | `COUNT(is_red_flag = true AND date = today)` |

---

## 5. Banner "Đang thực hiện Pre-Exam"

### 5.1 Điều kiện hiển thị

Hiển thị khi Technician hiện tại có **đúng 1 BN** đang ở trạng thái "Đang đo" được gán cho mình. Ẩn nếu không có.

### 5.2 Nội dung

| Thành phần | Mô tả |
|---|---|
| Label | "Đang thực hiện Pre-Exam" (12px, bold, `#0C447C`) |
| Thông tin BN | `{Họ tên}` — `{Năm sinh}` · `{Nhóm bệnh dự kiến}` · check-in `{giờ check-in}` |
| Nút | "Tiếp tục đo" (primary, background `#534AB7`, text white) |

### 5.3 Hành vi

- Click "Tiếp tục đo" → điều hướng đến màn hình Pre-Exam Step 1 (hoặc step đang dở), dữ liệu đã nhập được giữ nguyên (auto-save).
- Banner có nền `#E6F1FB`, border `0.5px solid #B5D4F4`, border-radius `8px`.

### 5.4 Ràng buộc

- Hệ thống chỉ cho phép **1 BN "Đang đo" / Technician** tại 1 thời điểm.
- Nếu Technician click "Nhận BN" trên BN khác khi đang đo dở → hiện popup xác nhận: *"Bạn đang đo BN {tên}. Muốn tạm dừng và nhận BN mới?"*
  - "Tạm dừng & nhận BN mới" → BN cũ chuyển về "Chờ khám" (giữ data, bỏ gán Technician), BN mới chuyển sang "Đang đo".
  - "Hủy" → đóng popup, không thay đổi.

---

## 6. Toolbar

### 6.1 Filter pills

Dãy ngang, mỗi pill hiển thị `{label}` + `{count}`. Pill active có nền đen (text trắng), pill inactive có border nhạt.

| Pill | Filter logic | Thứ tự |
|---|---|---|
| Tất cả | Hiện tất cả BN trong ngày | 1 |
| Chờ khám | `status = "Chờ khám"` | 2 |
| Đang đo | `status = "Đang đo" AND assigned_to = current_technician` | 3 |
| Hoàn tất | `status = "Hoàn tất"` | 4 |
| Red flag | `is_red_flag = true` | 5 |

**Lưu ý:** Tab "Đang đo" chỉ hiện BN của Technician hiện tại, không hiện BN đang được Technician khác đo.

### 6.2 Search

- Input text, placeholder: "Tìm BN theo tên, SĐT..."
- Tìm kiếm real-time (debounce 300ms), filter trên `patient_name` và `phone`.
- Kết hợp với filter pill đang active.

---

## 7. Bảng bệnh nhân

### 7.1 Cấu trúc cột

| # | Tên cột | Width | Nội dung | Ghi chú |
|---|---|---|---|---|
| 1 | # | 32px | Số thứ tự | Auto-increment theo vị trí hiển thị |
| 2 | Họ tên | 22% | `patient_name` | font-weight 500. BN red flag: text đỏ `#A32D2D` |
| 3 | Sinh | 46px | `birth_year` | Chỉ hiện năm sinh (4 chữ số) |
| 4 | Check-in | 60px | `checkin_time` | Format `HH:mm` |
| 5 | Chờ | 54px | Thời gian chờ | Tính realtime từ `checkin_time` đến hiện tại |
| 6 | Lý do khám | auto | `chief_complaint` | Text ellipsis nếu dài. BN red flag: text đỏ |
| 7 | Loại | 54px | Mới / Tái khám | Pill nhỏ có border |
| 8 | Trạng thái | 82px | Badge màu | Theo 4 trạng thái |
| 9 | ⋯ | 40px | Icon 3 chấm dọc | Mở dropdown menu thao tác |

### 7.2 Sắp xếp mặc định

- Sắp xếp theo thời gian check-in tăng dần (FIFO) — BN check-in trước lên trên.
- BN "Đang đo" (của Technician hiện tại) luôn ghim lên đầu bảng.
- BN "Hoàn tất" hiển thị mờ (opacity 0.55) để focus BN cần xử lý.

### 7.3 Cột "Chờ" — Quy tắc hiển thị

| Trạng thái | Hiển thị | Màu |
|---|---|---|
| Chờ khám | `{N} ph` (tính realtime) | Amber `#BA7517` mặc định; đỏ `#A32D2D` nếu ≥ 25 phút |
| Đang đo | `{N} ph` (tính từ check-in) | Amber `#BA7517` |
| Red flag | `—` | Text tertiary |
| Hoàn tất | `—` | Text tertiary |

Cập nhật mỗi 60 giây.

### 7.4 Cột "Loại" — Quy tắc

| Giá trị | Điều kiện |
|---|---|
| Mới | BN không có lượt khám trước trong hệ thống |
| Tái khám | BN có ≥ 1 lượt khám trước |

Hiển thị dạng pill nhỏ có border nhạt, font 11px.

### 7.5 Cột "Trạng thái" — Badge

| Trạng thái | Background | Text color | Label |
|---|---|---|---|
| Chờ khám | `#FAEEDA` | `#854F0B` | Chờ khám |
| Đang đo | `#E6F1FB` | `#0C447C` | Đang đo |
| Red flag | `#FCEBEB` | `#791F1F` | Red flag |
| Hoàn tất | `#E1F5EE` | `#085041` | Hoàn tất |

---

## 8. Menu thao tác (⋯)

Menu dropdown mở từ icon 3 chấm dọc. Nội dung khác nhau theo trạng thái BN.

### 8.1 Ma trận thao tác × trạng thái

| Thao tác | Chờ khám | Đang đo | Red flag | Hoàn tất |
|---|---|---|---|---|
| Nhận BN | ✅ primary | — | — | — |
| Tiếp tục đo | — | ✅ primary | — | — |
| Hoàn tất chuyển BS | — | ✅ | — | — |
| Trả lại hàng đợi | — | ✅ | — | — |
| Chuyển BS ngay | — | ✅ (đỏ) | — | — |
| Xem kết quả | ✅ | ✅ | ✅ | ✅ |

### 8.2 Chi tiết từng thao tác

#### 8.2.1 Nhận BN

- **Khả dụng:** Chờ khám
- **Hành vi:** Click → chuyển trạng thái BN sang "Đang đo", gán `assigned_to = current_technician`, ghi `pre_exam_start_time = now()`. Tự động điều hướng đến màn hình Pre-Exam Step 1.
- **Chuyển trạng thái:** Chờ khám → Đang đo
- **Edge case:** Nếu Technician đang đo BN khác → hiện popup xác nhận tạm dừng (xem mục 5.4).
- **Hiển thị:** Menu item primary (text tím `#534AB7`, font-weight 500).

#### 8.2.2 Tiếp tục đo

- **Khả dụng:** Đang đo (chỉ BN được gán cho Technician hiện tại)
- **Hành vi:** Mở lại màn hình Pre-Exam tại step đang thực hiện. Dữ liệu đã nhập được giữ nguyên (auto-save).
- **Không chuyển trạng thái.**
- **Hiển thị:** Menu item primary.

#### 8.2.3 Hoàn tất chuyển BS

- **Khả dụng:** Đang đo
- **Hành vi:** Validate dữ liệu core bắt buộc:
  - Chief complaint (lý do khám) — bắt buộc
  - UCVA (thị lực cơ bản) OD & OS — bắt buộc
  - Red flag check (đã check qua) — bắt buộc
- **Nếu đủ:** Chuyển trạng thái, BN vào hàng đợi Doctor.
- **Nếu thiếu:** Hiện cảnh báo danh sách field còn thiếu, không cho hoàn tất.
- **Chuyển trạng thái:** Đang đo → Hoàn tất (chờ BS khám)
- **Hiển thị:** Menu item bình thường (có icon check).

#### 8.2.4 Trả lại hàng đợi

- **Khả dụng:** Đang đo
- **Hành vi:** Popup xác nhận: *"Trả BN {tên} về hàng đợi? Dữ liệu đã nhập sẽ được giữ nguyên."*
  - "Xác nhận" → BN quay về "Chờ khám", bỏ gán Technician, giữ nguyên data đã nhập. Technician khác hoặc chính Technician này có thể nhận lại sau.
  - "Hủy" → đóng popup.
- **Chuyển trạng thái:** Đang đo → Chờ khám
- **Trường hợp sử dụng:** BN ra ngoài tạm, Technician nghỉ giải lao, chuyển ca.
- **Hiển thị:** Menu item bình thường.

#### 8.2.5 Chuyển BS ngay

- **Khả dụng:** Đang đo (khi phát hiện red flag)
- **Hành vi:** Popup xác nhận red flag:
  - Dropdown chọn lý do red flag (bắt buộc):
    - Đau mắt nhiều
    - Giảm thị lực đột ngột
    - Triệu chứng lệch 1 bên rõ
    - Khác (kèm text field ghi chú)
  - Nút "Chuyển BS ngay" (đỏ, primary) + "Hủy"
- **Sau xác nhận:** BN bỏ qua Step 2, chuyển thẳng vào hàng đợi ưu tiên Doctor. Ghi nhận `is_red_flag = true`, `red_flag_reason`, `red_flag_time`.
- **Chuyển trạng thái:** Đang đo → Red flag (ưu tiên BS khám)
- **Hiển thị:** Menu item đỏ (text `#A32D2D`), có separator phía trên để tách biệt.

#### 8.2.6 Xem kết quả

- **Khả dụng:** Tất cả trạng thái
- **Hành vi:** Mở panel/popup read-only hiển thị:
  - Thông tin cá nhân BN
  - Tiền sử bệnh, lần khám trước (nếu tái khám)
  - Dữ liệu Pre-Exam đã thu thập (nếu có): UCVA, Auto-Ref, IOP, red flag check...
- **Technician không sửa được hồ sơ BN** — chỉ Receptionist có quyền sửa thông tin cá nhân.
- **Hiển thị:** Menu item bình thường, luôn nằm cuối menu, có separator phía trên.

---

## 9. Flow đặc biệt: BS yêu cầu đo bổ sung

Khi Doctor khám xong BN red flag (hoặc BN thường) và cần thêm dữ liệu Pre-Exam:

1. Doctor chỉ định "Đo bổ sung" trong EMR, kèm ghi chú cụ thể (vd: "Cần đo IOP + Keratometry").
2. BN xuất hiện lại trong hàng đợi Technician với:
   - Trạng thái: "Chờ khám"
   - Badge đặc biệt: "Đo bổ sung" (nền tím nhạt)
   - Tooltip/ghi chú hiển thị chỉ định từ BS
3. Technician nhận BN → mở Pre-Exam screen với dữ liệu cũ đã có, chỉ cần bổ sung phần BS yêu cầu.
4. Hoàn tất → chuyển lại BS.

---

## 10. Quyền hạn Technician

| Hành động | Được phép | Ghi chú |
|---|---|---|
| Xem danh sách BN chờ khám | ✅ | Tất cả BN "Chờ khám" trong ngày |
| Nhận BN & thực hiện Pre-Exam | ✅ | Gán cho mình, mở Pre-Exam screen |
| Nhập dữ liệu đo lường | ✅ | Trong Pre-Exam screen |
| Chuyển BN sang BS | ✅ | Hoàn tất hoặc Red flag fast-track |
| Trả BN về hàng đợi | ✅ | Tạm dừng, giữ data |
| Sửa hồ sơ BN (thông tin cá nhân) | ❌ | Thuộc quyền Receptionist |
| Sửa kết quả Pre-Exam đã hoàn tất | ❌ | Phải qua BS chỉ định đo lại |
| Xem BN đang đo của Technician khác | ❌ | Chỉ thấy BN được gán cho mình |
| Hủy lượt khám | ❌ | Thuộc quyền Receptionist |

---

## 11. Realtime & Auto-refresh

| Dữ liệu | Tần suất cập nhật |
|---|---|
| KPI cards | Mỗi 30 giây hoặc khi có thay đổi trạng thái |
| Cột "Chờ" (thời gian chờ) | Mỗi 60 giây |
| Danh sách BN (thêm/bớt/đổi trạng thái) | Realtime (WebSocket hoặc polling 10s) |
| Banner "Đang thực hiện" | Realtime |

---

## 12. Edge cases

| Case | Xử lý |
|---|---|
| Technician đang đo, click "Nhận BN" khác | Popup xác nhận tạm dừng BN hiện tại (mục 5.4) |
| BN "Chờ khám" bị Technician khác nhận trước | Xóa khỏi danh sách / hiện toast "BN đã được nhận bởi {tên Technician khác}" |
| BN "Chờ khám" bị Receptionist hủy lượt khám | Xóa khỏi danh sách realtime |
| Không có BN nào trong hàng đợi | Hiện empty state: "Không có bệnh nhân chờ khám" |
| BS yêu cầu đo bổ sung | BN xuất hiện lại ở "Chờ khám" với badge "Đo bổ sung" (mục 9) |
| Technician trả BN về hàng đợi | BN quay về "Chờ khám", giữ data, giữ vị trí FIFO theo check-in gốc |

---

## 13. Ghi chú Data Structure

Tuân thủ quy tắc chung từ đoạn chat nhóm Ganka28:

- **Visit-based**: Mỗi lượt khám là 1 record riêng, không overwrite dữ liệu lượt trước.
- **OD/OS tách riêng**: Mọi dữ liệu đo lường ghi riêng cho từng mắt.
- **Structured data ưu tiên**: Dùng dropdown, checkbox thay vì free text khi có thể.
- **Có unit rõ ràng**: Mọi giá trị đo phải kèm đơn vị.
- **Track lịch sử**: Ghi nhận timestamp, người thực hiện cho mỗi thay đổi trạng thái.
