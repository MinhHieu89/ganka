# User Stories: Trung Tâm Kính (Optical Center)

**Phase:** 08 - Optical Center
**Ngày tạo:** 2026-03-06
**Yêu cầu liên quan:** OPT-01, OPT-02, OPT-03, OPT-04, OPT-05, OPT-06, OPT-07, OPT-08, OPT-09
**Số lượng user stories:** 21

---

## OPT-01: Quản lý danh mục gọng kính

### US-OPT-001: Quản lý danh mục gọng kính

**Là một** nhân viên quầy kính,
**Tôi muốn** quản lý danh mục gọng kính với đầy đủ thông tin thương hiệu, mẫu, màu sắc, kích thước, chất liệu, và giá bán,
**Để** có thể tra cứu nhanh và tư vấn chính xác cho khách hàng.

**Yêu cầu liên quan:** OPT-01

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên vào mục "Trung Tâm Kính" → "Danh Mục Gọng" → Nhấn "Thêm gọng mới"
2. Nhân viên nhập đầy đủ thông tin: Thương hiệu (ví dụ: Ray-Ban), Mẫu (ví dụ: RB3025), Màu sắc (Vàng), Kích thước (52-18-140), Chất liệu (Kim loại), Giới tính (Nam/Nữ/Unisex), Loại gọng (Full-rim), Giá bán (VNĐ), Giá nhập (VNĐ), Số lượng tồn kho ban đầu
3. Nhân viên nhấn "Lưu" → Hệ thống tạo gọng mới và hiển thị trong bảng danh mục với tất cả thông tin
4. Gọng mới xuất hiện trong danh sách với trạng thái "Còn hàng" nếu số lượng > 0
5. Nhân viên có thể chỉnh sửa hoặc xoá gọng hiện có từ danh sách

**Trường hợp ngoại lệ:**
1. Nếu gọng đã có trong hệ thống (trùng thương hiệu + mẫu + màu + kích thước) → Hệ thống cảnh báo: "Gọng kính này đã tồn tại trong danh mục"
2. Nếu số lượng tồn kho = 0 → Gọng vẫn được tạo nhưng hiển thị trạng thái "Hết hàng"
3. Nếu nhân viên xoá gọng đang có trong đơn hàng chưa hoàn tất → Hệ thống từ chối: "Không thể xoá gọng kính đang trong đơn đặt hàng"

**Trường hợp lỗi:**
1. Thiếu thông tin bắt buộc (Thương hiệu, Mẫu, Giá bán) → Hệ thống hiển thị lỗi validation từng trường
2. Giá bán âm hoặc bằng 0 → Hệ thống hiển thị lỗi: "Giá bán phải lớn hơn 0"
3. Kích thước không đúng định dạng (phải là XX-XX-XXX) → Hệ thống hiển thị: "Kích thước không hợp lệ. Định dạng: chiều rộng tròng-cầu mũi-gọng (ví dụ: 52-18-140)"

#### Ghi chú kỹ thuật
- Entity: Frame (FrameId, Brand, Model, Color, LensWidth, BridgeWidth, TempleLength, Material, Gender, FrameType, SellingPrice, CostPrice, StockQuantity, BranchId)
- Kích thước được lưu riêng: LensWidth, BridgeWidth, TempleLength (mm)
- API endpoints: GET /api/optical/frames, POST /api/optical/frames, PUT /api/optical/frames/{id}, DELETE /api/optical/frames/{id}

---

### US-OPT-002: Quét mã vạch để tra cứu gọng kính

**Là một** nhân viên quầy kính,
**Tôi muốn** quét mã vạch EAN-13 trên gọng kính bằng máy quét hoặc camera điện thoại để tìm kiếm trong hệ thống,
**Để** tra cứu thông tin và tư vấn giá nhanh chóng mà không cần nhập tay.

**Yêu cầu liên quan:** OPT-01

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên nhấn vào ô tìm kiếm trên trang danh mục gọng → Ô tìm kiếm được focus
2. Nhân viên đưa máy quét mã vạch USB qua gọng kính → Mã vạch EAN-13 được tự động nhập vào ô tìm kiếm
3. Hệ thống tức thì tìm kiếm và hiển thị gọng kính tương ứng với đầy đủ thông tin (tên, màu, kích thước, giá, tồn kho)
4. Với camera điện thoại/máy tính bảng: nhân viên nhấn nút "Quét camera" → Hệ thống bật giao diện quét → Nhân viên hướng camera vào mã vạch → Mã được nhận diện tự động

**Trường hợp ngoại lệ:**
1. Mã vạch của nhà sản xuất không có trong hệ thống → Hệ thống hiển thị: "Không tìm thấy gọng với mã vạch [XXXXXXX]. Bạn có muốn thêm mới không?" với nút "Thêm gọng mới"
2. Quét mã vạch bị mờ hoặc hỏng → Camera không nhận diện được → Nhân viên nhập mã thủ công vào ô tìm kiếm

**Trường hợp lỗi:**
1. Camera không có quyền truy cập (browser permission) → Hệ thống hiển thị hướng dẫn cấp quyền camera
2. Định dạng mã vạch không phải EAN-13 → Hệ thống hiển thị: "Mã vạch không hợp lệ. Hệ thống chỉ hỗ trợ mã EAN-13"

#### Ghi chú kỹ thuật
- USB barcode scanner: keyboard input mode, không cần API đặc biệt
- Camera scanning: sử dụng thư viện web-based (html5-qrcode hoặc tương đương)
- EAN-13 validation regex: 13 chữ số
- API endpoint: GET /api/optical/frames/barcode/{barcode}

---

### US-OPT-003: Quản lý tồn kho gọng kính

**Là một** nhân viên quầy kính,
**Tôi muốn** theo dõi số lượng tồn kho của từng gọng kính và nhận cảnh báo khi gần hết hàng,
**Để** chủ động đặt hàng bổ sung và tránh bán hết hàng.

