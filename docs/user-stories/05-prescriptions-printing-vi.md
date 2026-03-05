# Đơn thuốc và In tài liệu (Prescriptions & Document Printing) - User Stories

**Phạm vi:** Kê đơn thuốc (từ danh mục nhà thuốc và ngoài danh mục), kê đơn kính (optical Rx), cảnh báo dị ứng thuốc, in đơn thuốc, in đơn kính, in giấy chuyển viện, in giấy đồng ý, in nhãn nhà thuốc, và quản lý danh mục thuốc.
**Yêu cầu liên quan:** RX-01, RX-02, RX-03, RX-04, RX-05, PRT-01, PRT-02, PRT-04, PRT-05, PRT-06
**Số lượng user stories:** 15 (+ 1 hoãn lại)

---

## US-RX-001: Bác sĩ kê đơn thuốc từ danh mục nhà thuốc

**Là một** bác sĩ, **Tôi muốn** kê đơn thuốc bằng cách chọn thuốc từ danh mục nhà thuốc của phòng khám, **Để** tạo đơn thuốc chính xác với thông tin thuốc đầy đủ và liên kết được với tồn kho.

**Yêu cầu liên quan:** RX-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở lượt khám và chọn phần "Đơn thuốc" (DrugPrescriptionSection) → Hệ thống hiển thị danh sách thuốc đã kê (nếu có) và nút "Thêm thuốc"
2. Bác sĩ nhấn "Thêm thuốc" → Hệ thống hiển thị dialog với ô tìm kiếm thuốc từ danh mục (DrugCombobox)
3. Bác sĩ nhập tên thuốc → Hệ thống hiển thị danh sách kết quả từ danh mục với tên thuốc, hàm lượng, dạng bào chế (viên/nhỏ mắt/kem/tiêm)
4. Bác sĩ chọn thuốc → Hệ thống tự động điền thông tin: tên thuốc, hàm lượng, dạng bào chế, đường dùng, đơn vị tính, và mẫu liều lượng mặc định
5. Bác sĩ nhập liều lượng chi tiết: liều dùng (dose), tần suất (frequency), thời gian (duration), đường dùng (route) → Hệ thống tự động tạo chỉ dẫn sử dụng (instruction text)
6. Bác sĩ nhấn "Thêm" → Thuốc được thêm vào danh sách đơn thuốc với viền đường gạch đứt (dashed border) biểu thị chưa lưu
7. Bác sĩ nhấn "Lưu đơn thuốc" → Hệ thống lưu toàn bộ đơn thuốc và hiển thị thông báo "Lưu đơn thuốc thành công"

**Trường hợp ngoại lệ:**
1. Nếu bác sĩ muốn sửa thuốc đã thêm → Nhấn biểu tượng chỉnh sửa trên dòng thuốc → Hệ thống hiển thị dialog chỉnh sửa
2. Nếu bác sĩ muốn xóa thuốc → Nhấn biểu tượng xóa trên dòng thuốc → Hệ thống xóa thuốc khỏi danh sách
3. Nếu không tìm thấy thuốc trong danh mục → Bác sĩ có thể thêm thuốc ngoài danh mục (Xem thêm: US-RX-002)

**Trường hợp lỗi:**
1. Khi lưu đơn thuốc thất bại → Hệ thống hiển thị toast lỗi: "Lưu đơn thuốc thất bại"
2. Khi không tải được danh mục thuốc → Hệ thống hiển thị thông báo lỗi trong ô tìm kiếm
3. Khi token hết hạn trong lúc lưu → Hệ thống tự động refresh token và thử lại

### Ghi chú kỹ thuật
- DrugPrescriptionSection sử dụng VisitSection wrapper với headerExtra slot
- DrugCombobox sử dụng Popover + Command search pattern (tương tự Icd10Combobox)
- Thuốc chưa lưu hiển thị với dashed border để phân biệt với thuốc đã lưu trên server
- Form/Route lưu dưới dạng int (không phải enum type) để tránh cross-module dependency
- API endpoint: POST /api/clinical/visits/{visitId}/prescriptions
- Tất cả React Query mutations có onError callback với toast.error

---

## US-RX-002: Bác sĩ thêm thuốc ngoài danh mục vào đơn

**Là một** bác sĩ, **Tôi muốn** thêm thuốc ngoài danh mục bằng cách nhập thông tin thủ công, **Để** kê đơn những thuốc đặc biệt hoặc chưa có trong danh mục phòng khám.

**Yêu cầu liên quan:** RX-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ trong dialog thêm thuốc, chọn tab "Ngoài danh mục" (off-catalog) → Hệ thống hiển thị form nhập thủ công
2. Bác sĩ nhập tên thuốc, hàm lượng, dạng bào chế, đường dùng, liều dùng, tần suất, thời gian → Hệ thống cho phép nhập tự do (free-text)
3. Bác sĩ nhấn "Thêm" → Thuốc được thêm vào danh sách đơn với nhãn "Ngoài danh mục" (off-catalog badge)
4. Hệ thống đánh dấu thuốc ngoài danh mục là "thủ công" (manual) → Không liên kết với tồn kho

