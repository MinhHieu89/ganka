# Xác thực và Phân quyền - User Stories

**Phạm vi:** Quản lý đăng nhập, phiên làm việc, vai trò, quyền hạn và nhật ký truy cập cho toàn bộ nhân viên phòng khám Ganka28.
**Yêu cầu liên quan:** AUTH-01, AUTH-02, AUTH-03, AUTH-04, AUTH-05
**Số lượng user stories:** 8

---

## US-AUTH-001: Đăng nhập hệ thống

**Là một** nhân viên phòng khám, **Tôi muốn** đăng nhập vào hệ thống bằng email và mật khẩu, **Để** truy cập các chức năng theo vai trò của tôi.

**Yêu cầu liên quan:** AUTH-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Người dùng truy cập trang đăng nhập (`/login`) → Hệ thống hiển thị form với các trường: **Email**, **Mật khẩu**, **Ghi nhớ đăng nhập**
2. Người dùng nhập email và mật khẩu hợp lệ → Hệ thống xác thực thành công qua bcrypt validation
3. Hệ thống tạo JWT token chứa role-based claims (userId, email, roles[], permissions[]) → Chuyển hướng đến Dashboard (Trang chủ)
4. Nếu chọn "Ghi nhớ đăng nhập" → Phiên đăng nhập được lưu qua HTTP-only cookie với thời hạn dài hơn

**Trường hợp ngoại lệ:**
1. Nếu người dùng để trống email hoặc mật khẩu → Hệ thống hiển thị lỗi validation tương ứng
2. Nếu người dùng đã đăng nhập và truy cập `/login` → Hệ thống chuyển hướng về Dashboard

**Trường hợp lỗi:**
1. Khi nhập sai email hoặc mật khẩu → Hệ thống hiển thị thông báo "Tên đăng nhập hoặc mật khẩu không đúng"
2. Khi tài khoản bị vô hiệu hóa (Không hoạt động) → Hệ thống từ chối đăng nhập

### Ghi chú kỹ thuật
- API endpoint: `POST /api/auth/login`
- JWT token chứa: userId, email, roles[], permissions[]
- Refresh token lưu trong HTTP-only cookie (không lưu trong JavaScript/localStorage)
- Mật khẩu xác thực bằng bcrypt
- Frontend gửi trường `email` (username field maps to email parameter)
- Zod schema định nghĩa bên trong component function để truy cập hàm `t()` cho thông báo lỗi đa ngôn ngữ

---

## US-AUTH-002: Hệ thống hỗ trợ 8 vai trò nhân viên

**Là một** quản trị viên, **Tôi muốn** hệ thống hỗ trợ phân chia nhân viên theo 8 vai trò khác nhau, **Để** mỗi người chỉ truy cập được các chức năng phù hợp với vị trí công việc.

**Yêu cầu liên quan:** AUTH-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Hệ thống hỗ trợ 8 vai trò với tên tiếng Việt:
   - Quản trị viên (Admin)
   - Bác sĩ (Doctor)
   - Kỹ thuật viên (Technician)
   - Điều dưỡng (Nurse)
   - Thu ngân (Cashier)
   - Nhân viên kính mắt (OpticalStaff)
   - Quản lý (Manager)
   - Kế toán (Accountant)
2. Mỗi vai trò có bộ quyền mặc định (preset permission templates) phù hợp với chức năng công việc
3. Quản trị viên có thể gán một hoặc nhiều vai trò cho mỗi người dùng khi tạo hoặc chỉnh sửa tài khoản

**Trường hợp ngoại lệ:**
1. Nếu một người dùng có nhiều vai trò → Hệ thống gộp tất cả quyền từ các vai trò đã gán
2. Nếu vai trò hệ thống (System Role) → Không thể xóa, chỉ có thể chỉnh sửa quyền hạn

**Trường hợp lỗi:**
1. Khi cố gán vai trò không tồn tại → Hệ thống trả về lỗi validation

### Ghi chú kỹ thuật
- Vai trò được phân loại: vai trò hệ thống (Vai trò hệ thống) và vai trò tùy chỉnh (Vai trò tùy chỉnh)
- JWT claims chứa danh sách roles[] và permissions[] tổng hợp
- Sidebar menu hiển thị có điều kiện dựa trên quyền Auth.Manage hoặc Auth.View

---

