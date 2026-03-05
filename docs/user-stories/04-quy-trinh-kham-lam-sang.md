# Quy trình Khám Lâm sàng - User Stories

**Phạm vi:** Quy trình khám lâm sàng bao gồm: tạo và quản lý lượt khám (bệnh án), theo dõi trạng thái quy trình khám qua bảng Kanban, ghi nhận dữ liệu khúc xạ và đo lường nhãn khoa, chẩn đoán ICD-10 với hỗ trợ mắt phải/trái, ký duyệt hồ sơ và sửa đổi sau ký duyệt.
**Yêu cầu liên quan:** CLN-01, CLN-02, CLN-03, CLN-04, REF-01, REF-02, REF-03, DX-01, DX-02
**Số lượng user stories:** 10

---

## US-CLN-001: Bác sĩ tạo hồ sơ lượt khám điện tử

**Là một** bác sĩ, **Tôi muốn** tạo hồ sơ lượt khám (bệnh án) điện tử liên kết với bệnh nhân và bác sĩ phụ trách, và hồ sơ trở thành bất biến sau khi ký duyệt, **Để** đảm bảo tính toàn vẹn pháp lý của hồ sơ y tế.

**Yêu cầu liên quan:** CLN-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên tiếp nhận tạo lượt khám mới từ trang Khám bệnh → Hệ thống hiển thị form với: Chọn bệnh nhân, Chọn bác sĩ, tùy chọn Khám vãng lai
2. Hệ thống tạo lượt khám với trạng thái "Bản nháp" → Chuyển hướng đến trang chi tiết lượt khám
3. Trang chi tiết hiển thị các phần: Thông tin bệnh nhân, Khúc xạ, Ghi chú khám, Chẩn đoán → Mỗi phần là VisitSection (card thu gọn được)
4. Bác sĩ hoàn thành khám và nhấn "Ký duyệt" → Hệ thống hiển thị AlertDialog không thể đóng bằng click ngoài, yêu cầu xác nhận
5. Bác sĩ xác nhận ký duyệt → Hệ thống khóa hồ sơ, chuyển trạng thái thành "Đã ký", ghi nhận thời gian ký

**Trường hợp ngoại lệ:**
1. Nếu lượt khám liên kết với lịch hẹn đã có → Hệ thống hiển thị thông tin lịch hẹn liên kết
2. Nếu bệnh nhân có dị ứng → Hệ thống hiển thị cảnh báo dị ứng trên thẻ bệnh nhân (Xem thêm: US-PAT-005)

**Trường hợp lỗi:**
1. Khi cố chỉnh sửa hồ sơ đã ký duyệt → Hệ thống từ chối, yêu cầu tạo bản sửa đổi
2. Khi không chọn bệnh nhân hoặc bác sĩ → Hệ thống hiển thị lỗi validation

### Ghi chú kỹ thuật
- VisitSection là wrapper card thu gọn được (collapsible) với prop defaultOpen và slot headerExtra
- SignOff sử dụng AlertDialog (non-dismissible) cho xác nhận ký duyệt
- Trạng thái lượt khám: draft (Bản nháp) → signed (Đã ký) → amended (Đã sửa đổi)
- Hồ sơ sau ký duyệt trở thành bất biến (immutable after sign-off)
- API endpoint: POST /api/clinical/visits

---

## US-CLN-002: Sửa đổi hồ sơ đã ký duyệt (Amendment)

**Là một** bác sĩ, **Tôi muốn** tạo bản sửa đổi cho hồ sơ đã ký duyệt khi phát hiện sai sót, với lý do rõ ràng và bảo toàn bản gốc, **Để** đảm bảo tính minh bạch trong chỉnh sửa hồ sơ y tế.