**Trường hợp ngoại lệ:**
1. Nếu bác sĩ nhập tên thuốc giống thuốc trong danh mục → Hệ thống không cần cảnh báo (bác sĩ có quyền chọn ngoài danh mục)
2. Nếu bác sĩ để trống trường bắt buộc (tên thuốc, liều dùng) → Hệ thống hiển thị lỗi validation

**Trường hợp lỗi:**
1. Khi lưu đơn thuốc có thuốc ngoài danh mục thất bại → Hệ thống hiển thị toast lỗi tương tự thuốc trong danh mục
2. Khi nhập ký tự đặc biệt không hợp lệ → Hệ thống từ chối và hiển thị cảnh báo validation

### Ghi chú kỹ thuật
- Thuốc ngoài danh mục lưu với DrugCatalogItemId = null và IsOffCatalog = true
- Off-catalog badge hiển thị màu secondary để phân biệt trực quan
- Thuốc ngoài danh mục không trừ tồn kho khi cấp phát (Phase 6)
- Trường bắt buộc: tên thuốc (drugName), liều dùng chỉ dẫn (instructions)

---

## US-RX-003: Thuốc liên kết danh mục được đánh dấu trừ tồn kho

**Là một** dược sĩ, **Tôi muốn** hệ thống tự động đánh dấu thuốc liên kết danh mục để trừ tồn kho khi cấp phát, **Để** quản lý tồn kho chính xác và phân biệt với thuốc ngoài danh mục.

**Yêu cầu liên quan:** RX-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ kê đơn thuốc từ danh mục nhà thuốc → Hệ thống tự động liên kết thuốc với DrugCatalogItemId
2. Khi xem đơn thuốc → Thuốc trong danh mục hiển thị bình thường, thuốc ngoài danh mục hiển thị nhãn "Ngoài danh mục" và biểu tượng "Thủ công"
3. Khi dược sĩ cấp phát thuốc (Phase 6) → Hệ thống chỉ trừ tồn kho cho thuốc có DrugCatalogItemId (thuốc trong danh mục)
4. Thuốc ngoài danh mục được đánh dấu "Thủ công" → Dược sĩ phải tự theo dõi số lượng cấp phát

**Trường hợp ngoại lệ:**
1. Nếu thuốc trong danh mục nhưng hết tồn kho → Hệ thống vẫn cho phép kê đơn (cảnh báo tồn kho là chức năng Phase 6)
2. Nếu thuốc bị xóa khỏi danh mục sau khi kê đơn → Đơn thuốc vẫn giữ thông tin thuốc gốc (denormalized data)

**Trường hợp lỗi:**
1. Khi không xác định được trạng thái liên kết danh mục → Hệ thống mặc định là "Thủ công" để đảm bảo an toàn

### Ghi chú kỹ thuật
- PrescriptionItem có trường DrugCatalogItemId (nullable) và IsOffCatalog (boolean)
- Thông tin thuốc lưu denormalized trong PrescriptionItem (tên, hàm lượng, dạng bào chế) để tránh mất dữ liệu khi danh mục thay đổi
- Tự động trừ tồn kho sẽ được implement tại Phase 6 (Pharmacy module)
- API endpoint kiểm tra: GET /api/pharmacy/drugs/{id}/stock (Phase 6)

---

## US-RX-004: Bác sĩ kê đơn kính với thông số khúc xạ

**Là một** bác sĩ, **Tôi muốn** kê đơn kính (optical Rx) với đầy đủ thông số khúc xạ cho mắt xa và mắt gần, **Để** bệnh nhân có thể đặt kính chính xác tại cửa hàng kính mắt.

**Yêu cầu liên quan:** RX-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở lượt khám và chọn phần "Đơn kính" (OpticalPrescriptionSection) → Hệ thống hiển thị form đơn kính với bố cục OD/OS song song
2. Hệ thống hiển thị các trường cho Mắt xa (Distance Rx):
   - SPH (Spherical), CYL (Cylinder), AXIS, ADD cho mỗi mắt (OD/OS)
   - Far PD (khoảng cách đồng tử mắt xa)
3. Hệ thống hiển thị phần Mắt gần (Near Rx) dạng thu gọn mặc định (collapsible):
   - SPH, CYL, AXIS, ADD cho mỗi mắt (OD/OS)
   - Near PD (khoảng cách đồng tử mắt gần)
4. Bác sĩ nhập thông số khúc xạ → Hệ thống validation giá trị hợp lệ
5. Bác sĩ chọn loại tròng kính (Lens Type): một tròng (single vision) / hai tròng (bifocal) / đa tròng (progressive) / đọc sách (reading)
6. Bác sĩ nhấn "Lưu đơn kính" → Hệ thống lưu và hiển thị "Lưu đơn kính thành công"

