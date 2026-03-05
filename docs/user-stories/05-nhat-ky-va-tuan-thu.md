# Nhật ký và Tuân thủ - User Stories

**Phạm vi:** Nhật ký kiểm toán (audit trail) cấp trường dữ liệu, nhật ký truy cập, tính bất biến của dữ liệu kiểm toán, và tuân thủ ICD-10 cho Sở Y tế
**Yêu cầu liên quan:** AUD-01, AUD-02, AUD-03, AUD-04
**Số lượng user stories:** 6

---

## US-AUD-001: Ghi nhật ký kiểm toán cấp trường dữ liệu

**Là một** quản trị viên, **Tôi muốn** hệ thống tự động ghi lại mọi thay đổi trên hồ sơ bệnh án ở cấp trường dữ liệu, **Để** có thể truy vết chính xác ai đã thay đổi gì, khi nào, giá trị cũ và mới.

**Yêu cầu liên quan:** AUD-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên cập nhật thông tin bệnh nhân (ví dụ: số điện thoại) → Hệ thống tự động ghi nhận bản ghi kiểm toán với các trường: người thực hiện, thời gian, thực thể bị thay đổi, trường thay đổi, giá trị cũ, giá trị mới
2. Bác sĩ cập nhật dữ liệu khúc xạ trong lượt khám → Hệ thống ghi nhận từng trường thay đổi (SPH, CYL, AXIS, v.v.) với giá trị cũ/mới riêng biệt
3. Nhân viên tạo mới bản ghi (bệnh nhân, lượt khám, lịch hẹn) → Hệ thống ghi nhận hành động "Tạo mới" với toàn bộ giá trị ban đầu

**Trường hợp ngoại lệ:**
1. Nếu cập nhật nhiều trường cùng lúc → Hệ thống ghi nhận từng trường thay đổi riêng biệt trong cùng một bản ghi kiểm toán
2. Nếu giá trị cũ và mới giống nhau → Hệ thống không tạo bản ghi kiểm toán cho trường đó

**Trường hợp lỗi:**
1. Khi ghi nhật ký kiểm toán thất bại → Hệ thống vẫn hoàn thành thao tác chính nhưng ghi log lỗi để theo dõi

### Ghi chú kỹ thuật
- AuditInterceptor đặt trong Audit.Infrastructure (không phải Shared.Infrastructure) để tránh tham chiếu vòng giữa các project
- Sử dụng pattern IAuditReadContext/IAuditReadRepository cho truy cập DB ở tầng Application
- EF Core SaveChangesInterceptor tự động phát hiện thay đổi trên các entity implement IAuditable
- Bản ghi kiểm toán lưu trong schema "audit" riêng biệt

---

## US-AUD-002: Ghi nhật ký truy cập hệ thống

**Là một** quản trị viên, **Tôi muốn** hệ thống ghi lại tất cả lượt đăng nhập, đăng xuất, và truy cập hồ sơ bệnh án, **Để** giám sát được ai đã truy cập thông tin gì và khi nào.

**Yêu cầu liên quan:** AUD-02

> Xem thêm: US-AUTH-005 (Ghi nhật ký đăng nhập/đăng xuất trong quy trình xác thực)

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên đăng nhập thành công → Hệ thống ghi nhận bản ghi truy cập với: người dùng, thời gian, hành động "Đăng nhập", địa chỉ IP
2. Nhân viên đăng xuất → Hệ thống ghi nhận hành động "Đăng xuất"
3. Nhân viên xem hồ sơ bệnh nhân → Hệ thống ghi nhận hành động "Xem bản ghi" với thông tin thực thể được truy cập

**Trường hợp ngoại lệ:**
1. Nếu đăng nhập thất bại (sai mật khẩu) → Hệ thống vẫn ghi nhận lần đăng nhập thất bại với lý do
2. Nếu phiên hết hạn tự động → Hệ thống ghi nhận sự kiện hết phiên

**Trường hợp lỗi:**
1. Khi không thể xác định người dùng (token không hợp lệ) → Hệ thống ghi nhận truy cập ẩn danh với thông tin request

