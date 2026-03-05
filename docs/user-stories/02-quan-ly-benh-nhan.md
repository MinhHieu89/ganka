# Quản lý Bệnh nhân - User Stories

**Phạm vi:** Đăng ký, tìm kiếm, quản lý hồ sơ và dị ứng bệnh nhân tại phòng khám Ganka28, bao gồm bệnh nhân khám bệnh và khách mua thuốc.
**Yêu cầu liên quan:** PAT-01, PAT-02, PAT-03, PAT-04, PAT-05
**Số lượng user stories:** 8

---

## US-PAT-001: Đăng ký bệnh nhân khám bệnh

**Là một** nhân viên phòng khám, **Tôi muốn** đăng ký hồ sơ bệnh nhân khám bệnh với đầy đủ thông tin cơ bản, **Để** tạo hồ sơ y tế và theo dõi quá trình khám chữa bệnh.

**Yêu cầu liên quan:** PAT-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên truy cập trang Bệnh nhân → Nhấn "Đăng ký bệnh nhân"
2. Hệ thống hiển thị form đăng ký với loại "Bệnh nhân khám bệnh" và các trường:
   - **Họ và tên** (bắt buộc, 3-50 ký tự)
   - **Số điện thoại** (bắt buộc, định dạng VD: 0901234567)
   - **Ngày sinh** (bắt buộc, date picker với định dạng dd/MM/yyyy)
   - **Giới tính** (bắt buộc: Nam / Nữ / Khác)
   - **Địa chỉ** (tùy chọn)
   - **Số CCCD** (tùy chọn)
3. Nhân viên điền thông tin hợp lệ → Nhấn "Lưu"
4. Hệ thống tự động tạo mã bệnh nhân theo định dạng **GK-YYYY-NNNN** (ví dụ: GK-2026-0001) → Hiển thị hồ sơ bệnh nhân mới

**Trường hợp ngoại lệ:**
1. Nếu đầu năm mới → Bộ đếm mã bệnh nhân reset về 0001 (GK-2027-0001)
2. Nếu hai nhân viên đăng ký cùng lúc → Hệ thống xử lý concurrency để đảm bảo mã bệnh nhân không trùng

**Trường hợp lỗi:**
1. Khi họ tên ngắn hơn 3 ký tự hoặc dài hơn 50 ký tự → Hệ thống hiển thị "Tối thiểu 3 ký tự" / "Tối đa 50 ký tự"
2. Khi số điện thoại không hợp lệ → Hệ thống hiển thị "Số điện thoại không hợp lệ (VD: 0901234567)"
3. Khi số điện thoại đã được đăng ký cho bệnh nhân khác → Hệ thống cảnh báo trùng số điện thoại

### Ghi chú kỹ thuật
- API endpoint: `POST /api/patients`
- Mã bệnh nhân sinh ở tầng ứng dụng (application-level): lấy MAX+1 theo năm, không dùng SQL Server sequence
- RowVersion concurrency token trên entity Patient cho optimistic concurrency
- DatePicker sử dụng date-fns format với Vietnamese dd/MM/yyyy locale
- Unique constraint filter trên PatientCode với HasFilter cho NULL exclusion
- Vietnamese_CI_AI collation cho tìm kiếm theo tên

---

## US-PAT-002: Đăng ký khách mua thuốc

**Là một** nhân viên phòng khám, **Tôi muốn** đăng ký nhanh khách mua thuốc chỉ với tên và số điện thoại, **Để** phục vụ khách hàng mua thuốc không cần tạo hồ sơ y tế đầy đủ.

**Yêu cầu liên quan:** PAT-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên truy cập trang Bệnh nhân → Nhấn "Đăng ký bệnh nhân"
2. Nhân viên chọn loại "Khách mua thuốc" → Form chỉ yêu cầu:
   - **Họ và tên** (bắt buộc)
   - **Số điện thoại** (bắt buộc)
3. Nhân viên điền thông tin → Nhấn "Lưu" → Hệ thống tạo hồ sơ khách hàng lightweight (không có mã bệnh nhân GK-YYYY-NNNN đầy đủ)

**Trường hợp ngoại lệ:**
1. Nếu muốn chuyển khách mua thuốc thành bệnh nhân khám bệnh → Nhân viên mở hồ sơ → Bổ sung thông tin (ngày sinh, giới tính) → Cập nhật loại bệnh nhân (PatientType)
2. Nếu khách mua thuốc quay lại khám bệnh → Tìm hồ sơ cũ bằng số điện thoại → Nâng cấp hồ sơ thay vì tạo mới