**Trường hợp ngoại lệ:**
1. Nếu bác sĩ chỉ cần kê đơn mắt xa → Phần mắt gần để trống (thu gọn mặc định)
2. Nếu chỉ kê đơn một mắt → Cho phép để trống thông số mắt còn lại
3. Nếu bệnh nhân đã có đơn kính lượt khám trước → Bác sĩ có thể tham khảo nhưng không sao chép tự động

**Trường hợp lỗi:**
1. Khi lưu đơn kính thất bại → Hệ thống hiển thị toast lỗi: "Lưu đơn kính thất bại"
2. Khi nhập giá trị SPH/CYL ngoài phạm vi hợp lệ → Hệ thống hiển thị cảnh báo validation
3. Khi nhập AXIS ngoài 0-180 độ → Hệ thống từ chối và hiển thị lỗi

### Ghi chú kỹ thuật
- OpticalPrescriptionSection sử dụng VisitSection wrapper
- OpticalPrescriptionForm là component riêng biệt cho logic form tái sử dụng
- Phần Near Rx collapsible mặc định (defaultOpen=false) vì đa số đơn chỉ cần mắt xa
- SetOpticalPrescription domain method xóa bản cũ trước khi thêm mới (một đơn kính mỗi lượt khám)
- Các trường decimal sử dụng precision(5,2) cho giá trị diopter
- API endpoint: POST /api/clinical/visits/{visitId}/optical-prescription

---

## US-RX-005: Tự động điền đơn kính từ kết quả khúc xạ

**Là một** bác sĩ, **Tôi muốn** hệ thống tự động điền thông số đơn kính từ kết quả khúc xạ manifest của lượt khám hiện tại, **Để** tiết kiệm thời gian nhập liệu và giảm sai sót.

**Yêu cầu liên quan:** RX-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở phần "Đơn kính" → Hệ thống kiểm tra lượt khám có kết quả khúc xạ manifest (refractionType = 0) hay không
2. Nếu có kết quả manifest → Hệ thống hiển thị nút "Tự động điền từ khúc xạ" (Auto-fill from refraction)
3. Bác sĩ nhấn nút tự động điền → Hệ thống sao chép SPH, CYL, AXIS, ADD, PD từ kết quả manifest vào form đơn kính
4. Nếu cả hai mắt có giá trị PD → Hệ thống tính trung bình PD cho khoảng cách đồng tử
5. Bác sĩ xem lại và chỉnh sửa thông số nếu cần → Nhấn "Lưu đơn kính" → Hệ thống lưu đơn kính đã chỉnh sửa

**Trường hợp ngoại lệ:**
1. Nếu lượt khám chưa có kết quả khúc xạ → Hệ thống ẩn nút tự động điền, bác sĩ nhập thủ công
2. Nếu chỉ có kết quả khúc xạ một mắt → Hệ thống tự động điền cho mắt có dữ liệu, để trống mắt còn lại
3. Nếu lượt khám có nhiều loại khúc xạ (manifest + cycloplegic) → Hệ thống chỉ sử dụng kết quả manifest (type 0)

**Trường hợp lỗi:**
1. Khi không đọc được dữ liệu khúc xạ → Hệ thống hiển thị thông báo lỗi và cho phép nhập thủ công
2. Khi giá trị khúc xạ không hợp lệ → Hệ thống vẫn hiển thị giá trị và để bác sĩ tự chỉnh sửa

### Ghi chú kỹ thuật
- Auto-fill chỉ sử dụng manifest refraction (refractionType = 0)
- PD trung bình: khi cả OD và OS đều có PD, tính (PD_OD + PD_OS) / 2
- Tự động điền là pre-fill, không phải tự động lưu - bác sĩ phải xác nhận và nhấn "Lưu"
- API: đọc từ dữ liệu refraction hiện có của lượt khám (client-side logic)

---

## US-RX-006: Đơn thuốc tuân thủ định dạng Bộ Y tế

**Là một** bác sĩ, **Tôi muốn** đơn thuốc được tạo theo đúng định dạng quy định của Bộ Y tế (MOH), **Để** đảm bảo tính hợp pháp và được nhà thuốc bên ngoài chấp nhận.

**Yêu cầu liên quan:** RX-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ kê đơn thuốc → Hệ thống tự động định dạng đơn thuốc theo quy định Bộ Y tế bao gồm:
   - Thông tin phòng khám: logo, tên phòng khám, địa chỉ, số điện thoại, số giấy phép hoạt động
   - Thông tin bệnh nhân: họ tên, tuổi, giới tính, địa chỉ, mã bệnh nhân
   - Thông tin bác sĩ: họ tên, chức danh
   - Chẩn đoán: mã ICD-10 và tên bệnh bằng tiếng Việt
   - Danh sách thuốc: STT, tên thuốc, hàm lượng, số lượng, cách dùng (liều lượng, tần suất, thời gian)
   - Ngày kê đơn và chữ ký bác sĩ
2. Các trường bắt buộc theo quy định được đánh dấu và validation trước khi in
3. Liều lượng định dạng theo chuẩn Bộ Y tế: "[Liều dùng] x [Tần suất]/ngày, [Đường dùng], [Thời gian]"