**Yêu cầu liên quan:** OPT-01

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên vào "Danh Mục Gọng" → Cột "Tồn kho" hiển thị số lượng hiện tại cho từng gọng
2. Khi tồn kho ≤ ngưỡng cảnh báo (mặc định = 2) → Hàng được tô màu vàng và hiển thị biểu tượng cảnh báo
3. Khi tồn kho = 0 → Hàng được tô màu đỏ và hiển thị "Hết hàng"
4. Mỗi khi giao kính cho bệnh nhân → Hệ thống tự động trừ 1 đơn vị tồn kho
5. Nhân viên có thể nhập số lượng nhập hàng để tăng tồn kho (ghi nhận lịch sử nhập hàng)

**Trường hợp ngoại lệ:**
1. Tồn kho âm (bán nhiều hơn số lượng có) → Hệ thống ngăn bán và hiển thị: "Gọng kính này đã hết hàng"
2. Nhân viên điều chỉnh tồn kho thủ công → Hệ thống yêu cầu nhập lý do điều chỉnh (để lưu audit)

**Trường hợp lỗi:**
1. Nhập số lượng âm khi nhập hàng → Hệ thống hiển thị lỗi: "Số lượng nhập phải lớn hơn 0"
2. Lỗi cập nhật tồn kho → Hệ thống hiển thị toast lỗi: "Cập nhật tồn kho thất bại. Vui lòng thử lại"

#### Ghi chú kỹ thuật
- StockQuantity giảm khi GlassesOrder trạng thái chuyển sang "Delivered"
- Cảnh báo tồn kho: LOW_STOCK_THRESHOLD = 2 (configurable)
- API endpoint: POST /api/optical/frames/{id}/stock-adjustment

---

## OPT-02: Quản lý danh mục tròng kính

### US-OPT-004: Quản lý danh mục tròng kính

**Là một** nhân viên quầy kính,
**Tôi muốn** quản lý danh mục tròng kính với đầy đủ thông tin thương hiệu, loại, chất liệu, và khoảng số độ có sẵn,
**Để** tra cứu nhanh khi lắp kính cho bệnh nhân.

**Yêu cầu liên quan:** OPT-02

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên vào "Trung Tâm Kính" → "Danh Mục Tròng" → Nhấn "Thêm tròng mới"
2. Nhân viên nhập: Thương hiệu (Essilor, Hoya, Việt Pháp), Loại (Đơn tâm/Hai tâm/Đa tâm/Đọc sách), Chất liệu (CR-39/Polycarbonate/Hi-Index), Lớp phủ (chống UV, chống ánh sáng xanh, chống trầy...), Khoảng số độ (SPH từ -20D đến +12D, CYL từ -6D đến +6D), Giá bán, Giá nhập
3. Nhân viên nhấn "Lưu" → Tròng kính được thêm vào danh mục
4. Với tròng kính có sẵn (bulk stock): nhập số lượng tồn kho theo từng mức độ (ví dụ: SPH -1.00 = 5 cặp, SPH -1.25 = 3 cặp)

**Trường hợp ngoại lệ:**
1. Tròng kính đặt theo đơn (không tồn kho) → Đánh dấu "Đặt theo đơn" → Không yêu cầu nhập số lượng tồn kho
2. Tròng kính hết stock cho một mức độ nhất định → Hệ thống cảnh báo "Hết hàng" cho mức đó, không ngăn chọn (sẽ chuyển sang đặt theo đơn)

**Trường hợp lỗi:**
1. Khoảng số độ không hợp lệ (SPH > +12D hoặc < -20D) → Hệ thống hiển thị lỗi: "Số độ vượt quá khoảng hỗ trợ"
2. Thiếu thông tin bắt buộc (Thương hiệu, Loại, Giá bán) → Hệ thống hiển thị lỗi validation

#### Ghi chú kỹ thuật
- Entity: Lens (LensId, Brand, Type, Material, Coating, MinSph, MaxSph, MinCyl, MaxCyl, SellingPrice, CostPrice, IsStocked, BranchId)
- LensStock entity: LensId, Sph, Cyl, Quantity (cho tồng kính có sẵn)
- API endpoints: GET /api/optical/lenses, POST /api/optical/lenses

---

### US-OPT-005: Theo dõi tồn kho tròng kính theo số độ

**Là một** nhân viên quầy kính,
**Tôi muốn** theo dõi số lượng tồng kính tồn kho theo từng mức số độ cụ thể,
**Để** biết chính xác tròng nào còn hàng và tròng nào cần đặt thêm.

**Yêu cầu liên quan:** OPT-02

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên vào chi tiết tròng kính → Tab "Tồn kho theo số độ" → Hệ thống hiển thị bảng: Số độ (SPH/CYL) × Số lượng
2. Khi gán tròng kính cho đơn hàng kính → Hệ thống tự động trừ 1 đơn vị từ ô tồn kho tương ứng
3. Khi tồn kho ≤ 2 cặp → Hệ thống tô màu vàng cảnh báo
4. Khi tồn kho = 0 → Hệ thống chuyển tự động sang chế độ "Đặt theo đơn" cho mức độ đó

**Trường hợp ngoại lệ:**
1. Bệnh nhân có số độ ngoài khoảng tồn kho → Hệ thống tự động tạo yêu cầu đặt hàng từ nhà cung cấp

**Trường hợp lỗi:**
1. Lỗi cập nhật tồn kho sau khi gán → Hệ thống hoàn tác gán tròng và hiển thị lỗi để nhân viên thử lại

#### Ghi chú kỹ thuật
- LensStock được tra cứu bằng (LensId, Sph, Cyl) composite key
- Trừ tồn kho qua domain event LensAssignedToOrder

---

### US-OPT-006: Đặt hàng tròng kính từ nhà cung cấp

**Là một** nhân viên quầy kính,
**Tôi muốn** tạo đơn đặt hàng tròng kính từ các nhà cung cấp (Essilor, Hoya, Việt Pháp) theo đơn thuốc của bệnh nhân,
**Để** đảm bảo bệnh nhân nhận được đúng loại tròng kính phù hợp.