**Yêu cầu liên quan:** CLN-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở hồ sơ đã ký duyệt và nhấn "Sửa đổi" → Hệ thống yêu cầu nhập lý do sửa đổi (tối thiểu 10 ký tự)
2. Bác sĩ nhập lý do và xác nhận → Hệ thống chụp snapshot trạng thái đã ký tại thời điểm bắt đầu sửa đổi
3. Hệ thống tạo bản ghi VisitAmendment với FieldChangesJson lưu danh sách thay đổi theo từng trường
4. Hồ sơ chuyển trạng thái thành "Đã sửa đổi" → Bản gốc được bảo toàn hoàn toàn

**Trường hợp ngoại lệ:**
1. Nếu có nhiều lần sửa đổi → Hệ thống hiển thị lịch sử tất cả bản sửa đổi theo thứ tự thời gian
2. Nếu hồ sơ chưa được ký duyệt → Nút "Sửa đổi" không hiển thị, bác sĩ chỉnh sửa trực tiếp

**Trường hợp lỗi:**
1. Khi lý do sửa đổi dưới 10 ký tự → Hệ thống hiển thị: "Lý do phải có ít nhất 10 ký tự"
2. Khi không nhập lý do → Hệ thống từ chối tạo bản sửa đổi

### Ghi chú kỹ thuật
- VisitAmendment.FieldChangesJson lưu mảng JSON các FieldChange record (field-level diff)
- Snapshot trạng thái đã ký được chụp tại thời điểm bắt đầu sửa đổi (initiation), không phải so sánh trước/sau (before/after diff)
- Visit.StartAmendment nhận tham số VisitAmendment (domain method)
- PropertyAccessMode.Field trên navigation property Amendments cho EF Core backing field access
- Lịch sử sửa đổi hiển thị: người sửa, thời gian, trường thay đổi, giá trị cũ, giá trị mới

---

## US-CLN-003: Theo dõi trạng thái quy trình khám qua bảng Kanban

**Là một** nhân viên phòng khám, **Tôi muốn** theo dõi trạng thái quy trình khám của tất cả bệnh nhân qua bảng Kanban với 5 cột trạng thái, **Để** nắm bắt tiến trình khám và điều phối nhân viên hiệu quả.

**Yêu cầu liên quan:** CLN-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở trang Khám bệnh → Hệ thống hiển thị bảng Kanban với 5 cột:
   - **Tiếp nhận** (Giai đoạn 0)
   - **Khám nghiệm** (Giai đoạn 1: Đo khúc xạ/TL)
   - **Bác sĩ** (Giai đoạn 2: Bác sĩ khám, 3: Chẩn đoán hình ảnh, 4: Bác sĩ đọc KQ)
   - **Xử lý** (Giai đoạn 5: Kê đơn, 6: Thu ngân)
   - **Hoàn tất** (Giai đoạn 7: Nhà thuốc/Kính)
2. Mỗi thẻ bệnh nhân hiển thị: tên bệnh nhân, thời gian chờ, cảnh báo dị ứng (nếu có) → Nhân viên nhanh chóng nhận biết thông tin quan trọng
3. Nhân viên kéo thẻ bệnh nhân sang cột tiếp theo → Hệ thống cập nhật giai đoạn quy trình

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân chờ trên 60 phút → Thẻ thời gian chờ hiển thị badge màu đỏ (destructive)
2. Nếu bệnh nhân chờ 30-60 phút → Thẻ hiển thị badge màu vàng (secondary)
3. Nếu bệnh nhân chờ dưới 30 phút → Thẻ hiển thị badge mặc định (outline)
4. Nếu không có bệnh nhân đang trong quy trình → Hiển thị: "Không có bệnh nhân đang trong quy trình"

**Trường hợp lỗi:**
1. Khi kéo thả bị lỗi kết nối → Hệ thống hoàn tác vị trí thẻ và hiển thị toast lỗi

