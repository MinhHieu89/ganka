# User Stories: Phác Đồ Điều Trị (Treatment Protocols)

**Phase:** 09 - Treatment Protocols
**Ngày tạo:** 2026-03-08
**Yêu cầu liên quan:** TRT-01, TRT-02, TRT-03, TRT-04, TRT-05, TRT-06, TRT-07, TRT-08, TRT-09, TRT-10, TRT-11
**Số lượng user stories:** 11

---

## TRT-01: Tạo gói liệu trình điều trị

### US-TRT-001: Bác sĩ tạo gói liệu trình IPL/LLLT/chăm sóc mi mắt

**Là một** bác sĩ,
**Tôi muốn** tạo gói liệu trình điều trị (IPL, LLLT, hoặc chăm sóc mi mắt) cho bệnh nhân với số buổi từ 1 đến 6 và giá linh hoạt (theo buổi hoặc trọn gói),
**Để** lên phác đồ điều trị phù hợp với tình trạng bệnh lý khô mắt của từng bệnh nhân.

**Yêu cầu liên quan:** TRT-01

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở hồ sơ bệnh nhân → Chọn tab "Liệu trình điều trị" → Nhấn "Tạo liệu trình mới"
2. Bác sĩ chọn loại điều trị từ danh sách: IPL (Intense Pulsed Light), LLLT (Low-Level Light Therapy), Chăm sóc mi mắt (Lid Care)
3. Bác sĩ nhập thông tin liệu trình: số buổi điều trị (1-6 buổi), hình thức tính giá (theo buổi hoặc trọn gói), giá tiền (VNĐ), ghi chú phác đồ
4. Bác sĩ nhấn "Tạo liệu trình" → Hệ thống lưu liệu trình với trạng thái "Đang hoạt động" (Active)
5. Liệu trình mới xuất hiện trong danh sách liệu trình của bệnh nhân với thông tin: loại điều trị, số buổi, giá, trạng thái

**Trường hợp ngoại lệ:**
1. Bệnh nhân đã có liệu trình cùng loại đang hoạt động → Hệ thống cảnh báo: "Bệnh nhân đã có liệu trình [loại] đang hoạt động. Bạn có muốn tạo thêm không?" nhưng vẫn cho phép tạo (theo TRT-06)
2. Số buổi nằm ngoài khoảng 1-6 → Hệ thống hiển thị lỗi: "Số buổi điều trị phải từ 1 đến 6"
3. Bác sĩ chọn hình thức trọn gói nhưng chỉ có 1 buổi → Hệ thống cảnh báo: "Gói trọn gói thường áp dụng cho 2 buổi trở lên. Bạn có muốn tiếp tục?"

**Trường hợp lỗi:**
1. Thiếu thông tin bắt buộc (loại điều trị, số buổi, giá) → Hệ thống hiển thị lỗi validation từng trường
2. Giá tiền âm hoặc bằng 0 → Hệ thống hiển thị: "Giá tiền phải lớn hơn 0"
3. Lỗi lưu liệu trình → Hệ thống hiển thị toast lỗi: "Tạo liệu trình thất bại. Vui lòng thử lại"

#### Ghi chú kỹ thuật
- Entity: TreatmentPackage (PackageId, PatientId, DoctorId, TreatmentType enum: IPL/LLLT/LidCare, TotalSessions, PricingType enum: PerSession/Package, SessionPrice, PackagePrice, Status, CreatedAt)
- API endpoint: POST /api/treatment/packages
- Chỉ user có role Doctor mới được tạo (TRT-10)

---

## TRT-02: Theo dõi buổi điều trị

### US-TRT-002: Hệ thống theo dõi buổi điều trị đã hoàn thành và còn lại

**Là một** bác sĩ / nhân viên,
**Tôi muốn** xem số buổi điều trị đã hoàn thành và số buổi còn lại trong mỗi liệu trình,
**Để** theo dõi tiến độ điều trị và lên lịch cho các buổi tiếp theo.