**Yêu cầu liên quan:** OPT-02

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Khi tạo đơn kính và tròng kính cần đặt theo đơn → Hệ thống hiển thị form "Đặt tròng kính từ nhà cung cấp"
2. Nhân viên chọn nhà cung cấp (Essilor/Hoya/Việt Pháp), xác nhận thông số tròng (lấy từ đơn thuốc: SPH, CYL, AXIS, ADD), chọn loại tròng và lớp phủ
3. Hệ thống tạo yêu cầu đặt hàng và gắn với đơn kính của bệnh nhân
4. Nhân viên theo dõi trạng thái đặt hàng: Đã đặt → Đang sản xuất → Đã nhận

**Trường hợp ngoại lệ:**
1. Nhà cung cấp không có tròng phù hợp số độ đặc biệt → Nhân viên liên hệ trực tiếp và cập nhật thủ công

**Trường hợp lỗi:**
1. Thiếu đơn thuốc kính để tham chiếu → Hệ thống hiển thị: "Vui lòng đảm bảo bệnh nhân đã có đơn kính từ bác sĩ"

#### Ghi chú kỹ thuật
- LensOrder entity: LensOrderId, SupplierId, GlassesOrderId, LensId, Sph, Cyl, Axis, Add, Status, CreatedAt
- Nhà cung cấp: dùng Supplier entity từ Pharmacy module (Phase 6), tag bổ sung loại "Optical"

---

## OPT-03: Quản lý vòng đời đơn hàng kính

### US-OPT-007: Tạo đơn hàng kính từ đơn thuốc

**Là một** nhân viên quầy kính,
**Tôi muốn** tạo đơn hàng kính cho bệnh nhân bằng cách chọn gọng và tròng từ danh mục và liên kết với đơn thuốc kính từ bác sĩ,
**Để** đảm bảo kính được làm đúng theo toa của bác sĩ.

**Yêu cầu liên quan:** OPT-03

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên vào "Đơn Hàng Kính" → Nhấn "Tạo đơn mới" → Tìm bệnh nhân (theo tên, điện thoại, mã bệnh nhân)
2. Hệ thống hiển thị đơn thuốc kính hiện có của bệnh nhân (từ bác sĩ HIS) với thông số: SPH, CYL, AXIS, ADD, PD
3. Nhân viên chọn đơn thuốc muốn thực hiện → Hệ thống tự động điền thông số kính
4. Nhân viên chọn gọng kính từ danh mục (quét mã vạch hoặc tìm kiếm) → Hệ thống hiển thị tồn kho và giá
5. Nhân viên chọn tròng kính phù hợp → Hệ thống kiểm tra tồn kho (có sẵn hoặc đặt theo đơn)
6. Nhân viên điền ngày dự kiến hoàn thành và ghi chú đặc biệt → Nhấn "Tạo đơn hàng"
7. Hệ thống tạo đơn hàng với trạng thái "Đã đặt" (Ordered) và gửi tự động sang thanh toán

**Trường hợp ngoại lệ:**
1. Bệnh nhân có nhiều đơn thuốc kính → Hệ thống hiển thị danh sách để nhân viên chọn đúng đơn
2. Gọng kính hết hàng → Hệ thống cảnh báo và hỏi có muốn đặt mua không
3. Bệnh nhân không có đơn thuốc kính → Hệ thống thông báo: "Bệnh nhân chưa có đơn kính. Vui lòng yêu cầu bác sĩ kê đơn trước"

**Trường hợp lỗi:**
1. Không thể tải danh sách đơn thuốc → Hệ thống hiển thị lỗi: "Không thể lấy thông tin đơn thuốc. Vui lòng thử lại"
2. Thiếu thông số bắt buộc (gọng hoặc tròng) → Hệ thống hiển thị lỗi validation

#### Ghi chú kỹ thuật
- GlassesOrder entity: GlassesOrderId, PatientId, PrescriptionId, FrameId, LensId, Status, ProcessingType (InHouse/Outsourced), EstimatedDeliveryDate, Notes, CreatedAt, BranchId
- Cross-module query qua Clinical.Contracts để lấy đơn thuốc kính
- Status ban đầu: Ordered

---

### US-OPT-008: Theo dõi trạng thái đơn hàng kính

**Là một** nhân viên quầy kính,
**Tôi muốn** theo dõi trạng thái xử lý của từng đơn hàng kính qua từng bước,
**Để** cập nhật tiến độ và thông báo kịp thời cho bệnh nhân.

**Yêu cầu liên quan:** OPT-03

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên vào "Đơn Hàng Kính" → Xem danh sách đơn hàng với cột trạng thái: Đã đặt / Đang xử lý / Đã nhận tròng / Sẵn sàng / Đã giao
2. Nhân viên mở chi tiết đơn → Nhấn "Chuyển trạng thái" → Hệ thống hiển thị bước tiếp theo hợp lệ
3. Đơn gia công nội bộ: Đã đặt → Đang xử lý (lắp kính) → Sẵn sàng → Đã giao
4. Đơn gia công bên ngoài: Đã đặt → Đang xử lý (gửi sang lab) → Đã nhận tròng (nhận từ lab) → Sẵn sàng → Đã giao
5. Khi đơn chuyển sang "Sẵn sàng" → Hệ thống thêm đơn vào danh sách "Chờ lấy kính" và hiển thị thời gian sẵn sàng
6. Khi giao kính → Nhân viên xác nhận chữ ký/mã PIN bệnh nhân → Đơn chuyển "Đã giao" → Hệ thống trừ tồn kho

**Trường hợp ngoại lệ:**
1. Đơn hàng quá hạn (EstimatedDeliveryDate đã qua mà chưa sang "Sẵn sàng") → Hệ thống hiển thị biểu tượng cảnh báo và ghi chú "Trễ hạn"
2. Nhân viên muốn bỏ qua bước → Hệ thống từ chối: "Phải hoàn tất bước trước khi chuyển tiếp"

