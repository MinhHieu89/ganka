# Giao diện và Hệ thống - User Stories

**Phạm vi:** Giao diện đa ngôn ngữ (Việt/Anh), chuyển đổi ngôn ngữ theo người dùng, và hạ tầng hệ thống bao gồm kiến trúc mô-đun, hỗ trợ đa chi nhánh, mẫu bệnh mở rộng, sao lưu, lưu trữ hình ảnh, và xuất dữ liệu
**Yêu cầu liên quan:** UI-01, UI-02, ARC-01, ARC-02, ARC-03, ARC-04, ARC-05, ARC-06
**Số lượng user stories:** 8

---

## US-SYS-001: Giao diện song ngữ Việt - Anh

**Là một** nhân viên phòng khám, **Tôi muốn** tất cả văn bản, nhãn, menu, và báo cáo trên giao diện đều có sẵn bằng tiếng Việt và tiếng Anh (tiếng Việt là ngôn ngữ chính), **Để** sử dụng hệ thống thoải mái bằng ngôn ngữ quen thuộc.

**Yêu cầu liên quan:** UI-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên truy cập hệ thống lần đầu → Giao diện hiển thị mặc định bằng tiếng Việt với đầy đủ dấu thanh điệu
2. Tất cả menu sidebar (Trang chủ, Bệnh nhân, Lịch hẹn, Khám bệnh, Quản trị, v.v.) → Hiển thị bằng tiếng Việt
3. Tất cả nhãn form, nút bấm, thông báo lỗi, thông báo thành công → Hiển thị bằng ngôn ngữ đang chọn
4. Bảng dữ liệu (tiêu đề cột, phân trang, "Không có dữ liệu") → Hiển thị bằng ngôn ngữ đang chọn

**Trường hợp ngoại lệ:**
1. Nếu một chuỗi dịch bị thiếu → Hệ thống hiển thị khóa dịch (translation key) thay vì để trống
2. Thuật ngữ kỹ thuật y khoa (ICD-10, SPH, CYL, AXIS, ADD, PD, VA, IOP) → Giữ nguyên bằng tiếng Anh không dịch

**Trường hợp lỗi:**
1. Khi file dịch không tải được → Hệ thống sử dụng ngôn ngữ mặc định (tiếng Việt) và ghi log lỗi

### Ghi chú kỹ thuật
- Sử dụng thư viện i18next với cấu trúc file: `frontend/public/locales/vi/*.json` và `frontend/public/locales/en/*.json`
- Tiếng Việt là ngôn ngữ mặc định (fallbackLng: 'vi')
- Các file dịch bao gồm: common.json, auth.json, patient.json, scheduling.json, clinical.json, audit.json
- Tất cả văn bản tiếng Việt sử dụng dấu thanh điệu đúng (theo quyết định Phase 01.2-07)

---

## US-SYS-002: Chuyển đổi ngôn ngữ theo người dùng

**Là một** nhân viên phòng khám, **Tôi muốn** chuyển đổi ngôn ngữ giao diện giữa tiếng Việt và tiếng Anh, và lựa chọn được lưu theo tài khoản của tôi, **Để** mỗi lần đăng nhập hệ thống tự động hiển thị ngôn ngữ ưa thích.

**Yêu cầu liên quan:** UI-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên nhấn vào nút chuyển ngôn ngữ (LanguageToggle) trên thanh header → Giao diện chuyển sang ngôn ngữ còn lại ngay lập tức
2. Hệ thống gửi yêu cầu cập nhật ngôn ngữ cho tài khoản → API lưu lựa chọn ngôn ngữ vào cơ sở dữ liệu
3. Nhân viên đăng xuất rồi đăng nhập lại → Giao diện hiển thị ngôn ngữ đã chọn trước đó

**Trường hợp ngoại lệ:**
1. Nếu API cập nhật ngôn ngữ thất bại → Giao diện vẫn chuyển ngôn ngữ cục bộ nhưng lần đăng nhập sau sẽ trở về ngôn ngữ cũ
2. Nếu người dùng mới chưa chọn ngôn ngữ → Mặc định tiếng Việt

**Trường hợp lỗi:**
1. Khi mất kết nối mạng → Chuyển ngôn ngữ cục bộ vẫn hoạt động, đồng bộ lại khi có mạng

### Ghi chú kỹ thuật
- Component LanguageToggle hiển thị trên SiteHeader (thanh trên cùng)
- Lựa chọn ngôn ngữ được lưu trữ theo người dùng (per user), không theo trình duyệt
- API endpoint: PUT /api/auth/language với body { language: "vi" | "en" }
- Khi đăng nhập, hệ thống đọc ngôn ngữ từ profile người dùng và áp dụng cho i18next

---

## US-SYS-003: Kiến trúc mô-đun cho tích hợp dịch vụ bên ngoài