### Ghi chú kỹ thuật
- Sử dụng @dnd-kit với PointerSensor (distance: 8) + TouchSensor (delay: 200) cho desktop và tablet
- 5 cột Kanban nhóm 8 giai đoạn: Tiếp nhận[0], Khám nghiệm[1], Bác sĩ[2,3,4], Xử lý[5,6], Hoàn tất[7]
- Wait time badge: destructive (>= 60 phút), secondary (>= 30 phút), outline (< 30 phút)
- Mỗi thẻ hiển thị cảnh báo dị ứng nếu bệnh nhân có dị ứng đã ghi nhận

---

## US-CLN-004: Dashboard hiển thị bệnh nhân đang khám theo giai đoạn

**Là một** nhân viên phòng khám, **Tôi muốn** xem dashboard tổng quan tất cả bệnh nhân đang trong quy trình khám và giai đoạn hiện tại, **Để** phối hợp công việc giữa các bộ phận.

**Yêu cầu liên quan:** CLN-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở trang Khám bệnh → Hệ thống hiển thị bảng Kanban (tích hợp với US-CLN-003) với tổng số bệnh nhân đang trong quy trình
2. Mỗi thẻ bệnh nhân hiển thị: tên, giai đoạn hiện tại, thời gian chờ → Nhân viên nắm bắt tình hình tổng quan
3. Nhân viên nhấn "Xem chi tiết" trên thẻ → Hệ thống chuyển đến trang chi tiết lượt khám
4. Nhân viên kéo thẻ bệnh nhân sang cột khác → Hệ thống cập nhật giai đoạn với nút "Chuyển tiếp"

**Trường hợp ngoại lệ:**
1. Nếu có nhiều bệnh nhân cùng giai đoạn → Thẻ xếp chồng trong cột, cuộn để xem
2. Nếu bệnh nhân có cảnh báo dị ứng → Hiển thị icon cảnh báo: "Bệnh nhân có dị ứng"

**Trường hợp lỗi:**
1. Khi mất kết nối API → Hệ thống hiển thị thông báo lỗi và dữ liệu cuối cùng đã tải

### Ghi chú kỹ thuật
- Dashboard tích hợp với bảng Kanban (US-CLN-003), cùng một giao diện
- Hiển thị tổng số bệnh nhân đang trong quy trình
- API endpoint: GET /api/clinical/visits?active=true

---

## US-CLN-005: Ghi nhận dữ liệu khúc xạ và đo lường nhãn khoa

**Là một** kỹ thuật viên hoặc bác sĩ, **Tôi muốn** ghi nhận dữ liệu khúc xạ (SPH, CYL, AXIS, ADD, PD), thị lực (VA), nhãn áp (IOP) và trục nhãn cầu cho từng mắt, **Để** lưu trữ đầy đủ kết quả đo lường phục vụ chẩn đoán và theo dõi.

**Yêu cầu liên quan:** REF-01, REF-02, REF-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Kỹ thuật viên mở phần "Khúc xạ" trong lượt khám → Hệ thống hiển thị RefractionForm với hai cột: MP (Mắt phải/OD) và MT (Mắt trái/OS)
2. Kỹ thuật viên chọn loại khúc xạ: Thường quy (manifest) / Máy đo tự động (autorefraction) / Liệt điều tiết (cycloplegic) → Hệ thống hiển thị form tương ứng
3. Kỹ thuật viên nhập SPH, CYL, TRỤC, ADD, PD cho mỗi mắt → Hệ thống tự động lưu khi rời trường (auto-save on blur, debounce 500ms)
4. Kỹ thuật viên nhập thị lực: TL không kính (UCVA) và TL chỉnh kính (BCVA) cho mỗi mắt → Hệ thống lưu tự động
5. Kỹ thuật viên nhập nhãn áp (IOP): chọn phương pháp đo (Goldmann / Không tiếp xúc / iCare / Tonopen / Khác), nhập giá trị và thời gian đo → Hệ thống lưu tự động
6. Kỹ thuật viên nhập trục nhãn cầu (Axial Length) cho mỗi mắt → Hệ thống lưu tự động
7. Hệ thống hiển thị thông báo "Đã lưu khúc xạ" sau mỗi lần lưu thành công