**Yêu cầu liên quan:** TRT-02

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ hoặc nhân viên mở hồ sơ bệnh nhân → Tab "Liệu trình điều trị" → Hệ thống hiển thị danh sách tất cả liệu trình
2. Mỗi liệu trình hiển thị thanh tiến trình (progress bar): "Buổi X/Y hoàn thành" (ví dụ: "Buổi 3/6 hoàn thành")
3. Danh sách các buổi điều trị hiển thị chi tiết: ngày thực hiện, bác sĩ thực hiện, trạng thái (Đã hoàn thành / Đã lên lịch / Chưa lên lịch)
4. Số buổi còn lại được tính tự động: Còn lại = Tổng buổi - Buổi đã hoàn thành

**Trường hợp ngoại lệ:**
1. Liệu trình chưa có buổi nào → Hiển thị: "Buổi 0/Y hoàn thành" với thanh tiến trình trống
2. Liệu trình đã hoàn tất tất cả buổi → Hiển thị: "Hoàn thành" với thanh tiến trình đầy và trạng thái "Đã hoàn thành"

**Trường hợp lỗi:**
1. Không thể tải thông tin buổi điều trị → Hệ thống hiển thị toast lỗi: "Không thể tải thông tin liệu trình. Vui lòng thử lại"
2. Dữ liệu buổi điều trị không nhất quán (số buổi hoàn thành > tổng buổi) → Hệ thống ghi log cảnh báo và hiển thị dữ liệu thực tế

#### Ghi chú kỹ thuật
- Query: SELECT COUNT(*) FROM TreatmentSessions WHERE PackageId = @id AND Status = 'Completed'
- CompletedSessions / TotalSessions hiển thị trên TreatmentPackageDetail page
- API endpoint: GET /api/treatment/packages/{id}

---

## TRT-03: Ghi nhận điểm OSDI

### US-TRT-003: Hệ thống ghi nhận điểm OSDI mỗi buổi điều trị

**Là một** bác sĩ,
**Tôi muốn** ghi nhận điểm OSDI (Ocular Surface Disease Index) của bệnh nhân tại mỗi buổi điều trị,
**Để** đánh giá mức độ khô mắt và theo dõi hiệu quả điều trị qua từng buổi.

**Yêu cầu liên quan:** TRT-03

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở form ghi nhận buổi điều trị → Mục "Điểm OSDI" hiển thị với ô nhập số
2. Bác sĩ nhập điểm OSDI (thang 0-100) → Hệ thống hiển thị phân loại tự động:
   - 0-12: Bình thường (Xanh lá)
   - 13-22: Nhẹ (Vàng)
   - 23-32: Trung bình (Cam)
   - 33-100: Nặng (Đỏ)
3. Sau khi lưu buổi điều trị → Điểm OSDI được lưu cùng buổi điều trị
4. Bác sĩ có thể xem biểu đồ xu hướng OSDI (OSDI Trend Chart) hiển thị điểm OSDI qua các buổi điều trị theo dạng đường (line chart)

**Trường hợp ngoại lệ:**
1. Bác sĩ không nhập điểm OSDI (bỏ qua) → Hệ thống cho phép lưu buổi điều trị mà không có OSDI (trường không bắt buộc)
2. Buổi điều trị đầu tiên → Biểu đồ xu hướng chỉ hiển thị 1 điểm, không có đường so sánh

**Trường hợp lỗi:**
1. Điểm OSDI nằm ngoài khoảng 0-100 → Hệ thống hiển thị lỗi: "Điểm OSDI phải nằm trong khoảng 0 đến 100"
2. Điểm OSDI không phải số → Hệ thống hiển thị lỗi: "Vui lòng nhập giá trị số hợp lệ"

#### Ghi chú kỹ thuật
- TreatmentSession entity chứa trường OsdiScore (nullable decimal)
- Phân loại OSDI: domain logic trong TreatmentSession hoặc value object
- OSDI Trend Chart: frontend component OsdiTrendChart, dữ liệu từ GET /api/treatment/packages/{id}/sessions

---

## TRT-04: Tự động đánh dấu hoàn thành

### US-TRT-004: Hệ thống tự động đánh dấu hoàn thành khi tất cả buổi điều trị xong

**Là một** bác sĩ,
**Tôi muốn** hệ thống tự động chuyển trạng thái liệu trình sang "Đã hoàn thành" khi tất cả các buổi điều trị đã được thực hiện,
**Để** không cần đánh dấu thủ công và đảm bảo trạng thái liệu trình luôn chính xác.