**Là một** quản trị viên, **Tôi muốn** hệ thống sử dụng kiến trúc mô-đun để tích hợp với các dịch vụ bên ngoài, **Để** có thể thay đổi nhà cung cấp mà không ảnh hưởng đến nghiệp vụ.

**Yêu cầu liên quan:** ARC-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Hệ thống sử dụng dịch vụ lưu trữ file (Azure Blob Storage) → Nghiệp vụ gọi qua interface (port) trong tầng Domain, triển khai cụ thể (adapter) trong tầng Infrastructure
2. Khi cần thay đổi từ Azure sang AWS S3 → Chỉ cần tạo adapter mới trong Infrastructure, không sửa code nghiệp vụ
3. Tất cả tích hợp bên ngoài (email, SMS, lưu trữ) → Đều đi qua domain port + infrastructure adapter

**Trường hợp ngoại lệ:**
1. Nếu dịch vụ bên ngoài tạm ngưng → Hệ thống xử lý lỗi tại tầng adapter, trả về Result lỗi cho nghiệp vụ

### Ghi chú kỹ thuật
- Pattern Anti-Corruption Layer (ACL): domain định nghĩa interface (port), infrastructure triển khai adapter
- Mỗi module có cấu trúc: Domain (entities, ports) → Application (use cases) → Infrastructure (adapters) → Presentation (API endpoints)
- Ví dụ: IFileStorageService (port trong Domain) → AzureBlobStorageAdapter (adapter trong Infrastructure)
- StorageBlobInfo đổi tên để tránh xung đột với Azure.Storage.Blobs.Models.BlobInfo
- Architecture tests (NetArchTest) kiểm tra tầng Domain không tham chiếu Infrastructure

---

## US-SYS-004: Hỗ trợ đa chi nhánh từ kiến trúc

**Là một** quản trị viên, **Tôi muốn** hệ thống hỗ trợ đa chi nhánh từ kiến trúc, **Để** có thể mở rộng khi phòng khám phát triển.

**Yêu cầu liên quan:** ARC-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Tất cả aggregate root (Patient, Visit, Appointment, v.v.) → Đều có trường BranchId
2. Truy vấn dữ liệu → EF Core global query filter tự động lọc theo BranchId hiện tại
3. Tạo dữ liệu mới → BranchId tự động gán từ context người dùng đang đăng nhập

**Trường hợp ngoại lệ:**
1. Phiên bản v1 chỉ có một chi nhánh → BranchId mặc định, không cần giao diện quản lý chi nhánh
2. Nếu cần mở rộng sau này → Thêm giao diện chọn chi nhánh và quản lý chi nhánh mà không thay đổi data model

### Ghi chú kỹ thuật
- BranchId (Guid) trên tất cả aggregate root entities
- EF Core global query filters: `.HasQueryFilter(e => e.BranchId == currentBranchId)`
- Hiện tại v1 hoạt động đơn chi nhánh, kiến trúc sẵn sàng cho mở rộng đa chi nhánh
- Không cần UI quản lý chi nhánh trong v1

---

## US-SYS-005: Mở rộng mẫu bệnh không cần thay đổi mã nguồn

**Là một** quản trị viên, **Tôi muốn** hệ thống hỗ trợ thêm mẫu bệnh mới (disease templates) mà không cần thay đổi mã nguồn, **Để** có thể mở rộng cho các bệnh khác ngoài Dry Eye.

**Yêu cầu liên quan:** ARC-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Hiện tại hệ thống hỗ trợ quy trình khám tổng quát (refraction, diagnosis, sign-off) → Áp dụng được cho mọi bệnh nhãn khoa
2. Mẫu Dry Eye (OSDI, TBUT, Schirmer, v.v.) → Được thiết kế như template có thể cấu hình, không hard-code vào quy trình khám chính
3. Khi cần thêm mẫu Myopia Control hoặc Glaucoma → Thêm template mới qua cấu hình, tái sử dụng quy trình khám chính

**Trường hợp ngoại lệ:**
1. Nếu mẫu bệnh mới yêu cầu trường dữ liệu chưa có → Cần thêm migration cho trường mới nhưng không thay đổi quy trình core

### Ghi chú kỹ thuật
- Kiến trúc template engine cho phép thêm disease template qua config/plugin
- Quy trình khám chính (Visit → Refraction → Diagnosis → Sign-off) là framework chung
- Template bệnh cụ thể (Dry Eye, Myopia Control) là plugin mở rộng
- Axial Length đã được ghi nhận trong refraction từ v1 để sẵn sàng cho Myopia Control template

---

## US-SYS-006: Sao lưu dữ liệu tự động hàng ngày

**Là một** quản trị viên, **Tôi muốn** hệ thống tự động sao lưu dữ liệu hàng ngày, **Để** đảm bảo không mất dữ liệu khi có sự cố.

