# Nhà thuốc & Kho Vật tư Tiêu hao (Pharmacy & Consumables) - User Stories

**Phạm vi:** Quản lý tồn kho thuốc (theo lô, hạn dùng, nhà cung cấp), nhập kho từ hóa đơn nhà cung cấp và Excel hàng loạt, cảnh báo hạn dùng và tồn kho thấp, cấp phát thuốc theo đơn HIS với FEFO tự động, bán lẻ không đơn (OTC), kiểm tra hiệu lực đơn thuốc 7 ngày, quản lý nhà cung cấp thuốc, và kho vật tư tiêu hao độc lập cho điều trị.
**Yêu cầu liên quan:** PHR-01, PHR-02, PHR-03, PHR-04, PHR-05, PHR-06, PHR-07, CON-01, CON-02, CON-03
**Số lượng user stories:** 12

---

## US-PHR-001: Quản lý tồn kho thuốc theo lô

**Là một** dược sĩ, **Tôi muốn** xem và quản lý toàn bộ tồn kho thuốc của phòng khám với thông tin theo lô và hạn sử dụng, **Để** kiểm soát chính xác số lượng và chất lượng thuốc trong kho.

**Yêu cầu liên quan:** PHR-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Dược sĩ truy cập mục "Nhà thuốc" → "Tồn kho thuốc" từ sidebar → Hệ thống hiển thị bảng danh sách tất cả thuốc với cột: Tên thuốc, Hàm lượng, Dạng bào chế, Tổng tồn kho, Số lô đang có, Lô gần hết hạn nhất, Trạng thái (Đủ/Thấp/Hết)
2. Dược sĩ nhấn vào một dòng thuốc → Hệ thống hiển thị panel mở rộng (expand) hoặc chuyển trang chi tiết với danh sách tất cả lô: Số lô, Ngày hết hạn, Số lượng còn lại, Giá mua, Nhà cung cấp
3. Dược sĩ có thể lọc theo trạng thái (Đủ / Tồn thấp / Sắp hết hạn / Hết hạn) và tìm kiếm theo tên thuốc
4. Hệ thống hiển thị tổng hợp: tổng số mặt hàng, số mặt hàng tồn thấp, số mặt hàng sắp hết hạn ở đầu trang (summary cards)

**Trường hợp ngoại lệ:**
1. Nếu thuốc chưa có lô nào → Hệ thống hiển thị "Chưa có tồn kho" và trạng thái "Hết"
2. Nếu tất cả các lô của một thuốc đã hết hạn → Hệ thống đánh dấu thuốc là "Hết hạn" màu đỏ
3. Nếu tồn kho = 0 nhưng còn lô chưa hết hạn → Hệ thống hiển thị "Hết hàng" thay vì "Hết hạn"

**Trường hợp lỗi:**
1. Khi không tải được danh sách tồn kho → Hệ thống hiển thị toast lỗi: "Không thể tải dữ liệu tồn kho"
2. Khi không có quyền Pharmacy.ViewStock → Hệ thống trả về lỗi 403 và chuyển hướng về trang chính

### Ghi chú kỹ thuật
- DrugInventoryBatch là entity con của DrugCatalogItem (một thuốc có nhiều lô)
- Tổng tồn kho = SUM(Quantity) của tất cả lô còn hạn sử dụng
- Cột "Lô gần hết hạn nhất" lấy ExpiryDate nhỏ nhất trong các lô còn hàng
- DataTable component tái sử dụng với expand row pattern
- API endpoints: GET /api/pharmacy/drugs (với include=batches), GET /api/pharmacy/drugs/{id}/batches
- Badge màu: Đủ = green, Thấp = yellow, Hết hàng = red, Hết hạn = gray

---

## US-PHR-002: Nhập kho thuốc từ hóa đơn nhà cung cấp

**Là một** dược sĩ, **Tôi muốn** nhập kho thuốc bằng cách điền thông tin từ hóa đơn nhà cung cấp vào hệ thống, **Để** cập nhật tồn kho chính xác sau mỗi lần nhập hàng.

**Yêu cầu liên quan:** PHR-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Dược sĩ truy cập mục "Nhập kho" → "Nhập từ hóa đơn" → Hệ thống hiển thị form nhập kho với các trường: Nhà cung cấp (combobox), Số hóa đơn, Ngày hóa đơn, Ghi chú
2. Dược sĩ chọn nhà cung cấp từ combobox → Hệ thống hiển thị danh sách thuốc của nhà cung cấp đó với giá mua mặc định đã cấu hình
3. Dược sĩ nhấn "Thêm dòng thuốc" → Hệ thống thêm dòng mới vào bảng nhập: Thuốc (combobox), Số lô, Ngày hết hạn, Số lượng, Giá mua
4. Dược sĩ điền thông tin từng dòng thuốc. Nếu đã có giá mua mặc định theo nhà cung cấp → Hệ thống tự động điền giá, dược sĩ có thể sửa
5. Dược sĩ nhấn "Xác nhận nhập kho" → Hệ thống tạo các lô mới trong kho và hiển thị "Nhập kho thành công. Đã nhập [X] mặt hàng, tổng [Y] đơn vị"

