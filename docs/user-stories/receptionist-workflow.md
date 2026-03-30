# User Stories: Quy Trình Lễ Tân (Receptionist Workflow)

**Phase:** 14 - Implement Receptionist Role Flow
**Ngày tạo:** 2026-03-28
**Yêu cầu liên quan:** RCP-01, RCP-02, RCP-03, RCP-04, RCP-05, RCP-06, RCP-07
**Số lượng user stories:** 16

---

## Dashboard Lễ Tân (SCR-002a)

### US-RCP-001: Lễ tân xem dashboard với KPI và danh sách bệnh nhân hôm nay

**Là một** lễ tân,
**Tôi muốn** xem dashboard với 4 ô KPI (Lịch hẹn hôm nay, Chờ khám, Đang khám, Hoàn thành) và bảng danh sách bệnh nhân theo ngày,
**Để** nắm bắt toàn bộ tình hình hàng đợi bệnh nhân và thực hiện các thao tác tiếp nhận, check-in, đặt lịch ngay trên một màn hình duy nhất.

**Yêu cầu liên quan:** RCP-01
**Quyết định liên quan:** D-01, D-02, D-03, D-04

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân đăng nhập hệ thống với role Receptionist -> Hệ thống redirect đến `/dashboard` và render giao diện riêng cho lễ tân (khác với giao diện bác sĩ/kỹ thuật viên)
2. Phần KPI hiển thị 4 ô số liệu: Lịch hẹn hôm nay (tím), Chờ khám (amber), Đang khám (xanh dương), Hoàn thành (xanh lá)
3. Ô "Lịch hẹn hôm nay" hiển thị tổng số lịch hẹn và sub-text "X chưa đến"
4. Bảng danh sách hiển thị các cột: STT, Họ tên, Năm sinh, Giờ hẹn, Nguồn (Hẹn/Walk-in), Lý do khám, Trạng thái, Thao tác
5. Bảng sắp xếp mặc định theo giờ hẹn (sớm nhất lên trước)
6. KPI cập nhật tự động mỗi 30 giây bằng polling
7. Bảng danh sách cập nhật tự động mỗi 15 giây

**Trường hợp ngoại lệ:**
1. Không có bệnh nhân nào trong ngày -> Bảng hiển thị trạng thái rỗng, KPI hiển thị số 0
2. Lễ tân click vào 1 ô KPI -> Bảng tự động lọc theo trạng thái tương ứng
3. Bệnh nhân walk-in không có giờ hẹn -> Cột "Giờ hẹn" hiển thị "—"

**Trường hợp lỗi:**
1. Lỗi kết nối mạng -> Hệ thống hiển thị cảnh báo và giữ lại dữ liệu cũ trên màn hình
2. Polling thất bại -> Hệ thống retry lần kế tiếp, không mất dữ liệu hiển thị

#### Ghi chú kỹ thuật
- Route: `/dashboard` với role-based rendering (Receptionist vs Clinical)
- Polling: 30s cho KPI, 15s cho bảng danh sách
- API: GET /api/scheduling/receptionist-dashboard

---

### US-RCP-002: Lễ tân lọc bệnh nhân theo trạng thái

**Là một** lễ tân,
**Tôi muốn** lọc danh sách bệnh nhân theo 4 trạng thái (Chưa đến, Chờ khám, Đang khám, Hoàn thành) hoặc xem tất cả,
**Để** tập trung vào nhóm bệnh nhân cần xử lý tiếp theo (ví dụ: chỉ xem những bệnh nhân "Chưa đến" để gọi nhắc).

**Yêu cầu liên quan:** RCP-01
**Quyết định liên quan:** D-03

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân nhìn thấy thanh filter dạng pill buttons: Tất cả, Chưa đến, Chờ khám, Đang khám, Xong
2. Mặc định chọn "Tất cả" -> Bảng hiển thị toàn bộ bệnh nhân trong ngày
3. Lễ tân click "Chưa đến" -> Bảng chỉ hiển thị bệnh nhân có trạng thái "Chưa đến" (tím)
4. Lễ tân click "Chờ khám" -> Bảng chỉ hiển thị bệnh nhân có trạng thái "Chờ khám" (amber)
5. Số lượng bệnh nhân hiển thị cập nhật ngay khi chuyển filter
6. Click vào ô KPI cũng lọc bảng tương tự như click pill button

**Trường hợp ngoại lệ:**
1. Không có bệnh nhân nào ở trạng thái đã chọn -> Bảng hiển thị trạng thái rỗng: "Không có bệnh nhân"
2. Lễ tân click nhanh nhiều filter liên tục -> Hệ thống cancel request trước và chỉ thực hiện request mới nhất