## US-AUTH-003: Cấu hình quyền hạn chi tiết theo vai trò

**Là một** quản trị viên, **Tôi muốn** cấu hình quyền CRUD chi tiết cho từng vai trò trên từng module/hành động, **Để** kiểm soát chính xác ai được phép làm gì trong hệ thống.

**Yêu cầu liên quan:** AUTH-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản trị viên truy cập trang Quản lý vai trò → Hệ thống hiển thị danh sách vai trò với số quyền (Số quyền) cho mỗi vai trò
2. Quản trị viên chọn một vai trò → Hệ thống hiển thị ma trận quyền (permission matrix) theo module:
   - Các module: Xác thực, Bệnh nhân, Khám bệnh, Lịch hẹn, Nhà thuốc, Kính mắt, Thu ngân, Điều trị, Nhật ký, Cài đặt
   - Các hành động: Xem, Tạo, Sửa, Xóa, Xuất, Quản lý
3. Quản trị viên đánh dấu/bỏ đánh dấu các quyền → Nhấn "Lưu thay đổi" → Hệ thống cập nhật và hiển thị "Cập nhật quyền thành công"
4. Có nút "Chọn tất cả" để đánh dấu toàn bộ quyền của một module

**Trường hợp ngoại lệ:**
1. Nếu thu hồi quyền khi người dùng đang đăng nhập → Quyền mới áp dụng khi token được làm mới (silent refresh)
2. Nếu tạo vai trò mới → Hệ thống cho phép kế thừa quyền từ preset template hoặc cấu hình từ đầu

**Trường hợp lỗi:**
1. Khi cố cập nhật quyền mà không có quyền Auth.Manage → Hệ thống trả về lỗi 403 (Forbidden)

### Ghi chú kỹ thuật
- Permission matrix UI hiển thị dạng bảng module x hành động
- Quyền lưu dưới dạng `{Module}.{Action}` (ví dụ: `Patient.Create`, `Clinical.View`)
- FluentValidation xác thực request trước khi xử lý
- Khi vai trò được cập nhật, JWT token hiện tại vẫn giữ quyền cũ cho đến khi refresh

---

## US-AUTH-004: Duy trì phiên đăng nhập và hết hạn tự động

**Là một** nhân viên phòng khám, **Tôi muốn** phiên đăng nhập được duy trì khi tải lại trang và tự động hết hạn sau thời gian không hoạt động, **Để** không phải đăng nhập lại liên tục nhưng vẫn đảm bảo bảo mật.

**Yêu cầu liên quan:** AUTH-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Người dùng đăng nhập thành công → Hệ thống lưu refresh token trong HTTP-only cookie
2. Người dùng tải lại trang (F5/refresh) → Hệ thống thực hiện silent refresh tự động → Phiên được khôi phục mà không cần đăng nhập lại
3. Người dùng không hoạt động trong 28 phút → Hệ thống hiển thị cảnh báo "Phiên đăng nhập sắp hết hạn" (AlertDialog, không thể đóng bằng click ngoài) với đếm ngược 2 phút
4. Trong cảnh báo, người dùng có hai lựa chọn:
   - Nhấn "Gia hạn" → Hệ thống gia hạn phiên làm việc
   - Nhấn "Đăng xuất" → Hệ thống đăng xuất ngay lập tức
5. Nếu hết 2 phút không thao tác → Hệ thống tự động đăng xuất → Hiển thị "Phiên đăng nhập đã hết hạn"

**Trường hợp ngoại lệ:**
1. Nếu mở nhiều tab trình duyệt → Tất cả tab chia sẻ cùng phiên qua HTTP-only cookie
2. Nếu chọn "Ghi nhớ đăng nhập" → Cookie có Max-Age dài hơn, phiên tồn tại lâu hơn
3. Nếu refresh token hết hạn → Hệ thống chuyển về trang đăng nhập

**Trường hợp lỗi:**
1. Khi silent refresh thất bại (refresh token không hợp lệ) → Hệ thống chuyển về trang đăng nhập
2. Khi server không phản hồi trong quá trình refresh → Hệ thống hiển thị trạng thái lỗi