**Trường hợp lỗi:**
1. Chuyển trạng thái thất bại → Hệ thống hiển thị lỗi và giữ nguyên trạng thái hiện tại
2. Bệnh nhân không đến lấy kính sau 30 ngày → Hệ thống đánh dấu "Chờ lấy lâu" để nhân viên theo dõi

#### Ghi chú kỹ thuật
- Status transitions: Ordered → Processing → Received (outsourced only) → Ready → Delivered
- Domain event: GlassesOrderStatusChanged → cập nhật tồn kho khi Delivered
- Overdue alert: cron job hàng ngày kiểm tra EstimatedDeliveryDate

---

### US-OPT-009: Xem danh sách đơn hàng kính theo trạng thái

**Là một** nhân viên quầy kính,
**Tôi muốn** xem danh sách đơn hàng kính được lọc theo trạng thái và tìm kiếm theo tên bệnh nhân,
**Để** nhanh chóng ưu tiên xử lý và theo dõi đơn hàng hiệu quả.

**Yêu cầu liên quan:** OPT-03

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên vào "Đơn Hàng Kính" → Hệ thống hiển thị bảng đơn hàng kính với cột: Mã đơn, Tên bệnh nhân, Gọng, Tròng, Trạng thái, Ngày dự kiến, Ghi chú
2. Nhân viên có thể lọc theo trạng thái (Tất cả / Đã đặt / Đang xử lý / Sẵn sàng / Đã giao)
3. Nhân viên có thể tìm kiếm theo tên bệnh nhân hoặc mã đơn hàng
4. Kết quả hiển thị theo thứ tự ngày dự kiến gần nhất trước

**Trường hợp ngoại lệ:**
1. Không có đơn hàng nào theo bộ lọc → Hệ thống hiển thị "Không có đơn hàng phù hợp"

**Trường hợp lỗi:**
1. Lỗi tải danh sách → Hệ thống hiển thị: "Không thể tải danh sách đơn hàng. Vui lòng thử lại"

#### Ghi chú kỹ thuật
- API endpoint: GET /api/optical/orders?status=&search=&page=&pageSize=
- Phân trang: mặc định 20 đơn/trang

---

## OPT-04: Xác nhận thanh toán trước khi xử lý

### US-OPT-010: Chặn xử lý kính khi chưa thanh toán

**Là một** nhân viên quầy kính,
**Tôi muốn** hệ thống tự động kiểm tra trạng thái thanh toán trước khi cho phép chuyển đơn kính sang bước "Đang xử lý",
**Để** đảm bảo doanh thu không bị thất thoát và không gia công kính cho bệnh nhân chưa thanh toán.

**Yêu cầu liên quan:** OPT-04

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Đơn kính ở trạng thái "Đã đặt" → Bệnh nhân thanh toán đầy đủ qua quầy thu ngân (Phase 7)
2. Hệ thống nhận xác nhận thanh toán (qua domain event PaymentConfirmed) → Tự động mở khóa nút "Chuyển sang Đang xử lý"
3. Nhân viên nhấn "Chuyển sang Đang xử lý" → Đơn hàng chuyển trạng thái thành công

**Trường hợp ngoại lệ:**
1. Bệnh nhân chưa thanh toán hoặc thanh toán chưa đủ → Nút "Chuyển sang Đang xử lý" bị vô hiệu hóa
2. Khi di chuột qua nút bị vô hiệu hóa → Tooltip hiển thị: "Đơn hàng chưa được thanh toán đầy đủ. Vui lòng thu ngân xác nhận thanh toán"

**Trường hợp lỗi:**
1. Lỗi kết nối với module thanh toán → Hệ thống hiển thị cảnh báo: "Không thể xác minh trạng thái thanh toán. Vui lòng liên hệ thu ngân trực tiếp"
2. Nhân viên cố tình bypass qua API → API trả về lỗi 400: "Đơn hàng chưa được thanh toán đầy đủ"

#### Ghi chú kỹ thuật
- Cross-module query qua Billing.Contracts: IsInvoiceFullyPaid(invoiceId)
- Domain event: PaymentConfirmed → cập nhật GlassesOrder.IsPaymentConfirmed = true
- Backend validation: kiểm tra IsPaymentConfirmed trước khi cho phép chuyển trạng thái Processing

---

## OPT-05: Kính áp tròng qua HIS

### US-OPT-011: Kê đơn kính áp tròng qua bác sĩ HIS

**Là một** bác sĩ,
**Tôi muốn** kê đơn kính áp tròng (Ortho-K, kính mềm) trực tiếp trong hệ thống HIS khi khám bệnh,
**Để** đảm bảo bệnh nhân nhận được kính áp tròng theo đúng phác đồ điều trị mà không cần thêm bước xử lý tại quầy kính.

**Yêu cầu liên quan:** OPT-05

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ trong quá trình khám → Mở tab "Đơn thuốc kính áp tròng" → Chọn loại kính (Ortho-K / Kính mềm hàng tháng / Kính mềm hàng ngày)
2. Bác sĩ điền thông số: BC (Base Curve), Diameter, Sph, loại kính (thương hiệu), hướng dẫn sử dụng, lịch tái khám
3. Bác sĩ ký duyệt đơn → Hệ thống lưu đơn thuốc kính áp tròng vào hồ sơ bệnh nhân
4. Đơn thuốc được in ra theo định dạng chuẩn → Bệnh nhân nhận đơn và tự mua ở cửa hàng kính bên ngoài
5. Kính áp tròng KHÔNG được quản lý tồn kho tại quầy kính (không có bước nhập hàng/bán hàng tại optical counter)

**Trường hợp ngoại lệ:**
1. Bác sĩ điền BC không phù hợp với loại kính → Hệ thống cảnh báo nhưng không chặn (bác sĩ quyết định cuối)
2. Bệnh nhân đang đeo kính áp tròng loại khác → Hệ thống hiển thị lịch sử đơn kính áp tròng cũ để bác sĩ tham chiếu