**Trường hợp lỗi:**
1. Lỗi lọc dữ liệu -> Hệ thống hiển thị toast lỗi và giữ lại filter trước đó

---

### US-RCP-003: Lễ tân tìm kiếm bệnh nhân bằng số điện thoại hoặc tên

**Là một** lễ tân,
**Tôi muốn** tìm kiếm bệnh nhân theo số điện thoại hoặc tên ngay trên dashboard,
**Để** nhanh chóng tra cứu hồ sơ bệnh nhân cũ khi họ đến khám hoặc gọi điện.

**Yêu cầu liên quan:** RCP-01
**Quyết định liên quan:** D-01

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân nhập số điện thoại hoặc tên bệnh nhân vào ô tìm kiếm trên action bar
2. Sau 2 ký tự, hệ thống bắt đầu autocomplete real-time, hiển thị danh sách kết quả phù hợp
3. Kết quả hiển thị: họ tên, năm sinh, mã bệnh nhân, số điện thoại
4. Lễ tân click vào 1 kết quả -> Mở popup tạo lượt khám walk-in (SCR-005 Flow B) cho bệnh nhân cũ

**Trường hợp ngoại lệ:**
1. Không tìm thấy bệnh nhân -> Hiển thị "Không tìm thấy bệnh nhân. Bấm 'Tiếp nhận BN mới' để tạo hồ sơ."
2. Nhiều bệnh nhân trùng tên -> Hiển thị danh sách tất cả, phân biệt bằng năm sinh và số điện thoại
3. Lễ tân xóa ô tìm kiếm -> Danh sách trở về trạng thái bình thường (hiển thị bệnh nhân trong ngày)

**Trường hợp lỗi:**
1. Lỗi search API -> Hệ thống hiển thị toast lỗi và giữ ô search có thể nhập lại

#### Ghi chú kỹ thuật
- Reuse `usePatientSearch` hook (debounced, autocomplete)
- Search toàn bộ database bệnh nhân, không chỉ bệnh nhân hôm nay

---

## Tiếp Nhận Bệnh Nhân Mới (SCR-003)

### US-RCP-004: Lễ tân tiếp nhận bệnh nhân mới với form nhập liệu 4 phần

**Là một** lễ tân,
**Tôi muốn** tạo hồ sơ bệnh nhân mới qua form 4 phần (Thông tin cá nhân, Thông tin khám, Tiền sử bệnh, Lifestyle),
**Để** thu thập đủ thông tin cần thiết trước khi bệnh nhân vào hàng đợi khám.

**Yêu cầu liên quan:** RCP-02
**Quyết định liên quan:** D-07, D-08

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân bấm nút "Tiếp nhận BN mới" trên dashboard -> Hệ thống mở form với 4 section, tất cả expand mặc định
2. Section 1 (Thông tin cá nhân): họ tên (bắt buộc), giới tính (bắt buộc), ngày sinh (bắt buộc), số điện thoại (bắt buộc), email, địa chỉ, nghề nghiệp
3. Section 2 (Thông tin khám): lý do đến khám (bắt buộc) với đếm ký tự (tối đa 500)
4. Section 3 (Tiền sử bệnh): tiền sử bệnh mắt, bệnh toàn thân, thuốc đang dùng, dị ứng - tất cả tùy chọn
5. Section 4 (Lifestyle): screen time (giờ/ngày), môi trường làm việc (dropdown), contact lens (dropdown), ghi chú - tất cả tùy chọn
6. Lễ tân điền đầy đủ thông tin bắt buộc và bấm "Lưu" -> Hệ thống tạo hồ sơ mới với mã bệnh nhân tự sinh (GK-YYYY-NNNN)
7. Bệnh nhân xuất hiện trên dashboard với trạng thái "Chờ khám"

**Trường hợp ngoại lệ:**
1. Lễ tân bấm "Lưu & Chuyển tiền khám" -> Hệ thống lưu hồ sơ và tự động chuyển bệnh nhân sang giai đoạn Pre-Exam (không qua Reception)
2. Lễ tân bấm "Hủy" -> Hệ thống đóng form, không lưu dữ liệu, quay về dashboard
3. Các section có thể thu gọn/mở rộng bằng cách click vào header section