**Yêu cầu liên quan:** TRT-04

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ ghi nhận buổi điều trị cuối cùng (buổi Y/Y) → Nhấn "Lưu"
2. Hệ thống phát hiện: số buổi đã hoàn thành = tổng số buổi trong liệu trình
3. Hệ thống tự động chuyển trạng thái liệu trình từ "Đang hoạt động" (Active) sang "Đã hoàn thành" (Completed)
4. Giao diện hiển thị badge "Đã hoàn thành" trên liệu trình và thông báo: "Liệu trình đã hoàn tất tất cả buổi điều trị"

**Trường hợp ngoại lệ:**
1. Bác sĩ thêm buổi mới vào liệu trình đã hoàn thành (qua TRT-07) → Trạng thái tự động chuyển lại "Đang hoạt động"
2. Liệu trình bị hủy (TRT-09) trước khi hoàn thành → Trạng thái chuyển sang "Đã hủy", không phải "Đã hoàn thành"

**Trường hợp lỗi:**
1. Lỗi cập nhật trạng thái tự động → Hệ thống ghi log lỗi và giữ trạng thái "Đang hoạt động" → Bác sĩ có thể đánh dấu thủ công nếu cần
2. Dữ liệu buổi điều trị không nhất quán → Hệ thống không tự động chuyển trạng thái, yêu cầu kiểm tra

#### Ghi chú kỹ thuật
- Domain event: AllSessionsCompleted → TreatmentPackage.MarkAsCompleted()
- Logic kiểm tra trong TreatmentSession handler: sau khi lưu session, đếm completed sessions vs TotalSessions
- Status enum: Active, Completed, Cancelled, Suspended

---

## TRT-05: Khoảng cách tối thiểu giữa các buổi

### US-TRT-005: Hệ thống kiểm tra khoảng cách tối thiểu giữa các buổi điều trị

**Là một** bác sĩ,
**Tôi muốn** hệ thống kiểm tra và cảnh báo nếu lên lịch buổi điều trị tiếp theo quá sớm so với khoảng cách tối thiểu quy định,
**Để** đảm bảo an toàn cho bệnh nhân và tuân thủ phác đồ điều trị chuẩn.

**Yêu cầu liên quan:** TRT-05

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ tạo buổi điều trị mới cho liệu trình → Chọn ngày thực hiện
2. Hệ thống kiểm tra khoảng cách từ buổi điều trị gần nhất:
   - IPL: tối thiểu 2-4 tuần
   - LLLT: tối thiểu 1-2 tuần
   - Chăm sóc mi mắt: tối thiểu 1-2 tuần
3. Nếu khoảng cách hợp lệ → Buổi điều trị được tạo thành công
4. Khoảng cách tối thiểu có thể cấu hình trong phần cài đặt hệ thống

**Trường hợp ngoại lệ:**
1. Ngày chọn quá sớm (vi phạm khoảng cách tối thiểu) → Hệ thống hiển thị cảnh báo: "Khoảng cách giữa buổi điều trị chưa đủ [X ngày]. Ngày sớm nhất có thể: [ngày]" nhưng vẫn cho phép bác sĩ ghi đè (override) nếu có lý do lâm sàng
2. Buổi điều trị đầu tiên → Không cần kiểm tra khoảng cách (không có buổi trước)
3. Bác sĩ thay đổi cấu hình khoảng cách tối thiểu → Hệ thống áp dụng cho các buổi mới, không ảnh hưởng buổi đã lên lịch

**Trường hợp lỗi:**
1. Không thể tải thông tin buổi điều trị trước → Hệ thống hiển thị cảnh báo: "Không thể xác minh khoảng cách buổi điều trị. Vui lòng kiểm tra thủ công"
2. Cấu hình khoảng cách không hợp lệ (số âm hoặc bằng 0) → Hệ thống sử dụng giá trị mặc định

#### Ghi chú kỹ thuật
- MinimumIntervalDays: cấu hình trong TreatmentSettings per TreatmentType
- Mặc định: IPL = 14 ngày, LLLT = 7 ngày, LidCare = 7 ngày
- Validation trong domain: TreatmentPackage.ValidateSessionInterval(newSessionDate)
- API: POST /api/treatment/packages/{id}/sessions (validation trả về warning, không hard-block)

---

## TRT-06: Nhiều liệu trình đồng thời

### US-TRT-006: Bệnh nhân có thể có nhiều liệu trình đồng thời