**Trường hợp ngoại lệ:**
1. Nếu số lô đã tồn tại cho thuốc đó → Hệ thống cảnh báo "Số lô [X] đã tồn tại cho thuốc [Y]. Bạn có muốn cộng dồn số lượng không?"
2. Nếu ngày hết hạn nhỏ hơn hoặc bằng hôm nay → Hệ thống không cho phép nhập và hiển thị lỗi validation
3. Nếu dược sĩ muốn xóa một dòng thuốc → Nhấn nút xóa trên dòng đó → Dòng bị xóa khỏi bảng nhập
4. Nếu không tìm thấy thuốc trong danh mục → Hệ thống thông báo "Thuốc chưa có trong danh mục. Vui lòng thêm vào danh mục trước"

**Trường hợp lỗi:**
1. Khi xác nhận nhập kho thất bại → Hệ thống hiển thị toast lỗi: "Nhập kho thất bại. [Chi tiết lỗi]"
2. Khi không có quyền Pharmacy.ManageStock → Hệ thống trả về lỗi 403

### Ghi chú kỹ thuật
- Mỗi dòng nhập tạo một DrugInventoryBatch mới trong database
- Nếu số lô trùng → merge thêm quantity vào lô hiện có (cùng batch number, ngày hết hạn phải khớp)
- StockImport là aggregate với StockImportLine items (audit trail cho từng lần nhập)
- Command: ImportStockFromInvoiceCommand → ImportStockFromInvoiceHandler
- API endpoint: POST /api/pharmacy/stock/import-invoice
- Sau khi nhập thành công → invalidate cache tồn kho thuốc

---

## US-PHR-003: Nhập kho hàng loạt từ Excel

**Là một** dược sĩ, **Tôi muốn** nhập kho thuốc từ file Excel theo mẫu chuẩn, **Để** tiết kiệm thời gian khi nhập lô hàng lớn hoặc tải tồn kho ban đầu vào hệ thống.

**Yêu cầu liên quan:** PHR-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Dược sĩ truy cập mục "Nhập kho" → "Nhập từ Excel" → Hệ thống hiển thị trang với nút tải mẫu Excel và khu vực upload file
2. Dược sĩ tải mẫu Excel → Hệ thống cung cấp file mẫu với các cột: Tên thuốc, Số lô, Ngày hết hạn (dd/mm/yyyy), Số lượng, Giá mua (VND), Tên nhà cung cấp, Số hóa đơn
3. Dược sĩ điền dữ liệu vào mẫu và upload file → Hệ thống đọc file và hiển thị bảng preview với kết quả validation từng dòng: xanh (hợp lệ), vàng (cảnh báo), đỏ (lỗi)
4. Dược sĩ xem xét kết quả preview. Các dòng lỗi được đánh dấu với mô tả lỗi chi tiết. Dược sĩ có thể xóa các dòng lỗi hoặc chỉ import các dòng hợp lệ
5. Dược sĩ nhấn "Xác nhận nhập kho [X] dòng hợp lệ" → Hệ thống nhập kho và hiển thị kết quả: "Nhập thành công [X]/[Y] dòng"

**Trường hợp ngoại lệ:**
1. Nếu file không đúng định dạng Excel (.xlsx, .xls) → Hệ thống từ chối và hiển thị "Vui lòng sử dụng file Excel (.xlsx hoặc .xls)"
2. Nếu file quá lớn (> 10MB) → Hệ thống từ chối và hiển thị giới hạn kích thước
3. Nếu tên thuốc trong Excel không khớp với danh mục → Dòng đó được đánh dấu lỗi "Không tìm thấy thuốc trong danh mục"
4. Nếu tất cả dòng đều lỗi → Hệ thống không cho phép nhập và hiển thị: "Không có dòng hợp lệ nào để nhập"

**Trường hợp lỗi:**
1. Khi upload file thất bại → Hệ thống hiển thị toast lỗi: "Upload file thất bại. Vui lòng thử lại"
2. Khi xử lý file lỗi (file bị hỏng) → Hệ thống hiển thị: "Không thể đọc file. Vui lòng kiểm tra định dạng file"
3. Khi nhập kho thất bại sau preview → Hệ thống hiển thị chi tiết dòng nào thất bại