**Trường hợp lỗi:**
1. Thiếu trường bắt buộc -> Hệ thống highlight trường lỗi và hiển thị thông báo "Vui lòng điền đầy đủ thông tin bắt buộc"
2. Số điện thoại không đúng định dạng -> Hệ thống hiển thị lỗi "Số điện thoại phải có 10-11 số và bắt đầu bằng 0"
3. Ngày sinh vượt quá ngày hiện tại -> Hệ thống hiển thị lỗi "Ngày sinh không hợp lệ"

#### Ghi chú kỹ thuật
- Form pattern: React Hook Form + Zod schema
- Route: Từ dashboard mở form (page hoặc modal)
- API: POST /api/patients

---

### US-RCP-005: Hệ thống phát hiện trùng số điện thoại khi đăng ký bệnh nhân

**Là một** lễ tân,
**Tôi muốn** được cảnh báo khi nhập số điện thoại đã tồn tại trong hệ thống,
**Để** tránh tạo trùng hồ sơ cho cùng một bệnh nhân.

**Yêu cầu liên quan:** RCP-02
**Quyết định liên quan:** D-07

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân nhập số điện thoại vào form tiếp nhận bệnh nhân mới
2. Hệ thống kiểm tra real-time số điện thoại trong database
3. Nếu số điện thoại đã tồn tại -> Hiển thị thanh cảnh báo vàng ngay dưới field số điện thoại: "SĐT 0912 345 678 đã tồn tại -- BN: **Nguyễn Văn An** (1985)"
4. Thanh cảnh báo có nút "Mở hồ sơ cũ" -> Click sẽ redirect đến hồ sơ bệnh nhân đã có

**Trường hợp ngoại lệ:**
1. Số điện thoại chưa tồn tại -> Không hiển thị cảnh báo, lễ tân tiếp tục nhập thông tin bình thường
2. Lễ tân vẫn muốn tạo hồ sơ mới dù trùng số điện thoại -> Hệ thống cho phép (trường hợp người thân cùng số)

**Trường hợp lỗi:**
1. Lỗi kiểm tra trùng số điện thoại -> Hệ thống cho phép tiếp tục nhập (không chặn form), hiển thị cảnh báo "Không thể kiểm tra trùng SĐT"

#### Ghi chú kỹ thuật
- Debounce kiểm tra 500ms sau khi ngừng gõ
- API: GET /api/patients/check-duplicate?phone={phone}

---

### US-RCP-006: Lễ tân lưu và chuyển tiền khám (auto-advance to Pre-Exam)

**Là một** lễ tân,
**Tôi muốn** lưu hồ sơ bệnh nhân và tự động chuyển bệnh nhân sang giai đoạn Pre-Exam (đo khúc xạ/thị lực),
**Để** giảm một bước thao tác thủ công và bệnh nhân được vào khám nhanh hơn.

**Yêu cầu liên quan:** RCP-02
**Quyết định liên quan:** D-08

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân điền đầy đủ thông tin bắt buộc trên form tiếp nhận
2. Lễ tân bấm nút "Lưu & Chuyển tiền khám" (primary button)
3. Hệ thống lưu hồ sơ bệnh nhân mới
4. Hệ thống tự động tạo lượt khám (Visit) với WorkflowStage = PreExam (bỏ qua Reception)
5. Bệnh nhân xuất hiện trên dashboard bác sĩ/kỹ thuật viên ở giai đoạn Pre-Exam
6. Trên dashboard lễ tân, bệnh nhân hiển thị với trạng thái "Đang khám" (vì đã ở Pre-Exam)

**Trường hợp ngoại lệ:**
1. Lễ tân bấm "Lưu" (không chuyển tiền khám) -> Hệ thống chỉ lưu hồ sơ, bệnh nhân ở trạng thái "Chờ khám" (Reception)

**Trường hợp lỗi:**
1. Lỗi tạo Visit -> Hệ thống vẫn lưu hồ sơ bệnh nhân thành công, hiển thị toast: "Đã lưu hồ sơ nhưng chưa thể chuyển tiền khám. Vui lòng chuyển thủ công."
2. Validation lỗi -> Hệ thống không lưu và highlight các trường cần sửa

#### Ghi chú kỹ thuật
- Button "Lưu & Chuyển tiền khám" gọi 2 API liên tiếp: POST /api/patients rồi POST /api/clinical/visits (với stage = PreExam)
- Data flow: Lý do khám -> Chief Complaint, Tiền sử bệnh -> EMR History, Dị ứng -> EMR allergy warnings

---

## Đặt Lịch Hẹn (SCR-004)

### US-RCP-007: Lễ tân đặt lịch hẹn cho bệnh nhân có hồ sơ