**Là một** bác sĩ,
**Tôi muốn** tạo nhiều liệu trình điều trị đồng thời cho cùng một bệnh nhân (ví dụ: IPL + LLLT),
**Để** phối hợp nhiều phương pháp điều trị nhằm đạt hiệu quả tốt nhất cho bệnh nhân khô mắt.

**Yêu cầu liên quan:** TRT-06

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở hồ sơ bệnh nhân → Tab "Liệu trình điều trị" → Nhấn "Tạo liệu trình mới"
2. Bệnh nhân đã có liệu trình IPL đang hoạt động → Bác sĩ tạo thêm liệu trình LLLT
3. Hệ thống cho phép tạo liệu trình mới mà không yêu cầu hoàn thành liệu trình cũ
4. Danh sách liệu trình hiển thị tất cả các liệu trình đang hoạt động với trạng thái và tiến trình riêng biệt
5. Mỗi liệu trình có lịch điều trị độc lập, không ảnh hưởng lẫn nhau

**Trường hợp ngoại lệ:**
1. Bệnh nhân có nhiều liệu trình cùng loại (ví dụ: 2 liệu trình IPL) → Hệ thống cảnh báo nhưng vẫn cho phép (bác sĩ quyết định)
2. Bệnh nhân có quá nhiều liệu trình đang hoạt động (>5) → Hệ thống hiển thị cảnh báo: "Bệnh nhân đang có [N] liệu trình hoạt động. Vui lòng xác nhận"

**Trường hợp lỗi:**
1. Lỗi tải danh sách liệu trình → Hệ thống hiển thị toast lỗi: "Không thể tải danh sách liệu trình"
2. Xung đột lịch điều trị (2 buổi cùng ngày cùng giờ) → Hệ thống cảnh báo: "Bệnh nhân đã có buổi điều trị khác vào thời gian này"

#### Ghi chú kỹ thuật
- Không có ràng buộc unique trên (PatientId, TreatmentType, Status=Active)
- Danh sách liệu trình: GET /api/treatment/packages?patientId={id}&status=Active
- Frontend: TreatmentPackageList component hiển thị tất cả liệu trình theo tab hoặc danh sách

---

## TRT-07: Chỉnh sửa liệu trình giữa chừng

### US-TRT-007: Bác sĩ chỉnh sửa liệu trình giữa chừng

**Là một** bác sĩ,
**Tôi muốn** chỉnh sửa liệu trình điều trị đang thực hiện (thêm/bớt buổi, thay đổi thông số),
**Để** điều chỉnh phác đồ phù hợp với phản ứng điều trị thực tế của bệnh nhân.

**Yêu cầu liên quan:** TRT-07

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở chi tiết liệu trình đang hoạt động → Nhấn "Chỉnh sửa liệu trình"
2. Bác sĩ có thể thay đổi: số buổi tổng (tăng hoặc giảm), giá liệu trình, ghi chú phác đồ
3. Bác sĩ nhập lý do chỉnh sửa (bắt buộc) → Nhấn "Lưu thay đổi"
4. Hệ thống cập nhật liệu trình và ghi nhận lịch sử chỉnh sửa (audit trail)
5. Thanh tiến trình cập nhật theo tổng buổi mới

**Trường hợp ngoại lệ:**
1. Giảm số buổi xuống thấp hơn số buổi đã hoàn thành → Hệ thống hiển thị lỗi: "Không thể giảm xuống [X] buổi vì đã hoàn thành [Y] buổi"
2. Liệu trình đã hoàn thành → Bác sĩ thêm buổi mới → Trạng thái tự động chuyển lại "Đang hoạt động"
3. Chỉnh sửa giá ảnh hưởng đến hóa đơn đã phát hành → Hệ thống cảnh báo: "Hóa đơn liên quan đã được phát hành. Chênh lệch giá sẽ được ghi nhận riêng"

**Trường hợp lỗi:**
1. Thiếu lý do chỉnh sửa → Hệ thống hiển thị: "Vui lòng nhập lý do chỉnh sửa liệu trình"
2. Số buổi không hợp lệ (0 hoặc âm) → Hệ thống hiển thị lỗi validation
3. Lỗi lưu chỉnh sửa → Hệ thống hiển thị toast lỗi và giữ nguyên dữ liệu cũ