**Trường hợp ngoại lệ:**
1. Nếu thiếu thông tin phòng khám (logo, địa chỉ) → Hệ thống sử dụng giá trị mặc định từ Clinic Settings
2. Nếu bác sĩ ghi đè chỉ dẫn sử dụng (free-text override) → Hệ thống sử dụng văn bản ghi đè thay vì tự động tạo

**Trường hợp lỗi:**
1. Khi thiếu trường bắt buộc (tên bệnh nhân, chẩn đoán) → Hệ thống hiển thị cảnh báo trước khi in
2. Khi không tải được thông tin phòng khám → Hệ thống sử dụng giá trị mặc định cố định

### Ghi chú kỹ thuật
- Đơn thuốc in trên giấy A5 (148 x 210mm) theo chuẩn phòng khám Việt Nam
- QuestPDF (.NET, MIT license) tạo PDF phía backend
- Font Noto Sans nhúng (embedded) hỗ trợ đầy đủ dấu tiếng Việt
- Clinic header lấy từ ClinicSettings (cấu hình được bởi quản trị viên)
- API endpoint: GET /api/clinical/{visitId}/print/drug-prescription

---

## US-RX-007: Cảnh báo dị ứng thuốc khi kê đơn

**Là một** bác sĩ, **Tôi muốn** hệ thống cảnh báo ngay khi tôi chọn thuốc mà bệnh nhân bị dị ứng, **Để** tránh kê đơn thuốc nguy hiểm cho bệnh nhân.

**Yêu cầu liên quan:** RX-05

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ chọn thuốc từ danh mục → Hệ thống tự động đối chiếu tên thuốc với danh sách dị ứng của bệnh nhân
2. Nếu phát hiện trùng khớp dị ứng → Hệ thống hiển thị banner cảnh báo màu đỏ ngay bên dưới trường chọn thuốc: "Cảnh báo: Bệnh nhân dị ứng với [tên chất dị ứng]. Thuốc [tên thuốc] có thể gây phản ứng dị ứng."
3. Banner cảnh báo hiển thị liên tục khi thuốc còn trong đơn để bác sĩ luôn nhận thức

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân chưa có danh sách dị ứng → Hệ thống không hiển thị cảnh báo (không có dữ liệu để đối chiếu)
2. Nếu thuốc ngoài danh mục → Hệ thống vẫn đối chiếu tên thuốc nhập thủ công với danh sách dị ứng (bidirectional Contains matching)
3. Nếu bệnh nhân có nhiều dị ứng trùng khớp → Hệ thống liệt kê tất cả các dị ứng liên quan trong banner

**Trường hợp lỗi:**
1. Khi không tải được danh sách dị ứng bệnh nhân → Hệ thống hiển thị cảnh báo: "Không thể kiểm tra dị ứng. Vui lòng xác nhận thủ công"
2. Khi kết nối mạng gián đoạn trong lúc kiểm tra → Hệ thống hiển thị trạng thái lỗi và cho phép bác sĩ tiếp tục với cảnh báo

### Ghi chú kỹ thuật
- Danh sách dị ứng lấy từ Patient module qua cross-module query (GetPatientAllergiesQuery)
- Đối chiếu hai chiều (bidirectional Contains): kiểm tra cả tên thuốc chứa tên dị ứng VÀ tên dị ứng chứa tên thuốc
- AllergyAlert component tái sử dụng từ Phase 2 (banner mode)
- Dị ứng được fetch trong DrugPrescriptionSection qua usePatientById (self-contained)

---

## US-RX-008: Hộp thoại xác nhận khi lưu đơn có dị ứng

**Là một** bác sĩ, **Tôi muốn** hệ thống bắt buộc xác nhận khi tôi lưu đơn thuốc có thuốc gây dị ứng, **Để** đảm bảo tôi đã có ý thức về rủi ro và ra quyết định có chủ đích.

**Yêu cầu liên quan:** RX-05

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ nhấn "Lưu đơn thuốc" khi đơn có thuốc trùng khớp dị ứng → Hệ thống hiển thị AlertDialog (không thể tắt bằng cách nhấn ngoài):
   - Tiêu đề: "Cảnh báo dị ứng thuốc"
   - Nội dung: Liệt kê cụ thể tên thuốc và dị ứng xung đột
   - Hai nút: "Xác nhận kê đơn" (destructive) và "Hủy bỏ"
2. Bác sĩ nhấn "Xác nhận kê đơn" → Hệ thống lưu đơn thuốc với ghi chú xác nhận dị ứng
3. Bác sĩ nhấn "Hủy bỏ" → Hệ thống quay lại màn hình kê đơn để chỉnh sửa

**Trường hợp ngoại lệ:**
1. Nếu đơn thuốc có nhiều thuốc dị ứng → AlertDialog liệt kê tất cả các thuốc và dị ứng tương ứng
2. Nếu bác sĩ xóa thuốc dị ứng khỏi đơn trước khi lưu → Hệ thống không hiển thị AlertDialog