**Yêu cầu liên quan:** ARC-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Hàng ngày → Azure SQL tự động tạo bản sao lưu đầy đủ
2. Khi có sự cố mất dữ liệu → Quản trị viên có thể khôi phục dữ liệu đến bất kỳ thời điểm nào trong 35 ngày gần nhất (point-in-time recovery)
3. Quá trình sao lưu → Diễn ra tự động, không ảnh hưởng đến hiệu suất hệ thống trong giờ làm việc

**Trường hợp ngoại lệ:**
1. Nếu cần khôi phục dữ liệu cũ hơn 35 ngày → Cần liên hệ đội kỹ thuật để kiểm tra bản sao lưu dài hạn

**Trường hợp lỗi:**
1. Khi sao lưu tự động thất bại → Azure gửi cảnh báo qua email/monitoring

### Ghi chú kỹ thuật
- Azure SQL Database tự động sao lưu với chu kỳ hàng ngày
- Point-in-time recovery cho phép khôi phục đến bất kỳ thời điểm nào trong 35 ngày
- Không cần cấu hình thủ công, tính năng sao lưu có sẵn trên Azure SQL
- Bản sao lưu được lưu trữ tại vùng Azure tương ứng với database

---

## US-SYS-007: Lưu trữ hình ảnh y khoa an toàn

**Là một** quản trị viên, **Tôi muốn** hệ thống lưu trữ hình ảnh y khoa an toàn với khả năng khôi phục, **Để** bảo vệ dữ liệu hình ảnh của bệnh nhân.

**Yêu cầu liên quan:** ARC-05

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên tải lên hình ảnh y khoa (ảnh chụp mắt, kết quả chẩn đoán hình ảnh) → Hệ thống lưu trữ trên Azure Blob Storage
2. Nếu hình ảnh bị xóa nhầm → Có thể khôi phục nhờ tính năng soft delete
3. Nếu cần truy cập phiên bản cũ của hình ảnh → Hệ thống hỗ trợ versioning cho blob

**Trường hợp ngoại lệ:**
1. Nếu dung lượng lưu trữ đạt ngưỡng cảnh báo → Hệ thống thông báo quản trị viên
2. Nếu hình ảnh có kích thước quá lớn → Hệ thống giới hạn và thông báo lỗi

**Trường hợp lỗi:**
1. Khi Azure Blob Storage không khả dụng → Hệ thống hiển thị lỗi tải lên và cho phép thử lại
2. Khi tải lên bị gián đoạn → File không hoàn chỉnh không được lưu

### Ghi chú kỹ thuật
- Azure Blob Storage với soft delete (cho phép khôi phục blob đã xóa)
- Blob versioning để lưu giữ phiên bản trước của file
- StorageBlobInfo (đổi tên từ BlobInfo để tránh xung đột với Azure.Storage.Blobs.Models.BlobInfo)
- Pattern ACL: IFileStorageService trong Domain, AzureBlobStorageAdapter trong Infrastructure
- Hỗ trợ upload qua multipart/form-data

---

## US-SYS-008: Xuất toàn bộ dữ liệu hệ thống

**Là một** quản trị viên, **Tôi muốn** có khả năng xuất toàn bộ dữ liệu của hệ thống, **Để** đảm bảo quyền sở hữu dữ liệu và không bị ràng buộc với nhà cung cấp.

**Yêu cầu liên quan:** ARC-06

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản trị viên yêu cầu xuất dữ liệu → Hệ thống cung cấp khả năng xuất toàn bộ dữ liệu bao gồm: bệnh nhân, lượt khám, chẩn đoán, hồ sơ kiểm toán
2. Dữ liệu xuất → Ở định dạng chuẩn (CSV/JSON) có thể nhập vào hệ thống khác
3. Hình ảnh y khoa → Có thể tải xuống toàn bộ từ Azure Blob Storage

**Trường hợp ngoại lệ:**
1. Nếu dữ liệu quá lớn → Hệ thống hỗ trợ xuất theo từng module hoặc khoảng thời gian
2. Nếu cần xuất cho mục đích audit → Bao gồm cả nhật ký kiểm toán trong file xuất

**Trường hợp lỗi:**
1. Khi quá trình xuất bị gián đoạn → Hệ thống cho phép xuất lại mà không ảnh hưởng đến dữ liệu gốc

### Ghi chú kỹ thuật
- Đảm bảo không có vendor lock-in: dữ liệu thuộc sở hữu của Ganka28
- Hỗ trợ xuất từ tất cả các schema: auth, audit, patient, scheduling, clinical, reference
- API endpoint xuất nhật ký kiểm toán: GET /api/audit/logs/export (CSV)
- Kiến trúc modular giúp dễ dàng thêm endpoint xuất cho từng module
- Dữ liệu hình ảnh trên Azure Blob Storage có thể tải xuống trực tiếp hoặc qua Azure Storage Explorer

---

*Tài liệu này mô tả các user stories cho chức năng Giao diện và Hệ thống đã được triển khai trong Phase 1.*
*Cập nhật: 2026-03-05*