#### Ghi chú kỹ thuật
- TreatmentPackage.ModifyProtocol(newTotalSessions, newPrice, reason): domain method
- TreatmentPackageModification entity: ModificationId, PackageId, PreviousTotalSessions, NewTotalSessions, PreviousPrice, NewPrice, Reason, ModifiedBy, ModifiedAt
- Audit trail: lưu mọi thay đổi qua modification records
- API endpoint: PUT /api/treatment/packages/{id}

---

## TRT-08: Chuyển đổi loại điều trị

### US-TRT-008: Bác sĩ chuyển đổi loại điều trị giữa chừng

**Là một** bác sĩ,
**Tôi muốn** chuyển bệnh nhân từ loại điều trị này sang loại khác giữa liệu trình (ví dụ: từ IPL sang LLLT),
**Để** thay đổi phương pháp điều trị khi bệnh nhân không đáp ứng tốt với phương pháp hiện tại.

**Yêu cầu liên quan:** TRT-08

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở chi tiết liệu trình đang hoạt động → Nhấn "Chuyển đổi loại điều trị"
2. Hệ thống hiển thị thông tin hiện tại: loại điều trị, số buổi đã thực hiện, số buổi còn lại
3. Bác sĩ chọn loại điều trị mới (ví dụ: từ IPL → LLLT), nhập số buổi mới, giá mới, và lý do chuyển đổi
4. Bác sĩ xác nhận chuyển đổi → Hệ thống tạo liệu trình mới với loại điều trị mới và đánh dấu liệu trình cũ là "Đã chuyển đổi" (Switched)
5. Liệu trình mới liên kết với liệu trình cũ để truy vết lịch sử

**Trường hợp ngoại lệ:**
1. Chuyển đổi sang cùng loại điều trị → Hệ thống cảnh báo: "Loại điều trị mới trùng với loại hiện tại. Bạn có muốn chỉnh sửa liệu trình hiện tại thay vì chuyển đổi?"
2. Liệu trình cũ chưa có buổi nào → Hệ thống hỏi: "Liệu trình hiện tại chưa có buổi điều trị. Bạn muốn hủy liệu trình này và tạo mới?"

**Trường hợp lỗi:**
1. Thiếu lý do chuyển đổi → Hệ thống hiển thị: "Vui lòng nhập lý do chuyển đổi loại điều trị"
2. Lỗi tạo liệu trình mới → Hệ thống không đánh dấu liệu trình cũ là "Đã chuyển đổi" (rollback) và hiển thị lỗi
3. Không có quyền Doctor → Hệ thống trả về lỗi 403: "Chỉ bác sĩ mới có quyền chuyển đổi loại điều trị"

#### Ghi chú kỹ thuật
- TreatmentPackage.SwitchType(newType, newSessions, newPrice, reason): domain method
- Tạo TreatmentPackage mới với PreviousPackageId = liệu trình cũ
- Status cũ: Switched; Status mới: Active
- API endpoint: POST /api/treatment/packages/{id}/switch

---

## TRT-09: Hủy liệu trình và hoàn tiền

### US-TRT-009: Quản lý duyệt hủy liệu trình với phí khấu trừ

**Là một** quản lý,
**Tôi muốn** xử lý yêu cầu hủy liệu trình điều trị của bệnh nhân với phí khấu trừ cấu hình được (10-20%),
**Để** đảm bảo quy trình hủy minh bạch và bảo vệ doanh thu phòng khám.

**Yêu cầu liên quan:** TRT-09

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bệnh nhân yêu cầu hủy liệu trình → Nhân viên mở chi tiết liệu trình → Nhấn "Yêu cầu hủy"
2. Hệ thống hiển thị thông tin: tổng giá trị liệu trình, số buổi đã thực hiện, số buổi còn lại, số tiền hoàn trả dự kiến
3. Hệ thống tính toán phí khấu trừ: phí hủy = phần trăm cấu hình (10-20%) x giá trị các buổi chưa thực hiện
4. Số tiền hoàn trả = Giá trị buổi chưa thực hiện - Phí khấu trừ
5. Quản lý xem xét và phê duyệt → Nhập mã PIN xác thực → Nhấn "Phê duyệt hủy"
6. Hệ thống chuyển trạng thái liệu trình sang "Đã hủy" (Cancelled) và ghi nhận khoản hoàn trả