**Trường hợp ngoại lệ:**
1. Nếu cần ghi nhận nhiều loại khúc xạ (manifest + cycloplegic) → Mỗi loại lưu thành bản ghi khúc xạ riêng biệt
2. Nếu không cần nhập IOP → Phương pháp IOP Select hiển thị trạng thái rỗng (empty string cho no-selection state)
3. Nếu không có giá trị cho một số trường → Cho phép để trống (trường không bắt buộc)

**Trường hợp lỗi:**
1. Khi lưu thất bại → Hệ thống hiển thị toast lỗi: "Lưu dữ liệu khúc xạ thất bại"
2. Khi nhập giá trị ngoài phạm vi hợp lệ → Hệ thống validation từ chối

### Ghi chú kỹ thuật
- RefractionForm sử dụng debounced auto-save on blur (500ms) thay vì nút lưu riêng
- Các trường decimal sử dụng precision(5,2) cho mọi giá trị diopter/VA/IOP/axial length
- RefractionDto sử dụng 'type' (read path) trong khi updateRefraction gửi 'refractionType' (write path) - asymmetric DTO naming
- IOP method Select sử dụng empty string (không phải undefined) cho no-selection state
- PropertyAccessMode.Field trên navigation property Refractions cho EF Core backing field access
- Ba loại khúc xạ: manifest, autorefraction, cycloplegic - mỗi loại là bản ghi riêng
- Tất cả React Query mutations phải có onError callback với toast.error

---

## US-CLN-006: Ghi nhận thị lực, nhãn áp và trục nhãn cầu

**Là một** kỹ thuật viên hoặc bác sĩ, **Tôi muốn** ghi nhận chi tiết thị lực (có/không chỉnh kính), nhãn áp (với phương pháp đo và thời gian) và trục nhãn cầu cho từng mắt, **Để** có dữ liệu đầy đủ phục vụ chẩn đoán và theo dõi tiến triển.

**Yêu cầu liên quan:** REF-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Kỹ thuật viên mở phần Khúc xạ → Hệ thống hiển thị các trường VA, IOP, Axial Length bên cạnh dữ liệu khúc xạ
2. Kỹ thuật viên nhập TL không kính (UCVA) cho MP và MT → Hệ thống lưu tự động
3. Kỹ thuật viên nhập TL chỉnh kính (BCVA) cho MP và MT → Hệ thống lưu tự động
4. Kỹ thuật viên chọn phương pháp đo nhãn áp từ Select → Hệ thống hiển thị danh sách: Goldmann, Không tiếp xúc, iCare, Tonopen, Khác
5. Kỹ thuật viên nhập giá trị nhãn áp và thời gian đo → Hệ thống lưu tự động
6. Kỹ thuật viên nhập trục nhãn cầu cho mỗi mắt → Hệ thống lưu tự động

**Trường hợp ngoại lệ:**
1. Nếu chưa chọn phương pháp đo IOP → Select hiển thị trạng thái rỗng, cho phép bỏ qua
2. Nếu chỉ đo một mắt → Cho phép để trống mắt còn lại

**Trường hợp lỗi:**
1. Khi nhập giá trị IOP ngoài phạm vi → Hệ thống validation cảnh báo

### Ghi chú kỹ thuật
- IOP method lưu dưới dạng enum trong database
- IOP Select sử dụng empty string cho no-selection state (Select luôn ở trạng thái controlled)
- VA, IOP, Axial Length đều sử dụng precision(5,2)
- Dữ liệu ghi nhận riêng cho mỗi mắt (OD/OS)

---

## US-CLN-007: Tìm kiếm và chọn mã ICD-10 cho chẩn đoán