**Trường hợp lỗi:**
1. Thiếu thông số bắt buộc (BC, Diameter, Sph) → Hệ thống hiển thị lỗi validation
2. Không thể lưu đơn thuốc → Hệ thống hiển thị toast lỗi: "Lưu đơn thuốc thất bại"

#### Ghi chú kỹ thuật
- ContactLensPrescription entity trong Clinical module (không phải Optical module)
- Không có tồn kho kính áp tròng tại quầy kính
- Đơn thuốc được in qua QuestPDF với định dạng chuẩn

---

## OPT-06: Gói kính combo

### US-OPT-012: Tạo và áp dụng gói kính combo có sẵn

**Là một** quản trị viên / nhân viên quầy kính,
**Tôi muốn** tạo các gói kính combo có sẵn (gọng + tròng cố định) với giá trọn gói,
**Để** tư vấn và áp dụng nhanh cho bệnh nhân mà không cần chọn từng thành phần.

**Yêu cầu liên quan:** OPT-06

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Admin vào "Cài Đặt" → "Gói Kính Combo" → Nhấn "Tạo gói mới"
2. Admin đặt tên gói (ví dụ: "Gói Phổ Thông"), chọn gọng mẫu, chọn tròng mẫu, nhập giá trọn gói (ít hơn tổng giá từng thành phần)
3. Admin nhấn "Lưu" → Gói được kích hoạt và hiển thị trong danh sách combo có sẵn
4. Khi tạo đơn kính: nhân viên chọn "Áp dụng gói combo" → Chọn gói từ danh sách → Hệ thống tự động điền gọng và tròng tương ứng với giá trọn gói

**Trường hợp ngoại lệ:**
1. Gọng trong gói hết hàng → Gói hiển thị cảnh báo "Gọng tạm hết hàng" nhưng vẫn có thể chọn (nhân viên xử lý thủ công)
2. Admin muốn ngưng gói → Chuyển trạng thái gói sang "Ngưng hoạt động" → Không hiển thị khi tạo đơn mới

**Trường hợp lỗi:**
1. Giá combo lớn hơn tổng giá thành phần → Hệ thống cảnh báo: "Giá combo lớn hơn giá từng thành phần. Bạn có muốn tiếp tục không?"
2. Thiếu tên gói hoặc giá → Hệ thống hiển thị lỗi validation

#### Ghi chú kỹ thuật
- PresetCombo entity: ComboId, Name, FrameId, LensId, ComboPrice, IsActive, CreatedAt
- API endpoints: GET /api/optical/combos, POST /api/optical/combos, PUT /api/optical/combos/{id}

---

### US-OPT-013: Tạo gói kính combo tùy chỉnh khi đặt hàng

**Là một** nhân viên quầy kính,
**Tôi muốn** tạo gói kính tùy chỉnh ad-hoc khi tạo đơn hàng bằng cách chọn gọng và tròng khác nhau và điều chỉnh giá,
**Để** linh hoạt phục vụ nhu cầu đặc biệt của từng bệnh nhân với mức giá phù hợp.

**Yêu cầu liên quan:** OPT-06

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Khi tạo đơn kính → Nhân viên chọn "Gói tùy chỉnh" thay vì combo có sẵn
2. Nhân viên chọn gọng bất kỳ + tròng bất kỳ → Hệ thống hiển thị tổng giá mặc định (giá gọng + giá tròng)
3. Nhân viên nhập giá điều chỉnh (override) → Hệ thống tính tự động phần trăm giảm giá
4. Nhân viên thêm ghi chú lý do điều chỉnh giá → Nhấn "Xác nhận"
5. Hệ thống tạo đơn với giá tùy chỉnh và ghi vào dòng chi tiết hóa đơn (Phase 7 billing) dưới dạng "Gói kính - [Tên bệnh nhân]"

**Trường hợp ngoại lệ:**
1. Giảm giá quá mức (ví dụ hơn 30%) → Hệ thống yêu cầu xác nhận bằng mã PIN quản lý (tương tự Phase 7 FIN-07)

**Trường hợp lỗi:**
1. Giá tùy chỉnh nhỏ hơn hoặc bằng 0 → Hệ thống hiển thị lỗi: "Giá phải lớn hơn 0"
2. Không nhập lý do khi điều chỉnh giá → Hệ thống hiển thị: "Vui lòng nhập lý do điều chỉnh giá"

#### Ghi chú kỹ thuật
- CustomCombo không lưu entity riêng — lưu trực tiếp vào GlassesOrder với CustomPrice và DiscountReason
- Giảm giá lớn trigger VerifyManagerPin (tương tự pattern Phase 7)

---

## OPT-07: Quản lý bảo hành

### US-OPT-014: Tạo yêu cầu bảo hành kính

**Là một** nhân viên quầy kính,
**Tôi muốn** tạo yêu cầu bảo hành cho bệnh nhân khi kính bị lỗi trong thời gian bảo hành 12 tháng,
**Để** xử lý nhanh và lưu hồ sơ bảo hành đầy đủ.

**Yêu cầu liên quan:** OPT-07

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên vào "Bảo Hành" → Tìm đơn kính của bệnh nhân (theo tên, điện thoại, hoặc mã đơn)
2. Hệ thống hiển thị thông tin đơn kính: ngày giao, ngày hết bảo hành (ngày giao + 12 tháng), còn trong bảo hành hay không
3. Nhân viên nhấn "Tạo yêu cầu bảo hành" → Điền: Loại xử lý (Thay thế/Sửa chữa/Giảm giá), Mô tả vấn đề, Ghi chú đánh giá
4. Nhân viên tải lên ảnh/tài liệu hỗ trợ (Azure Blob) → Nhấn "Gửi yêu cầu"
5. Nếu loại = Sửa chữa hoặc Giảm giá → Nhân viên tự xử lý, ghi nhận kết quả
6. Hệ thống lưu đầy đủ hồ sơ bảo hành với audit trail