**Trường hợp lỗi:**
1. Khi xác nhận lưu nhưng lưu thất bại → Hệ thống hiển thị toast lỗi và giữ AlertDialog mở để thử lại
2. Khi mất kết nối khi xác nhận → Hệ thống hiển thị thông báo lỗi mạng

### Ghi chú kỹ thuật
- Sử dụng AlertDialog (non-dismissible) theo pattern của SignOff confirmation
- Nút "Xác nhận kê đơn" sử dụng variant destructive (màu đỏ) để nhấn mạnh rủi ro
- AlertDialog liệt kê cụ thể: "[Tên thuốc A] - Dị ứng: [Chất dị ứng X], [Tên thuốc B] - Dị ứng: [Chất dị ứng Y]"
- Đơn thuốc vẫn được lưu bình thường sau khi xác nhận (không có trường đặc biệt ghi nhận việc xác nhận)

---

## US-PRT-001: In đơn thuốc với tiêu đề phòng khám

**Là một** bác sĩ, **Tôi muốn** in đơn thuốc thành file PDF với đầy đủ tiêu đề phòng khám và thông tin bệnh nhân, **Để** bệnh nhân mang đơn đến nhà thuốc mua thuốc.

**Yêu cầu liên quan:** PRT-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở lượt khám có đơn thuốc → Nhấn nút "In đơn thuốc" → Hệ thống gửi yêu cầu tạo PDF đến backend
2. Backend tạo PDF với bố cục:
   - **Tiêu đề phòng khám:** Logo, tên phòng khám, địa chỉ, số điện thoại, số giấy phép (từ ClinicSettings)
   - **Thông tin bệnh nhân:** Họ tên, tuổi, giới tính, địa chỉ, mã bệnh nhân
   - **Chẩn đoán:** Mã ICD-10 và tên bệnh tiếng Việt
   - **Danh sách thuốc:** Bảng gồm STT, tên thuốc, hàm lượng, số lượng, cách dùng
   - **Chân trang:** Ngày kê đơn, chữ ký bác sĩ, tên bác sĩ
3. Hệ thống mở PDF trong tab mới của trình duyệt → Bác sĩ in hoặc tải về

**Trường hợp ngoại lệ:**
1. Nếu phòng khám chưa cấu hình logo → PDF hiển thị tên phòng khám thay vì logo
2. Nếu đơn thuốc không có thuốc nào → Hệ thống thông báo "Đơn thuốc trống, không thể in"
3. Nếu lượt khám chưa có chẩn đoán → Hệ thống vẫn cho phép in nhưng hiển thị phần chẩn đoán trống

**Trường hợp lỗi:**
1. Khi tạo PDF thất bại → Hệ thống hiển thị toast lỗi: "Không thể tạo đơn thuốc PDF"
2. Khi trình duyệt chặn popup/tab mới → Hệ thống hướng dẫn người dùng cho phép popup

### Ghi chú kỹ thuật
- Giấy A5 (148 x 210mm) theo chuẩn phòng khám Việt Nam
- QuestPDF Community license (miễn phí) với font Noto Sans nhúng cho dấu tiếng Việt
- PrintButton component sử dụng native fetch cho blob PDF response
- Blob URL mở trong tab mới, revokeObjectURL sau 30 giây
- API endpoint: GET /api/clinical/{visitId}/print/drug-prescription trả về Results.File(pdf, "application/pdf")
- Cross-schema raw SQL lấy thông tin bệnh nhân (tránh cross-module project reference)

---

## US-PRT-002: In đơn kính

**Là một** bác sĩ, **Tôi muốn** in đơn kính (optical Rx) thành file PDF với đầy đủ thông số khúc xạ, **Để** bệnh nhân mang đến cửa hàng kính mắt đặt kính.

**Yêu cầu liên quan:** PRT-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở lượt khám có đơn kính → Nhấn nút "In đơn kính" → Hệ thống gửi yêu cầu tạo PDF
2. Backend tạo PDF với bố cục:
   - **Tiêu đề phòng khám:** Logo, tên phòng khám, địa chỉ, số điện thoại (từ ClinicSettings)
   - **Thông tin bệnh nhân:** Họ tên, tuổi, giới tính, mã bệnh nhân
   - **Thông số Mắt xa (Distance Rx):** Bảng OD/OS với SPH, CYL, AXIS, ADD, Far PD
   - **Thông số Mắt gần (Near Rx):** Bảng OD/OS với SPH, CYL, AXIS, ADD, Near PD (nếu có)
   - **Loại tròng kính:** Single vision / Bifocal / Progressive / Reading
   - **Chân trang:** Ngày kê đơn, chữ ký bác sĩ, tên bác sĩ
3. Hệ thống mở PDF trong tab mới → Bác sĩ in hoặc tải về

**Trường hợp ngoại lệ:**
1. Nếu đơn kính chỉ có thông số mắt xa → PDF ẩn phần mắt gần
2. Nếu chỉ kê đơn một mắt → PDF hiển thị "N/A" cho mắt không có dữ liệu
3. Nếu chưa chọn loại tròng kính → Hệ thống để trống trường loại tròng kính trên PDF