**Là một** bác sĩ, **Tôi muốn** tìm kiếm mã ICD-10 bằng tiếng Việt hoặc tiếng Anh với danh sách mã yêu thích ghim lên đầu, **Để** nhanh chóng ghi nhận chẩn đoán chính xác cho bệnh nhân.

**Yêu cầu liên quan:** DX-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ nhấn "Thêm chẩn đoán" trong phần Chẩn đoán của lượt khám → Hệ thống hiển thị Icd10Combobox
2. Bác sĩ nhập từ khóa tìm kiếm (tiếng Việt hoặc tiếng Anh) → Hệ thống tìm kiếm song ngữ với Contains, mã yêu thích được ghim lên đầu kết quả (OrderByDescending)
3. Bác sĩ chọn mã ICD-10 từ kết quả → Hệ thống thêm chẩn đoán vào lượt khám
4. Hệ thống hiển thị thông báo "Đã thêm chẩn đoán"
5. Bác sĩ có thể đánh dấu mã là "Chính" hoặc "Phụ" → Hệ thống lưu phân loại chẩn đoán

**Trường hợp ngoại lệ:**
1. Nếu bác sĩ đã ghim mã ICD-10 yêu thích → Mã yêu thích luôn hiển thị đầu tiên trong kết quả
2. Nếu tìm kiếm không có kết quả → Hiển thị thông báo không tìm thấy
3. Nếu chưa có chẩn đoán nào → Hiển thị: "Chưa có chẩn đoán"

**Trường hợp lỗi:**
1. Khi thêm chẩn đoán thất bại → Hệ thống hiển thị toast: "Thêm chẩn đoán thất bại"
2. Khi xóa chẩn đoán thất bại → Hệ thống hiển thị toast: "Xóa chẩn đoán thất bại"

### Ghi chú kỹ thuật
- Icd10Combobox sử dụng Button trigger (không phải Input) với div wrapper để tránh click-to-toggle anti-pattern
- SearchIcd10Codes sử dụng Contains cho tìm kiếm song ngữ, favorites ghim qua OrderByDescending
- DoctorIcd10Favorite là junction table per-doctor (không phải global IsFavorite field trên Icd10Code)
- PropertyAccessMode.Field trên navigation property Diagnoses cho EF Core backing field access
- API endpoint: GET /api/clinical/icd10/search?query=...

---

## US-CLN-008: Hệ thống bắt buộc chọn mắt (laterality) cho mã ICD-10 nhãn khoa

**Là một** bác sĩ, **Tôi muốn** hệ thống bắt buộc chọn mắt (phải/trái/hai mắt) khi nhập mã ICD-10 nhãn khoa, **Để** đảm bảo chẩn đoán luôn ghi rõ vị trí mắt theo chuẩn ICD-10.

**Yêu cầu liên quan:** DX-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ chọn mã ICD-10 yêu cầu laterality → Hệ thống hiển thị inline laterality selector trong Icd10Combobox
2. Bác sĩ chọn mắt: MP (Phải) / MT (Trái) / 2M (Hai mắt) → Hệ thống lưu chẩn đoán với laterality tương ứng
3. Nếu chọn "2M (Hai mắt)" → Hệ thống tạo 2 bản ghi chẩn đoán riêng biệt với hậu tố mã .1 (mắt phải) và .2 (mắt trái) theo chuẩn ICD-10

**Trường hợp ngoại lệ:**
1. Nếu mã ICD-10 không yêu cầu laterality → Hệ thống mặc định laterality 0 (OD), lưu nhưng không hiển thị lựa chọn mắt cho người dùng
2. Nếu bác sĩ chọn cùng mã ICD-10 cho cả hai mắt riêng lẻ → Hệ thống cho phép 2 bản ghi riêng biệt

**Trường hợp lỗi:**
1. Khi không chọn mắt cho mã yêu cầu laterality → Hệ thống từ chối lưu chẩn đoán