**Trường hợp lỗi:**
1. Khi số điện thoại đã tồn tại → Hệ thống gợi ý khách hàng đã đăng ký, cho phép chọn hồ sơ có sẵn

### Ghi chú kỹ thuật
- PatientType phân biệt: bệnh nhân khám bệnh (Medical) vs khách mua thuốc (WalkIn)
- Khách mua thuốc không yêu cầu ngày sinh, giới tính
- Cùng API endpoint `POST /api/patients` với trường `patientType` khác nhau
- Enum PatientType và Gender nằm trong Patient.Domain, được tham chiếu từ Patient.Contracts

---

## US-PAT-003: Trường thông tin bắt buộc có thể cấu hình

**Là một** nhân viên phòng khám, **Tôi muốn** hệ thống thông báo khi thiếu thông tin bắt buộc cho giấy chuyển viện hoặc báo cáo Sở Y tế, **Để** bổ sung kịp thời trước khi cần xuất dữ liệu.

**Yêu cầu liên quan:** PAT-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Khi bệnh nhân cần giấy chuyển viện hoặc xuất dữ liệu pháp lý → Hệ thống kiểm tra các trường cấu hình bắt buộc:
   - **Địa chỉ** (bắt buộc cho chuyển viện)
   - **CCCD - Căn cước công dân** (bắt buộc cho báo cáo pháp lý)
2. Nếu thiếu thông tin → Hệ thống hiển thị cảnh báo "Thiếu thông tin bắt buộc" với mô tả: "Các trường sau cần thiết cho giấy chuyển viện và xuất dữ liệu pháp lý"
3. Cảnh báo liệt kê cụ thể các trường còn thiếu (Địa chỉ, CCCD) → Có nút "Cập nhật hồ sơ" để bổ sung nhanh

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân không cần chuyển viện hay xuất dữ liệu → Các trường này không bắt buộc khi đăng ký thông thường
2. Nếu ngữ cảnh chuyển viện (referral context) → Áp dụng bộ validation nghiêm ngặt nhất

**Trường hợp lỗi:**
1. Khi cố xuất giấy chuyển viện mà thiếu trường bắt buộc → Hệ thống chặn xuất và yêu cầu bổ sung

### Ghi chú kỹ thuật
- PatientFieldValidationResult records nằm trong Domain.Services (không phải Contracts) do hướng phụ thuộc
- Referral context là downstream nghiêm ngặt nhất cho validation endpoint
- Validation API endpoint riêng cho kiểm tra trường bắt buộc theo ngữ cảnh
- UI sử dụng component cảnh báo chuyên biệt (fieldWarning) với title, description, danh sách trường và nút hành động

---

## US-PAT-004: Tìm kiếm bệnh nhân

**Là một** nhân viên phòng khám, **Tôi muốn** tìm kiếm bệnh nhân nhanh chóng bằng tên, số điện thoại hoặc mã bệnh nhân, **Để** truy cập hồ sơ bệnh nhân kịp thời phục vụ khám chữa bệnh.

**Yêu cầu liên quan:** PAT-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên nhấn tổ hợp phím **Ctrl+K** ở bất kỳ trang nào → Hệ thống mở GlobalSearch (Tìm kiếm)
2. Hệ thống hiển thị danh sách "Bệnh nhân gần đây" ngay khi focus vào ô tìm kiếm (trước khi nhập từ khóa)
3. Nhân viên nhập từ khóa tìm kiếm → Hệ thống tìm kiếm deferred (trì hoãn) theo:
   - **Họ và tên** (hỗ trợ tìm không dấu nhờ Vietnamese_CI_AI collation)
   - **Số điện thoại**
   - **Mã bệnh nhân** (GK-YYYY-NNNN)
4. Kết quả trả về trong **3 giây hoặc ít hơn** → Nhân viên chọn bệnh nhân → Chuyển đến hồ sơ bệnh nhân

**Trường hợp ngoại lệ:**
1. Nếu nhập "nguyen" → Hệ thống tìm được cả "Nguyễn" (accent-insensitive, case-insensitive)
2. Nếu danh sách kết quả dài → Hệ thống phân trang hoặc giới hạn hiển thị kết quả đầu tiên

**Trường hợp lỗi:**
1. Khi không tìm thấy kết quả → Hệ thống hiển thị "Không tìm thấy kết quả"
2. Khi API tìm kiếm bị lỗi → Hệ thống hiển thị trạng thái lỗi phù hợp

