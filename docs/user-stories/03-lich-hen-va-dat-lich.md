# Lịch hẹn và Đặt lịch - User Stories

**Phạm vi:** Quản lý lịch hẹn khám bệnh, đặt lịch hẹn bởi nhân viên và bệnh nhân tự đặt qua trang công khai, ngăn chặn đặt trùng, lịch hiển thị theo bác sĩ, cấu hình thời lượng khám và giờ hoạt động phòng khám.
**Yêu cầu liên quan:** SCH-01, SCH-02, SCH-03, SCH-04, SCH-05, SCH-06
**Số lượng user stories:** 9

---

## US-SCH-001: Nhân viên đặt lịch hẹn cho bệnh nhân

**Là một** nhân viên phòng khám, **Tôi muốn** đặt lịch hẹn cho bệnh nhân với bác sĩ được chỉ định, **Để** bệnh nhân có lịch khám cụ thể và bác sĩ biết trước lịch trình làm việc.

**Yêu cầu liên quan:** SCH-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở trang Lịch hẹn và nhấn "Đặt lịch hẹn" → Hệ thống hiển thị form đặt lịch với các trường: Bệnh nhân, Bác sĩ, Ngày, Giờ bắt đầu, Loại lịch hẹn, Ghi chú
2. Nhân viên chọn bệnh nhân từ danh sách → Hệ thống hiển thị tên bệnh nhân đã chọn
3. Nhân viên chọn bác sĩ từ DoctorSelector → Hệ thống tự động chọn bác sĩ đầu tiên nếu chỉ có một bác sĩ; onChange trả về cả doctorId và doctorName
4. Nhân viên chọn ngày bằng DatePicker và giờ bằng Select riêng biệt → Hệ thống ghép startDate + startTime trong handleSubmit
5. Nhân viên chọn loại lịch hẹn (Bệnh nhân mới / Tái khám / Điều trị / Ortho-K) → Hệ thống gán thời lượng mặc định tương ứng
6. Nhân viên nhấn lưu → Hệ thống tạo lịch hẹn với trạng thái "Đã xác nhận", lưu PatientName và DoctorName denormalized trên entity Appointment

**Trường hợp ngoại lệ:**
1. Nếu chỉ có một bác sĩ trong hệ thống → DoctorSelector tự động chọn bác sĩ đó
2. Nếu nhân viên không nhập ghi chú → Hệ thống cho phép tạo lịch hẹn mà không có ghi chú

**Trường hợp lỗi:**
1. Khi đặt lịch vào thời gian đã qua → Hệ thống từ chối và hiển thị thông báo lỗi
2. Khi đặt lịch ngoài giờ hoạt động phòng khám → Hệ thống hiển thị thông báo "Ngoài giờ hoạt động phòng khám"
3. Khi khung giờ đã có bệnh nhân khác → Hệ thống hiển thị thông báo "Khung giờ này đã được đặt"

### Ghi chú kỹ thuật
- DoctorSelector sử dụng onChange với 2 tham số (doctorId, doctorName) cho mọi consumer
- DatePicker và time Select là hai trường riêng biệt (startDate kiểu Date, startTime kiểu string), được ghép lại trong handleSubmit
- Appointment entity lưu denormalized PatientName và DoctorName để tránh cross-module join
- Loại lịch hẹn: newPatient, followUp, treatment, orthoK
- API endpoint: POST /api/scheduling/appointments

---

## US-SCH-002: Bệnh nhân tự đặt lịch hẹn qua trang công khai

**Là một** bệnh nhân, **Tôi muốn** tự đặt lịch hẹn khám qua trang web công khai mà không cần tài khoản đăng nhập, **Để** tôi có thể đặt lịch thuận tiện bất kỳ lúc nào.

**Yêu cầu liên quan:** SCH-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bệnh nhân truy cập trang /book → Hệ thống hiển thị form đặt lịch công khai với thông tin phòng khám (Phòng khám Nhãn khoa Ganka28)
2. Bệnh nhân nhập: Họ và tên, Số điện thoại, Email, Ngày mong muốn, Giờ mong muốn (Buổi sáng/Buổi chiều/Buổi tối), Bác sĩ mong muốn (không bắt buộc), Lý do khám → Hệ thống validate các trường bắt buộc
3. Bệnh nhân nhấn "Gửi yêu cầu đặt lịch" → Hệ thống tạo yêu cầu với trạng thái "Chờ xác nhận"
4. Hệ thống hiển thị trang xác nhận với mã tham chiếu → Bệnh nhân ghi nhớ mã tham chiếu để kiểm tra trạng thái
5. Hệ thống hiển thị thông báo: "Nhân viên sẽ xác nhận trong vòng 24 giờ"

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân không chọn bác sĩ → Hệ thống ghi nhận yêu cầu với lựa chọn "Không có yêu cầu"
2. Nếu bệnh nhân muốn đặt lịch khác → Hệ thống hiển thị nút "Đặt lịch khác" sau khi gửi thành công