**Trường hợp ngoại lệ:**
1. Đơn kính đã hết thời hạn bảo hành → Hệ thống hiển thị cảnh báo "Đã hết hạn bảo hành [N ngày]" nhưng vẫn cho phép tạo yêu cầu (xử lý ngoại lệ)
2. Bệnh nhân không có đơn kính trong hệ thống → Hệ thống hiển thị: "Không tìm thấy đơn kính. Bệnh nhân có thể đã mua kính ở nơi khác"

**Trường hợp lỗi:**
1. Tải lên ảnh thất bại → Hệ thống hiển thị lỗi tải ảnh và cho phép thử lại mà không mất dữ liệu đã nhập
2. Thiếu mô tả vấn đề → Hệ thống hiển thị lỗi validation

#### Ghi chú kỹ thuật
- WarrantyClaim entity: ClaimId, GlassesOrderId, ClaimDate, ResolutionType (Replace/Repair/Discount), Description, AssessmentNotes, Status, RequiresManagerApproval, BranchId
- Azure Blob: upload ảnh bảo hành, lưu URL vào WarrantyDocument entity
- IAzureBlobService: pattern tái sử dụng từ Phase 4 (Medical Imaging)

---

### US-OPT-015: Phê duyệt bảo hành thay thế bởi quản lý

**Là một** quản lý,
**Tôi muốn** xem xét và phê duyệt hoặc từ chối các yêu cầu bảo hành loại "Thay thế" bằng mã PIN xác thực,
**Để** kiểm soát chi phí thay thế kính và đảm bảo chỉ thực hiện khi thực sự cần thiết.

**Yêu cầu liên quan:** OPT-07

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên tạo yêu cầu bảo hành loại "Thay thế" → Hệ thống tự động đặt trạng thái "Chờ phê duyệt quản lý"
2. Quản lý nhận thông báo (hoặc vào màn hình "Chờ phê duyệt") → Xem chi tiết yêu cầu và ảnh hỗ trợ
3. Quản lý nhập mã PIN xác thực → Chọn "Phê duyệt" hoặc "Từ chối" kèm lý do
4. Nếu phê duyệt → Yêu cầu bảo hành chuyển sang "Đã phê duyệt", nhân viên tiến hành thay thế
5. Nếu từ chối → Yêu cầu chuyển sang "Từ chối" với lý do → Nhân viên thông báo bệnh nhân

**Trường hợp ngoại lệ:**
1. Quản lý không có mặt → Nhân viên có thể ghi chú và để trạng thái "Chờ phê duyệt" → Xử lý sau

**Trường hợp lỗi:**
1. Sai mã PIN quản lý → Hệ thống hiển thị: "Mã PIN không đúng. Vui lòng thử lại"
2. Quản lý từ chối nhưng không nhập lý do → Hệ thống hiển thị: "Vui lòng nhập lý do từ chối"

#### Ghi chú kỹ thuật
- RequiresManagerApproval = true khi ResolutionType = Replace
- VerifyManagerPin endpoint: tái sử dụng từ Phase 7 (Auth module)
- WarrantyClaim.Status: Pending → Approved / Rejected

---

### US-OPT-016: Tải lên tài liệu bảo hành

**Là một** nhân viên quầy kính,
**Tôi muốn** tải lên ảnh chụp hoặc tài liệu liên quan đến yêu cầu bảo hành,
**Để** có bằng chứng lưu trữ và hỗ trợ quản lý phê duyệt chính xác.

**Yêu cầu liên quan:** OPT-07

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Trong form tạo yêu cầu bảo hành → Nhân viên nhấn "Tải lên ảnh" → Chọn file từ máy tính hoặc chụp ảnh từ camera
2. Hệ thống hiển thị xem trước ảnh → Nhân viên xác nhận → Hệ thống tải lên Azure Blob Storage
3. Tối đa 5 ảnh/tài liệu mỗi yêu cầu, định dạng: JPG, PNG, PDF, dung lượng tối đa 10MB/file
4. Sau khi tải lên thành công → URL ảnh được lưu vào WarrantyDocument entity, liên kết với yêu cầu bảo hành

**Trường hợp ngoại lệ:**
1. Nhân viên tải lên file không đúng định dạng → Hệ thống từ chối: "Định dạng không hỗ trợ. Chỉ chấp nhận JPG, PNG, PDF"
2. Nhân viên tải lên quá 5 file → Hệ thống ngăn: "Đã đạt số lượng tài liệu tối đa (5 files)"

**Trường hợp lỗi:**
1. Azure Blob upload thất bại → Hệ thống hiển thị lỗi và cho phép thử lại mà không mất nội dung form
2. File vượt quá 10MB → Hệ thống hiển thị ngay: "Tệp quá lớn. Vui lòng chọn file dưới 10MB"

#### Ghi chú kỹ thuật
- IAzureBlobService: tái sử dụng pattern từ Phase 4
- WarrantyDocument entity: DocumentId, ClaimId, BlobUrl, FileName, FileSize, ContentType, UploadedAt
- Container: "warranty-documents" trên Azure Blob

---

## OPT-08: Lịch sử đơn thuốc kính

### US-OPT-017: Xem lịch sử đơn thuốc kính của bệnh nhân

**Là một** bác sĩ / nhân viên quầy kính,
**Tôi muốn** xem toàn bộ lịch sử đơn thuốc kính của bệnh nhân theo thứ tự thời gian,
**Để** nắm được tiến trình thay đổi thị lực và tư vấn kính phù hợp.

**Yêu cầu liên quan:** OPT-08

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ hoặc nhân viên mở hồ sơ bệnh nhân → Tab "Lịch sử kính" → Hệ thống hiển thị tất cả đơn thuốc kính theo thứ tự mới nhất trước
2. Mỗi đơn thuốc hiển thị: Ngày kê, Bác sĩ kê, Thông số đầy đủ (SPH/CYL/AXIS/ADD/PD cả hai mắt), Loại kính đặt
3. Nhân viên có thể xem đơn kính nào đã được thực hiện (có đơn hàng kính liên kết) và đơn nào chưa