### Ghi chú kỹ thuật
- GlobalSearch sử dụng pattern Command + Popover với phím tắt Ctrl+K
- Deferred API search: không gọi API ngay khi gõ, chờ người dùng ngừng gõ (debounce)
- Vietnamese_CI_AI collation trên SQL Server cho tìm kiếm tên không phân biệt dấu và hoa/thường
- recentPatientsStore lưu danh sách bệnh nhân gần đây trên client (dùng cho breadcrumb patient name lookup)
- shouldFilter={false} trên Command để tránh xung đột giữa cmdk internal filtering và external filtering

---

## US-PAT-005: Quản lý danh sách dị ứng bệnh nhân

**Là một** nhân viên phòng khám, **Tôi muốn** ghi nhận và quản lý danh sách dị ứng có cấu trúc cho từng bệnh nhân, **Để** bác sĩ có thể xem cảnh báo dị ứng khi khám và kê đơn.

**Yêu cầu liên quan:** PAT-05

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở hồ sơ bệnh nhân → Truy cập phần "Dị ứng"
2. Nhân viên nhấn "Thêm dị ứng" → Hệ thống hiển thị combobox tìm kiếm dị ứng với:
   - Gợi ý từ danh mục có sẵn (autocomplete) theo 5 danh mục:
     - **Thuốc nhãn khoa** (ophthalmicDrug)
     - **Thuốc tổng hợp** (generalDrug)
     - **Vật liệu** (material)
     - **Môi trường** (environmental)
     - **Tùy chỉnh** (custom)
   - Hỗ trợ nhập tự do (free-text) nếu dị ứng không có trong danh mục
3. Nhân viên chọn hoặc nhập tên dị ứng → Chọn mức độ: **Nhẹ** / **Trung bình** / **Nặng**
4. Nhấn lưu → Dị ứng xuất hiện trong danh sách của bệnh nhân

**Hiển thị cảnh báo dị ứng:**
1. Khi bệnh nhân có dị ứng → Hệ thống hiển thị **AllergyAlert** trên hồ sơ bệnh nhân:
   - Chế độ **banner đầy đủ**: hiển thị "Cảnh báo dị ứng" với danh sách chi tiết
   - Chế độ **tooltip thu gọn**: hiển thị "N dị ứng" (ví dụ: "3 dị ứng") với tooltip chi tiết khi hover
2. Khi bệnh nhân không có dị ứng → Hiển thị "Không có dị ứng"

**Trường hợp ngoại lệ:**
1. Nếu nhập dị ứng bằng tiếng Anh → Autocomplete lưu English canonical key cho backend, nhưng hiển thị tiếng Việt qua categoryKeyMap và i18n
2. Nếu xóa dị ứng → Hệ thống hiển thị xác nhận "Bạn có chắc muốn xóa dị ứng này?"

**Trường hợp lỗi:**
1. Khi thêm dị ứng trùng → Hệ thống cảnh báo dị ứng đã tồn tại

### Ghi chú kỹ thuật
- Allergy combobox sử dụng Button trigger (không phải Input) với div wrapper để tránh click-to-toggle anti-pattern
- categoryKeyMap maps English category strings sang i18n keys cho runtime translation
- AllergyAlert component có 2 mode: full banner và compact tooltip -- tái sử dụng cho downstream prescribing
- AllergyCatalogSeeder là IHostedService với idempotent seeding (bỏ qua nếu data đã tồn tại)
- Autocomplete luôn lưu English canonical key cho backend consistency
- Mức độ dị ứng (AllergySeverity): Mild, Moderate, Severe -- enum trong Patient.Domain

> Xem thêm: US-CLN-xxx (Hiển thị cảnh báo dị ứng trong quy trình khám lâm sàng)

---

## US-PAT-006: Xem hồ sơ bệnh nhân

**Là một** nhân viên phòng khám, **Tôi muốn** xem đầy đủ hồ sơ bệnh nhân với các tab thông tin khác nhau, **Để** nắm bắt toàn diện tình trạng và lịch sử khám chữa bệnh.

**Yêu cầu liên quan:** PAT-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên chọn bệnh nhân từ danh sách hoặc kết quả tìm kiếm → Hệ thống hiển thị trang hồ sơ bệnh nhân
2. Trang hồ sơ gồm:
   - **Header**: Ảnh đại diện (có viền accent lớn), họ tên, mã bệnh nhân, thông tin cơ bản với icon, nhóm nút hành động
   - **Tab Tổng quan**: Thông tin cá nhân, thông tin hệ thống, danh sách dị ứng
   - **Tab Lịch hẹn**: Lịch hẹn sắp tới và đã qua
   - **Tab Lịch sử khám**: Danh sách các lượt khám trước đây