**Trường hợp lỗi:**
1. Khi gửi quá 5 yêu cầu/phút từ cùng một IP → Hệ thống từ chối với thông báo giới hạn tần suất (rate limiting)
2. Khi đã đạt số lượng yêu cầu đặt lịch tối đa → Hệ thống hiển thị "Bạn đã đạt số lượng yêu cầu đặt lịch tối đa"
3. Khi không nhập các trường bắt buộc → Hệ thống hiển thị lỗi validation tương ứng

### Ghi chú kỹ thuật
- Trang /book nằm ngoài layout _authenticated (không yêu cầu đăng nhập)
- Public API sử dụng endpoint /api/public/booking không có RequireAuthorization
- Rate limiting: RequireRateLimiting với 5 request/phút/IP
- Sử dụng publicApi client riêng biệt (không có auth middleware) cho các endpoint /api/public/booking
- Quy trình: Submit (public) → Pending → Approve/Reject (staff)

---

## US-SCH-003: Bệnh nhân kiểm tra trạng thái đặt lịch

**Là một** bệnh nhân, **Tôi muốn** kiểm tra trạng thái yêu cầu đặt lịch bằng mã tham chiếu, **Để** tôi biết lịch hẹn đã được duyệt hay chưa.

**Yêu cầu liên quan:** SCH-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bệnh nhân truy cập trang /book/status → Hệ thống hiển thị form nhập mã tham chiếu
2. Bệnh nhân nhập mã tham chiếu và nhấn "Kiểm tra" → Hệ thống tìm kiếm yêu cầu đặt lịch
3. Nếu trạng thái "Chờ xác nhận" → Hiển thị: "Yêu cầu đặt lịch của bạn đang được nhân viên xem xét"
4. Nếu trạng thái "Đã duyệt" → Hiển thị: "Lịch hẹn của bạn đã được xác nhận vào ngày [ngày]"
5. Nếu trạng thái "Đã từ chối" → Hiển thị: "Yêu cầu đặt lịch không được duyệt. Lý do: [lý do]"

**Trường hợp lỗi:**
1. Khi nhập mã tham chiếu không tồn tại → Hệ thống hiển thị thông báo không tìm thấy
2. Khi không nhập mã tham chiếu → Hệ thống yêu cầu nhập mã

### Ghi chú kỹ thuật
- Trang /book/status nằm ngoài layout _authenticated
- Sử dụng publicApi client cho truy vấn trạng thái
- Hiển thị số điện thoại phòng khám với thông báo: "Vui lòng gọi [số] để được hỗ trợ"

---

## US-SCH-004: Hệ thống ngăn chặn đặt lịch trùng

**Là một** nhân viên phòng khám, **Tôi muốn** hệ thống tự động phát hiện và ngăn chặn đặt lịch trùng khung giờ cho cùng một bác sĩ, **Để** đảm bảo mỗi bác sĩ chỉ khám một bệnh nhân tại một thời điểm.

**Yêu cầu liên quan:** SCH-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên đặt lịch hẹn cho bác sĩ A lúc 14:00 → Hệ thống tạo thành công
2. Nhân viên đặt lịch hẹn cho bác sĩ A lúc 15:00 (không trùng) → Hệ thống tạo thành công
3. Nhân viên đặt lịch hẹn cho bác sĩ B cùng lúc 14:00 (khác bác sĩ) → Hệ thống tạo thành công

**Trường hợp ngoại lệ:**
1. Nếu hai nhân viên cùng lúc đặt lịch cho cùng bác sĩ, cùng giờ (concurrent) → Hệ thống chỉ cho phép một yêu cầu thành công, yêu cầu còn lại nhận lỗi
2. Nếu lịch hẹn mới chồng lấn một phần với lịch hẹn hiện có → Hệ thống phát hiện và từ chối

**Trường hợp lỗi:**
1. Khi đặt lịch trùng khung giờ cho cùng bác sĩ → Hệ thống hiển thị thông báo "Khung giờ này đã được đặt"
2. Khi lỗi xảy ra do concurrent booking → Hệ thống trả về lỗi validation rõ ràng

### Ghi chú kỹ thuật
- Kiểm tra trùng lịch 2 lớp: application-level HasOverlappingAsync + DB unique filtered index
- Trên giao diện lịch, khung giờ đã đặt được hiển thị trực quan (visual indication)
- Xử lý concurrent booking attempts thông qua database constraint