**Trường hợp ngoại lệ:**
1. Liệu trình chưa bắt đầu (0 buổi hoàn thành) → Hoàn trả toàn bộ trừ phí khấu trừ
2. Liệu trình đã hoàn thành tất cả buổi → Không cho phép hủy: "Liệu trình đã hoàn tất, không thể hủy"
3. Quản lý từ chối yêu cầu hủy → Nhập lý do từ chối → Trạng thái giữ nguyên "Đang hoạt động"
4. Phần trăm khấu trừ ngoài khoảng 10-20% → Quản lý có thể điều chỉnh trong khoảng cho phép

**Trường hợp lỗi:**
1. Sai mã PIN quản lý → Hệ thống hiển thị: "Mã PIN không đúng. Vui lòng thử lại"
2. Lỗi xử lý hoàn tiền → Hệ thống hiển thị lỗi và không chuyển trạng thái liệu trình
3. Cấu hình phí khấu trừ không hợp lệ → Hệ thống sử dụng giá trị mặc định (10%)

#### Ghi chú kỹ thuật
- TreatmentPackage.Cancel(deductionPercent, approvedBy, reason): domain method
- CancellationDeductionPercent: cấu hình trong TreatmentSettings (mặc định 10%, khoảng 10-20%)
- RefundAmount = (TotalSessions - CompletedSessions) * SessionPrice * (1 - DeductionPercent/100)
- VerifyManagerPin endpoint: tái sử dụng từ Auth module
- API endpoint: POST /api/treatment/packages/{id}/cancel

---

## TRT-10: Phân quyền bác sĩ

### US-TRT-010: Chỉ bác sĩ mới được tạo và chỉnh sửa phác đồ điều trị

**Là một** quản trị viên hệ thống,
**Tôi muốn** chỉ cho phép người dùng có vai trò Bác sĩ (Doctor) tạo và chỉnh sửa phác đồ điều trị,
**Để** đảm bảo an toàn y khoa và chỉ bác sĩ có chuyên môn mới quyết định phác đồ cho bệnh nhân.

**Yêu cầu liên quan:** TRT-10

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Người dùng có vai trò Doctor đăng nhập → Truy cập tab "Liệu trình điều trị" → Các nút "Tạo liệu trình", "Chỉnh sửa", "Chuyển đổi loại" hiển thị đầy đủ
2. Bác sĩ tạo/chỉnh sửa/chuyển đổi liệu trình → Hệ thống xử lý thành công
3. Backend xác thực quyền qua middleware: chỉ role Doctor được phép gọi các API tạo/sửa liệu trình

**Trường hợp ngoại lệ:**
1. Nhân viên (không phải bác sĩ) cần xem thông tin liệu trình → Cho phép xem (read-only) nhưng ẩn các nút thao tác
2. Quản lý cần hủy liệu trình (TRT-09) → Quản lý có quyền hủy nhưng không có quyền tạo/sửa phác đồ
3. Bác sĩ bị thu hồi quyền → Các liệu trình đang hoạt động vẫn giữ nguyên nhưng không thể tạo/sửa mới

**Trường hợp lỗi:**
1. Người dùng không có quyền Doctor cố gắng tạo liệu trình qua API → Backend trả về lỗi 403: "Chỉ bác sĩ mới có quyền tạo phác đồ điều trị"
2. Người dùng không có quyền Doctor cố gắng chỉnh sửa → Backend trả về lỗi 403: "Chỉ bác sĩ mới có quyền chỉnh sửa phác đồ điều trị"
3. Token hết hạn khi đang thao tác → Hệ thống chuyển hướng đến trang đăng nhập

#### Ghi chú kỹ thuật
- Authorization policy: RequireRole("Doctor") trên các endpoint tạo/sửa/switch liệu trình
- Frontend: kiểm tra user.roles.includes("Doctor") để hiển thị/ẩn nút thao tác
- Endpoint hủy: RequireRole("Manager") hoặc RequireRole("Owner")
- Permissions: Treatment.Create, Treatment.Edit, Treatment.Switch (Doctor), Treatment.Cancel (Manager)

---

## TRT-11: Ghi nhận vật tư tiêu hao

### US-TRT-011: Hệ thống ghi nhận vật tư tiêu hao mỗi buổi điều trị