**Là một** lễ tân,
**Tôi muốn** đặt lịch hẹn cho bệnh nhân đã có hồ sơ trong hệ thống bằng cách search tên hoặc số điện thoại,
**Để** bệnh nhân có lịch hẹn và đến đúng ngày giờ đã hẹn.

**Yêu cầu liên quan:** RCP-03
**Quyết định liên quan:** D-09, D-10, D-12

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân bấm nút "Đặt lịch hẹn" trên dashboard -> Hệ thống mở trang `/appointments/new` với layout 2 cột
2. Cột trái: lễ tân nhập số điện thoại hoặc tên vào ô tìm kiếm
3. Hệ thống tìm thấy bệnh nhân -> Hiển thị thanh xanh lá: "Đã tìm thấy: **Lê Minh Châu** (1978) -- BN-20260110-0045"
4. Thông tin bệnh nhân hiển thị dạng card read-only: họ tên, SĐT, năm sinh, giới tính, lần khám gần nhất
5. Lễ tân nhập lý do khám (bắt buộc), chọn bác sĩ chỉ định (tùy chọn, mặc định = BS lần khám trước), ghi chú (tùy chọn)
6. Cột phải: lễ tân chọn ngày trên calendar mini và slot giờ trên grid khung giờ (sáng/chiều, slot 30 phút)
7. Grid hiển thị slot trống (xanh), đã đặt (xám), đang chọn (tím)
8. Thanh xác nhận ở cuối hiển thị tóm tắt: tên BN, ngày giờ, lý do, BS -> Lễ tân đọc lại cho BN qua điện thoại
9. Lễ tân bấm "Xác nhận đặt hẹn" -> Hệ thống tạo appointment, appointment hiển thị trên dashboard vào đúng ngày hẹn với trạng thái "Chưa đến"

**Trường hợp ngoại lệ:**
1. Tất cả slot trong ngày đã đầy -> Hệ thống hiển thị thông báo và gợi ý chọn ngày khác
2. Lễ tân chọn bác sĩ cụ thể -> Grid chỉ hiển thị slot của bác sĩ đó (1 BN/BS/slot)
3. Lễ tân chọn "Không chỉ định (BS nào trống)" -> Grid hiển thị tất cả slot tự do

**Trường hợp lỗi:**
1. Lỗi tạo appointment -> Hệ thống hiển thị toast lỗi, không đóng form, lễ tân có thể thử lại
2. Slot vừa được đặt bởi người khác -> Hệ thống hiển thị "Slot này vừa được đặt. Vui lòng chọn slot khác."

#### Ghi chú kỹ thuật
- Route: `/appointments/new` (full page, không phải dialog)
- Slot: 30 phút cố định, lấy giờ hoạt động từ ClinicSchedule entity
- API: POST /api/scheduling/appointments

---

### US-RCP-008: Lễ tân đặt lịch hẹn cho bệnh nhân mới (qua điện thoại, chỉ lưu tên + SĐT)

**Là một** lễ tân,
**Tôi muốn** đặt lịch hẹn cho bệnh nhân mới chưa có hồ sơ bằng cách nhập tên và số điện thoại,
**Để** bệnh nhân có lịch hẹn mà không cần tạo hồ sơ đầy đủ lúc gọi điện (thông tin đầy đủ sẽ bổ sung khi đến check-in).

**Yêu cầu liên quan:** RCP-03
**Quyết định liên quan:** D-09, D-11

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân search số điện thoại trên form đặt lịch hẹn -> Hệ thống không tìm thấy
2. Hiển thị thanh vàng: "Không tìm thấy BN với SĐT này. Nhập thông tin bên dưới để tạo hẹn cho BN mới."
3. Form chuyển sang chế độ BN mới: hiển thị các field nhập tay: Họ tên (bắt buộc), SĐT (bắt buộc, auto-fill từ ô search), Lý do khám (bắt buộc), Bác sĩ chỉ định (tùy chọn), Ghi chú (tùy chọn)
4. Lễ tân chọn ngày và slot giờ trên cột phải
5. Lễ tân bấm "Xác nhận đặt hẹn" -> Hệ thống tạo appointment với thông tin guest (GuestName, GuestPhone, GuestReason) trên Appointment record
6. Hệ thống KHÔNG tạo Patient record -> Patient record chỉ được tạo khi BN đến check-in

**Trường hợp ngoại lệ:**
1. Số điện thoại không đúng định dạng -> Hiển thị lỗi validation
2. Bệnh nhân mới gọi lại đặt thêm lịch -> Hệ thống cho phép nhiều appointment cho cùng guest phone