### Ghi chú kỹ thuật
- Sử dụng thư viện ClosedXML (MIT license) phía backend để đọc/ghi Excel
- Template Excel có sheet "Dữ liệu nhập kho" với header cố định và sheet "Hướng dẫn" giải thích từng cột
- Match thuốc bằng tên (Vietnamese_CI_AI collation, accent-insensitive)
- Validation: ngày hết hạn phải > hôm nay, số lượng > 0, giá mua >= 0
- Import endpoint trả về ImportResult{SuccessCount, FailureCount, Errors[]} với HTTP 200
- Preview state lưu tạm trong component state (không persist đến backend cho đến khi xác nhận)

---

## US-PHR-004: Cảnh báo hạn sử dụng thuốc

**Là một** dược sĩ, **Tôi muốn** nhận cảnh báo về thuốc sắp hết hạn sử dụng, **Để** xử lý kịp thời trước khi thuốc bị hỏng và không thể sử dụng được.

**Yêu cầu liên quan:** PHR-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Dược sĩ đăng nhập vào hệ thống → Sidebar hiển thị badge cảnh báo màu vàng/đỏ trên mục "Nhà thuốc" nếu có thuốc sắp hết hạn
2. Dược sĩ truy cập "Nhà thuốc" → "Cảnh báo" → Hệ thống hiển thị dashboard cảnh báo với hai phần:
   - **Thuốc sắp hết hạn:** Danh sách thuốc có lô hết hạn trong 30/60/90 ngày tới (theo ngưỡng cấu hình), hiển thị: Tên thuốc, Số lô, Ngày hết hạn, Số ngày còn lại, Số lượng
   - **Thuốc đã hết hạn:** Danh sách thuốc có lô đã hết hạn (cần xử lý/hủy)
3. Dược sĩ nhấn vào một dòng thuốc → Hệ thống chuyển đến trang chi tiết lô thuốc đó để xem và xử lý
4. Quản trị viên có thể cấu hình ngưỡng cảnh báo (30, 60, hoặc 90 ngày) trong phần Cài đặt nhà thuốc

**Trường hợp ngoại lệ:**
1. Nếu không có thuốc nào sắp hết hạn → Hệ thống hiển thị "Không có cảnh báo hạn dùng" với biểu tượng xanh lá (trạng thái tốt)
2. Nếu cùng một thuốc có nhiều lô sắp hết hạn → Hệ thống liệt kê từng lô riêng biệt
3. Nếu lô đã hết hạn nhưng tồn kho = 0 → Hệ thống vẫn hiển thị nhưng với ghi chú "Đã hết hàng"

**Trường hợp lỗi:**
1. Khi không tải được dữ liệu cảnh báo → Hệ thống hiển thị toast lỗi: "Không thể tải dữ liệu cảnh báo"
2. Khi lưu cài đặt ngưỡng thất bại → Hệ thống hiển thị toast lỗi: "Lưu cài đặt thất bại"

### Ghi chú kỹ thuật
- Query lấy lô có ExpiryDate <= NOW() + configuredDays (default: 30 ngày)
- PharmacyAlertSettings lưu trong PharmacySettings bảng: ExpiryWarningDays (default: 30), LowStockThreshold per drug
- Sidebar badge count = số lô sắp hết hạn (trong 30 ngày) + số thuốc dưới mức tồn tối thiểu
- API endpoint: GET /api/pharmacy/alerts/expiry?days={days}
- Mỗi ngưỡng (30/60/90) hiển thị bằng màu khác nhau: 30=đỏ, 60=cam, 90=vàng

---

## US-PHR-005: Cảnh báo tồn kho thuốc thấp

**Là một** dược sĩ, **Tôi muốn** nhận cảnh báo khi tồn kho thuốc xuống dưới mức tối thiểu đã cấu hình, **Để** đặt hàng nhập thêm kịp thời trước khi hết thuốc.

**Yêu cầu liên quan:** PHR-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Dược sĩ truy cập "Nhà thuốc" → "Cảnh báo" → Phần "Tồn kho thấp" hiển thị danh sách thuốc có tổng tồn kho < mức tối thiểu, gồm: Tên thuốc, Tồn kho hiện tại, Mức tối thiểu, Thiếu (chênh lệch), Nhà cung cấp thường xuyên
2. Quản trị viên hoặc dược sĩ có quyền vào trang chi tiết thuốc để cấu hình mức tồn kho tối thiểu (MinStockLevel) cho từng thuốc
3. Khi tồn kho vừa xuống dưới ngưỡng tối thiểu → Hệ thống tự động thêm vào danh sách cảnh báo trong lần tải trang tiếp theo