### Ghi chú kỹ thuật
- Timeout: 30 phút không hoạt động, cảnh báo xuất hiện ở phút thứ 28
- Activity tracking throttled mỗi 30 giây (tránh gọi API quá nhiều)
- AlertDialog (non-dismissible) từ shadcn/ui cho modal cảnh báo
- Silent refresh thực hiện khi page load (beforeLoad trong TanStack Router)
- `silentRefresh` là hàm async độc lập (không phải hook) để dùng ngoài React context
- Refresh token rotation: RememberMe được trả về trong RefreshTokenResponse để xác định cookie Max-Age
- Tất cả API calls sử dụng `credentials: 'include'` để gửi HTTP-only cookie
- Presentation layer quản lý cookie logic -- handlers không biết về HTTP transport

---

## US-AUTH-005: Ghi nhật ký đăng nhập và truy cập

**Là một** quản trị viên, **Tôi muốn** hệ thống ghi lại tất cả hoạt động đăng nhập và truy cập dữ liệu, **Để** kiểm tra và giám sát việc sử dụng hệ thống.

**Yêu cầu liên quan:** AUTH-05

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Khi bất kỳ người dùng nào đăng nhập thành công → Hệ thống ghi lại: thời gian, email, IP, kết quả (thành công)
2. Khi đăng nhập thất bại → Hệ thống ghi lại: thời gian, email đã thử, IP, lý do thất bại
3. Khi người dùng xem hồ sơ bệnh nhân hoặc dữ liệu y tế → Hệ thống ghi nhật ký truy cập (access log)
4. Quản trị viên truy cập trang Nhật ký hoạt động → Xem danh sách nhật ký với bộ lọc theo thời gian, người dùng, loại hành động

**Trường hợp ngoại lệ:**
1. Nếu số lượng nhật ký lớn → Hệ thống hỗ trợ phân trang và lọc để tìm kiếm hiệu quả
2. Nếu người dùng đăng xuất → Hệ thống ghi lại sự kiện đăng xuất

**Trường hợp lỗi:**
1. Khi hệ thống ghi nhật ký gặp lỗi → Giao dịch chính vẫn hoàn tất (nhật ký không chặn luồng chính)

### Ghi chú kỹ thuật
- AuditInterceptor ghi nhật ký thay đổi dữ liệu ở mức field-level
- AccessLoggingMiddleware ghi nhật ký truy cập cho mọi request xem dữ liệu
- Cả hai đặt trong Audit.Infrastructure (không phải Shared.Infrastructure) để tránh circular reference
- Nhật ký không thể sửa hoặc xóa (immutable), lưu trữ tối thiểu 10 năm
- Bộ lọc trên trang nhật ký theo mẫu thiết kế của trang bệnh nhân (audit log filters restyled)

> Xem thêm: US-AUD-001, US-AUD-002 (Nhật ký và Tuân thủ - chi tiết về xem và quản lý nhật ký)

---

## US-AUTH-006: Quản trị viên tạo tài khoản người dùng mới

**Là một** quản trị viên, **Tôi muốn** tạo tài khoản mới cho nhân viên với họ tên, email và vai trò, **Để** nhân viên mới có thể truy cập hệ thống ngay.

**Yêu cầu liên quan:** AUTH-01, AUTH-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản trị viên truy cập trang Quản lý người dùng → Nhấn "Thêm người dùng"
2. Hệ thống hiển thị form với các trường: **Họ tên**, **Email**, **Mật khẩu**, **Chọn vai trò**
3. Quản trị viên điền thông tin và chọn một hoặc nhiều vai trò → Nhấn "Lưu"
4. Hệ thống tạo tài khoản thành công → Hiển thị thông báo "Tạo người dùng thành công"
5. Tài khoản mới xuất hiện trong danh sách người dùng với trạng thái "Hoạt động"

**Trường hợp ngoại lệ:**
1. Nếu không chọn vai trò nào → Hệ thống yêu cầu chọn ít nhất một vai trò

**Trường hợp lỗi:**
1. Khi email đã tồn tại → Hệ thống hiển thị lỗi trùng email
2. Khi mật khẩu không đủ mạnh → Hệ thống hiển thị yêu cầu mật khẩu (tối thiểu ký tự, chữ hoa, số, ký tự đặc biệt)

### Ghi chú kỹ thuật
- API endpoint: `POST /api/auth/users`
- Mật khẩu được hash bằng bcrypt trước khi lưu
- Sử dụng React Hook Form instance riêng cho chế độ tạo mới (tách biệt với chế độ chỉnh sửa)
- FluentValidation xác thực trên server, Zod schema xác thực trên client