### Ghi chú kỹ thuật
- AccessLoggingMiddleware trong Audit.Infrastructure xử lý ghi nhật ký truy cập
- Middleware chặn tất cả HTTP request đến các endpoint được bảo vệ
- Hành động được phân loại: Đăng nhập, Đăng nhập thất bại, Đăng xuất, Xem bản ghi, Tạo mới, Cập nhật, Xóa, Xuất dữ liệu
- API endpoint: GET /api/audit/access-logs (phân quyền Admin)

---

## US-AUD-003: Tính bất biến và lưu trữ nhật ký kiểm toán

**Là một** quản trị viên, **Tôi muốn** nhật ký kiểm toán không thể bị sửa đổi hoặc xóa, và được lưu trữ tối thiểu 10 năm, **Để** đảm bảo tính toàn vẹn dữ liệu cho mục đích pháp lý và tuân thủ quy định.

**Yêu cầu liên quan:** AUD-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Hệ thống lưu trữ bản ghi kiểm toán → Bản ghi không có chức năng cập nhật hoặc xóa trong toàn bộ hệ thống
2. Quản trị viên xem nhật ký kiểm toán → Chỉ có quyền đọc, không có nút sửa hoặc xóa
3. Sau 10 năm → Dữ liệu kiểm toán vẫn có thể truy xuất được

**Trường hợp ngoại lệ:**
1. Nếu database đầy → Hệ thống cảnh báo quản trị viên nhưng không tự động xóa dữ liệu kiểm toán
2. Nếu cần di chuyển dữ liệu cũ → Chỉ cho phép lưu trữ (archive) sang cold storage, không cho phép xóa

**Trường hợp lỗi:**
1. Khi có nỗ lực xóa bản ghi kiểm toán qua API → Hệ thống từ chối với mã lỗi 403 (Forbidden)
2. Khi có nỗ lực cập nhật bản ghi kiểm toán → Hệ thống từ chối thao tác

### Ghi chú kỹ thuật
- Repository cho audit log không cung cấp phương thức Update hoặc Delete
- Không có API endpoint cho DELETE hoặc PUT trên audit records
- Chính sách lưu trữ 10 năm được thực thi qua cấu hình Azure SQL retention
- Schema "audit" tách biệt giúp quản lý vòng đời dữ liệu kiểm toán độc lập

---

## US-AUD-004: Hỗ trợ mã ICD-10 cho sẵn sàng dữ liệu Sở Y tế

**Là một** quản trị viên, **Tôi muốn** hệ thống hỗ trợ mã bệnh ICD-10 từ ngày đầu tiên, **Để** đảm bảo dữ liệu phòng khám sẵn sàng khi Sở Y tế yêu cầu (hạn chót 31/12/2026).

**Yêu cầu liên quan:** AUD-04

> Xem thêm: US-CLN-007, US-CLN-008 (Sử dụng ICD-10 trong quy trình chẩn đoán lâm sàng)

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Khi hệ thống khởi động lần đầu → Dữ liệu ICD-10 được tự động seed vào cơ sở dữ liệu
2. Bác sĩ tìm kiếm mã bệnh → Hệ thống trả về kết quả ICD-10 bằng cả tiếng Việt và tiếng Anh
3. Bác sĩ chẩn đoán cho bệnh nhân → Mã ICD-10 được lưu cùng với bản ghi lượt khám

**Trường hợp ngoại lệ:**
1. Nếu mã ICD-10 cần cập nhật phiên bản mới → Hệ thống hỗ trợ cập nhật dữ liệu tham chiếu mà không ảnh hưởng đến dữ liệu chẩn đoán đã lưu
2. Nếu bác sĩ tìm kiếm mã không tồn tại → Hệ thống hiển thị "Không tìm thấy kết quả"

**Trường hợp lỗi:**
1. Khi dữ liệu seed ICD-10 bị lỗi → Hệ thống ghi log lỗi và cho phép vận hành thủ công

### Ghi chú kỹ thuật
- ReferenceDbContext với schema "reference" chứa dữ liệu ICD-10 dùng chung giữa các module
- Dữ liệu ICD-10 seed dưới dạng EmbeddedResource trong assembly Audit.Infrastructure
- Bảng Icd10Code chứa: Code, NameVi, NameEn, RequiresLaterality, IsFavorite
- DoctorIcd10Favorite lưu danh sách mã yêu thích riêng của từng bác sĩ
- API endpoint: GET /api/clinical/icd10/search?query={term}

---