3. Nếu bệnh nhân có dị ứng → AllergyAlert hiển thị nổi bật trên header

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân chưa có lịch hẹn → Tab Lịch hẹn hiển thị "Chưa có lịch hẹn" với nút "Đặt lịch hẹn"
2. Nếu module Lịch hẹn chưa sẵn sàng → PatientAppointmentTab sử dụng retry:false để xử lý gracefully

**Trường hợp lỗi:**
1. Khi bệnh nhân không tồn tại (URL không hợp lệ) → Hệ thống hiển thị "Không tìm thấy bệnh nhân"
2. Khi không có quyền xem → Hệ thống phân biệt lỗi xác thực (auth error) và không tìm thấy (not-found)

### Ghi chú kỹ thuật
- PatientProfileHeader thiết kế với Card, avatar lớn có viền accent, metadata có icon, nhóm nút hành động
- recentPatientsStore dùng cho breadcrumb patient name lookup (không gọi API thêm)
- Auth error vs not-found differentiation pattern trong page error states
- API endpoint: `GET /api/patients/{id}`

---

## US-PAT-007: Chỉnh sửa thông tin bệnh nhân

**Là một** nhân viên phòng khám, **Tôi muốn** chỉnh sửa thông tin bệnh nhân với xử lý xung đột khi nhiều người cùng sửa, **Để** đảm bảo hồ sơ luôn chính xác và nhất quán.

**Yêu cầu liên quan:** PAT-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở hồ sơ bệnh nhân → Nhấn "Sửa" trên phần thông tin cần thay đổi
2. Hệ thống chuyển sang chế độ chỉnh sửa inline → Nhân viên cập nhật thông tin
3. Nhân viên nhấn "Lưu" → Hệ thống kiểm tra RowVersion → Cập nhật thành công
4. Nếu nhấn "Hủy" → Hệ thống hoàn tác thay đổi, quay về chế độ xem

**Trường hợp ngoại lệ:**
1. Nếu hai nhân viên cùng sửa hồ sơ → Người lưu sau nhận lỗi concurrency (RowVersion mismatch) → Hệ thống yêu cầu tải lại dữ liệu mới nhất trước khi sửa tiếp

**Trường hợp lỗi:**
1. Khi dữ liệu không hợp lệ → Hệ thống hiển thị ServerValidationAlert cho lỗi non-field, lỗi field-level hiển thị inline
2. Khi mất kết nối trong quá trình lưu → Hệ thống hiển thị thông báo lỗi qua toast

### Ghi chú kỹ thuật
- Optimistic concurrency qua RowVersion trên entity Patient
- DbUpdateConcurrencyException catch trong Application layer (Microsoft.EntityFrameworkCore reference)
- Inline edit với React Hook Form
- ServerValidationAlert component cho hiển thị non-field error nhất quán
- API functions throw JSON.stringify(err) khi có errors dict cho structured validation handling
- API endpoint: `PUT /api/patients/{id}`

---

## US-PAT-008: Tải ảnh đại diện bệnh nhân

**Là một** nhân viên phòng khám, **Tôi muốn** tải ảnh đại diện lên cho bệnh nhân, **Để** dễ dàng nhận diện bệnh nhân khi đến khám.

**Yêu cầu liên quan:** PAT-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở hồ sơ bệnh nhân → Nhấn vào vùng ảnh đại diện hoặc nút "Tải ảnh lên"
2. Hệ thống mở dialog chọn tệp → Nhân viên chọn ảnh từ máy tính
3. Hệ thống tải ảnh lên → Hiển thị ảnh mới trên hồ sơ bệnh nhân

**Trường hợp ngoại lệ:**
1. Nếu ảnh có kích thước quá lớn → Hệ thống thông báo giới hạn kích thước
2. Nếu định dạng không hỗ trợ → Hệ thống chỉ chấp nhận ảnh (JPEG, PNG)

**Trường hợp lỗi:**
1. Khi tải ảnh thất bại (mất kết nối) → Hệ thống hiển thị thông báo lỗi qua toast
2. Khi không có quyền chỉnh sửa → Hệ thống từ chối thao tác

### Ghi chú kỹ thuật
- Upload sử dụng native fetch với FormData (không dùng openapi-fetch) cho multipart file support
- Ảnh lưu trên Azure Blob Storage với soft delete và versioning
- StorageBlobInfo (đổi tên để tránh xung đột với Azure.Storage.Blobs.Models.BlobInfo)
- API endpoint: `POST /api/patients/{id}/photo`
- Tất cả React Query mutation có onError callback với toast.error cho phản hồi người dùng