**Trường hợp ngoại lệ:**
1. Nếu thuốc chưa được cấu hình mức tối thiểu (MinStockLevel = null) → Hệ thống không cảnh báo cho thuốc đó
2. Nếu MinStockLevel = 0 → Hệ thống chỉ cảnh báo khi hết hàng hoàn toàn (tồn kho = 0)
3. Nếu tồn kho = 0 và MinStockLevel > 0 → Thuốc được đánh dấu "Hết hàng" (severity cao hơn "Tồn thấp")

**Trường hợp lỗi:**
1. Khi lưu mức tối thiểu thất bại → Hệ thống hiển thị toast lỗi: "Lưu mức tồn kho tối thiểu thất bại"
2. Khi không tải được dữ liệu cảnh báo tồn thấp → Hệ thống hiển thị thông báo lỗi tải dữ liệu

### Ghi chú kỹ thuật
- MinStockLevel lưu trên DrugCatalogItem (nullable int)
- Query: SELECT * FROM drugs WHERE TotalStock < MinStockLevel AND MinStockLevel IS NOT NULL
- TotalStock tính từ SUM của DrugInventoryBatch.Quantity nơi ExpiryDate > GETDATE() và Quantity > 0
- API endpoint: GET /api/pharmacy/alerts/low-stock
- Cập nhật MinStockLevel: PUT /api/pharmacy/drugs/{id}/settings

---

## US-PHR-006: Cấp phát thuốc theo đơn HIS

**Là một** dược sĩ, **Tôi muốn** cấp phát thuốc cho bệnh nhân dựa trên đơn thuốc điện tử từ HIS với hệ thống tự động chọn lô theo nguyên tắc FEFO, **Để** đảm bảo cấp phát đúng đơn, trừ tồn kho chính xác, và ưu tiên dùng thuốc sắp hết hạn trước.

**Yêu cầu liên quan:** PHR-05

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Dược sĩ truy cập "Nhà thuốc" → "Hàng chờ cấp phát" → Hệ thống hiển thị danh sách đơn thuốc chờ cấp phát với: Tên bệnh nhân, Mã đơn, Bác sĩ kê đơn, Thời gian, Trạng thái (Chờ / Đang xử lý / Đã cấp)
2. Dược sĩ nhấn vào một đơn thuốc → Hệ thống hiển thị chi tiết đơn với từng dòng thuốc: Tên thuốc, Liều dùng, Số lượng cần cấp
3. Với mỗi dòng thuốc, hệ thống tự động đề xuất lô theo FEFO (lô hết hạn sớm nhất trước): hiển thị số lô được chọn và tồn kho lô đó
4. Dược sĩ xem xét đề xuất lô. Nếu muốn đổi lô → Nhấn "Chọn lô khác" → Hệ thống hiển thị danh sách tất cả lô còn hàng để dược sĩ chọn thủ công
5. Dược sĩ nhấn "Xác nhận cấp phát" → Hệ thống trừ tồn kho các lô đã chọn và đánh dấu đơn là "Đã cấp phát"
6. Hệ thống hiển thị "Cấp phát thành công" và hiển thị nút "In nhãn thuốc" để in nhãn dán lên hộp thuốc

**Trường hợp ngoại lệ:**
1. Nếu đơn thuốc đã hết hạn (> 7 ngày kể từ ngày kê) → Hệ thống hiển thị banner cảnh báo màu vàng: "Đơn thuốc đã hết hiệu lực ngày [X]. Bạn có chắc chắn muốn cấp phát không?" với nút "Xác nhận vẫn cấp phát" và "Hủy"
2. Nếu không đủ tồn kho cho một dòng thuốc → Hệ thống hiển thị cảnh báo đỏ trên dòng đó: "Không đủ tồn kho. Cần [X] nhưng chỉ còn [Y]"
3. Nếu tất cả các lô của thuốc đã hết hạn → Hệ thống không cho phép cấp phát dòng thuốc đó và hiển thị "Thuốc hết hạn"
4. Dược sĩ có thể bỏ qua (skip) một dòng thuốc nếu không đủ hàng → Đơn được cấp phát một phần (partial), dòng bị bỏ qua được đánh dấu "Chưa cấp"