**Trường hợp lỗi:**
1. Lỗi tạo appointment -> Hệ thống hiển thị toast lỗi và giữ lại dữ liệu đã nhập

#### Ghi chú kỹ thuật
- Appointment.PatientId = null cho guest booking
- Thông tin lưu trên Appointment: GuestName, GuestPhone, GuestReason
- Patient record được tạo khi check-in (SCR-005)

---

### US-RCP-009: Hệ thống hiển thị slot trống theo bác sĩ và ngày

**Là một** lễ tân,
**Tôi muốn** nhìn thấy các slot giờ trống và đã đặt theo từng ngày và bác sĩ trên grid khung giờ,
**Để** chọn được slot phù hợp cho bệnh nhân một cách nhanh chóng.

**Yêu cầu liên quan:** RCP-03
**Quyết định liên quan:** D-10, D-12

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân chọn 1 ngày trên calendar mini -> Grid khung giờ cập nhật hiển thị các slot của ngày đó
2. Grid chia theo buổi: Sáng và Chiều, mỗi slot 30 phút
3. Giờ hoạt động lấy từ ClinicSchedule entity (VD: T2-T6 13:00-20:00, T7-CN 08:00-12:00)
4. Mỗi slot hiển thị 3 trạng thái: Trống (xanh lá), Đã đặt (xám), Đang chọn (tím)
5. Header grid hiển thị "X slot trống / Y tổng" để lễ tân biết còn bao nhiêu chỗ

**Trường hợp ngoại lệ:**
1. Ngày lễ hoặc ngày nghỉ -> Grid không hiển thị slot (hoặc hiển thị "Ngày nghỉ")
2. Lễ tân chọn bác sĩ -> Grid chỉ hiển thị slot của bác sĩ đó, mỗi slot tối đa 1 bệnh nhân
3. Lễ tân chọn "BS nào trống" -> Grid hiển thị slot tự do, không giới hạn theo bác sĩ

**Trường hợp lỗi:**
1. Lỗi tải lịch hẹn -> Hệ thống hiển thị toast lỗi và grid hiển thị trạng thái loading

#### Ghi chú kỹ thuật
- API: GET /api/scheduling/available-slots?date={date}&doctorId={doctorId}
- ClinicSchedule entity cung cấp giờ hoạt động theo ngày trong tuần
- Slot 30 phút cố định (D-10)

---

## Check-in & Tạo Lượt Khám (SCR-005)

### US-RCP-010: Lễ tân check-in bệnh nhân có hồ sơ đầy đủ

**Là một** lễ tân,
**Tôi muốn** check-in bệnh nhân có hồ sơ đầy đủ bằng popup xác nhận nhanh,
**Để** đưa bệnh nhân vào hàng đợi "Chờ khám" chỉ trong vài giây mà không cần nhập thêm thông tin.

**Yêu cầu liên quan:** RCP-04
**Quyết định liên quan:** D-05, D-06

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân bấm nút "Check-in" trên cột Thao tác của bệnh nhân có trạng thái "Chưa đến"
2. Hệ thống kiểm tra hồ sơ: có đủ 5 trường bắt buộc (họ tên, ngày sinh, giới tính, SĐT, lý do khám)
3. Hệ thống mở popup hiển thị thông tin bệnh nhân: avatar initials (tím), họ tên, mã BN, giờ hẹn, năm sinh, giới tính, SĐT, nghề nghiệp, lý do khám, lần khám gần nhất
4. Popup hiển thị note xanh: "Xác nhận thông tin với BN trước khi check-in. Nếu cần sửa, bấm Sửa thông tin."
5. Lễ tân bấm "Xác nhận check-in" -> Hệ thống chuyển trạng thái "Chưa đến" -> "Chờ khám", ghi nhận `checked_in_at` = thời điểm hiện tại
6. Popup đóng, quay về dashboard. KPI "Chưa đến" giảm 1, KPI "Chờ khám" tăng 1

**Trường hợp ngoại lệ:**
1. Lễ tân bấm "Sửa thông tin" -> Đóng popup, mở form Intake ở chế độ edit với dữ liệu pre-fill. Sau khi lưu Intake -> BN tự động check-in và chuyển "Chờ khám"
2. Lễ tân bấm "Hủy" -> Đóng popup, BN vẫn ở trạng thái "Chưa đến"

**Trường hợp lỗi:**
1. Lỗi check-in -> Hệ thống hiển thị toast lỗi, BN vẫn ở trạng thái "Chưa đến", popup không đóng