**Trường hợp lỗi:**
1. Khi tạo PDF thất bại → Hệ thống hiển thị toast lỗi: "Không thể tạo đơn kính PDF"
2. Khi lượt khám chưa có đơn kính → Hệ thống thông báo "Chưa có đơn kính để in"

### Ghi chú kỹ thuật
- Giấy A4 cho đơn kính (rộng hơn để chứa bảng thông số)
- Bảng khúc xạ sử dụng grid layout với OD/OS rows và SPH/CYL/AXIS/ADD/PD columns
- QuestPDF với font Noto Sans nhúng cho dấu tiếng Việt
- API endpoint: GET /api/clinical/{visitId}/print/optical-prescription

---

## US-PRT-003: In giấy chuyển viện

**Là một** bác sĩ, **Tôi muốn** in giấy chuyển viện (referral letter) cho bệnh nhân khi cần chuyển tuyến điều trị, **Để** bệnh nhân có giấy giới thiệu chính thức đến cơ sở y tế khác.

**Yêu cầu liên quan:** PRT-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở lượt khám → Nhấn nút "In giấy chuyển viện" → Hệ thống gửi yêu cầu tạo PDF
2. Backend tạo PDF với bố cục:
   - **Tiêu đề phòng khám:** Logo, tên phòng khám, địa chỉ, số điện thoại (từ ClinicSettings)
   - **Tiêu đề tài liệu:** "GIẤY CHUYỂN VIỆN"
   - **Thông tin bệnh nhân:** Họ tên, ngày sinh, giới tính, địa chỉ, mã bệnh nhân
   - **Chẩn đoán:** Mã ICD-10 và tên bệnh tiếng Việt
   - **Lý do chuyển viện:** Lý do cần chuyển tuyến điều trị
   - **Tóm tắt bệnh sử:** Quá trình khám và điều trị tại phòng khám
   - **Chân trang kép:** Cột trái - dấu bệnh viện tiếp nhận, cột phải - chữ ký bác sĩ chuyển
3. Hệ thống mở PDF trong tab mới → Bác sĩ in và ký tên

**Trường hợp ngoại lệ:**
1. Nếu chưa nhập lý do chuyển viện → Hệ thống để trống trường, bác sĩ viết tay sau khi in
2. Nếu chưa có chẩn đoán → Hệ thống vẫn cho phép in với phần chẩn đoán trống

**Trường hợp lỗi:**
1. Khi tạo PDF thất bại → Hệ thống hiển thị toast lỗi: "Không thể tạo giấy chuyển viện"

### Ghi chú kỹ thuật
- Giấy A4 cho giấy chuyển viện
- ReferralLetterDocument có chân trang kép (dual footer): cột dấu bệnh viện tiếp nhận + cột chữ ký bác sĩ chuyển
- API endpoint: GET /api/clinical/{visitId}/print/referral-letter
- Dữ liệu bệnh nhân lấy bằng cross-schema raw SQL

---

## US-PRT-004: In giấy đồng ý điều trị

**Là một** bác sĩ, **Tôi muốn** in giấy đồng ý điều trị (consent form) cho bệnh nhân trước khi thực hiện thủ thuật, **Để** đảm bảo bệnh nhân đã được thông tin và đồng ý với quy trình điều trị.

**Yêu cầu liên quan:** PRT-05

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở lượt khám → Nhấn nút "In giấy đồng ý" → Hệ thống gửi yêu cầu tạo PDF
2. Backend tạo PDF với bố cục:
   - **Tiêu đề phòng khám:** Logo, tên phòng khám, địa chỉ, số điện thoại (từ ClinicSettings)
   - **Tiêu đề tài liệu:** "GIẤY ĐỒNG Ý ĐIỀU TRỊ / THỦ THUẬT"
   - **Thông tin bệnh nhân:** Họ tên, ngày sinh, giới tính, địa chỉ
   - **Loại thủ thuật/điều trị:** Tên thủ thuật được thực hiện
   - **Nội dung đồng ý:** Các điều khoản bệnh nhân đồng ý (đã được giải thích về quy trình, rủi ro, phương án thay thế)
   - **Không gian ký tên:** Dòng cho bệnh nhân ký, dòng cho bác sĩ ký, và không gian cho vân tay (fingerprint)
   - **Ngày tháng:** Ngày lập giấy đồng ý
3. Hệ thống mở PDF trong tab mới → Bác sĩ in để bệnh nhân ký

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân là trẻ em → Người giám hộ ký thay (phần ký tên ghi "Người giám hộ")
2. Nếu chưa có thông tin thủ thuật cụ thể → Bác sĩ viết tay bổ sung sau khi in

**Trường hợp lỗi:**
1. Khi tạo PDF thất bại → Hệ thống hiển thị toast lỗi: "Không thể tạo giấy đồng ý"