**Trường hợp lỗi:**
1. Khi xác nhận cấp phát thất bại → Hệ thống hiển thị toast lỗi: "Cấp phát thất bại. Vui lòng thử lại"
2. Khi mất kết nối trong lúc cấp phát → Hệ thống kiểm tra trạng thái đơn và thông báo kết quả thực tế
3. Khi tồn kho bị thay đổi bởi giao dịch khác trong lúc cấp phát (race condition) → Hệ thống trả về lỗi optimistic concurrency và yêu cầu tải lại trang

### Ghi chú kỹ thuật
- FEFO: ORDER BY ExpiryDate ASC, sau đó BatchCreatedAt ASC (xác định thứ tự khi cùng ngày hết hạn)
- DispensingRecord entity: gồm PrescriptionId, DispensingLines[]{PrescriptionItemId, BatchId, Quantity, DispensingDate}
- Đơn thuốc có trạng thái: Pending → InProgress → Dispensed / PartiallyDispensed
- Prescription validity check: DispensingDate - PrescriptionDate > 7 days → cảnh báo (cho phép override)
- Sidebar badge: COUNT(prescriptions WHERE Status = Pending)
- API endpoints: GET /api/pharmacy/dispensing-queue, POST /api/pharmacy/dispensing/{prescriptionId}/dispense

---

## US-PHR-007: Kiểm tra hiệu lực đơn thuốc 7 ngày

**Là một** dược sĩ, **Tôi muốn** hệ thống tự động kiểm tra và cảnh báo khi đơn thuốc đã quá 7 ngày kể từ ngày kê, **Để** tuân thủ quy định về hạn sử dụng đơn thuốc của Bộ Y tế.

**Yêu cầu liên quan:** PHR-07

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Dược sĩ mở đơn thuốc để cấp phát → Hệ thống tự động tính số ngày từ ngày kê đến hôm nay
2. Nếu đơn thuốc còn trong 7 ngày → Không có cảnh báo, luồng cấp phát bình thường
3. Nếu đơn đã quá 7 ngày → Hệ thống hiển thị banner cảnh báo nổi bật: "Đơn thuốc hết hiệu lực - Kê ngày [DD/MM/YYYY], đã quá [X] ngày (hiệu lực 7 ngày theo quy định)"
4. Dược sĩ vẫn có thể cấp phát nhưng phải xác nhận qua dialog: "Đơn thuốc quá hạn. Dược sĩ xác nhận vẫn cấp phát và nhận trách nhiệm về quyết định này" với ô nhập lý do bắt buộc
5. Hệ thống ghi lại lý do override vào DispensingRecord để audit trail

**Trường hợp ngoại lệ:**
1. Nếu đơn thuốc kê cho bệnh nhân mãn tính (chronic prescription) → Hệ thống vẫn áp dụng quy tắc 7 ngày (không có ngoại lệ tự động cho v1)
2. Nếu ngày kê đơn trong tương lai (lỗi dữ liệu) → Hệ thống hiển thị cảnh báo dữ liệu bất thường
3. Nếu cùng ngày kê đơn (0 ngày) → Không cảnh báo (đơn mới)

**Trường hợp lỗi:**
1. Khi không xác định được ngày kê đơn (null) → Hệ thống hiển thị cảnh báo: "Không xác định được ngày kê đơn. Vui lòng kiểm tra thủ công"
2. Khi lưu lý do override thất bại → Hệ thống vẫn cho phép cấp phát nhưng log lỗi phía backend

### Ghi chú kỹ thuật
- Tính ngày: (DispensingDate.Date - PrescriptionDate.Date).Days > 7
- Banner dùng AlertDialog (non-dismissible) với variant=destructive cho đơn quá hạn
- Override reason lưu trong DispensingRecord.ExpiryOverrideReason (nullable string)
- DispensingDate mặc định = server time (không cho phép chọn ngày trước)
- Logic validation phía backend để tránh bypass qua API

---

## US-PHR-008: Bán lẻ thuốc không đơn (OTC Walk-in)

**Là một** dược sĩ, **Tôi muốn** bán thuốc không đơn (OTC) cho khách hàng vãng lai không có đơn thuốc HIS, **Để** phục vụ nhu cầu mua thuốc thông thường tại phòng khám và trừ tồn kho tự động.

**Yêu cầu liên quan:** PHR-06

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Dược sĩ truy cập "Nhà thuốc" → "Bán lẻ OTC" → Hệ thống hiển thị form bán lẻ nhanh
2. Dược sĩ tùy chọn liên kết với khách hàng: tìm kiếm tên/số điện thoại (hoặc để anonymous) → Hệ thống tự động điền tên khách hàng nếu tìm thấy
3. Dược sĩ thêm thuốc: tìm kiếm tên thuốc → chọn → Hệ thống hiển thị tồn kho hiện tại và giá bán
4. Dược sĩ nhập số lượng cho từng thuốc → Hệ thống tự động áp dụng FEFO để chọn lô (tương tự cấp phát đơn)
5. Dược sĩ nhấn "Xác nhận bán" → Hệ thống trừ tồn kho và tạo OTC Sale record → Hiển thị "Bán hàng thành công"