#### Ghi chú kỹ thuật
- Popup: Dialog modal (shadcn/ui Dialog)
- API: POST /api/scheduling/appointments/{id}/check-in

---

### US-RCP-011: Lễ tân check-in bệnh nhân có hồ sơ chưa đầy đủ (bổ sung thông tin)

**Là một** lễ tân,
**Tôi muốn** check-in bệnh nhân có hồ sơ thiếu thông tin và bổ sung các trường còn thiếu,
**Để** đảm bảo bệnh nhân có đủ dữ liệu trước khi vào khám (ví dụ: bệnh nhân đặt hẹn qua điện thoại chỉ có tên và SĐT).

**Yêu cầu liên quan:** RCP-04
**Quyết định liên quan:** D-05, D-06

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân bấm nút "Check-in" trên cột Thao tác của bệnh nhân có trạng thái "Chưa đến"
2. Hệ thống kiểm tra hồ sơ: thiếu trường bắt buộc (ví dụ: thiếu ngày sinh, giới tính)
3. Hệ thống mở popup với avatar initials (amber thay vì tím để cảnh báo)
4. Fields có dữ liệu hiển thị bình thường. Fields thiếu hiển thị "Chưa có" (italic, màu nhạt)
5. Popup hiển thị cảnh báo vàng: "Hồ sơ chưa đầy đủ -- BN đặt hẹn qua điện thoại, chỉ có tên + SĐT. Cần bổ sung ngày sinh, giới tính và các thông tin khác trước khi khám."
6. Lễ tân bấm "Check-in & bổ sung hồ sơ ->" -> Hệ thống đóng popup và mở form Intake (SCR-003) ở chế độ edit với dữ liệu hiện có pre-fill
7. Lễ tân điền đầy đủ thông tin còn thiếu và bấm "Lưu" -> Hệ thống lưu hồ sơ và tự động check-in, chuyển "Chờ khám"

**Trường hợp ngoại lệ:**
1. Lễ tân bấm "Hủy" -> Đóng popup, BN vẫn ở trạng thái "Chưa đến", hồ sơ không thay đổi
2. Bệnh nhân guest (đặt hẹn qua điện thoại, chưa có Patient record) -> Hệ thống tạo Patient record mới từ thông tin GuestName, GuestPhone trên Appointment

**Trường hợp lỗi:**
1. Lỗi lưu hồ sơ -> Hệ thống hiển thị lỗi validation trên form Intake, không check-in

#### Ghi chú kỹ thuật
- Logic phân loại: có đủ 5 trường bắt buộc = popup BN cũ, thiếu bất kỳ trường nào = popup BN mới (cảnh báo)
- Guest booking (PatientId = null): tạo Patient record từ GuestName + GuestPhone trước khi check-in

---

### US-RCP-012: Lễ tân tạo lượt khám walk-in cho bệnh nhân không có hẹn

**Là một** lễ tân,
**Tôi muốn** tạo lượt khám trực tiếp cho bệnh nhân cũ đã có hồ sơ nhưng không có lịch hẹn (walk-in tái khám),
**Để** đưa bệnh nhân vào hàng đợi "Chờ khám" nhanh chóng mà không cần đặt lịch hẹn trước.

**Yêu cầu liên quan:** RCP-04
**Quyết định liên quan:** D-05

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân search bệnh nhân bằng SĐT/tên trên dashboard -> Tìm thấy bệnh nhân cũ
2. Lễ tân click vào kết quả search -> Hệ thống mở popup "Tạo lượt khám"
3. Popup hiển thị thông tin bệnh nhân (read-only): họ tên, mã BN, năm sinh, giới tính, SĐT, lần khám gần nhất
4. Lễ tân nhập lý do khám (bắt buộc) và ghi chú (tùy chọn)
5. Lễ tân bấm "Xác nhận tạo lượt khám" -> Hệ thống tạo Visit mới với source = Walk-in, chuyển bệnh nhân sang "Chờ khám"
6. Bệnh nhân xuất hiện trên dashboard với badge "Walk-in" (coral) và trạng thái "Chờ khám"

**Trường hợp ngoại lệ:**
1. Bệnh nhân đã có lượt khám chưa hoàn thành trong ngày -> Hệ thống cảnh báo: "BN đã có lượt khám đang xử lý. Bạn có muốn tạo thêm lượt khám mới?"
2. Lễ tân bấm "Hủy" -> Đóng popup, không tạo lượt khám

**Trường hợp lỗi:**
1. Lỗi tạo Visit -> Hệ thống hiển thị toast lỗi, popup không đóng