---

## US-SCH-005: Nhân viên xem lịch hẹn theo bác sĩ trên giao diện lịch

**Là một** nhân viên phòng khám, **Tôi muốn** xem lịch hẹn của từng bác sĩ trên giao diện lịch trực quan với mã màu theo loại hẹn, **Để** nhanh chóng nắm bắt lịch trình khám bệnh trong ngày/tuần/tháng.

**Yêu cầu liên quan:** SCH-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở trang Lịch hẹn → Hệ thống hiển thị lịch FullCalendar với các lịch hẹn của bác sĩ đang chọn
2. Nhân viên chọn bác sĩ từ DoctorSelector → Lịch cập nhật hiển thị chỉ lịch hẹn của bác sĩ đó
3. Nhân viên chuyển chế độ xem: Ngày / Tuần / Tháng → Lịch cập nhật tương ứng
4. Mỗi lịch hẹn hiển thị mã màu theo loại: Bệnh nhân mới, Tái khám, Điều trị, Ortho-K → Nhân viên dễ phân biệt loại hẹn
5. Nhân viên nhấp vào lịch hẹn trên lịch → Hệ thống hiển thị chi tiết lịch hẹn

**Trường hợp ngoại lệ:**
1. Nếu không có lịch hẹn trong khoảng thời gian đang xem → Hệ thống hiển thị "Không có lịch hẹn"
2. Nếu nhân viên kéo lịch hẹn sang thời gian khác → Hệ thống hiển thị xác nhận đổi lịch

**Trường hợp lỗi:**
1. Khi kéo lịch hẹn vào khung giờ đã có bệnh nhân khác → Hệ thống từ chối và hiển thị lỗi trùng lịch

### Ghi chú kỹ thuật
- Sử dụng thư viện FullCalendar với 3 chế độ xem: dayView, weekView, monthView
- CSS của FullCalendar được theme bằng CSS variables khớp với shadcn/ui design tokens
- Sử dụng color-mix(in oklch, ...) cho các biến thể opacity
- DoctorSelector tự động chọn bác sĩ đầu tiên nếu chỉ có một

---

## US-SCH-006: Cấu hình thời lượng khám theo loại lịch hẹn

**Là một** quản trị viên, **Tôi muốn** cấu hình thời lượng khám mặc định cho từng loại lịch hẹn, **Để** lịch hẹn được tạo với khoảng thời gian phù hợp cho mỗi loại khám.

**Yêu cầu liên quan:** SCH-05

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Khi tạo lịch hẹn loại "Bệnh nhân mới" → Hệ thống gán thời lượng mặc định 30 phút
2. Khi tạo lịch hẹn loại "Tái khám" → Hệ thống gán thời lượng mặc định 20 phút
3. Khi tạo lịch hẹn loại "Điều trị" → Hệ thống gán thời lượng mặc định 30-45 phút
4. Khi tạo lịch hẹn loại "Ortho-K" → Hệ thống gán thời lượng mặc định 60-90 phút
5. Trên lịch, mỗi lịch hẹn chiếm đúng khoảng thời gian tương ứng với thời lượng đã cấu hình

**Trường hợp ngoại lệ:**
1. Nếu quản trị viên thay đổi thời lượng mặc định → Hệ thống áp dụng giá trị mới cho các lịch hẹn tạo sau đó (không ảnh hưởng lịch hẹn cũ)

**Trường hợp lỗi:**
1. Khi nhập thời lượng không hợp lệ (âm hoặc quá lớn) → Hệ thống từ chối và hiển thị lỗi validation

### Ghi chú kỹ thuật
- Thời lượng mặc định: newPatient=30min, followUp=20min, treatment=30-45min, orthoK=60-90min
- Quản trị viên có thể override giá trị mặc định
- Thời lượng hiển thị trên lịch FullCalendar tương ứng kích thước event

---

## US-SCH-007: Hệ thống tuân thủ giờ hoạt động phòng khám

**Là một** nhân viên phòng khám, **Tôi muốn** hệ thống tự động kiểm tra và cảnh báo khi đặt lịch ngoài giờ hoạt động, **Để** đảm bảo lịch hẹn luôn nằm trong thời gian phòng khám mở cửa.

**Yêu cầu liên quan:** SCH-06

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên đặt lịch vào Thứ 3 lúc 14:00 → Hệ thống chấp nhận (trong giờ hoạt động Thứ 3 - Thứ 6: 13:00 - 20:00)
2. Nhân viên đặt lịch vào Thứ 7 lúc 9:00 → Hệ thống chấp nhận (trong giờ hoạt động Thứ 7 - Chủ nhật: 08:00 - 12:00)
3. Trên giao diện lịch, giờ hoạt động được hiển thị rõ ràng → Nhân viên biết khung giờ nào khả dụng