**Trường hợp ngoại lệ:**
1. Nếu không tìm thấy khách hàng → Dược sĩ có thể bán anonymous (không bắt buộc liên kết) hoặc tạo mới walk-in customer (PAT-02)
2. Nếu số lượng yêu cầu vượt tồn kho → Hệ thống hiển thị cảnh báo: "Chỉ còn [X] đơn vị trong kho"
3. Nếu thuốc không phải OTC (cần đơn theo quy định) → Hệ thống vẫn cho bán nhưng hiển thị nhắc nhở (không chặn ở v1)
4. Nếu dược sĩ muốn hủy giao dịch trước khi xác nhận → Nhấn "Hủy" → Hệ thống xóa form mà không thay đổi tồn kho

**Trường hợp lỗi:**
1. Khi xác nhận bán thất bại → Hệ thống hiển thị toast lỗi: "Bán hàng thất bại. Tồn kho không thay đổi"
2. Khi race condition (cùng lúc bán và cấp phát) → Hệ thống xử lý bằng optimistic concurrency và thông báo

### Ghi chú kỹ thuật
- OtcSale entity: PatientId (nullable), SaleDate, SaleLines[]{DrugCatalogItemId, BatchId, Quantity, SellingPrice}
- Cùng cơ chế FEFO với cấp phát đơn (shared service DeductStock)
- Giá bán lấy từ DrugCatalogItem.SellingPrice (không phải giá mua theo lô)
- Không có payment trong Phase 6 — OtcSale.PaymentStatus = "Pending" (Phase 7 xử lý)
- Không có receipt/invoice generation trong Phase 6 (deferred to Phase 7)
- API endpoint: POST /api/pharmacy/otc-sales

---

## US-PHR-009: Quản lý nhà cung cấp thuốc

**Là một** quản trị viên, **Tôi muốn** quản lý danh sách nhà cung cấp thuốc với thông tin liên hệ và giá mua mặc định theo từng thuốc, **Để** nhập kho thuận tiện hơn với giá mua được điền sẵn và theo dõi nguồn gốc hàng hóa.

**Yêu cầu liên quan:** PHR-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản trị viên truy cập "Nhà thuốc" → "Nhà cung cấp" → Hệ thống hiển thị bảng danh sách nhà cung cấp: Tên, Liên hệ, Số điện thoại, Email, Số mặt hàng cung cấp, Ngày hợp tác
2. Quản trị viên nhấn "Thêm nhà cung cấp" → Hệ thống hiển thị form: Tên nhà cung cấp (bắt buộc), Người liên hệ, Số điện thoại, Email, Địa chỉ, Ghi chú
3. Quản trị viên lưu → Nhà cung cấp được thêm vào hệ thống và hiển thị "Thêm nhà cung cấp thành công"
4. Quản trị viên mở chi tiết nhà cung cấp → Thêm giá mua mặc định cho từng thuốc: chọn thuốc từ danh mục → nhập giá mua thường xuyên → Hệ thống lưu SupplierDrugPrice
5. Khi dược sĩ nhập kho và chọn nhà cung cấp này → Giá mua được tự động điền vào cột giá

**Trường hợp ngoại lệ:**
1. Nếu nhà cung cấp đã tồn tại (cùng tên) → Hệ thống cảnh báo nhưng vẫn cho phép lưu (các nhà cung cấp có thể cùng tên khác tỉnh)
2. Nếu quản trị viên muốn vô hiệu hóa nhà cung cấp (không xóa) → Nhấn "Vô hiệu hóa" → Nhà cung cấp không hiển thị trong combobox khi nhập kho
3. Một thuốc có thể có nhiều nhà cung cấp → Hệ thống lưu bảng SupplierDrugPrice với composite key (SupplierId, DrugId)

**Trường hợp lỗi:**
1. Khi lưu nhà cung cấp thất bại → Hệ thống hiển thị toast lỗi: "Lưu nhà cung cấp thất bại"
2. Khi không có quyền Pharmacy.ManageSuppliers → Hệ thống trả về lỗi 403

### Ghi chú kỹ thuật
- Supplier entity: Id, Name, ContactPerson, Phone, Email, Address, Notes, IsActive, BranchId
- SupplierDrugPrice entity: SupplierId, DrugCatalogItemId, DefaultPurchasePrice, UpdatedAt
- API endpoints: GET/POST/PUT /api/pharmacy/suppliers, GET/PUT /api/pharmacy/suppliers/{id}/pricing