### Ghi chú kỹ thuật
- Giấy A4 cho giấy đồng ý
- ConsentFormDocument bao gồm không gian vân tay (fingerprint space) bên cạnh chữ ký bệnh nhân/bác sĩ
- API endpoint: GET /api/clinical/{visitId}/print/consent-form
- Template nội dung đồng ý là cố định (không cấu hình được bởi người dùng ở v1)

---

## US-PRT-005: In nhãn nhà thuốc

**Là một** dược sĩ, **Tôi muốn** in nhãn dán lên hộp thuốc với thông tin liều dùng cho từng thuốc trong đơn, **Để** bệnh nhân dễ dàng nhận biết cách sử dụng thuốc tại nhà.

**Yêu cầu liên quan:** PRT-06

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Dược sĩ mở đơn thuốc → Nhấn nút "In nhãn" → Hệ thống gửi yêu cầu tạo PDF nhãn cho từng thuốc trong đơn
2. Backend tạo PDF với bố cục nhãn (70x35mm) cho mỗi thuốc:
   - **Tên phòng khám:** Tên ngắn gọn
   - **Tên bệnh nhân:** Họ tên đầy đủ
   - **Tên thuốc:** Tên thuốc và hàm lượng
   - **Cách dùng:** Liều dùng, tần suất, đường dùng (ví dụ: "Nhỏ 1 giọt x 3 lần/ngày, mắt phải")
   - **Hạn sử dụng:** Ngày hết hạn thuốc (nếu có)
   - **Ngày cấp:** Ngày phát thuốc
3. Hệ thống tạo PDF chứa tất cả nhãn của đơn thuốc → Mở trong tab mới để in

**Trường hợp ngoại lệ:**
1. Nếu đơn có nhiều thuốc → Mỗi thuốc một nhãn, sắp xếp theo thứ tự trong đơn
2. Nếu chỉ dẫn sử dụng dài hơn khung nhãn → Hệ thống cắt bớt và hiển thị phần quan trọng nhất
3. Nếu thuốc ngoài danh mục không có hạn sử dụng → Để trống trường hạn sử dụng

**Trường hợp lỗi:**
1. Khi tạo nhãn thất bại → Hệ thống hiển thị toast lỗi: "Không thể tạo nhãn nhà thuốc"
2. Khi đơn thuốc không có thuốc → Hệ thống thông báo "Đơn thuốc trống, không thể in nhãn"

### Ghi chú kỹ thuật
- PharmacyLabelDocument sử dụng giấy 70x35mm custom PageSize với lề 3mm (standard adhesive label stock)
- Mỗi nhãn là một trang riêng biệt trong PDF
- Font size nhỏ hơn (9-10pt) để vừa khung nhãn
- API endpoint: GET /api/clinical/{visitId}/print/pharmacy-label

---

## US-PRT-006: In hóa đơn / phiếu thu (Hoãn lại đến Phase 7)

> **Trạng thái: HOÃN LẠI ĐẾN PHASE 7**
> Chức năng in hóa đơn và phiếu thu phụ thuộc vào hệ thống tính phí và thanh toán (billing) chưa được xây dựng. Hệ thống billing sẽ được implement tại Phase 7 (Billing & Payments). Khi đó, chức năng in hóa đơn sẽ bao gồm: chi tiết các khoản phí, phương thức thanh toán, và định dạng hóa đơn theo quy định.

**Là một** thu ngân, **Tôi muốn** in hóa đơn/phiếu thu với chi tiết các khoản phí và phương thức thanh toán, **Để** cung cấp bằng chứng thanh toán cho bệnh nhân.

**Yêu cầu liên quan:** PRT-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Thu ngân hoàn tất thu phí → Nhấn "In hóa đơn" → Hệ thống tạo PDF hóa đơn với:
   - Thông tin phòng khám
   - Thông tin bệnh nhân
   - Danh sách các khoản phí (khám, thuốc, thủ thuật, kính mắt)
   - Tổng tiền và phương thức thanh toán
   - Mã hóa đơn và ngày xuất
2. Hệ thống mở PDF trong tab mới để in

**Lý do hoãn lại:**
- Hệ thống tính phí (billing system) chưa được xây dựng
- Cần có dữ liệu giá dịch vụ, chính sách giảm giá, phương thức thanh toán
- Sẽ được implement tại Phase 7 cùng với module Billing & Payments

---

## US-RX-009: Quản trị viên quản lý danh mục thuốc

**Là một** quản trị viên, **Tôi muốn** thêm, sửa, và quản lý danh mục thuốc của phòng khám, **Để** bác sĩ có danh sách thuốc cập nhật khi kê đơn.

**Yêu cầu liên quan:** RX-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản trị viên truy cập trang "Quản lý danh mục thuốc" từ sidebar Nhà thuốc → Hệ thống hiển thị bảng danh sách thuốc với cột: Tên thuốc, Hàm lượng, Dạng bào chế, Đường dùng, Đơn vị tính, Trạng thái
2. Quản trị viên nhấn "Thêm thuốc mới" → Hệ thống hiển thị dialog với các trường:
   - Tên thuốc (bắt buộc)
   - Tên hoạt chất (generic name)
   - Hàm lượng (strength, ví dụ: "0.5%", "500mg")
   - Dạng bào chế (Form): Nhỏ mắt / Viên / Kem / Tiêm / Gel / Dung dịch
   - Đường dùng (Route): Nhỏ mắt / Uống / Tiêm bắp / Tiêm tĩnh mạch / Bôi ngoài da
   - Đơn vị tính: Chai / Tuýp / Hộp / Lọ / Ống
   - Mẫu liều lượng mặc định (default dosage template)