## US-AUD-005: Quản trị viên xem nhật ký kiểm toán

**Là một** quản trị viên, **Tôi muốn** xem và lọc nhật ký kiểm toán theo nhiều tiêu chí, **Để** giám sát hoạt động của nhân viên và phát hiện các thay đổi bất thường.

**Yêu cầu liên quan:** AUD-01, AUD-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản trị viên truy cập trang "Nhật ký hoạt động" → Hệ thống hiển thị danh sách nhật ký kiểm toán mới nhất với phân trang
2. Quản trị viên lọc theo người thực hiện → Hệ thống chỉ hiển thị các bản ghi của người dùng được chọn
3. Quản trị viên lọc theo loại hành động (Tạo mới, Cập nhật, Xóa, Xem) → Hệ thống lọc kết quả tương ứng
4. Quản trị viên lọc theo khoảng thời gian (Từ ngày - Đến ngày) → Hệ thống hiển thị bản ghi trong khoảng thời gian đó
5. Quản trị viên xem chi tiết một bản ghi → Hệ thống hiển thị thông tin trường thay đổi, giá trị cũ, giá trị mới

**Trường hợp ngoại lệ:**
1. Nếu không có kết quả phù hợp với bộ lọc → Hệ thống hiển thị "Không tìm thấy nhật ký"
2. Nếu bản ghi không có thay đổi chi tiết (ví dụ: hành động "Xem") → Hệ thống hiển thị "Không có thay đổi chi tiết"

**Trường hợp lỗi:**
1. Khi người dùng không có quyền Admin → Hệ thống từ chối truy cập trang nhật ký

### Ghi chú kỹ thuật
- Trang nhật ký nằm trong menu Quản trị (sidebar: "Nhật ký hoạt động")
- Yêu cầu quyền Audit.View hoặc Auth.Manage để truy cập
- Bộ lọc bao gồm: Người dùng, Loại hành động, Khoảng thời gian (Từ ngày, Đến ngày)
- API endpoint: GET /api/audit/logs?user={userId}&actionType={type}&from={date}&to={date}&page={n}
- Bảng hiển thị các cột: Người thực hiện, Hành động, Thực thể, Chi tiết, Thời gian
- Hỗ trợ sao chép bản ghi (nút "Sao chép")

---

## US-AUD-006: Quản trị viên xem nhật ký truy cập và xuất báo cáo

**Là một** quản trị viên, **Tôi muốn** xem nhật ký truy cập hệ thống và xuất dữ liệu ra file CSV, **Để** phục vụ báo cáo tuân thủ và kiểm tra bảo mật.

**Yêu cầu liên quan:** AUD-01, AUD-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản trị viên truy cập trang nhật ký truy cập → Hệ thống hiển thị danh sách các lượt đăng nhập, đăng xuất, và truy cập bản ghi
2. Quản trị viên lọc theo lượt đăng nhập thất bại → Hệ thống chỉ hiển thị các lần đăng nhập không thành công
3. Quản trị viên lọc theo truy cập bản ghi bệnh nhân → Hệ thống hiển thị ai đã xem hồ sơ nào
4. Quản trị viên nhấn "Xuất CSV" → Hệ thống tạo file CSV chứa dữ liệu nhật ký theo bộ lọc hiện tại và tải về

**Trường hợp ngoại lệ:**
1. Nếu dữ liệu xuất quá lớn → Hệ thống giới hạn số bản ghi xuất và thông báo cho người dùng
2. Nếu không có dữ liệu để xuất → Nút "Xuất CSV" bị vô hiệu hóa

**Trường hợp lỗi:**
1. Khi quá trình xuất CSV gặp lỗi → Hệ thống hiển thị thông báo lỗi và cho phép thử lại

### Ghi chú kỹ thuật
- API endpoint xuất: GET /api/audit/logs/export (trả về file CSV)
- File CSV bao gồm các cột: Thời gian, Người dùng, Hành động, Thực thể, Chi tiết
- Phân trang hỗ trợ điều hướng: Trang trước, Trang sau
- Nút xuất CSV nằm trong thanh công cụ phía trên bảng dữ liệu

---

*Tài liệu này mô tả các user stories cho chức năng Nhật ký và Tuân thủ đã được triển khai trong Phase 1.*
*Cập nhật: 2026-03-05*