#### Ghi chú kỹ thuật
- Popup: Dialog modal với field lý do khám + ghi chú
- API: POST /api/clinical/visits (với source = WalkIn)

---

## Dashboard Actions (SCR-006)

### US-RCP-013: Lễ tân đổi lịch hẹn cho bệnh nhân

**Là một** lễ tân,
**Tôi muốn** đổi ngày giờ hẹn cho bệnh nhân khi họ gọi điện báo thay đổi,
**Để** cập nhật lịch hẹn mà không cần hủy và đặt lại từ đầu.

**Yêu cầu liên quan:** RCP-05
**Quyết định liên quan:** D-13

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân bấm icon ⋯ trên dòng bệnh nhân có trạng thái "Chưa đến" -> Menu dropdown hiển thị
2. Lễ tân chọn "Đổi lịch hẹn" (icon lịch, màu tím) -> Hệ thống mở popup đổi lịch
3. Popup hiển thị: thông tin BN (tên, SĐT), card lịch hẹn hiện tại (ngày giờ cũ, lý do), calendar mini, grid slot giờ
4. Lễ tân chọn ngày mới và slot giờ mới trên calendar/grid
5. Thanh xác nhận hiển thị so sánh: lịch cũ (gạch ngang) -> lịch mới (bold)
6. Lễ tân bấm "Xác nhận đổi lịch" -> Hệ thống cập nhật appointment với ngày giờ mới
7. Dashboard cập nhật: nếu ngày mới là hôm nay -> BN vẫn hiển thị; nếu ngày mới là ngày khác -> BN biến mất khỏi danh sách hôm nay

**Trường hợp ngoại lệ:**
1. Lễ tân chọn cùng ngày cùng giờ cũ -> Hệ thống hiển thị lỗi: "Vui lòng chọn thời gian khác với lịch hẹn hiện tại"
2. Slot đã đầy -> Hệ thống hiển thị slot đó là "Đã đặt" (xám), không cho chọn

**Trường hợp lỗi:**
1. Slot vừa được đặt bởi người khác -> Hệ thống hiển thị "Slot này vừa được đặt. Vui lòng chọn slot khác."
2. Lỗi cập nhật appointment -> Hệ thống hiển thị toast lỗi, lịch hẹn cũ vẫn giữ nguyên

#### Ghi chú kỹ thuật
- Chỉ áp dụng cho trạng thái "Chưa đến" (BN chưa đến)
- API: PUT /api/scheduling/appointments/{id}/reschedule

---

### US-RCP-014: Lễ tân đánh dấu bệnh nhân không đến (no-show) với ghi chú

**Là một** lễ tân,
**Tôi muốn** đánh dấu bệnh nhân không đến khám (no-show) với ghi chú tùy chọn và lựa chọn đặt hẹn lại,
**Để** cập nhật trạng thái và giữ liệu trình sạch cho các bệnh nhân còn lại.

**Yêu cầu liên quan:** RCP-06
**Quyết định liên quan:** D-13, D-14, D-15

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân bấm icon ⋯ trên dòng bệnh nhân có trạng thái "Chưa đến" -> Chọn "Đánh dấu không đến" (icon warning, màu amber)
2. Hệ thống mở popup no-show với: thông tin BN (tên, SĐT), field ghi chú (tùy chọn, text input), checkbox "Đặt hẹn lại"
3. Lễ tân nhập ghi chú (VD: "BN gọi báo bận, không đến được") và bấm "Xác nhận"
4. Hệ thống cập nhật appointment: trạng thái = NoShow, ghi nhận `no_show_at`, `no_show_by`, `no_show_notes`
5. Bệnh nhân biến mất khỏi danh sách dashboard (hoặc hiển thị với trạng thái đặc biệt)
6. KPI "Lịch hẹn hôm nay" sub-text "X chưa đến" giảm 1

**Trường hợp ngoại lệ:**
1. Lễ tân tick checkbox "Đặt hẹn lại" -> Sau khi xác nhận no-show, hệ thống tự động chuyển đến trang `/appointments/new` với thông tin bệnh nhân đã điền sẵn
2. Lễ tân không nhập ghi chú -> Hệ thống vẫn cho phép xác nhận (ghi chú là tùy chọn)

**Trường hợp lỗi:**
1. Lỗi cập nhật trạng thái -> Hệ thống hiển thị toast lỗi, BN vẫn ở trạng thái "Chưa đến"

#### Ghi chú kỹ thuật
- Chỉ áp dụng cho trạng thái "Chưa đến"
- Checkbox "Đặt hẹn lại" navigate đến `/appointments/new` với patient pre-fill (D-15)
- API: POST /api/scheduling/appointments/{id}/no-show