**Là một** bác sĩ / nhân viên,
**Tôi muốn** ghi nhận các vật tư tiêu hao đã sử dụng trong mỗi buổi điều trị (gel, miếng dán, thuốc nhỏ mắt...),
**Để** theo dõi chi phí vật tư, quản lý tồn kho, và tính toán giá thành điều trị chính xác.

**Yêu cầu liên quan:** TRT-11

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ hoặc nhân viên mở form ghi nhận buổi điều trị → Mục "Vật tư tiêu hao" hiển thị
2. Người dùng nhấn "Thêm vật tư" → Tìm kiếm vật tư từ kho tiêu hao (Consumables Warehouse, Phase 6)
3. Người dùng chọn vật tư, nhập số lượng sử dụng → Hệ thống hiển thị tồn kho hiện tại và đơn giá
4. Người dùng có thể thêm nhiều loại vật tư cho một buổi điều trị
5. Khi lưu buổi điều trị → Hệ thống tự động trừ tồn kho vật tư tương ứng từ kho tiêu hao
6. Danh sách vật tư đã sử dụng hiển thị trong chi tiết buổi điều trị

**Trường hợp ngoại lệ:**
1. Vật tư hết tồn kho → Hệ thống cảnh báo: "Vật tư [tên] đã hết hàng trong kho" nhưng vẫn cho phép ghi nhận (xử lý nhập kho sau)
2. Buổi điều trị không sử dụng vật tư → Người dùng bỏ qua mục vật tư, lưu buổi điều trị bình thường
3. Cần sửa số lượng vật tư đã ghi nhận → Bác sĩ chỉnh sửa buổi điều trị → Hệ thống cập nhật lại tồn kho (hoàn trả/trừ thêm)

**Trường hợp lỗi:**
1. Số lượng vật tư nhập âm hoặc bằng 0 → Hệ thống hiển thị lỗi: "Số lượng phải lớn hơn 0"
2. Lỗi trừ tồn kho → Hệ thống hiển thị cảnh báo nhưng vẫn lưu buổi điều trị (ghi log để xử lý sau)
3. Vật tư không tồn tại trong kho → Hệ thống hiển thị: "Vật tư không tìm thấy. Vui lòng kiểm tra lại"

#### Ghi chú kỹ thuật
- SessionConsumable entity: SessionId, ConsumableId (FK to Pharmacy.Consumable), Quantity, UnitCost
- Cross-module: tham chiếu Pharmacy.Contracts để lấy danh sách vật tư và trừ tồn kho
- Domain event: ConsumableUsedInSession → trừ tồn kho trong Pharmacy module
- API endpoint: POST /api/treatment/sessions/{id}/consumables
- Frontend component: ConsumableSelector trong TreatmentSessionForm

---

## Tóm tắt User Stories

| ID | Tên | Yêu cầu | Vai trò |
|----|-----|---------|---------|
| US-TRT-001 | Tạo gói liệu trình IPL/LLLT/chăm sóc mi mắt | TRT-01 | Bác sĩ |
| US-TRT-002 | Theo dõi buổi điều trị đã hoàn thành và còn lại | TRT-02 | Bác sĩ / Nhân viên |
| US-TRT-003 | Ghi nhận điểm OSDI mỗi buổi điều trị | TRT-03 | Bác sĩ |
| US-TRT-004 | Tự động đánh dấu hoàn thành khi tất cả buổi xong | TRT-04 | Hệ thống |
| US-TRT-005 | Kiểm tra khoảng cách tối thiểu giữa các buổi | TRT-05 | Bác sĩ |
| US-TRT-006 | Nhiều liệu trình đồng thời cho cùng bệnh nhân | TRT-06 | Bác sĩ |
| US-TRT-007 | Chỉnh sửa liệu trình giữa chừng | TRT-07 | Bác sĩ |
| US-TRT-008 | Chuyển đổi loại điều trị giữa chừng | TRT-08 | Bác sĩ |
| US-TRT-009 | Duyệt hủy liệu trình với phí khấu trừ | TRT-09 | Quản lý |
| US-TRT-010 | Chỉ bác sĩ mới được tạo và chỉnh sửa phác đồ | TRT-10 | Quản trị viên |
| US-TRT-011 | Ghi nhận vật tư tiêu hao mỗi buổi điều trị | TRT-11 | Bác sĩ / Nhân viên |