**Trường hợp ngoại lệ:**
1. Bệnh nhân chưa có đơn thuốc kính → Hệ thống hiển thị: "Bệnh nhân chưa có lịch sử kính"
2. Bệnh nhân có đơn thuốc kính áp tròng → Hiển thị riêng trong tab "Kính áp tròng"

**Trường hợp lỗi:**
1. Không thể tải lịch sử → Hệ thống hiển thị toast lỗi: "Không thể tải lịch sử đơn thuốc kính"

#### Ghi chú kỹ thuật
- Query cross-module: Clinical.Contracts.GetOpticalPrescriptionsByPatient(patientId)
- Hiển thị dạng timeline hoặc bảng với pagination
- API endpoint: GET /api/optical/patients/{patientId}/prescription-history

---

### US-OPT-018: So sánh số kính theo năm

**Là một** bác sĩ,
**Tôi muốn** xem bảng so sánh thay đổi số kính của bệnh nhân qua các năm,
**Để** đánh giá tiến triển thị lực và hiệu quả điều trị (đặc biệt quan trọng cho bệnh nhân cận thị đang điều chỉnh với Ortho-K).

**Yêu cầu liên quan:** OPT-08

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở hồ sơ bệnh nhân → Tab "Lịch sử kính" → Nhấn "So sánh theo năm"
2. Hệ thống hiển thị bảng so sánh: cột là từng năm (ví dụ 2023, 2024, 2025), hàng là từng thông số (SPH mắt phải, SPH mắt trái, CYL mắt phải, CYL mắt trái...)
3. Các ô hiển thị giá trị số kính tương ứng. Ô có thay đổi so với năm trước được tô màu (tăng = đỏ, giảm = xanh, không đổi = xám)
4. Phần trăm thay đổi năm nay so với năm trước được tính và hiển thị bên cạnh giá trị

**Trường hợp ngoại lệ:**
1. Bệnh nhân chỉ có 1 năm dữ liệu → Hệ thống hiển thị: "Cần ít nhất 2 lần kê đơn để so sánh"
2. Năm dữ liệu bị thiếu (không có đơn trong một năm cụ thể) → Ô hiển thị "N/A"

**Trường hợp lỗi:**
1. Lỗi tải dữ liệu so sánh → Hệ thống hiển thị toast lỗi và vẫn hiển thị danh sách đơn thuốc

#### Ghi chú kỹ thuật
- Group đơn thuốc theo năm kê đơn
- Lấy đơn thuốc gần nhất trong mỗi năm để so sánh
- API endpoint: GET /api/optical/patients/{patientId}/year-comparison

---

## OPT-09: Kiểm kê kho

### US-OPT-019: Bắt đầu phiên kiểm kê kho gọng kính

**Là một** nhân viên quầy kính / quản lý,
**Tôi muốn** tạo phiên kiểm kê kho mới và ghi lại số lượng gọng kính thực tế bằng cách quét mã vạch,
**Để** phát hiện chênh lệch giữa số liệu hệ thống và thực tế trong kho.

**Yêu cầu liên quan:** OPT-09

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên vào "Kiểm Kê" → Nhấn "Tạo phiên kiểm kê mới" → Đặt tên phiên (ví dụ: "Kiểm kê tháng 3/2026") → Nhấn "Bắt đầu"
2. Hệ thống tạo phiên kiểm kê, snapshot số lượng tồn kho hệ thống tại thời điểm bắt đầu
3. Nhân viên quét mã vạch từng gọng → Hệ thống ghi nhận gọng đã quét và số lượng đếm được
4. Nếu quét lại mã vạch đã có → Hệ thống tăng số lượng đếm thêm 1 (cộng dồn)
5. Nhân viên có thể tạm dừng và tiếp tục sau (phiên lưu trạng thái)
6. Sau khi quét xong → Nhấn "Hoàn tất kiểm kê" → Hệ thống tạo báo cáo chênh lệch

**Trường hợp ngoại lệ:**
1. Gọng không có mã vạch → Nhân viên tìm kiếm thủ công bằng tên/thương hiệu và nhập số lượng
2. Phiên kiểm kê bị gián đoạn (mất điện, tắt máy) → Hệ thống lưu tự động → Tiếp tục từ lần cuối đã quét

**Trường hợp lỗi:**
1. Quét mã vạch không nhận diện → Hệ thống hiển thị: "Mã vạch không tìm thấy trong hệ thống. Ghi nhận thủ công?"
2. Lỗi lưu dữ liệu → Hệ thống hiển thị lỗi và giữ nguyên số lượng đã quét

#### Ghi chú kỹ thuật
- StocktakingSession entity: SessionId, Name, StartedAt, CompletedAt, Status (InProgress/Completed), BranchId
- StocktakingItem entity: SessionId, FrameId, SystemQuantity (snapshot lúc bắt đầu), CountedQuantity, Difference
- Auto-save mỗi 30 giây khi phiên đang chạy

---

### US-OPT-020: Quét mã vạch trong quá trình kiểm kê

**Là một** nhân viên quầy kính,
**Tôi muốn** quét mã vạch gọng kính bằng máy quét USB hoặc camera điện thoại khi đi kiểm kê tại quầy,
**Để** đếm nhanh và chính xác số lượng tồn kho thực tế.

**Yêu cầu liên quan:** OPT-09

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở phiên kiểm kê trên điện thoại/máy tính bảng → Giao diện kiểm kê hiển thị với nút "Quét camera"
2. Nhân viên nhấn "Quét camera" → Camera bật → Nhân viên hướng vào mã vạch EAN-13 của gọng
3. Hệ thống nhận diện mã vạch → Hiển thị tên gọng và số lượng đếm hiện tại → Tự động +1
4. Nhân viên tiếp tục quét các gọng khác → Danh sách gọng đã quét cập nhật theo thời gian thực
5. Với máy quét USB tại quầy: focus vào ô tìm kiếm → Quét → Tự động +1 số lượng