**Trường hợp ngoại lệ:**
1. Nếu quản trị viên thay đổi giờ hoạt động → Hệ thống áp dụng lịch mới (không ảnh hưởng lịch hẹn cũ)

**Trường hợp lỗi:**
1. Khi đặt lịch vào Thứ 2 (ngày nghỉ) → Hệ thống từ chối: "Ngoài giờ hoạt động phòng khám" / "Đóng cửa"
2. Khi đặt lịch vào Thứ 4 lúc 21:00 (ngoài giờ) → Hệ thống từ chối với thông báo ngoài giờ hoạt động
3. Khi đặt lịch vào Chủ nhật lúc 13:00 (ngoài giờ cuối tuần) → Hệ thống từ chối

### Ghi chú kỹ thuật
- Giờ hoạt động mặc định: Thứ 3 - Thứ 6: 13:00 - 20:00, Thứ 7 - Chủ nhật: 08:00 - 12:00, Thứ 2: Nghỉ
- Giờ hoạt động có thể cấu hình bởi quản trị viên
- Validation kiểm tra giờ hoạt động khi tạo lịch hẹn
- Xử lý timezone: SE Asia Standard Time (Windows) / Asia/Ho_Chi_Minh (Linux) qua OperatingSystem.IsWindows()

---

## US-SCH-008: Nhân viên duyệt hoặc từ chối yêu cầu đặt lịch tự đặt

**Là một** nhân viên phòng khám, **Tôi muốn** duyệt hoặc từ chối các yêu cầu đặt lịch của bệnh nhân tự đặt, **Để** kiểm soát lịch khám và đảm bảo sắp xếp phù hợp.

**Yêu cầu liên quan:** SCH-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên xem danh sách yêu cầu đặt lịch chờ duyệt → Hệ thống hiển thị các yêu cầu có trạng thái "Chờ xác nhận" trong PendingBookingsPanel
2. Nhân viên chọn yêu cầu và nhấn "Duyệt" → Hệ thống hiển thị ApproveBookingDialog inline trong panel
3. Nhân viên xác nhận thông tin (bác sĩ, ngày giờ chính thức) → Hệ thống chuyển trạng thái thành "Đã duyệt" và tạo lịch hẹn chính thức
4. Bệnh nhân kiểm tra trạng thái qua mã tham chiếu → Hiển thị "Đã duyệt" với ngày giờ xác nhận

**Trường hợp ngoại lệ:**
1. Nếu nhân viên từ chối yêu cầu → Hệ thống yêu cầu nhập lý do từ chối, chuyển trạng thái thành "Đã từ chối"
2. Nếu không có yêu cầu chờ duyệt → Panel hiển thị trạng thái trống

**Trường hợp lỗi:**
1. Khi duyệt yêu cầu vào khung giờ đã có lịch hẹn khác → Hệ thống cảnh báo trùng lịch

### Ghi chú kỹ thuật
- ApproveBookingDialog là inline trong PendingBookingsPanel (không phải standalone component)
- Quy trình: Pending → Approve (tạo Appointment chính thức) hoặc Reject (ghi lý do)
- Panel hiển thị trong trang lịch hẹn

---

## US-SCH-009: Nhân viên xem bảng yêu cầu đặt lịch chờ duyệt

**Là một** nhân viên phòng khám, **Tôi muốn** xem tổng quan các yêu cầu đặt lịch đang chờ duyệt, **Để** xử lý kịp thời các yêu cầu của bệnh nhân.

**Yêu cầu liên quan:** SCH-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở trang Lịch hẹn → Hệ thống hiển thị PendingBookingsPanel bên cạnh lịch
2. Panel hiển thị danh sách yêu cầu chờ duyệt với: Tên bệnh nhân, Số điện thoại, Ngày mong muốn, Giờ mong muốn, Lý do khám
3. Mỗi yêu cầu có nút "Duyệt" và "Từ chối" → Nhân viên xử lý trực tiếp từ danh sách

**Trường hợp ngoại lệ:**
1. Nếu không có yêu cầu nào đang chờ → Hệ thống hiển thị trạng thái trống trong panel

**Trường hợp lỗi:**
1. Khi mất kết nối → Hệ thống hiển thị thông báo lỗi tải dữ liệu

### Ghi chú kỹ thuật
- PendingBookingsPanel hiển thị cùng trang với lịch FullCalendar
- Danh sách được sắp xếp theo thời gian yêu cầu (cũ nhất trước)
- API endpoint: GET /api/scheduling/bookings?status=pending