---

### US-RCP-015: Lễ tân hủy lịch hẹn với lý do bắt buộc

**Là một** lễ tân,
**Tôi muốn** hủy lịch hẹn của bệnh nhân với lý do bắt buộc,
**Để** ghi nhận rõ ràng lý do hủy và giải phóng slot giờ cho bệnh nhân khác.

**Yêu cầu liên quan:** RCP-06
**Quyết định liên quan:** D-13, D-14

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân bấm icon ⋯ trên dòng bệnh nhân có trạng thái "Chưa đến" -> Chọn "Hủy hẹn" (icon X circle, màu đỏ)
2. Hệ thống mở popup hủy hẹn với: thông tin BN (tên, SĐT, lịch hẹn hiện tại), dropdown lý do hủy (bắt buộc)
3. Các lý do hủy: "BN yêu cầu hủy", "Phòng khám hủy", "Trùng lịch", "Lý do khác"
4. Nếu chọn "Lý do khác" -> Hiển thị textarea để lễ tân nhập lý do cụ thể
5. Lễ tân chọn lý do và bấm "Xác nhận hủy hẹn" -> Hệ thống cập nhật appointment: trạng thái = Cancelled, ghi nhận `cancelled_by`, `cancelled_reason`
6. Bệnh nhân biến mất khỏi danh sách dashboard, slot giờ được giải phóng

**Trường hợp ngoại lệ:**
1. Lễ tân chưa chọn lý do -> Nút "Xác nhận hủy hẹn" bị disable, không cho bấm
2. Lễ tân bấm "Hủy" trên popup -> Đóng popup, lịch hẹn vẫn giữ nguyên

**Trường hợp lỗi:**
1. Lỗi hủy appointment -> Hệ thống hiển thị toast lỗi, lịch hẹn vẫn giữ nguyên

#### Ghi chú kỹ thuật
- Chỉ áp dụng cho trạng thái "Chưa đến"
- Lý do hủy bắt buộc (dropdown) - D-14
- API: POST /api/scheduling/appointments/{id}/cancel

---

### US-RCP-016: Lễ tân hủy lượt khám với lý do và tùy chọn đặt hẹn lại

**Là một** lễ tân,
**Tôi muốn** hủy lượt khám của bệnh nhân đang chờ khám với lý do bắt buộc và tùy chọn đặt hẹn lại,
**Để** xử lý trường hợp bệnh nhân đổi ý không muốn khám nữa sau khi đã check-in.

**Yêu cầu liên quan:** RCP-06
**Quyết định liên quan:** D-13, D-14, D-15

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Lễ tân bấm icon ⋯ trên dòng bệnh nhân có trạng thái "Chờ khám" -> Chọn "Hủy lượt khám" (icon X circle, màu đỏ)
2. Hệ thống mở popup hủy lượt khám với: thông tin BN (tên, SĐT), dropdown lý do hủy (bắt buộc), checkbox "Đặt hẹn lại"
3. Các lý do hủy: "BN yêu cầu hủy", "Chờ quá lâu", "Lý do cá nhân", "Lý do khác"
4. Nếu chọn "Lý do khác" -> Hiển thị textarea để nhập lý do cụ thể
5. Lễ tân chọn lý do và bấm "Xác nhận hủy lượt khám" -> Hệ thống cập nhật Visit: status = Cancelled, ghi nhận `cancelled_reason`, `cancelled_by`
6. Bệnh nhân chuyển sang trạng thái hủy trên dashboard (hoặc biến mất), KPI "Chờ khám" giảm 1

**Trường hợp ngoại lệ:**
1. Lễ tân tick checkbox "Đặt hẹn lại" -> Sau khi xác nhận hủy, hệ thống tự động chuyển đến trang `/appointments/new` với thông tin bệnh nhân đã điền sẵn
2. Lễ tân chưa chọn lý do -> Nút "Xác nhận hủy lượt khám" bị disable

**Trường hợp lỗi:**
1. Lỗi hủy Visit -> Hệ thống hiển thị toast lỗi, lượt khám vẫn giữ nguyên

#### Ghi chú kỹ thuật
- Chỉ áp dụng cho trạng thái "Chờ khám" (BN đã check-in nhưng chưa vào khám)
- Lý do hủy bắt buộc (dropdown) - D-14
- Checkbox "Đặt hẹn lại" navigate đến `/appointments/new` với patient pre-fill (D-15)
- API: POST /api/clinical/visits/{id}/cancel