3. Quản trị viên nhập thông tin và nhấn "Lưu" → Hệ thống thêm thuốc vào danh mục và hiển thị "Thêm thuốc thành công"
4. Quản trị viên chọn thuốc trong danh sách và nhấn "Sửa" → Hệ thống hiển thị dialog chỉnh sửa với dữ liệu hiện tại → Quản trị viên cập nhật và nhấn "Lưu"

**Trường hợp ngoại lệ:**
1. Nếu quản trị viên muốn vô hiệu hóa thuốc (không xóa hẳn) → Nhấn nút "Vô hiệu hóa" → Thuốc chuyển sang trạng thái "Không hoạt động" và không hiển thị trong tìm kiếm kê đơn
2. Nếu quản trị viên muốn kích hoạt lại thuốc → Nhấn nút "Kích hoạt" → Thuốc trở lại trạng thái hoạt động
3. Nếu nhập tên thuốc trùng lặp → Hệ thống cảnh báo nhưng vẫn cho phép lưu (có thể có thuốc cùng tên khác hàm lượng)

**Trường hợp lỗi:**
1. Khi lưu thuốc thất bại → Hệ thống hiển thị toast lỗi: "Lưu thuốc thất bại"
2. Khi không có quyền Pharmacy.Manage → Hệ thống trả về lỗi 403 (Forbidden)

### Ghi chú kỹ thuật
- DrugCatalogItem là AggregateRoot với BranchId cho multi-branch
- IsActive soft-delete thay vì Entity.MarkDeleted
- DrugFormDialog sử dụng single form instance với mode prop (thay vì dual-form pattern)
- DRUG_FORM_MAP và DRUG_ROUTE_MAP export từ pharmacy-api.ts cho tái sử dụng
- Dạng bào chế và đường dùng lưu dưới dạng int (enum normalization pattern)
- API endpoints: GET/POST/PUT /api/pharmacy/drugs

---

## US-RX-010: Quản trị viên cấu hình tiêu đề phòng khám

**Là một** quản trị viên, **Tôi muốn** cấu hình thông tin tiêu đề phòng khám hiển thị trên tất cả tài liệu in, **Để** tài liệu chuyên nghiệp và chính xác cho từng chi nhánh.

**Yêu cầu liên quan:** RX-04, PRT-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản trị viên truy cập trang "Cài đặt phòng khám" (Clinic Settings) từ sidebar → Hệ thống hiển thị form cấu hình với các trường:
   - Logo phòng khám (upload hình ảnh)
   - Tên phòng khám (bắt buộc)
   - Địa chỉ (bắt buộc)
   - Số điện thoại
   - Số fax
   - Số giấy phép hoạt động
   - Khẩu hiệu (tagline)
2. Quản trị viên tải lên logo phòng khám → Hệ thống upload và hiển thị preview
3. Quản trị viên nhập/sửa thông tin và nhấn "Lưu cài đặt" → Hệ thống lưu và hiển thị "Cập nhật cài đặt thành công"
4. Tất cả tài liệu in (đơn thuốc, đơn kính, giấy chuyển viện, giấy đồng ý, nhãn nhà thuốc) tự động sử dụng thông tin đã cấu hình

**Trường hợp ngoại lệ:**
1. Nếu chưa cấu hình bất kỳ thông tin nào → Tài liệu in sử dụng giá trị mặc định trống (blank)
2. Nếu logo upload không hợp lệ (format, kích thước) → Hệ thống từ chối và hiển thị cảnh báo
3. Nếu có nhiều chi nhánh → Mỗi chi nhánh có cấu hình riêng (BranchId isolation)

**Trường hợp lỗi:**
1. Khi lưu cài đặt thất bại → Hệ thống hiển thị toast lỗi: "Cập nhật cài đặt thất bại"
2. Khi upload logo thất bại → Hệ thống hiển thị toast lỗi: "Upload logo thất bại"
3. Khi không có quyền → Hệ thống trả về lỗi 403

### Ghi chú kỹ thuật
- ClinicSettings lưu trong ReferenceDbContext (reference schema) là dữ liệu chia sẻ cross-module
- Upsert pattern: CreateOrUpdateAsync -- một dòng cài đặt duy nhất cho mỗi chi nhánh
- Logo upload sử dụng native fetch + FormData (theo pattern patient photo upload)
- IClinicSettingsService inject trực tiếp vào DocumentService (không qua message bus)
- API endpoints: GET/PUT /api/settings/clinic, POST /api/settings/clinic/logo
- Translations trong common.json clinicSettings namespace