---

## US-CON-001: Quản lý kho vật tư tiêu hao

**Là một** y tá hoặc dược sĩ, **Tôi muốn** xem và quản lý tồn kho vật tư tiêu hao dùng trong điều trị (IPL gel, tấm che mắt, đầu LLLT, v.v.) tách biệt với kho thuốc, **Để** đảm bảo đủ vật tư cho các buổi điều trị và không nhầm lẫn với tồn kho thuốc.

**Yêu cầu liên quan:** CON-01, CON-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Y tá/dược sĩ truy cập mục "Vật tư tiêu hao" từ sidebar (mục riêng biệt với "Nhà thuốc") → Hệ thống hiển thị bảng danh sách vật tư với cột: Tên vật tư, Phân loại, Đơn vị, Tồn kho, Loại theo dõi (Theo lô/Đơn giản), Trạng thái
2. Y tá nhấn "Điều chỉnh tồn kho" trên một vật tư → Hệ thống hiển thị dialog điều chỉnh: Loại điều chỉnh (Nhập thêm/Xuất/Hiệu chỉnh), Số lượng, Lý do
3. Với vật tư theo dõi theo lô (expiry-tracked): form điều chỉnh có thêm trường Số lô và Ngày hết hạn
4. Y tá nhấn "Xác nhận" → Hệ thống cập nhật tồn kho và tạo audit record cho điều chỉnh → Hiển thị "Cập nhật tồn kho thành công"
5. Hệ thống đã được seeded sẵn với ~10-15 vật tư IPL/LLLT thông dụng: IPL gel, tấm che mắt IPL, đầu LLLT dùng một lần, tấm lót chăm sóc mi mắt, khăn lau vô khuẩn, thuốc nhỏ gây tê (Benoxinate), v.v.

**Trường hợp ngoại lệ:**
1. Nếu điều chỉnh dẫn đến tồn kho âm → Hệ thống cảnh báo: "Điều chỉnh này sẽ dẫn đến tồn kho âm (-[X] [đơn vị]). Xác nhận tiếp tục?" với lý do bắt buộc
2. Nếu vật tư theo dõi theo lô và ngày hết hạn đã qua → Hệ thống không cho phép nhập mới lô đó
3. Nếu quản trị viên muốn thêm vật tư mới chưa có trong seed data → Nhấn "Thêm vật tư" → Form: Tên, Phân loại (IPL/LLLT/Chăm sóc mi/Vệ sinh/Khác), Đơn vị, Loại theo dõi (Theo lô/Đơn giản), Mức tồn tối thiểu

**Trường hợp lỗi:**
1. Khi lưu điều chỉnh tồn kho thất bại → Hệ thống hiển thị toast lỗi: "Điều chỉnh tồn kho thất bại"
2. Khi không tải được danh sách vật tư → Hệ thống hiển thị thông báo lỗi tải dữ liệu
3. Khi không có quyền Consumables.ManageStock → Hệ thống trả về lỗi 403

### Ghi chú kỹ thuật
- ConsumableItem entity trong ConsumablesDbContext (schema tách biệt hoàn toàn với pharmacy schema)
- TrackingType enum: ExpiryTracked (có số lô + ngày hết hạn) vs SimpleStock (chỉ số lượng)
- ConsumableStockAdjustment entity: ghi lại tất cả thay đổi tồn kho với lý do và timestamp
- Seed data trong ConsumableCatalogSeeder (IHostedService pattern, idempotent)
- Vật tư tiêu hao seed: IPL Gel 60ml, Tấm che mắt IPL (đôi), Đầu LLLT dùng một lần, Tấm lót mi mắt, Khăn lau vô khuẩn 10x10cm, Thuốc nhỏ gây tê Benoxinate 0.4%, Bông y tế cuộn, Gạc vô khuẩn, Túi đá lạnh mini, Kẹp phẫu thuật (sterile)
- API endpoints: GET /api/consumables/items, POST /api/consumables/adjustments

---

## US-CON-002: Cảnh báo tồn kho vật tư thấp

**Là một** y tá, **Tôi muốn** nhận cảnh báo khi vật tư tiêu hao xuống dưới mức tối thiểu, **Để** đặt mua thêm trước khi thiếu hụt ảnh hưởng đến lịch điều trị của bệnh nhân.