### Ghi chú kỹ thuật
- OU (hai mắt) tạo 2 bản ghi chẩn đoán với hậu tố .1 (OD) và .2 (OS) theo quy ước ICD-10
- Non-laterality codes mặc định laterality 0 (OD) - lưu nhưng không có ý nghĩa lâm sàng khi requiresLaterality=false
- Inline laterality selector tích hợp trong Icd10Combobox
- Laterality options: OD (MP - Phải), OS (MT - Trái), OU (2M - Hai mắt)

---

## US-CLN-009: Hiển thị cảnh báo dị ứng trong quy trình khám

**Là một** bác sĩ, **Tôi muốn** nhìn thấy cảnh báo dị ứng của bệnh nhân khi khám, **Để** tránh kê đơn thuốc hoặc thực hiện thủ thuật gây dị ứng.

> Xem thêm: US-PAT-005 (Quản lý danh sách dị ứng bệnh nhân)

**Yêu cầu liên quan:** PAT-05 (khía cạnh lâm sàng)

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở lượt khám của bệnh nhân có dị ứng đã ghi nhận → Hệ thống hiển thị cảnh báo dị ứng trong phần thông tin bệnh nhân
2. Trên bảng Kanban, thẻ bệnh nhân có dị ứng hiển thị icon cảnh báo: "Bệnh nhân có dị ứng"
3. Bác sĩ nhấn vào cảnh báo → Hệ thống hiển thị chi tiết danh sách dị ứng

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân không có dị ứng → Không hiển thị cảnh báo
2. Nếu dị ứng mức độ nghiêm trọng → Cảnh báo nổi bật hơn

**Trường hợp lỗi:**
1. Khi không thể tải thông tin dị ứng → Hệ thống vẫn cho phép khám nhưng hiển thị cảnh báo không tải được

### Ghi chú kỹ thuật
- AllergyAlert component có 2 mode: full banner + compact tooltip cho tái sử dụng downstream (prescribing)
- Dữ liệu dị ứng lấy từ module Patient, hiển thị trong context Clinical

---

## US-CLN-010: Bác sĩ xem tóm tắt lượt khám trước khi ký duyệt

**Là một** bác sĩ, **Tôi muốn** xem tóm tắt toàn bộ lượt khám (khúc xạ, chẩn đoán, ghi chú) trước khi ký duyệt, **Để** kiểm tra tổng thể thông tin trước khi khóa hồ sơ.

**Yêu cầu liên quan:** CLN-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở trang chi tiết lượt khám → Hệ thống hiển thị tất cả phần: Thông tin bệnh nhân, Khúc xạ, Ghi chú khám, Chẩn đoán trong các VisitSection
2. Mỗi VisitSection có thể thu gọn/mở rộng → Bác sĩ xem nhanh hoặc chi tiết từng phần
3. Bác sĩ xem xong và nhấn "Ký duyệt lượt khám" → Hệ thống hiển thị cảnh báo: "Thao tác này sẽ khóa hồ sơ. Chỉnh sửa sau đó sẽ cần tạo bản sửa đổi."
4. Bác sĩ xác nhận → Hồ sơ được ký duyệt

**Trường hợp ngoại lệ:**
1. Nếu chưa nhập chẩn đoán → Hệ thống cho phép ký duyệt nhưng hiển thị cảnh báo
2. Nếu ghi chú khám được auto-save → Hiển thị thông báo "Đã lưu ghi chú"

**Trường hợp lỗi:**
1. Khi ký duyệt thất bại → Hệ thống hiển thị toast lỗi, hồ sơ giữ nguyên trạng thái "Bản nháp"

### Ghi chú kỹ thuật
- VisitSection sử dụng collapsible card với defaultOpen prop (phần quan trọng mở mặc định)
- headerExtra slot cho phép đặt nội dung bổ sung trên tiêu đề phần
- Ghi chú khám auto-save tương tự RefractionForm
- Thông báo ký duyệt sử dụng AlertDialog non-dismissible