**Trường hợp ngoại lệ:**
1. Nhân viên quét nhầm → Nhấn "Sửa" vào mục đó → Nhập lại số lượng đúng
2. Gọng bị quét nhiều lần (nhầm) → Nhân viên điều chỉnh về số lượng đúng trước khi hoàn tất

**Trường hợp lỗi:**
1. Camera mất kết nối → Hệ thống chuyển sang chế độ nhập tay với thông báo: "Camera không khả dụng. Đang dùng chế độ nhập thủ công"
2. Không có kết nối internet khi kiểm kê → Hệ thống lưu cục bộ và đồng bộ khi có mạng trở lại

#### Ghi chú kỹ thuật
- Mobile-first design cho màn hình kiểm kê
- Web barcode scanner: html5-qrcode library (miễn phí, hỗ trợ EAN-13)
- Offline support: lưu IndexedDB, sync khi có mạng

---

### US-OPT-021: Xem báo cáo chênh lệch sau kiểm kê

**Là một** quản lý / nhân viên quầy kính,
**Tôi muốn** xem báo cáo chi tiết sau khi hoàn tất kiểm kê, hiển thị sự chênh lệch giữa số lượng thực tế và số liệu hệ thống,
**Để** phát hiện thất thoát, nhầm lẫn và điều chỉnh tồn kho cho chính xác.

**Yêu cầu liên quan:** OPT-09

#### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên nhấn "Hoàn tất kiểm kê" → Hệ thống tạo báo cáo tự động
2. Báo cáo gồm 3 phần:
   - **Chênh lệch số lượng:** Danh sách gọng có số đếm ≠ số hệ thống (thừa/thiếu bao nhiêu)
   - **Không quét được (thiếu trong kiểm kê):** Gọng hệ thống có tồn kho > 0 nhưng không được quét
   - **Quét nhưng không có trong hệ thống:** Mã vạch quét được nhưng không tìm thấy trong danh mục
3. Mỗi mục hiển thị: Tên gọng, Mã vạch, Số lượng hệ thống, Số lượng đếm thực tế, Chênh lệch
4. Quản lý xem xét báo cáo → Nhấn "Điều chỉnh tồn kho" → Hệ thống cập nhật tồn kho theo số liệu thực tế đã kiểm kê

**Trường hợp ngoại lệ:**
1. Không có chênh lệch nào → Báo cáo hiển thị: "Tồn kho khớp hoàn toàn" và xác nhận hoàn tất
2. Quản lý không đồng ý điều chỉnh toàn bộ → Có thể chọn từng mục để điều chỉnh hoặc bỏ qua

**Trường hợp lỗi:**
1. Lỗi tạo báo cáo → Hệ thống lưu phiên kiểm kê và thông báo: "Tạo báo cáo thất bại. Vui lòng thử lại hoặc liên hệ quản trị viên"
2. Lỗi điều chỉnh tồn kho → Hệ thống hiển thị lỗi và KHÔNG cập nhật tồn kho để tránh dữ liệu sai

#### Ghi chú kỹ thuật
- Báo cáo chênh lệch tạo khi Session.Status chuyển sang Completed
- Stocktaking report có thể xuất PDF qua QuestPDF
- Điều chỉnh tồn kho: tạo StockAdjustment records với lý do "Kiểm kê [Session Name]"
- API endpoint: GET /api/optical/stocktaking/{sessionId}/discrepancy-report, POST /api/optical/stocktaking/{sessionId}/apply-adjustments

---

## Tóm tắt User Stories

| ID | Tên | Yêu cầu | Vai trò |
|----|-----|---------|---------|
| US-OPT-001 | Quản lý danh mục gọng kính | OPT-01 | Nhân viên quầy kính |
| US-OPT-002 | Quét mã vạch tra cứu gọng | OPT-01 | Nhân viên quầy kính |
| US-OPT-003 | Quản lý tồn kho gọng kính | OPT-01 | Nhân viên quầy kính |
| US-OPT-004 | Quản lý danh mục tròng kính | OPT-02 | Nhân viên quầy kính |
| US-OPT-005 | Theo dõi tồn kho tròng theo số độ | OPT-02 | Nhân viên quầy kính |
| US-OPT-006 | Đặt hàng tròng từ nhà cung cấp | OPT-02 | Nhân viên quầy kính |
| US-OPT-007 | Tạo đơn hàng kính từ đơn thuốc | OPT-03 | Nhân viên quầy kính |
| US-OPT-008 | Theo dõi trạng thái đơn hàng kính | OPT-03 | Nhân viên quầy kính |
| US-OPT-009 | Xem danh sách đơn hàng theo trạng thái | OPT-03 | Nhân viên quầy kính |
| US-OPT-010 | Chặn xử lý kính khi chưa thanh toán | OPT-04 | Hệ thống |
| US-OPT-011 | Kê đơn kính áp tròng qua HIS | OPT-05 | Bác sĩ |
| US-OPT-012 | Tạo và áp dụng gói combo có sẵn | OPT-06 | Quản trị viên / Nhân viên |
| US-OPT-013 | Tạo gói combo tùy chỉnh | OPT-06 | Nhân viên quầy kính |
| US-OPT-014 | Tạo yêu cầu bảo hành kính | OPT-07 | Nhân viên quầy kính |
| US-OPT-015 | Phê duyệt bảo hành thay thế | OPT-07 | Quản lý |
| US-OPT-016 | Tải lên tài liệu bảo hành | OPT-07 | Nhân viên quầy kính |
| US-OPT-017 | Xem lịch sử đơn thuốc kính | OPT-08 | Bác sĩ / Nhân viên |
| US-OPT-018 | So sánh số kính theo năm | OPT-08 | Bác sĩ |
| US-OPT-019 | Bắt đầu phiên kiểm kê kho | OPT-09 | Nhân viên / Quản lý |
| US-OPT-020 | Quét mã vạch khi kiểm kê | OPT-09 | Nhân viên quầy kính |
| US-OPT-021 | Xem báo cáo chênh lệch kiểm kê | OPT-09 | Quản lý / Nhân viên |