**Yêu cầu liên quan:** CON-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Y tá truy cập "Vật tư tiêu hao" → "Cảnh báo tồn kho" → Hệ thống hiển thị danh sách vật tư có tồn kho dưới mức tối thiểu: Tên vật tư, Tồn kho hiện tại, Mức tối thiểu, Thiếu (chênh lệch)
2. Sidebar hiển thị badge cảnh báo trên mục "Vật tư tiêu hao" nếu có vật tư tồn thấp (tương tự badge Nhà thuốc)
3. Quản trị viên/y tá có quyền cấu hình MinStockLevel cho từng vật tư trong trang cài đặt vật tư
4. Vật tư theo dõi theo lô: cảnh báo hết hạn tương tự PHR-03 nhưng trong phần "Vật tư tiêu hao"

**Trường hợp ngoại lệ:**
1. Nếu vật tư chưa được cấu hình mức tối thiểu → Không hiển thị cảnh báo tồn thấp cho vật tư đó
2. Nếu vật tư bị vô hiệu hóa (IsActive = false) → Không hiển thị trong danh sách cảnh báo
3. Nếu không có vật tư nào dưới ngưỡng → Hiển thị trạng thái "Tất cả vật tư đầy đủ" với icon xanh lá

**Trường hợp lỗi:**
1. Khi không tải được dữ liệu cảnh báo vật tư → Hệ thống hiển thị toast lỗi tương ứng
2. Khi cập nhật mức tối thiểu thất bại → Hệ thống hiển thị toast lỗi: "Cập nhật mức tối thiểu thất bại"

### Ghi chú kỹ thuật
- Query tương tự PHR-04/05 nhưng cho ConsumableItem
- ConsumableItem.MinStockLevel (nullable int)
- API endpoint: GET /api/consumables/alerts/low-stock
- Badge consumables = số vật tư dưới MinStockLevel + số lô sắp hết hạn (30 ngày)

---

## US-CON-003: Chuẩn bị cơ sở hạ tầng cho trừ tồn kho tự động (Phase 9)

**Là một** quản trị viên hệ thống, **Tôi muốn** hệ thống vật tư tiêu hao được xây dựng sẵn sàng cho tính năng trừ tồn kho tự động theo phiên điều trị, **Để** khi Phase 9 (Treatment Protocols) triển khai có thể tích hợp liền mạch mà không cần tái cấu trúc lớn.

**Yêu cầu liên quan:** CON-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Hệ thống có bảng ConsumableItem với trường IsAutoDeductEnabled (boolean, default = false) — sẵn sàng để Phase 9 bật lên khi cần
2. Hệ thống có bảng TreatmentConsumableTemplate (Phase 9 sẽ populate): TreatmentTypeId, ConsumableItemId, DefaultQuantity — ghi lại mặc định tiêu hao cho mỗi loại điều trị
3. Quản trị viên có thể xem interface placeholder trong trang cài đặt vật tư: "Liên kết với phiên điều trị (sẽ khả dụng trong Phase 9)" — chỉ hiển thị, chưa hoạt động
4. Architecture đảm bảo ConsumablesDbContext được đặt trong module riêng biệt để Treatments module (Phase 9) có thể tham chiếu qua Consumables.Contracts

**Trường hợp ngoại lệ:**
1. Nếu admin cố tình bật IsAutoDeductEnabled = true cho một vật tư → Hệ thống chấp nhận lưu nhưng tính năng tự động trừ chưa hoạt động cho đến khi Phase 9 triển khai
2. Nếu Phase 9 chưa triển khai → Tất cả điều chỉnh tồn kho vật tư vẫn là thủ công (US-CON-001)

**Trường hợp lỗi:**
1. Không có trường hợp lỗi đặc biệt — đây là story về cấu trúc dữ liệu, không phải chức năng người dùng

### Ghi chú kỹ thuật
- ConsumableItem.IsAutoDeductEnabled: bool = false (default, Phase 9 sẽ toggle thành true)
- TreatmentConsumableTemplate table: để trống trong Phase 6, Phase 9 sẽ populate bằng seeder/migration
- Consumables.Contracts project: IConsumableStockService interface với method DeductStock(consumableId, quantity, sessionId) — interface được định nghĩa trong Phase 6, implementation bổ sung trong Phase 9
- ConsumablesDbContext ở trong Consumables module — không ghép nối với PharmacyDbContext
- Đây là preparedness/scaffolding story: không có UI mới, chỉ đảm bảo data model và interfaces sẵn sàng

---

*Phiên bản: 1.0*
*Ngày tạo: 2026-03-06*
*Giai đoạn: Phase 6 - Pharmacy & Consumables*
*Yêu cầu được bao phủ: PHR-01, PHR-02, PHR-03, PHR-04, PHR-05, PHR-06, PHR-07, CON-01, CON-02, CON-03*