---

## US-AUTH-007: Quản trị viên quản lý danh sách người dùng

**Là một** quản trị viên, **Tôi muốn** xem danh sách, chỉnh sửa thông tin và vô hiệu hóa/kích hoạt tài khoản người dùng, **Để** quản lý nhân viên phòng khám hiệu quả.

**Yêu cầu liên quan:** AUTH-01, AUTH-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản trị viên truy cập trang Quản lý người dùng → Hệ thống hiển thị bảng danh sách với các cột: Họ tên, Email, Vai trò, Trạng thái, Thao tác
2. Quản trị viên nhấn "Sửa" trên một người dùng → Hệ thống hiển thị form chỉnh sửa với thông tin hiện tại
3. Quản trị viên thay đổi thông tin → Nhấn "Lưu thay đổi" → Hiển thị "Cập nhật người dùng thành công"
4. Quản trị viên nhấn vô hiệu hóa → Hệ thống hiển thị xác nhận "Bạn có chắc chắn muốn vô hiệu hóa người dùng này?" → Xác nhận → Trạng thái chuyển thành "Không hoạt động"

**Trường hợp ngoại lệ:**
1. Nếu vô hiệu hóa người dùng đang đăng nhập → Phiên hiện tại vẫn hoạt động cho đến khi token hết hạn
2. Nếu đặt lại mật khẩu → Hệ thống tạo mật khẩu mới cho người dùng

**Trường hợp lỗi:**
1. Khi cố vô hiệu hóa tài khoản quản trị viên duy nhất → Hệ thống từ chối để tránh khóa toàn bộ hệ thống

### Ghi chú kỹ thuật
- DataTable nhận TanStack Table instance đã cấu hình sẵn (không nhận raw data)
- React Hook Form instance riêng cho chế độ chỉnh sửa (tách biệt với tạo mới để tránh union type issues)
- Trạng thái: Hoạt động / Không hoạt động
- API endpoints: `GET /api/auth/users`, `PUT /api/auth/users/{id}`, `PATCH /api/auth/users/{id}/status`

---

## US-AUTH-008: Quản trị viên tạo và cấu hình vai trò với ma trận quyền

**Là một** quản trị viên, **Tôi muốn** tạo vai trò mới và cấu hình quyền hạn thông qua ma trận quyền, **Để** linh hoạt phân quyền theo nhu cầu thực tế của phòng khám.

**Yêu cầu liên quan:** AUTH-02, AUTH-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản trị viên truy cập trang Quản lý vai trò → Nhấn "Tạo vai trò"
2. Hệ thống hiển thị form với các trường: **Tên**, **Mô tả**
3. Quản trị viên nhập thông tin vai trò → Hệ thống hiển thị ma trận quyền với tất cả module và hành động
4. Quản trị viên cấu hình quyền (đánh dấu/bỏ đánh dấu) → Nhấn "Lưu" → Hiển thị "Tạo vai trò thành công"
5. Vai trò mới xuất hiện trong danh sách với nhãn "Vai trò tùy chỉnh" và hiển thị số quyền đã cấu hình

**Trường hợp ngoại lệ:**
1. Nếu muốn tạo vai trò giống vai trò có sẵn → Có thể sao chép quyền từ preset template
2. Nếu chỉnh sửa vai trò hệ thống → Chỉ có thể thay đổi quyền, không thể xóa vai trò

**Trường hợp lỗi:**
1. Khi tạo vai trò với tên đã tồn tại → Hệ thống hiển thị lỗi trùng tên
2. Khi cố xóa vai trò đang được gán cho người dùng → Hệ thống từ chối và hiển thị cảnh báo

### Ghi chú kỹ thuật
- API endpoints: `POST /api/auth/roles`, `PUT /api/auth/roles/{id}`, `PUT /api/auth/roles/{id}/permissions`
- Vai trò hệ thống (systemRole) có 8 preset templates tương ứng 8 vai trò mặc định
- Vai trò tùy chỉnh (customRole) cho phép cấu hình tự do
- Permission matrix UI sử dụng dạng bảng với nút "Chọn tất cả" cho từng module
- RequireAuthorization áp dụng ở group level cho tất cả admin routes
